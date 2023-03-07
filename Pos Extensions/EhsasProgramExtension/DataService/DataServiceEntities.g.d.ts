import { ProxyEntities } from "PosApi/Entities";
export { ProxyEntities };
export declare namespace Entities {
    class EhsasProgramEntity {
        IsEhsasProgramAllowed: boolean;
        SubsidyInquiryResponse: Entities.SubsidyInquiryResponse;
        AuthToken: string;
        ExtensionProperties: ProxyEntities.CommerceProperty[];
        constructor(odataObject?: any);
    }
    class BeneficiaryInquiryRequestParameters {
        CNICNumber: string;
        Products: Entities.Product[];
        currentTransactionId: string;
        constructor(odataObject?: any);
    }
    class VerifyEhsasProgramOtpRequestParameters {
        OTP: string;
        SubsidyInquiryResponse: Entities.SubsidyInquiryResponse;
        AuthToken: string;
        currentTransactionId: string;
        constructor(odataObject?: any);
    }
    class VerifyEhsasProgramOtpResponseEntity {
        SubsidyPaymentResponse: Entities.SubsidyPaymentResponse;
        AuthToken: string;
        ExtensionProperties: ProxyEntities.CommerceProperty[];
        constructor(odataObject?: any);
    }
    class SubsidyInquiryResponse {
        subsidyCommodityResTxnInfo: Entities.SubsidyCommodityResTxnInfo[];
        subsidyInquiryResTxnInfo: Entities.SubsidyInquiryResTxnInfo;
        info: Entities.ResponseInfo;
        constructor(odataObject?: any);
    }
    class SubsidyCommodityResTxnInfo {
        unit: string;
        amount: number;
        defaultRate: number;
        code: string;
        netAmount: number;
        rate: number;
        subsidy: number;
        qty: number;
        name: string;
        constructor(odataObject?: any);
    }
    class SubsidyInquiryResTxnInfo {
        totalValue: number;
        dateTime: string;
        totalSubsidy: number;
        netAmount: number;
        cnic: string;
        itemsCount: number;
        authId: string;
        constructor(odataObject?: any);
    }
    class ResponseInfo {
        response_code: string;
        response_desc: string;
        STAN: string;
        RRN: string;
        constructor(odataObject?: any);
    }
    class Product {
        ItemId: string;
        InventDimId: string;
        Amount: number;
        Quantity: number;
        ProductId: number;
        constructor(odataObject?: any);
    }
    class SubsidyPaymentResponse {
        subsidyCommodityResTxnInfo: Entities.SubsidyPaymentResponseSubsidyCommodityResTxnInfo[];
        subsidyPaymentResTxnInfo: Entities.SubsidyPaymentResponseSubsidyPaymentResTxnInfo;
        info: Entities.ResponseInfo;
        constructor(odataObject?: any);
    }
    class SubsidyPaymentResponseSubsidyCommodityResTxnInfo {
        unit: string;
        amount: number;
        defaultRate: number;
        code: string;
        netAmount: number;
        rate: number;
        subsidy: number;
        qty: number;
        name: string;
        constructor(odataObject?: any);
    }
    class SubsidyPaymentResponseSubsidyPaymentResTxnInfo {
        totalValue: number;
        dateTime: string;
        totalSubsidy: number;
        netAmount: number;
        cnic: string;
        billNo: string;
        itemsCount: number;
        constructor(odataObject?: any);
    }
    class ResendOtpRequestParameter {
        AuthToken: string;
        AuthId: string;
        Cnic: string;
        MerchantId: string;
        currentTransactionId: string;
        constructor(odataObject?: any);
    }
    class ResendOtpResponseEntity {
        EhsasProgramResendOtpResponse: Entities.EhsasProgramResendOtpResponse;
        AuthToken: string;
        ExtensionProperties: ProxyEntities.CommerceProperty[];
        constructor(odataObject?: any);
    }
    class EhsasProgramResendOtpResponse {
        info: Entities.ResponseInfo;
        constructor(odataObject?: any);
    }
}
