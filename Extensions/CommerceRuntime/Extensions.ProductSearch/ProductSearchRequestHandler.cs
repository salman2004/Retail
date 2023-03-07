
namespace CDC.Commerce.Runtime.CustomerSearch
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Dynamics.Commerce.Runtime;
    using Microsoft.Dynamics.Commerce.Runtime.Data;
    using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    using Microsoft.Dynamics.Commerce.Runtime.Messages;
    using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    using Microsoft.Dynamics.Retail.Diagnostics;

    public class ProductSearchRequestHandler : IRequestHandlerAsync
    {
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(GetProductSearchResultsDataRequest)
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<Response> Execute(Request request)
        {
            ThrowIf.Null(request, "request");

            EntityDataServiceResponse<ProductSearchResult> response = await this.ExecuteNextAsync<EntityDataServiceResponse<ProductSearchResult>>(request);
            response = await  FilterProductsByInventoryAsync(response, request.RequestContext);

            return response;
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="products"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<EntityDataServiceResponse<ProductSearchResult>> FilterProductsByInventoryAsync(EntityDataServiceResponse<ProductSearchResult> products, RequestContext context)
        {
            GetConfigurationParameters(context, "FilterProductsOnInventory", out string isProductFilterAllowed);
            if (!products.IsNullOrEmpty() && Convert.ToBoolean(string.IsNullOrEmpty(isProductFilterAllowed) ? "false" : isProductFilterAllowed))
            {
                GetInventDimIdsFromItemId(products, out List<ExtensionsEntity> entities, context);
                if (!entities.IsNullOrEmpty())
                {
                    PrepareProductVariantsData(entities, out List<ItemVariantInventoryDimension> itemVariantInventoryDimension);
                    GetProductVariantsDataRequest getProductVariantsDataRequest = new GetProductVariantsDataRequest(itemVariantInventoryDimension)
                    {
                        RequestContext = context
                    };
                    EntityDataServiceResponse<ProductVariant> productVariants=  await context.Runtime.ExecuteAsync<EntityDataServiceResponse<ProductVariant>>(getProductVariantsDataRequest, context);

                    PrepareListProductWarehousr(productVariants, context, out List<ProductWarehouse> productWarehouses);
                    GetEstimatedProductWarehouseAvailabilityServiceRequest getProductDimensionsInventoryAvailabilityDataRequest = new GetEstimatedProductWarehouseAvailabilityServiceRequest(productWarehouses)
                    {
                        RequestContext = context
                    };
                    GetEstimatedProductWarehouseAvailabilityServiceResponse productWarehouseAvailabilityServiceResponse = await context.Runtime.ExecuteAsync<GetEstimatedProductWarehouseAvailabilityServiceResponse>(getProductDimensionsInventoryAvailabilityDataRequest, context);
                    entities = AddProductInventoryToEntities(productWarehouseAvailabilityServiceResponse, entities);

                    List<ProductSearchResult> fileredProducts = new List<ProductSearchResult>();
                    foreach (var item in products)
                    {
                        if (entities.Where(a => a.GetProperty("ITEMID").ToString() == item.ItemId).Any(a => Convert.ToInt32(a.GetProperty("PhysicalAvailable")) != 0))
                        {
                            fileredProducts.Add(item);
                        }
                    }
                    return new EntityDataServiceResponse<ProductSearchResult>(fileredProducts.AsPagedResult());
                }

            }
            return products;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="availabilityServiceResponse"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        public List<ExtensionsEntity> AddProductInventoryToEntities(GetEstimatedProductWarehouseAvailabilityServiceResponse availabilityServiceResponse, List<ExtensionsEntity> entities)
        {
            foreach (var item in availabilityServiceResponse.ProductWarehouseInventoryInformation.ProductWarehouseInventoryAvailabilities)
            {
                entities.Where(en => en.GetProperty("DISTINCTPRODUCTVARIANT").ToString() == item.ProductId.ToString())?.FirstOrDefault()?.SetProperty("PhysicalAvailable", item.PhysicalAvailable);
            }
            return entities;
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
        /// <param name="productVariants"></param>
        /// <param name="context"></param>
        /// <param name="productWarehouses"></param>
        public void PrepareListProductWarehousr(EntityDataServiceResponse<ProductVariant> productVariants, RequestContext context, out List<ProductWarehouse> productWarehouses)
        {
            productWarehouses = new List<ProductWarehouse>();
            foreach (var item in productVariants)
            {
                productWarehouses.Add(new ProductWarehouse(item.DistinctProductVariantId, context.GetChannelConfiguration().InventLocation, context.GetChannelConfiguration().InventLocationDataAreaId));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void PrepareProductVariantsData(List<ExtensionsEntity> entities, out List<ItemVariantInventoryDimension> itemVariantInventoryDimension)
        {
            itemVariantInventoryDimension = new List<ItemVariantInventoryDimension>();
            foreach (var item in entities)
            {
                itemVariantInventoryDimension.Add(new ItemVariantInventoryDimension(item.GetProperty("ITEMID")?.ToString() ?? string.Empty, item.GetProperty("INVENTDIMID")?.ToString() ?? string.Empty));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="products"></param>
        /// <param name="entities"></param>
        /// <param name="context"></param>
        public void GetInventDimIdsFromItemId(EntityDataServiceResponse<ProductSearchResult> products, out List<ExtensionsEntity> entities, RequestContext context)
        {
            if (products == null || products.IsNullOrEmpty())
            {
                entities = new List<ExtensionsEntity>();
                return;
            }

            using (DatabaseContext databaseContext = new DatabaseContext(context))
            {
                SqlQuery query = new SqlQuery();
                query.QueryString = $@"Select DISTINCT I1.INVENTDIMID,I1.ITEMID,I1.DISTINCTPRODUCTVARIANT from ax.ECORESPRODUCT E1 INNER JOIN ax.INVENTDIMCOMBINATION I1 on I1.ITEMID IN ({string.Join(",", products.Select(sl => "'" + sl.ItemId+ "'"))}) where I1.DATAAREAID = @dataAreaId";
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

        
    }
}
