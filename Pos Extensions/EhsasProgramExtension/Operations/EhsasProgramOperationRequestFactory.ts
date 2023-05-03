import { ExtensionOperationRequestFactoryFunctionType, IOperationContext } from "PosApi/Create/Operations";
import { ClientEntities } from "PosApi/Entities";
import EhsasProgramOperationResponse from "./EhsasProgramOperationResponse";
import EhsasProgramOperationRequest from "./EhsasProgramOperationRequest";

let getOperationRequest: ExtensionOperationRequestFactoryFunctionType<EhsasProgramOperationResponse> =
    
    function (
        context: IOperationContext,
        operationId: number,
        actionParameters: string[],
        correlationId: string
    ): Promise<ClientEntities.ICancelableDataResult<EhsasProgramOperationRequest<EhsasProgramOperationResponse>>> {
        let operationRequest: EhsasProgramOperationRequest<EhsasProgramOperationResponse> = new EhsasProgramOperationRequest<EhsasProgramOperationResponse>(correlationId);
        return Promise.resolve(<ClientEntities.ICancelableDataResult<EhsasProgramOperationRequest<EhsasProgramOperationResponse>>>{
            canceled: false,
            data: operationRequest
        });
    };
export default getOperationRequest;