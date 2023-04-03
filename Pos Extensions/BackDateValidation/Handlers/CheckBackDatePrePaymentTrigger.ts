import { IMessageDialogOptions, ShowMessageDialogClientRequest, ShowMessageDialogClientResponse} from "PosApi/Consume/Dialogs";
import { ClientEntities, ProxyEntities } from "PosApi/Entities";
import * as Triggers from "PosApi/Extend/Triggers/PaymentTriggers";
import { DateExtensions, ObjectExtensions } from "PosApi/TypeExtensions";
import { StoreOperations, Entities } from "../DataService/DataServiceRequests.g";
import { GetConnectionStatusClientRequest, GetConnectionStatusClientResponse } from "PosApi/Consume/Device";

export default class CheckBackDatePrePaymentTrigger extends Triggers.PrePaymentTrigger {

    async execute(options: Triggers.IPrePaymentTriggerOptions): Promise<ClientEntities.ICancelable> {
        let localDeviceDateTime: string;

        let statusCheckRequest: GetConnectionStatusClientRequest<GetConnectionStatusClientResponse> =
            new GetConnectionStatusClientRequest<GetConnectionStatusClientResponse>();
        let statusCheckResponse: GetConnectionStatusClientResponse = (await this.context.runtime.executeAsync(statusCheckRequest)).data;

        if (statusCheckResponse.result == ClientEntities.ConnectionStatusType.Online) {
            localDeviceDateTime = DateExtensions.now.toLocaleString("en-US").replace(",", "");
        } else {
            localDeviceDateTime = DateExtensions.now.toLocaleString("en-PK").replace(",", "");

            if (!ObjectExtensions.isNullOrUndefined(options.cart) && !ObjectExtensions.isNullOrUndefined(options.cart.ExtensionProperties) && options.cart.ExtensionProperties.length > 0 && options.cart.ExtensionProperties.filter(a => a.Key == "CSDCardResetDateTime").length > 0) {
                const cardResetDateTime = options.cart.ExtensionProperties.filter(a => a.Key == "CSDCardResetDateTime")[0].Value.StringValue;

                const cardLastTransactionDate = new Date();
                const stringYear = cardResetDateTime.substr(0, 4);
                const stringMonth = cardResetDateTime.substr(5, 2);
                const stringdate = cardResetDateTime.substr(8, 2);
                const stringHour = cardResetDateTime.substr(11, 2);
                const stringMinute = cardResetDateTime.substr(14, 2);
                const stringSecond = cardResetDateTime.substr(17, 2);

                cardLastTransactionDate.setFullYear(Number(stringYear), Number(stringMonth) - 1, Number(stringdate));
                cardLastTransactionDate.setHours(Number(stringHour), Number(stringMinute), Number(stringSecond));
                
                if (DateExtensions.isFutureDate(cardLastTransactionDate)) {
                    this.showMessage("Transaction has been tempered", "Error");
                    return Promise.resolve({ canceled: true, data: null });
                }
            }
        }


        let request: StoreOperations.ValidateTimeRequest<StoreOperations.ValidateTimeResponse> = new StoreOperations.ValidateTimeRequest<StoreOperations.ValidateTimeResponse>(localDeviceDateTime);
        let response: StoreOperations.ValidateTimeResponse = await(await this.context.runtime.executeAsync<StoreOperations.ValidateTimeResponse>(request)).data
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