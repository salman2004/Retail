import { CartViewTotalsPanelCustomFieldBase } from "PosApi/Extend/Views/CartView";
import { ProxyEntities } from "PosApi/Entities";
export default class LoyaltyCardRemainingBalanceCustomField extends CartViewTotalsPanelCustomFieldBase {
    computeValue(cart: ProxyEntities.Cart): string;
}
