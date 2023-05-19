System.register(["PosApi/Create/Operations", "PosApi/Entities", "../DataService/DataServiceRequests.g", "PosApi/Consume/Dialogs", "PosApi/Consume/Device", "./EhsasProgramOperationRequest", "PosApi/Consume/Cart"], function (exports_1, context_1) {
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
    var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
        return new (P || (P = Promise))(function (resolve, reject) {
            function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
            function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
            function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
            step((generator = generator.apply(thisArg, _arguments || [])).next());
        });
    };
    var __generator = (this && this.__generator) || function (thisArg, body) {
        var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t;
        return { next: verb(0), "throw": verb(1), "return": verb(2) };
        function verb(n) { return function (v) { return step([n, v]); }; }
        function step(op) {
            if (f) throw new TypeError("Generator is already executing.");
            while (_) try {
                if (f = 1, y && (t = y[op[0] & 2 ? "return" : op[0] ? "throw" : "next"]) && !(t = t.call(y, op[1])).done) return t;
                if (y = 0, t) op = [0, t.value];
                switch (op[0]) {
                    case 0: case 1: t = op; break;
                    case 4: _.label++; return { value: op[1], done: false };
                    case 5: _.label++; y = op[1]; op = [0]; continue;
                    case 7: op = _.ops.pop(); _.trys.pop(); continue;
                    default:
                        if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                        if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                        if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                        if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                        if (t[2]) _.ops.pop();
                        _.trys.pop(); continue;
                }
                op = body.call(thisArg, _);
            } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
            if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
        }
    };
    var __moduleName = context_1 && context_1.id;
    var Operations_1, Entities_1, DataServiceRequests_g_1, Dialogs_1, Device_1, EhsasProgramOperationRequest_1, CartOperations, EhsasProgramOperationRequestHandler;
    return {
        setters: [
            function (Operations_1_1) {
                Operations_1 = Operations_1_1;
            },
            function (Entities_1_1) {
                Entities_1 = Entities_1_1;
            },
            function (DataServiceRequests_g_1_1) {
                DataServiceRequests_g_1 = DataServiceRequests_g_1_1;
            },
            function (Dialogs_1_1) {
                Dialogs_1 = Dialogs_1_1;
            },
            function (Device_1_1) {
                Device_1 = Device_1_1;
            },
            function (EhsasProgramOperationRequest_1_1) {
                EhsasProgramOperationRequest_1 = EhsasProgramOperationRequest_1_1;
            },
            function (CartOperations_1) {
                CartOperations = CartOperations_1;
            }
        ],
        execute: function () {
            EhsasProgramOperationRequestHandler = (function (_super) {
                __extends(EhsasProgramOperationRequestHandler, _super);
                function EhsasProgramOperationRequestHandler() {
                    var _this = _super !== null && _super.apply(this, arguments) || this;
                    _this.SUCCESS = "Success";
                    return _this;
                }
                EhsasProgramOperationRequestHandler.prototype.supportedRequestType = function () {
                    return EhsasProgramOperationRequest_1.default;
                };
                EhsasProgramOperationRequestHandler.prototype.executeAsync = function (request) {
                    return __awaiter(this, void 0, void 0, function () {
                        var statusCheckRequest, statusCheckResponse, subTitleMsg, title, numPadLabel, result, productIds, product_1, products_1, cnicNumber, cartClientRequest, cartClientResponse, parameters, ehsasProgramRequest, ehsasProgramResponse, subsidyInquiryResInfo, otp, verifyOtpParameters, verifyOtp, verifyOtpResponse;
                        return __generator(this, function (_a) {
                            switch (_a.label) {
                                case 0:
                                    statusCheckRequest = new Device_1.GetConnectionStatusClientRequest();
                                    return [4 /*yield*/, this.context.runtime.executeAsync(statusCheckRequest)];
                                case 1:
                                    statusCheckResponse = (_a.sent()).data;
                                    if (statusCheckResponse.result != Entities_1.ClientEntities.ConnectionStatusType.Online) {
                                        this.showMessage("Ehsas program doesnot support offline", "Ehsas Program");
                                        return [2 /*return*/, Promise.resolve({
                                                canceled: true,
                                                data: null
                                            })];
                                    }
                                    subTitleMsg = "Plese enter national identification number.\n\n"
                                        + "CNIC Number should not include (-).\n\n";
                                    title = "Ehsas Program";
                                    numPadLabel = "National identification number:";
                                    return [4 /*yield*/, this.GetInputFromNumericInputDialog(subTitleMsg, title, numPadLabel)];
                                case 2:
                                    result = _a.sent();
                                    if (!!result.canceled) return [3 /*break*/, 9];
                                    productIds = [];
                                    products_1 = [];
                                    cnicNumber = result.data.result.value;
                                    cartClientRequest = new CartOperations.GetCurrentCartClientRequest();
                                    return [4 /*yield*/, this.context.runtime.executeAsync(cartClientRequest)];
                                case 3: return [4 /*yield*/, (_a.sent()).data];
                                case 4:
                                    cartClientResponse = _a.sent();
                                    cartClientResponse.result.CartLines.forEach(function (cartLine) {
                                        product_1 = new DataServiceRequests_g_1.Entities.Product();
                                        product_1.Amount = cartLine.Price,
                                            product_1.InventDimId = cartLine.InventoryDimensionId,
                                            product_1.ItemId = cartLine.ItemId,
                                            product_1.Quantity = cartLine.Quantity,
                                            product_1.ProductId = cartLine.ProductId;
                                        products_1.push(product_1);
                                    });
                                    parameters = new DataServiceRequests_g_1.Entities.BeneficiaryInquiryRequestParameters();
                                    parameters.CNICNumber = cnicNumber;
                                    parameters.Products = products_1;
                                    parameters.currentTransactionId = cartClientResponse.result.Id;
                                    ehsasProgramRequest = new DataServiceRequests_g_1.EhsasProgram.GetEhsasProgramVerificationRequest(parameters);
                                    return [4 /*yield*/, this.context.runtime.executeAsync(ehsasProgramRequest)];
                                case 5:
                                    ehsasProgramResponse = (_a.sent()).data;
                                    if (!ehsasProgramResponse.result.IsEhsasProgramAllowed) return [3 /*break*/, 9];
                                    subsidyInquiryResInfo = ehsasProgramResponse.result.SubsidyInquiryResponse.subsidyInquiryResTxnInfo;
                                    return [4 /*yield*/, this.getOtpResult(subsidyInquiryResInfo.authId, subsidyInquiryResInfo.cnic, ehsasProgramResponse.result.AuthToken, cartClientResponse.result.Id)];
                                case 6:
                                    otp = _a.sent();
                                    if (!(otp != "")) return [3 /*break*/, 9];
                                    verifyOtpParameters = new DataServiceRequests_g_1.Entities.VerifyEhsasProgramOtpRequestParameters();
                                    verifyOtpParameters.OTP = otp;
                                    verifyOtpParameters.AuthToken = ehsasProgramResponse.result.AuthToken;
                                    verifyOtpParameters.SubsidyInquiryResponse = ehsasProgramResponse.result.SubsidyInquiryResponse;
                                    verifyOtpParameters.currentTransactionId = cartClientResponse.result.Id;
                                    verifyOtp = new DataServiceRequests_g_1.EhsasProgram.VerifyEhsasProgramOtpRequest(verifyOtpParameters);
                                    return [4 /*yield*/, this.context.runtime.executeAsync(verifyOtp)];
                                case 7: return [4 /*yield*/, (_a.sent()).data];
                                case 8:
                                    verifyOtpResponse = _a.sent();
                                    _a.label = 9;
                                case 9: return [2 /*return*/, Promise.resolve({
                                        canceled: false,
                                        data: null
                                    })];
                            }
                        });
                    });
                };
                EhsasProgramOperationRequestHandler.prototype.getOtpResult = function (authId, cnic, authToken, currentTransactionId) {
                    return __awaiter(this, void 0, void 0, function () {
                        var otpDialogResult;
                        return __generator(this, function (_a) {
                            switch (_a.label) {
                                case 0: return [4 /*yield*/, this.GetInputFromAlphaNumericDialog(this.context, "Ehsas Program", "Enter verification code.\n\n", "One time password")];
                                case 1:
                                    otpDialogResult = (_a.sent());
                                    if (!otpDialogResult.canceled) return [3 /*break*/, 2];
                                    return [2 /*return*/, this.resendOtp(authId, cnic, authToken, currentTransactionId)];
                                case 2: return [4 /*yield*/, otpDialogResult.data.result.value];
                                case 3: return [2 /*return*/, _a.sent()];
                            }
                        });
                    });
                };
                EhsasProgramOperationRequestHandler.prototype.resendOtp = function (authId, cnic, authToken, currentTransactionId) {
                    return __awaiter(this, void 0, void 0, function () {
                        var resendOtpDialogResult, resendOtpRequestParameter, resendOtprequest, resendOtpResponse;
                        return __generator(this, function (_a) {
                            switch (_a.label) {
                                case 0: return [4 /*yield*/, this.showMessageBox(this.context, "Didn't recieve otp", "Ehsas Program", "Did'nt recevice otp. Click Resend OTP or cancel ehsas applicable discount")];
                                case 1:
                                    resendOtpDialogResult = _a.sent();
                                    if (!(resendOtpDialogResult.data.result.dialogResult != "CancelResult")) return [3 /*break*/, 4];
                                    resendOtpRequestParameter = new DataServiceRequests_g_1.Entities.ResendOtpRequestParameter();
                                    resendOtpRequestParameter.AuthId = authId;
                                    resendOtpRequestParameter.Cnic = cnic;
                                    resendOtpRequestParameter.AuthToken = authToken;
                                    resendOtpRequestParameter.MerchantId = "";
                                    resendOtpRequestParameter.currentTransactionId = currentTransactionId;
                                    resendOtprequest = new DataServiceRequests_g_1.EhsasProgram.ResendEhsasProgramOtpRequest(resendOtpRequestParameter);
                                    return [4 /*yield*/, this.context.runtime.executeAsync(resendOtprequest)];
                                case 2: return [4 /*yield*/, (_a.sent()).data];
                                case 3:
                                    resendOtpResponse = _a.sent();
                                    return [2 /*return*/, this.getOtpResult(authId, cnic, authToken, currentTransactionId)];
                                case 4: return [2 /*return*/, ""];
                            }
                        });
                    });
                };
                EhsasProgramOperationRequestHandler.prototype.showMessage = function (message, title) {
                    var dialogRequest = new Dialogs_1.ShowMessageDialogClientRequest({
                        title: title,
                        message: message
                    });
                    this.context.runtime.executeAsync(dialogRequest);
                };
                EhsasProgramOperationRequestHandler.prototype.GetInputFromNumericInputDialog = function (subTitleMsg, titleMsg, numPadLabel) {
                    return __awaiter(this, void 0, void 0, function () {
                        var numericInputDialogOptions, dialogRequest;
                        return __generator(this, function (_a) {
                            switch (_a.label) {
                                case 0:
                                    numericInputDialogOptions = {
                                        title: titleMsg,
                                        subTitle: subTitleMsg,
                                        numPadLabel: numPadLabel,
                                        defaultNumber: ""
                                    };
                                    dialogRequest = new Dialogs_1.ShowNumericInputDialogClientRequest(numericInputDialogOptions);
                                    return [4 /*yield*/, this.context.runtime.executeAsync(dialogRequest)];
                                case 1: return [4 /*yield*/, (_a.sent())];
                                case 2: return [2 /*return*/, _a.sent()];
                            }
                        });
                    });
                };
                EhsasProgramOperationRequestHandler.prototype.GetInputFromAlphaNumericDialog = function (context, title, subTitleMsg, numPadLabel) {
                    return __awaiter(this, void 0, void 0, function () {
                        var alphanumericInputDialogOptions, dialogRequest;
                        return __generator(this, function (_a) {
                            switch (_a.label) {
                                case 0:
                                    alphanumericInputDialogOptions = {
                                        title: title,
                                        subTitle: subTitleMsg,
                                        numPadLabel: numPadLabel,
                                        defaultValue: ""
                                    };
                                    dialogRequest = new Dialogs_1.ShowAlphanumericInputDialogClientRequest(alphanumericInputDialogOptions);
                                    return [4 /*yield*/, context.runtime.executeAsync(dialogRequest)];
                                case 1: return [2 /*return*/, _a.sent()];
                            }
                        });
                    });
                };
                EhsasProgramOperationRequestHandler.prototype.showMessageBox = function (context, subTitle, title, message) {
                    return __awaiter(this, void 0, void 0, function () {
                        var messageDialogOptions, dialogRequest;
                        return __generator(this, function (_a) {
                            switch (_a.label) {
                                case 0:
                                    messageDialogOptions = {
                                        title: title,
                                        subTitle: subTitle,
                                        message: message,
                                        showCloseX: true,
                                        button1: {
                                            id: "RESENDOTP",
                                            label: "Resend OTP",
                                            result: "RESENDOTP"
                                        },
                                        button2: {
                                            id: "Cancel",
                                            label: "Cancel",
                                            result: "CancelResult"
                                        }
                                    };
                                    dialogRequest = new Dialogs_1.ShowMessageDialogClientRequest(messageDialogOptions);
                                    return [4 /*yield*/, context.runtime.executeAsync(dialogRequest)];
                                case 1: return [2 /*return*/, _a.sent()];
                            }
                        });
                    });
                };
                return EhsasProgramOperationRequestHandler;
            }(Operations_1.ExtensionOperationRequestHandlerBase));
            exports_1("default", EhsasProgramOperationRequestHandler);
        }
    };
});
//# sourceMappingURL=C:/RetailSDK/Update/RetailSDK_FinalV28/Pos/Extensions/EhsasProgramExtension/Operations/EhsasProgramOperationRequestHandler.js.map