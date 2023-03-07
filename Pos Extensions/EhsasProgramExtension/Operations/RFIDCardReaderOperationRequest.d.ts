import { ExtensionOperationRequestBase } from "PosApi/Create/Operations";
import EndOfDayOperationResponse from "./RFIDCardReaderOperationResponse";
export default class EndOfDayOperationRequest<TResponse extends EndOfDayOperationResponse> extends ExtensionOperationRequestBase<TResponse> {
    constructor(correlationId: string);
}
