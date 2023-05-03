import { ClientEntities, ProxyEntities } from "PosApi/Entities";
import * as Triggers from "PosApi/Extend/Triggers/OperationTriggers";
import { ObjectExtensions } from "PosApi/TypeExtensions";
import { StoreOperations } from "../DataService/DataServiceRequests.g";
import { Entities } from "../DataService/DataServiceEntities.g";
import { IMessageDialogOptions, ShowMessageDialogClientRequest, ShowMessageDialogClientResponse } from "PosApi/Consume/Dialogs";

export default class FractionalSalePreOperationTrigger extends Triggers.PreOperationTrigger {
    /**
     * Executes the trigger functionality.
     * @param {Triggers.IOperationTriggerOptions} options The options provided to the trigger.
     */
    public async execute(options: Triggers.IOperationTriggerOptions): Promise<ClientEntities.ICancelable> {
        let operationId: ProxyEntities.RetailOperation = options.operationRequest.operationId;
        if (operationId == Commerce.Proxy.Entities.RetailOperation.ChangeUnitOfMeasure)
        {
            var cartLineUnitOfMeasures = options.operationRequest["options"]["cartLineUnitOfMeasures"];
            let productsInformation = new Array<Entities.ProductInformation>()

            if (!ObjectExtensions.isNullOrUndefined(cartLineUnitOfMeasures)) {
                for (let index = 0; index < cartLineUnitOfMeasures.length; index++) {
                    let productInformation: Entities.ProductInformation = {
                        RetailStoreId: cartLineUnitOfMeasures[index].cartLine.WarehouseId,
                        ProductId: cartLineUnitOfMeasures[index].cartLine.ProductId,
                        UnitOfMeasure: cartLineUnitOfMeasures[index].cartLine.UnitOfMeasureSymbol
                    };
                    productsInformation.push(productInformation);
                }

            }
            let res: StoreOperations.ValidateFractionalSaleRequest<StoreOperations.ValidateFractionalSaleResponse> = new StoreOperations.ValidateFractionalSaleRequest<StoreOperations.ValidateFractionalSaleResponse>(productsInformation);

            let response: StoreOperations.ValidateFractionalSaleResponse = await (await this.context.runtime.executeAsync(res)).data;

            if (!response.result) {
                this.showMessage("The product is not authorized for fractional sale.", "Error");
                return Promise.resolve({ canceled: true, data: null });
            }
        }
        return Promise.resolve({ canceled: false, data: null });
    }

    private showMessage(message: string, title: string): void {
        let dialogRequest: ShowMessageDialogClientRequest<ShowMessageDialogClientResponse> =
            new ShowMessageDialogClientRequest<ShowMessageDialogClientResponse>(<IMessageDialogOptions>{
                title: title,
                message: message
            });
        this.context.runtime.executeAsync(dialogRequest);
    }
}