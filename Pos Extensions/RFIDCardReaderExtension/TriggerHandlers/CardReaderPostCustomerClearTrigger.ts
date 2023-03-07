import * as Triggers from "PosApi/Extend/Triggers/CustomerTriggers";
import { SaveExtensionPropertiesOnCartClientRequest, SaveExtensionPropertiesOnCartClientResponse } from "PosApi/Consume/Cart";
import { ClientEntities, ProxyEntities } from "PosApi/Entities";
import { ObjectExtensions } from "PosApi/TypeExtensions";
import { HardwareStationDeviceActionRequest, HardwareStationDeviceActionResponse } from "PosApi/Consume/Peripherals";
import { Entities } from "../DataService/DataServiceRequests.g";

export default class CardReaderPostCustomerClearTrigger extends Triggers.PostCustomerClearTrigger
{
    async execute(options: Triggers.ICustomerClearTriggerOptions): Promise<void> {

        options.cart.ExtensionProperties = [];

        let cartExtensionProperty: ProxyEntities.CommerceProperty = new ProxyEntities.CommercePropertyClass();
        cartExtensionProperty.Key = "CDCCardReaderValue";
        cartExtensionProperty.Value = new ProxyEntities.CommercePropertyValueClass();
        cartExtensionProperty.Value.StringValue = '';

        let cartExtensionPropertyCardBalnce: ProxyEntities.CommerceProperty = new ProxyEntities.CommercePropertyClass();
        cartExtensionPropertyCardBalnce.Key = "CSDCardBalance";
        cartExtensionPropertyCardBalnce.Value = new ProxyEntities.CommercePropertyValueClass();
        cartExtensionPropertyCardBalnce.Value.StringValue = '00000';

        let cartExtensionPropertyCardNumber: ProxyEntities.CommerceProperty = new ProxyEntities.CommercePropertyClass();
        cartExtensionPropertyCardNumber.Key = "CSDCardNumber";
        cartExtensionPropertyCardNumber.Value = new ProxyEntities.CommercePropertyValueClass();
        cartExtensionPropertyCardNumber.Value.StringValue = '00000';

        let saveExtensionProperty: SaveExtensionPropertiesOnCartClientRequest<SaveExtensionPropertiesOnCartClientResponse>
            = new SaveExtensionPropertiesOnCartClientRequest<SaveExtensionPropertiesOnCartClientResponse>([cartExtensionProperty, cartExtensionPropertyCardNumber, cartExtensionPropertyCardBalnce, cartExtensionPropertyCardNumber], '');
        await this.context.runtime.executeAsync(saveExtensionProperty);

        return Promise.resolve();
    }
        
}
    