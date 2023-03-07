import * as Triggers from "PosApi/Extend/Triggers/ApplicationTriggers";
export default class PostLogOnTrigger extends Triggers.PostLogOnTrigger {
    execute(options: Triggers.IPostLogOnTriggerOptions): Promise<void>;
}
