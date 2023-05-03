import { IExtensionCommandContext } from "PosApi/Extend/Views/AppBarCommands";
import * as CustomerDetailsView from "PosApi/Extend/Views/CustomerDetailsView";
export default class AddLoyaltyCustomerCommand extends CustomerDetailsView.CustomerDetailsExtensionCommandBase {
    private _disableAddButton;
    constructor(context: IExtensionCommandContext<CustomerDetailsView.ICustomerDetailsToExtensionCommandMessageTypeMap>);
    protected init(state: CustomerDetailsView.ICustomerDetailsExtensionCommandState): void;
    protected execute(): void;
}
