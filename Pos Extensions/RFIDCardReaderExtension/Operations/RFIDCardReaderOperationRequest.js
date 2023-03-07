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
    var Operations_1, EndOfDayOperationRequest;
    return {
        setters: [
            function (Operations_1_1) {
                Operations_1 = Operations_1_1;
            }
        ],
        execute: function () {
            EndOfDayOperationRequest = (function (_super) {
                __extends(EndOfDayOperationRequest, _super);
                function EndOfDayOperationRequest(correlationId) {
                    return _super.call(this, 5001, correlationId) || this;
                }
                return EndOfDayOperationRequest;
            }(Operations_1.ExtensionOperationRequestBase));
            exports_1("default", EndOfDayOperationRequest);
        }
    };
});
//# sourceMappingURL=C:/RetailSDK/Update/RetailSDK_Version28/Pos/Extensions/RFIDCardReaderExtension/Operations/RFIDCardReaderOperationRequest.js.map