import { ExtensionOperationRequestType, ExtensionOperationRequestHandlerBase } from "PosApi/Create/Operations";
import { AddAffiliationOperationRequest, AddAffiliationOperationResponse, GetCurrentCartClientRequest, SaveExtensionPropertiesOnCartClientRequest, SaveExtensionPropertiesOnCartClientResponse } from "PosApi/Consume/Cart";
import { ClientEntities, ProxyEntities } from "PosApi/Entities";
import { EhsasProgram, Entities } from "../DataService/DataServiceRequests.g";
import { IAlphanumericInputDialogOptions, IAlphanumericInputDialogResult, IMessageDialogOptions, INumericInputDialogOptions, INumericInputDialogResult, ShowAlphanumericInputDialogClientRequest, ShowAlphanumericInputDialogClientResponse, ShowAlphanumericInputDialogError, ShowMessageDialogClientRequest, ShowMessageDialogClientResponse, ShowNumericInputDialogClientRequest, ShowNumericInputDialogClientResponse, ShowNumericInputDialogError } from "PosApi/Consume/Dialogs";
import { GetConnectionStatusClientRequest, GetConnectionStatusClientResponse } from "PosApi/Consume/Device";
import EhsasProgramOperationResponse from "./EhsasProgramOperationResponse";
import EhsasProgramOperationRequest from "./EhsasProgramOperationRequest";
import * as CartOperations from "PosApi/Consume/Cart";
import { ProductSearchExtensionCommandBase } from "PosApi/Extend/Views/SearchView";
import { GetProductsByIdsClientRequest, GetProductsByIdsClientResponse } from "PosApi/Consume/Products";
import * as Dialogs from "PosApi/Create/Dialogs";
import { IExtensionContext } from "PosApi/Framework/ExtensionContext";
import { ObjectExtensions } from "PosApi/TypeExtensions";

export default class EhsasProgramOperationRequestHandler<TResponse extends EhsasProgramOperationResponse> extends ExtensionOperationRequestHandlerBase<TResponse> {
    SUCCESS :string = "Success";

    supportedRequestType(): ExtensionOperationRequestType < TResponse > {
        return EhsasProgramOperationRequest;
    }

