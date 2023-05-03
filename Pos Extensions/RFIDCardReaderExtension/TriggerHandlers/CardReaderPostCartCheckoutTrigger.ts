/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

import * as Triggers from "PosApi/Extend/Triggers/TransactionTriggers";
import { ClientEntities, ProxyEntities } from "PosApi/Entities";
import { ObjectExtensions } from "PosApi/TypeExtensions";
import { HardwareStationDeviceActionRequest, HardwareStationDeviceActionResponse } from "PosApi/Consume/Peripherals";

export default class CardReaderPostCartCheckoutTrigger extends Triggers.PostCartCheckoutTrigger {
    /**
     * Executes the trigger functionality.
     * @param {Triggers.IPostCartCheckoutTriggerOptions} options The options provided to the trigger.
     */
    public async execute(options: Triggers.IPostCartCheckoutTriggerOptions): Promise<void>  {

        if (ObjectExtensions.isNullOrUndefined(options.salesOrder.ExtensionProperties) || options.cart.IsReturnByReceipt) {
            return Promise.resolve();
        }

        let properties: Commerce.Proxy.Entities.CommerceProperty[] = options.cart.ExtensionProperties;

        let SampleCommerceProperties: ProxyEntities.CommerceProperty[] = options.cart.ExtensionProperties.filter((extensionProperty: ProxyEntities.CommerceProperty) => {
            return extensionProperty.Key === "CDCCardReaderValue";
        });

        if (SampleCommerceProperties.length  <= 0) {
            return Promise.resolve();
        }
        let cardReaderProperty: string = this.getPropertyValue(properties, "CDCCardReaderValue").StringValue;

        if (ObjectExtensions.isNullOrUndefined(cardReaderProperty) && cardReaderProperty == null || options.cart.LoyaltyCardId.length == 0)
        {
            return Promise.resolve();
        }       

        var obj = JSON.parse(cardReaderProperty);
        let isCardRebate: boolean = false;
        let usedPoints: string = "00000"; 
        let shopCode: string = this.getPropertyValue(properties, "CSDstoreId").StringValue;;

        if (obj.csdCardNumber != null && !ObjectExtensions.isNullOrUndefined(obj.csdCardNumber))
        {
            let csdCardNumber: string = obj.csdCardNumber.toString();
            if (/^[a-zA-Z]+$/.test(csdCardNumber.charAt(0)))
            {
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
        await(await this.context.runtime.executeAsync(hardwareStationDeviceActionRequest)).data;
       

        return Promise.resolve();
    }

    private getPropertyValue(extensionProperties: ProxyEntities.CommerceProperty[], column: string): ProxyEntities.CommercePropertyValue {
        extensionProperties = extensionProperties || [];
        return extensionProperties.filter((prop: ProxyEntities.CommerceProperty) => prop.Key === column)
            .map((prop: ProxyEntities.CommerceProperty) => prop.Value)[0];
    }
}