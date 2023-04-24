using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Collections.Specialized.BitVector32;

namespace Shelf_Register
{
    public class Config
    {

        public static string device_name = "";
        public static double readingTimer = 0;

        //API
        public static string address_api = "";
        public static string rfmaster_sub = "";
        public static string rfmaster_sub_delete = "";
        public static string rfmaster_sub_rfids_to_jans = "";
        public static string rfmaster_key = "";
        public static string image_api_local = "";
        public static string image_api = "";
        public static string image_sub = "";
        public static string insert_raw_data = "";
        public static string set_smart_shelf_setting = "";
        public static string get_smart_shelf_setting = "";
        public static string rfid_to_status_smartshelf = "";
        public static string get_smart_shelf_names = "";
        public static string sub_reset_smartshelf = "";
        public static string sub_clear_raw_data = "";
        public static string insert_more_info_smartshelf = "";
        public static string get_smartshelf_location = "";
        public static string get_smartshelf_location_by_col = "";
        public static string bquery_api = "";
        public static string bquery_sub = "";
        public static string api_key = "";
        public static string SHOPCD = "";
        public static string reload = "";
        public static string sync_api = "";
        public static string sync_sub = "";
        //readCSV
        public static string path = "";

        public static DataTable dt = new DataTable();
        public static Dictionary<string, (int row, int col)> positionPos = new Dictionary<string, (int, int)>()
        {
            { "pictureBox_1_1" , (1,1) },
            { "pictureBox_1_2" , (1,2) },
            { "pictureBox_1_3" , (1,3) },
            { "pictureBox_1_4" , (1,4) },
            { "pictureBox_1_5" , (1,5) },
            { "pictureBox_1_6" , (1,6) },
            { "pictureBox_2_1" , (2,1) },
            { "pictureBox_2_2" , (2,2) },
            { "pictureBox_2_3" , (2,3) },
            { "pictureBox_2_4" , (2,4) },
            { "pictureBox_2_5" , (2,5) },
            { "pictureBox_2_6" , (2,6) },
            { "pictureBox_3_1" , (3,1) },
            { "pictureBox_3_2" , (3,2) },
            { "pictureBox_3_3" , (3,3) },
            { "pictureBox_3_4" , (3,4) },
            { "pictureBox_3_5" , (3,5) },
            { "pictureBox_3_6" , (3,6) },
            { "pictureBox_4_1" , (4,1) },
            { "pictureBox_4_2" , (4,2) },
            { "pictureBox_4_3" , (4,3) },
            { "pictureBox_4_4" , (4,4) },
            { "pictureBox_4_5" , (4,5) },
            { "pictureBox_4_6" , (4,6) },
            { "pictureBox_5_1" , (5,1) },
            { "pictureBox_5_2" , (5,2) },
            { "pictureBox_5_3" , (5,3) },
            { "pictureBox_5_4" , (5,4) },
            { "pictureBox_5_5" , (5,5) },
            { "pictureBox_5_6" , (5,6) },
            { "pictureBox_6_1" , (6,1) },
            { "pictureBox_6_2" , (6,2) },
            { "pictureBox_6_3" , (6,3) },
            { "pictureBox_6_4" , (6,4) },
            { "pictureBox_6_5" , (6,5) },
            { "pictureBox_6_6" , (6,6) }

        };
        public static Dictionary<string, string> mappingTextBox = new Dictionary<string, string>()
        {
            { "pictureBox_1_1" ,"textBox_1_1"},
            { "pictureBox_1_2" ,"textBox_1_2"},
            { "pictureBox_1_3" ,"textBox_1_3"},
            { "pictureBox_1_4" ,"textBox_1_4"},
            { "pictureBox_1_5" ,"textBox_1_5"},
            { "pictureBox_1_6" ,"textBox_1_6"},
            { "pictureBox_2_1" ,"textBox_2_1"},
            { "pictureBox_2_2" ,"textBox_2_2"},
            { "pictureBox_2_3" ,"textBox_2_3"},
            { "pictureBox_2_4" ,"textBox_2_4"},
            { "pictureBox_2_5" ,"textBox_2_5"},
            { "pictureBox_2_6" ,"textBox_2_6"},
            { "pictureBox_3_1" ,"textBox_3_1"},
            { "pictureBox_3_2" ,"textBox_3_2"},
            { "pictureBox_3_3" ,"textBox_3_3"},
            { "pictureBox_3_4" ,"textBox_3_4"},
            { "pictureBox_3_5" ,"textBox_3_5"},
            { "pictureBox_3_6" ,"textBox_3_6"},
            { "pictureBox_4_1" ,"textBox_4_1"},
            { "pictureBox_4_2" ,"textBox_4_2"},
            { "pictureBox_4_3" ,"textBox_4_3"},
            { "pictureBox_4_4" ,"textBox_4_4"},
            { "pictureBox_4_5" ,"textBox_4_5"},
            { "pictureBox_4_6" ,"textBox_4_6"},
            { "pictureBox_5_1" ,"textBox_5_1"},
            { "pictureBox_5_2" ,"textBox_5_2"},
            { "pictureBox_5_3" ,"textBox_5_3"},
            { "pictureBox_5_4" ,"textBox_5_4"},
            { "pictureBox_5_5" ,"textBox_5_5"},
            { "pictureBox_5_6" ,"textBox_5_6"},
            { "pictureBox_6_1" ,"textBox_6_1"},
            { "pictureBox_6_2" ,"textBox_6_2"},
            { "pictureBox_6_3" ,"textBox_6_3"},
            { "pictureBox_6_4" ,"textBox_6_4"},
            { "pictureBox_6_5" ,"textBox_6_5"},
            { "pictureBox_6_6" ,"textBox_6_6"}
        };

