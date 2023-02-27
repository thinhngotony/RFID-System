using InterfaceLib.DEFINE;
using Rfid.Helper.Services.Mq;
using Shelf_Register.Devices.TEC2140;
using SuperSimpleTcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Shelf_Register
{
    public static class Global
    {
        public static Front mainForm = null;
        public static Setting settingForm = null;
        public static AxOPOSRFIDLib.AxOPOSRFID OPOSRFID1 = new AxOPOSRFIDLib.AxOPOSRFID();
        public static TEC2140 opos;


        public static void SetForm(Front _form)
        {
            mainForm = _form;
        }

        public static void SetForm2(Setting _form)
        {
            settingForm = _form;
        }

        public static List<InventoryTagInfo> readedTagsRepo = new List<InventoryTagInfo>();

        public static System.Timers.Timer TimerRegister = new System.Timers.Timer();

        public static Connect_Ethernet EthernetForm = new Connect_Ethernet();


        public static MqClient dataRegister;
        public static MqClient dataLocation ;
        public static List<RawData> rawDataList = new List<RawData>();

        public static List<ViewData> viewDataList = new List<ViewData>();
        public static string lastRFID;
        public static SimpleTcpClient multiConnections;
        public static string apiStatus = "";
        public static string apiMessage = "";
        public static string rfidcode = "";
        public static string barcode = "";
        public static bool barcode_state = true;
        public static ProductData product = new ProductData();
        public static Dictionary<string, ProductPos> productPos = new Dictionary<string, ProductPos>();
        public static List<SettingAntena> settingPosition = new List<SettingAntena>();
        public static List<SimpleTcpClient> connectedTcpHosts = new List<SimpleTcpClient>();

        public class SettingAntena
        {
            public int row { get; set; }
            public int col { get; set; }
        }

        public class ProductPos
        {
            public string RFIDcode { get; set; }
            public string Jancode { set; get; }
            public string shelf_pos { set; get; }
            public string shelf_col_pos { get; set; }
            public string isbn { set; get; }
            public string product_name { set; get; }
            public string shelf_name { set; get; }
            public string link_image { set; get; }
            public string link_image_bq { set; get; }
            public string status { set; get; }
            public List<string> RFIDlist { set; get; }
            public PictureBox picture_box { set; get; }
            public ProductPos()
            {
                RFIDcode = "";
                Jancode = "";
                RFIDlist = null;
                shelf_pos = "";
                shelf_col_pos = "";
                isbn = "";
                product_name = "";
                shelf_name = "";
                link_image = "";
                status = "";
                picture_box = null;
            }
        }

        public class RawData
        {
            public string rfid { get; set; }
            public int? rssi { get; set; }
            public string shelf_no { get; set; }
            public int? antena_no { get; set; }
            
        }
        public class ViewData
        {
            public string rfid { get; set; }
            public int? count { get; set; }
            public int? rssi { get; set; }

        }


        public class ProductData
        {
            public string RFIDcode { get; set; }
            public string Jancode { set; get; }
            public string Jancode2 { set; get; }
            public string media_name { get; set; }
            public string tax_type { get; set; }
            public int cost_price { get; set; }
            public string media_cd { set; get; }
            public string goods_name_kana { set; get; }
            public string artist_name { set; get; }
            public string makerCD { set; get; }
            public string maker_name { set; get; }
            public string genreCD { set; get; }
            public string maker_name_kana { set; get; }
            public string ccode { set; get; }
            public string selling_date { set; get; }
            public int price { set; get; }
            public double tax_rate { set; get; }
            public string goods_type { set; get; }
            public int goods_cd_type { set; get; }
            public string goods_name { set; get; }
            public string artist_kana { set; get; }
            public string rfid_goods_type { set; get; }
            public double price_intax { set; get; }
            public float cost_rate { set; get; }
            public string isbn { set; get; }
            public string link_image { set; get; }

            public ProductData()
            {
                RFIDcode = "";
                Jancode = "";
                Jancode2 = "";
                media_name = "";
                artist_name = "";
                genreCD = "";
                ccode = "";
                price = 0;
                goods_name = "";
                rfid_goods_type = "";
                isbn = "";
                link_image = "";
            }
        }
    }
}
