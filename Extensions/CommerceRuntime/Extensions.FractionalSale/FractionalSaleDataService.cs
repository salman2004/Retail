namespace CDC.Commerce.Runtime.FractionalSale
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CDC.Commerce.Runtime.FractionalSale.Entities;
    using Microsoft.Dynamics.Commerce.Runtime;
    using Microsoft.Dynamics.Commerce.Runtime.Data;
    using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    using Microsoft.Dynamics.Commerce.Runtime.Messages;
    using System.Linq;
    using Newtonsoft.Json;
    using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;

    public class FractionalSaleDataService : IRequestHandlerAsync
    {
        /// <summary>
        /// Gets the collection of supported request types by this handler.
        /// </summary>
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(FractionSaleRequest)
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {
            if(request is FractionSaleRequest)
            {
                return await ValidateFractionalSale((FractionSaleRequest)request);
            }
            else
            {
                throw new CommerceException("Retail Server","Request type not supported.");
            }
            
        }

        private async Task<FractionSaleResponse> ValidateFractionalSale(FractionSaleRequest request)
        {
            var response = new FractionSaleResponse(true, string.Empty);
            try
            {
                ChannelConfiguration channelConfigs = request.RequestContext.GetChannelConfiguration();
                if (request != null)
                {
                    var UnitNotAllowedForFractionalSale = GetRetailConfigurationParameter(request, "UnitNotAllowedForFractionalSale", channelConfigs.InventLocationDataAreaId);
                    if (string.IsNullOrWhiteSpace(UnitNotAllowedForFractionalSale))
                    {
                        return new FractionSaleResponse(true, "");
                    }
                    else
                    {
                        var factionalSalesUnits = UnitNotAllowedForFractionalSale.ToLower().Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        
                        foreach (var product in request.ProductsInformation)
                        {
                            if (factionalSalesUnits.Any(x=>x == product.UnitOfMeasure?.ToLower()))
                            {
                                var isProductExists = await IsFractionalSaleProduct(request, product.ProductId, channelConfigs.InventLocation);
                                if (!isProductExists)
                                {
                                    response = new FractionSaleResponse(false, "");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                throw new CommerceException("Retail Server", exception.Message);
            }

            return response;
        }

        private string GetRetailConfigurationParameter(Request request, string name, string company)
        {
            var configurationRequest = new GetConfigurationParametersDataRequest(request.RequestContext.GetChannelConfiguration().RecordId);
            var configurationResponse = request.RequestContext.ExecuteAsync<EntityDataServiceResponse<RetailConfigurationParameter>>(configurationRequest).Result;

            string result = configurationResponse?.PagedEntityCollection?.Where(cp => string.Equals(cp.Name.ToUpper().Trim(), (name).ToUpper().Trim(), StringComparison.OrdinalIgnoreCase))?.FirstOrDefault()?.Value ?? string.Empty;
            return result;
        }

        private async Task<bool> IsFractionalSaleProduct(Request request, long productRecId, string storeNumber)
        {
            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                {
                    DatabaseSchema = "EXT",
                    Select = new ColumnSet("RECID"),
                    From = "CDCPRODUCTFRACTIONALSALE",
                    Where = "VARIANT = @productRecId AND STORENUMBER = @storeNumber"
                };
                query.Parameters["@productRecId"] = productRecId;
                query.Parameters["@storeNumber"] = storeNumber;

                var cdcProductFractionalSale = await databaseContext.ReadEntityAsync<CDCProductFractionalSale>(query).ConfigureAwait(false);
                if (cdcProductFractionalSale.FirstOrDefault() != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