    async executeAsync(request: Commerce.OperationRequest<TResponse>): Promise<ClientEntities.ICancelableDataResult<TResponse>> {
        
        let statusCheckRequest: GetConnectionStatusClientRequest<GetConnectionStatusClientResponse> =
            new GetConnectionStatusClientRequest<GetConnectionStatusClientResponse>();
        let statusCheckResponse: GetConnectionStatusClientResponse = (await this.context.runtime.executeAsync(statusCheckRequest)).data;

        if (statusCheckResponse.result != ClientEntities.ConnectionStatusType.Online)
        {
            this.showMessage("Ehsas program doesnot support offline", "Ehsas Program");
            
            return Promise.resolve({
                canceled: true,
                data: null
            });
        }

        let subTitleMsg: string = "Plese enter national identification number.\n\n"
            + "CNIC Number should not include (-).\n\n"
        let title: string = "Ehsas Program";
        let numPadLabel: string = "National identification number:";

        let result: ClientEntities.ICancelableDataResult<ShowNumericInputDialogClientResponse> = await this.GetInputFromNumericInputDialog(subTitleMsg, title, numPadLabel);

        if (!result.canceled) {
            let productIds: Array<number> = [];
            let product: Entities.Product;
            let products: Array<Entities.Product> = [];

            let cnicNumber: string = result.data.result.value;
            let cartClientRequest: CartOperations.GetCurrentCartClientRequest<CartOperations.GetCurrentCartClientResponse> = new CartOperations.GetCurrentCartClientRequest<CartOperations.GetCurrentCartClientResponse>();
            let cartClientResponse: CartOperations.GetCurrentCartClientResponse = await (await this.context.runtime.executeAsync<CartOperations.GetCurrentCartClientResponse>(cartClientRequest)).data;

             

            cartClientResponse.result.CartLines.forEach((cartLine) =>
            {
                product = new Entities.Product();
                product.Amount = cartLine.Price,
                product.InventDimId= cartLine.InventoryDimensionId,
                product.ItemId= cartLine.ItemId,
                product.Quantity = cartLine.Quantity,
                product.ProductId = cartLine.ProductId
                
                products.push(product);
            });

            let parameters: Entities.BeneficiaryInquiryRequestParameters = new Entities.BeneficiaryInquiryRequestParameters();
            parameters.CNICNumber = cnicNumber;
            parameters.Products = products;
            parameters.currentTransactionId = cartClientResponse.result.Id;

            let ehsasProgramRequest: EhsasProgram.GetEhsasProgramVerificationRequest<EhsasProgram.GetEhsasProgramVerificationResponse> = new EhsasProgram.GetEhsasProgramVerificationRequest<EhsasProgram.GetEhsasProgramVerificationResponse>(parameters);
            let ehsasProgramResponse: EhsasProgram.GetEhsasProgramVerificationResponse = (await this.context.runtime.executeAsync<EhsasProgram.GetEhsasProgramVerificationResponse>(ehsasProgramRequest)).data;

            if (ehsasProgramResponse.result.IsEhsasProgramAllowed) {

                let subsidyInquiryResInfo: Entities.SubsidyInquiryResTxnInfo = ehsasProgramResponse.result.SubsidyInquiryResponse.subsidyInquiryResTxnInfo;
                let otp: string = await this.getOtpResult(subsidyInquiryResInfo.authId, subsidyInquiryResInfo.cnic, ehsasProgramResponse.result.AuthToken, cartClientResponse.result.Id);
                if (otp != "")
                {
                    let verifyOtpParameters: Entities.VerifyEhsasProgramOtpRequestParameters = new Entities.VerifyEhsasProgramOtpRequestParameters();
                    verifyOtpParameters.OTP = otp;
                    verifyOtpParameters.AuthToken = ehsasProgramResponse.result.AuthToken;
                    verifyOtpParameters.SubsidyInquiryResponse = ehsasProgramResponse.result.SubsidyInquiryResponse;
                    verifyOtpParameters.currentTransactionId = cartClientResponse.result.Id;

                    let verifyOtp: EhsasProgram.VerifyEhsasProgramOtpRequest<EhsasProgram.VerifyEhsasProgramOtpResponse> = new EhsasProgram.VerifyEhsasProgramOtpRequest<EhsasProgram.VerifyEhsasProgramOtpResponse>(verifyOtpParameters);
                    let verifyOtpResponse: EhsasProgram.VerifyEhsasProgramOtpResponse = await (await this.context.runtime.executeAsync<EhsasProgram.VerifyEhsasProgramOtpResponse>(verifyOtp)).data;
                    
                }
                
            }
        }
        return Promise.resolve({
            canceled: false,
            data: null
        });
    }

    private async getOtpResult(authId: string, cnic: string, authToken: string, currentTransactionId : string ): Promise<string>
    {
        //Send Otp For Verification
        let otpDialogResult: ClientEntities.ICancelableDataResult<ShowNumericInputDialogClientResponse> = (await this.GetInputFromAlphaNumericDialog(this.context, "Ehsas Program", "Enter verification code.\n\n", "One time password"));
        if (otpDialogResult.canceled)
        {
            return this.resendOtp(authId, cnic, authToken, currentTransactionId);
        }
        else {
           return await otpDialogResult.data.result.value;
        }
    }
    
