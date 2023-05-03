import { ClientEntities } from "PosApi/Entities";
import * as Triggers from "PosApi/Extend/Triggers/OperationTriggers";
export default class FractionalSalePreOperationTrigger extends Triggers.PreOperationTrigger {
    execute(options: Triggers.IOperationTriggerOptions): Promise<ClientEntities.ICancelable>;
    private showMessage(message, title);
}
