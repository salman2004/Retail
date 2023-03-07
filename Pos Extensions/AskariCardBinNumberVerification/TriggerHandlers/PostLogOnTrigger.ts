import { GetChannelConfigurationClientRequest, GetChannelConfigurationClientResponse } from "PosApi/Consume/Device";
import * as Triggers from "PosApi/Extend/Triggers/ApplicationTriggers";
import { StringExtensions } from "PosApi/TypeExtensions";
import { Global } from "../Global"

export default class PostLogOnTrigger extends Triggers.PostLogOnTrigger {

    public async execute(options: Triggers.IPostLogOnTriggerOptions): Promise<void> {
        let configRequest: GetChannelConfigurationClientRequest<GetChannelConfigurationClientResponse> = new GetChannelConfigurationClientRequest<GetChannelConfigurationClientResponse>("");
        const response = await(await this.context.runtime.executeAsync(configRequest)).data;

        if (response.result.ExtensionProperties.filter(ep => ep.Key == "AskariCardOperationType").length > 0) {
            Global.AskariCardOperationType = response.result.ExtensionProperties.filter(ep => ep.Key == "AskariCardOperationType")[0].Value.StringValue;
        }

        if (response.result.ExtensionProperties.filter(ep => ep.Key == "AskariCardTenderMethod" ).length > 0) {
            Global.AskariCardTenderMethod = response.result.ExtensionProperties.filter(ep => ep.Key == "AskariCardTenderMethod")[0].Value.StringValue;
        }

        if (response.result.ExtensionProperties.filter(ep => ep.Key == "AskariCardInfoCode").length > 0) {
            Global.AskariCardInfoCode = response.result.ExtensionProperties.filter(ep => ep.Key == "AskariCardInfoCode")[0].Value.StringValue;
        }
    }
}