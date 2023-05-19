System.register(["PosApi/Entities", "./DataServiceEntities.g", "PosApi/Consume/DataService"], function (exports_1, context_1) {
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
    var Entities_1, DataServiceEntities_g_1, DataService_1, EhsasProgram;
    return {
        setters: [
            function (Entities_1_1) {
                Entities_1 = Entities_1_1;
            },
            function (DataServiceEntities_g_1_1) {
                DataServiceEntities_g_1 = DataServiceEntities_g_1_1;
            },
            function (DataService_1_1) {
                DataService_1 = DataService_1_1;
            }
        ],
        execute: function () {
            exports_1("ProxyEntities", Entities_1.ProxyEntities);
            exports_1("Entities", DataServiceEntities_g_1.Entities);
            (function (EhsasProgram) {
                var GetEhsasProgramVerificationResponse = (function (_super) {
                    __extends(GetEhsasProgramVerificationResponse, _super);
                    function GetEhsasProgramVerificationResponse() {
                        return _super !== null && _super.apply(this, arguments) || this;
                    }
                    return GetEhsasProgramVerificationResponse;
                }(DataService_1.DataServiceResponse));
                EhsasProgram.GetEhsasProgramVerificationResponse = GetEhsasProgramVerificationResponse;
                var GetEhsasProgramVerificationRequest = (function (_super) {
                    __extends(GetEhsasProgramVerificationRequest, _super);
                    function GetEhsasProgramVerificationRequest(beneficiaryParameters) {
                        var _this = _super.call(this) || this;
                        _this._entitySet = "EhsasProgram";
                        _this._entityType = "EhsasProgramEntity";
                        _this._method = "GetEhsasProgramVerification";
                        _this._parameters = { beneficiaryParameters: beneficiaryParameters };
                        _this._isAction = true;
                        _this._returnType = DataServiceEntities_g_1.Entities.EhsasProgramEntity;
                        _this._isReturnTypeCollection = false;
                        return _this;
                    }
                    return GetEhsasProgramVerificationRequest;
                }(DataService_1.DataServiceRequest));
                EhsasProgram.GetEhsasProgramVerificationRequest = GetEhsasProgramVerificationRequest;
                var VerifyEhsasProgramOtpResponse = (function (_super) {
                    __extends(VerifyEhsasProgramOtpResponse, _super);
                    function VerifyEhsasProgramOtpResponse() {
                        return _super !== null && _super.apply(this, arguments) || this;
                    }
                    return VerifyEhsasProgramOtpResponse;
                }(DataService_1.DataServiceResponse));
                EhsasProgram.VerifyEhsasProgramOtpResponse = VerifyEhsasProgramOtpResponse;
                var VerifyEhsasProgramOtpRequest = (function (_super) {
                    __extends(VerifyEhsasProgramOtpRequest, _super);
                    function VerifyEhsasProgramOtpRequest(otpRequestParameters) {
                        var _this = _super.call(this) || this;
                        _this._entitySet = "EhsasProgram";
                        _this._entityType = "EhsasProgramEntity";
                        _this._method = "VerifyEhsasProgramOtp";
                        _this._parameters = { otpRequestParameters: otpRequestParameters };
                        _this._isAction = true;
                        _this._returnType = DataServiceEntities_g_1.Entities.VerifyEhsasProgramOtpResponseEntity;
                        _this._isReturnTypeCollection = false;
                        return _this;
                    }
                    return VerifyEhsasProgramOtpRequest;
                }(DataService_1.DataServiceRequest));
                EhsasProgram.VerifyEhsasProgramOtpRequest = VerifyEhsasProgramOtpRequest;
                var ResendEhsasProgramOtpResponse = (function (_super) {
                    __extends(ResendEhsasProgramOtpResponse, _super);
                    function ResendEhsasProgramOtpResponse() {
                        return _super !== null && _super.apply(this, arguments) || this;
                    }
                    return ResendEhsasProgramOtpResponse;
                }(DataService_1.DataServiceResponse));
                EhsasProgram.ResendEhsasProgramOtpResponse = ResendEhsasProgramOtpResponse;
                var ResendEhsasProgramOtpRequest = (function (_super) {
                    __extends(ResendEhsasProgramOtpRequest, _super);
                    function ResendEhsasProgramOtpRequest(resendOtpRequestParameters) {
                        var _this = _super.call(this) || this;
                        _this._entitySet = "EhsasProgram";
                        _this._entityType = "EhsasProgramEntity";
                        _this._method = "ResendEhsasProgramOtp";
                        _this._parameters = { resendOtpRequestParameters: resendOtpRequestParameters };
                        _this._isAction = true;
                        _this._returnType = DataServiceEntities_g_1.Entities.ResendOtpResponseEntity;
                        _this._isReturnTypeCollection = false;
                        return _this;
                    }
                    return ResendEhsasProgramOtpRequest;
                }(DataService_1.DataServiceRequest));
                EhsasProgram.ResendEhsasProgramOtpRequest = ResendEhsasProgramOtpRequest;
            })(EhsasProgram || (EhsasProgram = {}));
            exports_1("EhsasProgram", EhsasProgram);
        }
    };
});
//# sourceMappingURL=C:/RetailSDK/Update/RetailSDK_FinalV28/Pos/Extensions/EhsasProgramExtension/DataService/DataServiceRequests.g.js.map