        //Config timer
        public static int time_check = 10;
        public static int time_set_location = 10;
        public static string TcpHostShelf1 = "";
        public static string TcpHostShelf2 = "";
        public static bool status_mode = false;
        public static bool scan_mode = false;


        //new 20220910: get product from local file
        public static DataTable dataListLocal = new DataTable();
        public static string path_shelfpro_local = "";
        public static string static_img_folder = "";
        public static Dictionary<string, List<string>> TcpShelfHost_Dictionary = new Dictionary<string, List<string>>();
        public static List<string> TcpHost = new List<string>();
        public static List<string> smart_shelf_names = new List<string>();

        // Variable for Setting.cs
        // public static List<Panel> panel_List = new List<Panel>();
        public static string nameOfShelf = "";
        public static string load_position_mst_antena = "";
        public static string clear_position_mst_antena = "";
        public static string update_position_mst_antena = "";

        public static List<CheckBox> antenaLoadList = new List<CheckBox>();
        //public static List<TextBox> antenaNoList = new List<TextBox>();
        public static List<string> antenaNoList = new List<string>();
        public static bool isLoadSetting = false;
        public static string antenaPower = "";


        // Add new feature 
        public static string ipDevice = "";

        //RabbitMQ
        public static string rabbitMQ = "";


        private static Dictionary<string, List<string>> ReadTcpHosts(string value)
        {
            Dictionary<string, List<string>> temp = new Dictionary<string, List<string>>();
            if (string.IsNullOrEmpty(value))
            {
                Console.WriteLine("Null value in config TCPHosts");
            }
            else
            {

                string[] listShelves = value.Split('?');
                foreach (var shelf in listShelves)
                {
                    string nameOfShelf = shelf.Split('|')[0];
                    string stringValue = shelf.Split('|')[1];
                    List<string> values = stringValue.Split(',').ToList();
                    temp.Add(nameOfShelf, values);
                }

            }
            return temp;
        }

        private static List<string> readFileIni(string path)
        {
            List<string> result = new List<string>();
            if (!File.Exists(path))
            {
                MessageBox.Show("Error. Can not found file!\nPlease add config file! ",
                                   "Can not found file!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
            else
                try
                {
                    StreamReader sr = new StreamReader(path);
                    string line = sr.ReadLine();
                    while (line != null)
                    {
                        result.Add(line);
                        line = sr.ReadLine();
                    }
                    sr.Close();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error. Can not read file!" + "\n" + e,
                                   "Can not read file!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                }

            return result;

        }

        private static Dictionary<string, string> getDictionaryConfig(string path)
        {
            Dictionary<string, string> Config = new Dictionary<string, string>();
            List<string> result = readFileIni(path);
            foreach (string line in result)
            {
                if (!line.Contains("="))
                {
                    MessageBox.Show("Config file have Incorrect syntax!",
                                   "Format Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                }
                string[] temp = line.Split('=');
                Config[temp[0].Trim()] = temp[1].Trim();
            }
            return Config;
        }

        public static void readConfig()
        {
            Dictionary<string, string> dataSetPower = getDictionaryConfig(@"Common/config/PowerSetting.ini");
            List<string> keyList = new List<string>(dataSetPower.Values);
            antenaPower = String.Join(",", keyList.ToArray());

            Dictionary<string, string> dataInFile = getDictionaryConfig(@"Common/config/SmartShelfConfig.ini");
            api_key = dataInFile["api_key"];
            address_api = dataInFile["address_api"];
            set_smart_shelf_setting = dataInFile["set_smart_shelf_setting"];
            get_smart_shelf_setting = dataInFile["get_smart_shelf_setting"];
            get_smart_shelf_names = dataInFile["get_smart_shelf_names"];
            sub_reset_smartshelf = dataInFile["sub_reset_smartshelf"];
            sub_clear_raw_data = dataInFile["sub_clear_raw_data"];
            get_smartshelf_location = dataInFile["get_smartshelf_location"];
            get_smartshelf_location_by_col = dataInFile["get_smartshelf_location_by_col"];
            rfmaster_sub_rfids_to_jans = dataInFile["sub_url_rfid_to_jan"];
            insert_more_info_smartshelf = dataInFile["insert_more_info_smartshelf"];
            image_api = dataInFile["image_api"];
            image_api_local = dataInFile["image_api_local"];
            insert_raw_data = dataInFile["insert_raw_data"];
            bquery_api = dataInFile["bquery_api"];
            bquery_sub = dataInFile["bquery_sub"];
            device_name = dataInFile["device_name"];
            time_check = (int)Int64.Parse(dataInFile["check_interval_miliseconds"]) / 1000;
            rfid_to_status_smartshelf = dataInFile["rfid_to_status_smartshelf"];
            TcpShelfHost_Dictionary = ReadTcpHosts(dataInFile["TcpShelfHost"]);
            time_set_location = (int)Int64.Parse(dataInFile["setlocation_interval_miliseconds"]) / 1000;
            update_position_mst_antena = dataInFile["update_position_mst_antena"];
            clear_position_mst_antena = dataInFile["clear_position_mst_antena"];
            load_position_mst_antena = dataInFile["load_position_mst_antena"];
            ipDevice = dataInFile["ipDevice"];
            path_shelfpro_local = dataInFile["path_shelfpro_local"];
            static_img_folder = dataInFile["static_img_folder"];
            rabbitMQ = dataInFile["rabbitMQ"];
            readingTimer = Convert.ToDouble(dataInFile["readingTimer"]);

        }

    }
}
