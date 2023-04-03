import { ProxyEntities } from "PosApi/Entities";
import { Entities } from "./DataServiceEntities.g";
import { DataServiceRequest, DataServiceResponse } from "PosApi/Consume/DataService";
export { ProxyEntities };
export { Entities };
export declare namespace StoreOperations {
    class ValidateTimeResponse extends DataServiceResponse {
        result: boolean;
    }
    class ValidateTimeRequest<TResponse extends ValidateTimeResponse> extends DataServiceRequest<TResponse> {
        constructor(deviceDateTime: string);
    }
}
