using CDC.Commerce.HardwareStation.RFIDCardReader.Model;
using Microsoft.Dynamics.Commerce.HardwareStation;
using System;
using System.Threading;

namespace CDC.Commerce.HardwareStation.RFIDCardReader
{
    public class CardReader
    {
        public CardHelper cardHelper { get; set; }

        public CardReader()
        {
            Chilkat.Global glob = new Chilkat.Global();
            glob.UnlockBundle("YASOOB.CB1022024_TNcAqC5qnUpg");
        }

        public void CloseConnection()
        {
            this.cardHelper.mifareReader.PortOpen = false;
            this.cardHelper.mifareReader.GNetCancel();
            this.cardHelper.mifareReader.mfRequest();
        }
        public CardReaderResponse ReadCard()
        {
            cardHelper = new CardHelper();            
            cardHelper.InitializeCard();
            if (cardHelper.cardId != "0" && cardHelper.cardId != string.Empty)
            {
                string persnalInfo = cardHelper.GetPersnalInfo(GetPersonalPublicKey());
                string loyaltyInfo = cardHelper.GetCardInfo(GetLoyalityPublicKey());

                if (persnalInfo != null || loyaltyInfo != null)
                {
                    this.CloseConnection();
                    if (char.IsLetter(persnalInfo[0]))
                    {
                        return PrepareRebateCardInfo(persnalInfo, loyaltyInfo);
                    }
                    else
                    {
                        return PrepareLoyaltyCardInfo(persnalInfo, loyaltyInfo);
                    }
                }
                else
                {
                    throw new PeripheralException("Microsoft_Dynamics_Commerce_HardwareStation_RFID_Card_Error", "The information parsed from the card is invalid. Do remember to insert the card inside the card reader jacket. ");
                }
            }
            else
            {
                this.CloseConnection();
                throw new PeripheralException("Microsoft_Dynamics_Commerce_HardwareStation_RFID_Card_Error", "Unable to detect card. ");
            }                      
        }
        public static LoyaltyCardReaderResponse PrepareLoyaltyCardInfo(string persnalInfo, string loyaltyCardInfo)
        {
            string tmpTime = string.Empty;
            LoyaltyCardReaderResponse cardReaderResponse = new LoyaltyCardReaderResponse();

            try
            {
                cardReaderResponse.csdCardNumber = persnalInfo.Substring(0, 10).TrimEnd();
                cardReaderResponse.firstName = persnalInfo.Substring(10, 16).TrimEnd();
                cardReaderResponse.lastName = persnalInfo.Substring(26, 16).TrimEnd();
                cardReaderResponse.writtenCardNumber = persnalInfo.Substring(42, 16).TrimEnd();
                cardReaderResponse.cateogry = persnalInfo.Substring(58, 3).TrimEnd();

                tmpTime = loyaltyCardInfo.Substring(0, 16);
                cardReaderResponse.lastTransactionDateTime = DateTime.Parse(tmpTime);
                cardReaderResponse.isCardActivated = CardHelper.ConvertStringToBoolean(loyaltyCardInfo.Substring(16, 1));
                cardReaderResponse.isCardBlocked = CardHelper.ConvertStringToBoolean(loyaltyCardInfo.Substring(17, 1).TrimEnd());
                cardReaderResponse.lastShopCode = loyaltyCardInfo.Substring(18, 3);
                cardReaderResponse.totalPoints = loyaltyCardInfo.Substring(21, 5);
                cardReaderResponse.balancePoints = loyaltyCardInfo.Substring(26, 5);
                cardReaderResponse.usedPoints = loyaltyCardInfo.Substring(31, 5);
            }
            catch (Exception ex)
            {
                throw new PeripheralException("Microsoft_Dynamics_Commerce_HardwareStation_RFID_Card_Error", "Please do not lift the card when reading from card. " + ex.Message, ex);
            }
            return cardReaderResponse;
        }

        public static RebateCardReaderResponse PrepareRebateCardInfo(string persnalInfo, string rebateCardInfo)
        {
            string tmpTime = string.Empty;
            RebateCardReaderResponse cardReaderResponse = new RebateCardReaderResponse();

            try
            {
                cardReaderResponse.csdCardNumber = persnalInfo.Substring(0, 10).TrimEnd();
                cardReaderResponse.firstName = persnalInfo.Substring(10, 16).TrimEnd();
                cardReaderResponse.lastName = persnalInfo.Substring(26, 16).TrimEnd();
                cardReaderResponse.rank = persnalInfo.Substring(42, 16).TrimEnd();
                cardReaderResponse.writtenCardNumber = persnalInfo.Substring(58, 16).TrimEnd();
                cardReaderResponse.limit = persnalInfo.Substring(74, 5).TrimEnd();

                cardReaderResponse.balance = rebateCardInfo.Substring(0, 5).TrimEnd();
                tmpTime = rebateCardInfo.Substring(5, 16);
                cardReaderResponse.lastTransactionDateTime = DateTime.Parse(tmpTime);
                //if (cardReaderResponse.lastTransactionDateTime.Month < DateTime.Today.Month )
                //{
                //    cardReaderResponse.balance = cardReaderResponse.limit;
                //}
                cardReaderResponse.isCardActivated = CardHelper.ConvertStringToBoolean(rebateCardInfo.Substring(21, 1));
                cardReaderResponse.isCardBlocked = CardHelper.ConvertStringToBoolean(rebateCardInfo.Substring(22, 1).TrimEnd());

            }
            catch (Exception ex)
            {
                throw new PeripheralException("Microsoft_Dynamics_Commerce_HardwareStation_RFID_Card_Error", "Please do not lift the card when reading from card. " + ex.Message, ex);
            }

            return cardReaderResponse;
        }

        public static string GetPersonalPublicKey()
        {
            return "<RSAKeyValue><Modulus>xbmia1n5xwvbHVsrXTqUsGNlA/NRpbw+BU6ngC5pOGFg5gHZqeLea+9AgooL+EM0Yyop3Ns+fl6k6YsdZnzfcHYq4UGOygDjtU14HejN1rOQub5kNQH46k3zJbZwkwxLKghNbyMaNnDpJqQ10EVGzyTqfVFyYAmYAoN/WS6DpB8=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
        }
        public static string GetLoyalityPublicKey()
        {
            return "<RSAKeyValue><Modulus>nF/3OZ3fMUI8i++V2tqyAWHMjL0tew9FQ+e+UqAcxH24RpWPPhGLc6AdC8ZmBO6lsOgZMG2Xgj0c55Dl2IOqHL3uQygaGMPGKJeC2bRmPKJpIJmcNUyaDN7OErMPSQNunbnAspsJnvMMwydrv6Y+mzxhMbKlTrBejZPs1wO1YeM=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
        }        
        
    }
}
