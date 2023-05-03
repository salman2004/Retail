import * as Triggers from "PosApi/Extend/Triggers/PaymentTriggers";
import { ClientEntities } from "PosApi/Entities";
export default class CardReaderPreAddTenderLineTrigger extends Triggers.PreAddTenderLineTrigger {
    execute(options: Triggers.IPreAddTenderLineTriggerOptions): Promise<ClientEntities.ICancelable>;
    private getPropertyValue(extensionProperties, column);
}
