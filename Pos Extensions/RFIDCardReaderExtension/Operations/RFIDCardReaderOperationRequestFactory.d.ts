import RFIDCardReaderOperationResponse from "./RFIDCardReaderOperationResponse";
import { ExtensionOperationRequestFactoryFunctionType } from "PosApi/Create/Operations";
declare let getOperationRequest: ExtensionOperationRequestFactoryFunctionType<RFIDCardReaderOperationResponse>;
export default getOperationRequest;
