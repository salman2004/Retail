System.register(["PosApi/Extend/Views/CartView"], function (exports_1, context_1) {
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
    var CartView_1, EhsaasProgramChargesCustomField;
    return {
        setters: [
            function (CartView_1_1) {
                CartView_1 = CartView_1_1;
            }
        ],
        execute: function () {
            EhsaasProgramChargesCustomField = (function (_super) {
                __extends(EhsaasProgramChargesCustomField, _super);
                function EhsaasProgramChargesCustomField() {
                    return _super !== null && _super.apply(this, arguments) || this;
                }
                EhsaasProgramChargesCustomField.prototype.computeValue = function (cart) {
                    if (cart != null && cart.ChargeLines != null && cart.ChargeLines.filter(function (chargecode) { return chargecode.ChargeCode.toLowerCase().indexOf("ehsaas") >= 0; }).length > 0) {
                        return cart.ChargeLines.filter(function (chargecode) { return chargecode.ChargeCode.toLowerCase().indexOf("ehsaas") >= 0; })[0].CalculatedAmount.toString();
                    }
                    else {
                        return "0.00";
                    }
                };
                return EhsaasProgramChargesCustomField;
            }(CartView_1.CartViewTotalsPanelCustomFieldBase));
            exports_1("default", EhsaasProgramChargesCustomField);
        }
    };
});
//# sourceMappingURL=C:/RetailSDK/Update/RetailSDK_Version28/Pos/Extensions/CustomFields/CartViewExtension/EhsaasProgramChargesCustomField.js.map