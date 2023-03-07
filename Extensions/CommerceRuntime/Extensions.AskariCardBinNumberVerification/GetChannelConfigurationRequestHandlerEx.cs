﻿namespace CDC.Commerce.Runtime.AskariCardBinNumberVerification
{
    using CDC.Commerce.Runtime.AskariCardBinNumberVerification.Model;
    using Microsoft.Dynamics.Commerce.Runtime;
    using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    using Microsoft.Dynamics.Commerce.Runtime.Messages;
    using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class GetChannelConfigurationRequestHandlerEx : IRequestTriggerAsync
    {
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(GetChannelConfigurationDataRequest)
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public async Task OnExecuted(Request request, Response response)
        {
            await Task.CompletedTask;

            GetChannelConfigurationDataRequest dataRequest = (GetChannelConfigurationDataRequest)request;
            SingleEntityDataServiceResponse<ChannelConfiguration> entityDataServiceResponse = (SingleEntityDataServiceResponse<ChannelConfiguration>)response;
            CommerceProperty askariCardOperationTypeProperty = new CommerceProperty("AskariCardOperationType", GetConfigurationParameters(request.RequestContext, dataRequest.ChannelId, "AskariCardOperationType"));
            CommerceProperty askariCardTenderMethodProperty = new CommerceProperty("AskariCardTenderMethod", GetConfigurationParameters(request.RequestContext, dataRequest.ChannelId, "AskariCardTenderMethod"));
            CommerceProperty askariCardInfoCodeProperty = new CommerceProperty("AskariCardInfoCode", GetConfigurationParameters(request.RequestContext, dataRequest.ChannelId, "AskariCardInfoCode"));
            entityDataServiceResponse.Entity.ExtensionProperties.Add(askariCardOperationTypeProperty);
            entityDataServiceResponse.Entity.ExtensionProperties.Add(askariCardTenderMethodProperty);
            entityDataServiceResponse.Entity.ExtensionProperties.Add(askariCardInfoCodeProperty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task OnExecuting(Request request)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string GetConfigurationParameters(RequestContext context, long channelId ,string key)
        {
            var configurationRequest = new GetConfigurationParametersDataRequest(channelId);
            var configurationResponse = context.ExecuteAsync<EntityDataServiceResponse<RetailConfigurationParameter>>(configurationRequest).Result;

            RetailConfigurationParameter paramter = configurationResponse?.PagedEntityCollection?.Where(cp => string.Equals(cp.Name.ToUpper().Trim(), (key).ToUpper().Trim(), StringComparison.OrdinalIgnoreCase))?.FirstOrDefault();

            return paramter?.Value ?? string.Empty;
        }
    }
}
