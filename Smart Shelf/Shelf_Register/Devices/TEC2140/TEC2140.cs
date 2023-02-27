using AxOPOSRFIDLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Shelf_Register.Devices.TEC2140
{
    public class TEC2140
    {
        public TEC2140()
        {
            Global.OPOSRFID1.DataEvent += OPOSRFID1_DataEvent;
            Global.OPOSRFID1.ErrorEvent += OPOSRFID1_ErrorEvent;
        }


        public void OPOSRFID1_DataEvent(object sender, _DOPOSRFIDEvents_DataEventEvent e)
        {

            int TagCount;
            string UserData;

            TagCount = Global.OPOSRFID1.TagCount;

            for (int i = 0; i < TagCount; i++)
            {
                UserData = " Userdata=" + Global.OPOSRFID1.CurrentTagUserData;

                if (UserData == " Userdata=")
                {
                    UserData = "";
                }
                var code_value = Global.OPOSRFID1.CurrentTagID + UserData;
                string new_code = ConvertTagIDCode(code_value);
                if (!Global.rfidcode.Equals(new_code) && Global.mainForm.btnConnect.Text == "READING...")
                {
                    Global.rfidcode = new_code;
                    Global.mainForm.updateView();
                }
    
                Global.OPOSRFID1.NextTag();

            }


            Global.OPOSRFID1.DataEventEnabled = true;
        }

        public string ConvertTagIDCode(string code_value)
        {
            Dictionary<char, char> nibble_code = new Dictionary<char, char> { { ':', 'A' }, { ';', 'B' }, { '<', 'C' }, { '=', 'D' }, { '>', 'E' }, { '?', 'F' } };
            var stringBuilder = new StringBuilder();
            foreach (var character in code_value)
            {
                if (nibble_code.TryGetValue(character, out var value))
                {
                    stringBuilder.Append(value);
                }
                else
                {
                    stringBuilder.Append(character);
                }
            }
            return stringBuilder.ToString();
        }

        public void OPOSRFID1_ErrorEvent(Object sender, _DOPOSRFIDEvents_ErrorEventEvent e)
        {

        }

        public int OPOS_EnableDevice(AxOPOSRFID OPOSRFID1)
        {
            int Result;
            int phase;
            string strData;

            // Open Device
            string device_name = Global.mainForm.txtScanner.Text;
            Result = OPOSRFID1.Open(device_name);
            if (Result != OposStatus.OposSuccess)
            {
                return 0;
            }

            Result = OPOSRFID1.ClaimDevice(3000);
            if (Result != OposStatus.OposSuccess)
            {

                OPOSRFID1.Close();
                return 1;
            }

            OPOSRFID1.DeviceEnabled = true;
            Result = OPOSRFID1.ResultCode;
            if (Result != OposStatus.OposSuccess)
            {

                OPOSRFID1.Close();
                return 2;
            }

            //    'DirectIOを用いて現在の位相状態を取得する
            phase = 0;
            strData = "";
            Result = OPOSRFID1.DirectIO(115, ref phase, ref strData);
            OPOSRFID1.BinaryConversion = OposStatus.OposBcNibble;
            //OPOSRFID1.BinaryConversion = OposStatus.OposBcNone;
            Result = OPOSRFID1.ResultCode;
            if (Result != OposStatus.OposSuccess)
            {
                OPOSRFID1.Close();
            }

            OPOSRFID1.ProtocolMask = OposStatus.RfidPrEpc1g2;
            Result = OPOSRFID1.ResultCode;
            if (Result != OposStatus.OposSuccess)
            {
                OPOSRFID1.Close();
            }
            return -1;
        }

        public void OPOS_StartReading(AxOPOSRFID OPOSRFID1)
        {
            try
            {
                int Result;
                //OPOSRFID1.ClearInputProperties();
                OPOSRFID1.ReadTimerInterval = Config.interval_read_TEC2140;
                OPOSRFID1.DataEventEnabled = true;

                if (Global.OPOSRFID1.TagCount > 0)
                {

                    Global.OPOSRFID1.ClearInputProperties();
                }

                PhaseChange(OPOSRFID1);
                Result = OPOSRFID1.StartReadTags(OposStatus.RfidRtId, "000000000000000000000000", "000000000000000000000000", 0, 0, 1000, "00000000");
                if (Result != OposStatus.OposSuccess)
                {
                    Console.WriteLine("read err");
                }

                //OPOSRFID1.DataEventEnabled = true;
                //Session.isReading = true;
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        private void PhaseChange(AxOPOSRFID OPOSRFID1)
        {
            int Result;
            int intData;
            string strData;
            //'DirectIOを使用して位相の有効／無効を制御する
            //'位相を有効にするDirectIOを実行する
            intData = 0;
            strData = "";
            Result = OPOSRFID1.DirectIO(116, ref intData, ref strData);
            if (Result == OposStatus.OposEBusy)
            {
                Console.WriteLine("読み取り中です。StopReadTagsを実行してください");
            }
            else if (Result == OposStatus.OposEIllegal)
            {
                Console.WriteLine("共存できない機能を使用している可能性があります");
            }
            else if (Result != OposStatus.OposSuccess)
            {
                Console.WriteLine("位相設定失敗しました");
            }

        }

        public void OPOS_StopReading(AxOPOSRFID OPOSRFID1)
        {
            try
            {
                int Result;
                Result = OPOSRFID1.StopReadTags("00000000");
                if (Result != OposStatus.OposSuccess)
                {
                    Console.WriteLine("Err Stop");
                }
            } catch(Exception e) { Console.WriteLine(e.ToString()); }

        }
    }
}
