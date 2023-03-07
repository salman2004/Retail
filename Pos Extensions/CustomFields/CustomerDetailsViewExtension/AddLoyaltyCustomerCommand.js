System.register(["PosApi/Extend/Views/CustomerDetailsView"], function (exports_1, context_1) {
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
    var CustomerDetailsView, AddLoyaltyCustomerCommand;
    return {
        setters: [
            function (CustomerDetailsView_1) {
                CustomerDetailsView = CustomerDetailsView_1;
            }
        ],
        execute: function () {
            AddLoyaltyCustomerCommand = (function (_super) {
                __extends(AddLoyaltyCustomerCommand, _super);
                function AddLoyaltyCustomerCommand(context) {
                    var _this = _super.call(this, context) || this;
                    _this.loyaltyCardsLoadedHandler = function (data) {
                        _this._disableAddButton = false;
                        if (data != null && data.loyaltyCards != null && data.loyaltyCards.length > 0) {
                            _this._disableAddButton = true;
                        }
                    };
                    return _this;
                }
                AddLoyaltyCustomerCommand.prototype.init = function (state) {
                    if (this._disableAddButton) {
                        var element = document.getElementById("customerDetailsView_cmdAddCustomerToSale");
                        var elementEdit = document.getElementById("customerDetailsView_cmdEditCustomer");
                        element.disabled = true;
                        elementEdit.disabled = true;
                    }
                };
                AddLoyaltyCustomerCommand.prototype.execute = function () {
                };
                return AddLoyaltyCustomerCommand;
            }(CustomerDetailsView.CustomerDetailsExtensionCommandBase));
            exports_1("default", AddLoyaltyCustomerCommand);
        }
    };
});
//# sourceMappingURL=C:/RetailSDK/Update/RetailSDK_Version28/Pos/Extensions/CustomFields/CustomerDetailsViewExtension/AddLoyaltyCustomerCommand.js.map