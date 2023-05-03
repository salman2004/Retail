
/* tslint:disable */
import { ProxyEntities } from "PosApi/Entities";
import { Entities } from "./DataServiceEntities.g";
import { DataServiceRequest, DataServiceResponse } from "PosApi/Consume/DataService";
export { ProxyEntities };
export { Entities };

export namespace EhsasProgram {
  // Entity Set EhsasProgramEntity
  export class GetEhsasProgramVerificationResponse extends DataServiceResponse {
    public result: Entities.EhsasProgramEntity;
  }

  export class GetEhsasProgramVerificationRequest<TResponse extends GetEhsasProgramVerificationResponse> extends DataServiceRequest<TResponse> {
    /**
     * Constructor
     */
      public constructor(beneficiaryParameters: Entities.BeneficiaryInquiryRequestParameters) {
        super();

        this._entitySet = "EhsasProgram";
        this._entityType = "EhsasProgramEntity";
        this._method = "GetEhsasProgramVerification";
        this._parameters = { beneficiaryParameters: beneficiaryParameters };
        this._isAction = true;
        this._returnType = Entities.EhsasProgramEntity;
        this._isReturnTypeCollection = false;
        
      }
  }

  export class VerifyEhsasProgramOtpResponse extends DataServiceResponse {
    public result: Entities.VerifyEhsasProgramOtpResponseEntity;
  }

  export class VerifyEhsasProgramOtpRequest<TResponse extends VerifyEhsasProgramOtpResponse> extends DataServiceRequest<TResponse> {
    /**
     * Constructor
     */
      public constructor(otpRequestParameters: Entities.VerifyEhsasProgramOtpRequestParameters) {
        super();

        this._entitySet = "EhsasProgram";
        this._entityType = "EhsasProgramEntity";
        this._method = "VerifyEhsasProgramOtp";
        this._parameters = { otpRequestParameters: otpRequestParameters };
        this._isAction = true;
        this._returnType = Entities.VerifyEhsasProgramOtpResponseEntity;
        this._isReturnTypeCollection = false;
        
      }
  }

  export class ResendEhsasProgramOtpResponse extends DataServiceResponse {
    public result: Entities.ResendOtpResponseEntity;
  }

  export class ResendEhsasProgramOtpRequest<TResponse extends ResendEhsasProgramOtpResponse> extends DataServiceRequest<TResponse> {
    /**
     * Constructor
     */
      public constructor(resendOtpRequestParameters: Entities.ResendOtpRequestParameter) {
        super();

        this._entitySet = "EhsasProgram";
        this._entityType = "EhsasProgramEntity";
        this._method = "ResendEhsasProgramOtp";
        this._parameters = { resendOtpRequestParameters: resendOtpRequestParameters };
        this._isAction = true;
        this._returnType = Entities.ResendOtpResponseEntity;
        this._isReturnTypeCollection = false;
        
      }
  }

}
