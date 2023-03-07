import { IAlphanumericNumPadOptions, INumericNumPadOptions } from "PosApi/Consume/Controls";
import { IAlphanumericInputDialogResult, IMessageDialogOptions, INumericInputDialogOptions, INumericInputDialogResult, ShowMessageDialogClientRequest, ShowMessageDialogClientResponse, ShowNumericInputDialogClientRequest, ShowNumericInputDialogClientResponse} from "PosApi/Consume/Dialogs";
import { INumPadInputBroker } from "PosApi/Consume/Peripherals";
import { ClientEntities, ProxyEntities } from "PosApi/Entities";
import * as Triggers from "PosApi/Extend/Triggers/OperationTriggers";
import { ObjectExtensions, StringExtensions } from "PosApi/TypeExtensions";
import { StoreOperations, Entities } from "../DataService/DataServiceRequests.g";
import { Global } from "../Global";
import * as CartOperations from "PosApi/Consume/Cart";
import { IExtensionContext } from "PosApi/Framework/ExtensionContext";

export default class AskariCardSalePreOperationTrigger extends Triggers.PreOperationTrigger {
    /**
     * Executes the trigger functionality.
     * @param {Triggers.IOperationTriggerOptions} options The options provided to the trigger.
     */
    public async execute(options: Triggers.IOperationTriggerOptions): Promise<ClientEntities.ICancelable> {
        let operationId: ProxyEntities.RetailOperation = options.operationRequest.operationId;
        
        if (StringExtensions.isNullOrWhitespace(Global.AskariCardOperationType) || StringExtensions.isNullOrWhitespace(Global.AskariCardTenderMethod)) {
            return Promise.resolve({ canceled: false });
        }

        if (Number(operationId) == Number(Global.AskariCardOperationType))
        {
            if (!ObjectExtensions.isNullOrUndefined(options.operationRequest["options"]["tenderType"])) {
                const re = options.operationRequest["options"]["tenderType"];

                if (Number(re.TenderTypeId) == Number(Global.AskariCardTenderMethod)) {
                    let numPadOptions: INumericInputDialogOptions = {
                        title: "Pay using askari card ",
                        subTitle: "For extra discount",
                        numPadLabel: "Please enter 16-digit card number:",
                        defaultNumber: ""                        
                    };

                    let dialogRequest: ShowNumericInputDialogClientRequest<ShowNumericInputDialogClientResponse> = new ShowNumericInputDialogClientRequest<ShowNumericInputDialogClientResponse>(numPadOptions);
                    let result: ClientEntities.ICancelableDataResult<ShowNumericInputDialogClientResponse> = await this.context.runtime.executeAsync(dialogRequest)
                    if (!result.canceled) {
                        if (result.data.result.value.length != 16)
                        {
                            this.showMessage("Card number should be 16 digits", "Askari card");
                            return Promise.resolve({ canceled: true });
                        }

                        let cartClientResponse: CartOperations.GetCurrentCartClientResponse = (await this.getCurrentCart());

                        const reasonCodeLine = new Commerce.Proxy.Entities.ReasonCodeLineClass();
                        reasonCodeLine.ReasonCodeId = Global.AskariCardInfoCode;
                        reasonCodeLine.Amount = 0;
                        reasonCodeLine.Information = result.data.result.value;
                        reasonCodeLine.TransactionId = cartClientResponse.result.Id;
                        reasonCodeLine.InputTypeValue = Commerce.Proxy.Entities.ReasonCodeInputType.Text;

                        if (cartClientResponse.result.ReasonCodeLines.filter(rl => rl.ReasonCodeId == Global.AskariCardInfoCode).length > 0) {

                            const reasonCodeLines = cartClientResponse.result.ReasonCodeLines.filter(rl => rl.ReasonCodeId == Global.AskariCardInfoCode);
                            reasonCodeLines.forEach(rs => rs.Information = result.data.result.value);
                            let saveReasonCodeLine: CartOperations.SaveReasonCodeLinesOnCartClientRequest<CartOperations.SaveReasonCodeLinesOnCartClientResponse> =
                                new CartOperations.SaveReasonCodeLinesOnCartClientRequest<CartOperations.SaveReasonCodeLinesOnCartClientResponse>(reasonCodeLines);
                            let saveReasonCodeLinesOnCartClientResponse: CartOperations.SaveReasonCodeLinesOnCartClientResponse = await (await this.context.runtime.executeAsync(saveReasonCodeLine)).data;

                        }

                        if (cartClientResponse.result.ReasonCodeLines.filter(rl => rl.ReasonCodeId == Global.AskariCardInfoCode).length == 0) {
                            
                            let saveReasonCodeLine: CartOperations.SaveReasonCodeLinesOnCartClientRequest<CartOperations.SaveReasonCodeLinesOnCartClientResponse> =
                                new CartOperations.SaveReasonCodeLinesOnCartClientRequest<CartOperations.SaveReasonCodeLinesOnCartClientResponse>([reasonCodeLine]);
                            let saveReasonCodeLinesOnCartClientResponse: CartOperations.SaveReasonCodeLinesOnCartClientResponse = await (await this.context.runtime.executeAsync(saveReasonCodeLine)).data;
                            
                        }

                       
                        
                        let validateBinNumberRequest: StoreOperations.ValidateBinNumberRequest<StoreOperations.ValidateBinNumberResponse> =
                            new StoreOperations.ValidateBinNumberRequest<StoreOperations.ValidateBinNumberResponse>(result.data.result.value, cartClientResponse.result.Id);

                        let refreshRequest: CartOperations.RefreshCartClientRequest<CartOperations.RefreshCartClientResponse> = new CartOperations.RefreshCartClientRequest<CartOperations.RefreshCartClientResponse>();
                        await this.context.runtime.executeAsync(refreshRequest);
                    
                        let validateBinNumberResponse: StoreOperations.ValidateBinNumberResponse = await (await this.context.runtime.executeAsync(validateBinNumberRequest)).data;
                        if (validateBinNumberResponse.result) {
                            return Promise.resolve({ canceled: false });
                        }
                        else {
                            this.showMessage("There was an error validating card number", "Error")
                            return Promise.resolve({ canceled: true });
                        }
                    }
                    else {
                        return Promise.resolve({ canceled: true });
                    }
                }
                
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

    private async getCurrentCart(): Promise<CartOperations.GetCurrentCartClientResponse>
    {
        let cartClientRequest: CartOperations.GetCurrentCartClientRequest<CartOperations.GetCurrentCartClientResponse> = new CartOperations.GetCurrentCartClientRequest<CartOperations.GetCurrentCartClientResponse>();
        return await (await this.context.runtime.executeAsync<CartOperations.GetCurrentCartClientResponse>(cartClientRequest)).data;
    }
}