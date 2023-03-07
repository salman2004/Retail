import { ExtensionOperationRequestType, ExtensionOperationRequestHandlerBase } from "PosApi/Create/Operations";
import { ClientEntities } from "PosApi/Entities";
import { ShowAlphanumericInputDialogClientResponse, ShowMessageDialogClientResponse } from "PosApi/Consume/Dialogs";
import EhsasProgramOperationResponse from "./EhsasProgramOperationResponse";
import { IExtensionContext } from "PosApi/Framework/ExtensionContext";
export default class EhsasProgramOperationRequestHandler<TResponse extends EhsasProgramOperationResponse> extends ExtensionOperationRequestHandlerBase<TResponse> {
    SUCCESS: string;
    supportedRequestType(): ExtensionOperationRequestType<TResponse>;
    executeAsync(request: Commerce.OperationRequest<TResponse>): Promise<ClientEntities.ICancelableDataResult<TResponse>>;
    private getOtpResult(authId, cnic, authToken, currentTransactionId);
    private resendOtp(authId, cnic, authToken, currentTransactionId);
    private showMessage(message, title);
    private GetInputFromNumericInputDialog(subTitleMsg, titleMsg, numPadLabel);
    GetInputFromAlphaNumericDialog(context: IExtensionContext, title: string, subTitleMsg: string, numPadLabel: string): Promise<ClientEntities.ICancelableDataResult<ShowAlphanumericInputDialogClientResponse>>;
    showMessageBox(context: IExtensionContext, subTitle: string, title: string, message: string): Promise<ClientEntities.ICancelableDataResult<ShowMessageDialogClientResponse>>;
}
