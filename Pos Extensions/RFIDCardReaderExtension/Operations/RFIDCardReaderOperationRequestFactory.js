System.register(["./RFIDCardReaderOperationRequest"], function (exports_1, context_1) {
    "use strict";
    var __moduleName = context_1 && context_1.id;
    var RFIDCardReaderOperationRequest_1, getOperationRequest;
    return {
        setters: [
            function (RFIDCardReaderOperationRequest_1_1) {
                RFIDCardReaderOperationRequest_1 = RFIDCardReaderOperationRequest_1_1;
            }
        ],
        execute: function () {
            getOperationRequest = function (context, operationId, actionParameters, correlationId) {
                var operationRequest = new RFIDCardReaderOperationRequest_1.default(correlationId);
                return Promise.resolve({
                    canceled: false,
                    data: operationRequest
                });
            };
            exports_1("default", getOperationRequest);
        }
    };
});
//# sourceMappingURL=C:/RetailSDK/Update/RetailSDK_Version28/Pos/Extensions/RFIDCardReaderExtension/Operations/RFIDCardReaderOperationRequestFactory.js.map