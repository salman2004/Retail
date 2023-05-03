import * as Triggers from "PosApi/Extend/Triggers/OperationTriggers";
export default class EhsasProgramFailureTrigger extends Triggers.OperationFailureTrigger {
    execute(options: Triggers.IOperationFailureTriggerOptions): Promise<void>;
}
