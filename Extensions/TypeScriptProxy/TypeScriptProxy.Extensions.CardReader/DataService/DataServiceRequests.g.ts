
/* tslint:disable */
import { ProxyEntities } from "PosApi/Entities";
import { Entities } from "./DataServiceEntities.g";
import { DataServiceRequest, DataServiceResponse } from "PosApi/Consume/DataService";
export { ProxyEntities };
export { Entities };

  export namespace StoreOperations {

  export class AuthenticateCardResponse extends DataServiceResponse {
    public result: boolean;
  }

  export class AuthenticateCardRequest<TResponse extends AuthenticateCardResponse> extends DataServiceRequest<TResponse> {
    /**
     * Constructor
     */
      public constructor(cnicNumber: string, cardNumber: string) {
        super();

        this._entitySet = "";
        this._entityType = "";
        this._method = "AuthenticateCard";
        this._parameters = { cnicNumber: cnicNumber, cardNumber: cardNumber };
        this._isAction = true;
        this._returnType = null;
        this._isReturnTypeCollection = false;
        
      }
  }

}
