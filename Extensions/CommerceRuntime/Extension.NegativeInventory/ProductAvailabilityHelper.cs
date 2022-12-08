namespace Contoso.Commerce.Runtime.Extensions
{
    using Microsoft.Dynamics.Commerce.Runtime;
    using Microsoft.Dynamics.Commerce.Runtime.Data;
    using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    using Microsoft.Dynamics.Commerce.Runtime.Framework.Exceptions;
    using Microsoft.Dynamics.Commerce.Runtime.Messages;
    using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    using Microsoft.Dynamics.Commerce.Runtime.Workflow.Orders;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    internal static class ProductAvailabilityHelper
    {
        internal static async Task CalculateInventoryAtSave(SaveCartRequest request)
        {
            // Validate cart check out
            GetSalesOrderDetailsByTransactionIdServiceRequest getSalesOrderRequest = new GetSalesOrderDetailsByTransactionIdServiceRequest(request.Cart.Id, SearchLocation.Local);
            if (request.RequestContext.ExecuteAsync<GetSalesOrderDetailsServiceResponse>(getSalesOrderRequest).Result.SalesOrder != null)
            {
                throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CartAlreadyCheckedOut, request.Cart.Id);
            }

            // Get the scanned cart line
            CartLine line = request.Cart.CartLines.FirstOrDefault();
            if (line == null)
            {
                return;
            }

            // Exclude item type other then item
            ReleasedProductType itemType = GetItemType(request.RequestContext, line.ProductId);
            if (itemType != ReleasedProductType.Item)
            {
                return;
            }

            // Get channel configuration
            ChannelConfiguration config = request.RequestContext.GetChannelConfiguration();
            if (config == null)
            {
                return;
            }

            // Is line gift card item, don't calculate inventory for gift card item
            bool isGiftCardLine = line.IsGiftCardLine || config.GiftCardItemId == line.ItemId;
            if (isGiftCardLine || request.IsGiftCardOperation)
            {
                return;
            }

            // Get the current sales transaction
            SalesTransaction salesTransaction = await LoadCurrentSalesTransaction(request).ConfigureAwait(false);
            if (salesTransaction == null)
            {
                return;
            }

            // Don't validate inventory during recall suspended transaction or customer order/quote cretaion
            if (salesTransaction.ExtensibleSalesTransactionType == ExtensibleSalesTransactionType.SuspendedTransaction
                || salesTransaction.ExtensibleSalesTransactionType == ExtensibleSalesTransactionType.AsyncCustomerQuote
                || ((salesTransaction.ExtensibleSalesTransactionType == ExtensibleSalesTransactionType.CustomerOrder || salesTransaction.ExtensibleSalesTransactionType == ExtensibleSalesTransactionType.AsyncCustomerOrder)
                    && (salesTransaction.CustomerOrderType == CustomerOrderType.SalesOrder || salesTransaction.CustomerOrderType == CustomerOrderType.Quote)
                    && (salesTransaction.CustomerOrderMode == CustomerOrderMode.CustomerOrderCreateOrEdit || salesTransaction.CustomerOrderMode == CustomerOrderMode.QuoteCreateOrEdit)))
            {
                return;
            }

            // Is new line
            bool isNewSalesLine = string.IsNullOrWhiteSpace(line.LineId) && !isGiftCardLine && !line.IsVoided && line.Quantity >= decimal.Zero;

            // Line quantity
            decimal lineQty = line.Quantity;

            // Current cart quantity
            decimal currentCartQty = salesTransaction.ActiveSalesLines.Where(l => l.ProductId == line.ProductId).Sum(l => l.Quantity);
            if (!isNewSalesLine)
            {
                currentCartQty -= salesTransaction.ActiveSalesLines.Where(l => l.LineId == line.LineId).Sum(l => l.Quantity);
            }

            decimal qtyToValidate = lineQty + currentCartQty;

            ItemAvailability itemAvailability = await GetEstimatedAvailability(request.RequestContext, line.ProductId).ConfigureAwait(false);

            decimal availableQty = Convert.ToDecimal(itemAvailability?.AvailableQuantity);

            // Validate scanned quantity
            if (qtyToValidate > availableQty)
            {
                string messageËrror = string.Format("There is an insufficient quantity of the product available.");
                string messageInventory = string.Format("Available physical quantity for item {0} is {1}.", line.ItemId, Math.Round(availableQty, 2));
                Exception exception = new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InsufficientQuantityAvailable, request.Cart.Id, messageInventory);

                throw new CommerceException("Microsoft_Dynamics_Commerce_30104", ExceptionSeverity.Warning, exception, messageËrror)
                {
                    LocalizedMessage = string.Format("{0} {1}", messageËrror, messageInventory),
                    LocalizedMessageParameters = new object[] { }
                };
            }
        }

        internal static async Task<ItemAvailability> GetEstimatedAvailability(RequestContext context, long productId)
        {
            List<ProductWarehouse> productWarehouses = new List<ProductWarehouse>()
            {
                new ProductWarehouse(productId, context.GetChannel().InventoryLocationId, context.GetChannelConfiguration().InventLocationDataAreaId)
            };
            GetEstimatedProductWarehouseAvailabilityServiceRequest availabilityServiceRequest = new GetEstimatedProductWarehouseAvailabilityServiceRequest(productWarehouses) { RequestContext = context };
            GetEstimatedProductWarehouseAvailabilityServiceResponse availabilityServiceResponse = await context.ExecuteAsync<GetEstimatedProductWarehouseAvailabilityServiceResponse>(availabilityServiceRequest).ConfigureAwait(false);

            ProductWarehouseInventoryAvailability warehouseAvailability = availabilityServiceResponse.ProductWarehouseInventoryInformation.ProductWarehouseInventoryAvailabilities.Where(wa => wa.InventLocationId == context.GetChannel().InventoryLocationId && wa.ProductId == productId).FirstOrDefault();

            ItemAvailability itemAvailability = null;
            if (itemAvailability == null)
            {
                itemAvailability = new ItemAvailability()
                {
                    ProductId = productId,
                    AvailableQuantity = decimal.Zero,
                    PhysicalReserved = decimal.Zero,
                    OrderedSum = decimal.Zero
                };
            }

            if (warehouseAvailability != null)
            {
                itemAvailability.AvailableQuantity = warehouseAvailability.PhysicalAvailable;
                itemAvailability.PhysicalReserved = warehouseAvailability.PhysicalReserved;
            }

            return itemAvailability;
        }

        /// <summary>
        /// Load the current sales transaction for this terminal
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static async Task<SalesTransaction> LoadCurrentSalesTransaction(SaveCartRequest request)
        {
            SalesTransaction salesTransaction = request.SalesTransaction;
            if (salesTransaction == null && !string.IsNullOrEmpty(request.Cart.Id))
            {
                GetCartRequest getCartRequest = new GetCartRequest(new CartSearchCriteria(request.Cart.Id, request.Cart.Version), QueryResultSettings.SingleRecord) { RequestContext = request.RequestContext };
                GetCartResponse getCartResponse = await request.RequestContext.ExecuteAsync<GetCartResponse>(getCartRequest).ConfigureAwait(false);
                salesTransaction = getCartResponse.Transactions.FirstOrDefault();
            }

            return salesTransaction;
        }

        public static ReleasedProductType GetItemType(RequestContext context, long productId)
        {
            if (productId <= decimal.Zero)
            {
                return ReleasedProductType.None;
            }

            ExtensionsEntity itemDetail = null;

            SqlQuery query = new SqlQuery();
            query.QueryString = @"SELECT TOP 1 PRODUCTTYPE FROM AX.ECORESPRODUCT WHERE RECID = @PRODUCT";
            query.Parameters["@PRODUCT"] = productId;

            try
            {
                using (DatabaseContext databaseContext = new DatabaseContext(context))
                {
                    itemDetail = databaseContext.ReadEntity<ExtensionsEntity>(query).FirstOrDefault();
                }
            }
            catch (Exception)
            {
                itemDetail = null;
            }

            int productType = Convert.ToInt32(itemDetail?.GetProperty("PRODUCTTYPE"));

            return (ReleasedProductType)productType;
        }
    }
}
