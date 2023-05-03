using Chilkat;
using GIGATMS.NF;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using CDC.Commerce.HardwareStation.RFIDCardReader.Model;
using Microsoft.Dynamics.Commerce.HardwareStation;

namespace CDC.Commerce.HardwareStation.RFIDCardReader
{
    public class RebateCardWriter
    {
        public const string serialPortPrefix = "COM";
        public string cardId { get; set; }
        public MifareReader mifareReader { get; set; }

        public RebateCardWriter()
        {
            Chilkat.Global glob = new Chilkat.Global();
            glob.UnlockBundle("YASOOB.CB1022024_TNcAqC5qnUpg");
        }

        public void CloseConnection()
        {
            this.mifareReader.PortOpen = false;
            this.mifareReader.GNetCancel();
            this.mifareReader.mfRequest();
        }

        public void InitializeCard()
        {
            this.mifareReader = new MifareReader();
            this.mifareReader.CommPort = GetPortNumbers();
            this.mifareReader.PortOpen = true;
            this.mifareReader.mfRequest();
            cardId = mifareReader.mfAnticollision().ToString();
        }

        public short GetPortNumbers()
        {
            string[] portnames = SerialPort.GetPortNames();
            string port = portnames.Where(a => a.ToUpper().Contains(serialPortPrefix)).ToList().FirstOrDefault().ToUpper();
            port = port.Replace("COM", String.Empty);

            return Convert.ToInt16(port);
        }

        public string PreparePersonalInfoPlainText(string csdCardNumber, string firstName, string lastName, string Rank ,string cardId, string limit)
        {
            Chilkat.StringBuilder stringBuilder = new Chilkat.StringBuilder();
            stringBuilder.Append(csdCardNumber.PadRight(10));
            stringBuilder.Append(firstName.PadRight(16));
            stringBuilder.Append(lastName.PadRight(16));
            stringBuilder.Append(Rank.PadRight(16));
            stringBuilder.Append(cardId.PadRight(16));
            stringBuilder.Append(limit.PadLeft(5 , '0'));
            string result = stringBuilder.ToString().PadRight(117);
            Console.WriteLine(result.Length);

            return result;
        }

        public void WritePersnalInfo(RebateCardReaderResponse rebateCard)
        {
            int index = 0;
            byte startSector = 2;
            byte[] piCipher = GetEncryptedPersnalInfo(rebateCard.csdCardNumber,rebateCard.firstName, rebateCard.lastName, rebateCard.rank, rebateCard.writtenCardNumber, rebateCard.limit);

            WriteData(Convert.ToByte(startSector + 0), 0, piCipher.Skip(index).Take(16).ToArray());
            index = index + 16;
            WriteData(Convert.ToByte(startSector + 0), 1, piCipher.Skip(index).Take(16).ToArray());
            index = index + 16;
            WriteData(Convert.ToByte(startSector + 0), 2, piCipher.Skip(index).Take(16).ToArray());
            index = index + 16;
            WriteData(Convert.ToByte(startSector + 2), 0, piCipher.Skip(index).Take(16).ToArray());
            index = index + 16;
            WriteData(Convert.ToByte(startSector + 2), 1, piCipher.Skip(index).Take(16).ToArray());
            index = index + 16;
            WriteData(Convert.ToByte(startSector + 2), 2, piCipher.Skip(index).Take(16).ToArray());
            index = index + 16;
            WriteData(Convert.ToByte(startSector + 1), 0, piCipher.Skip(index).Take(16).ToArray());
            index = index + 16;
            WriteData(Convert.ToByte(startSector + 1), 1, piCipher.Skip(index).Take(16).ToArray());

        }

