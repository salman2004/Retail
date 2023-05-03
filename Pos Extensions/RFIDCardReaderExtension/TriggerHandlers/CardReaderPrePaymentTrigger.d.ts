import * as Triggers from "PosApi/Extend/Triggers/PaymentTriggers";
import { ClientEntities } from "PosApi/Entities";
export default class CardReaderPrePaymentTrigger extends Triggers.PrePaymentTrigger {
    execute(options: Triggers.IPrePaymentTriggerOptions): Promise<ClientEntities.ICancelable>;
    private getPropertyValue(extensionProperties, column);
}
