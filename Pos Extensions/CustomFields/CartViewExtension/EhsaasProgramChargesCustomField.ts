import { CartViewTotalsPanelCustomFieldBase } from "PosApi/Extend/Views/CartView";
import { ProxyEntities, ClientEntities } from "PosApi/Entities";

export default class EhsaasProgramChargesCustomField extends CartViewTotalsPanelCustomFieldBase {
    public computeValue(cart: ProxyEntities.Cart): string {
        
        if (cart != null && cart.ChargeLines != null && cart.ChargeLines.filter(chargecode => chargecode.ChargeCode.toLowerCase().indexOf("ehsaas") >= 0).length > 0) {
            return cart.ChargeLines.filter(chargecode => chargecode.ChargeCode.toLowerCase().indexOf("ehsaas") >= 0)[0].CalculatedAmount.toString();
        }
        else {
            return "0.00"
        }
    }
}