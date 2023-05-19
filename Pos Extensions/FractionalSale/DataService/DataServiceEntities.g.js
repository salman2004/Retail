System.register(["PosApi/Entities"], function (exports_1, context_1) {
    "use strict";
    var __moduleName = context_1 && context_1.id;
    var Entities_1, Entities;
    return {
        setters: [
            function (Entities_1_1) {
                Entities_1 = Entities_1_1;
            }
        ],
        execute: function () {
            exports_1("ProxyEntities", Entities_1.ProxyEntities);
            (function (Entities) {
                var ProductInformation = (function () {
                    function ProductInformation(odataObject) {
                        odataObject = odataObject || {};
                        this.RetailStoreId = odataObject.RetailStoreId;
                        this.ProductId = (odataObject.ProductId != null) ? parseInt(odataObject.ProductId, 10) : undefined;
                        this.UnitOfMeasure = odataObject.UnitOfMeasure;
                    }
                    return ProductInformation;
                }());
                Entities.ProductInformation = ProductInformation;
            })(Entities || (Entities = {}));
            exports_1("Entities", Entities);
        }
    };
});
//# sourceMappingURL=C:/RetailSDK/Update/RetailSDK_FinalV28/Pos/Extensions/FractionalSale/DataService/DataServiceEntities.g.js.map