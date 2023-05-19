System.register(["./EhsasProgramOperationRequest"], function (exports_1, context_1) {
    "use strict";
    var __moduleName = context_1 && context_1.id;
    var EhsasProgramOperationRequest_1, getOperationRequest;
    return {
        setters: [
            function (EhsasProgramOperationRequest_1_1) {
                EhsasProgramOperationRequest_1 = EhsasProgramOperationRequest_1_1;
            }
        ],
        execute: function () {
            getOperationRequest = function (context, operationId, actionParameters, correlationId) {
                var operationRequest = new EhsasProgramOperationRequest_1.default(correlationId);
                return Promise.resolve({
                    canceled: false,
                    data: operationRequest
                });
            };
            exports_1("default", getOperationRequest);
        }
    };
});
//# sourceMappingURL=C:/RetailSDK/Update/RetailSDK_FinalV28/Pos/Extensions/EhsasProgramExtension/Operations/EhsasProgramOperationRequestFactory.js.map