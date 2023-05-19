import { ClientEntities } from "PosApi/Entities";
import * as request from "PosApi/Consume/Products";
import * as requestHandler from "PosApi/Extend/RequestHandlers/ScanResultsRequestHandlers";
import { Global } from "../Global";


export default class ScanResultClientRequestHandler extends requestHandler.GetScanResultClientRequestHandler
{
    executeAsync(request: Commerce.GetScanResultClientRequest<Commerce.GetScanResultClientResponse>): Promise<ClientEntities.ICancelableDataResult<Commerce.GetScanResultClientResponse>> {
        Global.Barcode = request.scanText;
        return this.defaultExecuteAsync(request);
    }
}
    