        public bool WriteData(byte sectorNo, byte blockNo, byte[] hexData)
        {

            mifareReader.mfAuthenticate(sectorNo, GIGATMS.NF.MifareReader.bKeyTypeConstants.KEY_A, null);
            if (mifareReader.mfWrite(blockNo, hexData))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public byte[] GetEncryptedPersnalInfo(string csdCardNumber, string firstName, string lastName, string rank, string cardId,string limit)
        {
            Rsa oRsaPersonal = new Rsa();
            string persnalInfo =  this.PreparePersonalInfoPlainText(csdCardNumber, firstName, lastName, rank, cardId, limit);
            oRsaPersonal.ImportPublicKey(GetPersonalPublicKey());
            oRsaPersonal.ImportPrivateKey(GetPersonalPrivateKey());
            byte[] personalBytes = oRsaPersonal.EncryptString(persnalInfo, true);


            return personalBytes;
        }

        public byte[] GetEncryptedRebateInfo(string balance, DateTime lastTransactionDate, bool isCardActive, bool isCardBlocked)
        {
            Rsa oRsaPersonal = new Rsa();
            string rebateInfo = PrepareRebateInfo(balance, lastTransactionDate, isCardActive, isCardBlocked);
            oRsaPersonal.ImportPublicKey(GetRebatePublickey());
            oRsaPersonal.ImportPrivateKey(GetRebatePrivateKey());
            byte[] personalBytes = oRsaPersonal.EncryptString(rebateInfo, true);

            return personalBytes;
        }


        public void WriteReabteInfo(RebateCardReaderResponse rebateCard)
        {
            int index = 0;
            byte startSector = 7;

            byte[] piCipher = GetEncryptedRebateInfo(rebateCard.balance, DateTime.Today, rebateCard.isCardActivated, rebateCard.isCardBlocked);

            WriteData(Convert.ToByte(startSector + 2), 0, piCipher.Skip(index).Take(16).ToArray());
            index = index + 16;
            WriteData(Convert.ToByte(startSector + 2), 1, piCipher.Skip(index).Take(16).ToArray());
            index = index + 16;
            WriteData(Convert.ToByte(startSector + 2), 2, piCipher.Skip(index).Take(16).ToArray());
            index = index + 16;
            WriteData(Convert.ToByte(startSector + 1), 0, piCipher.Skip(index).Take(16).ToArray());
            index = index + 16;
            WriteData(Convert.ToByte(startSector + 1), 1, piCipher.Skip(index).Take(16).ToArray());
            index = index + 16;
            WriteData(Convert.ToByte(startSector + 1), 2, piCipher.Skip(index).Take(16).ToArray());
            index = index + 16;
            WriteData(Convert.ToByte(startSector + 0), 0, piCipher.Skip(index).Take(16).ToArray());
            index = index + 16;
            WriteData(Convert.ToByte(startSector + 0), 1, piCipher.Skip(index).Take(16).ToArray());
        }

        public string PrepareRebateInfo(string balance , DateTime dateTime, bool isCardActivated, bool isCardBlocked)
        {

            char _isCardBlocked = 'N';
            char _isCardActivated = 'N';

            if (isCardActivated)
            {
                _isCardActivated = 'Y';
            }
            if (isCardBlocked)
            {
                _isCardBlocked = 'Y';
            }

            Chilkat.StringBuilder stringBuilder = new Chilkat.StringBuilder();
            stringBuilder.Append(balance.PadLeft(5, '0'));
            stringBuilder.Append(dateTime.ToString("dd-MM-yyyy HH:mm").PadRight(16));
            stringBuilder.Append(_isCardActivated.ToString());
            stringBuilder.Append(_isCardBlocked.ToString());

            return stringBuilder.ToString().PadRight(117);
        }
        
        public static string GetPersonalPublicKey()
        {
            return "<RSAKeyValue><Modulus>xbmia1n5xwvbHVsrXTqUsGNlA/NRpbw+BU6ngC5pOGFg5gHZqeLea+9AgooL+EM0Yyop3Ns+fl6k6YsdZnzfcHYq4UGOygDjtU14HejN1rOQub5kNQH46k3zJbZwkwxLKghNbyMaNnDpJqQ10EVGzyTqfVFyYAmYAoN/WS6DpB8=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
        }
        public static string GetPersonalPrivateKey()
        {
            return "<RSAKeyValue><Modulus>xbmia1n5xwvbHVsrXTqUsGNlA/NRpbw+BU6ngC5pOGFg5gHZqeLea+9AgooL+EM0Yyop3Ns+fl6k6YsdZnzfcHYq4UGOygDjtU14HejN1rOQub5kNQH46k3zJbZwkwxLKghNbyMaNnDpJqQ10EVGzyTqfVFyYAmYAoN/WS6DpB8=</Modulus><Exponent>AQAB</Exponent><P>2bRdnchGRdS2cDRSZuIg4aOAH9FQeU0+o2iccp0nl3R9KsiGPWvD1nLhC9Ba1fmYizd1fDI5dgygZKcvr+ohlw==</P><Q>6IGPGTT9H4R+uqvgOCa/OfIKEaiqydd0jzUH2Uf8px+xJtmYloq9l4Ho66qLl296zx3DvzbtDwU2KgVN0MlSuQ==</Q><DP>GbfC72a/VnSAcNTdfyXreHxWIGwbs5i6c5diE/AYwz2Ro8I4iXz3j5fWmgytDmYD7T5J9LgCLb3kHL/bVE62VQ==</DP><DQ>PwryLBmMEMGyQxdbgp4u951DUap0NKpw9mugpy+3t/EF7czObPNNmQkmiNADbZpSqFofu3c/K/VzzE0H3nbYMQ==</DQ><InverseQ>ePNrIE3dpbfvixDKSN3pA8RLovA3/T5ngE3eWP8wAAtENc+OGLvHnL6LIRy9SJQ9ZzAEgJovUT2eZIDAiboG2w==</InverseQ><D>OxuI3c2qeSHmsUppVrfl7irwbjNlU4XIBp8iPJPjOYsGxAuGjZ43/o0pvDxXYrJ4bTKUDCVwc6eIZhaDxfkLdBUQ46+u3E4QI8jzoE9PtaXuXaMi+/s7hnPB5mu63fdaPbo4UUwvMPP+p9utDq3M59AiOhXaURr4XxiTlj9KDpk=</D></RSAKeyValue>";
        }
        public string GetDeveolperWriteKey()
        {
            return "gulfraz.khan@csd.gov.pk";
        }

        public string GetRebatePublickey()
        {
            return "<RSAKeyValue><Modulus>nF/3OZ3fMUI8i++V2tqyAWHMjL0tew9FQ+e+UqAcxH24RpWPPhGLc6AdC8ZmBO6lsOgZMG2Xgj0c55Dl2IOqHL3uQygaGMPGKJeC2bRmPKJpIJmcNUyaDN7OErMPSQNunbnAspsJnvMMwydrv6Y+mzxhMbKlTrBejZPs1wO1YeM=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
        }

        public string GetRebatePrivateKey()
        {
            return "<RSAKeyValue><Modulus>nF/3OZ3fMUI8i++V2tqyAWHMjL0tew9FQ+e+UqAcxH24RpWPPhGLc6AdC8ZmBO6lsOgZMG2Xgj0c55Dl2IOqHL3uQygaGMPGKJeC2bRmPKJpIJmcNUyaDN7OErMPSQNunbnAspsJnvMMwydrv6Y+mzxhMbKlTrBejZPs1wO1YeM=</Modulus><Exponent>AQAB</Exponent><P>wcTcYA1Ur9uXyCVK9qoZvjlKxLiDXTJCnI40MDPym5MGldW7VV2EtLPC+g1/M/h/9+Ca6dcmt1ecexzvKy+XKQ==</P><Q>zpiqb/eaYabQYrV6cuzQMTWj2xfjn5JPXQ0LsWRYyE+vGj7gTLUCoeSA9pUwFL+f49Jk06YAgWJqHDWbxPTOKw==</Q><DP>qMC9/JkfjBh+07xG0RPLX7Odvj3DikLfaGEgamqTe5JMReniQLI1hPqZcBSZF7XwHPzrbYQHH92ZVk8YrE/CIQ==</DP><DQ>ilkDsa0+zev2mlNrJ3DcTkfcbYiG3sIMsYRd8zH+nk12Nf4rGFMS6zTpA7eOFibovJiU+oKszfgIlNF1eaRyNw==</DQ><InverseQ>aE/gEtMvxcScxRojWiX+vgqIHoLql7T6/vh72ObRXf5wNYmCn5eK7+HnZGiCmGBkLnO5HTgGd7ts5KSRKeXiZg==</InverseQ><D>QuOz62PyAG6eWpdx6QtqYDf22O7lIQIftVPQcCSIGa7TX/ICs2Cq35tKQWqs4gg5POqTN8lxKSc+EXnEL7DmVzjO9BnTd6IG5C02WnPDqlLfvfflq4svvKGE83PhK80O+gMFJ1B41ZM5q98B86ul5mmtKbZ95iAB8zHjSI6IEtk=</D></RSAKeyValue>";
        }
    }
}
