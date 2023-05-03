
  /* tslint:disable */
  import { ProxyEntities } from "PosApi/Entities";
  // @ts-ignore
  import { DateExtensions } from "PosApi/TypeExtensions";
  export { ProxyEntities };

  export namespace Entities {
  
  /**
   * EhsasProgramEntity entity class.
   */
  export class EhsasProgramEntity {
      public IsEhsasProgramAllowed: boolean;
      public SubsidyInquiryResponse: Entities.SubsidyInquiryResponse;
      public AuthToken: string;
      public ExtensionProperties: ProxyEntities.CommerceProperty[];
      
      // Navigation properties names
      
      /**
       * Construct an object from odata response.
       * @param {any} odataObject The odata result object.
       */
      constructor(odataObject?: any) {
      odataObject = odataObject || {};
            this.IsEhsasProgramAllowed = odataObject.IsEhsasProgramAllowed;
              
        if (odataObject.SubsidyInquiryResponse == null) {
        this.SubsidyInquiryResponse = undefined;
        } else if (odataObject.SubsidyInquiryResponse['@odata.type'] == null) {
        this.SubsidyInquiryResponse = new Entities.SubsidyInquiryResponse(odataObject.SubsidyInquiryResponse);
        } else {
        var className: string = odataObject.SubsidyInquiryResponse['@odata.type'];
        className = className.substr(className.lastIndexOf('.') + 1).concat("Class");
        // @ts-ignore
        this.SubsidyInquiryResponse = new ProxyEntities[className](odataObject.SubsidyInquiryResponse)
        }

      
            this.AuthToken = odataObject.AuthToken;
              
        this.ExtensionProperties = undefined;
        if (odataObject.ExtensionProperties) {
        this.ExtensionProperties = [];
        for (var i = 0; i < odataObject.ExtensionProperties.length; i++) {
        if (odataObject.ExtensionProperties[i] != null) {
        if (odataObject.ExtensionProperties[i]['@odata.type'] != null) {
        var className: string = odataObject.ExtensionProperties[i]['@odata.type'];
        className = className.substr(className.lastIndexOf('.') + 1).concat("Class");
        // @ts-ignore
        this.ExtensionProperties[i] = new ProxyEntities[className](odataObject.ExtensionProperties[i])
        } else {
        this.ExtensionProperties[i] = new ProxyEntities.CommercePropertyClass(odataObject.ExtensionProperties[i]);
        }
                    } else {
        this.ExtensionProperties[i] = undefined;
        }
        }
        }
      
      }
  }

  /**
   * BeneficiaryInquiryRequestParameters entity class.
   */
  export class BeneficiaryInquiryRequestParameters {
      public CNICNumber: string;
      public Products: Entities.Product[];
      public currentTransactionId: string;
      
      // Navigation properties names
      
      /**
       * Construct an object from odata response.
       * @param {any} odataObject The odata result object.
       */
      constructor(odataObject?: any) {
      odataObject = odataObject || {};
            this.CNICNumber = odataObject.CNICNumber;
              
        this.Products = undefined;
        if (odataObject.Products) {
        this.Products = [];
        for (var i = 0; i < odataObject.Products.length; i++) {
        if (odataObject.Products[i] != null) {
        if (odataObject.Products[i]['@odata.type'] != null) {
        var className: string = odataObject.Products[i]['@odata.type'];
        className = className.substr(className.lastIndexOf('.') + 1).concat("Class");
        // @ts-ignore
        this.Products[i] = new ProxyEntities[className](odataObject.Products[i])
        } else {
        this.Products[i] = new Entities.Product(odataObject.Products[i]);
        }
                    } else {
        this.Products[i] = undefined;
        }
        }
        }
      
            this.currentTransactionId = odataObject.currentTransactionId;
              
      }
  }

  /**
   * VerifyEhsasProgramOtpRequestParameters entity class.
   */
  export class VerifyEhsasProgramOtpRequestParameters {
      public OTP: string;
      public SubsidyInquiryResponse: Entities.SubsidyInquiryResponse;
      public AuthToken: string;
      public currentTransactionId: string;
      
      // Navigation properties names
      
      /**
       * Construct an object from odata response.
       * @param {any} odataObject The odata result object.
       */
      constructor(odataObject?: any) {
      odataObject = odataObject || {};
            this.OTP = odataObject.OTP;
              
        if (odataObject.SubsidyInquiryResponse == null) {
        this.SubsidyInquiryResponse = undefined;
        } else if (odataObject.SubsidyInquiryResponse['@odata.type'] == null) {
        this.SubsidyInquiryResponse = new Entities.SubsidyInquiryResponse(odataObject.SubsidyInquiryResponse);
        } else {
        var className: string = odataObject.SubsidyInquiryResponse['@odata.type'];
        className = className.substr(className.lastIndexOf('.') + 1).concat("Class");
        // @ts-ignore
        this.SubsidyInquiryResponse = new ProxyEntities[className](odataObject.SubsidyInquiryResponse)
        }

      
            this.AuthToken = odataObject.AuthToken;
              
            this.currentTransactionId = odataObject.currentTransactionId;
              
      }
  }

  /**
   * VerifyEhsasProgramOtpResponseEntity entity class.
   */
  export class VerifyEhsasProgramOtpResponseEntity {
      public SubsidyPaymentResponse: Entities.SubsidyPaymentResponse;
      public AuthToken: string;
      public ExtensionProperties: ProxyEntities.CommerceProperty[];
      
      // Navigation properties names
      
      /**
       * Construct an object from odata response.
       * @param {any} odataObject The odata result object.
       */
      constructor(odataObject?: any) {
      odataObject = odataObject || {};
        if (odataObject.SubsidyPaymentResponse == null) {
        this.SubsidyPaymentResponse = undefined;
        } else if (odataObject.SubsidyPaymentResponse['@odata.type'] == null) {
        this.SubsidyPaymentResponse = new Entities.SubsidyPaymentResponse(odataObject.SubsidyPaymentResponse);
        } else {
        var className: string = odataObject.SubsidyPaymentResponse['@odata.type'];
        className = className.substr(className.lastIndexOf('.') + 1).concat("Class");
        // @ts-ignore
        this.SubsidyPaymentResponse = new ProxyEntities[className](odataObject.SubsidyPaymentResponse)
        }

      
            this.AuthToken = odataObject.AuthToken;
              
        this.ExtensionProperties = undefined;
        if (odataObject.ExtensionProperties) {
        this.ExtensionProperties = [];
        for (var i = 0; i < odataObject.ExtensionProperties.length; i++) {
        if (odataObject.ExtensionProperties[i] != null) {
        if (odataObject.ExtensionProperties[i]['@odata.type'] != null) {
        var className: string = odataObject.ExtensionProperties[i]['@odata.type'];
        className = className.substr(className.lastIndexOf('.') + 1).concat("Class");
        // @ts-ignore
        this.ExtensionProperties[i] = new ProxyEntities[className](odataObject.ExtensionProperties[i])
        } else {
        this.ExtensionProperties[i] = new ProxyEntities.CommercePropertyClass(odataObject.ExtensionProperties[i]);
        }
                    } else {
        this.ExtensionProperties[i] = undefined;
        }
        }
        }
      
      }
  }

  /**
   * SubsidyInquiryResponse entity class.
   */
  export class SubsidyInquiryResponse {
      public subsidyCommodityResTxnInfo: Entities.SubsidyCommodityResTxnInfo[];
      public subsidyInquiryResTxnInfo: Entities.SubsidyInquiryResTxnInfo;
      public info: Entities.ResponseInfo;
      
      // Navigation properties names
      
      /**
       * Construct an object from odata response.
       * @param {any} odataObject The odata result object.
       */
      constructor(odataObject?: any) {
      odataObject = odataObject || {};
        this.subsidyCommodityResTxnInfo = undefined;
        if (odataObject.subsidyCommodityResTxnInfo) {
        this.subsidyCommodityResTxnInfo = [];
        for (var i = 0; i < odataObject.subsidyCommodityResTxnInfo.length; i++) {
        if (odataObject.subsidyCommodityResTxnInfo[i] != null) {
        if (odataObject.subsidyCommodityResTxnInfo[i]['@odata.type'] != null) {
        var className: string = odataObject.subsidyCommodityResTxnInfo[i]['@odata.type'];
        className = className.substr(className.lastIndexOf('.') + 1).concat("Class");
        // @ts-ignore
        this.subsidyCommodityResTxnInfo[i] = new ProxyEntities[className](odataObject.subsidyCommodityResTxnInfo[i])
        } else {
        this.subsidyCommodityResTxnInfo[i] = new Entities.SubsidyCommodityResTxnInfo(odataObject.subsidyCommodityResTxnInfo[i]);
        }
                    } else {
        this.subsidyCommodityResTxnInfo[i] = undefined;
        }
        }
        }
      
        if (odataObject.subsidyInquiryResTxnInfo == null) {
        this.subsidyInquiryResTxnInfo = undefined;
        } else if (odataObject.subsidyInquiryResTxnInfo['@odata.type'] == null) {
        this.subsidyInquiryResTxnInfo = new Entities.SubsidyInquiryResTxnInfo(odataObject.subsidyInquiryResTxnInfo);
        } else {
        var className: string = odataObject.subsidyInquiryResTxnInfo['@odata.type'];
        className = className.substr(className.lastIndexOf('.') + 1).concat("Class");
        // @ts-ignore
        this.subsidyInquiryResTxnInfo = new ProxyEntities[className](odataObject.subsidyInquiryResTxnInfo)
        }

      
        if (odataObject.info == null) {
        this.info = undefined;
        } else if (odataObject.info['@odata.type'] == null) {
        this.info = new Entities.ResponseInfo(odataObject.info);
        } else {
        var className: string = odataObject.info['@odata.type'];
        className = className.substr(className.lastIndexOf('.') + 1).concat("Class");
        // @ts-ignore
        this.info = new ProxyEntities[className](odataObject.info)
        }

      
      }
  }

  /**
   * SubsidyCommodityResTxnInfo entity class.
   */
  export class SubsidyCommodityResTxnInfo {
      public unit: string;
      public amount: number;
      public defaultRate: number;
      public code: string;
      public netAmount: number;
      public rate: number;
      public subsidy: number;
      public qty: number;
      public name: string;
      
      // Navigation properties names
      
      /**
       * Construct an object from odata response.
       * @param {any} odataObject The odata result object.
       */
      constructor(odataObject?: any) {
      odataObject = odataObject || {};
            this.unit = odataObject.unit;
              
            this.amount = (odataObject.amount != null) ? parseFloat(odataObject.amount) : undefined;
              
            this.defaultRate = (odataObject.defaultRate != null) ? parseFloat(odataObject.defaultRate) : undefined;
              
            this.code = odataObject.code;
              
            this.netAmount = (odataObject.netAmount != null) ? parseFloat(odataObject.netAmount) : undefined;
              
            this.rate = (odataObject.rate != null) ? parseFloat(odataObject.rate) : undefined;
              
            this.subsidy = (odataObject.subsidy != null) ? parseFloat(odataObject.subsidy) : undefined;
              
            this.qty = (odataObject.qty != null) ? parseFloat(odataObject.qty) : undefined;
              
            this.name = odataObject.name;
              
      }
  }

  /**
   * SubsidyInquiryResTxnInfo entity class.
   */
  export class SubsidyInquiryResTxnInfo {
      public totalValue: number;
      public dateTime: string;
      public totalSubsidy: number;
      public netAmount: number;
      public cnic: string;
      public itemsCount: number;
      public authId: string;
      
      // Navigation properties names
      
      /**
       * Construct an object from odata response.
       * @param {any} odataObject The odata result object.
       */
      constructor(odataObject?: any) {
      odataObject = odataObject || {};
            this.totalValue = (odataObject.totalValue != null) ? parseFloat(odataObject.totalValue) : undefined;
              
            this.dateTime = odataObject.dateTime;
              
            this.totalSubsidy = (odataObject.totalSubsidy != null) ? parseFloat(odataObject.totalSubsidy) : undefined;
              
            this.netAmount = (odataObject.netAmount != null) ? parseFloat(odataObject.netAmount) : undefined;
              
            this.cnic = odataObject.cnic;
              
            this.itemsCount = (odataObject.itemsCount != null) ? parseFloat(odataObject.itemsCount) : undefined;
              
            this.authId = odataObject.authId;
              
      }
  }

  /**
   * ResponseInfo entity class.
   */
  export class ResponseInfo {
      public response_code: string;
      public response_desc: string;
      public STAN: string;
      public RRN: string;
      
      // Navigation properties names
      
      /**
       * Construct an object from odata response.
       * @param {any} odataObject The odata result object.
       */
      constructor(odataObject?: any) {
      odataObject = odataObject || {};
            this.response_code = odataObject.response_code;
              
            this.response_desc = odataObject.response_desc;
              
            this.STAN = odataObject.STAN;
              
            this.RRN = odataObject.RRN;
              
      }
  }

  /**
   * Product entity class.
   */
  export class Product {
      public ItemId: string;
      public InventDimId: string;
      public Amount: number;
      public Quantity: number;
      public ProductId: number;
      
      // Navigation properties names
      
      /**
       * Construct an object from odata response.
       * @param {any} odataObject The odata result object.
       */
      constructor(odataObject?: any) {
      odataObject = odataObject || {};
            this.ItemId = odataObject.ItemId;
              
            this.InventDimId = odataObject.InventDimId;
              
            this.Amount = (odataObject.Amount != null) ? parseFloat(odataObject.Amount) : undefined;
              
            this.Quantity = (odataObject.Quantity != null) ? parseFloat(odataObject.Quantity) : undefined;
              
            this.ProductId = (odataObject.ProductId != null) ? parseInt(odataObject.ProductId, 10) : undefined;
              
      }
  }

  /**
   * SubsidyPaymentResponse entity class.
   */
  export class SubsidyPaymentResponse {
      public subsidyCommodityResTxnInfo: Entities.SubsidyPaymentResponseSubsidyCommodityResTxnInfo[];
      public subsidyPaymentResTxnInfo: Entities.SubsidyPaymentResponseSubsidyPaymentResTxnInfo;
      public info: Entities.ResponseInfo;
      
      // Navigation properties names
      
      /**
       * Construct an object from odata response.
       * @param {any} odataObject The odata result object.
       */
      constructor(odataObject?: any) {
      odataObject = odataObject || {};
        this.subsidyCommodityResTxnInfo = undefined;
        if (odataObject.subsidyCommodityResTxnInfo) {
        this.subsidyCommodityResTxnInfo = [];
        for (var i = 0; i < odataObject.subsidyCommodityResTxnInfo.length; i++) {
        if (odataObject.subsidyCommodityResTxnInfo[i] != null) {
        if (odataObject.subsidyCommodityResTxnInfo[i]['@odata.type'] != null) {
        var className: string = odataObject.subsidyCommodityResTxnInfo[i]['@odata.type'];
        className = className.substr(className.lastIndexOf('.') + 1).concat("Class");
        // @ts-ignore
        this.subsidyCommodityResTxnInfo[i] = new ProxyEntities[className](odataObject.subsidyCommodityResTxnInfo[i])
        } else {
        this.subsidyCommodityResTxnInfo[i] = new Entities.SubsidyPaymentResponseSubsidyCommodityResTxnInfo(odataObject.subsidyCommodityResTxnInfo[i]);
        }
                    } else {
        this.subsidyCommodityResTxnInfo[i] = undefined;
        }
        }
        }
      
        if (odataObject.subsidyPaymentResTxnInfo == null) {
        this.subsidyPaymentResTxnInfo = undefined;
        } else if (odataObject.subsidyPaymentResTxnInfo['@odata.type'] == null) {
        this.subsidyPaymentResTxnInfo = new Entities.SubsidyPaymentResponseSubsidyPaymentResTxnInfo(odataObject.subsidyPaymentResTxnInfo);
        } else {
        var className: string = odataObject.subsidyPaymentResTxnInfo['@odata.type'];
        className = className.substr(className.lastIndexOf('.') + 1).concat("Class");
        // @ts-ignore
        this.subsidyPaymentResTxnInfo = new ProxyEntities[className](odataObject.subsidyPaymentResTxnInfo)
        }

      
        if (odataObject.info == null) {
        this.info = undefined;
        } else if (odataObject.info['@odata.type'] == null) {
        this.info = new Entities.ResponseInfo(odataObject.info);
        } else {
        var className: string = odataObject.info['@odata.type'];
        className = className.substr(className.lastIndexOf('.') + 1).concat("Class");
        // @ts-ignore
        this.info = new ProxyEntities[className](odataObject.info)
        }

      
      }
  }

  /**
   * SubsidyPaymentResponseSubsidyCommodityResTxnInfo entity class.
   */
  export class SubsidyPaymentResponseSubsidyCommodityResTxnInfo {
      public unit: string;
      public amount: number;
      public defaultRate: number;
      public code: string;
      public netAmount: number;
      public rate: number;
      public subsidy: number;
      public qty: number;
      public name: string;
      
      // Navigation properties names
      
      /**
       * Construct an object from odata response.
       * @param {any} odataObject The odata result object.
       */
      constructor(odataObject?: any) {
      odataObject = odataObject || {};
            this.unit = odataObject.unit;
              
            this.amount = (odataObject.amount != null) ? parseFloat(odataObject.amount) : undefined;
              
            this.defaultRate = (odataObject.defaultRate != null) ? parseFloat(odataObject.defaultRate) : undefined;
              
            this.code = odataObject.code;
              
            this.netAmount = (odataObject.netAmount != null) ? parseFloat(odataObject.netAmount) : undefined;
              
            this.rate = (odataObject.rate != null) ? parseFloat(odataObject.rate) : undefined;
              
            this.subsidy = (odataObject.subsidy != null) ? parseFloat(odataObject.subsidy) : undefined;
              
            this.qty = (odataObject.qty != null) ? parseFloat(odataObject.qty) : undefined;
              
            this.name = odataObject.name;
              
      }
  }

  /**
   * SubsidyPaymentResponseSubsidyPaymentResTxnInfo entity class.
   */
  export class SubsidyPaymentResponseSubsidyPaymentResTxnInfo {
      public totalValue: number;
      public dateTime: string;
      public totalSubsidy: number;
      public netAmount: number;
      public cnic: string;
      public billNo: string;
      public itemsCount: number;
      
      // Navigation properties names
      
      /**
       * Construct an object from odata response.
       * @param {any} odataObject The odata result object.
       */
      constructor(odataObject?: any) {
      odataObject = odataObject || {};
            this.totalValue = (odataObject.totalValue != null) ? parseFloat(odataObject.totalValue) : undefined;
              
            this.dateTime = odataObject.dateTime;
              
            this.totalSubsidy = (odataObject.totalSubsidy != null) ? parseFloat(odataObject.totalSubsidy) : undefined;
              
            this.netAmount = (odataObject.netAmount != null) ? parseFloat(odataObject.netAmount) : undefined;
              
            this.cnic = odataObject.cnic;
              
            this.billNo = odataObject.billNo;
              
            this.itemsCount = (odataObject.itemsCount != null) ? parseFloat(odataObject.itemsCount) : undefined;
              
      }
  }

  /**
   * ResendOtpRequestParameter entity class.
   */
  export class ResendOtpRequestParameter {
      public AuthToken: string;
      public AuthId: string;
      public Cnic: string;
      public MerchantId: string;
      public currentTransactionId: string;
      
      // Navigation properties names
      
      /**
       * Construct an object from odata response.
       * @param {any} odataObject The odata result object.
       */
      constructor(odataObject?: any) {
      odataObject = odataObject || {};
            this.AuthToken = odataObject.AuthToken;
              
            this.AuthId = odataObject.AuthId;
              
            this.Cnic = odataObject.Cnic;
              
            this.MerchantId = odataObject.MerchantId;
              
            this.currentTransactionId = odataObject.currentTransactionId;
              
      }
  }

  /**
   * ResendOtpResponseEntity entity class.
   */
  export class ResendOtpResponseEntity {
      public EhsasProgramResendOtpResponse: Entities.EhsasProgramResendOtpResponse;
      public AuthToken: string;
      public ExtensionProperties: ProxyEntities.CommerceProperty[];
      
      // Navigation properties names
      
      /**
       * Construct an object from odata response.
       * @param {any} odataObject The odata result object.
       */
      constructor(odataObject?: any) {
      odataObject = odataObject || {};
        if (odataObject.EhsasProgramResendOtpResponse == null) {
        this.EhsasProgramResendOtpResponse = undefined;
        } else if (odataObject.EhsasProgramResendOtpResponse['@odata.type'] == null) {
        this.EhsasProgramResendOtpResponse = new Entities.EhsasProgramResendOtpResponse(odataObject.EhsasProgramResendOtpResponse);
        } else {
        var className: string = odataObject.EhsasProgramResendOtpResponse['@odata.type'];
        className = className.substr(className.lastIndexOf('.') + 1).concat("Class");
        // @ts-ignore
        this.EhsasProgramResendOtpResponse = new ProxyEntities[className](odataObject.EhsasProgramResendOtpResponse)
        }

      
            this.AuthToken = odataObject.AuthToken;
              
        this.ExtensionProperties = undefined;
        if (odataObject.ExtensionProperties) {
        this.ExtensionProperties = [];
        for (var i = 0; i < odataObject.ExtensionProperties.length; i++) {
        if (odataObject.ExtensionProperties[i] != null) {
        if (odataObject.ExtensionProperties[i]['@odata.type'] != null) {
        var className: string = odataObject.ExtensionProperties[i]['@odata.type'];
        className = className.substr(className.lastIndexOf('.') + 1).concat("Class");
        // @ts-ignore
        this.ExtensionProperties[i] = new ProxyEntities[className](odataObject.ExtensionProperties[i])
        } else {
        this.ExtensionProperties[i] = new ProxyEntities.CommercePropertyClass(odataObject.ExtensionProperties[i]);
        }
                    } else {
        this.ExtensionProperties[i] = undefined;
        }
        }
        }
      
      }
  }

  /**
   * EhsasProgramResendOtpResponse entity class.
   */
  export class EhsasProgramResendOtpResponse {
      public info: Entities.ResponseInfo;
      
      // Navigation properties names
      
      /**
       * Construct an object from odata response.
       * @param {any} odataObject The odata result object.
       */
      constructor(odataObject?: any) {
      odataObject = odataObject || {};
        if (odataObject.info == null) {
        this.info = undefined;
        } else if (odataObject.info['@odata.type'] == null) {
        this.info = new Entities.ResponseInfo(odataObject.info);
        } else {
        var className: string = odataObject.info['@odata.type'];
        className = className.substr(className.lastIndexOf('.') + 1).concat("Class");
        // @ts-ignore
        this.info = new ProxyEntities[className](odataObject.info)
        }

      
      }
  }

}
