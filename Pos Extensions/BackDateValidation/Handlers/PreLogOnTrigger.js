System.register(["PosApi/Consume/Device", "PosApi/Consume/Dialogs", "PosApi/Entities", "PosApi/Extend/Triggers/ApplicationTriggers", "PosApi/TypeExtensions", "../DataService/DataServiceRequests.g"], function (exports_1, context_1) {
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
    var Device_1, Dialogs_1, Entities_1, Triggers, TypeExtensions_1, DataServiceRequests_g_1, PreLogOnTrigger;
    return {
        setters: [
            function (Device_1_1) {
                Device_1 = Device_1_1;
            },
            function (Dialogs_1_1) {
                Dialogs_1 = Dialogs_1_1;
            },
            function (Entities_1_1) {
                Entities_1 = Entities_1_1;
            },
            function (Triggers_1) {
                Triggers = Triggers_1;
            },
            function (TypeExtensions_1_1) {
                TypeExtensions_1 = TypeExtensions_1_1;
            },
            function (DataServiceRequests_g_1_1) {
                DataServiceRequests_g_1 = DataServiceRequests_g_1_1;
            }
        ],
        execute: function () {
            PreLogOnTrigger = (function (_super) {
                __extends(PreLogOnTrigger, _super);
                function PreLogOnTrigger() {
                    return _super !== null && _super.apply(this, arguments) || this;
                }
                PreLogOnTrigger.prototype.execute = function (options) {
                    return __awaiter(this, void 0, void 0, function () {
                        var localDeviceDateTime, statusCheckRequest, statusCheckResponse, request, response;
                        return __generator(this, function (_a) {
                            switch (_a.label) {
                                case 0:
                                    statusCheckRequest = new Device_1.GetConnectionStatusClientRequest();
                                    return [4 /*yield*/, this.context.runtime.executeAsync(statusCheckRequest)];
                                case 1:
                                    statusCheckResponse = (_a.sent()).data;
                                    if (statusCheckResponse.result == Entities_1.ClientEntities.ConnectionStatusType.Online) {
                                        localDeviceDateTime = TypeExtensions_1.DateExtensions.now.toLocaleString("en-US").replace(",", "");
                                    }
                                    else {
                                        localDeviceDateTime = TypeExtensions_1.DateExtensions.now.toLocaleString("en-PK").replace(",", "");
                                    }
                                    request = new DataServiceRequests_g_1.StoreOperations.ValidateTimeRequest(localDeviceDateTime);
                                    return [4 /*yield*/, this.context.runtime.executeAsync(request)];
                                case 2: return [4 /*yield*/, (_a.sent()).data];
                                case 3:
                                    response = _a.sent();
                                    if (response.result == true) {
                                        return [2 /*return*/, Promise.resolve({ canceled: false })];
                                    }
                                    else {
                                        this.showMessage("System datetime is not correct", "DateTime Validation");
                                        return [2 /*return*/, Promise.resolve({ canceled: true })];
                                    }
                                    return [2 /*return*/];
                            }
                        });
                    });
                };
                PreLogOnTrigger.prototype.showMessage = function (message, title) {
                    var dialogRequest = new Dialogs_1.ShowMessageDialogClientRequest({
                        title: title,
                        message: message
                    });
                    this.context.runtime.executeAsync(dialogRequest);
                };
                return PreLogOnTrigger;
            }(Triggers.PreLogOnTrigger));
            exports_1("default", PreLogOnTrigger);
        }
    };
});
//# sourceMappingURL=C:/RetailSDK/Update/RetailSDK_FinalV28/Pos/Extensions/BackDateValidation/Handlers/PreLogOnTrigger.js.map