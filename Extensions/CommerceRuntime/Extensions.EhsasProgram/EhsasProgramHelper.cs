using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Product = CDC.Commerce.Runtime.EhsasProgram.Model.Product;
using System.Text;
using CDC.Commerce.Runtime.EhsasProgram.Model;
using types = Microsoft.Dynamics.Commerce.Runtime.Data.Types;
using System.Threading.Tasks;
using Microsoft.Dynamics.Commerce.Runtime.Framework;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using System.Text.RegularExpressions;

namespace CDC.Commerce.Runtime.EhsasProgram
{
    public static class EhsasProgramHelper
    {
        public const string SAVEEHSAASPROGRAMMEINFORMATION = "ext.SAVEEHSAASPROGRAMMEINFORMATION";
        public const string TRANSACTIONID = "@transactionId";
        public const string INFORMATIONCODE = "@InformationCode";
        public const string INFORMATION = "@Information";
        public const string DATAAREAID = "@dataAreaId";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static RetailConfigurationParameter GetConfigurationParameters(RequestContext context, string key)
        {
            var configurationRequest = new GetConfigurationParametersDataRequest(context.GetChannelConfiguration().RecordId);
            var configurationResponse = context.ExecuteAsync<EntityDataServiceResponse<RetailConfigurationParameter>>(configurationRequest).Result;

            RetailConfigurationParameter paramter = configurationResponse?.PagedEntityCollection?.Where(cp => string.Equals(cp.Name.ToUpper().Trim(), (key).ToUpper().Trim(), StringComparison.OrdinalIgnoreCase))?.FirstOrDefault();

            return paramter;
        }

