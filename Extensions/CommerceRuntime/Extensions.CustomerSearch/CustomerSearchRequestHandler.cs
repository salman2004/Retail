
namespace CDC.Commerce.Runtime.CustomerSearch
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Dynamics.Commerce.Runtime;
    using Microsoft.Dynamics.Commerce.Runtime.Data;
    using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    using Microsoft.Dynamics.Commerce.Runtime.Messages;
    using Microsoft.Dynamics.Retail.Diagnostics;

    public class CustomerSearchRequestHandler : IRequestHandlerAsync
    {
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(CustomersSearchRequest)
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {
            ThrowIf.Null(request, "request");

            Type requestType = request.GetType();
            if (requestType == typeof(CustomersSearchRequest))
            {
                return await GetCustomerSearchResultAsync((CustomersSearchRequest)request);
            }
            else
            {
                throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
            }
        }

        public async Task<Response> GetCustomerSearchResultAsync(CustomersSearchRequest request)
        {
            CustomersSearchResponse response = await this.ExecuteNextAsync<CustomersSearchResponse>(request);
            GetConfigurationParameters(request.RequestContext, "BlockSearchLoyalyCustomerOnPos", out string result);
            if (Convert.ToBoolean(string.IsNullOrEmpty(result) ? "false" : result))
            {
                response = FilterCustomerSearchResult(response, request.RequestContext);   
            }
            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configName"></param>
        /// <param name="result"></param>
        public void GetConfigurationParameters(RequestContext context, string configName, out string result)
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
        /// <param name="searchResult"></param>
        /// <returns></returns>
        public CustomersSearchResponse FilterCustomerSearchResult(CustomersSearchResponse searchResult, RequestContext context)
        {
            GetLoyaltyCardCustomers(searchResult, context, out List<ExtensionsEntity> entities);
            if (entities.Count > 0)
            {   
                searchResult = new CustomersSearchResponse(searchResult.Customers.Where(cus => !entities.Any(e => e.GetProperty("PARTYNUMBER").ToString() == cus.PartyNumber)).AsPagedResult());
            }
            return searchResult;
        }

        /// <summary>
        /// Get Product Size to multiply with quatity
        /// </summary>
        /// <param name="request"></param>
        /// <param name="inventDimCombination"></param>
        public void GetLoyaltyCardCustomers(CustomersSearchResponse response, RequestContext context,out List<ExtensionsEntity> entities)
        {
            if (response.Customers == null || response.Customers.Count() == 0)
            {
                entities = new List<ExtensionsEntity>();
                return;
            }

            using (DatabaseContext databaseContext = new DatabaseContext(context))
            {
                SqlQuery query = new SqlQuery();
                query.QueryString = $@"Select DISTINCT D1.PARTYNUMBER from ax.RETAILLOYALTYCARD R1 join ax.DIRPARTYTABLE D1 on D1.RECID = R1.PARTY WHERE D1.PARTYNUMBER IN({string.Join(",", response.Customers.Select(cus => "'"+cus.PartyNumber+ "'"))})";
               
                try
                {
                    entities = databaseContext.ReadEntity<ExtensionsEntity>(query).ToList();
                }
                catch (Exception ex)
                {
                    RetailLogger.Log.AxGenericErrorEvent(ex.Message);
                    entities = new List<ExtensionsEntity>();
                }
            }
        }
    }
}
