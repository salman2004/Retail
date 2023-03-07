import { ExtensionOperationRequestType, ExtensionOperationRequestHandlerBase } from "PosApi/Create/Operations";
import RFIDCardReaderOperationResponse from "./RFIDCardReaderOperationResponse";
import RFIDCardReaderOperationRequest from "./RFIDCardReaderOperationRequest";
import { SaveExtensionPropertiesOnCartClientRequest, SaveExtensionPropertiesOnCartClientResponse } from "PosApi/Consume/Cart";
import { ClientEntities, ProxyEntities } from "PosApi/Entities";
import { HardwareStationDeviceActionRequest, HardwareStationDeviceActionResponse } from "PosApi/Consume/Peripherals";
import { AddLoyaltyCardToCartOperationRequest, AddLoyaltyCardToCartOperationResponse } from "PosApi/Consume/Cart";
import { IMessageDialogOptions, INumericInputDialogOptions, INumericInputDialogResult, ShowMessageDialogClientRequest, ShowMessageDialogClientResponse, ShowNumericInputDialogClientRequest, ShowNumericInputDialogClientResponse, ShowNumericInputDialogError } from "PosApi/Consume/Dialogs";
import { StoreOperations } from "../DataService/DataServiceRequests.g";
import { DateExtensions, ObjectExtensions, StringExtensions } from "PosApi/TypeExtensions";
import { DateFormatter } from "PosApi/Consume/Formatters";

export default class RFIDCardReaderOperationRequestHandler<TResponse extends RFIDCardReaderOperationResponse> extends ExtensionOperationRequestHandlerBase<TResponse> {

    /**
     * Gets the supported request type.
     * @return {RequestType<TResponse>} The supported request type.
     */
    public supportedRequestType(): ExtensionOperationRequestType<TResponse> {
        return RFIDCardReaderOperationRequest;
    }
    
