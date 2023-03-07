namespace CDC.Commerce.RetailProxy.BackDateValidation.Adapters
{
    using CDC.Commerce.Runtime.BackDateValidation.Model;
    using Microsoft.Dynamics.Commerce.RetailProxy.Adapters;
    using System.Threading.Tasks;

    class StoreOperationsManager : IStoreOperationsManager
    {
        public async Task<bool> ValidateTime(string deviceDateTime)
        {
            var response = await CommerceRuntimeManager.Runtime.ExecuteAsync<BackDateValidationResponse>(new BackDateValidationRequest(deviceDateTime), null).ConfigureAwait(false);
            return response.IsDateValidated;
        }
    }
}
