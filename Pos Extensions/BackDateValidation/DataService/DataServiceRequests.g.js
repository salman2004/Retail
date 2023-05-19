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
                var ValidateTimeResponse = (function (_super) {
                    __extends(ValidateTimeResponse, _super);
                    function ValidateTimeResponse() {
                        return _super !== null && _super.apply(this, arguments) || this;
                    }
                    return ValidateTimeResponse;
                }(DataService_1.DataServiceResponse));
                StoreOperations.ValidateTimeResponse = ValidateTimeResponse;
                var ValidateTimeRequest = (function (_super) {
                    __extends(ValidateTimeRequest, _super);
                    function ValidateTimeRequest(deviceDateTime) {
                        var _this = _super.call(this) || this;
                        _this._entitySet = "";
                        _this._entityType = "";
                        _this._method = "ValidateTime";
                        _this._parameters = { deviceDateTime: deviceDateTime };
                        _this._isAction = true;
                        _this._returnType = null;
                        _this._isReturnTypeCollection = false;
                        return _this;
                    }
                    return ValidateTimeRequest;
                }(DataService_1.DataServiceRequest));
                StoreOperations.ValidateTimeRequest = ValidateTimeRequest;
            })(StoreOperations || (StoreOperations = {}));
            exports_1("StoreOperations", StoreOperations);
        }
    };
});
//# sourceMappingURL=C:/RetailSDK/Update/RetailSDK_FinalV28/Pos/Extensions/BackDateValidation/DataService/DataServiceRequests.g.js.map