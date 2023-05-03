import { ExtensionOperationRequestBase } from "PosApi/Create/Operations";
import EndOfDayOperationResponse from "./RFIDCardReaderOperationResponse";

/**
 * (Sample) Operation request for executing end of day operations.
 */
export default class EndOfDayOperationRequest<TResponse extends EndOfDayOperationResponse> extends ExtensionOperationRequestBase<TResponse> {
    constructor(correlationId: string) {
        super(5001, correlationId);
    }
    
}