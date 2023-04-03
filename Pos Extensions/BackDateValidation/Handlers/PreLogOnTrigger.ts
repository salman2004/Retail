import { GetConnectionStatusClientRequest, GetConnectionStatusClientResponse } from "PosApi/Consume/Device";
import { IMessageDialogOptions, ShowMessageDialogClientRequest, ShowMessageDialogClientResponse } from "PosApi/Consume/Dialogs";
import { ClientEntities, ProxyEntities } from "PosApi/Entities";
import * as Triggers from "PosApi/Extend/Triggers/ApplicationTriggers";
import { DateExtensions, StringExtensions } from "PosApi/TypeExtensions";
import { StoreOperations, Entities } from "../DataService/DataServiceRequests.g";

export default class PreLogOnTrigger extends Triggers.PreLogOnTrigger {
    /**
     * Executes the trigger functionality.
     * @param {Triggers.IPreLogOnTriggerOptions} options The options provided to the trigger.
     */
    public async execute(options: Triggers.IPreLogOnTriggerOptions): Promise<ClientEntities.ICancelable> {
        let localDeviceDateTime: string;

        let statusCheckRequest: GetConnectionStatusClientRequest<GetConnectionStatusClientResponse> =
            new GetConnectionStatusClientRequest<GetConnectionStatusClientResponse>();
        let statusCheckResponse: GetConnectionStatusClientResponse = (await this.context.runtime.executeAsync(statusCheckRequest)).data;

        if (statusCheckResponse.result == ClientEntities.ConnectionStatusType.Online) {
            localDeviceDateTime = DateExtensions.now.toLocaleString("en-US").replace(",", "");
        } else {
            localDeviceDateTime = DateExtensions.now.toLocaleString("en-PK").replace(",", "");
        }

        
        let request: StoreOperations.ValidateTimeRequest<StoreOperations.ValidateTimeResponse> = new StoreOperations.ValidateTimeRequest<StoreOperations.ValidateTimeResponse>(localDeviceDateTime);
        let response: StoreOperations.ValidateTimeResponse = await (await this.context.runtime.executeAsync<StoreOperations.ValidateTimeResponse>(request)).data
        if (response.result == true) {
            return Promise.resolve({ canceled: false });
        }
        else {
            this.showMessage("System datetime is not correct", "DateTime Validation");
            return Promise.resolve({ canceled: true });
        }
        
    }

    private showMessage(message: string, title: string): void {
        let dialogRequest: ShowMessageDialogClientRequest<ShowMessageDialogClientResponse> =
            new ShowMessageDialogClientRequest<ShowMessageDialogClientResponse>(<IMessageDialogOptions>{
                title: title,
                message: message
            });
        this.context.runtime.executeAsync(dialogRequest);
    }
}
