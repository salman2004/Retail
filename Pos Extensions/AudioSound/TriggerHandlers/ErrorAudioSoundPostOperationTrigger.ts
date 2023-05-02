import * as Triggers from "PosApi/Extend/Triggers/OperationTriggers";
import { SaveExtensionPropertiesOnCartClientRequest, SaveExtensionPropertiesOnCartClientResponse } from "PosApi/Consume/Cart";

export default class ErrorAudioSoundPostOperationTrigger extends Triggers.OperationFailureTrigger {

    execute(options: Triggers.IOperationFailureTriggerOptions): Promise<void> {

        const audio = new Audio("../../../Assets/windows-error-sound-effect-35894.mp3");
        audio.play();

        return Promise.resolve();
    }
    

}
