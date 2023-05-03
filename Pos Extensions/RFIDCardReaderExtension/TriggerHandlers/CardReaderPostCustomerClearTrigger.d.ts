import * as Triggers from "PosApi/Extend/Triggers/CustomerTriggers";
export default class CardReaderPostCustomerClearTrigger extends Triggers.PostCustomerClearTrigger {
    execute(options: Triggers.ICustomerClearTriggerOptions): Promise<void>;
}
