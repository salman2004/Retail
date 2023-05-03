import * as CartOperations from "PosApi/Consume/Cart";
import { ClientEntities, ProxyEntities } from "PosApi/Entities";
import * as Triggers from "PosApi/Extend/Triggers/OperationTriggers";


export default class EhsasProgramFailureTrigger extends Triggers.PostOperationTrigger {

    /**
    * Executes the trigger functionality.
    * @param {Triggers.IOperationTriggerOptions} options The options provided to the trigger.
    */

    async execute(options: Triggers.IOperationTriggerOptions): Promise<void> {
        let operationId: ProxyEntities.RetailOperation = options.operationRequest.operationId;

        if (operationId.valueOf() == 5002) {
            let refreshCartRequest: CartOperations.RefreshCartClientRequest<CartOperations.RefreshCartClientResponse> = new CartOperations.RefreshCartClientRequest<CartOperations.RefreshCartClientResponse>('');
            await this.context.runtime.executeAsync<CartOperations.RefreshCartClientResponse>(refreshCartRequest);
        }
        return Promise.resolve();
    }
}