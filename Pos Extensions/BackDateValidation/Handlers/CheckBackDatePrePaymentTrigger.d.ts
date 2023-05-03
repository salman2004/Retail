import { ClientEntities } from "PosApi/Entities";
import * as Triggers from "PosApi/Extend/Triggers/PaymentTriggers";
export default class CheckBackDatePrePaymentTrigger extends Triggers.PrePaymentTrigger {
    execute(options: Triggers.IPrePaymentTriggerOptions): Promise<ClientEntities.ICancelable>;
    private showMessage(message, title);
}