        /// <summary>
        /// Get Product Size to multiply with quatity
        /// </summary>
        /// <param name="request"></param>
        /// <param name="inventDimCombination"></param>
        public static void GetProductsSize(RequestContext context, List<string> inventDimCombinations, out List<ExtensionsEntity> entities)
        {
            if (inventDimCombinations == null || inventDimCombinations.Count() == 0)
            {
                entities = new List<ExtensionsEntity>();
                return;
            }

            using (DatabaseContext databaseContext = new DatabaseContext(context))
            {
                SqlQuery query = new SqlQuery();
                query.QueryString = $@"SELECT INVENTSIZEID, INVENTDIMID FROM [ax].INVENTDIM WHERE INVENTDIMID IN({string.Join(",", inventDimCombinations.Select(p => "'" + p + "'"))}) AND DATAAREAID = @dataAreaId";
                query.Parameters["@dataAreaId"] = context.GetChannelConfiguration().InventLocationDataAreaId;

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

        /// <summary>
        /// api helper
        /// </summary>
        /// <param name="address"></param>
        /// <param name="httpClient"></param>
        /// <param name="body"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static HttpResponseMessage GetResponseFromEhsasProgramService(string address, HttpClient httpClient, string body, HttpMethod method)
        {
            HttpResponseMessage response;
            HttpClient client = httpClient;

            if (method == HttpMethod.Post)
            {
                StringContent content = new StringContent(body, Encoding.UTF8, "application/json");
                System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                response = client.PostAsync(address, content).Result;
            }
            else
            {
                response = client.GetAsync(address).Result;
            }
            return response;
        }

        /// <summary>
        /// Random Number generator
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string GenerateRandomString(int size)
        {
            Random random = new Random();
            StringBuilder builder = new StringBuilder();
            int ch;
            for (int i = 0; i < size; i++)
            {
                ch = random.Next(0, 9);
                builder.Append(ch);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Get Ehsas program codes from db
        /// </summary>
        /// <param name="context"></param>
        public static  void GetEhsasProgramCodes(RequestContext context, Product[] products, out List<ExtensionsEntity> entities)
        {
            if (products == null || products.Count() == 0)
            {
                entities = new List<ExtensionsEntity>();
                return;
            }

            using (DatabaseContext databaseContext = new DatabaseContext(context))
            {
                SqlQuery query = new SqlQuery();
                query.QueryString = $@"SELECT CDCEHSASPROGRAMCODE, ITEMID  FROM [ext].INVENTTABLE WHERE ITEMID IN({string.Join(",", products.Select(p => "'" + p.ItemId + "'"))}) AND DATAAREAID = @dataAreaId";
                query.Parameters["@dataAreaId"] = context.GetChannelConfiguration().InventLocationDataAreaId;

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task<Cart> GetCurrentSalesTransactionAsync(string transactionId, RequestContext context)
        {
            CartSearchCriteria cartSearchCriteria = new CartSearchCriteria(transactionId);
            GetCartRequest getCartRequest = new GetCartRequest(cartSearchCriteria, QueryResultSettings.SingleRecord);
            getCartRequest.RequestContext = context;
            GetCartResponse response = await context.Runtime.ExecuteAsync<GetCartResponse>(getCartRequest, context);
            return response.Carts.FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="grossMarginCap"></param>
        public static void GetConfigurationParameters(RequestContext context,string configName ,out string result)
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
        /// <param name="transactionId"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task SaveInfoCodeLine(string transactionId, RequestContext context, string infoCodetext, string infoCodeId)
        {
            if (string.IsNullOrEmpty(infoCodetext) || string.IsNullOrEmpty(infoCodeId))
            {
                return;
            }
            int errorCode;
            ParameterSet parameters = new ParameterSet();
            parameters[TRANSACTIONID] = transactionId;
            parameters[DATAAREAID] = context.GetChannelConfiguration().InventLocationDataAreaId;
            parameters[INFORMATION] = infoCodetext;
            parameters[INFORMATIONCODE] = infoCodeId;

            try
            {
                using (DatabaseContext databaseContext = new DatabaseContext(context))
                {
                    errorCode = await databaseContext.ExecuteStoredProcedureNonQueryAsync(SAVEEHSAASPROGRAMMEINFORMATION, parameters, resultSettings: null).ConfigureAwait(false);
                }

                if (errorCode != 0)
                {
                    throw new CommerceException("CustomError", "Ehsaas Integration Error")
                    {
                        LocalizedMessage = "There was error saving the Ehsaas Information"
                    };
                }
            }
            catch (Exception ex)
            {
                throw new CommerceException("CustomError", "Ehsaas Integration Error")
                {
                    LocalizedMessage = ex.Message
                };
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task<Response> UpdateCart(Cart cart, RequestContext context)
        {
            UpdateCartRequest updateCartRequest = new UpdateCartRequest(cart);
            updateCartRequest.RequestContext = context;
            return await context.Runtime.ExecuteAsync<Response>(updateCartRequest, context);
        }

        /// <summary>
        /// 
        /// </summary>
        public static async Task<Response> AddSubsidyAsChargeAsync(RequestContext context, string currentTransactionId, decimal totalSubsidy)
        {
            GetConfigurationParameters(context, "EhsaasChargeCode", out string chargeCode);
            AddChargeRequest addChargeRequest = new AddChargeRequest(currentTransactionId, Microsoft.Dynamics.Commerce.Runtime.DataModel.ChargeModule.Sales, chargeCode, Decimal.Negate(totalSubsidy));
            addChargeRequest.RequestContext = context;
            return await context.Runtime.ExecuteAsync<Response>(addChargeRequest, context);
        }

        /// <summary>
        /// GetSubsidyInquiryParamters
        /// </summary>
        /// <param name="commodities"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static SubsidyInquiryEntity GetSubsidyInquiryParamters(RequestContext context,List<Commodity> commodities, List<ExtensionsEntity> entities, Product[] products, string cnic)
        {
            decimal productSize = decimal.One;
            SubsidyInquiryEntity subsidyInquiryEntity = new SubsidyInquiryEntity();
            List<SubsidyInquiryCommodity> subsidyInquiryCommodity = new List<SubsidyInquiryCommodity>();
            GetProductsSize(context, products.Select(p => p.InventDimId).ToList(), out List<ExtensionsEntity> productSizes);

            foreach (var item in entities.Where(en => commodities.Any(cm => cm.code == (en.GetProperty("CDCEHSASPROGRAMCODE")?.ToString() ?? string.Empty))))
            {
                foreach (var product in products.Where(pd => pd.ItemId == item.GetProperty("ITEMID").ToString())) 
                {
                    productSize = Convert.ToDecimal(Regex.Match(productSizes?.Where(ps => ps.GetProperty("INVENTDIMID").ToString() == product.InventDimId)?.FirstOrDefault()?.GetProperty("INVENTSIZEID")?.ToString() ?? decimal.One.ToString(), @"\d+\.*\d*").Value);
                    subsidyInquiryCommodity.Add(new SubsidyInquiryCommodity
                    {
                        code = item.GetProperty("CDCEHSASPROGRAMCODE").ToString(),
                        amount = product.Amount * product.Quantity,
                        defaultRate = decimal.Zero,
                        name = product.ItemId,
                        qty = productSize,
                        rate = product.Amount / productSize

                    });
                }
            }

            subsidyInquiryEntity.commodity = subsidyInquiryCommodity;
            subsidyInquiryEntity.info = new Info
            {
                rrn = EhsasProgramHelper.GenerateRandomString(12),
                stan = EhsasProgramHelper.GenerateRandomString(6)
            };

            subsidyInquiryEntity.subsidyInquiryReqTxnInfo = new SubsidyInquiryReqTxnInfo
            {
                cnic = cnic,
                dateTime = DateTime.Now.ToString("yyyyMMddhhmmss"),
                itemsCount = subsidyInquiryCommodity.Count(),
                merchantId = EhsasProgramHelper.GetMerchantId(context).GetProperty("CDCMERCHANTID")?.ToString() ?? string.Empty,// request.RequestContext.GetTerminalId();
                totalValue = products.Sum(p => p.Amount * p.Quantity)
            };

            return subsidyInquiryEntity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ExtensionsEntity GetEhsasProgramConfigurations(RequestContext context)
        {
            ExtensionsEntity entity;
            using (DatabaseContext databaseContext = new DatabaseContext(context))
            {
                var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                {
                    DatabaseSchema = "ext",
                    Select = new ColumnSet("CDCEHSASPROGRAMAUTHENTICATIONLINK,CDCEHSASPROGRAMBENEVERIFICATIONLINK,CDCEHSASPROGRAMSUBSIDYINQUIRYLINK,CDCEHSASPROGRAMSUBSIDYPAYMENTLINK,CDCEHSASPROGRAMSUBSIDYPAYMENTINQUIRYLINK, CDCEHSASPROGRAMRESENDOTPLINK ,CDCEHSASPROGRAMUSERNAME,CDCEHSASPROGRAMPASSWORD,CDCEHSASPROGRAMCHANNEL"),
                    From = "RETAILPARAMETERS",
                    Where = "DATAAREAID = @dataAreaId",
                    OrderBy = "CDCEHSASPROGRAMAUTHENTICATIONLINK,CDCEHSASPROGRAMBENEVERIFICATIONLINK,CDCEHSASPROGRAMSUBSIDYINQUIRYLINK,CDCEHSASPROGRAMSUBSIDYPAYMENTLINK,CDCEHSASPROGRAMSUBSIDYPAYMENTINQUIRYLINK, CDCEHSASPROGRAMRESENDOTPLINK ,CDCEHSASPROGRAMUSERNAME,CDCEHSASPROGRAMPASSWORD,CDCEHSASPROGRAMCHANNEL"
                };

                query.Parameters["@dataAreaId"] = context.GetChannelConfiguration().InventLocationDataAreaId;
                try
                {
                    entity = databaseContext.ReadEntity<ExtensionsEntity>(query).ToList().FirstOrDefault();
                }
                catch (Exception)
                {
                    entity = new ExtensionsEntity();
                }
                return entity;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public static ExtensionsEntity GetMerchantId(RequestContext context)
        {
            ExtensionsEntity entity;
            using (DatabaseContext databaseContext = new DatabaseContext(context))
            {
                var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                {
                    DatabaseSchema = "ext",
                    Select = new ColumnSet("CDCMERCHANTID"),
                    From = "RETAILSTORETABLE",
                    Where = "STORENUMBER = @storenumber",
                    OrderBy = "CDCMERCHANTID"
                };

                query.Parameters["@storenumber"] = context.GetChannelConfiguration().InventLocation;
                try
                {
                    entity = databaseContext.ReadEntity<ExtensionsEntity>(query).ToList().FirstOrDefault();
                }
                catch (Exception)
                {
                    entity = new ExtensionsEntity();
                }
                return entity;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static string PrepareAuthenticationRequestParameters(ExtensionsEntity entity)
        {
            AuthenticationRequestParameterEntity authenticationRequestParameterEntity = new AuthenticationRequestParameterEntity();
            authenticationRequestParameterEntity.password = entity.GetProperty("CDCEHSASPROGRAMPASSWORD")?.ToString() ?? string.Empty;
            authenticationRequestParameterEntity.username = entity.GetProperty("CDCEHSASPROGRAMUSERNAME")?.ToString() ?? string.Empty;
            authenticationRequestParameterEntity.channel = entity.GetProperty("CDCEHSASPROGRAMCHANNEL")?.ToString() ?? string.Empty;

            return JsonHelper.Serialize(authenticationRequestParameterEntity);
        }
    }
}
