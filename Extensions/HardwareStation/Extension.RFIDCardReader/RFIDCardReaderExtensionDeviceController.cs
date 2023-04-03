using CDC.Commerce.HardwareStation.RFIDCardReader.Model;
using Microsoft.Dynamics.Commerce.HardwareStation;
using Microsoft.Dynamics.Commerce.Runtime.Hosting.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CDC.Commerce.HardwareStation.RFIDCardReader
{    
    [RoutePrefix("RFIDCARDREADEREXTENSIONDEVICE")]
    public class RFIDCardReaderExtensionDeviceController : IController
    {
        /// <param name="request">Custom request.<param>
        /// <returns>Result of card reader response.</returns>
        [HttpPost]
        public async Task<CardReaderResponse> GetLoyaltyCardInfo(CustomRequest request, IEndpointContext context)
        {          
            CardReaderResponse response;
            CardReader cardReader = new CardReader();
            try
            {
                response = cardReader.ReadCard();
            }
            catch (Exception ex)
            {
                //CardReader.cardHelper.mifareReader.mfHalt();
                throw ex;
            }
            return await Task.FromResult(response);
        }

        /// <param name="request">Custom request.<param>
        /// <returns>Result of card reader response.</returns>
        [HttpPost]
        public async Task<bool> ActivateCard(ActivateCardRequest request, IEndpointContext context)
        {
            CardReader cardReader = new CardReader();
            CardReaderResponse response = cardReader.ReadCard();
            
            if (string.IsNullOrEmpty(response.csdCardNumber))
            {
                throw new PeripheralException("Microsoft_Dynamics_Commerce_HardwareStation_RFID_Card_Error", "There was an error fetching card. Card is removed. Please place the card again on the card reader and try again.");
            }
            
            if (response.csdCardNumber == request.csdCardNumber)
            {
                if (char.IsLetter(request.csdCardNumber[0]))
                {
                    RebateCardWriter cardWriter = new RebateCardWriter();
                    cardWriter.InitializeCard();
                    RebateCardReaderResponse cardReaderResponse = JsonConvert.DeserializeObject<RebateCardReaderResponse>(request.cardInfo);
                    cardReaderResponse.isCardActivated = true;
                    if (cardReaderResponse.lastTransactionDateTime.Month < DateTime.Now.Month || cardReaderResponse.lastTransactionDateTime.Year < DateTime.Now.Year)
                    {
                        cardReaderResponse.balance = cardReaderResponse.limit;
                    }
                    cardWriter.WritePersnalInfo(cardReaderResponse);
                    cardWriter.WriteReabteInfo(cardReaderResponse);
                    cardWriter.CloseConnection();

                }
                if (!char.IsLetter(request.csdCardNumber[0]))
                {
                    LoyaltyCardWriter cardWriter = new LoyaltyCardWriter();
                    cardWriter.InitializeCard();
                    LoyaltyCardReaderResponse cardReaderResponse = JsonConvert.DeserializeObject<LoyaltyCardReaderResponse>(request.cardInfo);
                    cardReaderResponse.isCardActivated = true;
                    //reset card balance
                    if (cardReaderResponse.lastTransactionDateTime.Month < DateTime.Now.Month || cardReaderResponse.lastTransactionDateTime.Year < DateTime.Now.Year)
                    {
                        cardReaderResponse.usedPoints = "00000";
                        cardReaderResponse.balancePoints = cardReaderResponse.totalPoints;
                    }
                    cardWriter.WritePersnalInfo(cardReaderResponse);
                    cardWriter.WriteLoyaltyInfo(cardReaderResponse);
                    cardWriter.CloseConnection();
                }
            }
            else
            {
                
                throw new PeripheralException("Microsoft_Dynamics_Commerce_HardwareStation_RFID_Card_Error", "Card is changed. Please place the same card that was placed when activating the card.");
            }
           
            return await Task.FromResult(true);
        }

        [HttpPost]
        public async Task<bool> WriteTransactionalDataOnCard(WriteCardRequest request, IEndpointContext context)
        {
            CardReader cardReader = new CardReader();
            CardReaderResponse response = cardReader.ReadCard();
            if (string.IsNullOrEmpty(response.csdCardNumber))
            {
                throw new PeripheralException("Microsoft_Dynamics_Commerce_HardwareStation_RFID_Card_Error", "There was an error fetching card. Card is removed. Please place the card again on the card reader and try again.");
            }

            if (response.csdCardNumber == request.csdCardNumber)
            {
                if (char.IsLetter(request.csdCardNumber[0]))
                {
                    RebateCardWriter cardWriter = new RebateCardWriter();
                    cardWriter.InitializeCard();
                    RebateCardReaderResponse cardReaderResponse = JsonConvert.DeserializeObject<RebateCardReaderResponse>(request.cardInfo);
                    cardReaderResponse.balance = Convert.ToString(request.usedPoints);
                    cardWriter.WritePersnalInfo(cardReaderResponse);
                    cardWriter.WriteReabteInfo(cardReaderResponse);
                    cardWriter.CloseConnection();
                    cardWriter.mifareReader.mfHalt();
                }
                else
                {
                    LoyaltyCardWriter cardWriter = new LoyaltyCardWriter();
                    cardWriter.InitializeCard();
                    LoyaltyCardReaderResponse cardReaderResponse = JsonConvert.DeserializeObject<LoyaltyCardReaderResponse>(request.cardInfo);
                    cardReaderResponse.lastShopCode = request.shopCode;                                     
                    cardWriter.WritePersnalInfo(cardReaderResponse);
                    cardWriter.WriteLoyaltyInfo(cardReaderResponse);
                    cardWriter.CloseConnection();
                    cardWriter.mifareReader.mfHalt();
                }
            }
            else
            {
                cardReader.cardHelper.CloseConnection();
                throw new PeripheralException("Microsoft_Dynamics_Commerce_HardwareStation_RFID_Card_Error", "The card selected is incorrect. Please place the previous card");
            }
            cardReader.cardHelper.mifareReader.mfHalt();
            
            return await Task.FromResult(true);
        }

        [HttpPost]
        public async Task<bool> checkCard(CustomRequest request, IEndpointContext context)
        {
            if (string.IsNullOrEmpty(request.Message))
            {
                throw new PeripheralException("Microsoft_Dynamics_Commerce_HardwareStation_RFID_Card_Error", "Provided card id is not correct");
            }           
            try
            {
                CardReader cardReader = new CardReader();
                CardReaderResponse response = cardReader.ReadCard();
                if (response.writtenCardNumber == request.Message)
                {
                    return await Task.FromResult(true);
                }
                else
                {
                    throw new PeripheralException("Microsoft_Dynamics_Commerce_HardwareStation_RFID_Card_Error", "Card number doesnot match");
                }
            }
            catch (Exception ex)
            {
                throw new PeripheralException("Microsoft_Dynamics_Commerce_HardwareStation_RFID_Card_Error", ex.Message);
            }
        }
    }    
    
}
