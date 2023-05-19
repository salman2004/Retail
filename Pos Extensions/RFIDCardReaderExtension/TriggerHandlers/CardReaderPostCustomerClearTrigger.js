System.register(["PosApi/Extend/Triggers/CustomerTriggers", "PosApi/Consume/Cart", "PosApi/Entities"], function (exports_1, context_1) {
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
    var Triggers, Cart_1, Entities_1, CardReaderPostCustomerClearTrigger;
    return {
        setters: [
            function (Triggers_1) {
                Triggers = Triggers_1;
            },
            function (Cart_1_1) {
                Cart_1 = Cart_1_1;
            },
            function (Entities_1_1) {
                Entities_1 = Entities_1_1;
            }
        ],
        execute: function () {
            CardReaderPostCustomerClearTrigger = (function (_super) {
                __extends(CardReaderPostCustomerClearTrigger, _super);
                function CardReaderPostCustomerClearTrigger() {
                    return _super !== null && _super.apply(this, arguments) || this;
                }
                CardReaderPostCustomerClearTrigger.prototype.execute = function (options) {
                    return __awaiter(this, void 0, void 0, function () {
                        var cartExtensionProperty, cartExtensionPropertyCardBalnce, cartExtensionPropertyCardNumber, saveExtensionProperty;
                        return __generator(this, function (_a) {
                            switch (_a.label) {
                                case 0:
                                    options.cart.ExtensionProperties = [];
                                    cartExtensionProperty = new Entities_1.ProxyEntities.CommercePropertyClass();
                                    cartExtensionProperty.Key = "CDCCardReaderValue";
                                    cartExtensionProperty.Value = new Entities_1.ProxyEntities.CommercePropertyValueClass();
                                    cartExtensionProperty.Value.StringValue = '';
                                    cartExtensionPropertyCardBalnce = new Entities_1.ProxyEntities.CommercePropertyClass();
                                    cartExtensionPropertyCardBalnce.Key = "CSDCardBalance";
                                    cartExtensionPropertyCardBalnce.Value = new Entities_1.ProxyEntities.CommercePropertyValueClass();
                                    cartExtensionPropertyCardBalnce.Value.StringValue = '00000';
                                    cartExtensionPropertyCardNumber = new Entities_1.ProxyEntities.CommercePropertyClass();
                                    cartExtensionPropertyCardNumber.Key = "CSDCardNumber";
                                    cartExtensionPropertyCardNumber.Value = new Entities_1.ProxyEntities.CommercePropertyValueClass();
                                    cartExtensionPropertyCardNumber.Value.StringValue = '00000';
                                    saveExtensionProperty = new Cart_1.SaveExtensionPropertiesOnCartClientRequest([cartExtensionProperty, cartExtensionPropertyCardNumber, cartExtensionPropertyCardBalnce, cartExtensionPropertyCardNumber], '');
                                    return [4 /*yield*/, this.context.runtime.executeAsync(saveExtensionProperty)];
                                case 1:
                                    _a.sent();
                                    return [2 /*return*/, Promise.resolve()];
                            }
                        });
                    });
                };
                return CardReaderPostCustomerClearTrigger;
            }(Triggers.PostCustomerClearTrigger));
            exports_1("default", CardReaderPostCustomerClearTrigger);
        }
    };
});
//# sourceMappingURL=C:/RetailSDK/Update/RetailSDK_FinalV28/Pos/Extensions/RFIDCardReaderExtension/TriggerHandlers/CardReaderPostCustomerClearTrigger.js.map