
/* tslint:disable */
import { ProxyEntities } from "PosApi/Entities";
import { Entities } from "./DataServiceEntities.g";
import { DataServiceRequest, DataServiceResponse } from "PosApi/Consume/DataService";
export { ProxyEntities };
export { Entities };

  export namespace StoreOperations {

  export class ValidateTimeResponse extends DataServiceResponse {
    public result: boolean;
  }

  export class ValidateTimeRequest<TResponse extends ValidateTimeResponse> extends DataServiceRequest<TResponse> {
    /**
     * Constructor
     */
      public constructor(deviceDateTime: string) {
        super();

        this._entitySet = "";
        this._entityType = "";
        this._method = "ValidateTime";
        this._parameters = { deviceDateTime: deviceDateTime };
        this._isAction = true;
        this._returnType = null;
        this._isReturnTypeCollection = false;
        
      }
  }

}
