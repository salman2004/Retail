System.register(["PosApi/Extend/RequestHandlers/ScanResultsRequestHandlers", "../Global"], function (exports_1, context_1) {
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
    var requestHandler, Global_1, ScanResultClientRequestHandler;
    return {
        setters: [
            function (requestHandler_1) {
                requestHandler = requestHandler_1;
            },
            function (Global_1_1) {
                Global_1 = Global_1_1;
            }
        ],
        execute: function () {
            ScanResultClientRequestHandler = (function (_super) {
                __extends(ScanResultClientRequestHandler, _super);
                function ScanResultClientRequestHandler() {
                    return _super !== null && _super.apply(this, arguments) || this;
                }
                ScanResultClientRequestHandler.prototype.executeAsync = function (request) {
                    Global_1.Global.Barcode = request.scanText;
                    return this.defaultExecuteAsync(request);
                };
                return ScanResultClientRequestHandler;
            }(requestHandler.GetScanResultClientRequestHandler));
            exports_1("default", ScanResultClientRequestHandler);
        }
    };
});
//# sourceMappingURL=C:/RetailSDK/Update/RetailSDK_FinalV28/Pos/Extensions/ProductBarcode/RequestHandlers/ScanResultClientRequestHandler.js.map