using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CDC.Commerce.Runtime.CustomOpCat.Entities;
using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
using Newtonsoft.Json;

namespace CDC.Commerce.Runtime.CustomOpCat.RequestHandlers
{
    public class OpCatService : IRequestHandlerAsync
    {
        public enum ProductOrderSequence
        {
            Regular,
            Ascending,
            Descending
        }
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                      typeof(GetProductDimensionValuesDataRequest),
                      typeof(GetVariantProductsServiceRequest),
                      typeof(GetProductBarcodeDataRequest)
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {
            if (request.GetType() == typeof(GetProductBarcodeDataRequest))
            {
                GetProductBarcodeDataRequest barcodeDataRequest = (GetProductBarcodeDataRequest)request;
                
                RequestContext requestContext = request.RequestContext;
                ChannelConfiguration channelConfiguration = requestContext.GetChannelConfiguration();
                channelConfiguration.SetProperty("Barcode", barcodeDataRequest.Barcode);

                GetProductBarcodeDataRequest productBarcodeDataRequest = new GetProductBarcodeDataRequest(barcodeDataRequest.Barcode)
                {
                    RequestContext = requestContext
                };

                GetProductBarcodeDataResponse response = await this.ExecuteNextAsync<GetProductBarcodeDataResponse>(productBarcodeDataRequest);
                
                return response;
            }

            if (request.GetType() == typeof(GetVariantProductsServiceRequest))
            {
                GetVariantProductsServiceRequest getVariantProductsServiceRequest = (GetVariantProductsServiceRequest)request;
                foreach (var item in getVariantProductsServiceRequest.MatchingDimensionValues)
                {
                    if (item.DimensionValue.Value.IndexOf(" (RS.") >= 0)
                    {
                        item.DimensionValue.Value = item.DimensionValue.Value.Substring(0, item.DimensionValue.Value.IndexOf(" (RS."));
                    }
                }
                return await ExecuteBaseRequestAsync(getVariantProductsServiceRequest);
            }

            List<ProductDimension> productDimensions = new List<ProductDimension>();
            CommerceProperty priceProperty = new CommerceProperty();
            int count = 0;
            int productOrderSequence = 0;
            ProductDimension productDimension = new ProductDimension();
            Dictionary<decimal, ProductDimensionValue> DimensionPriceValues = new Dictionary<decimal, ProductDimensionValue>();
            GetVariantProductsServiceRequest valuesDataRequest;
            GetProductDimensionValuesDataRequest getProductDimensionValues = (GetProductDimensionValuesDataRequest)request;
            EntityDataServiceResponse<ProductDimensionValue> dimensionValuesResponse = (EntityDataServiceResponse<ProductDimensionValue>)await ExecuteBaseRequestAsync(request);

            if (getProductDimensionValues.RequestContext.GetChannelConfiguration().IsPropertyDefined("Barcode") && !dimensionValuesResponse.IsNullOrEmpty() && dimensionValuesResponse.First().DimensionType != ProductDimensionType.Configuration)
            {
                List<ProductDimensionValue> productDimensionValueList = new List<ProductDimensionValue>();
                //Just to make it optimized (Run only once when selecting size)
                try
                {
                    if (!string.IsNullOrEmpty(getProductDimensionValues.RequestContext.GetChannelConfiguration().GetProperty("Barcode").ToString()))
                    {
                        var result = GetProductVariatnsUsingBarcodeAsync(getProductDimensionValues.MasterProductId.ToString(), getProductDimensionValues.RequestContext.GetChannelConfiguration().GetProperty("Barcode").ToString(), request);
                        if (result.Count == 0)
                        {
                            return dimensionValuesResponse;
                        }
                        if (dimensionValuesResponse.First().DimensionType == ProductDimensionType.Size)
                        {
                            foreach (var item in result)
                            {
                                if (productDimensionValueList.Where(a => a.DimensionId == item.GetProperty("INVENTSIZEID").ToString()).Count() == 0)
                                {
                                    productDimensionValueList.Add(dimensionValuesResponse.Where(a => a.DimensionId == item.GetProperty("INVENTSIZEID").ToString()).First());
                                }
                            }
                        }
                        else if (dimensionValuesResponse.First().DimensionType == ProductDimensionType.Color)
                        {
                            foreach (var item in result)
                            {
                                if (productDimensionValueList.Where(a => a.DimensionId == item.GetProperty("INVENTCOLORID").ToString()).Count() == 0)
                                {
                                    productDimensionValueList.Add(dimensionValuesResponse.Where(a => a.DimensionId == item.GetProperty("INVENTCOLORID").ToString()).First());
                                }
                            }
                        }
                        else if (dimensionValuesResponse.First().DimensionType == ProductDimensionType.Style)
                        {
                            foreach (var item in result)
                            {
                                if (productDimensionValueList.Where(a => a.DimensionId == item.GetProperty("INVENTSTYLEID").ToString()).Count() == 0)
                                {
                                    productDimensionValueList.Add(dimensionValuesResponse.Where(a => a.DimensionId == item.GetProperty("INVENTSTYLEID").ToString()).First());
                                }
                            }
                        }
                        //getProductDimensionValues.RequestContext.GetChannelConfiguration().SetProperty("Barcode", string.Empty);
                        return new EntityDataServiceResponse<ProductDimensionValue>(productDimensionValueList.AsPagedResult());
                    }
                }
                catch (Exception)
                {
                    return dimensionValuesResponse;
                }
            }

            if (!dimensionValuesResponse.IsNullOrEmpty() && dimensionValuesResponse.First().DimensionType == ProductDimensionType.Configuration)
            {   
                getProductDimensionValues.RequestContext.GetChannelConfiguration().SetProperty("Barcode", string.Empty);
                foreach (var item in dimensionValuesResponse)
                {
                    if (item.Value.IndexOf(" (RS.") >= 0)
                    {
                        item.Value = item.Value.Substring(0, item.Value.IndexOf(" (RS."));
                    }
                    productDimension.DimensionValue = item;
                    productDimension.DimensionType = item.DimensionType;
                    productDimension.EntityName = item.EntityName;
                    productDimension.DisplayOrder = 0.00000M;
                    productDimension.ProductId = item.ProductId;

                    productDimensions.Add(productDimension);
                    productDimensions.AddRange(getProductDimensionValues.MatchingDimensionValues);

                    List<ProductDimensionCombination> productDimensionCombinations = new List<ProductDimensionCombination>();
                    ProductDimensionCombination productDimensionCombination = new ProductDimensionCombination(productDimensions);
                    productDimensionCombinations.Add(productDimensionCombination);

                    GetInventoryAvailabilityByDimensionsServiceRequest availabilityByDimensionsServiceRequest = new GetInventoryAvailabilityByDimensionsServiceRequest(request.RequestContext.GetChannelConfiguration().InventLocation, getProductDimensionValues.MasterProductId, productDimensionCombinations);
                    availabilityByDimensionsServiceRequest.RequestContext = request.RequestContext;
                    availabilityByDimensionsServiceRequest.QueryResultSettings = QueryResultSettings.AllRecords;
                    EntityDataServiceResponse<ItemAvailability> itemAvailability = (EntityDataServiceResponse<ItemAvailability>)await ExecuteBaseRequestAsync(availabilityByDimensionsServiceRequest);


                    valuesDataRequest = new GetVariantProductsServiceRequest(request.RequestContext.GetPrincipal().ChannelId, getProductDimensionValues.MasterProductId, productDimensions, QueryResultSettings.AllRecords);
                    valuesDataRequest.RequestContext = getProductDimensionValues.RequestContext;
                    GetProductsServiceResponse result = (GetProductsServiceResponse)await ExecuteBaseRequestAsync(valuesDataRequest);
                    productDimensions.Clear();
                    productDimensionCombinations.Clear();
                    productDimensionCombination = new ProductDimensionCombination();
                    productDimension = new ProductDimension();

                    if (result.Products.Count() > 0)
                    {
                        productOrderSequence = await this.GetProductOrderSequence(request, result.Products.First().ItemId);
                        if (productOrderSequence != (int)ProductOrderSequence.Regular)
                        {
                            dimensionValuesResponse = FilterPromoProducts(request.RequestContext, dimensionValuesResponse);
                            item.Value = item.Value + " (RS." + String.Format("{0:0.00}", result.Products.First().Price) + ")";
                            item.DisplayOrder = result.Products.First().Price;

                            if (itemAvailability.First().AvailableQuantity <= 0)
                            {
                                List<ProductDimensionValue> values = dimensionValuesResponse.ToList();
                                if (values.Count > 0)
                                {
                                    values.RemoveAt(count);
                                }
                                dimensionValuesResponse = new EntityDataServiceResponse<ProductDimensionValue>(values.AsPagedResult());
                                continue;
                            }
                        }
                    }

                    count = count + 1;
                }

                if (productOrderSequence == (int)ProductOrderSequence.Ascending)
                {
                    return new EntityDataServiceResponse<ProductDimensionValue>(dimensionValuesResponse.OrderBy(a => a.DisplayOrder).AsPagedResult());
                }
                if (productOrderSequence == (int)ProductOrderSequence.Descending)
                {
                    return new EntityDataServiceResponse<ProductDimensionValue>(dimensionValuesResponse.OrderByDescending(a => a.DisplayOrder).AsPagedResult());
                }
            }

            return dimensionValuesResponse;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<int> GetProductOrderSequence(Request request, string name)
        {
            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                {
                    DatabaseSchema = "ext",
                    Select = new ColumnSet("CDCPRODUCTORDERSEQUENCE"),
                    From = "INVENTTABLE",
                    Where = "ITEMID = @itemId",
                    OrderBy = "CDCMARGINCAPPROTECTION"
                };
                query.Parameters["@itemId"] = name;

                var cdcProductOrderSequence = await databaseContext.ReadEntityAsync<InventTable>(query).ConfigureAwait(false);
                int cdcProductOrderSequenceNumber = Convert.ToInt32(Convert.ToString(cdcProductOrderSequence.FirstOrDefault()?.GetProperty("CDCPRODUCTORDERSEQUENCE")?? decimal.Zero.ToString()));
                return cdcProductOrderSequenceNumber;
            }
        }

