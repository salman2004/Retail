
/* tslint:disable */
import { ProxyEntities } from "PosApi/Entities";
import { Entities } from "./DataServiceEntities.g";
import { DataServiceRequest, DataServiceResponse } from "PosApi/Consume/DataService";
export { ProxyEntities };
export { Entities };

  export namespace StoreOperations {

  export class ValidateBinNumberResponse extends DataServiceResponse {
    public result: boolean;
  }

  export class ValidateBinNumberRequest<TResponse extends ValidateBinNumberResponse> extends DataServiceRequest<TResponse> {
    /**
     * Constructor
     */
      public constructor(cardNumber: string, transactionId: string) {
        super();

        this._entitySet = "";
        this._entityType = "";
        this._method = "ValidateBinNumber";
        this._parameters = { cardNumber: cardNumber, transactionId: transactionId };
        this._isAction = true;
        this._returnType = null;
        this._isReturnTypeCollection = false;
        
      }
  }

}
