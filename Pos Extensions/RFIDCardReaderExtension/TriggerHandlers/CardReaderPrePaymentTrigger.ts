import * as Triggers from "PosApi/Extend/Triggers/PaymentTriggers";
import { ClientEntities, ProxyEntities } from "PosApi/Entities";
import { ObjectExtensions } from "PosApi/TypeExtensions";
import { HardwareStationDeviceActionRequest, HardwareStationDeviceActionResponse } from "PosApi/Consume/Peripherals";

export default class CardReaderPrePaymentTrigger extends Triggers.PrePaymentTrigger
{
    
    
    async execute(options: Triggers.IPrePaymentTriggerOptions): Promise<ClientEntities.ICancelable> {
        
        let properties: Commerce.Proxy.Entities.CommerceProperty[] = options.cart.ExtensionProperties;

        let SampleCommerceProperties: ProxyEntities.CommerceProperty[] = options.cart.ExtensionProperties.filter((extensionProperty: ProxyEntities.CommerceProperty) => {
            return extensionProperty.Key === "CDCCardReaderValue";
        });

        let CSDCardNumber: ProxyEntities.CommerceProperty[] = options.cart.ExtensionProperties.filter((extensionProperty: ProxyEntities.CommerceProperty) => {
            return extensionProperty.Key === "CSDCardNumber";
        });

        if (options.cart.LoyaltyCardId == "" || ObjectExtensions.isNullOrUndefined(options.cart.LoyaltyCardId) || ObjectExtensions.isNullOrUndefined(CSDCardNumber)) {
            return Promise.resolve({
                canceled: false,
                data: null
            });
        }

        if (SampleCommerceProperties.length > 0)
        {
            let cardReaderProperty: string = this.getPropertyValue(properties, "CDCCardReaderValue").StringValue;

            if (ObjectExtensions.isNullOrUndefined(cardReaderProperty) || cardReaderProperty == "" || cardReaderProperty == null || options.cart.IsReturnByReceipt) {
                return Promise.resolve({
                    canceled: false,
                    data: null
                });
            }

            var obj = JSON.parse(cardReaderProperty);

            var customRequest = {
                Message: obj.writtenCardNumber.toString()
            };

            let hardwareStationDeviceActivationRequest: HardwareStationDeviceActionRequest<HardwareStationDeviceActionResponse> =
                new HardwareStationDeviceActionRequest("RFIDCARDREADEREXTENSIONDEVICE",
                    "checkCard", customRequest);
            let response: HardwareStationDeviceActionResponse = await (await this.context.runtime.executeAsync(hardwareStationDeviceActivationRequest)).data;

            if (response.response == true)
            {
                return Promise.resolve({
                    canceled: false
                });
            }
            else
            {
                options.cart.DiscountAmount = 0;
                options.cart.TotalManualDiscountAmount = 0.00;
                options.cart.TotalManualDiscountPercentage = 0.00;
                options.cart.CartLines.forEach((line) => {
                    line.DiscountAmount = 0;
                    line.DiscountLines = null;
                    line.LineManualDiscountAmount = 0;
                    line.LineManualDiscountPercentage = 0.00;
                    line.LineDiscount = 0;
                    line.LinePercentageDiscount = 0.00;
                });
                options.cart.AffiliationLines = null;

                return Promise.resolve({
                    canceled: true
                });
            }
        }
       
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
    