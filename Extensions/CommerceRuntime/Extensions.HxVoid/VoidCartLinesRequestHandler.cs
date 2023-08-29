namespace CDC
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


    public sealed class CreateOrUpdateCustomerDataRequestHandler : SingleAsyncRequestHandler<VoidCartLinesServiceRequest>
    {
        /// <summary>
        /// Executes the workflow to create or update a customer.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The response.</returns>
        protected override async Task<Response> Process(VoidCartLinesServiceRequest request)
        {
            ThrowIf.Null(request, "request");

            
      
            string mainLineToVoidId = string.Empty, relatedLineToVoidId = string.Empty;

            List<CartLine> cartLinesList = new List<CartLine>();

            VoidCartLinesServiceRequest relatedCartLine = (VoidCartLinesServiceRequest)request;

            CartLine cartLine = relatedCartLine.LinesToVoid.FirstOrDefault();

            if(Convert.ToBoolean(cartLine.GetProperty("QLVoided")) == true)
            {

                throw new CommerceException("Microsoft_Dynamics_Commerce", string.Format("Quantity Limit Product Once Voided Cannot be Unvoided, Please Add a New Item"))
                {
                    LocalizedMessage = string.Format("Quantity Limit Product Once Voided Cannot be Unvoided, Please Add a New Item"),
                    LocalizedMessageParameters = new object[] { }
                };
            }

            if(cartLine.RelatedDiscountedLineIds.Any())
            {
                CommerceProperty commerceProperty = new CommerceProperty("QLVoided", true);

                mainLineToVoidId = cartLine.LineId;

                relatedLineToVoidId = cartLine.RelatedDiscountedLineIds[0];

                cartLine.SetProperty("QLVoided",true);
                cartLinesList.Add(cartLine);

                Cart cart = await getCartAsync(request.RequestContext, relatedCartLine.OriginalCart.Id).ConfigureAwait(false);

                CartLine cartLine2 = cart.CartLines.Where(x => x.LineId == relatedLineToVoidId).SingleOrDefault();

                cartLine2.IsVoided = true;

                cartLine2.SetProperty("QLVoided", true);
                cartLinesList.Add(cartLine2);
                
                RequestContext context = request.RequestContext;

                request = new VoidCartLinesServiceRequest(cartLinesList.AsEnumerable(), request.OriginalCart);

                request.RequestContext = context;

            }
            var response = await this.ExecuteNextAsync<VoidCartLinesServiceResponse>(request).ConfigureAwait(false);

            if(mainLineToVoidId != String.Empty && relatedLineToVoidId != String.Empty) { 
                response.UpdatedCart.SalesLines.FirstOrDefault(sl=>sl.LineId == mainLineToVoidId).SetProperty("QLVoided", true);
                response.UpdatedCart.SalesLines.FirstOrDefault(sl => sl.LineId == relatedLineToVoidId).SetProperty("QLVoided", true);
            }
            return response;

        }
        public async Task<Cart> getCartAsync(RequestContext requestContext, string cartId)
        {
            CartSearchCriteria cartSearchCriteria = new CartSearchCriteria(cartId);
            GetCartRequest getCartRequest = new GetCartRequest(cartSearchCriteria, QueryResultSettings.AllRecords);
            GetCartResponse getCartResponse = await requestContext.ExecuteAsync<GetCartResponse>(getCartRequest).ConfigureAwait(false);
            return getCartResponse.Carts.FirstOrDefault();
        }

    }


}