    private async resendOtp(authId: string, cnic: string, authToken: string, currentTransactionId: string): Promise<string>
    {
        let resendOtpDialogResult: ClientEntities.ICancelableDataResult<ShowMessageDialogClientResponse> = await this.showMessageBox(this.context, "Didn't recieve otp", "Ehsas Program", "Did'nt recevice otp. Click Resend OTP or cancel ehsas applicable discount");
        if (resendOtpDialogResult.data.result.dialogResult != "CancelResult")
        {
            let resendOtpRequestParameter = new Entities.ResendOtpRequestParameter();
            resendOtpRequestParameter.AuthId = authId;
            resendOtpRequestParameter.Cnic = cnic;
            resendOtpRequestParameter.AuthToken = authToken;
            resendOtpRequestParameter.MerchantId = "";
            resendOtpRequestParameter.currentTransactionId = currentTransactionId;
            

            let resendOtprequest: EhsasProgram.ResendEhsasProgramOtpRequest<EhsasProgram.ResendEhsasProgramOtpResponse>
                = new EhsasProgram.ResendEhsasProgramOtpRequest<EhsasProgram.ResendEhsasProgramOtpResponse>(resendOtpRequestParameter);
            let resendOtpResponse = await (await this.context.runtime.executeAsync<EhsasProgram.ResendEhsasProgramOtpResponse>(resendOtprequest)).data;
            return this.getOtpResult(authId, cnic, authToken, currentTransactionId );
        }
        return "";
    }

    private showMessage(message: string, title: string): void {
        let dialogRequest: ShowMessageDialogClientRequest<ShowMessageDialogClientResponse> =
            new ShowMessageDialogClientRequest<ShowMessageDialogClientResponse>(<IMessageDialogOptions>{
                title: title,
                message: message
            });
        this.context.runtime.executeAsync(dialogRequest);
    }

    private async GetInputFromNumericInputDialog(subTitleMsg: string, titleMsg: string, numPadLabel: string): Promise<ClientEntities.ICancelableDataResult<ShowNumericInputDialogClientResponse>>
    {   
        let numericInputDialogOptions: INumericInputDialogOptions = {
            title: titleMsg,
            subTitle: subTitleMsg,
            numPadLabel: numPadLabel,
            defaultNumber: ""
        };

        let dialogRequest: ShowNumericInputDialogClientRequest<ShowNumericInputDialogClientResponse> =
            new ShowNumericInputDialogClientRequest<ShowNumericInputDialogClientResponse>(numericInputDialogOptions);
        return await (await this.context.runtime.executeAsync(dialogRequest));
    }

    public async GetInputFromAlphaNumericDialog(context: IExtensionContext, title: string, subTitleMsg: string, numPadLabel: string): Promise<ClientEntities.ICancelableDataResult<ShowAlphanumericInputDialogClientResponse>> {

        let alphanumericInputDialogOptions: IAlphanumericInputDialogOptions = {
            title: title,
            subTitle: subTitleMsg,
            numPadLabel: numPadLabel,
            defaultValue: ""
            //onBeforeClose: this.onBeforeClose.bind(this)
        };

        let dialogRequest: ShowAlphanumericInputDialogClientRequest<ShowAlphanumericInputDialogClientResponse> =
            new ShowAlphanumericInputDialogClientRequest<ShowAlphanumericInputDialogClientResponse>(alphanumericInputDialogOptions);
        return await context.runtime.executeAsync<ShowAlphanumericInputDialogClientResponse>(dialogRequest);
    }
    
    public async showMessageBox(context: IExtensionContext, subTitle: string, title: string, message: string): Promise<ClientEntities.ICancelableDataResult<ShowMessageDialogClientResponse>> {

        let messageDialogOptions: IMessageDialogOptions = {
            title: title,
            subTitle: subTitle,
            message: message,
            showCloseX: true,
            button1: {
                id: "RESENDOTP",
                label: "Resend OTP",
                result: "RESENDOTP"
            },
            button2: {
                id: "Cancel",
                label: "Cancel",
                result: "CancelResult"
            }
        };

        let dialogRequest: ShowMessageDialogClientRequest<ShowMessageDialogClientResponse> =
            new ShowMessageDialogClientRequest<ShowMessageDialogClientResponse>(messageDialogOptions);

        return await context.runtime.executeAsync<ShowMessageDialogClientResponse>(dialogRequest);
    }

    
}

