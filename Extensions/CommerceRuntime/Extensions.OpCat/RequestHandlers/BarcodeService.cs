using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDC.Commerce.Runtime.CustomOpCat.RequestHandlers
{
    public class BarcodeService : IRequestHandlerAsync
    {
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(GetProductBarcodeDataRequest)
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {
            GetProductBarcodeDataResponse dataResponse =(GetProductBarcodeDataResponse) await ExecuteBaseRequestAsync(request);  
            request.SetProperty("Barcode",dataResponse.Barcode.ItemBarcodeValue);
            return dataResponse;
        }

        public async Task<Response> ExecuteBaseRequestAsync(Request request)
        {
            var requestHandler = request.RequestContext.Runtime.GetNextAsyncRequestHandler(request.GetType(), this);
            Response response = await request.RequestContext.Runtime.ExecuteAsync<Response>(request, request.RequestContext, requestHandler, false).ConfigureAwait(false);
            return response;
        }
    }
}
