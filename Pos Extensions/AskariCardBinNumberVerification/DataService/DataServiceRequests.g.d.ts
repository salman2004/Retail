import { ProxyEntities } from "PosApi/Entities";
import { Entities } from "./DataServiceEntities.g";
import { DataServiceRequest, DataServiceResponse } from "PosApi/Consume/DataService";
export { ProxyEntities };
export { Entities };
export declare namespace StoreOperations {
    class ValidateBinNumberResponse extends DataServiceResponse {
        result: boolean;
    }
    class ValidateBinNumberRequest<TResponse extends ValidateBinNumberResponse> extends DataServiceRequest<TResponse> {
        constructor(cardNumber: string, transactionId: string);
    }
}
