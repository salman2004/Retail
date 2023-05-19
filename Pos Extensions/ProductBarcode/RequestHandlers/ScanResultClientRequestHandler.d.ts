import { ClientEntities } from "PosApi/Entities";
import * as requestHandler from "PosApi/Extend/RequestHandlers/ScanResultsRequestHandlers";
export default class ScanResultClientRequestHandler extends requestHandler.GetScanResultClientRequestHandler {
    executeAsync(request: Commerce.GetScanResultClientRequest<Commerce.GetScanResultClientResponse>): Promise<ClientEntities.ICancelableDataResult<Commerce.GetScanResultClientResponse>>;
}
