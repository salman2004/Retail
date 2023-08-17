using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDC.Commerce.Runtime.UnitConversion
{
    public class CartSearchCriteriaTrigger : IRequestTriggerAsync
    {
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(GetCartRequest)
                };
            }
        }

        public async Task OnExecuted(Request request, Response response)
        {
            try
            {
                if (response is GetCartResponse)
                {
                    var cartResponse = (GetCartResponse)response;
                    var salesTransaction = cartResponse.Transactions.SingleOrDefault<SalesTransaction>();
                    if (salesTransaction != null && salesTransaction.SalesLines != null)
                    {
                        await PopulateMasterProductIds(request.RequestContext, salesTransaction.SalesLines);
                    }
                }
            }
            catch (Exception exception)
            {
                throw new CommerceException("Microsoft_Dynamics_Commerce_30104", exception.Message);
            }

        }

        public async Task OnExecuting(Request request)
        {
            await Task.CompletedTask;
        }

        private static async Task PopulateMasterProductIds(RequestContext context, IEnumerable<SalesLine> salesLines)
        {
            IEnumerable<string> strings = salesLines.Where<SalesLine>((Func<SalesLine, bool>)(l => !string.IsNullOrWhiteSpace(l.ItemId) && !string.IsNullOrWhiteSpace(l.InventoryDimensionId))).Select<SalesLine, string>((Func<SalesLine, string>)(l => l.ItemId)).Distinct<string>((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase);
            if (!strings.Any<string>())
                return;
            GetItemsDataRequest itemsDataRequest = new GetItemsDataRequest(strings);
            itemsDataRequest.QueryResultSettings = new QueryResultSettings(new ColumnSet(new string[2]
            {
                "ItemId",
                "PRODUCT"
            }), PagingInfo.AllRecords);
            IDictionary<string, Item> dictionary = (IDictionary<string, Item>)(await context.ExecuteAsync<GetItemsDataResponse>((Request)itemsDataRequest).ConfigureAwait(false)).Items.ToDictionary<Item, string, Item>((Func<Item, string>)(p => p.ItemId), (Func<Item, Item>)(p => p), (IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase);
            foreach (SalesLine salesLine in salesLines)
            {
                Item obj;
                if (salesLine.MasterProductId == 0L && !string.IsNullOrWhiteSpace(salesLine.ItemId) && dictionary.TryGetValue(salesLine.ItemId, out obj) && obj.Product != salesLine.ProductId)
                    salesLine.MasterProductId = obj.Product;
            }
        }
    }
}
