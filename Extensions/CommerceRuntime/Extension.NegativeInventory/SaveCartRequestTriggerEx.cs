namespace Contoso.Commerce.Runtime.Extensions
{
    using Microsoft.Dynamics.Commerce.Runtime;
    using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    using Microsoft.Dynamics.Commerce.Runtime.Messages;
    using Microsoft.Dynamics.Retail.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class SaveCartRequestTriggerEx : IRequestTriggerAsync
    {
        /// <summary>
        /// Gets the supported requests for this trigger.
        /// </summary>
        public IEnumerable<Type> SupportedRequestTypes => new Type[]
        {
            typeof(SaveCartRequest)
        };

        public async Task OnExecuted(Request request, Response response)
        {
            await Task.CompletedTask;
        }

        public async Task OnExecuting(Request request)
        {
            this.GetConfigurationParameters(request.RequestContext, out bool checkInventory);
            if ((request is SaveCartRequest) && checkInventory)
            {
                // Calculate inventory at save
                await ProductAvailabilityHelper.CalculateInventoryAtSave(request as SaveCartRequest).ConfigureAwait(false);
            }

            await Task.CompletedTask;
        }

        private void GetConfigurationParameters(RequestContext context, out bool checkInventory)
        {
            // Get the configuration parameters
            var configurationRequest = new GetConfigurationParametersDataRequest(context.GetChannelConfiguration().RecordId);
            var configurationResponse = context.ExecuteAsync<EntityDataServiceResponse<RetailConfigurationParameter>>(configurationRequest).Result;

            string checkInventoryValidation = configurationResponse?.PagedEntityCollection?.Where(cp => string.Equals(cp.Name.ToUpper().Trim(), ("BlockNegativeInventoryOnPOS").ToUpper().Trim(), StringComparison.OrdinalIgnoreCase))?.FirstOrDefault()?.Value ?? bool.FalseString;
            try
            {
                checkInventory = Convert.ToBoolean(checkInventoryValidation);
            }
            catch (Exception ex)
            {
                RetailLogger.Log.AxGenericErrorEvent($"Reding BlockNegativeInventoryOnPOS config failed. {ex?.Message ?? string.Empty}");
                checkInventory = false;
            }
        }
    }
}
