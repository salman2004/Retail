
namespace CDC
{
    namespace Commerce.Runtime.AggregateSalesLines
    {
        using System.Collections.ObjectModel;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;

        public sealed class AggregateSalesLinesCollectionRequestHandler : SingleRequestHandler<AggregateSalesLinesCollectionRequest, AggregateSalesLinesCollectionResponse>
        {
            protected override AggregateSalesLinesCollectionResponse Process(AggregateSalesLinesCollectionRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.SalesLines, "request.SalesLines");
                Collection<SalesLine> salesLines = request.SalesLines;

                if (request.RequestContext.GetDeviceConfiguration().AggregateItemsForPrinting)
                {
                    SalesLineAggregationHelper salesLineAggregationHelper = new SalesLineAggregationHelper();
                    salesLines = salesLineAggregationHelper.AggregateSalesLines(request.SalesLines, request.RequestContext);
                }

                return new AggregateSalesLinesCollectionResponse(salesLines);
            }
        }
    }
}
