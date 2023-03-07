import { ProxyEntities } from "PosApi/Entities";
import { Entities } from "./DataServiceEntities.g";
import { DataServiceRequest, DataServiceResponse } from "PosApi/Consume/DataService";
export { ProxyEntities };
export { Entities };
export declare namespace EhsasProgram {
    class GetEhsasProgramVerificationResponse extends DataServiceResponse {
        result: Entities.EhsasProgramEntity;
    }
    class GetEhsasProgramVerificationRequest<TResponse extends GetEhsasProgramVerificationResponse> extends DataServiceRequest<TResponse> {
        constructor(beneficiaryParameters: Entities.BeneficiaryInquiryRequestParameters);
    }
    class VerifyEhsasProgramOtpResponse extends DataServiceResponse {
        result: Entities.VerifyEhsasProgramOtpResponseEntity;
    }
    class VerifyEhsasProgramOtpRequest<TResponse extends VerifyEhsasProgramOtpResponse> extends DataServiceRequest<TResponse> {
        constructor(otpRequestParameters: Entities.VerifyEhsasProgramOtpRequestParameters);
    }
    class ResendEhsasProgramOtpResponse extends DataServiceResponse {
        result: Entities.ResendOtpResponseEntity;
    }
    class ResendEhsasProgramOtpRequest<TResponse extends ResendEhsasProgramOtpResponse> extends DataServiceRequest<TResponse> {
        constructor(resendOtpRequestParameters: Entities.ResendOtpRequestParameter);
    }
}
