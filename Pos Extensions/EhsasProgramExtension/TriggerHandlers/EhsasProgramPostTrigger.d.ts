import * as Triggers from "PosApi/Extend/Triggers/OperationTriggers";
export default class EhsasProgramFailureTrigger extends Triggers.PostOperationTrigger {
    execute(options: Triggers.IOperationTriggerOptions): Promise<void>;
}
