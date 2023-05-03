namespace CDC.Commerce.RetailProxy.AskariCardBinNumberVerification.Adapters
{
    using CDC.Commerce.Runtime.AskariCardBinNumberVerification.Model;
    using Microsoft.Dynamics.Commerce.RetailProxy.Adapters;
    using System.Threading.Tasks;

    class StoreOperationsManager : IStoreOperationsManager
    {
        public async Task<bool> ValidateBinNumber(string cardNumber, string transactionId)
        {
            var response = await CommerceRuntimeManager.Runtime.ExecuteAsync<AskariCardBinVerificationResponse>(new AskariCardBinVerificationRequest(cardNumber, transactionId), null).ConfigureAwait(false);
            return response.IsDateValidated;
        }
    }
}
