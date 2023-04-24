using InterfaceLib.APIdefine;
using InterfaceLib.DEFINE;
using Rfid.Helper.Services.Mq;
using SuperSimpleTcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows.Forms;
using System.Xml.Linq;
//using static Shelf_Register.Global;

namespace Shelf_Register
{
    public class Connect_Ethernet
    {
        public static void Scan_ReceivedDataEvent(List<InventoryTagInfo> tagsInfoList)
        {
            tagsInfoList.ForEach(p =>
            {
                Console.WriteLine($"{p.DetectedTime} | {p.Ant} | {p.Epc}| {p.Rssi}");
            });
            
            foreach (var data in tagsInfoList)
            {
                Global.rawDataList.Add(new Global.RawData
                {
                    rfid = data.Epc,
                    rssi = data.Rssi,
                    antena_no = data.Ant,
                    shelf_no = Config.nameOfShelf,
                });
            };
        }

        public void UpdateViewTimerRegister()
        {
            Global.TimerRegister.Interval = Config.readingTimer;
            Global.TimerRegister.Enabled = false;
            Global.TimerRegister.Elapsed += UpdateViewTimer_Elapsed;
        }

        public void UpdateViewTimer_Elapsed(object sender, ElapsedEventArgs e)
        {

            Global.TimerRegister.Enabled = false;
            Global.rfidcode = Global.readedTagsRepo.OrderByDescending(p => p.Rssi).FirstOrDefault()?.Epc;


            if (Global.readedTagsRepo.Exists(x => x.Epc == Global.lastRFID))
            {

                if (Global.mainForm.txtRfid.Text == "")
                {
                    Global.lastRFID = Global.mainForm.txtRfid.Text; //780
                    Global.mainForm.txtJan.Text = "";
                    Global.mainForm.txtName.Text = "";
                    Global.mainForm.pictureBox.Load(Const.blank_image);
                }
            }
            else
            {
                Global.mainForm.updateView();
                Global.lastRFID = Global.mainForm.txtRfid.Text;
            }


            Console.WriteLine("=================================");
            Console.WriteLine("RFID: " + Global.rfidcode);
            Console.WriteLine("Last rfid: " + Global.lastRFID);
            Console.WriteLine("View: " + Global.mainForm.txtRfid.Text);
            Console.WriteLine("=================================");


            Global.readedTagsRepo.Clear();
            Global.TimerRegister.Enabled = true;

        }

        public static void Register_ReceivedDataEvent(List<InventoryTagInfo> tagsInfoList)
        {

            Global.readedTagsRepo.AddRange(tagsInfoList);
            
            tagsInfoList.ForEach(p =>
            {
                Console.WriteLine($"{p.DetectedTime} | {p.Ant} | {p.Epc}| {p.Rssi}");

            });

        }


        public static void Location()
        {
            try
            {
                var con = Global.dataLocation.GetRbMqConnection();

                var channel = Global.dataLocation.CreateRbMqChannel(con);

                Global.dataLocation.ReceivedDataEvent += Scan_ReceivedDataEvent;

                Global.dataLocation.IsClearMessageWhenInit = true;

                Global.dataLocation.SubcribeMessages(channel);

                Global.dataLocation.EnableReceivedData = false;

            }
            catch (Exception)
            {
                Console.WriteLine("Error at function - Location()");
            }


        }

        public static void Register()
        {
            try
            {
                var con = Global.dataRegister.GetRbMqConnection();

                var channel = Global.dataRegister.CreateRbMqChannel(con);

                Global.dataRegister.ReceivedDataEvent += Register_ReceivedDataEvent;

                Global.dataRegister.IsClearMessageWhenInit = true;

                Global.dataRegister.SubcribeMessages(channel);

                Global.dataRegister.EnableReceivedData = false;
            } 

            catch(Exception) 
            {
                Console.WriteLine("Error at function - Register()");
            }


        }


    }
}