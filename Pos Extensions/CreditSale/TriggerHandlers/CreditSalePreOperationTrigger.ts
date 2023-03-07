import { GetCurrentCartClientRequest, GetCurrentCartClientResponse } from "PosApi/Consume/Cart";
import { GetCustomerClientRequest, GetCustomerClientResponse } from "PosApi/Consume/Customer";
import { GetChannelConfigurationClientRequest, GetChannelConfigurationClientResponse } from "PosApi/Consume/Device";
import { IMessageDialogOptions, ShowMessageDialogClientRequest, ShowMessageDialogClientResponse } from "PosApi/Consume/Dialogs";
import { ClientEntities, ProxyEntities } from "PosApi/Entities";
import * as Triggers from "PosApi/Extend/Triggers/OperationTriggers";
import { ReportDetailsExtensionCommandBase } from "PosApi/Extend/Views/ReportDetailsView";
import { ObjectExtensions } from "PosApi/TypeExtensions";

export default class CreditSalePreOperationTrigger extends Triggers.PreOperationTrigger {
    /**
     * Executes the trigger functionality.
     * @param {Triggers.IOperationTriggerOptions} options The options provided to the trigger.
     */
    public async execute(options: Triggers.IOperationTriggerOptions): Promise<ClientEntities.ICancelable> {
        let operationId: ProxyEntities.RetailOperation = options.operationRequest.operationId;

        if (operationId === ProxyEntities.RetailOperation.PayCustomerAccount)
        {
            let cartRequest: GetCurrentCartClientRequest<GetCurrentCartClientResponse> = new GetCurrentCartClientRequest<GetCurrentCartClientResponse>();
            let cartResponse: GetCurrentCartClientResponse = await (await this.context.runtime.executeAsync(cartRequest)).data;

            let customerRequest: GetCustomerClientRequest<GetCustomerClientResponse> = new GetCustomerClientRequest<GetCustomerClientResponse>(cartResponse.result.CustomerId);
            let customerResponse: GetCustomerClientResponse = await (await this.context.runtime.executeAsync(customerRequest)).data;

            try {
                if (customerResponse.result.CustomerGroup.toUpperCase() == "INSTITUTE" || (customerResponse.result.CustomerGroup.toUpperCase() == "REBATE" && cartResponse.result.LoyaltyCardId[0].toUpperCase() == 'E')) {
                    return Promise.resolve({ canceled: false })
                } else {
                    this.showMessage('This payment method is only allowed for institutional or employee credit sale', 'Error')
                    return Promise.resolve({ canceled: true })
                }
            } catch (e) {
                this.showMessage('This payment method is only allowed for institutional or employee credit sale', 'Error')
                return Promise.resolve({ canceled: true })
            }
            
        }

        return Promise.resolve({ canceled: false });
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