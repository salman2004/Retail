import { CartViewTotalsPanelCustomFieldBase } from "PosApi/Extend/Views/CartView";
import { ProxyEntities, ClientEntities } from "PosApi/Entities";
import { ObjectExtensions } from "PosApi/TypeExtensions";

export default class TotalChargesCustomField extends CartViewTotalsPanelCustomFieldBase {
    public computeValue(cart: ProxyEntities.Cart): string {

        let totalCharges: number = 0;
        if (cart != null && cart.ChargeLines != null && cart.ChargeLines.length > 0) {
            cart.ChargeLines.forEach((cl) => {
                totalCharges += cl.CalculatedAmount
            });
        }
        
        if (cart != null && cart.ChargeLines != null && cart.ChargeLines.length > 0 && cart.ChargeLines.filter(chargecode => chargecode.ChargeCode.toLowerCase().indexOf("ehsaas") >= 0).length > 0) {
            return (totalCharges - cart.ChargeLines.filter(chargecode => chargecode.ChargeCode.toLowerCase().indexOf("ehsaas") >= 0)[0].CalculatedAmount).toString();
        }
        else {
            return totalCharges.toString();
        }
    }
}