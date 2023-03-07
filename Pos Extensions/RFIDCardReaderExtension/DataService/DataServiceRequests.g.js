System.register(["PosApi/Entities", "PosApi/Consume/DataService"], function (exports_1, context_1) {
    "use strict";
    var __extends = (this && this.__extends) || (function () {
        var extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return function (d, b) {
            extendStatics(d, b);
            function __() { this.constructor = d; }
            d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
        };
    })();
    var __moduleName = context_1 && context_1.id;
    var Entities_1, DataService_1, StoreOperations;
    return {
        setters: [
            function (Entities_1_1) {
                Entities_1 = Entities_1_1;
            },
            function (DataService_1_1) {
                DataService_1 = DataService_1_1;
            }
        ],
        execute: function () {
            exports_1("ProxyEntities", Entities_1.ProxyEntities);
            (function (StoreOperations) {
                var AuthenticateCardResponse = (function (_super) {
                    __extends(AuthenticateCardResponse, _super);
                    function AuthenticateCardResponse() {
                        return _super !== null && _super.apply(this, arguments) || this;
                    }
                    return AuthenticateCardResponse;
                }(DataService_1.DataServiceResponse));
                StoreOperations.AuthenticateCardResponse = AuthenticateCardResponse;
                var AuthenticateCardRequest = (function (_super) {
                    __extends(AuthenticateCardRequest, _super);
                    function AuthenticateCardRequest(cnicNumber, cardNumber) {
                        var _this = _super.call(this) || this;
                        _this._entitySet = "";
                        _this._entityType = "";
                        _this._method = "AuthenticateCard";
                        _this._parameters = { cnicNumber: cnicNumber, cardNumber: cardNumber };
                        _this._isAction = true;
                        _this._returnType = null;
                        _this._isReturnTypeCollection = false;
                        return _this;
                    }
                    return AuthenticateCardRequest;
                }(DataService_1.DataServiceRequest));
                StoreOperations.AuthenticateCardRequest = AuthenticateCardRequest;
            })(StoreOperations || (StoreOperations = {}));
            exports_1("StoreOperations", StoreOperations);
        }
    };
});
//# sourceMappingURL=C:/RetailSDK/Update/RetailSDK_Version28/Pos/Extensions/RFIDCardReaderExtension/DataService/DataServiceRequests.g.js.map