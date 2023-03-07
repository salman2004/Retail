import RFIDCardReaderOperationResponse from "./RFIDCardReaderOperationResponse";
import RFIDCardReaderOperationRequest from "./RFIDCardReaderOperationRequest";
import { ExtensionOperationRequestFactoryFunctionType, IOperationContext } from "PosApi/Create/Operations";
import { ClientEntities } from "PosApi/Entities";

let getOperationRequest: ExtensionOperationRequestFactoryFunctionType<RFIDCardReaderOperationResponse> =
    /**
     * Gets an instance of EndOfDayOperationRequest.
     * @param {number} operationId The operation Id.
     * @param {string[]} actionParameters The action parameters.
     * @param {string} correlationId A telemetry correlation ID, used to group events logged from this request together with the calling context.
     * @return {EndOfDayOperationRequest<TResponse>} Instance of EndOfDayOperationRequest.
     */
    function (
        context: IOperationContext,
        operationId: number,
        actionParameters: string[],
        correlationId: string
    ): Promise<ClientEntities.ICancelableDataResult<RFIDCardReaderOperationRequest<RFIDCardReaderOperationResponse>>> {
        let operationRequest: RFIDCardReaderOperationRequest<RFIDCardReaderOperationResponse> = new RFIDCardReaderOperationRequest<RFIDCardReaderOperationResponse>(correlationId);
        return Promise.resolve(<ClientEntities.ICancelableDataResult<RFIDCardReaderOperationRequest<RFIDCardReaderOperationResponse>>>{
            canceled: false,
            data: operationRequest
        });
    };
export default getOperationRequest;