    /**
     * Executes the request handler asynchronously.
     * @param {RFIDCardReaderOperationRequest<TResponse>} request The request.
     * @return {Promise<ICancelableDataResult<TResponse>>} The cancelable async result containing the response.
     */
    public async executeAsync(request: RFIDCardReaderOperationRequest<TResponse>): Promise<ClientEntities.ICancelableDataResult<TResponse>> {
        
        let hardwareStationDeviceActionRequest: HardwareStationDeviceActionRequest<HardwareStationDeviceActionResponse> =
            new HardwareStationDeviceActionRequest("RFIDCARDREADEREXTENSIONDEVICE",
                "GetLoyaltyCardInfo", "");
        let response: HardwareStationDeviceActionResponse = await (await this.context.runtime.executeAsync(hardwareStationDeviceActionRequest)).data;
        let stringResponse: string = JSON.stringify(response.response);
        var obj = JSON.parse(stringResponse);
        
        if (obj.csdCardNumber != null || obj.csdCardNumber != "")
        {
            if (obj.isCardBlocked == true) {
                this.showMessage("The card attached to card reader is blocked. Please contact the concerned department.", "Error");
            }

            const stringDate = String(obj.lastTransactionDateTime);
            const cardLastTransactionDate = new Date();
            const stringYear = stringDate.substr(0, 4);
            const stringMonth = stringDate.substr(5, 2);
            const stringdate = stringDate.substr(8, 2);
            const stringHour = stringDate.substr(11, 2);
            const stringMinute = stringDate.substr(14, 2);

            cardLastTransactionDate.setFullYear(Number(stringYear), Number(stringMonth) - 1, Number(stringdate));
            cardLastTransactionDate.setHours(Number(stringHour), Number(stringMinute));
            if (DateExtensions.isFutureDate(cardLastTransactionDate)) {
                this.showMessage("Last Transaction dateTime is greater than system dateTime", "Error");
                return Promise.resolve({ canceled: false, data: null });
            }

            let cartExtensionProperty: ProxyEntities.CommerceProperty = new ProxyEntities.CommercePropertyClass();
            cartExtensionProperty.Key = "CDCCardReaderValue";
            cartExtensionProperty.Value = new ProxyEntities.CommercePropertyValueClass();
            cartExtensionProperty.Value.StringValue = stringResponse;

            let cartExtensionPropertylastTransactionDateTime: ProxyEntities.CommerceProperty = new ProxyEntities.CommercePropertyClass();
            cartExtensionPropertylastTransactionDateTime.Key = "CSDlastTransactionDateTime";
            cartExtensionPropertylastTransactionDateTime.Value = new ProxyEntities.CommercePropertyValueClass();
            cartExtensionPropertylastTransactionDateTime.Value.StringValue = obj.lastTransactionDateTime;

            let cartExtensionPropertyCardLimit: ProxyEntities.CommerceProperty = new ProxyEntities.CommercePropertyClass();
            cartExtensionPropertyCardLimit.Key = "CSDCardLimit";
            cartExtensionPropertyCardLimit.Value = new ProxyEntities.CommercePropertyValueClass();            

            let cartExtensionPropertyCardNumber: ProxyEntities.CommerceProperty = new ProxyEntities.CommercePropertyClass();
            cartExtensionPropertyCardNumber.Key = "CSDCardNumber";
            cartExtensionPropertyCardNumber.Value = new ProxyEntities.CommercePropertyValueClass();
            cartExtensionPropertyCardNumber.Value.StringValue = obj.csdCardNumber;

            let cartExtensionPropertyCardBalance: ProxyEntities.CommerceProperty = new ProxyEntities.CommercePropertyClass();
            cartExtensionPropertyCardBalance.Key = "CSDCardBalance";
            cartExtensionPropertyCardBalance.Value = new ProxyEntities.CommercePropertyValueClass();

            if (/[^a-zA-Z]/.test(obj.csdCardNumber.toString())) {                
                cartExtensionPropertyCardBalance.Value.StringValue = obj.balance;
                cartExtensionPropertyCardLimit.Value.StringValue = obj.limit;
            }            

            if (obj.isCardActivated == true && obj.isCardBlocked == false) {

                let saveExtensionProperty: SaveExtensionPropertiesOnCartClientRequest<SaveExtensionPropertiesOnCartClientResponse>
                    = new SaveExtensionPropertiesOnCartClientRequest<SaveExtensionPropertiesOnCartClientResponse>([cartExtensionProperty, cartExtensionPropertyCardNumber, cartExtensionPropertyCardBalance, cartExtensionPropertylastTransactionDateTime, cartExtensionPropertyCardLimit], hardwareStationDeviceActionRequest.correlationId);
                await this.context.runtime.executeAsync(saveExtensionProperty);

                await (await this.context.runtime.executeAsync(new StoreOperations.AuthenticateCardRequest<StoreOperations.AuthenticateCardResponse>(StringExtensions.EMPTY, obj.csdCardNumber))).data;

                let loyaltyCardRequest: AddLoyaltyCardToCartOperationRequest<AddLoyaltyCardToCartOperationResponse> = new AddLoyaltyCardToCartOperationRequest<AddLoyaltyCardToCartOperationResponse>(this.context.logger.getNewCorrelationId(), obj.csdCardNumber);
                await this.context.runtime.executeAsync(loyaltyCardRequest);
            }
            if (obj.isCardActivated == false && obj.isCardBlocked == false) {
                
                let subTitleMsg: string = "Enter Last 6 digits of card holder CNIC number.\n\n"
                    + "CNIC Number should not include (-).\n\n"
                let numericInputDialogOptions: INumericInputDialogOptions = {
                    title: "Card Activation",
                    subTitle: subTitleMsg,
                    numPadLabel: "Please enter cnic number:",
                    defaultNumber: ""
                };

                let dialogRequest: ShowNumericInputDialogClientRequest<ShowNumericInputDialogClientResponse> =
                    new ShowNumericInputDialogClientRequest<ShowNumericInputDialogClientResponse>(numericInputDialogOptions);
                let result: ClientEntities.ICancelableDataResult<ShowNumericInputDialogClientResponse> = await this.context.runtime.executeAsync(dialogRequest)
                if (!result.canceled) {

                    if (result.data.result.value.length == 0)
                    {
                        return Promise.resolve({ canceled: false, data: null });
                    }
                    let res: StoreOperations.AuthenticateCardResponse = await (await this.context.runtime.executeAsync(new StoreOperations.AuthenticateCardRequest<StoreOperations.AuthenticateCardResponse>(result.data.result.value, obj.csdCardNumber))).data;
                    if (res.result == true) {
                        let csdCardNumber: string = obj.csdCardNumber;
                        let writtenCardNumber: string = obj.writtenCardNumber;

                        var ActivateCardRequest = {
                            cardInfo: stringResponse,
                            csdCardNumber: csdCardNumber,
                            writtenCardNumber: writtenCardNumber
                        };
                        let response: HardwareStationDeviceActionResponse = await this.cardReaderHardwareStationRequest("RFIDCARDREADEREXTENSIONDEVICE", "ActivateCard", ActivateCardRequest);

                        if (response.response == true) {
                            this.showMessage("Card has been activated.", "Card status changed");
                        }
                        else {
                            this.showMessage("There was an error writing onto card.", "Error");
                        }
                    }
                    else {
                        this.showMessage("There was an error writing to card. Please contact administrator.", "Error");
                    }
                } else {
                    this.context.logger.logInformational("Card activation is canceled.");
                }
            }
        }
        else
        {
            this.showMessage("Card contains invalid information. Please contact concerened department.", "Error");
        }

        return Promise.resolve({
            canceled: false,
            data: null
        });
    }

    private showMessage(message: string, title: string): void
    {
        let dialogRequest: ShowMessageDialogClientRequest<ShowMessageDialogClientResponse> =
            new ShowMessageDialogClientRequest<ShowMessageDialogClientResponse>(<IMessageDialogOptions>{
                title: title,
                message: message
            });
        this.context.runtime.executeAsync(dialogRequest);
    }

    private async cardReaderHardwareStationRequest(device: string, action: string, actionData: any): Promise<HardwareStationDeviceActionResponse>
    {
        let hardwareStationDeviceActivationRequest: HardwareStationDeviceActionRequest<HardwareStationDeviceActionResponse> =
            new HardwareStationDeviceActionRequest(device,
                action, actionData);
        let response: HardwareStationDeviceActionResponse = await(await this.context.runtime.executeAsync(hardwareStationDeviceActivationRequest)).data;
        return response;
    }

    
}

