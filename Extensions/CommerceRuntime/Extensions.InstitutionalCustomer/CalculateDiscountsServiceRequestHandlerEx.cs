namespace CDC.Commerce.Runtime.InstitutionalCustomer
{
    using CDC.Commerce.Runtime.MarginCap.Entities;
    using CDC.CommerceRuntime.Entities.DataModel;
    using Microsoft.Dynamics.Commerce.Runtime;
    using Microsoft.Dynamics.Commerce.Runtime.Data;
    using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    using Microsoft.Dynamics.Commerce.Runtime.Messages;
    using Microsoft.Dynamics.Commerce.Runtime.Services;
    using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    using Microsoft.Dynamics.Retail.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using PE = Microsoft.Dynamics.Commerce.Runtime.Services.PricingEngine;

    public class CalculateDiscountsServiceRequestHandlerEx : IRequestHandlerAsync
    {
        /// <summary>
        /// Gets the collection of supported request types by this handler.
        /// </summary>
        public IEnumerable<Type> SupportedRequestTypes => new[]
                {
                    typeof(CalculateDiscountsServiceRequest)
                };

        /// <summary>
        /// Implements customized solutions for pricing services.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <returns>The response object.</returns>
        public async Task<Response> Execute(Request request)
        {
            ThrowIf.Null(request, "request");

            using (new PE.PricingEngineExtensionContext())
            {
                Type requestType = request.GetType();
                using (PE.SimpleProfiler profiler = new PE.SimpleProfiler(requestType.Name, true, 0))
                {
                    Response response;
                    if (requestType == typeof(CalculateDiscountsServiceRequest))
                    {
                        PE.PricingEngineExtensionRepository.RegisterDiscountPackage(new DiscountPackageOfferLineLimitEx());

                        response = await CalculateDiscountAsync((CalculateDiscountsServiceRequest)request).ConfigureAwait(false);
                    }
                    else
                    {
                        throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                    }

                    return response;
                }
            }
        }

        private async Task<GetPriceServiceResponse> CalculateDiscountAsync(CalculateDiscountsServiceRequest request)
        {
            // PE.PricingEngineExtensionRepository
            ChannelConfiguration channelConfiguration = request.RequestContext.GetChannelConfiguration();
            Customer customer = await GetCustomerAsync(request.RequestContext, request.Transaction.CustomerId).ConfigureAwait(false);
            PricingDataServiceManagerEx pricingDataServiceManagerEx = new PricingDataServiceManagerEx(request.RequestContext, request.Transaction);

            PE.PricingEngine.CalculateDiscountsForLines(
                pricingDataServiceManagerEx,
                request.Transaction,
                new ChannelCurrencyOperations(request.RequestContext),
                channelConfiguration.Currency,
                customer.LineDiscountGroup,
                customer.MultilineDiscountGroup,
                customer.TotalDiscountGroup,
                shouldTotalLines: true,
                request.CalculateSimpleDiscountOnly,
                request.RequestContext.GetNowInChannelTimeZone(),
                new PricingTransactionTotalingEx(request.RequestContext));

         //  var resp = await this.ExecuteNextAsync<GetPriceServiceResponse>(request);


            Collection<SalesLine> salesLinesBeforeReorderedDiscounts = request.Transaction.SalesLines;

            SortSalesLines(request.Transaction.SalesLines, request);
            calculateMRPGST1Discount(request);
            FilterMonthlyCapDiscounts(request);
           await filterMarginCapDiscounts(request);

            Collection<SalesLine> salesLinesAfterReorderedDiscounts = request.Transaction.SalesLines;

            request.Transaction.SalesLines = new Collection<SalesLine>();
            foreach (SalesLine salesLine in salesLinesBeforeReorderedDiscounts)
            {
                SalesLine salesLineToReorder = salesLinesAfterReorderedDiscounts.First(p => p.LineId == salesLine.LineId);
                request.Transaction.SalesLines.Add(salesLineToReorder);
            }
            applyTenderDiscountGST1(request);
            applyTenderDiscountMax(request);

            request.Transaction.DiscountAmount = request.Transaction.SalesLines.Where(sl => !sl.IsVoided).Sum(a => a.DiscountAmount);
            request.Transaction.DiscountAmountWithoutTax = request.Transaction.SalesLines.Where(sl => !sl.IsVoided).Sum(a => a.DiscountAmountWithoutTax);
            request.Transaction.PeriodicDiscountAmount = request.Transaction.SalesLines.Where(sl => !sl.IsVoided).Sum(a => a.PeriodicDiscount);
            request.Transaction.TenderDiscountAmount = request.Transaction.SalesLines.Where(sl => !sl.IsVoided).Sum(a => a.TenderDiscountAmount);


            request.Transaction.IsDiscountFullyCalculated = !request.CalculateSimpleDiscountOnly;

            request.Transaction.SetProperty("CSDstoreId", request.RequestContext.GetDeviceConfiguration().StoreNumber);
            //request = new CalculateDiscountsServiceRequest(SetPartialDiscount(request.Transaction)) { RequestContext = request.RequestContext};
            if (!request.Transaction.LoyaltyCardId.IsNullOrEmpty() && request.Transaction.IsPropertyDefined("CSDCardBalance") && !char.IsDigit(request.Transaction.LoyaltyCardId[0]) && Convert.ToBoolean(request.Transaction?.GetProperty("checkLoyaltyLimit")?.ToString() ?? "false"))
            {
                SalesAffiliationLoyaltyTier affiliation = request.Transaction.AffiliationLoyaltyTierLines.Where(a => a.AffiliationType == RetailAffiliationType.Loyalty).FirstOrDefault();
                GetAffiliationDiscounts(request.RequestContext, affiliation?.AffiliationId.ToString() ?? string.Empty, out List<ExtensionsEntity> discountExtensionEntity);
                decimal.TryParse(request.Transaction?.GetProperty("CSDCardBalance")?.ToString()?.Trim() ?? string.Empty, out decimal cardBalance);
                int balance = (int)request.Transaction.ActiveSalesLines.Where(sl => sl.DiscountAmount > 0 && !sl.DiscountLines.IsNullOrEmpty() && sl.DiscountLines.Any(dl => discountExtensionEntity.Any(de => de.GetProperty("OFFERID").ToString() == dl.OfferId))).Sum(sl => sl.Price * sl.QuantityDiscounted);
                if (balance > cardBalance)
                {
                    balance = (int)cardBalance;
                }
                request.Transaction.SetProperty("CSDMonthlyLimitUsed", Convert.ToString(cardBalance - balance).PadLeft(5, '0'));
                request.Transaction.SetProperty("TransactionDateTimeFinal", DateTime.Now);
            }

            return new GetPriceServiceResponse(request.Transaction);
        }


        private static void calculateMRPGST1Discount(CalculateDiscountsServiceRequest request)
        {
            foreach(var item in request.Transaction.SalesLines) {
                if (!item.IsVoided) { 
                var applicableDiscountAmount = ( item.Price*item.Quantity);
                var applicableOnMRPDiscountAmount = ((item.Price * item.Quantity)-item.TaxAmount);
            foreach (var discount in item.DiscountLines.Where(d=>d.DiscountLineType== DiscountLineType.PeriodicDiscount))
            {

                if ((Convert.ToInt32(item.GetProperty("CSDGSTYPEU"))) == 1)
                {
                    decimal discountAmount = ((applicableOnMRPDiscountAmount * (discount.Percentage / 100)));



                    discount.Percentage = (((discountAmount / applicableDiscountAmount) * 100));
                    discount.SetProperty("PartialDiscountPCT", 0);// retailDiscount.Percentage);
                    discount.Amount = discountAmount;
                    discount.EffectiveAmount = discountAmount;
                    discount.EffectivePercentage = (((discountAmount / applicableDiscountAmount) * 100)); //retailDiscount.Percentage;


                }
            }
                }
            }
        }
        private static void applyTenderDiscount(CalculateDiscountsServiceRequest request)
        {
            GetLoyaltyCardDetails(out string cardNumber, out decimal cardBalance, request.RequestContext, out DateTime lastTransactionDateTime, request);

            GetTenderDiscountOfferIds(request.RequestContext, out List<string> offerIds);
            GetTenderDiscountValue(request.RequestContext, out decimal maxDiscount, out decimal minDiscount);

            decimal totalTenderTypeAmount = request.Transaction.SalesLines.Where(p => !p.IsVoided).Sum(t => t.Quantity * t.Price);

            decimal tenderPerLine = 0;
            if (request.Transaction.AmountPaid > 0)
            {
                tenderPerLine = request.Transaction.AmountPaid / request.Transaction.SalesLines.Where(p => !p.IsVoided).Count();
            }
            decimal amountDue=0;
            decimal amountDuePerLine = 0;
            if (request.Transaction.SalesLines.Count > 0) { 
             amountDue = request.Transaction.AmountDue;
             amountDuePerLine = amountDue / request.Transaction.SalesLines.Where(p => !p.IsVoided).Count();
            }
            foreach (SalesLine salesLine in request.Transaction.SalesLines)
            {
                if (!salesLine.IsVoided) { 
                decimal applicableDiscountAmount = salesLine.Quantity * salesLine.Price;
                decimal applicableOnMRPDiscountAmount = salesLine.Quantity * salesLine.Price - salesLine.TaxAmount;
                cardBalance -= salesLine.NetAmount - salesLine.DiscountLines.Where(g => g.DiscountLineType == DiscountLineType.TenderTypeDiscount).Sum(p => p.Amount);


                if (totalTenderTypeAmount < maxDiscount)
                {
                    var otherDiscountAmount = salesLine.TotalDiscount;// - salesLine.TenderDiscountAmount;

                    foreach (DiscountLine item in salesLine.DiscountLines.Where(a => a.DiscountLineType == DiscountLineType.TenderTypeDiscount))
                    {
                            /* if (cardBalance <= 0)
                             {
                                 var discAmount = (salesLine.Quantity*salesLine.Price) - salesLine.DiscountLines.Where(p=>p.DiscountLineType != DiscountLineType.TenderTypeDiscount).Sum(t=>t.Amount);

                                 item.EffectiveAmount = discAmount * (item.Percentage / 100); //((((salesLine.Price * salesLine.Quantity) -(salesLine.TaxAmount) - (salesLine.DiscountLines.Where(d => !offerIds.Contains(d.OfferId)).Sum(a => a.EffectiveAmount))) * (item.Percentage / 100)));
                                 item.Amount = item.EffectiveAmount;
                             }
                             if ((Convert.ToInt32(salesLine.GetProperty("CSDGSTYPEU"))) == 1)
                             {
                                 item.EffectiveAmount = salesLine.NetAmount * (item.Percentage/100); //((((salesLine.Price * salesLine.Quantity) -(salesLine.TaxAmount) - (salesLine.DiscountLines.Where(d => !offerIds.Contains(d.OfferId)).Sum(a => a.EffectiveAmount))) * (item.Percentage / 100)));
                                 item.Amount = item.EffectiveAmount;
                             }*/


                            // decimal discAmount = (((salesLine.Quantity * salesLine.Price)) - otherDiscountAmount - tenderPerLine) * (item.Percentage / 100);

                            //    decimal discAmount = (amountDuePerLine - tenderPerLine) * (item.Percentage / 100);

                                decimal discAmount = (amountDuePerLine ) * (item.Percentage / 100);

                            //if ((Convert.ToInt32(salesLine.GetProperty("CSDGSTYPEU"))) == 1)
                            //{
                            //    discAmount = (((salesLine.Quantity * salesLine.Price)+salesLine.TaxAmount) - tenderPerLine) * (item.Percentage / 100);
                            //}
                            item.EffectiveAmount = discAmount;
                        item.Amount = discAmount;
                    }
                }
                else
                {
                    decimal discountPerLine = minDiscount / request.Transaction.SalesLines.Where(p => !p.IsVoided).Count();
                    foreach (DiscountLine item in salesLine.DiscountLines.Where(a => a.DiscountLineType == DiscountLineType.TenderTypeDiscount))
                    {
                        /* if (cardBalance <= 0)
                         {
                             var discAmount = (salesLine.Quantity*salesLine.Price) - salesLine.DiscountLines.Where(p=>p.DiscountLineType != DiscountLineType.TenderTypeDiscount).Sum(t=>t.Amount);

                             item.EffectiveAmount = discAmount * (item.Percentage / 100); //((((salesLine.Price * salesLine.Quantity) -(salesLine.TaxAmount) - (salesLine.DiscountLines.Where(d => !offerIds.Contains(d.OfferId)).Sum(a => a.EffectiveAmount))) * (item.Percentage / 100)));
                             item.Amount = item.EffectiveAmount;
                         }
                         if ((Convert.ToInt32(salesLine.GetProperty("CSDGSTYPEU"))) == 1)
                         {
                             item.EffectiveAmount = salesLine.NetAmount * (item.Percentage/100); //((((salesLine.Price * salesLine.Quantity) -(salesLine.TaxAmount) - (salesLine.DiscountLines.Where(d => !offerIds.Contains(d.OfferId)).Sum(a => a.EffectiveAmount))) * (item.Percentage / 100)));
                             item.Amount = item.EffectiveAmount;
                         }*/
                        decimal discAmount = discountPerLine;

                        item.EffectiveAmount = discAmount;
                        item.Amount = discAmount;
                    }
                }
                salesLine.DiscountAmount = salesLine.DiscountLines.Sum(a => a.EffectiveAmount);
              //  salesLine.DiscountAmountWithoutTax = salesLine.DiscountAmount - salesLine.TaxAmount; //salesLine.DiscountLines.Sum(a => a.DiscountCost);
                salesLine.PeriodicDiscount = salesLine.DiscountLines.Where(a => a.DiscountLineType == DiscountLineType.PeriodicDiscount).Sum(line => line.EffectiveAmount);
                salesLine.TenderDiscountAmount = salesLine.DiscountLines.Where(a => a.DiscountLineType == DiscountLineType.TenderTypeDiscount).Sum(line => line.EffectiveAmount);
            }
            }
            request.Transaction.DiscountAmount = request.Transaction.SalesLines.Where(sl => !sl.IsVoided).Sum(a => a.DiscountAmount);
            request.Transaction.DiscountAmountWithoutTax = request.Transaction.SalesLines.Where(sl => !sl.IsVoided).Sum(a => a.DiscountAmountWithoutTax);
            request.Transaction.PeriodicDiscountAmount = request.Transaction.SalesLines.Where(sl => !sl.IsVoided).Sum(a => a.PeriodicDiscount);
            request.Transaction.TenderDiscountAmount = request.Transaction.SalesLines.Where(sl => !sl.IsVoided).Sum(a => a.TenderDiscountAmount);


        }
        private static void applyTenderDiscountMax(CalculateDiscountsServiceRequest request)
        {
           
            GetLoyaltyCardDetails(out string cardNumber, out decimal cardBalance, request.RequestContext, out DateTime lastTransactionDateTime, request);

            GetTenderDiscountOfferIds(request.RequestContext, out List<string> offerIds);
            GetTenderDiscountValue(request.RequestContext, out decimal maxDiscount, out decimal minDiscount);

            decimal totalTenderTypeAmount = request.Transaction.AmountDue; //request.Transaction.SalesLines.Where(p => !p.IsVoided).Sum(t => t.Quantity * t.Price);

            if(totalTenderTypeAmount < maxDiscount)
            {
                return;
            }
            decimal tenderPerLine = 0;
            if (request.Transaction.AmountPaid > 0)
            {
                tenderPerLine = (request.Transaction.AmountPaid / request.Transaction.SalesLines.Where(p => !p.IsVoided).Count());
            }
            decimal amountDue = 0;
            decimal amountDuePerLine = 0;
            if (request.Transaction.SalesLines.Count > 0)
            {
                amountDue = (request.Transaction.AmountDue);
                amountDuePerLine =(amountDue / request.Transaction.SalesLines.Where(p => !p.IsVoided).Count());
            }
            foreach (SalesLine salesLine in request.Transaction.SalesLines)
            {
                if (!salesLine.IsVoided)
                {


                    if (totalTenderTypeAmount >= maxDiscount)
                    {
                   
                        decimal discountPerLine = (minDiscount / request.Transaction.SalesLines.Where(p => !p.IsVoided).Count());
                        decimal lineAskariDiscountPercentage = ((salesLine.Quantity*salesLine.Price)/ request.Transaction.SalesLines.Where(p => !p.IsVoided).Sum(t => t.Quantity * t.Price));
                        decimal lineAskaruDiscountAmount = ((lineAskariDiscountPercentage*minDiscount));
                        foreach (DiscountLine item in salesLine.DiscountLines.Where(a => a.DiscountLineType == DiscountLineType.TenderTypeDiscount))
                        {
                         
                            decimal discAmount = (lineAskaruDiscountAmount);

                            item.EffectiveAmount = discAmount;
                            item.Amount = discAmount;
                        
                        }
                    salesLine.DiscountAmount = salesLine.DiscountLines.Sum(a => a.EffectiveAmount);
                   // salesLine.DiscountAmountWithoutTax = salesLine.DiscountAmount - salesLine.TaxAmount;// salesLine.DiscountLines.Sum(a => a.DiscountCost);
                    salesLine.PeriodicDiscount = salesLine.DiscountLines.Where(a => a.DiscountLineType == DiscountLineType.PeriodicDiscount).Sum(line => line.EffectiveAmount);
                    salesLine.TenderDiscountAmount = salesLine.DiscountLines.Where(a => a.DiscountLineType == DiscountLineType.TenderTypeDiscount).Sum(line => line.EffectiveAmount);
                }
            }
            request.Transaction.DiscountAmount = request.Transaction.SalesLines.Where(sl => !sl.IsVoided).Sum(a => a.DiscountAmount);
            request.Transaction.DiscountAmountWithoutTax = request.Transaction.SalesLines.Where(sl => !sl.IsVoided).Sum(a => a.DiscountAmountWithoutTax);
            request.Transaction.PeriodicDiscountAmount = request.Transaction.SalesLines.Where(sl => !sl.IsVoided).Sum(a => a.PeriodicDiscount);
            request.Transaction.TenderDiscountAmount = request.Transaction.SalesLines.Where(sl => !sl.IsVoided).Sum(a => a.TenderDiscountAmount);


        }

            
                //RetailLogger log = new RetailLogger();
           
            //lo("total due "+ request.Transaction.AmountDue.ToString());

        }
        private  static void applyTenderDiscountGST1(CalculateDiscountsServiceRequest request)
        {
            GetLoyaltyCardDetails(out string cardNumber, out decimal cardBalance, request.RequestContext, out DateTime lastTransactionDateTime, request);

            GetTenderDiscountOfferIds(request.RequestContext, out List<string> offerIds);
            GetTenderDiscountValue(request.RequestContext, out decimal maxDiscount, out decimal minDiscount);

            decimal totalTenderTypeAmount = request.Transaction.AmountDue; //request.Transaction.SalesLines.Where(p => !p.IsVoided).Sum(t => t.Quantity * t.Price);

            if (totalTenderTypeAmount > maxDiscount)
            {
                return;
            }
            decimal tenderPerLine = 0;
            if (request.Transaction.AmountPaid > 0)
            {
                tenderPerLine = (request.Transaction.AmountPaid / request.Transaction.SalesLines.Where(p => !p.IsVoided).Count());
            }
            foreach (SalesLine salesLine in request.Transaction.SalesLines)
            {
                
                    if (!salesLine.IsVoided && (Convert.ToInt32(salesLine.GetProperty("CSDGSTYPEU"))) == 1)
                {


                    if (totalTenderTypeAmount < maxDiscount)
                    {


                        foreach (DiscountLine item in salesLine.DiscountLines.Where(a => a.DiscountLineType == DiscountLineType.TenderTypeDiscount))
                        {

                            /*  minDiscount = (request.Transaction.AmountDue * item.Percentage) / 100;
                              decimal discountPerLine = (minDiscount / request.Transaction.SalesLines.Where(p => !p.IsVoided).Count());
                              decimal lineAskariDiscountPercentage = ((salesLine.Quantity * salesLine.Price) / request.Transaction.SalesLines.Where(p => !p.IsVoided).Sum(t => t.Quantity * t.Price));
                              decimal lineAskaruDiscountAmount = ((lineAskariDiscountPercentage * minDiscount));
  */
                            /* var tenderDiscountAskariAmount = request.Transaction.AmountDue *(item.Percentage/100);
                             decimal discAmount = tenderDiscountAskariAmount * ((salesLine.Price*salesLine.Quantity)-salesLine.DiscountLines.Where(a=>a.DiscountLineType ==DiscountLineType.PeriodicDiscount).Sum(a=>a.EffectiveAmount)) / request.Transaction.SalesLines.Where(p => !p.IsVoided).Sum(t => t.Quantity * t.Price); //*(item.Percentage/100); //(lineAskaruDiscountAmount);
                             item.EffectivePercentage = item.Percentage;
                             item.EffectiveAmount = discAmount;
                             item.Amount = discAmount;*/

                            var minDiscountAskari = (int) request.Transaction.AmountDue * (item.Percentage / 100);
                            
                            var askariDiscountPercentage =(int)minDiscountAskari/ (request.Transaction.AmountDue+request.Transaction.AmountPaid);


                            decimal discountAmount  = askariDiscountPercentage * ((salesLine.Quantity * salesLine.Price) - salesLine.DiscountLines.Where(a => a.DiscountLineType == DiscountLineType.PeriodicDiscount).Sum(a => a.EffectiveAmount));
                            item.EffectivePercentage = askariDiscountPercentage; //((salesLine.Quantity * salesLine.Price) / request.Transaction.SalesLines.Where(p => !p.IsVoided).Sum(t => t.Quantity * t.Price));
                            item.EffectiveAmount = discountAmount;//((lineAskariDiscountPercentage * minDiscount));
                            item.Amount = discountAmount;

                        }
                        salesLine.DiscountAmount = salesLine.DiscountLines.Sum(a => a.EffectiveAmount);
                      //  salesLine.DiscountAmountWithoutTax = salesLine.DiscountAmount - salesLine.TaxAmount; //salesLine.DiscountLines.Sum(a => a.EffectiveAmount);
                        salesLine.PeriodicDiscount = salesLine.DiscountLines.Where(a => a.DiscountLineType == DiscountLineType.PeriodicDiscount).Sum(line => line.EffectiveAmount);
                        salesLine.TenderDiscountAmount = salesLine.DiscountLines.Where(a => a.DiscountLineType == DiscountLineType.TenderTypeDiscount).Sum(line => line.EffectiveAmount);
                    }
                }
                request.Transaction.DiscountAmount = request.Transaction.SalesLines.Where(sl => !sl.IsVoided).Sum(a => a.DiscountAmount);
                request.Transaction.DiscountAmountWithoutTax = request.Transaction.SalesLines.Where(sl => !sl.IsVoided).Sum(a => a.DiscountAmountWithoutTax);
                request.Transaction.PeriodicDiscountAmount = request.Transaction.SalesLines.Where(sl => !sl.IsVoided).Sum(a => a.PeriodicDiscount);
                request.Transaction.TenderDiscountAmount = request.Transaction.SalesLines.Where(sl => !sl.IsVoided).Sum(a => a.TenderDiscountAmount);


            }
        }


        private async Task applyTenderDiscountMarginal(CalculateDiscountsServiceRequest request)
        {

           
                GetMarginCapOnProductAndProductCategory capOnProductAndProductCategory;

            bool excludeDiscount, isMarginCapEnabledOnProductAndProductCategory;
            decimal  marginCapPercentageOnProductAndProductCategory = 0.00M;


            GetLoyaltyCardDetails(out string cardNumber, out decimal cardBalance, request.RequestContext, out DateTime lastTransactionDateTime, request);

            GetTenderDiscountOfferIds(request.RequestContext, out List<string> offerIds);
            GetTenderDiscountValue(request.RequestContext, out decimal maxDiscount, out decimal minDiscount);

            decimal totalTenderTypeAmount = request.Transaction.AmountDue; //request.Transaction.SalesLines.Where(p => !p.IsVoided).Sum(t => t.Quantity * t.Price);

            if (totalTenderTypeAmount > maxDiscount)
            {
                return;
            }
            decimal tenderPerLine = 0;
            if (request.Transaction.AmountPaid > 0)
            {
                tenderPerLine = (request.Transaction.AmountPaid / request.Transaction.SalesLines.Where(p => !p.IsVoided).Count());
            }
           
            foreach (SalesLine salesLine in request.Transaction.SalesLines)
            {
                if ((Convert.ToInt32(salesLine.GetProperty("CSDGSTYPEU"))) == 1)
                {
                    continue;
                }
                capOnProductAndProductCategory = GetMarginCapOnProductAndProductCategory(request, salesLine.ItemId, await GetProductIdAsync(request, salesLine));
                isMarginCapEnabledOnProductAndProductCategory = Convert.ToBoolean(Convert.ToInt32(capOnProductAndProductCategory?.GetProperty("ISMARGINCAPALLOWED")?.ToString() ?? decimal.Zero.ToString()));
                excludeDiscount = Convert.ToBoolean(Convert.ToInt32(capOnProductAndProductCategory?.GetProperty("EXCLUDEDISCOUNT")?.ToString() ?? decimal.Zero.ToString()));
                marginCapPercentageOnProductAndProductCategory = (Convert.ToDecimal(capOnProductAndProductCategory?.GetProperty("MARGINCAPPERCENTAGE")?.ToString() ?? decimal.Zero.ToString()));

                if (!salesLine.IsVoided && isMarginCapEnabledOnProductAndProductCategory && !excludeDiscount)
                {


                    if (totalTenderTypeAmount < maxDiscount)
                    {

                 
                        foreach (DiscountLine item in salesLine.DiscountLines.Where(a => a.DiscountLineType == DiscountLineType.TenderTypeDiscount))
                        {

                            /*  minDiscount = (request.Transaction.AmountDue * item.Percentage)/100;
                              decimal discountPerLine = (minDiscount / request.Transaction.SalesLines.Where(p => !p.IsVoided).Count());
                              decimal lineAskariDiscountPercentage = ((salesLine.Quantity * salesLine.Price) / request.Transaction.SalesLines.Where(p => !p.IsVoided).Sum(t => t.Quantity * t.Price));
                              decimal lineAskaruDiscountAmount = ((lineAskariDiscountPercentage * minDiscount));

                              decimal discAmount = (lineAskaruDiscountAmount);

                              item.EffectiveAmount = discAmount;
                              item.Amount = discAmount;*/

                            var minDiscountAskari = (int)request.Transaction.AmountDue * (item.Percentage / 100);

                            var askariDiscountPercentage = (int)minDiscountAskari / (request.Transaction.AmountDue + request.Transaction.AmountPaid);


                            decimal discountAmount = askariDiscountPercentage * ((salesLine.Quantity * salesLine.Price) - salesLine.DiscountLines.Where(a => a.DiscountLineType == DiscountLineType.PeriodicDiscount).Sum(a => a.EffectiveAmount));
                            item.EffectivePercentage = askariDiscountPercentage; //((salesLine.Quantity * salesLine.Price) / request.Transaction.SalesLines.Where(p => !p.IsVoided).Sum(t => t.Quantity * t.Price));
                            item.EffectiveAmount = discountAmount;//((lineAskariDiscountPercentage * minDiscount));
                            item.Amount = discountAmount;

                        }
                        salesLine.DiscountAmount = salesLine.DiscountLines.Sum(a => a.EffectiveAmount);
                       // salesLine.DiscountAmountWithoutTax = salesLine.DiscountAmount-salesLine.TaxAmount;//salesLine.DiscountLines.Sum(a => a.EffectiveAmount);
                        salesLine.PeriodicDiscount = salesLine.DiscountLines.Where(a => a.DiscountLineType == DiscountLineType.PeriodicDiscount).Sum(line => line.EffectiveAmount);
                        salesLine.TenderDiscountAmount = salesLine.DiscountLines.Where(a => a.DiscountLineType == DiscountLineType.TenderTypeDiscount).Sum(line => line.EffectiveAmount);
                    }
                }
                request.Transaction.DiscountAmount = request.Transaction.SalesLines.Where(sl => !sl.IsVoided).Sum(a => a.DiscountAmount);
                request.Transaction.DiscountAmountWithoutTax = request.Transaction.SalesLines.Where(sl => !sl.IsVoided).Sum(a => a.DiscountAmountWithoutTax);
                request.Transaction.PeriodicDiscountAmount = request.Transaction.SalesLines.Where(sl => !sl.IsVoided).Sum(a => a.PeriodicDiscount);
                request.Transaction.TenderDiscountAmount = request.Transaction.SalesLines.Where(sl => !sl.IsVoided).Sum(a => a.TenderDiscountAmount);


            }
        }

        private static void GetTenderDiscountOfferIds(RequestContext context, out List<string> offerIds)
        {
            offerIds = new List<string>();

            // Get the configuration parameters
            GetConfigurationParametersDataRequest configurationRequest = new GetConfigurationParametersDataRequest(context.GetChannelConfiguration().RecordId);
            EntityDataServiceResponse<RetailConfigurationParameter> configurationResponse = context.ExecuteAsync<EntityDataServiceResponse<RetailConfigurationParameter>>(configurationRequest).Result;

            string tenderDiscountOfferIds = configurationResponse?.PagedEntityCollection?.Where(cp => string.Equals(cp.Name.ToUpper().Trim(), ("MINDISCOUNTHEADER").ToUpper().Trim(), StringComparison.OrdinalIgnoreCase))?.FirstOrDefault()?.Value ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(tenderDiscountOfferIds))
            {
                offerIds = tenderDiscountOfferIds.Split(';').ToList();
            }
        }

        private static void GetTenderDiscountValue(RequestContext context, out decimal maxDiscount, out decimal minDiscount)
        {
            maxDiscount = decimal.Zero;
            minDiscount = decimal.Zero;
            // Get the configuration parameters
            GetConfigurationParametersDataRequest configurationRequest = new GetConfigurationParametersDataRequest(context.GetChannelConfiguration().RecordId);
            EntityDataServiceResponse<RetailConfigurationParameter> configurationResponse = context.ExecuteAsync<EntityDataServiceResponse<RetailConfigurationParameter>>(configurationRequest).Result;

            string tenderDiscountValue = configurationResponse?.PagedEntityCollection?.Where(cp => string.Equals(cp.Name.ToUpper().Trim(), ("MAXDISCOUNTAMOUNT").ToUpper().Trim(), StringComparison.OrdinalIgnoreCase))?.FirstOrDefault()?.Value ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(tenderDiscountValue) && decimal.TryParse(tenderDiscountValue, out decimal value))
            {
                maxDiscount = value;
            }

            string tenderDiscountMinValue = configurationResponse?.PagedEntityCollection?.Where(cp => string.Equals(cp.Name.ToUpper().Trim(), ("MINDISCOUNTVALUE").ToUpper().Trim(), StringComparison.OrdinalIgnoreCase))?.FirstOrDefault()?.Value ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(tenderDiscountMinValue) && decimal.TryParse(tenderDiscountMinValue, out decimal minvalue))
            {
                minDiscount = minvalue;
            }
        }

        private static void SortSalesLines(Collection<SalesLine> salesLines, CalculateDiscountsServiceRequest request)
        {
            if (salesLines.IsNullOrEmpty())
            {
                return;
            }

            try
            {
                GetItemGSTDetails(request.RequestContext, request.Transaction, out List<ExtensionsEntity> entities, request);
                foreach (SalesLine item in request.Transaction.SalesLines)
                {
                    ExtensionsEntity entity = entities.Where(a => (a.GetProperty("InventoryDimensionId")?.ToString()?.Trim() ?? string.Empty) == item.InventoryDimensionId && (a.GetProperty("ItemId")?.ToString()?.Trim() ?? string.Empty) == item.ItemId).FirstOrDefault();
                    item.SetProperty("CDCTOPONCART", entity?.GetProperty("CDCTOPONCART") ?? decimal.Zero);
                    item.SetProperty("CDCPRICINGPRIORITY", entity?.GetProperty("CDCPRICINGPRIORITY") ?? decimal.Zero);
                    item.SetProperty("GrossProfit", entity?.GetProperty("GrossProfit") ?? decimal.Zero);
                    item.SetProperty(("CSDGSTYPEU"), entity?.GetProperty("CDCGSTTYPE") ?? decimal.Zero);
                    item.SetProperty(("CDCMAXIMUMRETAILPRICE"), entity?.GetProperty("CDCMAXIMUMRETAILPRICE") ?? decimal.Zero);

                }
                List<SalesLine> salesLinesList = request.Transaction.SalesLines.OrderByDescending(a => Convert.ToDecimal(a.GetProperty("CDCTOPONCART"))).ThenByDescending(x => Convert.ToDecimal(x.GetProperty("CDCPRICINGPRIORITY"))).ThenByDescending(z => Convert.ToDecimal(z.GetProperty("GrossProfit"))).ToList();
                request.Transaction.SalesLines = new Collection<SalesLine>();
                foreach (SalesLine sl in salesLinesList.ToList())
                {
                    request.Transaction.SalesLines.Add(sl);
                }
                return;
            }
            catch (Exception)
            {
                return;
            }

        }

        private static void GetItemGSTDetails(RequestContext context, SalesTransaction transaction, out List<ExtensionsEntity> entities, CalculateDiscountsServiceRequest request)
        {
            if (transaction == null || transaction.ActiveSalesLines.IsNullOrEmpty())
            {
                entities = new List<ExtensionsEntity>();
                return;
            }

            using (DatabaseContext databaseContext = new DatabaseContext(context))
            {
                SqlQuery query = new SqlQuery
                {
                    QueryString = $@"SELECT DISTINCT IT.ITEMID, IT.CDCGSTTYPE, IT.CDCTOPONCART,I2.CDCMAXIMUMRETAILPRICE, DP.CDCPRICINGPRIORITY,C1.COSTPRICE, C1.INVENTCOLORID, C1.CONFIGID, C1.INVENTSTYLEID, C1.INVENTSIZEID, C1.INVENTLOCATIONID FROM [ax].INVENTDIMCOMBINATION IDM FULL OUTER JOIN [ext].INVENTDIMCOMBINATION I2 on I2.RECID = IDM.RECID FULL OUTER JOIN [ext].[INVENTTABLE] IT on IT.ITEMID = IDM.ITEMID FULL OUTER JOIN[ext].[CDCGSTTYPEDISCOUNTPRIORITY] DP ON IT.CDCGSTTYPE = DP.CDCGSTTYPE FULL OUTER JOIN ext.CDCPRODUCTVARIANTCOSTPRICE C1 on C1.ITEMID = IT.ITEMID WHERE IDM.INVENTDIMID IN({string.Join(",", transaction.SalesLines.Select(sl => "'" + sl.InventoryDimensionId + "'"))}) AND  IDM.ITEMID IN({string.Join(",", transaction.SalesLines.Select(sl => "'" + sl.ItemId + "'"))})  AND IT.DATAAREAID = @dataAreaId AND C1.INVENTLOCATIONID = @inventLocationId  order by CDCTOPONCART desc, CDCPRICINGPRIORITY desc"
                };
                query.Parameters["@dataAreaId"] = context.GetChannelConfiguration().InventLocationDataAreaId;
                query.Parameters["@inventLocationId"] = context.GetDeviceConfiguration().InventLocationId;

                try
                {
                    entities = databaseContext.ReadEntity<ExtensionsEntity>(query).ToList();
                    entities = CalculateGrossProfit(entities, request);
                }
                catch (Exception)
                {
                    entities = new List<ExtensionsEntity>();
                }
            }
        }
        public static List<ExtensionsEntity> CalculateGrossProfit(List<ExtensionsEntity> entities, CalculateDiscountsServiceRequest request)
        {
            try
            {
                foreach (SalesLine item in request.Transaction.SalesLines)
                {
                    ExtensionsEntity entity = entities.Where(sl => item.ItemId.Equals(Convert.ToString(sl.GetProperty("ITEMID") ?? string.Empty))
                                     && item.Variant.ColorId == ((sl.GetProperty("INVENTCOLORID")?.ToString()?.Trim() ?? string.Empty) != string.Empty ? Convert.ToString(sl.GetProperty("INVENTCOLORID")) : null)
                                     && item.Variant.StyleId == ((sl.GetProperty("INVENTSTYLEID")?.ToString()?.Trim() ?? string.Empty) != string.Empty ? Convert.ToString(sl.GetProperty("INVENTSTYLEID")) : null)
                                     && item.Variant.SizeId == ((sl.GetProperty("INVENTSIZEID")?.ToString()?.Trim() ?? string.Empty) != string.Empty ? Convert.ToString(sl.GetProperty("INVENTSIZEID")) : null)
                                     && item.Variant.ConfigId == ((sl.GetProperty("CONFIGID")?.ToString()?.Trim() ?? string.Empty) != string.Empty ? Convert.ToString(sl.GetProperty("CONFIGID")) : null)
                                     ).FirstOrDefault();

                    if (entity != null)
                    {
                        entity.SetProperty("InventoryDimensionId", item.InventoryDimensionId);
                        entity.SetProperty("GrossProfit", CalculateGrossMargin(Convert.ToDecimal(entity?.GetProperty("COSTPRICE") ?? decimal.Zero), item?.Price ?? decimal.Zero));

                        item.SetProperty("CDCTOPONCART", entity.GetProperty("CDCTOPONCART")?.ToString()?.Trim() ?? string.Empty);
                        item.SetProperty("CDCPRICINGPRIORITY", entity.GetProperty("CDCPRICINGPRIORITY")?.ToString()?.Trim() ?? string.Empty);
                        item.SetProperty("GrossProfit", entity.GetProperty("GrossProfit")?.ToString()?.Trim() ?? string.Empty);

                    }

                }
                return entities.OrderByDescending(a => a.GetProperty("CDCTOPONCART")).ThenByDescending(x => x.GetProperty("CDCPRICINGPRIORITY")).ThenByDescending(z => z.GetProperty("GrossProfit")).ToList();
            }
            catch (Exception ex)
            {
                RetailLogger.Log.AxGenericErrorEvent(string.Format("Error when sorting: {0}", ex.Message));
                return entities;
            }
        }
        private static decimal CalculateGrossMargin(decimal costPrice, decimal sellPrice)
        {
            if (costPrice <= decimal.Zero)
            {
                return decimal.Zero;
            }
            else
            {
                return ((sellPrice - costPrice) / sellPrice) * 100;
            }
        }
        private static void FilterMonthlyCapDiscounts(CalculateDiscountsServiceRequest request)
        {
            GetLoyaltyCardDetails(out string cardNumber, out decimal cardBalance, request.RequestContext, out DateTime lastTransactionDateTime, request);

            if (string.IsNullOrWhiteSpace(request.Transaction.LoyaltyCardId)
              || string.IsNullOrWhiteSpace(cardNumber)
              || !request.Transaction.LoyaltyCardId.Equals(cardNumber)
              || !request.Transaction.AffiliationLoyaltyTierLines.Any(alt => alt.AffiliationType == RetailAffiliationType.Loyalty))
            {
                //this.MonthlyLimitUsed = decimal.Zero;
                return;
            }


            SalesAffiliationLoyaltyTier affiliationLoyaltyTier = request.Transaction.AffiliationLoyaltyTierLines.FirstOrDefault(alt => alt.AffiliationType == RetailAffiliationType.Loyalty);

            GetLoyaltyDetails(request.RequestContext, affiliationLoyaltyTier.AffiliationId, out decimal loyaltyLimit, out bool checkLoyaltyLimit, request);
            request.Transaction.SetProperty("checkLoyaltyLimit", checkLoyaltyLimit);

            if (!checkLoyaltyLimit)
            {
                //this.MonthlyLimitUsed = decimal.Zero;
                return;
            }

            decimal cartTotal = decimal.Zero;
            Dictionary<string, decimal> itemPriceTotal = new Dictionary<string, decimal>();

            List<PeriodicDiscount> filteredRetailDiscounts = new List<PeriodicDiscount>();
            foreach (SalesLine salesLine in request.Transaction.SalesLines)
            {

                bool discountApplied = false;

                if (!salesLine.IsVoided)
                {
                    bool allowOnce = false;
                    decimal splitAmount = 0;
                    decimal applicableDiscountAmount = (salesLine.Quantity * salesLine.Price);
                    decimal applicableOnMRPDiscountAmount = (salesLine.Quantity * salesLine.Price);
                    if ((Convert.ToInt32(salesLine.GetProperty("CSDGSTYPEU"))) == 1)
                    {
                        applicableOnMRPDiscountAmount -= salesLine.TaxAmount;
                    }

                    foreach (DiscountLine retailDiscount in salesLine.PeriodicDiscountLines())
                    {
                        discountApplied = true;

                        if(retailDiscount.EffectiveAmount == Decimal.Zero)
                        {
                            salesLine.DiscountLines.Remove(retailDiscount);
                            discountApplied = false;
                        }

                        if (cardBalance == 0 && !allowOnce)
                        {
                            salesLine.DiscountLines.Remove(retailDiscount);
                            continue;
                            // break; //TODO: Need to handle loyalty and non loyalty discount removal while catering card limit amount scenario
                        }

                        else if (applicableDiscountAmount > cardBalance)
                        {
                            if (splitAmount == 0)
                            {
                                splitAmount = decimal.Round(cardBalance / 2,2, MidpointRounding.AwayFromZero);

                            }
                            decimal discountAmount = decimal.Round((cardBalance * (retailDiscount.Percentage / 100)),2);

                          //  discountAmount = (discountAmount);

                            retailDiscount.Percentage = (((discountAmount / applicableDiscountAmount) * 100));
                            retailDiscount.SetProperty("PartialDiscountPCT", 0);// retailDiscount.Percentage);
                            retailDiscount.Amount = discountAmount;
                            retailDiscount.EffectiveAmount = discountAmount;
                            retailDiscount.EffectivePercentage =(((discountAmount / applicableDiscountAmount) * 100)); //retailDiscount.Percentage;
                            

                            cartTotal += cardBalance;
                            // cardBalance -= applicableDiscountAmount;
                            //salesLine.NetAmount+salesLine.DiscountAmount;
                            if (allowOnce)
                            {
                                allowOnce = false;
                            }
                            else
                            {
                                allowOnce = true;
                            }
                            
                        }
                        else
                        {
                           /* if ((Convert.ToInt32(salesLine.GetProperty("CSDGSTYPEU"))) == 1)
                            {
                                decimal discountAmount = (applicableOnMRPDiscountAmount * (retailDiscount.Percentage / 100));



                                retailDiscount.Percentage = ((discountAmount / applicableDiscountAmount) * 100);
                                retailDiscount.SetProperty("PartialDiscountPCT", 0);// retailDiscount.Percentage);
                                retailDiscount.Amount = discountAmount;
                                retailDiscount.EffectiveAmount = discountAmount;
                                retailDiscount.EffectivePercentage = ((discountAmount / applicableDiscountAmount) * 100); //retailDiscount.Percentage;


                            }*/
                            cartTotal += salesLine.NetAmount;

                            // salesLine.NetAmount+salesLine.DiscountAmount;
                            // itemPriceTotal.Add(string.Format("{0}::{1}", retailDiscount.ItemId, retailDiscount.InventoryDimensionId), lineTotal);
                            // filteredRetailDiscounts.Add(retailDiscount);
                        }



                    }

                    if (discountApplied)
                    {
                        cardBalance -= applicableDiscountAmount;
                    }

                    if (cardBalance <= 0)
                    {
                        cardBalance = 0;

                    }
                    salesLine.DiscountAmount = salesLine.DiscountLines.Sum(a => a.EffectiveAmount);
                    //salesLine.DiscountAmountWithoutTax = salesLine.DiscountAmount- salesLine.TaxAmount;//salesLine.DiscountLines.Sum(a => a.DiscountCost);
                    salesLine.PeriodicDiscount = salesLine.DiscountLines.Where(a => a.DiscountLineType == DiscountLineType.PeriodicDiscount).Sum(line => line.EffectiveAmount);
                    salesLine.TenderDiscountAmount = salesLine.DiscountLines.Where(a => a.DiscountLineType == DiscountLineType.TenderTypeDiscount).Sum(line => line.EffectiveAmount);

                }

            }
            request.Transaction.DiscountAmount = request.Transaction.SalesLines.Where(sl => !sl.IsVoided).Sum(a => a.DiscountAmount);
            request.Transaction.DiscountAmountWithoutTax = request.Transaction.SalesLines.Where(sl => !sl.IsVoided).Sum(a => a.DiscountAmountWithoutTax);
            request.Transaction.PeriodicDiscountAmount = request.Transaction.SalesLines.Where(sl => !sl.IsVoided).Sum(a => a.PeriodicDiscount);
            request.Transaction.TenderDiscountAmount = request.Transaction.SalesLines.Where(sl => !sl.IsVoided).Sum(a => a.TenderDiscountAmount);

            return;
        }
        private static void GetLoyaltyDetails(RequestContext context, long affiliationId, out decimal loyaltyLimit, out bool checkLoyaltyLimit, CalculateDiscountsServiceRequest request)
        {
            loyaltyLimit = decimal.Zero;
            checkLoyaltyLimit = false;

            using (DatabaseContext databaseContext = new DatabaseContext(context))
            {
                SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                {
                    DatabaseSchema = "ext",
                    Select = new ColumnSet("CDCPROTECTMONTHLYCAT", "CDCMONTHLYCATLIMIT"),
                    From = "RETAILAFFILIATION",
                    Where = "RECID = @affiliationId"
                };

                query.Parameters["@affiliationId"] = affiliationId;

                try
                {
                    PagedResult<ExtensionsEntity> loyaltyDetail = databaseContext.ReadEntity<ExtensionsEntity>(query);
                    loyaltyLimit = Convert.ToDecimal(Convert.ToString(loyaltyDetail?.FirstOrDefault()?.GetProperty("CDCMONTHLYCATLIMIT") ?? decimal.Zero));
                    checkLoyaltyLimit = Convert.ToBoolean(Convert.ToInt32(Convert.ToString(loyaltyDetail?.FirstOrDefault()?.GetProperty("CDCPROTECTMONTHLYCAT") ?? decimal.Zero)));

                    if (!checkLoyaltyLimit)
                    {
                        request.Transaction.SetProperty("CSDMonthlyLimitUsed", "00000");
                        request.Transaction.SetProperty("CSDCardBalance", "00000");
                        request.Transaction.SetProperty("EmployeeCreditLimit", "00000");
                        // this.MonthlyLimitUsed = decimal.Zero;
                    }
                }
                catch (Exception)
                {
                    loyaltyLimit = decimal.Zero;
                    checkLoyaltyLimit = false;
                }
            }
        }

        private static decimal roundingRule(decimal roundAmount) { return roundAmount; }

        private static void GetLoyaltyCardDetails(out string cardNumber, out decimal cardBalance, RequestContext context, out DateTime lastTransactionDateTime, CalculateDiscountsServiceRequest request)
        {
            cardNumber = request.Transaction?.GetProperty("CSDCardNumber")?.ToString()?.Trim() ?? string.Empty;
            decimal.TryParse(request.Transaction?.GetProperty("CSDCardBalance")?.ToString()?.Trim() ?? string.Empty, out cardBalance);
            decimal.TryParse(request.Transaction?.GetProperty("CSDOldCardBalance")?.ToString()?.Trim() ?? string.Empty, out decimal oldCardBalance);
            DateTime.TryParse(request.Transaction?.GetProperty("CSDCardResetDateTime")?.ToString() ?? string.Empty, out DateTime resetBalanceDateTime);

            if (!string.IsNullOrEmpty(request.Transaction.LoyaltyCardId) && cardNumber != string.Empty && resetBalanceDateTime > DateTime.Now)
            {
                cardBalance = oldCardBalance;
                throw new CommerceException("Microsoft_Dynamics_Commerce_30104", "Card Balance")
                {
                    LocalizedMessage = "Card balance was reset in future date time",
                    LocalizedMessageParameters = new object[] { }
                };
            }

            lastTransactionDateTime = DateTime.Now;
            // cardBalance = ResetCardBalance(cardBalance, context, out lastTransactionDateTime);
        }


        private static SalesTransaction SetPartialDiscount(SalesTransaction salesTransaction)
        {
            if (salesTransaction == null || salesTransaction.ActiveSalesLines.IsNullOrEmpty())
            {
                return salesTransaction;
            }

            SalesLine line = salesTransaction.ActiveSalesLines.Where(a => a.DiscountLines.Any(dl => dl.IsPropertyDefined("PartialDiscountPCT"))).FirstOrDefault();
            if (line != null)
            {
                List<DiscountLine> discountLines = line.DiscountLines.Where(dl => dl.IsPropertyDefined("PartialDiscountPCT")).ToList();
                foreach (DiscountLine item in discountLines)
                {
                    decimal disocuntPercentage = Convert.ToDecimal(item?.GetProperty("PartialDiscountPCT")?.ToString() ?? decimal.Zero.ToString());
                    item.EffectiveAmount = (line.Price * line.Quantity) * (disocuntPercentage / 100);
                    item.Amount = item.EffectiveAmount / line.Quantity;
                    item.EffectivePercentage = disocuntPercentage;
                }

                line.DiscountAmount = line.DiscountLines.Sum(a => a.EffectiveAmount);
                line.DiscountAmountWithoutTax = line.DiscountLines.Sum(a => a.DiscountCost);
                line.PeriodicDiscount = line.DiscountLines.Where(a => a.DiscountLineType == DiscountLineType.PeriodicDiscount).Sum(l => l.EffectiveAmount);
                line.TenderDiscountAmount = line.DiscountLines.Where(a => a.DiscountLineType == DiscountLineType.TenderTypeDiscount).Sum(l => l.EffectiveAmount);

                salesTransaction.DiscountAmount = salesTransaction.ActiveSalesLines.Sum(a => a.DiscountAmount);
                salesTransaction.DiscountAmountWithoutTax = salesTransaction.ActiveSalesLines.Sum(a => a.DiscountAmountWithoutTax);
                salesTransaction.PeriodicDiscountAmount = salesTransaction.ActiveSalesLines.Sum(a => a.PeriodicDiscount);
                salesTransaction.TenderDiscountAmount = salesTransaction.ActiveSalesLines.Sum(a => a.TenderDiscountAmount);
            }

            return salesTransaction;
        }

        private static async Task<Customer> GetCustomerAsync(RequestContext context, string customerAccount)
        {
            Customer customer = null;
            if (!string.IsNullOrWhiteSpace(customerAccount))
            {
                GetCustomerDataRequest getCustomerDataRequest = new GetCustomerDataRequest(customerAccount);
                SingleEntityDataServiceResponse<Customer> getCustomerDataResponse = await context.ExecuteAsync<SingleEntityDataServiceResponse<Customer>>(getCustomerDataRequest).ConfigureAwait(false);
                customer = getCustomerDataResponse.Entity;
            }

            return customer ?? (new Customer());
        }

        private static void GetAffiliationDiscounts(RequestContext context, string affiliationId, out List<ExtensionsEntity> entities)
        {
            if (affiliationId == null || affiliationId.IsNullOrEmpty())
            {
                entities = new List<ExtensionsEntity>();
                return;
            }

            using (DatabaseContext databaseContext = new DatabaseContext(context))
            {
                SqlQuery query = new SqlQuery
                {
                    QueryString = $@"Select R2.OFFERID from ax.RETAILAFFILIATIONPRICEGROUP R1 JOIN ax.RetailDiscountPriceGroup R2 on R2.PRICEDISCGROUP = R1.PRICEDISCGROUP WHERE R1.RETAILAFFILIATION = @affiliationId AND R2.DATAAREAID = @dataAreaId"
                };
                query.Parameters["@dataAreaId"] = context.GetChannelConfiguration().InventLocationDataAreaId;
                query.Parameters["@affiliationId"] = affiliationId;

                try
                {
                    entities = databaseContext.ReadEntity<ExtensionsEntity>(query).ToList();
                }
                catch (Exception)
                {
                    entities = new List<ExtensionsEntity>();
                }
            }
        }


        public async Task filterMarginCapDiscounts(CalculateDiscountsServiceRequest request)
        {

            GetMarginCapOnProductAndProductCategory capOnProductAndProductCategory;
            GetMarginCapOnStoreAndLoyaltyProgram getMarginCapOnStoreAndLoyalty;
            CalculateDiscountsServiceRequest discountsServiceRequest;

            bool excludeDiscount, isMarginCapEnabledOnProductAndProductCategory, isMarginCapEnabledForStoreAndLoyaltyProgram = false;
            decimal marginCapPercentageOnStoreAndEntity, marginCapPercentageOnProductAndProductCategory = 0.00M;

            discountsServiceRequest = (CalculateDiscountsServiceRequest)request;
            discountsServiceRequest.Transaction.SetProperty("CSDstoreId", request.RequestContext.GetDeviceConfiguration().StoreNumber);

            if (discountsServiceRequest.Transaction.AffiliationLoyaltyTierLines.Where(a => a.AffiliationType == RetailAffiliationType.Loyalty).Count() == 0)
            {
                return;
            }

            getMarginCapOnStoreAndLoyalty = GetMarginCapOnStoreAndLoyaltyProgram(request);
            isMarginCapEnabledForStoreAndLoyaltyProgram = Convert.ToBoolean(Convert.ToInt32(getMarginCapOnStoreAndLoyalty?.GetProperty("ISMARGINCAPALLOWEDONSTOREANDLOYALTY")?.ToString() ?? decimal.Zero.ToString()));
            marginCapPercentageOnStoreAndEntity = (Convert.ToDecimal(getMarginCapOnStoreAndLoyalty?.GetProperty("MARGINCAPPERCENTAGE")?.ToString() ?? decimal.Zero.ToString()));
            GetTenderDiscountOfferIds(request.RequestContext, out List<string> offerIds);

            if (!isMarginCapEnabledForStoreAndLoyaltyProgram)
            {
                return;
            }

            foreach (var item in request.Transaction.SalesLines)
            {

                if (item.IsReturnLine() || item.IsVoided)
                {
                    continue;
                }
                else
                {
                    capOnProductAndProductCategory = GetMarginCapOnProductAndProductCategory(request, item.ItemId,  await GetProductIdAsync(request, item));
                    isMarginCapEnabledOnProductAndProductCategory = Convert.ToBoolean(Convert.ToInt32(capOnProductAndProductCategory?.GetProperty("ISMARGINCAPALLOWED")?.ToString() ?? decimal.Zero.ToString()));
                    excludeDiscount = Convert.ToBoolean(Convert.ToInt32(capOnProductAndProductCategory?.GetProperty("EXCLUDEDISCOUNT")?.ToString() ?? decimal.Zero.ToString()));
                    marginCapPercentageOnProductAndProductCategory = (Convert.ToDecimal(capOnProductAndProductCategory?.GetProperty("MARGINCAPPERCENTAGE")?.ToString() ?? decimal.Zero.ToString()));


                    if (excludeDiscount)
                    {
                        item.DiscountLines.Clear();
                        item.DiscountAmount = 0;
                        item.DiscountAmountWithoutTax = 0;
                        item.PeriodicDiscount = 0;
                        item.PeriodicPercentageDiscount = 0;
                    }
                    if (isMarginCapEnabledOnProductAndProductCategory)
                    {
                        if (marginCapPercentageOnProductAndProductCategory == 0)
                        {
                            marginCapPercentageOnProductAndProductCategory = marginCapPercentageOnStoreAndEntity;
                        }

                        // decimal totalDiscountPercentageWithoutTenderDiscount = item.DiscountLines.Where(a => !offerIds.Contains(a.OfferId)).Sum(a => a.EffectivePercentage);
                        decimal totalDiscountPercentageWithoutTenderDiscount = item.DiscountLines.Where(a => a.DiscountLineType != DiscountLineType.TenderTypeDiscount ).Sum(a => a.EffectivePercentage);

                        var costPrice = GetCostPrice(item, request);
                        var grossMargin = CalculateGrossMargin(costPrice, item?.Price ?? decimal.Zero);
                        grossMargin = (Convert.ToDecimal(String.Format("{0:0.00}", grossMargin)));

                        if ((grossMargin - marginCapPercentageOnProductAndProductCategory) <= totalDiscountPercentageWithoutTenderDiscount)
                        {
                            decimal newDiscount = (grossMargin - marginCapPercentageOnProductAndProductCategory);

                            if (newDiscount < Decimal.Zero)
                            {
                                newDiscount = Decimal.Zero;
                            }

                            foreach (var discountLine in item.PeriodicDiscountLines())
                            {
                                    discountLine.EffectivePercentage = newDiscount;
                                    discountLine.Percentage = newDiscount;
                                    discountLine.Amount = (item.AgreementPrice * item.Quantity * (discountLine.EffectivePercentage / 100));
                                    discountLine.EffectiveAmount = (item.AgreementPrice * item.Quantity * (discountLine.EffectivePercentage / 100));

                               
                            }

                            item.DiscountAmount = item.DiscountLines.Sum(a => a.EffectiveAmount);
                           // item.DiscountAmountWithoutTax = item.DiscountAmount - item.TaxAmount;
                            item.PeriodicDiscount = item.DiscountLines.Where(a => a.DiscountLineType == DiscountLineType.PeriodicDiscount).Sum(line => line.EffectiveAmount);
                            item.TenderDiscountAmount = item.DiscountLines.Where(a => a.DiscountLineType == DiscountLineType.TenderTypeDiscount).Sum(line => line.EffectiveAmount);

                            item.NetPrice -= item.DiscountAmount;
                          
                        }
                    }
                }
          

            }
            await applyTenderDiscountMarginal(request);

            return;
        }
        public GetMarginCapOnStoreAndLoyaltyProgram GetMarginCapOnStoreAndLoyaltyProgram(Request request)
        {
            CalculateDiscountsServiceRequest discountsServiceRequest = (CalculateDiscountsServiceRequest)request;
            ParameterSet parameters = new ParameterSet();
            parameters["@LoyaltyId"] = discountsServiceRequest.Transaction.AffiliationLoyaltyTierLines.Where(a => a.AffiliationType == RetailAffiliationType.Loyalty).FirstOrDefault().AffiliationId;
            parameters["@StoreNumber"] = request.RequestContext.GetDeviceConfiguration().StoreNumber;
            parameters["@DataAreaId"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                var result = databaseContext.ExecuteStoredProcedure<GetMarginCapOnStoreAndLoyaltyProgram>("ext.GETMARGINCAPONLOYALTYANDSTORE", parameters, QueryResultSettings.AllRecords);
                return result.FirstOrDefault();
            }
        }

        public GetMarginCapOnProductAndProductCategory GetMarginCapOnProductAndProductCategory(Request request, String itemId, long productId)
        {
            CalculateDiscountsServiceRequest discountsServiceRequest = (CalculateDiscountsServiceRequest)request;
            ParameterSet parameters = new ParameterSet();
            parameters["@ProductID"] = productId;
            parameters["@ItemId"] = itemId;
            parameters["@StoreNumber"] = request.RequestContext.GetDeviceConfiguration().StoreNumber;
            parameters["@DataAreaId"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                var result = databaseContext.ExecuteStoredProcedure<GetMarginCapOnProductAndProductCategory>("ext.GETMARGINCAPONPRODUCTANDPRODUCTCATEGORY", parameters, QueryResultSettings.AllRecords);
                return result.FirstOrDefault();
            }
        }
        public async Task<long> GetProductIdAsync(Request request, SalesLine item)
        {
            ProductSearchCriteria searchCriteria = new ProductSearchCriteria(request.RequestContext.GetDeviceConfiguration().ChannelId);
            searchCriteria.Ids.Add(item.ProductId);
            GetProductServiceRequest productServiceRequest = new GetProductServiceRequest(searchCriteria, request.RequestContext.LanguageId, false, QueryResultSettings.AllRecords);
            productServiceRequest.RequestContext = request.RequestContext;
            productServiceRequest.QueryResultSettings = QueryResultSettings.AllRecords;
            ProductSearchServiceResponse result = (ProductSearchServiceResponse)await ExecuteBaseRequestAsync(productServiceRequest);
            return result.ProductSearchResult.Results.FirstOrDefault().RecordId;
        }


        public decimal GetCostPrice(SalesLine item, Request request)
        {
            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                {
                    DatabaseSchema = "ext",
                    Select = new ColumnSet("CostPrice"),
                    From = "CDCPRODUCTVARIANTCOSTPRICE",
                    Where = "DATAAREAID = @dataAreaId AND ITEMID = @itemId AND CONFIGID = @configId AND INVENTLOCATIONID = @inventLocationId AND INVENTCOLORID = @inventColorId AND INVENTSTYLEID = @inventStyleId AND INVENTSIZEID = @inventSizeId",
                    OrderBy = "RECID"
                };

                query.Parameters["@dataAreaId"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
                query.Parameters["@itemId"] = item.ItemId;
                query.Parameters["@inventLocationId"] = item.InventoryLocationId ?? string.Empty;
                query.Parameters["@inventStyleId"] = item.Variant.StyleId ?? string.Empty;
                query.Parameters["@inventColorId"] = item.Variant.ColorId ?? string.Empty;
                query.Parameters["@inventSizeId"] = item.Variant.SizeId ?? string.Empty;
                query.Parameters["@configId"] = item.Variant?.ConfigId ?? string.Empty;

                var itemCostPrice = databaseContext.ReadEntity<ItemCostPrice>(query);
                if (!itemCostPrice.Results.IsNullOrEmpty())
                {
                    decimal costPrice = (Convert.ToDecimal(Convert.ToString(itemCostPrice.FirstOrDefault().GetProperty("COSTPRICE"))));
                    return costPrice;
                }
                else
                {
                    return 0.00M;
                }
            }
        }
        public async Task<Response> ExecuteBaseRequestAsync(Request request)
        {
           // var requestHandler = request.RequestContext.Runtime.GetNextAsyncRequestHandler(request.GetType(), this);
            Response response = await request.RequestContext.Runtime.ExecuteAsync<Response>(request, request.RequestContext).ConfigureAwait(false);
            return response;
        }


    }
}
