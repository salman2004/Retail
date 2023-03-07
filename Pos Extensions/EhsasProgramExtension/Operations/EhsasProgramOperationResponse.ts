import { Response } from "PosApi/Create/RequestHandlers";

export default class EhsasProgramOperationResponse extends Response {
    isEhsasProgramApplicable: boolean;
    ehsasProgramOfferId: string;
}