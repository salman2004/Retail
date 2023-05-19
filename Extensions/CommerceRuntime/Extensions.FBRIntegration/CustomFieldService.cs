using CDC.Commerce.Runtime.FBRIntegration.Entities;
using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EncodeQrCodeServiceRequest = Microsoft.Dynamics.Commerce.Runtime.Localization.Services.Messages.EncodeQrCodeServiceRequest;
using EncodeQrCodeServiceResponse = Microsoft.Dynamics.Commerce.Runtime.Localization.Services.Messages.EncodeQrCodeServiceResponse;

namespace CDC.Commerce.Runtime.FBRIntegration
{
    public class CustomFieldService : IRequestHandlerAsync
    {
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[] { typeof(GetSalesTransactionCustomReceiptFieldServiceRequest) };
            }
        }

        public async Task<Response> Execute(Request request)
        {
            Type requestedType = request.GetType();
            if (requestedType == typeof(GetSalesTransactionCustomReceiptFieldServiceRequest))
            {
                return await this.GetCustomReceiptFieldForSalesTransactionReceiptsAsync((GetSalesTransactionCustomReceiptFieldServiceRequest)request);
            }
            throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
        }

        private async Task<GetCustomReceiptFieldServiceResponse> GetCustomReceiptFieldForSalesTransactionReceiptsAsync(GetSalesTransactionCustomReceiptFieldServiceRequest request)
        {
            string receiptFieldName = request.CustomReceiptField.Trim();
            string returnValue = null;
            switch (receiptFieldName)
            {
                case "TOTALDISCOUNT":
                    {
                        returnValue = GetTotalDiscount(request);
                    }
                    break;
                case "FBRINVOICEID":
                    {
                        returnValue = await GetFBRIntegartionInvoiceIdAsync(request);                        
                    }
                    break;
                case "TAXINVOICE_QR":
                    {
                        returnValue = await GetQRCode(request).ConfigureAwait(false);
                    }                    
                    break;
                case "PREVIOUSBALANCE":
                    {
                        returnValue = GetPreviousBalance(request);
                    }
                    break;
                case "REMAIN":
                    {
                        returnValue = GetRemainingBalance(request);
                    }
                    break;
                case "EHSAASDISCOUNTS":
                    {
                        returnValue = GetEhsasDiscountAmount(request);
                    }
                    break;
                case "FBRCHARGES":
                    {
                        returnValue = GetFbrChargeAmount(request);
                    }
                    break;
                case "RECEIPTSUBTOTAL":
                    {
                        returnValue = GetReceiptSubtotal(request);
                    }
                    break;
                case "RECEIPTITEMS":
                    {
                        returnValue = GetReceiptItems(request);
                    }
                    break;
                case "CSDTOTALEXCLUDINGGST":
                    {
                        returnValue = GetTotalExcludingGst(request);
                    }
                    break;
                case "CSDGRANDTOTAL":
                    {
                        returnValue = GetGrandTotal(request);
                    }
                    break;
                case "CSDREBATE_LOYALTY":
                    {
                        returnValue = GetRebateLoyalty(request);
                    }
                    break;
                case "CSDTOBEPAID":
                    {
                        returnValue = GetToBePaid(request);
                    }
                    break;
                case "CSDOTHERDISCOUNT":
                    {
                        returnValue = GetOtherDiscount(request);
                    }
                    break;
                case "TOTALCHARGESWITHOUTEHSAASDDISCOUNT":
                    {
                        returnValue = GetTotalChargesWithoutEhsaasdDiscount(request);
                    }
                    break;
                case "CCR":
                    {
                        returnValue = GetCCRCharge(request);
                    }
                    break;
            }
            return new GetCustomReceiptFieldServiceResponse(returnValue);
        }

        /// <summary>
        /// Total Discount
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Total Discount</returns>
        private static string GetTotalDiscount(GetSalesTransactionCustomReceiptFieldServiceRequest request)
        {
            decimal totalDiscount = request.SalesOrder.ActiveSalesLines.Sum(sl => sl.DiscountAmount);
            return String.Format("{0:0.00}", totalDiscount);
        }

        /// <summary>
        /// ToBePaid
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static string GetCCRCharge(GetSalesTransactionCustomReceiptFieldServiceRequest request)
        {
            string ccrCharges = String.Format("{0:0.00}", request.SalesOrder.ChargeLines?.Where(cl => cl.ChargeCode == GetConfigurationParameters(request, "CardRefundChargeCode").Value)?.FirstOrDefault()?.CalculatedAmount ?? decimal.Zero);
            return ccrCharges == decimal.Zero.ToString() ? string.Empty : ccrCharges;
        }

        /// <summary>
        /// ToBePaid
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static string GetTotalChargesWithoutEhsaasdDiscount(GetSalesTransactionCustomReceiptFieldServiceRequest request)
        {
            string ehsasDiscount = GetEhsasDiscountAmount(request);
            return String.Format("{0:0.00}", request.SalesOrder.ChargeLines.Sum(a=> a.CalculatedAmount) - Convert.ToDecimal(ehsasDiscount));
        }

        /// <summary>
        /// ToBePaid
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static string GetOtherDiscount(GetSalesTransactionCustomReceiptFieldServiceRequest request)
        {
            string rebateOrLoyaltyDiscount = GetRebateLoyalty(request);
            rebateOrLoyaltyDiscount = rebateOrLoyaltyDiscount == string.Empty ? decimal.Zero.ToString() : rebateOrLoyaltyDiscount;
            return String.Format("{0:0.00}", request.SalesOrder.ActiveSalesLines.Sum(sl => sl.DiscountAmount) - Convert.ToDecimal(rebateOrLoyaltyDiscount));
        }

        /// <summary>
        /// ToBePaid
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static string GetToBePaid(GetSalesTransactionCustomReceiptFieldServiceRequest request)
        {
            return String.Format("{0:0.00}", Convert.ToDecimal(GetGrandTotal(request)) - request.SalesOrder.ActiveSalesLines.Sum(sl => sl.DiscountAmount) + (request.SalesOrder.ChargeLines.Sum(cl => cl.CalculatedAmount) -  Convert.ToDecimal(GetFbrChargeAmount(request))));
        }

        /// <summary>
        /// RebateLoyalty
        /// any discount on card
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static string GetRebateLoyalty(GetSalesTransactionCustomReceiptFieldServiceRequest request)
        {
            string value = string.Empty;
            if (!string.IsNullOrEmpty(request.SalesOrder.LoyaltyCardId))
            {
                decimal discount = decimal.Zero;
                var affiliation = request.SalesOrder.AffiliationLoyaltyTierLines.Where(a => a.AffiliationType == RetailAffiliationType.Loyalty).FirstOrDefault();
                GetAffiliationDiscounts(request.RequestContext, affiliation.AffiliationId.ToString() ?? string.Empty, out List<ExtensionsEntity> discountExtensionEntity);
                if (!discountExtensionEntity.IsNullOrEmpty())
                {
                    List<string> offerIds = discountExtensionEntity.Select(a => a.GetProperty("OFFERID").ToString()).ToList();
                    discount = request.SalesOrder.ActiveSalesLines.Where(sl => sl.DiscountAmount > 0 && !sl.DiscountLines.IsNullOrEmpty()).Sum(sl => sl.DiscountLines.Where(dl => offerIds.Contains(dl.OfferId)).Sum(dl => dl.EffectiveAmount));
                }
                value = String.Format("{0:0.00}", discount);
            }
            return value;
        }
        
        /// <summary>
        /// Grand Total
        /// GetTotalExcludingGst + FBR + Tax
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static string GetGrandTotal(GetSalesTransactionCustomReceiptFieldServiceRequest request)
        {
            return String.Format("{0:0.00}", decimal.Add(Convert.ToDecimal(GetReceiptSubtotal(request)), Convert.ToDecimal(GetFbrChargeAmount(request))));
        }

        /// <summary>
        /// CSD TOTAL EXCLUDING GST
        /// subtotal - tax
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static string GetTotalExcludingGst(GetSalesTransactionCustomReceiptFieldServiceRequest request)
        {
            return String.Format("{0:0.00}", request.SalesOrder.ActiveSalesLines?.Sum(sl => (sl.Price * sl.Quantity) - sl.TaxAmount) ?? decimal.Zero);       
        }
        
        /// <summary>
        /// Return Receipt Subtotal
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static string GetReceiptItems(GetSalesTransactionCustomReceiptFieldServiceRequest request)
        {
            return String.Format("{0:0.00}", request.SalesOrder.ActiveSalesLines?.Count ?? decimal.Zero);
        }

        /// <summary>
        /// Return Receipt Subtotal
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static string GetReceiptSubtotal(GetSalesTransactionCustomReceiptFieldServiceRequest request)
        {
            return String.Format("{0:0.00}", request.SalesOrder.ActiveSalesLines?.Sum(sl => sl.Price * sl.Quantity) ?? decimal.Zero);
        }

        /// <summary>
        /// Return ehsas discount
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static string GetEhsasDiscountAmount(GetSalesTransactionCustomReceiptFieldServiceRequest request)
        {
            GetConfigurationParameters(request.RequestContext, "EhsaasChargeCode", out string result);
            return String.Format("{0:0.00}", request.SalesOrder.ChargeLines.Where(cl => cl.ChargeCode == result)?.FirstOrDefault()?.CalculatedAmount ?? decimal.Zero);
        }

        /// <summary>
        /// Return fbr charges amount
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static string GetFbrChargeAmount(GetSalesTransactionCustomReceiptFieldServiceRequest request)
        {
            GetConfigurationParameters(request.RequestContext, "FbrChargeCode", out string result);
            return String.Format("{0:0.00}", request.SalesOrder.ChargeLines.Where(cl => cl.ChargeCode == result).FirstOrDefault()?.CalculatedAmount ?? decimal.Zero);
        }

        /// <summary>
        /// Return Previos balacne read from the card 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private  static string GetPreviousBalance(GetSalesTransactionCustomReceiptFieldServiceRequest request)
        {
            string value = (request.SalesOrder.LoyaltyCardId == string.Empty || request.SalesOrder.LoyaltyCardId.All(char.IsDigit)) ? string.Empty : request.SalesOrder.AttributeValues.Where(a => a.Name == "CSDCardBalance")?.FirstOrDefault()?.ToString() ?? string.Empty;
            if (!value?.All(a => a.Equals('0')) ?? false)
            {
                value = value.TrimStart('0');
            }
            return value;
        }
        
        /// <summary>
        /// Return Previos Remaining read from the card 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static string GetRemainingBalance(GetSalesTransactionCustomReceiptFieldServiceRequest request)
        {
            string value = (request.SalesOrder.LoyaltyCardId == string.Empty || request.SalesOrder.LoyaltyCardId.All(char.IsDigit)) ? string.Empty : request.SalesOrder.AttributeValues.Where(a => a.Name == "CSDMonthlyLimitUsed")?.FirstOrDefault()?.ToString() ?? string.Empty;
            if (!value?.All(a => a.Equals('0')) ?? false)
            {
                value = value.TrimStart('0');
            }
            return value;
        }
        /// <summary>
        /// Converts an image from "png" to "bmp".
        /// </summary>
        /// <param name="qrCode">Base64 represents the image to convert.</param>
        /// <returns>The image as base64.</returns>
        private static string ConvertImagePNGToBMP(string qrCode)
        {
            string convertedQRCode = qrCode;

            byte[] imageBytes = Convert.FromBase64String(qrCode);
            using (MemoryStream msFrom = new MemoryStream(imageBytes))
            {
                var image = Image.FromStream(msFrom);
                using (MemoryStream msTo = new MemoryStream())
                {
                    image.Save(msTo, ImageFormat.Bmp);
                    convertedQRCode = Convert.ToBase64String(msTo.ToArray());
                }
            }

            return convertedQRCode;
        }

        /// <summary>
        /// Gets the QR code for the receipt.
        /// </summary>
        /// <param name="request">The service request to get customreceipt field value.</param>
        /// <returns>QR code custom field value.</returns>
        public async Task<string> GetQRCode(GetSalesTransactionCustomReceiptFieldServiceRequest request)
        {
            var salesOrder = request.SalesOrder;
            string receiptFieldValue = string.Empty;
            if (true)
            {
                string fbrInvoiceId = await this.GetFBRIntegartionInvoiceIdAsync(request);
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append($"{fbrInvoiceId}");

                int qrCodeWidth = Convert.ToInt32(GetConfigurationParameters(request, "QrCodeWidth").Value);
                int qrCodeHeight = Convert.ToInt32(GetConfigurationParameters(request, "QrCodeHeight").Value);

                var qrCodeRequest = new EncodeQrCodeServiceRequest(stringBuilder.ToString())
                {
                    Width = qrCodeWidth, // Replace with desired QR code width
                    Height = qrCodeHeight // Replace with desired QR code width
                };
                EncodeQrCodeServiceResponse qrCodeDataResponse = await
                request.RequestContext.ExecuteAsync<EncodeQrCodeServiceResponse>(qrCodeRequest).ConfigureAwait(false);

                string qrCode = ConvertImagePNGToBMP(qrCodeDataResponse.QRCode);
                receiptFieldValue = $"<I:{qrCode}>";
                return receiptFieldValue;
            }
        }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="request"></param>
       /// <param name="key"></param>
       /// <returns></returns>
        private static RetailConfigurationParameter GetConfigurationParameters(GetSalesTransactionCustomReceiptFieldServiceRequest request, string key)
        {
            var configurationRequest = new GetConfigurationParametersDataRequest(request.RequestContext.GetChannelConfiguration().RecordId);
            var configurationResponse = request.RequestContext.ExecuteAsync<EntityDataServiceResponse<RetailConfigurationParameter>>(configurationRequest).Result;

            RetailConfigurationParameter paramter = configurationResponse?.PagedEntityCollection?.Where(cp => string.Equals(cp.Name.ToUpper().Trim(), (key).ToUpper().Trim(), StringComparison.OrdinalIgnoreCase))?.FirstOrDefault();

            return paramter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<string> GetFBRIntegartionInvoiceIdAsync(GetSalesTransactionCustomReceiptFieldServiceRequest request)
        {
            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                {
                    DatabaseSchema = "ext",
                    Select = new ColumnSet("CDCFBRINVOICEID"),
                    From = "CDCFBRINVOICEINTEGRATION",
                    Where = "TRANSACTIONID = @transactionId ",
                    OrderBy = "CDCFBRINVOICEID"
                };

                query.Parameters["@transactionId"] = request.SalesOrder.Id;

                var result = await databaseContext.ReadEntityAsync<InvoiceId>(query).ConfigureAwait(false);
                if (result.FirstOrDefault().GetProperty("CDCFBRINVOICEID")?.ToString()?.Trim() == string.Empty || result.FirstOrDefault().GetProperty("CDCFBRINVOICEID")?.ToString()?.Trim() == null)
                {
                    return String.Empty;
                }
                return Convert.ToString(result.FirstOrDefault().GetProperty("CDCFBRINVOICEID"));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configName"></param>
        /// <param name="result"></param>
        public static void GetConfigurationParameters(RequestContext context, string configName, out string result)
        {
            result = string.Empty;

            // Get the configuration parameters
            var configurationRequest = new GetConfigurationParametersDataRequest(context.GetChannelConfiguration().RecordId);
            var configurationResponse = context.ExecuteAsync<EntityDataServiceResponse<RetailConfigurationParameter>>(configurationRequest).Result;

            string value = configurationResponse?.PagedEntityCollection?.Where(cp => string.Equals(cp.Name.ToUpper().Trim(), (configName).ToUpper().Trim(), StringComparison.OrdinalIgnoreCase))?.FirstOrDefault()?.Value ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(value))
            {
                result = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="affiliationId"></param>
        /// <param name="entities"></param>
        private static void GetAffiliationDiscounts(RequestContext context, string affiliationId, out List<ExtensionsEntity> entities)
        {
            if (affiliationId == null || affiliationId.IsNullOrEmpty())
            {
                entities = new List<ExtensionsEntity>();
                return;
            }

            using (DatabaseContext databaseContext = new DatabaseContext(context))
            {
                SqlQuery query = new SqlQuery();
                query.QueryString = $@"Select R2.OFFERID from ax.RETAILAFFILIATIONPRICEGROUP R1 JOIN ax.RetailDiscountPriceGroup R2 on R2.PRICEDISCGROUP = R1.PRICEDISCGROUP WHERE R1.RETAILAFFILIATION = @affiliationId AND R2.DATAAREAID = @dataAreaId";
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

    }
}
