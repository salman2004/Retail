System.register(["PosApi/Extend/Triggers/PaymentTriggers", "PosApi/TypeExtensions", "PosApi/Consume/Peripherals"], function (exports_1, context_1) {
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
    var Triggers, TypeExtensions_1, Peripherals_1, CardReaderPreAddTenderLineTrigger;
    return {
        setters: [
            function (Triggers_1) {
                Triggers = Triggers_1;
            },
            function (TypeExtensions_1_1) {
                TypeExtensions_1 = TypeExtensions_1_1;
            },
            function (Peripherals_1_1) {
                Peripherals_1 = Peripherals_1_1;
            }
        ],
        execute: function () {
            CardReaderPreAddTenderLineTrigger = (function (_super) {
                __extends(CardReaderPreAddTenderLineTrigger, _super);
                function CardReaderPreAddTenderLineTrigger() {
                    return _super !== null && _super.apply(this, arguments) || this;
                }
                CardReaderPreAddTenderLineTrigger.prototype.execute = function (options) {
                    return __awaiter(this, void 0, void 0, function () {
                        var properties, tenderLineSum, SampleCommerceProperties, cardReaderProperty, obj, isCardRebate, usedPoints, shopCode, csdCardNumber, WriteCardRequest, hardwareStationDeviceActionRequest;
                        return __generator(this, function (_a) {
                            switch (_a.label) {
                                case 0:
                                    properties = options.cart.ExtensionProperties;
                                    tenderLineSum = 0;
                                    if (!TypeExtensions_1.ObjectExtensions.isNullOrUndefined(options.cart) && !TypeExtensions_1.ObjectExtensions.isNullOrUndefined(options.cart.TenderLines) && options.cart.TenderLines.length > 0) {
                                        options.cart.TenderLines.forEach(function (tl) {
                                            if (tl.VoidStatusValue != 1) {
                                                tenderLineSum = +tl.Amount;
                                            }
                                        });
                                    }
                                    if (Math.floor(tenderLineSum + options.tenderLine.Amount) < Math.floor(options.cart.AmountDue)) {
                                        return [2 /*return*/, Promise.resolve({
                                                canceled: false
                                            })];
                                    }
                                    if (TypeExtensions_1.ObjectExtensions.isNullOrUndefined(options.cart.ExtensionProperties) || options.cart.IsReturnByReceipt) {
                                        return [2 /*return*/, Promise.resolve({ canceled: false })];
                                    }
                                    SampleCommerceProperties = options.cart.ExtensionProperties.filter(function (extensionProperty) {
                                        return extensionProperty.Key === "CDCCardReaderValue";
                                    });
                                    if (SampleCommerceProperties.length <= 0) {
                                        return [2 /*return*/, Promise.resolve({ canceled: false })];
                                    }
                                    cardReaderProperty = this.getPropertyValue(properties, "CDCCardReaderValue").StringValue;
                                    if (TypeExtensions_1.ObjectExtensions.isNullOrUndefined(cardReaderProperty) && cardReaderProperty == "" && cardReaderProperty == null || options.cart.LoyaltyCardId.length == 0) {
                                        return [2 /*return*/, Promise.resolve({ canceled: false })];
                                    }
                                    obj = JSON.parse(cardReaderProperty);
                                    isCardRebate = false;
                                    usedPoints = "00000";
                                    shopCode = this.getPropertyValue(properties, "CSDstoreId").StringValue;
                                    ;
                                    if (obj.csdCardNumber != null && !TypeExtensions_1.ObjectExtensions.isNullOrUndefined(obj.csdCardNumber)) {
                                        csdCardNumber = obj.csdCardNumber.toString();
                                        if (/^[a-zA-Z]+$/.test(csdCardNumber.charAt(0))) {
                                            usedPoints = this.getPropertyValue(properties, "CSDMonthlyLimitUsed").StringValue;
                                            isCardRebate = true;
                                        }
                                    }
                                    WriteCardRequest = {
                                        shopCode: shopCode,
                                        usedPoints: usedPoints,
                                        cardInfo: cardReaderProperty,
                                        csdCardNumber: obj.csdCardNumber,
                                        writtenCardNumebr: obj.writtenCardNumebr,
                                        isRebateCard: isCardRebate
                                    };
                                    hardwareStationDeviceActionRequest = new Peripherals_1.HardwareStationDeviceActionRequest("RFIDCARDREADEREXTENSIONDEVICE", "WriteTransactionalDataOnCard", WriteCardRequest);
                                    return [4 /*yield*/, this.context.runtime.executeAsync(hardwareStationDeviceActionRequest)];
                                case 1: return [4 /*yield*/, (_a.sent()).data];
                                case 2:
                                    _a.sent();
                                    return [2 /*return*/, Promise.resolve({
                                            canceled: false,
                                            data: null
                                        })];
                            }
                        });
                    });
                };
                CardReaderPreAddTenderLineTrigger.prototype.getPropertyValue = function (extensionProperties, column) {
                    extensionProperties = extensionProperties || [];
                    return extensionProperties.filter(function (prop) { return prop.Key === column; })
                        .map(function (prop) { return prop.Value; })[0];
                };
                return CardReaderPreAddTenderLineTrigger;
            }(Triggers.PreAddTenderLineTrigger));
            exports_1("default", CardReaderPreAddTenderLineTrigger);
        }
    };
});
//# sourceMappingURL=C:/RetailSDK/Update/RetailSDK_FinalV28/Pos/Extensions/RFIDCardReaderExtension/TriggerHandlers/CardReaderPreAddTenderLineTrigger.js.map