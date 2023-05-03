
/* tslint:disable */
import { ProxyEntities } from "PosApi/Entities";
import { Entities } from "./DataServiceEntities.g";
import { DataServiceRequest, DataServiceResponse } from "PosApi/Consume/DataService";
export { ProxyEntities };
export { Entities };

  export namespace StoreOperations {

  export class ValidateFractionalSaleResponse extends DataServiceResponse {
    public result: boolean;
  }

  export class ValidateFractionalSaleRequest<TResponse extends ValidateFractionalSaleResponse> extends DataServiceRequest<TResponse> {
    /**
     * Constructor
     */
      public constructor(ProductsInformation: Entities.ProductInformation[]) {
        super();

        this._entitySet = "";
        this._entityType = "";
        this._method = "ValidateFractionalSale";
        this._parameters = { ProductsInformation: ProductsInformation };
        this._isAction = true;
        this._returnType = null;
        this._isReturnTypeCollection = false;
        
      }
  }

}