        public List<ExtensionsEntity> GetProductVariatnsUsingBarcodeAsync(string productNumber, string barcode, Request request)
        {
            ParameterSet parameters = new ParameterSet();
            parameters["@ProductNumber"] = productNumber;
            parameters["@BarcodeNumber"] = barcode;
            parameters["@DataAreaId"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                var result = databaseContext.ExecuteStoredProcedure<ExtensionsEntity>("[ext].[GETPRODUCTVARIANTSUSINGBARCODE]", parameters, QueryResultSettings.AllRecords);
                return result.ToList();
            }
        }

        public async Task<Response> ExecuteBaseRequestAsync(Request request)
        {
            var requestHandler = request.RequestContext.Runtime.GetNextAsyncRequestHandler(request.GetType(), this);
            Response response = await request.RequestContext.Runtime.ExecuteAsync<Response>(request, request.RequestContext, requestHandler, false).ConfigureAwait(false);
            return response;
        }

        public EntityDataServiceResponse<ProductDimensionValue> FilterPromoProducts(RequestContext context, EntityDataServiceResponse<ProductDimensionValue>  dimensionValuesResponse)
        {
            List<ProductDimensionValue> productDimensions = new List<ProductDimensionValue>();

            using (DatabaseContext databaseContext = new DatabaseContext(context))
            {
                SqlQuery query = new SqlQuery();
                query.QueryString = $@"Select RECID,OPCAT from ext.ECORESCONFIGURATION where RECID IN({string.Join(",", dimensionValuesResponse.Select(sl =>  sl.RecordId ))}) ";

                try
                {
                    List<ExtensionsEntity> entities = databaseContext.ReadEntity<ExtensionsEntity>(query).ToList();
                    foreach (var item in entities.Where(en => en.GetProperty("OPCAT").ToString() == decimal.One.ToString()))
                    {

                        productDimensions.AddRange(dimensionValuesResponse.Where(dv => dv.RecordId.ToString() == item.GetProperty("RECID").ToString()).ToList());
                    }

                    return new EntityDataServiceResponse<ProductDimensionValue>(productDimensions.AsPagedResult());
                }
                catch (Exception)
                {
                    return dimensionValuesResponse;
                }
            }
        }
    }
}