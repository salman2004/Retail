import { ExtensionOperationRequestBase } from "PosApi/Create/Operations";
import EhsasProgramOperationResponse from "./EhsasProgramOperationResponse";

export default class EhsasProgramOperationRequest<TResponse extends EhsasProgramOperationResponse> extends ExtensionOperationRequestBase<TResponse> {
    constructor(correlationId: string) {
        super(5002, correlationId);
    }
    
}