using Chilkat;
using GIGATMS.NF;
using Microsoft.Dynamics.Commerce.HardwareStation;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDC.Commerce.HardwareStation.RFIDCardReader
{
    public class CardHelper
    {
        public const string serialPortPrefix = "COM";
        public string cardId { get; set; }
        public MifareReader mifareReader { get; set; }

        public CardHelper()
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
            string port = null;
            try
            {
                string[] portnames = SerialPort.GetPortNames();
                port = portnames.Where(a => a.ToUpper().Contains(serialPortPrefix)).ToList().FirstOrDefault().ToUpper();
                port = port.Replace("COM", String.Empty);
            }
            catch (Exception ex)
            {
                //mifareReader.mfHalt();
                throw new PeripheralException("Microsoft_Dynamics_Commerce_HardwareStation_RFID_Card_Error","There was an error pairing with the RFID card reader device. "+ex.Message, ex);
            }
            return Convert.ToInt16(port);
        }

        public string GetCardInfo(string publicCardKey)
        {
            Rsa oRsaPersonal = new Rsa();
            byte StartSector = 7;
            byte[] value = new byte[0];
            value = Combine(value, ReadData(Convert.ToByte(StartSector + 2), 0));
            value = Combine(value, ReadData(Convert.ToByte(StartSector + 2), 1));
            value = Combine(value, ReadData(Convert.ToByte(StartSector + 2), 2));
            value = Combine(value, ReadData(Convert.ToByte(StartSector + 1), 0));
            value = Combine(value, ReadData(Convert.ToByte(StartSector + 1), 1));
            value = Combine(value, ReadData(Convert.ToByte(StartSector + 1), 2));
            value = Combine(value, ReadData(Convert.ToByte(StartSector + 0), 0));
            value = Combine(value, ReadData(Convert.ToByte(StartSector + 0), 1));

            oRsaPersonal.ImportPublicKey(publicCardKey);
            string decodedString = Encoding.Default.GetString(oRsaPersonal.DecryptBytes(value, false));
            if (decodedString != String.Empty)
            {
                return decodedString;
            }
            return null;
        }

        public string GetPersnalInfo(string persnalPublicKey)
        {
            Rsa oRsaPersonal = new Rsa();
            byte StartSector = 2;
            byte[] value = new byte[0];
            value = Combine(value, ReadData(Convert.ToByte(StartSector + 0), 0));
            value = Combine(value, ReadData(Convert.ToByte(StartSector + 0), 1));
            value = Combine(value, ReadData(Convert.ToByte(StartSector + 0), 2));
            value = Combine(value, ReadData(Convert.ToByte(StartSector + 2), 0));
            value = Combine(value, ReadData(Convert.ToByte(StartSector + 2), 1));
            value = Combine(value, ReadData(Convert.ToByte(StartSector + 2), 2));
            value = Combine(value, ReadData(Convert.ToByte(StartSector + 1), 0));
            value = Combine(value, ReadData(Convert.ToByte(StartSector + 1), 1));

            oRsaPersonal.ImportPublicKey(persnalPublicKey);
            string decodedString = oRsaPersonal.DecryptString(value, false);
            if (decodedString != String.Empty)
            {
                return decodedString;
            }
            return null;
        }
        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] bytes = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
            Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
            return bytes;
        }

        public byte[] ReadData(Byte SectorNo, Byte BlockNo)
        {
            byte[] newArray = new byte[16];
            string value = null;
            if (mifareReader.mfAuthenticate(SectorNo, GIGATMS.NF.MifareReader.bKeyTypeConstants.KEY_A, value))
            {
                if (mifareReader.mfRead(BlockNo, ref newArray))
                {
                    return newArray;
                }
            }
            return new byte[16];
        }

        public static bool ConvertStringToBoolean(string a)
        {
            if (a == "Y")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
