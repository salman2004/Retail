import { ExtensionOperationRequestType, ExtensionOperationRequestHandlerBase } from "PosApi/Create/Operations";
import RFIDCardReaderOperationResponse from "./RFIDCardReaderOperationResponse";
import RFIDCardReaderOperationRequest from "./RFIDCardReaderOperationRequest";
import { ClientEntities } from "PosApi/Entities";
export default class RFIDCardReaderOperationRequestHandler<TResponse extends RFIDCardReaderOperationResponse> extends ExtensionOperationRequestHandlerBase<TResponse> {
    supportedRequestType(): ExtensionOperationRequestType<TResponse>;
    executeAsync(request: RFIDCardReaderOperationRequest<TResponse>): Promise<ClientEntities.ICancelableDataResult<TResponse>>;
    private showMessage(message, title);
    private cardReaderHardwareStationRequest(device, action, actionData);
}
