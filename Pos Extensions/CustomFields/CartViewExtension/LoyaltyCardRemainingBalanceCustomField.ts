import { CartViewTotalsPanelCustomFieldBase } from "PosApi/Extend/Views/CartView";
import { ProxyEntities } from "PosApi/Entities";

export default class LoyaltyCardRemainingBalanceCustomField extends CartViewTotalsPanelCustomFieldBase {
    public computeValue(cart: ProxyEntities.Cart): string {

        if (cart != null && cart.ExtensionProperties != null && cart.LoyaltyCardId != null && cart.LoyaltyCardId != "" && cart.ExtensionProperties.filter(ep => ep.Key == "CSDCardBalance").length > 0) {
            return cart.ExtensionProperties.filter(ep => ep.Key == "CSDCardBalance")[0].Value.StringValue;
        }
        else {
            return "0.00"
        }
    }
}