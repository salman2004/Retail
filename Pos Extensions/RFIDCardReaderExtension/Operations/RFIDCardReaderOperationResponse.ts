import { Response } from "PosApi/Create/RequestHandlers";

/**
 * (Sample) Operation response of executing end of day operations.
 */
export default class RFIDCardReaderOperationResponse extends Response {
    csdCardNumber: string;
    firstName: string;
    lastName: string;
    writtenCardNumber: string;
    cateogry: string;
    lastTransactionDateTime: string;
    isCardActivated: boolean;
    isCardBlocked: boolean;
    lastShopCode: string;
    totalPoints: string;
    balancePoints: string;
    usedPoints: string;
}