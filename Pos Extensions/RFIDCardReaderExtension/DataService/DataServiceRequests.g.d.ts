import { ProxyEntities } from "PosApi/Entities";
import { Entities } from "./DataServiceEntities.g";
import { DataServiceRequest, DataServiceResponse } from "PosApi/Consume/DataService";
export { ProxyEntities };
export { Entities };
export declare namespace StoreOperations {
    class AuthenticateCardResponse extends DataServiceResponse {
        result: boolean;
    }
    class AuthenticateCardRequest<TResponse extends AuthenticateCardResponse> extends DataServiceRequest<TResponse> {
        constructor(cnicNumber: string, cardNumber: string);
    }
}
