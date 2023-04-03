import { ProxyEntities } from "PosApi/Entities";
import { Entities } from "./DataServiceEntities.g";
import { DataServiceRequest, DataServiceResponse } from "PosApi/Consume/DataService";
export { ProxyEntities };
export { Entities };
export declare namespace StoreOperations {
    class ValidateFractionalSaleResponse extends DataServiceResponse {
        result: boolean;
    }
    class ValidateFractionalSaleRequest<TResponse extends ValidateFractionalSaleResponse> extends DataServiceRequest<TResponse> {
        constructor(ProductsInformation: Entities.ProductInformation[]);
    }
}
