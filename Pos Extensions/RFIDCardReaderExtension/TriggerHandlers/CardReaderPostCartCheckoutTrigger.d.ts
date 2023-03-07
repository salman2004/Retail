import * as Triggers from "PosApi/Extend/Triggers/TransactionTriggers";
export default class CardReaderPostCartCheckoutTrigger extends Triggers.PostCartCheckoutTrigger {
    execute(options: Triggers.IPostCartCheckoutTriggerOptions): Promise<void>;
    private getPropertyValue(extensionProperties, column);
}
