System.register(["PosApi/Create/Operations"], function (exports_1, context_1) {
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
    var Operations_1, EhsasProgramOperationRequest;
    return {
        setters: [
            function (Operations_1_1) {
                Operations_1 = Operations_1_1;
            }
        ],
        execute: function () {
            EhsasProgramOperationRequest = (function (_super) {
                __extends(EhsasProgramOperationRequest, _super);
                function EhsasProgramOperationRequest(correlationId) {
                    return _super.call(this, 5002, correlationId) || this;
                }
                return EhsasProgramOperationRequest;
            }(Operations_1.ExtensionOperationRequestBase));
            exports_1("default", EhsasProgramOperationRequest);
        }
    };
});
//# sourceMappingURL=C:/RetailSDK/Update/RetailSDK_Version28/Pos/Extensions/EhsasProgramExtension/Operations/EhsasProgramOperationRequest.js.map