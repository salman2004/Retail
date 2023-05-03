System.register(["PosApi/Create/Operations", "./RFIDCardReaderOperationRequest", "PosApi/Consume/Cart", "PosApi/Entities", "PosApi/Consume/Peripherals", "PosApi/Consume/Dialogs", "../DataService/DataServiceRequests.g", "PosApi/TypeExtensions"], function (exports_1, context_1) {
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
    var Operations_1, RFIDCardReaderOperationRequest_1, Cart_1, Entities_1, Peripherals_1, Cart_2, Dialogs_1, DataServiceRequests_g_1, TypeExtensions_1, RFIDCardReaderOperationRequestHandler;
    return {
        setters: [
            function (Operations_1_1) {
                Operations_1 = Operations_1_1;
            },
            function (RFIDCardReaderOperationRequest_1_1) {
                RFIDCardReaderOperationRequest_1 = RFIDCardReaderOperationRequest_1_1;
            },
            function (Cart_1_1) {
                Cart_1 = Cart_1_1;
                Cart_2 = Cart_1_1;
            },
            function (Entities_1_1) {
                Entities_1 = Entities_1_1;
            },
            function (Peripherals_1_1) {
                Peripherals_1 = Peripherals_1_1;
            },
            function (Dialogs_1_1) {
                Dialogs_1 = Dialogs_1_1;
            },
            function (DataServiceRequests_g_1_1) {
                DataServiceRequests_g_1 = DataServiceRequests_g_1_1;
            },
            function (TypeExtensions_1_1) {
                TypeExtensions_1 = TypeExtensions_1_1;
            }
        ],
        execute: function () {
            RFIDCardReaderOperationRequestHandler = (function (_super) {
                __extends(RFIDCardReaderOperationRequestHandler, _super);
                function RFIDCardReaderOperationRequestHandler() {
                    return _super !== null && _super.apply(this, arguments) || this;
                }
                RFIDCardReaderOperationRequestHandler.prototype.supportedRequestType = function () {
                    return RFIDCardReaderOperationRequest_1.default;
                };
                RFIDCardReaderOperationRequestHandler.prototype.executeAsync = function (request) {
                    return __awaiter(this, void 0, void 0, function () {
                        var hardwareStationDeviceActionRequest, response, stringResponse, obj, stringDate, cardLastTransactionDate, stringYear, stringMonth, stringdate, stringHour, stringMinute, cartExtensionProperty, cartExtensionPropertylastTransactionDateTime, cartExtensionPropertyCardLimit, cartExtensionPropertyCardNumber, cartExtensionPropertyCardBalance, saveExtensionProperty, loyaltyCardRequest, subTitleMsg, numericInputDialogOptions, dialogRequest, result, res, csdCardNumber, writtenCardNumber, ActivateCardRequest, response_1;
                        return __generator(this, function (_a) {
                            switch (_a.label) {
                                case 0:
                                    hardwareStationDeviceActionRequest = new Peripherals_1.HardwareStationDeviceActionRequest("RFIDCARDREADEREXTENSIONDEVICE", "GetLoyaltyCardInfo", "");
                                    return [4 /*yield*/, this.context.runtime.executeAsync(hardwareStationDeviceActionRequest)];
                                case 1: return [4 /*yield*/, (_a.sent()).data];
                                case 2:
                                    response = _a.sent();
                                    stringResponse = JSON.stringify(response.response);
                                    obj = JSON.parse(stringResponse);
                                    if (!(obj.csdCardNumber != null || obj.csdCardNumber != "")) return [3 /*break*/, 16];
                                    if (obj.isCardBlocked == true) {
                                        this.showMessage("The card attached to card reader is blocked. Please contact the concerned department.", "Error");
                                    }
                                    stringDate = String(obj.lastTransactionDateTime);
                                    cardLastTransactionDate = new Date();
                                    stringYear = stringDate.substr(0, 4);
                                    stringMonth = stringDate.substr(5, 2);
                                    stringdate = stringDate.substr(8, 2);
                                    stringHour = stringDate.substr(11, 2);
                                    stringMinute = stringDate.substr(14, 2);
                                    cardLastTransactionDate.setFullYear(Number(stringYear), Number(stringMonth) - 1, Number(stringdate));
                                    cardLastTransactionDate.setHours(Number(stringHour), Number(stringMinute));
                                    if (TypeExtensions_1.DateExtensions.isFutureDate(cardLastTransactionDate)) {
                                        this.showMessage("Last Transaction dateTime is greater than system dateTime", "Error");
                                        return [2 /*return*/, Promise.resolve({ canceled: false, data: null })];
                                    }
                                    cartExtensionProperty = new Entities_1.ProxyEntities.CommercePropertyClass();
                                    cartExtensionProperty.Key = "CDCCardReaderValue";
                                    cartExtensionProperty.Value = new Entities_1.ProxyEntities.CommercePropertyValueClass();
                                    cartExtensionProperty.Value.StringValue = stringResponse;
                                    cartExtensionPropertylastTransactionDateTime = new Entities_1.ProxyEntities.CommercePropertyClass();
                                    cartExtensionPropertylastTransactionDateTime.Key = "CSDlastTransactionDateTime";
                                    cartExtensionPropertylastTransactionDateTime.Value = new Entities_1.ProxyEntities.CommercePropertyValueClass();
                                    cartExtensionPropertylastTransactionDateTime.Value.StringValue = obj.lastTransactionDateTime;
                                    cartExtensionPropertyCardLimit = new Entities_1.ProxyEntities.CommercePropertyClass();
                                    cartExtensionPropertyCardLimit.Key = "CSDCardLimit";
                                    cartExtensionPropertyCardLimit.Value = new Entities_1.ProxyEntities.CommercePropertyValueClass();
                                    cartExtensionPropertyCardNumber = new Entities_1.ProxyEntities.CommercePropertyClass();
                                    cartExtensionPropertyCardNumber.Key = "CSDCardNumber";
                                    cartExtensionPropertyCardNumber.Value = new Entities_1.ProxyEntities.CommercePropertyValueClass();
                                    cartExtensionPropertyCardNumber.Value.StringValue = obj.csdCardNumber;
                                    cartExtensionPropertyCardBalance = new Entities_1.ProxyEntities.CommercePropertyClass();
                                    cartExtensionPropertyCardBalance.Key = "CSDCardBalance";
                                    cartExtensionPropertyCardBalance.Value = new Entities_1.ProxyEntities.CommercePropertyValueClass();
                                    if (/[^a-zA-Z]/.test(obj.csdCardNumber.toString())) {
                                        cartExtensionPropertyCardBalance.Value.StringValue = obj.balance;
                                        cartExtensionPropertyCardLimit.Value.StringValue = obj.limit;
                                    }
                                    if (!(obj.isCardActivated == true && obj.isCardBlocked == false)) return [3 /*break*/, 7];
                                    saveExtensionProperty = new Cart_1.SaveExtensionPropertiesOnCartClientRequest([cartExtensionProperty, cartExtensionPropertyCardNumber, cartExtensionPropertyCardBalance, cartExtensionPropertylastTransactionDateTime, cartExtensionPropertyCardLimit], hardwareStationDeviceActionRequest.correlationId);
                                    return [4 /*yield*/, this.context.runtime.executeAsync(saveExtensionProperty)];
                                case 3:
                                    _a.sent();
                                    return [4 /*yield*/, this.context.runtime.executeAsync(new DataServiceRequests_g_1.StoreOperations.AuthenticateCardRequest(TypeExtensions_1.StringExtensions.EMPTY, obj.csdCardNumber))];
                                case 4: return [4 /*yield*/, (_a.sent()).data];
                                case 5:
                                    _a.sent();
                                    loyaltyCardRequest = new Cart_2.AddLoyaltyCardToCartOperationRequest(this.context.logger.getNewCorrelationId(), obj.csdCardNumber);
                                    return [4 /*yield*/, this.context.runtime.executeAsync(loyaltyCardRequest)];
                                case 6:
                                    _a.sent();
                                    _a.label = 7;
                                case 7:
                                    if (!(obj.isCardActivated == false && obj.isCardBlocked == false)) return [3 /*break*/, 15];
                                    subTitleMsg = "Enter Last 6 digits of card holder CNIC number.\n\n"
                                        + "CNIC Number should not include (-).\n\n";
                                    numericInputDialogOptions = {
                                        title: "Card Activation",
                                        subTitle: subTitleMsg,
                                        numPadLabel: "Please enter cnic number:",
                                        defaultNumber: ""
                                    };
                                    dialogRequest = new Dialogs_1.ShowNumericInputDialogClientRequest(numericInputDialogOptions);
                                    return [4 /*yield*/, this.context.runtime.executeAsync(dialogRequest)];
                                case 8:
                                    result = _a.sent();
                                    if (!!result.canceled) return [3 /*break*/, 14];
                                    if (result.data.result.value.length == 0) {
                                        return [2 /*return*/, Promise.resolve({ canceled: false, data: null })];
                                    }
                                    return [4 /*yield*/, this.context.runtime.executeAsync(new DataServiceRequests_g_1.StoreOperations.AuthenticateCardRequest(result.data.result.value, obj.csdCardNumber))];
                                case 9: return [4 /*yield*/, (_a.sent()).data];
                                case 10:
                                    res = _a.sent();
                                    if (!(res.result == true)) return [3 /*break*/, 12];
                                    csdCardNumber = obj.csdCardNumber;
                                    writtenCardNumber = obj.writtenCardNumber;
                                    ActivateCardRequest = {
                                        cardInfo: stringResponse,
                                        csdCardNumber: csdCardNumber,
                                        writtenCardNumber: writtenCardNumber
                                    };
                                    return [4 /*yield*/, this.cardReaderHardwareStationRequest("RFIDCARDREADEREXTENSIONDEVICE", "ActivateCard", ActivateCardRequest)];
                                case 11:
                                    response_1 = _a.sent();
                                    if (response_1.response == true) {
                                        this.showMessage("Card has been activated.", "Card status changed");
                                    }
                                    else {
                                        this.showMessage("There was an error writing onto card.", "Error");
                                    }
                                    return [3 /*break*/, 13];
                                case 12:
                                    this.showMessage("There was an error writing to card. Please contact administrator.", "Error");
                                    _a.label = 13;
                                case 13: return [3 /*break*/, 15];
                                case 14:
                                    this.context.logger.logInformational("Card activation is canceled.");
                                    _a.label = 15;
                                case 15: return [3 /*break*/, 17];
                                case 16:
                                    this.showMessage("Card contains invalid information. Please contact concerened department.", "Error");
                                    _a.label = 17;
                                case 17: return [2 /*return*/, Promise.resolve({
                                        canceled: false,
                                        data: null
                                    })];
                            }
                        });
                    });
                };
                RFIDCardReaderOperationRequestHandler.prototype.showMessage = function (message, title) {
                    var dialogRequest = new Dialogs_1.ShowMessageDialogClientRequest({
                        title: title,
                        message: message
                    });
                    this.context.runtime.executeAsync(dialogRequest);
                };
                RFIDCardReaderOperationRequestHandler.prototype.cardReaderHardwareStationRequest = function (device, action, actionData) {
                    return __awaiter(this, void 0, void 0, function () {
                        var hardwareStationDeviceActivationRequest, response;
                        return __generator(this, function (_a) {
                            switch (_a.label) {
                                case 0:
                                    hardwareStationDeviceActivationRequest = new Peripherals_1.HardwareStationDeviceActionRequest(device, action, actionData);
                                    return [4 /*yield*/, this.context.runtime.executeAsync(hardwareStationDeviceActivationRequest)];
                                case 1: return [4 /*yield*/, (_a.sent()).data];
                                case 2:
                                    response = _a.sent();
                                    return [2 /*return*/, response];
                            }
                        });
                    });
                };
                return RFIDCardReaderOperationRequestHandler;
            }(Operations_1.ExtensionOperationRequestHandlerBase));
            exports_1("default", RFIDCardReaderOperationRequestHandler);
        }
    };
});
//# sourceMappingURL=C:/RetailSDK/Update/RetailSDK_Version28/Pos/Extensions/RFIDCardReaderExtension/Operations/RFIDCardReaderOperationRequestHandler.js.map