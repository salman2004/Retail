
  /* tslint:disable */
  import { ProxyEntities } from "PosApi/Entities";
  // @ts-ignore
  import { DateExtensions } from "PosApi/TypeExtensions";
  export { ProxyEntities };

  export namespace Entities {
  
  /**
   * ProductInformation entity class.
   */
  export class ProductInformation {
      public RetailStoreId: string;
      public ProductId: number;
      public UnitOfMeasure: string;
      
      // Navigation properties names
      
      /**
       * Construct an object from odata response.
       * @param {any} odataObject The odata result object.
       */
      constructor(odataObject?: any) {
      odataObject = odataObject || {};
            this.RetailStoreId = odataObject.RetailStoreId;
              
            this.ProductId = (odataObject.ProductId != null) ? parseInt(odataObject.ProductId, 10) : undefined;
              
            this.UnitOfMeasure = odataObject.UnitOfMeasure;
              
      }
  }

}
