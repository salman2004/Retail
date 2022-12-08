using CDC.Commerce.Runtime.FBRIntegration.Entities;
using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
using Microsoft.Dynamics.Retail.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            }
            return new GetCustomReceiptFieldServiceResponse(returnValue);
        }

        /// <summary>
        /// Return Previos balacne read from the card 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private  static string GetPreviousBalance(GetSalesTransactionCustomReceiptFieldServiceRequest request)
        {
            return (request.SalesOrder.LoyaltyCardId == string.Empty || request.SalesOrder.LoyaltyCardId.All(char.IsDigit)) ? string.Empty : request.SalesOrder.AttributeValues.Where(a => a.Name == "CSDCardBalance")?.FirstOrDefault()?.ToString() ?? string.Empty;      
        }
        
        /// <summary>
        /// Return Previos Remaining read from the card 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static string GetRemainingBalance(GetSalesTransactionCustomReceiptFieldServiceRequest request)
        {
            return (request.SalesOrder.LoyaltyCardId == string.Empty || request.SalesOrder.LoyaltyCardId.All(char.IsDigit)) ? string.Empty : request.SalesOrder.AttributeValues.Where(a => a.Name == "CSDMonthlyLimitUsed")?.FirstOrDefault()?.ToString() ?? string.Empty;            
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

       
        private static RetailConfigurationParameter GetConfigurationParameters(GetSalesTransactionCustomReceiptFieldServiceRequest request, string key)
        {
            var configurationRequest = new GetConfigurationParametersDataRequest(request.RequestContext.GetChannelConfiguration().RecordId);
            var configurationResponse = request.RequestContext.ExecuteAsync<EntityDataServiceResponse<RetailConfigurationParameter>>(configurationRequest).Result;

            RetailConfigurationParameter paramter = configurationResponse?.PagedEntityCollection?.Where(cp => string.Equals(cp.Name.ToUpper().Trim(), (key).ToUpper().Trim(), StringComparison.OrdinalIgnoreCase))?.FirstOrDefault();

            return paramter;
        }


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
                if (result.Results.Count < 0)
                {
                    return String.Empty;
                }
                return Convert.ToString(result.FirstOrDefault().GetProperty("CDCFBRINVOICEID"));
            }
        }

        
    }
}
