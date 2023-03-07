import { ClientEntities, ProxyEntities } from "PosApi/Entities";
import { IExtensionCommandContext } from "PosApi/Extend/Views/AppBarCommands";
import * as CustomerDetailsView from "PosApi/Extend/Views/CustomerDetailsView";
import { ArrayExtensions } from "PosApi/TypeExtensions";

export default class AddLoyaltyCustomerCommand extends CustomerDetailsView.CustomerDetailsExtensionCommandBase {

    private _disableAddButton: boolean;
    /**
     * Creates a new instance of the CustomerCrossLoyaltyCommand class.
     * @param {IExtensionCommandContext<CustomerDetailsView.ICustomerDetailsToExtensionCommandMessageTypeMap>} context The command context.
     * @remarks The command context contains APIs through which a command can communicate with POS.
     */
    constructor(context: IExtensionCommandContext<CustomerDetailsView.ICustomerDetailsToExtensionCommandMessageTypeMap>) {
        super(context);

        this.loyaltyCardsLoadedHandler = (data: CustomerDetailsView.CustomerDetailsLoyaltyCardsLoadedData): void => {
            this._disableAddButton = false;
            if (data != null && data.loyaltyCards != null && data.loyaltyCards.length > 0) {
                this._disableAddButton = true;
            }
        };
    }

    /**
     * Initializes the command.
     * @param {CustomerDetailsView.ICustomerDetailsExtensionCommandState} state The state used to initialize the command.
     */
    protected init(state: CustomerDetailsView.ICustomerDetailsExtensionCommandState): void {
        if (this._disableAddButton) {
            var element = <HTMLInputElement>document.getElementById("customerDetailsView_cmdAddCustomerToSale");
            var elementEdit = <HTMLInputElement>document.getElementById("customerDetailsView_cmdEditCustomer");
            element.disabled = true;
            elementEdit.disabled = true;
        }
    }

    /**
     * Executes the command.
     */
    protected execute(): void {
        
    }
}