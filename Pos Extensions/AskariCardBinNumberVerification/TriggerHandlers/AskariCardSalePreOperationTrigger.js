System.register(["PosApi/Consume/Dialogs", "PosApi/Extend/Triggers/OperationTriggers", "PosApi/TypeExtensions", "../DataService/DataServiceRequests.g", "../Global", "PosApi/Consume/Cart"], function (exports_1, context_1) {
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
    var Dialogs_1, Triggers, TypeExtensions_1, DataServiceRequests_g_1, Global_1, CartOperations, AskariCardSalePreOperationTrigger;
    return {
        setters: [
            function (Dialogs_1_1) {
                Dialogs_1 = Dialogs_1_1;
            },
            function (Triggers_1) {
                Triggers = Triggers_1;
            },
            function (TypeExtensions_1_1) {
                TypeExtensions_1 = TypeExtensions_1_1;
            },
            function (DataServiceRequests_g_1_1) {
                DataServiceRequests_g_1 = DataServiceRequests_g_1_1;
            },
            function (Global_1_1) {
                Global_1 = Global_1_1;
            },
            function (CartOperations_1) {
                CartOperations = CartOperations_1;
            }
        ],
        execute: function () {
            AskariCardSalePreOperationTrigger = (function (_super) {
                __extends(AskariCardSalePreOperationTrigger, _super);
                function AskariCardSalePreOperationTrigger() {
                    return _super !== null && _super.apply(this, arguments) || this;
                }
                AskariCardSalePreOperationTrigger.prototype.execute = function (options) {
                    return __awaiter(this, void 0, void 0, function () {
                        var operationId, cartClientResponse, re, numPadOptions, dialogRequest, result_1, reasonCodeLine, reasonCodeLines, saveReasonCodeLine, saveReasonCodeLinesOnCartClientResponse, saveReasonCodeLine, saveReasonCodeLinesOnCartClientResponse, validateBinNumberRequest, refreshRequest, validateBinNumberResponse;
                        return __generator(this, function (_a) {
                            switch (_a.label) {
                                case 0:
                                    operationId = options.operationRequest.operationId;
                                    if (TypeExtensions_1.StringExtensions.isNullOrWhitespace(Global_1.Global.AskariCardOperationType) || TypeExtensions_1.StringExtensions.isNullOrWhitespace(Global_1.Global.AskariCardTenderMethod)) {
                                        return [2 /*return*/, Promise.resolve({ canceled: false })];
                                    }
                                    if (!(Number(operationId) == Number(Global_1.Global.AskariCardOperationType))) return [3 /*break*/, 14];
                                    return [4 /*yield*/, this.getCurrentCart()];
                                case 1:
                                    cartClientResponse = (_a.sent());
                                    if (!!TypeExtensions_1.ObjectExtensions.isNullOrUndefined(options.operationRequest["options"]["tenderType"])) return [3 /*break*/, 14];
                                    re = options.operationRequest["options"]["tenderType"];
                                    if (!(Number(re.TenderTypeId) == Number(Global_1.Global.AskariCardTenderMethod))) return [3 /*break*/, 14];
                                    return [4 /*yield*/, this.getCurrentCart()];
                                case 2:
                                    if ((_a.sent()).result.IsReturnByReceipt && cartClientResponse.result.AmountDue < 0) {
                                        this.showMessage("Tender type cannot be used in return transaction.", "Askari card");
                                        return [2 /*return*/, Promise.resolve({ canceled: true })];
                                    }
                                    numPadOptions = {
                                        title: "Pay using askari card ",
                                        subTitle: "For extra discount",
                                        numPadLabel: "Please enter 16-digit card number:",
                                        defaultNumber: ""
                                    };
                                    dialogRequest = new Dialogs_1.ShowNumericInputDialogClientRequest(numPadOptions);
                                    return [4 /*yield*/, this.context.runtime.executeAsync(dialogRequest)];
                                case 3:
                                    result_1 = _a.sent();
                                    if (!!result_1.canceled) return [3 /*break*/, 13];
                                    if (result_1.data.result.value.length != 16) {
                                        this.showMessage("Card number should be 16 digits", "Askari card");
                                        return [2 /*return*/, Promise.resolve({ canceled: true })];
                                    }
                                    reasonCodeLine = new Commerce.Proxy.Entities.ReasonCodeLineClass();
                                    reasonCodeLine.ReasonCodeId = Global_1.Global.AskariCardInfoCode;
                                    reasonCodeLine.Amount = 0;
                                    reasonCodeLine.Information = result_1.data.result.value;
                                    reasonCodeLine.TransactionId = cartClientResponse.result.Id;
                                    reasonCodeLine.InputTypeValue = Commerce.Proxy.Entities.ReasonCodeInputType.Text;
                                    if (!(cartClientResponse.result.ReasonCodeLines.filter(function (rl) { return rl.ReasonCodeId == Global_1.Global.AskariCardInfoCode; }).length > 0)) return [3 /*break*/, 6];
                                    reasonCodeLines = cartClientResponse.result.ReasonCodeLines.filter(function (rl) { return rl.ReasonCodeId == Global_1.Global.AskariCardInfoCode; });
                                    reasonCodeLines.forEach(function (rs) { return rs.Information = result_1.data.result.value; });
                                    saveReasonCodeLine = new CartOperations.SaveReasonCodeLinesOnCartClientRequest(reasonCodeLines);
                                    return [4 /*yield*/, this.context.runtime.executeAsync(saveReasonCodeLine)];
                                case 4: return [4 /*yield*/, (_a.sent()).data];
                                case 5:
                                    saveReasonCodeLinesOnCartClientResponse = _a.sent();
                                    _a.label = 6;
                                case 6:
                                    if (!(cartClientResponse.result.ReasonCodeLines.filter(function (rl) { return rl.ReasonCodeId == Global_1.Global.AskariCardInfoCode; }).length == 0)) return [3 /*break*/, 9];
                                    saveReasonCodeLine = new CartOperations.SaveReasonCodeLinesOnCartClientRequest([reasonCodeLine]);
                                    return [4 /*yield*/, this.context.runtime.executeAsync(saveReasonCodeLine)];
                                case 7: return [4 /*yield*/, (_a.sent()).data];
                                case 8:
                                    saveReasonCodeLinesOnCartClientResponse = _a.sent();
                                    _a.label = 9;
                                case 9:
                                    validateBinNumberRequest = new DataServiceRequests_g_1.StoreOperations.ValidateBinNumberRequest(result_1.data.result.value, cartClientResponse.result.Id);
                                    refreshRequest = new CartOperations.RefreshCartClientRequest();
                                    return [4 /*yield*/, this.context.runtime.executeAsync(refreshRequest)];
                                case 10:
                                    _a.sent();
                                    return [4 /*yield*/, this.context.runtime.executeAsync(validateBinNumberRequest)];
                                case 11: return [4 /*yield*/, (_a.sent()).data];
                                case 12:
                                    validateBinNumberResponse = _a.sent();
                                    if (validateBinNumberResponse.result) {
                                        return [2 /*return*/, Promise.resolve({ canceled: false })];
                                    }
                                    else {
                                        this.showMessage("There was an error validating card number", "Error");
                                        return [2 /*return*/, Promise.resolve({ canceled: true })];
                                    }
                                    return [3 /*break*/, 14];
                                case 13: return [2 /*return*/, Promise.resolve({ canceled: true })];
                                case 14: return [2 /*return*/, Promise.resolve({ canceled: false })];
                            }
                        });
                    });
                };
                AskariCardSalePreOperationTrigger.prototype.showMessage = function (message, title) {
                    var dialogRequest = new Dialogs_1.ShowMessageDialogClientRequest({
                        title: title,
                        message: message
                    });
                    this.context.runtime.executeAsync(dialogRequest);
                };
                AskariCardSalePreOperationTrigger.prototype.getCurrentCart = function () {
                    return __awaiter(this, void 0, void 0, function () {
                        var cartClientRequest;
                        return __generator(this, function (_a) {
                            switch (_a.label) {
                                case 0:
                                    cartClientRequest = new CartOperations.GetCurrentCartClientRequest();
                                    return [4 /*yield*/, this.context.runtime.executeAsync(cartClientRequest)];
                                case 1: return [4 /*yield*/, (_a.sent()).data];
                                case 2: return [2 /*return*/, _a.sent()];
                            }
                        });
                    });
                };
                return AskariCardSalePreOperationTrigger;
            }(Triggers.PreOperationTrigger));
            exports_1("default", AskariCardSalePreOperationTrigger);
        }
    };
});
//# sourceMappingURL=C:/RetailSDK/Update/RetailSDK_FinalV28/Pos/Extensions/AskariCardBinNumberVerification/TriggerHandlers/AskariCardSalePreOperationTrigger.js.map