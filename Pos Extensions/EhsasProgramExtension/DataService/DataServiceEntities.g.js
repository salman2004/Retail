System.register(["PosApi/Entities"], function (exports_1, context_1) {
    "use strict";
    var __moduleName = context_1 && context_1.id;
    var Entities_1, Entities;
    return {
        setters: [
            function (Entities_1_1) {
                Entities_1 = Entities_1_1;
            }
        ],
        execute: function () {
            exports_1("ProxyEntities", Entities_1.ProxyEntities);
            (function (Entities) {
                var EhsasProgramEntity = (function () {
                    function EhsasProgramEntity(odataObject) {
                        odataObject = odataObject || {};
                        this.IsEhsasProgramAllowed = odataObject.IsEhsasProgramAllowed;
                        if (odataObject.SubsidyInquiryResponse == null) {
                            this.SubsidyInquiryResponse = undefined;
                        }
                        else if (odataObject.SubsidyInquiryResponse['@odata.type'] == null) {
                            this.SubsidyInquiryResponse = new Entities.SubsidyInquiryResponse(odataObject.SubsidyInquiryResponse);
                        }
                        else {
                            var className = odataObject.SubsidyInquiryResponse['@odata.type'];
                            className = className.substr(className.lastIndexOf('.') + 1).concat("Class");
                            this.SubsidyInquiryResponse = new Entities_1.ProxyEntities[className](odataObject.SubsidyInquiryResponse);
                        }
                        this.AuthToken = odataObject.AuthToken;
                        this.ExtensionProperties = undefined;
                        if (odataObject.ExtensionProperties) {
                            this.ExtensionProperties = [];
                            for (var i = 0; i < odataObject.ExtensionProperties.length; i++) {
                                if (odataObject.ExtensionProperties[i] != null) {
                                    if (odataObject.ExtensionProperties[i]['@odata.type'] != null) {
                                        var className = odataObject.ExtensionProperties[i]['@odata.type'];
                                        className = className.substr(className.lastIndexOf('.') + 1).concat("Class");
                                        this.ExtensionProperties[i] = new Entities_1.ProxyEntities[className](odataObject.ExtensionProperties[i]);
                                    }
                                    else {
                                        this.ExtensionProperties[i] = new Entities_1.ProxyEntities.CommercePropertyClass(odataObject.ExtensionProperties[i]);
                                    }
                                }
                                else {
                                    this.ExtensionProperties[i] = undefined;
                                }
                            }
                        }
                    }
                    return EhsasProgramEntity;
                }());
                Entities.EhsasProgramEntity = EhsasProgramEntity;
                var BeneficiaryInquiryRequestParameters = (function () {
                    function BeneficiaryInquiryRequestParameters(odataObject) {
                        odataObject = odataObject || {};
                        this.CNICNumber = odataObject.CNICNumber;
                        this.Products = undefined;
                        if (odataObject.Products) {
                            this.Products = [];
                            for (var i = 0; i < odataObject.Products.length; i++) {
                                if (odataObject.Products[i] != null) {
                                    if (odataObject.Products[i]['@odata.type'] != null) {
                                        var className = odataObject.Products[i]['@odata.type'];
                                        className = className.substr(className.lastIndexOf('.') + 1).concat("Class");
                                        this.Products[i] = new Entities_1.ProxyEntities[className](odataObject.Products[i]);
                                    }
                                    else {
                                        this.Products[i] = new Entities.Product(odataObject.Products[i]);
                                    }
                                }
                                else {
                                    this.Products[i] = undefined;
                                }
                            }
                        }
                        this.currentTransactionId = odataObject.currentTransactionId;
                    }
                    return BeneficiaryInquiryRequestParameters;
                }());
                Entities.BeneficiaryInquiryRequestParameters = BeneficiaryInquiryRequestParameters;
                var VerifyEhsasProgramOtpRequestParameters = (function () {
                    function VerifyEhsasProgramOtpRequestParameters(odataObject) {
                        odataObject = odataObject || {};
                        this.OTP = odataObject.OTP;
                        if (odataObject.SubsidyInquiryResponse == null) {
                            this.SubsidyInquiryResponse = undefined;
                        }
                        else if (odataObject.SubsidyInquiryResponse['@odata.type'] == null) {
                            this.SubsidyInquiryResponse = new Entities.SubsidyInquiryResponse(odataObject.SubsidyInquiryResponse);
                        }
                        else {
                            var className = odataObject.SubsidyInquiryResponse['@odata.type'];
                            className = className.substr(className.lastIndexOf('.') + 1).concat("Class");
                            this.SubsidyInquiryResponse = new Entities_1.ProxyEntities[className](odataObject.SubsidyInquiryResponse);
                        }
                        this.AuthToken = odataObject.AuthToken;
                        this.currentTransactionId = odataObject.currentTransactionId;
                    }
                    return VerifyEhsasProgramOtpRequestParameters;
                }());
                Entities.VerifyEhsasProgramOtpRequestParameters = VerifyEhsasProgramOtpRequestParameters;
                var VerifyEhsasProgramOtpResponseEntity = (function () {
                    function VerifyEhsasProgramOtpResponseEntity(odataObject) {
                        odataObject = odataObject || {};
                        if (odataObject.SubsidyPaymentResponse == null) {
                            this.SubsidyPaymentResponse = undefined;
                        }
                        else if (odataObject.SubsidyPaymentResponse['@odata.type'] == null) {
                            this.SubsidyPaymentResponse = new Entities.SubsidyPaymentResponse(odataObject.SubsidyPaymentResponse);
                        }
                        else {
                            var className = odataObject.SubsidyPaymentResponse['@odata.type'];
                            className = className.substr(className.lastIndexOf('.') + 1).concat("Class");
                            this.SubsidyPaymentResponse = new Entities_1.ProxyEntities[className](odataObject.SubsidyPaymentResponse);
                        }
                        this.AuthToken = odataObject.AuthToken;
                        this.ExtensionProperties = undefined;
                        if (odataObject.ExtensionProperties) {
                            this.ExtensionProperties = [];
                            for (var i = 0; i < odataObject.ExtensionProperties.length; i++) {
                                if (odataObject.ExtensionProperties[i] != null) {
                                    if (odataObject.ExtensionProperties[i]['@odata.type'] != null) {
                                        var className = odataObject.ExtensionProperties[i]['@odata.type'];
                                        className = className.substr(className.lastIndexOf('.') + 1).concat("Class");
                                        this.ExtensionProperties[i] = new Entities_1.ProxyEntities[className](odataObject.ExtensionProperties[i]);
                                    }
                                    else {
                                        this.ExtensionProperties[i] = new Entities_1.ProxyEntities.CommercePropertyClass(odataObject.ExtensionProperties[i]);
                                    }
                                }
                                else {
                                    this.ExtensionProperties[i] = undefined;
                                }
                            }
                        }
                    }
                    return VerifyEhsasProgramOtpResponseEntity;
                }());
                Entities.VerifyEhsasProgramOtpResponseEntity = VerifyEhsasProgramOtpResponseEntity;
                var SubsidyInquiryResponse = (function () {
                    function SubsidyInquiryResponse(odataObject) {
                        odataObject = odataObject || {};
                        this.subsidyCommodityResTxnInfo = undefined;
                        if (odataObject.subsidyCommodityResTxnInfo) {
                            this.subsidyCommodityResTxnInfo = [];
                            for (var i = 0; i < odataObject.subsidyCommodityResTxnInfo.length; i++) {
                                if (odataObject.subsidyCommodityResTxnInfo[i] != null) {
                                    if (odataObject.subsidyCommodityResTxnInfo[i]['@odata.type'] != null) {
                                        var className = odataObject.subsidyCommodityResTxnInfo[i]['@odata.type'];
                                        className = className.substr(className.lastIndexOf('.') + 1).concat("Class");
                                        this.subsidyCommodityResTxnInfo[i] = new Entities_1.ProxyEntities[className](odataObject.subsidyCommodityResTxnInfo[i]);
                                    }
                                    else {
                                        this.subsidyCommodityResTxnInfo[i] = new Entities.SubsidyCommodityResTxnInfo(odataObject.subsidyCommodityResTxnInfo[i]);
                                    }
                                }
                                else {
                                    this.subsidyCommodityResTxnInfo[i] = undefined;
                                }
                            }
                        }
                        if (odataObject.subsidyInquiryResTxnInfo == null) {
                            this.subsidyInquiryResTxnInfo = undefined;
                        }
                        else if (odataObject.subsidyInquiryResTxnInfo['@odata.type'] == null) {
                            this.subsidyInquiryResTxnInfo = new Entities.SubsidyInquiryResTxnInfo(odataObject.subsidyInquiryResTxnInfo);
                        }
                        else {
                            var className = odataObject.subsidyInquiryResTxnInfo['@odata.type'];
                            className = className.substr(className.lastIndexOf('.') + 1).concat("Class");
                            this.subsidyInquiryResTxnInfo = new Entities_1.ProxyEntities[className](odataObject.subsidyInquiryResTxnInfo);
                        }
                        if (odataObject.info == null) {
                            this.info = undefined;
                        }
                        else if (odataObject.info['@odata.type'] == null) {
                            this.info = new Entities.ResponseInfo(odataObject.info);
                        }
                        else {
                            var className = odataObject.info['@odata.type'];
                            className = className.substr(className.lastIndexOf('.') + 1).concat("Class");
                            this.info = new Entities_1.ProxyEntities[className](odataObject.info);
                        }
                    }
                    return SubsidyInquiryResponse;
                }());
                Entities.SubsidyInquiryResponse = SubsidyInquiryResponse;
                var SubsidyCommodityResTxnInfo = (function () {
                    function SubsidyCommodityResTxnInfo(odataObject) {
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
                    return SubsidyCommodityResTxnInfo;
                }());
                Entities.SubsidyCommodityResTxnInfo = SubsidyCommodityResTxnInfo;
                var SubsidyInquiryResTxnInfo = (function () {
                    function SubsidyInquiryResTxnInfo(odataObject) {
                        odataObject = odataObject || {};
                        this.totalValue = (odataObject.totalValue != null) ? parseFloat(odataObject.totalValue) : undefined;
                        this.dateTime = odataObject.dateTime;
                        this.totalSubsidy = (odataObject.totalSubsidy != null) ? parseFloat(odataObject.totalSubsidy) : undefined;
                        this.netAmount = (odataObject.netAmount != null) ? parseFloat(odataObject.netAmount) : undefined;
                        this.cnic = odataObject.cnic;
                        this.itemsCount = (odataObject.itemsCount != null) ? parseFloat(odataObject.itemsCount) : undefined;
                        this.authId = odataObject.authId;
                    }
                    return SubsidyInquiryResTxnInfo;
                }());
                Entities.SubsidyInquiryResTxnInfo = SubsidyInquiryResTxnInfo;
                var ResponseInfo = (function () {
                    function ResponseInfo(odataObject) {
                        odataObject = odataObject || {};
                        this.response_code = odataObject.response_code;
                        this.response_desc = odataObject.response_desc;
                        this.STAN = odataObject.STAN;
                        this.RRN = odataObject.RRN;
                    }
                    return ResponseInfo;
                }());
                Entities.ResponseInfo = ResponseInfo;
                var Product = (function () {
                    function Product(odataObject) {
                        odataObject = odataObject || {};
                        this.ItemId = odataObject.ItemId;
                        this.InventDimId = odataObject.InventDimId;
                        this.Amount = (odataObject.Amount != null) ? parseFloat(odataObject.Amount) : undefined;
                        this.Quantity = (odataObject.Quantity != null) ? parseFloat(odataObject.Quantity) : undefined;
                        this.ProductId = (odataObject.ProductId != null) ? parseInt(odataObject.ProductId, 10) : undefined;
                    }
                    return Product;
                }());
                Entities.Product = Product;
                var SubsidyPaymentResponse = (function () {
                    function SubsidyPaymentResponse(odataObject) {
                        odataObject = odataObject || {};
                        this.subsidyCommodityResTxnInfo = undefined;
                        if (odataObject.subsidyCommodityResTxnInfo) {
                            this.subsidyCommodityResTxnInfo = [];
                            for (var i = 0; i < odataObject.subsidyCommodityResTxnInfo.length; i++) {
                                if (odataObject.subsidyCommodityResTxnInfo[i] != null) {
                                    if (odataObject.subsidyCommodityResTxnInfo[i]['@odata.type'] != null) {
                                        var className = odataObject.subsidyCommodityResTxnInfo[i]['@odata.type'];
                                        className = className.substr(className.lastIndexOf('.') + 1).concat("Class");
                                        this.subsidyCommodityResTxnInfo[i] = new Entities_1.ProxyEntities[className](odataObject.subsidyCommodityResTxnInfo[i]);
                                    }
                                    else {
                                        this.subsidyCommodityResTxnInfo[i] = new Entities.SubsidyPaymentResponseSubsidyCommodityResTxnInfo(odataObject.subsidyCommodityResTxnInfo[i]);
                                    }
                                }
                                else {
                                    this.subsidyCommodityResTxnInfo[i] = undefined;
                                }
                            }
                        }
                        if (odataObject.subsidyPaymentResTxnInfo == null) {
                            this.subsidyPaymentResTxnInfo = undefined;
                        }
                        else if (odataObject.subsidyPaymentResTxnInfo['@odata.type'] == null) {
                            this.subsidyPaymentResTxnInfo = new Entities.SubsidyPaymentResponseSubsidyPaymentResTxnInfo(odataObject.subsidyPaymentResTxnInfo);
                        }
                        else {
                            var className = odataObject.subsidyPaymentResTxnInfo['@odata.type'];
                            className = className.substr(className.lastIndexOf('.') + 1).concat("Class");
                            this.subsidyPaymentResTxnInfo = new Entities_1.ProxyEntities[className](odataObject.subsidyPaymentResTxnInfo);
                        }
                        if (odataObject.info == null) {
                            this.info = undefined;
                        }
                        else if (odataObject.info['@odata.type'] == null) {
                            this.info = new Entities.ResponseInfo(odataObject.info);
                        }
                        else {
                            var className = odataObject.info['@odata.type'];
                            className = className.substr(className.lastIndexOf('.') + 1).concat("Class");
                            this.info = new Entities_1.ProxyEntities[className](odataObject.info);
                        }
                    }
                    return SubsidyPaymentResponse;
                }());
                Entities.SubsidyPaymentResponse = SubsidyPaymentResponse;
                var SubsidyPaymentResponseSubsidyCommodityResTxnInfo = (function () {
                    function SubsidyPaymentResponseSubsidyCommodityResTxnInfo(odataObject) {
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
                    return SubsidyPaymentResponseSubsidyCommodityResTxnInfo;
                }());
                Entities.SubsidyPaymentResponseSubsidyCommodityResTxnInfo = SubsidyPaymentResponseSubsidyCommodityResTxnInfo;
                var SubsidyPaymentResponseSubsidyPaymentResTxnInfo = (function () {
                    function SubsidyPaymentResponseSubsidyPaymentResTxnInfo(odataObject) {
                        odataObject = odataObject || {};
                        this.totalValue = (odataObject.totalValue != null) ? parseFloat(odataObject.totalValue) : undefined;
                        this.dateTime = odataObject.dateTime;
                        this.totalSubsidy = (odataObject.totalSubsidy != null) ? parseFloat(odataObject.totalSubsidy) : undefined;
                        this.netAmount = (odataObject.netAmount != null) ? parseFloat(odataObject.netAmount) : undefined;
                        this.cnic = odataObject.cnic;
                        this.billNo = odataObject.billNo;
                        this.itemsCount = (odataObject.itemsCount != null) ? parseFloat(odataObject.itemsCount) : undefined;
                    }
                    return SubsidyPaymentResponseSubsidyPaymentResTxnInfo;
                }());
                Entities.SubsidyPaymentResponseSubsidyPaymentResTxnInfo = SubsidyPaymentResponseSubsidyPaymentResTxnInfo;
                var ResendOtpRequestParameter = (function () {
                    function ResendOtpRequestParameter(odataObject) {
                        odataObject = odataObject || {};
                        this.AuthToken = odataObject.AuthToken;
                        this.AuthId = odataObject.AuthId;
                        this.Cnic = odataObject.Cnic;
                        this.MerchantId = odataObject.MerchantId;
                        this.currentTransactionId = odataObject.currentTransactionId;
                    }
                    return ResendOtpRequestParameter;
                }());
                Entities.ResendOtpRequestParameter = ResendOtpRequestParameter;
                var ResendOtpResponseEntity = (function () {
                    function ResendOtpResponseEntity(odataObject) {
                        odataObject = odataObject || {};
                        if (odataObject.EhsasProgramResendOtpResponse == null) {
                            this.EhsasProgramResendOtpResponse = undefined;
                        }
                        else if (odataObject.EhsasProgramResendOtpResponse['@odata.type'] == null) {
                            this.EhsasProgramResendOtpResponse = new Entities.EhsasProgramResendOtpResponse(odataObject.EhsasProgramResendOtpResponse);
                        }
                        else {
                            var className = odataObject.EhsasProgramResendOtpResponse['@odata.type'];
                            className = className.substr(className.lastIndexOf('.') + 1).concat("Class");
                            this.EhsasProgramResendOtpResponse = new Entities_1.ProxyEntities[className](odataObject.EhsasProgramResendOtpResponse);
                        }
                        this.AuthToken = odataObject.AuthToken;
                        this.ExtensionProperties = undefined;
                        if (odataObject.ExtensionProperties) {
                            this.ExtensionProperties = [];
                            for (var i = 0; i < odataObject.ExtensionProperties.length; i++) {
                                if (odataObject.ExtensionProperties[i] != null) {
                                    if (odataObject.ExtensionProperties[i]['@odata.type'] != null) {
                                        var className = odataObject.ExtensionProperties[i]['@odata.type'];
                                        className = className.substr(className.lastIndexOf('.') + 1).concat("Class");
                                        this.ExtensionProperties[i] = new Entities_1.ProxyEntities[className](odataObject.ExtensionProperties[i]);
                                    }
                                    else {
                                        this.ExtensionProperties[i] = new Entities_1.ProxyEntities.CommercePropertyClass(odataObject.ExtensionProperties[i]);
                                    }
                                }
                                else {
                                    this.ExtensionProperties[i] = undefined;
                                }
                            }
                        }
                    }
                    return ResendOtpResponseEntity;
                }());
                Entities.ResendOtpResponseEntity = ResendOtpResponseEntity;
                var EhsasProgramResendOtpResponse = (function () {
                    function EhsasProgramResendOtpResponse(odataObject) {
                        odataObject = odataObject || {};
                        if (odataObject.info == null) {
                            this.info = undefined;
                        }
                        else if (odataObject.info['@odata.type'] == null) {
                            this.info = new Entities.ResponseInfo(odataObject.info);
                        }
                        else {
                            var className = odataObject.info['@odata.type'];
                            className = className.substr(className.lastIndexOf('.') + 1).concat("Class");
                            this.info = new Entities_1.ProxyEntities[className](odataObject.info);
                        }
                    }
                    return EhsasProgramResendOtpResponse;
                }());
                Entities.EhsasProgramResendOtpResponse = EhsasProgramResendOtpResponse;
            })(Entities || (Entities = {}));
            exports_1("Entities", Entities);
        }
    };
});
//# sourceMappingURL=C:/RetailSDK/Update/RetailSDK_Version28/Pos/Extensions/EhsasProgramExtension/DataService/DataServiceEntities.g.js.map