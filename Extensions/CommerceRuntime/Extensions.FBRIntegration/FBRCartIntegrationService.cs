using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using System.Linq;
using CDC.Commerce.Runtime.FBRIntegration.Entities;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using System.Net.Http.Headers;
using Microsoft.Dynamics.Retail.Diagnostics;
using System.IO;
using System.Collections.ObjectModel;

namespace CDC.Commerce.Runtime.FBRIntegration
{
    public class FBRCartIntegrationService : IRequestTriggerAsync
    {
        public const string SAVETRANSACTIONINVOICEID = "SaveTransactionIvoiceId";
        public const string TRANSACTIONID = "@transactionId";
        public const string INVOICEID = "@InvoiceId";
        public const string DATAAREAID = "@dataAreaId";

        /// <summary>
        /// Gets the supported requests for this trigger.
        /// </summary>
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[] { typeof(SaveSalesTransactionDataRequest) };
            }
        }

        /// <summary>
        /// Post trigger code.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="response">The response.</param>
        public async Task OnExecuted(Request request, Response response)
        {
            try
            {
                SaveSalesTransactionDataRequest transactionDataRequest = (SaveSalesTransactionDataRequest)request;
                if (transactionDataRequest.SalesTransaction.ExtensibleSalesTransactionType == ExtensibleSalesTransactionType.Sales && !transactionDataRequest.SalesTransaction.IsSuspended)
                {
                    await GetInvoiceNumberAsync(request);
                }
            }
            catch (Exception ex)
            {
                RetailLogger.Log.AxGenericErrorEvent($"FBR Integration failed. {ex?.Message ?? string.Empty}");
            }            
            await Task.CompletedTask;
        }

        /// <summary>
        /// Pre trigger code
        /// </summary>
        /// <param name="request">The request.</param>
        public async Task OnExecuting(Request request)
        {
            await Task.CompletedTask;
        }

        public async Task<int> GetPosRegistrationIdAsync(Request request)
        {
            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                {
                    DatabaseSchema = "ext",
                    Select = new ColumnSet("CDCFBRPOSREGISTRATIONID"),
                    From = "RETAILTERMINALTABLE",
                    Where = "TERMINALID = @terminalId ",
                    OrderBy = "CDCFBRPOSREGISTRATIONID"
                };

                query.Parameters["@terminalId"] = request.RequestContext.GetTerminal().TerminalId;

                var itemCostPrice = await databaseContext.ReadEntityAsync<PosRegistration>(query).ConfigureAwait(false);
                return Convert.ToInt32(itemCostPrice.FirstOrDefault().GetProperty("CDCFBRPOSREGISTRATIONID"));
            }
        }

        public async Task<FBRServiceUrl> GetServerIntegrationUrlAsync(Request request)
        {
            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                {
                    DatabaseSchema = "ext",
                    Select = new ColumnSet("CDCFBRSERVERURL", "CDCFBRSERVICECHECKURL"),
                    From = "RETAILSTORETABLE",
                    Where = "CDCFBRSERVERURL != @cdcfbrserverurl AND CDCFBRSERVICECHECKURL != @cdcfbrservicecheckurl AND STORENUMBER = @storenumber",
                    OrderBy = "CDCFBRSERVERURL, CDCFBRSERVICECHECKURL"
                };

                query.Parameters["@storenumber"] = request.RequestContext.GetChannelConfiguration().InventLocation;
                query.Parameters["@cdcfbrserverurl"] = string.Empty;
                query.Parameters["@cdcfbrservicecheckurl"] = string.Empty;
                var fbrServiceUrl = await databaseContext.ReadEntityAsync<FBRServiceUrl>(query).ConfigureAwait(false);
                if (fbrServiceUrl.Count() < 0)
                {
                    throw new CommerceException("CustomError", "FBR Integration Error")
                    {
                        LocalizedMessage = "There was an error finding the FBR service url. Please setup the fbr integration and run CDX job."
                    };
                }
                return fbrServiceUrl.FirstOrDefault();
            }
        }

        public async Task GetInvoiceNumberAsync(Request request)
        {
            SaveSalesTransactionDataRequest saveSalesTransaction = (SaveSalesTransactionDataRequest)request;            
            try
            {
                FBRServiceUrl serviceUrl = await GetServerIntegrationUrlAsync(request);

                if (serviceUrl.IsPropertyDefined("CDCFBRSERVERURL") && serviceUrl.IsPropertyDefined("CDCFBRSERVICECHECKURL"))
                {
                    string integrationUrl = Convert.ToString(serviceUrl.GetProperty("CDCFBRSERVERURL"));
                    string serviceCheckUrl = Convert.ToString(serviceUrl.GetProperty("CDCFBRSERVICECHECKURL"));
                    var res = GetResponseFromFBRService(serviceCheckUrl, null, HttpMethod.Get);
                    if (res.IsSuccessStatusCode)
                    {
                        string body = JsonConvert.SerializeObject(ConvertSalesTransactionToInvoiceAsync(saveSalesTransaction.SalesTransaction, saveSalesTransaction));
                        string json = JObject.Parse(body)["Result"].ToString();
                        
                        HttpResponseMessage response = GetResponseFromFBRService(integrationUrl, json, HttpMethod.Post);
                        if (response.IsSuccessStatusCode)
                        {
                            FbrResponse fbrResponse = JsonConvert.DeserializeObject<FbrResponse>(response.Content.ReadAsStringAsync().Result);
                            await SaveTransactionInvoiceIdAsync(saveSalesTransaction.SalesTransaction.Id, fbrResponse.Response, fbrResponse.InvoiceNumber, request);
                        }
                        else
                        {
                            await SaveTransactionInvoiceIdAsync(saveSalesTransaction.SalesTransaction.Id, response.Content.ReadAsStringAsync().Result.Substring(0, 199), string.Empty, request);
                        }
                    }
                    else
                    {
                        await SaveTransactionInvoiceIdAsync(saveSalesTransaction.SalesTransaction.Id, res.Content.ReadAsStringAsync().Result.Substring(0, 199), string.Empty, request);
                    }
                }
                else
                {
                    throw new CommerceException("CustomError", "FBR Integration Error")
                    {
                        LocalizedMessage = "There was an error finding the FBR service url. Please setup the fbr integration and run CDX job."
                    };
                }                
            }
            catch (Exception ex)
            {
                throw new CommerceException("CustomError", "FBR Integration Error")
                {
                    LocalizedMessage = ex.Message
                };
            }
        }

        public async Task SaveTransactionInvoiceIdAsync(string transactionId, string fbrResponse, string invoiceId, Request request)
        {
            int errorCode;
            ParameterSet parameters = new ParameterSet();
            parameters["@transactionId"] = transactionId;
            parameters["@InvoiceId"] = invoiceId;
            parameters["@fbrResponse"] = fbrResponse;
            parameters["@dataAreaId"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                errorCode = await databaseContext.ExecuteStoredProcedureNonQueryAsync("ext.SaveTransactionIvoiceId", parameters, resultSettings: null).ConfigureAwait(false);
            }
            if (errorCode != 0)
            {
                throw new CommerceException("CustomError", "FBR Integration Error")
                {
                    LocalizedMessage = "There was error saving the FBR invoice id"
                };
            }
        }

        public async Task<Invoice> ConvertSalesTransactionToInvoiceAsync(SalesTransaction salesTransaction, SaveSalesTransactionDataRequest request)
        {

            Invoice invoice = new Invoice();
            invoice.InvoiceNumber = string.Empty;
            invoice.POSID = await GetPosRegistrationIdAsync(request).ConfigureAwait(false);
            invoice.USIN = salesTransaction.ReceiptId;
            invoice.DateTime = DateTime.Now.ToString();
            invoice.BuyerNTN = string.Empty;
            invoice.BuyerCNIC = string.Empty;
            invoice.BuyerName = string.Empty;
            invoice.BuyerPhoneNumber = string.Empty;
            invoice.PaymentMode = GetPyamentMehtod(salesTransaction.TenderLines); //to review
            invoice.TotalSaleValue = Math.Abs((double)salesTransaction.SubtotalAmountWithoutTax);
            invoice.TotalQuantity = Math.Abs((double)salesTransaction.ActiveSalesLines.Sum(a => Math.Abs(a.Quantity)));
            invoice.TotalBillAmount = Math.Abs((double)salesTransaction.TotalAmount);
            invoice.TotalTaxCharged = Math.Abs((double)salesTransaction.TaxAmount);
            invoice.Discount = Math.Abs((double)salesTransaction.DiscountAmount);
            invoice.FurtherTax = 0.000;
            if (salesTransaction.IsReturnByReceipt && salesTransaction.ActiveSalesLines.Any(a=>!a.IsReturnLine()))
            {
                invoice.InvoiceType = 1;
            }
            else if (salesTransaction.IsReturnByReceipt && salesTransaction.ActiveSalesLines.All(a => a.IsReturnLine()))
            {
                invoice.InvoiceType = 3;
            }
            else if (!salesTransaction.IsReturnByReceipt &&  salesTransaction.ActiveSalesLines.All(a => !a.IsReturnLine()))
            {
                invoice.InvoiceType = 1;
            }
            else
            {
                invoice.InvoiceType = 1;
            }

            invoice.Items = new List<InvoiceItem>();
            InvoiceItem item;
            foreach (var salesLine in salesTransaction.ActiveSalesLines)
            {
                if (!salesLine.IsVoided)
                {
                    item = new InvoiceItem();
                    item.ItemCode = salesLine.ItemId;
                    item.ItemName = salesLine.Description.ToString();
                    item.PCTCode = this.GetPCTCode(request, salesLine.ItemId, out int gstType);
                    item.Quantity = Math.Abs((double)salesLine.Quantity);
                    item.TaxRate = (double)salesLine.TaxRatePercent;
                    item.SaleValue = (double)(salesLine.Price - salesLine.TaxAmount - salesLine.DiscountAmount);
                    item.Discount = (double)salesLine.DiscountAmount;
                    item.FurtherTax = 0.00;
                    item.TaxCharged = Math.Abs((double)salesLine.TaxAmount);
                    item.TotalAmount = Math.Abs((double)salesLine.TotalAmount);
                    if (gstType == 1)
                    {
                        item.InvoiceType = salesLine.IsReturnLine() == true ? 12 : 11;
                    }
                    else
                    {
                        item.InvoiceType = salesLine.IsReturnLine() == true ? 3 : 1;
                    }
                    invoice.Items.Add(item);
                }
            }
            return invoice;
        }

        public int GetPyamentMehtod(Collection<TenderLine> tenderLines)
        {
            if (tenderLines.Count() > 1)
            {
                return 5;
            }
            else if (tenderLines.Count() == 1 && tenderLines.FirstOrDefault().TenderTypeId == "1")
            {
                return 1;
            }
            else if (tenderLines.Count() == 1 && tenderLines.FirstOrDefault().TenderTypeId == "3")
            {
                return 2;
            }
            return 1;
        }

        public HttpResponseMessage GetResponseFromFBRService(string address, string invoice, HttpMethod method)
        {
            HttpResponseMessage response;
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "1298b5eb-b252-3d97-8622-a4a69d5bf818");
            if (method == HttpMethod.Post)
            {
                StringContent content = new StringContent(invoice, Encoding.UTF8, "application/json");
                System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                response = client.PostAsync(address, content).Result;
            }
            else
            {
                response = client.GetAsync(address).Result;
            }
            return response;
        }

        public string GetPCTCode(Request request, string itemId, out int gstType)
        {
            gstType = 0;
            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                {
                    DatabaseSchema = "ext",
                    Select = new ColumnSet("FBRHSCODE,CDCGSTTYPE"),
                    From = "INVENTTABLE",
                    Where = "ITEMID = @itemId and DATAAREAID = @dataAreaId",
                    OrderBy = "FBRHSCODE,CDCGSTTYPE"
                };

                query.Parameters["@dataAreaId"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
                query.Parameters["@itemId"] = itemId;
                var getPctCode = databaseContext.ReadEntity<PCTCodeEntity>(query);
                gstType = Convert.ToInt32(getPctCode.FirstOrDefault()?.GetProperty("CDCGSTTYPE") ?? 0);
                return Convert.ToString(getPctCode.FirstOrDefault()?.GetProperty("FBRHSCODE") ?? string.Empty);
            }
        }

    }
}
