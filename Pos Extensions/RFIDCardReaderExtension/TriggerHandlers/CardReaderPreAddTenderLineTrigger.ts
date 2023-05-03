import * as Triggers from "PosApi/Extend/Triggers/PaymentTriggers";
import { ClientEntities, ProxyEntities } from "PosApi/Entities";
import { ObjectExtensions } from "PosApi/TypeExtensions";
import { HardwareStationDeviceActionRequest, HardwareStationDeviceActionResponse } from "PosApi/Consume/Peripherals";
import { IMessageDialogOptions, ShowMessageDialogClientRequest, ShowMessageDialogClientResponse } from "PosApi/Consume/Dialogs";
import { CalculateTotalOperationRequest, CalculateTotalOperationResponse } from "PosApi/Consume/Cart";

export default class CardReaderPreAddTenderLineTrigger extends Triggers.PreAddTenderLineTrigger {


    async execute(options: Triggers.IPreAddTenderLineTriggerOptions): Promise<ClientEntities.ICancelable> {

        let properties: Commerce.Proxy.Entities.CommerceProperty[] = options.cart.ExtensionProperties;

        let tenderLineSum: number = 0;

        if (!ObjectExtensions.isNullOrUndefined(options.cart) && !ObjectExtensions.isNullOrUndefined(options.cart.TenderLines) && options.cart.TenderLines.length > 0) {
            options.cart.TenderLines.forEach((tl) => {
                if (tl.VoidStatusValue != 1) {
                    tenderLineSum = + tl.Amount;
                }
            });
        }

        if (Math.floor(tenderLineSum + options.tenderLine.Amount) < Math.floor(options.cart.AmountDue)) {
            return Promise.resolve({ 
                canceled: false
            });
        }

        if (ObjectExtensions.isNullOrUndefined(options.cart.ExtensionProperties) || options.cart.IsReturnByReceipt) {
            return Promise.resolve({ canceled: false });
        }
        
        let SampleCommerceProperties: ProxyEntities.CommerceProperty[] = options.cart.ExtensionProperties.filter((extensionProperty: ProxyEntities.CommerceProperty) => {
            return extensionProperty.Key === "CDCCardReaderValue";
        });

        if (SampleCommerceProperties.length <= 0) {
            return Promise.resolve({ canceled: false });
        }

        let cardReaderProperty: string = this.getPropertyValue(properties, "CDCCardReaderValue").StringValue;

        if (ObjectExtensions.isNullOrUndefined(cardReaderProperty) && cardReaderProperty == "" && cardReaderProperty == null || options.cart.LoyaltyCardId.length == 0) {
            return Promise.resolve({ canceled: false });
        }

        var obj = JSON.parse(cardReaderProperty);
        let isCardRebate: boolean = false;
        let usedPoints: string = "00000";
        let shopCode: string = this.getPropertyValue(properties, "CSDstoreId").StringValue;;

        if (obj.csdCardNumber != null && !ObjectExtensions.isNullOrUndefined(obj.csdCardNumber)) {
            let csdCardNumber: string = obj.csdCardNumber.toString();
            if (/^[a-zA-Z]+$/.test(csdCardNumber.charAt(0))) {
                usedPoints = this.getPropertyValue(properties, "CSDMonthlyLimitUsed").StringValue;
                isCardRebate = true;
            }
        }

        var WriteCardRequest = {
            shopCode: shopCode,
            usedPoints: usedPoints,
            cardInfo: cardReaderProperty,
            csdCardNumber: obj.csdCardNumber,
            writtenCardNumebr: obj.writtenCardNumebr,
            isRebateCard: isCardRebate
        };

        let hardwareStationDeviceActionRequest: HardwareStationDeviceActionRequest<HardwareStationDeviceActionResponse> =
            new HardwareStationDeviceActionRequest("RFIDCARDREADEREXTENSIONDEVICE",
                "WriteTransactionalDataOnCard", WriteCardRequest);
        await (await this.context.runtime.executeAsync(hardwareStationDeviceActionRequest)).data;

        return Promise.resolve({
            canceled: false,
            data: null
        });
    }

    private getPropertyValue(extensionProperties: ProxyEntities.CommerceProperty[], column: string): ProxyEntities.CommercePropertyValue {
        extensionProperties = extensionProperties || [];
        return extensionProperties.filter((prop: ProxyEntities.CommerceProperty) => prop.Key === column)
            .map((prop: ProxyEntities.CommerceProperty) => prop.Value)[0];
    }
    
    
}
