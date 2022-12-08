using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
using Microsoft.Dynamics.Retail.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDC.Commerce.Runtime.InstitutionalCustomer
{
    public class SortingCartLinesServiceRequestHandler : IRequestHandlerAsync
    {
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(CalculateDiscountsServiceRequest)
                };
            }
        }
        public async Task<Response> Execute(Request request)
        {
            ThrowIf.Null(request, "request");
            CalculateDiscountsServiceRequest discountRequest = (CalculateDiscountsServiceRequest)request;

            if (discountRequest.Transaction.LoyaltyCardId == (discountRequest.Transaction.GetProperty("CSDCardNumber")?.ToString()?.Trim() ?? null))
            {
                GetItemGSTDetails(request.RequestContext, discountRequest.Transaction, out List<ExtensionsEntity> entities);
            }
            return await this.ExecuteNextAsync<Response>(request);
        }

        private void GetItemGSTDetails(RequestContext context, SalesTransaction transaction, out List<ExtensionsEntity> entities)
        {
            if (transaction == null || transaction.ActiveSalesLines.IsNullOrEmpty())
            {
                entities = new List<ExtensionsEntity>();
                return;
            }

            using (DatabaseContext databaseContext = new DatabaseContext(context))
            {
                SqlQuery query = new SqlQuery();
                query.QueryString = $@"SELECT DISTINCT IT.ITEMID, IT.CDCGSTTYPE, IT.CDCTOPONCART, DP.CDCPRICINGPRIORITY,C1.COSTPRICE, C1.INVENTCOLORID, C1.CONFIGID, C1.INVENTSTYLEID, C1.INVENTSIZEID, C1.INVENTLOCATIONID FROM [ext].[INVENTTABLE] IT FULL OUTER JOIN[ext].[CDCGSTTYPEDISCOUNTPRIORITY] DP ON IT.CDCGSTTYPE = DP.CDCGSTTYPE FULL OUTER JOIN ext.CDCPRODUCTVARIANTCOSTPRICE C1 on C1.ITEMID = IT.ITEMID WHERE IT.ITEMID IN({string.Join(",", transaction.SalesLines.Select(sl => "'" + sl.ItemId + "'"))}) AND IT.DATAAREAID = @dataAreaId AND C1.INVENTLOCATIONID = @inventLocationId  order by CDCTOPONCART desc, CDCPRICINGPRIORITY desc";
                query.Parameters["@dataAreaId"] = context.GetChannelConfiguration().InventLocationDataAreaId;
                query.Parameters["@inventLocationId"] = context.GetDeviceConfiguration().InventLocationId;

                try
                {
                    entities = databaseContext.ReadEntity<ExtensionsEntity>(query).ToList();
                    CalculateGrossProfit(entities, transaction);
                }
                catch (Exception)
                {
                    entities = new List<ExtensionsEntity>();
                }
            }
        }

        private decimal CalculateGrossMargin(decimal costPrice, decimal sellPrice)
        {
            if (costPrice <= decimal.Zero)
            {
                return decimal.Zero;
            }
            else
            {
                return ((sellPrice - costPrice) / sellPrice) * 100;
            }
        }

        public void CalculateGrossProfit(List<ExtensionsEntity> entities, SalesTransaction Transaction)
        {
            try
            {
                foreach (var item in Transaction.SalesLines)
                {
                    ExtensionsEntity entity = entities.Where(sl => item.ItemId.Equals(Convert.ToString(sl.GetProperty("ITEMID") ?? string.Empty))
                                     && item.Variant.ColorId == ((sl.GetProperty("INVENTCOLORID")?.ToString()?.Trim() ?? string.Empty) != string.Empty ? Convert.ToString(sl.GetProperty("INVENTCOLORID")) : null)
                                     && item.Variant.StyleId == ((sl.GetProperty("INVENTSTYLEID")?.ToString()?.Trim() ?? string.Empty) != string.Empty ? Convert.ToString(sl.GetProperty("INVENTSTYLEID")) : null)
                                     && item.Variant.SizeId == ((sl.GetProperty("INVENTSIZEID")?.ToString()?.Trim() ?? string.Empty) != string.Empty ? Convert.ToString(sl.GetProperty("INVENTSIZEID")) : null)
                                     && item.Variant.ConfigId == ((sl.GetProperty("CONFIGID")?.ToString()?.Trim() ?? string.Empty) != string.Empty ? Convert.ToString(sl.GetProperty("CONFIGID")) : null)
                                     ).FirstOrDefault();


                    item.SetProperty("CDCTOPONCART", entity?.GetProperty("CDCTOPONCART") ?? decimal.Zero);
                    item.SetProperty("CDCPRICINGPRIORITY", entity?.GetProperty("CDCPRICINGPRIORITY") ?? decimal.Zero);
                    item.SetProperty("GrossProfit", CalculateGrossMargin(Convert.ToDecimal(entity?.GetProperty("COSTPRICE") ?? decimal.Zero), item?.Price ?? decimal.Zero));
                }

                Collection<SalesLine> lines = new Collection<SalesLine>(Transaction.SalesLines.OrderByDescending(a => a.GetProperty("CDCTOPONCART") ?? decimal.Zero).ThenByDescending(x => x.GetProperty("CDCPRICINGPRIORITY") ?? decimal.Zero).ThenByDescending(z => z.GetProperty("GrossProfit") ?? decimal.Zero).ToList());
                foreach (var item in lines)
                {
                    Transaction.SalesLines.Where(a => a.LineId == item.LineId).FirstOrDefault().LineNumber = lines.IndexOf(item) + 1;
                }
                Transaction.SalesLines.Where(a => a.ExtensionProperties.Count > 0).ToList().ForEach(b => b.ExtensionProperties = new Collection<CommerceProperty>());
            }
            catch (Exception ex)
            {
                RetailLogger.Log.AxGenericErrorEvent(string.Format("Error when sorting: {0}", ex.Message));
            }
        }
    }
}
