import { ClientEntities } from "PosApi/Entities";
import * as Triggers from "PosApi/Extend/Triggers/ApplicationTriggers";
export default class PreLogOnTrigger extends Triggers.PreLogOnTrigger {
    execute(options: Triggers.IPreLogOnTriggerOptions): Promise<ClientEntities.ICancelable>;
    private showMessage(message, title);
}
