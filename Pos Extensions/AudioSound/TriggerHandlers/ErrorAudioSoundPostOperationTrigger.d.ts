import * as Triggers from "PosApi/Extend/Triggers/OperationTriggers";
export default class ErrorAudioSoundPostOperationTrigger extends Triggers.OperationFailureTrigger {
    execute(options: Triggers.IOperationFailureTriggerOptions): Promise<void>;
}
