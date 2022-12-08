using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
using Microsoft.Dynamics.Retail.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDC.Commerce.Runtime.MarginCap.RequestHandlers
{
    public class SortingServiceRequestHandler : IRequestHandlerAsync
    {
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(CalculateDiscountsServiceRequest)
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {
            if (request.GetType() == typeof(CalculateDiscountsServiceRequest))
            {
                return await SortingCart((CalculateDiscountsServiceRequest)request);
            }
            else
            {
                throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
            }
        }

        private async Task<Response> SortingCart(CalculateDiscountsServiceRequest request)
        {
            GetLoyaltyCardDetails(out string cardNumber, request);
            if (string.IsNullOrWhiteSpace(request.Transaction.LoyaltyCardId)
                || string.IsNullOrWhiteSpace(cardNumber)
                || !request.Transaction.LoyaltyCardId.Equals(cardNumber)
                || !request.Transaction.AffiliationLoyaltyTierLines.Any(alt => alt.AffiliationType == RetailAffiliationType.Loyalty)
                || !request.Transaction.LoyaltyCardId.Equals(cardNumber))
            {
                return await this.ExecuteNextAsync<Response>(request);
            }
            else
            {
                GetItemGSTDetails(request.RequestContext, request.Transaction, out List<ExtensionsEntity> entities);
                entities = CalculateGrossProfit(entities, request.Transaction,request);
                entities = entities.ToList();

                Collection<SalesLine> salesLines = new Collection<SalesLine>();
                foreach (var item in entities)
                {
                    SalesLine line = request.Transaction.SalesLines.Where(a => a.LineId == (item.GetProperty("LineId")?.ToString()?.Trim() ?? string.Empty)).FirstOrDefault();
                    if (line != null)
                    {
                        line.LineNumber = salesLines.Count + 1;
                        salesLines.Add(line);
                    }
                }

                request.Transaction.SalesLines = salesLines; //new Collection<SalesLine>(request.Transaction.SalesLines.OrderByDescending(a => a.GetProperty("CDCTOPONCART")).ThenByDescending(x => x.GetProperty("CDCPRICINGPRIORITY")).ThenByDescending(z => z.GetProperty("GrossProfit")).ToList());
                return await this.ExecuteNextAsync<Response>(request);
            }
            //return new GetPriceServiceResponse(request.Transaction);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <param name="request"></param>
        private void GetLoyaltyCardDetails(out string cardNumber, CalculateDiscountsServiceRequest request)
        {
            cardNumber = request.Transaction?.GetProperty("CSDCardNumber")?.ToString()?.Trim() ?? string.Empty;   
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="salesTransaction"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public List<ExtensionsEntity> CalculateGrossProfit(List<ExtensionsEntity> entities, SalesTransaction salesTransaction, CalculateDiscountsServiceRequest request)
        {
            try
            {
                foreach (var item in entities)
                {
                    SalesLine line = request.Transaction.SalesLines.Where(sl => sl.ItemId.Equals(Convert.ToString(item.GetProperty("ITEMID") ?? string.Empty))
                                     && sl.Variant.ColorId == ((item.GetProperty("INVENTCOLORID")?.ToString()?.Trim() ?? string.Empty) != string.Empty ? Convert.ToString(item.GetProperty("INVENTCOLORID")) : null)
                                     && sl.Variant.StyleId == ((item.GetProperty("INVENTSTYLEID")?.ToString()?.Trim() ?? string.Empty) != string.Empty ? Convert.ToString(item.GetProperty("INVENTSTYLEID")) : null)
                                     && sl.Variant.SizeId == ((item.GetProperty("INVENTSIZEID")?.ToString()?.Trim() ?? string.Empty) != string.Empty ? Convert.ToString(item.GetProperty("INVENTSIZEID")) : null)
                                     && sl.Variant.ConfigId == ((item.GetProperty("CONFIGID")?.ToString()?.Trim() ?? string.Empty) != string.Empty ? Convert.ToString(item.GetProperty("CONFIGID")) : null)
                                     ).FirstOrDefault();

                    item.SetProperty("GrossProfit", CalculateGrossMargin(Convert.ToDecimal(item.GetProperty("COSTPRICE") ?? decimal.Zero), line?.Price ?? decimal.Zero));
                    item.SetProperty("LineId", line?.LineId);
                }
                return entities.OrderByDescending(a => a.GetProperty("CDCTOPONCART")).ThenByDescending(x => x.GetProperty("CDCPRICINGPRIORITY")).ThenByDescending(z => z.GetProperty("GrossProfit")).ToList();
            }
            catch (Exception ex)
            {
                RetailLogger.Log.AxGenericErrorEvent(string.Format("Error when sorting: {0}", ex.Message));
                return entities;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="costPrice"></param>
        /// <param name="sellPrice"></param>
        /// <returns></returns>
        private decimal CalculateGrossMargin(decimal costPrice, decimal sellPrice)
        {
            if (costPrice == decimal.Zero || sellPrice == decimal.Zero)
            {
                return decimal.Zero;
            }
            else
            {
                return ((sellPrice - costPrice) / sellPrice) * 100;
            }
        }

        /// <summary>
        /// Gets sorting 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="transaction"></param>
        /// <param name="entities"></param>
        private void GetItemGSTDetails(RequestContext context, SalesTransaction transaction, out List<ExtensionsEntity> entities)
        {
            if (transaction == null || transaction.ActiveSalesLines.IsNullOrEmpty())
            {
                entities = new List<ExtensionsEntity>();
                return;
            }

            using (DatabaseContext databaseContext = new DatabaseContext(context))
            {
                SqlQuery query = new SqlQuery();
                query.QueryString = $@"SELECT DISTINCT IT.ITEMID, IT.CDCGSTTYPE, IT.CDCTOPONCART, DP.CDCPRICINGPRIORITY,C1.COSTPRICE, C1.INVENTCOLORID, C1.CONFIGID, C1.INVENTSTYLEID, C1.INVENTSIZEID, C1.INVENTLOCATIONID FROM [ext].[INVENTTABLE] IT FULL OUTER JOIN[ext].[CDCGSTTYPEDISCOUNTPRIORITY] DP ON IT.CDCGSTTYPE = DP.CDCGSTTYPE FULL OUTER JOIN ext.CDCPRODUCTVARIANTCOSTPRICE C1 on C1.ITEMID = IT.ITEMID WHERE IT.ITEMID IN({string.Join(",", transaction.SalesLines.Select(sl => "'" + sl.ItemId + "'"))}) AND IT.DATAAREAID = @dataAreaId AND C1.INVENTLOCATIONID = @inventLocationId  order by CDCTOPONCART desc, CDCPRICINGPRIORITY desc";
                query.Parameters["@dataAreaId"] = context.GetChannelConfiguration().InventLocationDataAreaId;
                query.Parameters["@inventLocationId"] = context.GetDeviceConfiguration().InventLocationId;

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
    }
}
