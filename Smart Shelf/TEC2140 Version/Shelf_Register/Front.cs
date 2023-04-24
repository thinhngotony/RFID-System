using AxOPOSRFIDLib;
using Newtonsoft.Json.Linq;
using SuperSimpleTcp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;

using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Shelf_Register
{
    public partial class Front : Form
    {
        OPOS opos;
        public static SimpleTcpClient multiConnections;


        public string barcode = "";
        public bool barcode_state = true;
        string api_status = "";
        string api_message = "";
        bool rfid_reading = true;
        public Front()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.Manual;
            this.CenterToScreen();

            //KillProcessRFID.ControlKill.KillRFIDProcess_diffCurrent();
            init();
            Session.front = this;
            txtRfid.Text = Session.rfidcode;
            txtJan.Text = Session.barcode;
            txtScanner.Text = Session.device_name;
        }

        private void init()
        {
            opos = new OPOS();
            Dictionary<string, string> dataInFile = getDictionaryConfig("SMART_SHELF_CONFIG.ini");
            Session.api_key = dataInFile["api_key"];
            Session.address_api = dataInFile["address_api"];
            Session.set_smart_shelf_setting = dataInFile["set_smart_shelf_setting"];
            Session.get_smart_shelf_setting = dataInFile["get_smart_shelf_setting"];
            Session.get_smart_shelf_names = dataInFile["get_smart_shelf_names"];
            Session.sub_reset_smartshelf = dataInFile["sub_reset_smartshelf"];
            Session.sub_clear_raw_data = dataInFile["sub_clear_raw_data"];
            Session.get_smartshelf_location = dataInFile["get_smartshelf_location"];
            Session.get_smartshelf_location_by_col = dataInFile["get_smartshelf_location_by_col"];
            Session.rfmaster_sub_rfids_to_jans = dataInFile["sub_url_rfid_to_jan"];
            Session.insert_more_info_smartshelf = dataInFile["insert_more_info_smartshelf"];
            Session.image_api = dataInFile["image_api"];
            Session.image_api_local = dataInFile["image_api_local"];
            Session.bquery_api = dataInFile["bquery_api"];
            Session.bquery_sub = dataInFile["bquery_sub"];
            Session.device_name = dataInFile["device_name"];
            Session.rT = (int)Int64.Parse(dataInFile["rT"]);
            Session.time_check = (int)Int64.Parse(dataInFile["check_interval_miliseconds"]) / 1000;
            Session.JanLen = (int)Int64.Parse(dataInFile["JanLen"]);
            Session.rfid_to_status_smartshelf = dataInFile["rfid_to_status_smartshelf"];
            Session.TcpShelfHost_Dictionary = ReadTcpHosts(dataInFile["TcpShelfHost"]);
            Session.time_set_location = (int)Int64.Parse(dataInFile["setlocation_interval_miliseconds"]) / 1000;
            Session.update_position_mst_antena = dataInFile["update_position_mst_antena"];
            Session.clear_position_mst_antena = dataInFile["clear_position_mst_antena"];
            Session.load_position_mst_antena = dataInFile["load_position_mst_antena"];

            Dictionary<string, string> dataSetPower = getDictionaryConfig("PowerSetting.ini");
            List<string> keyList = new List<string>(dataSetPower.Values);
            Session.antenaPower = String.Join(",", keyList.ToArray());

            //Session.antenaPower = keyList.ToString();
            //Console.WriteLine(Session.antenaPower);

            



            foreach (PictureBox pictureBox_Items in ImageLayer.Controls.OfType<PictureBox>())
            {
                pictureBox_Items.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox_Items.Load("blank_background.png");
            }
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox.Load("blank_background.png");

            Task.Run(() => ApiGetSmartShelfNames()).Wait();
            cbShelf.DataSource = Session.smart_shelf_names;
            cbShelf.SelectedItem = "SHELF 1";
            txtInterval.Text = Session.time_check.ToString();
            txtLocation.Text = Session.time_set_location.ToString();

            DataTable dt = new DataTable();
            DataColumn dc = new DataColumn();
            if (dt.Columns.Count == 0)
            {
                dt.Columns.Add("RFID", typeof(string));
                dt.Columns.Add("Jancode", typeof(string));
                dt.Columns.Add("Product Name", typeof(string));
                dt.Columns.Add("Image", typeof(string));
            }


            //new 20220910: get product from local file
            Session.path_shelfpro_local = dataInFile["path_shelfpro_local"];
            Session.static_img_folder = dataInFile["static_img_folder"];
            if (!Directory.Exists(Session.static_img_folder))
            {
                Directory.CreateDirectory(Session.static_img_folder);
            }

            if (Session.TcpShelfHost_Dictionary.ContainsKey(cbShelf.Text.ToString()))
            {
                Session.TcpHost = Session.TcpShelfHost_Dictionary[cbShelf.Text.ToString()];
            }
            else
            {
                messageFromApp.Text += DateTime.Now.ToString("hh:mm:ss") + ": Can't find IP for TcpHosts \n";
            }


        }

        private Dictionary<string, List <string> > ReadTcpHosts(string value)
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

        public void updateView()
        {
            txtJan.Text = Session.barcode;
            txtRfid.Text = Session.rfidcode;
            txtName.Text = Session.product.goods_name;
            Session.lastRfidRead = txtRfid.Text;

        }

        public void updateName()
        {
            foreach (string key in Session.productPos.Keys)
            {
                if (Session.productPos[key].RFIDcode != "")
                {
                    PictureBox pic = getPictureBoxByName(key);
                    string key_text = key.Replace("pictureBox", "textBox");
                    foreach (var txtBox_Items in ImageLayer.Controls.OfType<TextBox>())
                    {
                        if (txtBox_Items.Name == key_text && txtBox_Items.Text == "")
                        {
                            txtBox_Items.BackColor = Color.PaleTurquoise;
                            txtBox_Items.Text += Session.productPos[key].Jancode;
                            txtBox_Items.Text += "\r\n";
                            txtBox_Items.Text += Session.productPos[key].product_name;
                            deleteName();
                        }
                        //Handle for swap data
                        else if (txtBox_Items.Name == key_text && txtBox_Items.Text != "")
                        {
                            txtBox_Items.Text = "";
                            txtBox_Items.BackColor = Color.PaleTurquoise;
                            txtBox_Items.Text += Session.productPos[key].Jancode;
                            txtBox_Items.Text += "\r\n";
                            txtBox_Items.Text += Session.productPos[key].product_name;
                            deleteName();
                        }
                    }
                }
            }
        }

        public void updateName_Scan()
        {
            foreach (var txtBox_Items in ImageLayer.Controls.OfType<TextBox>())
            {
                txtBox_Items.Text = txtBox_Items.Name.Substring(8, 3);               
            }
            foreach (string key in Session.productPos.Keys)
            {
                if (Session.productPos[key].RFIDcode != "")
                {
                    PictureBox pic = getPictureBoxByName(key);
                    string key_text = key.Replace("pictureBox", "textBox");
                    foreach (var txtBox_Items in ImageLayer.Controls.OfType<TextBox>())
                    {
                        if (txtBox_Items.Name == key_text)
                        {
                            txtBox_Items.BackColor = Color.PaleTurquoise;
                            txtBox_Items.Text += "\r\n";
                            txtBox_Items.Text += Session.productPos[key].Jancode;
                            txtBox_Items.Text += "\r\n";
                            txtBox_Items.Text += Session.productPos[key].product_name;
                            deleteName_Scan();                            
                        }
                        //Handle for swap data
                        else if (txtBox_Items.Name == key_text && txtBox_Items.Text != "")
                        {
                            txtBox_Items.Text = "";
                            txtBox_Items.BackColor = Color.PaleTurquoise;
                            txtBox_Items.Text += Session.productPos[key].Jancode;
                            txtBox_Items.Text += "\r\n";
                            txtBox_Items.Text += Session.productPos[key].product_name;
                            deleteName();
                        }
                    }
                }
            }

        }

        public void updateStatus()
        {
            foreach (string key in Session.productPos.Keys)
            {
                //PictureBox pic = Image_Items as PictureBox;
                Task.Run(() => ApiGetSmartShelfStatus(key, Session.productPos[key].RFIDcode)).Wait();

                if (Session.productPos[key].status == "00")
                {
                    string key_text = key.Replace("pictureBox", "textBox");
                    foreach (var txtBox_Items in ImageLayer.Controls.OfType<TextBox>())
                    {
                        if (txtBox_Items.Name == key_text)
                        {
                            txtBox_Items.BackColor = Color.LightGreen;
                        }
                        Session.status_mode = true;
                    }
                }
                else
                {
                    //Bugging
                    PictureBox pic = getPictureBoxByName(key);
                    if (pic != null) { 
                        Image img = changeOpacity(new Bitmap(pic.Image), 100);
                        pic.Image = img;
                    }

                    string key_text = key.Replace("pictureBox", "textBox");
                    foreach (var txtBox_Items in ImageLayer.Controls.OfType<TextBox>())
                    {
                        if (txtBox_Items.Name == key_text)
                        {
                            txtBox_Items.BackColor = Color.White;
                        }
                        else
                        {
                        }
                        Session.status_mode = true;
                    }
                }
            }

        }


        public void resetStatus()
        {
            foreach (PictureBox pictureBox_Items in ImageLayer.Controls.OfType<PictureBox>())
            {
                    Image img = changeOpacity(new Bitmap(pictureBox_Items.Image), 255);
                    pictureBox_Items.Image = img;
            }
            foreach (TextBox txtBox_Items in ImageLayer.Controls.OfType<TextBox>())
            {
                txtBox_Items.BackColor = Color.PaleTurquoise;
            }
        }

        public void updatePictureBox()
        {

            foreach (var Image_Items in ImageLayer.Controls.OfType<PictureBox>())
            {
                PictureBox pic = Image_Items as PictureBox;
                pic.SizeMode = PictureBoxSizeMode.StretchImage;
                if (Session.productPos.Keys.Contains(pic.Name))
                {                    
                    if (string.IsNullOrEmpty(Session.productPos[pic.Name].link_image))
                    {

                        if(Session.productPos[pic.Name].RFIDcode != "") 
                        {
                            Session.productPos[pic.Name].link_image = "noimage.png";
                            pic.Load("noimage.png");
                        }
                        else 
                        {
                            pic.Load("blank_background.png");
                        }
                    }
                    else
                    {
                        string url = GetImage(Session.productPos[pic.Name].link_image, Session.productPos[pic.Name].RFIDcode);
                        Session.productPos[pic.Name].link_image = url;
                        pic.Load(url);
                    }

                }
            }
        }


        public bool CheckValidUrl(string url)
        {
            // new 20220910: get product from local file
            Uri uriResult;
            bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if ((result == true) || File.Exists(url))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckValidUrlNoLocal(string url)
        {
            // new 20220910: get product from local file
            Uri uriResult;
            bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            return result;
        }

        public void deleteName()
        {
            foreach (var txtBox_Items in ImageLayer.Controls.OfType<TextBox>())
            {
                foreach (var picBox_Items in ImageLayer.Controls.OfType<PictureBox>())
                {
                    if (!Session.productPos.Keys.Contains(picBox_Items.Name) && Session.mappingTextBox[picBox_Items.Name] == txtBox_Items.Name && txtBox_Items.Text != "") 
                    {
                        txtBox_Items.Text = "";
                        if (Session.status_mode == false)
                        {
                            txtBox_Items.BackColor = Color.PaleTurquoise;
                        }   
                    } 
                }
            }
        }

        public void deleteName_Scan()
        {
            foreach (var txtBox_Items in ImageLayer.Controls.OfType<TextBox>())
            {
                foreach (var picBox_Items in ImageLayer.Controls.OfType<PictureBox>())
                {
                    if (!Session.productPos.Keys.Contains(picBox_Items.Name) && Session.mappingTextBox[picBox_Items.Name] == txtBox_Items.Name && txtBox_Items.Text != "")
                    {
                        txtBox_Items.Text = "";
                        if (txtBox_Items.Text == "") { 

                            txtBox_Items.Text = txtBox_Items.Name.Substring(8, 3);
                        }
                        if (Session.status_mode == false)
                        {
                            txtBox_Items.BackColor = Color.PaleTurquoise;
                        }
                    }
                }
            }
        }


        public void resetLabel(int mode)
        {
            if (mode == 1)
            {
                Session.product = new ProductData();
                Session.productPos = new Dictionary<string, ProductPos>();
                Session.barcode = "";
                Session.rfidcode = "";
                txtRfid.Text = "";
                txtJan.Text = "";
                txtName.Text = "";
                pictureBox.Load("blank_background.png");

                foreach (PictureBox pictureBox_Items in ImageLayer.Controls.OfType<PictureBox>())
                {
                    pictureBox_Items.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox_Items.Load("blank_background.png");
                }
                foreach (TextBox txtBox_Items in ImageLayer.Controls.OfType<TextBox>())
                {
                    txtBox_Items.Text = "";
                    txtBox_Items.BackColor = Color.PaleTurquoise;
                }
                Session.status_mode = false;
                Session.scan_mode = false;

            } else if (mode == 2)
            {
                foreach (PictureBox pictureBox_Items in ImageLayer.Controls.OfType<PictureBox>())
                {
                    pictureBox_Items.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox_Items.Load("blank_background.png");
                }
                foreach (TextBox txtBox_Items in ImageLayer.Controls.OfType<TextBox>())
                {
                    txtBox_Items.Text = "";
                    txtBox_Items.BackColor = Color.PaleTurquoise;
                }
                Session.status_mode = false;
            }
        }

        private async Task ApiGetDataFromBQ()
        {
            try
            {
                Session.product = new ProductData();
                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Session.bquery_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    api_key = Session.api_key,
                    jancode = Session.barcode,
                    search_mode = 2,
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await api_client.PostAsync(Session.bquery_sub, content);
                if (result.IsSuccessStatusCode)
                {
                    string resultContent = await result.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(resultContent);
                    api_status = (string)data["code"];
                    api_message = (string)data["message"];
                    if (api_status.Equals("00"))
                    {
                        Session.product.ccode = (string)data["data"]["c_code"] == null ? "//" : (string)data["data"]["c_code"];
                        Session.product.artist_name = (string)data["data"]["artist_name"] == "" ? "//" : (string)data["data"]["artist_name"];
                        Session.product.artist_kana = (string)data["data"]["artist_kana"] == "" ? "//" : (string)data["data"]["artist_kana"];
                        Session.product.Jancode = Session.barcode;
                        Session.product.goods_name = (string)data["data"]["goods_name"];
                        Session.product.goods_name_kana = (string)data["data"]["goods_name_kana"];
                        Session.product.media_cd = (string)data["data"]["media_cd"];
                        Session.product.genreCD = (string)data["data"]["genre_cd"];
                        Session.product.price = (int)data["data"]["price"];
                        Session.product.tax_rate = 1.1;
                        Session.product.price_intax = Convert.ToInt32(Math.Floor(Session.product.price * Session.product.tax_rate));
                        Session.product.cost_rate = (float)data["data"]["cost_rate"];
                        Session.product.makerCD = (string)data["data"]["publisher_cd"];
                        Session.product.maker_name = (string)data["data"]["publisher_name"];
                        Session.product.maker_name_kana = (string)data["data"]["publisher_name_kana"];
                        Session.product.selling_date = (string)data["data"]["selling_date"];
                        Session.product.isbn = (string)data["data"]["isbn"];
                    }
                    else
                    {
                        Console.WriteLine("Connect to ApiGetDataFromBQ Failed");
                    }

                }
            }
            catch (Exception)
            {
                Console.WriteLine("Connect to ApiGetDataFromBQ Failed");
            }
        }

        private async Task<(string, string, string)> ApiGetDataFromBQ_Sync(string key,string jancode)
        {
            try
            {
                Session.product = new ProductData();
                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Session.bquery_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    api_key = Session.api_key,
                    jancode = jancode,
                    search_mode = 2,
                });


                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await api_client.PostAsync(Session.bquery_sub, content);
                if (result.IsSuccessStatusCode)
                {
                    string resultContent = await result.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(resultContent);

                    api_status = (string)data["code"];
                    api_message = (string)data["message"];
                    if (api_status.Equals("00"))
                    {
                        if (Session.productPos.Keys.Contains(key))
                        {
                            Session.productPos[key].isbn = (string)data["data"]["isbn"];
                            Session.productPos[key].product_name = (string)data["data"]["goods_name"];
                        }
                        string isbn_result = (string)data["data"]["isbn"];
                        string product_name_result = (string)data["data"]["goods_name"];

                        return (isbn_result, product_name_result, jancode);
                    }
                    else
                    {
                        Console.WriteLine("Connect to ApiGetDataFromBQ_Sync Failed.");
                        return ("", "", "");
                    }


                } else
                {
                    return ("", "", "");
                }

            }
            catch (Exception)
            {
                Console.WriteLine("Connect to ApiGetDataFromBQ_Sync Failed.");
                return ("", "", "");

            }
        }

        private static Boolean isBase64(String str)
        {
            String patern = "^([A-Za-z0-9+/]{4})*([A-Za-z0-9+/]{3}=|[A-Za-z0-9+/]{2}==)?$";
            return Regex.IsMatch(str, patern);
        }

        public Image LoadImage(string base64_data)
        {
            //data:image/gif;base64,
            //this image is a single pixel (black)
            if (base64_data != "noimage.png" && base64_data != "blank_background.png")
            {
                byte[] bytes = Convert.FromBase64String(base64_data);

                Image image;
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    image = Image.FromStream(ms);
                }

                return image;
            }
            else
            {
                pictureBox.Load("noimage.png");
                return pictureBox.Image;
            }
        }

        public string GetImage(string data, string rfid)
        {
            //Check is base64
            if (!isBase64(data))
            {
                if (data == "noimage.png" || data == "blank_background.png")
                {
                    return "noimage.png";
                }
                return data;
            }

            byte[] bytes = Convert.FromBase64String(data);

            Image image;
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                image = Image.FromStream(ms);
                string name = Path.Combine(Session.static_img_folder, rfid + ".jpg");
                if (File.Exists(name))
                {
                    File.Delete(name);
                }
                image.Save(name, ImageFormat.Jpeg);
                return name;
            }
        }



        private async Task ApiGetImageLocal(string key, string isbn)
        {
            try
            {

                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Session.address_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string json = "";

                json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    api_key = Session.api_key,
                    isbn = isbn
                }
                );

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await api_client.PostAsync(Session.image_api_local, content);


                if (result.IsSuccessStatusCode)
                {
                    string resultContent = await result.Content.ReadAsStringAsync();
                    JObject api_result = JObject.Parse(resultContent);

                    api_message = (string)api_result["message"];
                    api_status = (string)api_result["code"];

                    if (api_status.Equals("00"))
                    {
                        string base64 = (string)api_result["base64"];
                        if (base64 != null)
                        {
                            //Session.productPos[key].link_image = base64;
                            //Handle get image link from BQ for Cuong
                            Session.productPos[key].link_image_bq = base64;
                            Image loadBase64 = LoadImage(base64);

                            // new 20220910: get product from local file
                            // save base64 image to local
                            string name = Path.Combine(Session.static_img_folder, Session.productPos[key].RFIDcode + ".jpg");

                            File.WriteAllBytes(name, Convert.FromBase64String((string)api_result["base64"]));
                            ShelfProduct new_product = new ShelfProduct();
                            new_product.jancode = Session.productPos[key].Jancode;
                            new_product.goods_name = Session.productPos[key].product_name;
                            new_product.path_img = name;
                            new_product.rfid = Session.productPos[key].RFIDcode;


                            ShelfLocal.UpdateLocalCSV(Session.path_shelfpro_local, new_product);

                            Session.productPos[key].link_image = name;
                            // Displayed in the user interface
                            if (Session.product.isbn != "")
                            {
                                foreach (PictureBox pictureBox_Items in ImageLayer.Controls.OfType<PictureBox>())
                                {
                                    if (Session.productPos.Keys.Contains(pictureBox_Items.Name))
                                    {
                                        pictureBox_Items.Image = loadBase64;
                                    }
                                }

                            }
                        }
                        else
                        {
                            Session.productPos[key].link_image = (string)api_result["cover"];
                            Console.WriteLine("Cover is", (string)api_result["cover"]);

                            // new 20220910: get product from local file
                            // download image to local and update CSV file
                            string url = (string)api_result["cover"];
                            //string name = Path.Combine(Session.static_img_folder, url.Split('/').Last());
                            string name = Path.Combine(Session.static_img_folder, string.Format("{0}.jpg", Session.productPos[key].RFIDcode));
                            bool result_download = ShelfLocal.DownloadImage(url, name, ImageFormat.Jpeg);
                            Console.WriteLine("Download image is sucess: " + result_download);

                            ShelfProduct new_product = new ShelfProduct();
                            new_product.jancode = Session.productPos[key].Jancode;
                            new_product.goods_name = Session.productPos[key].product_name;
                            new_product.path_img = name;
                            new_product.rfid = Session.productPos[key].RFIDcode;


                            ShelfLocal.UpdateLocalCSV(Session.path_shelfpro_local, new_product);

                            if (Session.product.isbn != "")
                            {
                                foreach (PictureBox pictureBox_Items in ImageLayer.Controls.OfType<PictureBox>())
                                {
                                    if (Session.productPos.Keys.Contains(pictureBox_Items.Name))
                                    {
                                        pictureBox_Items.LoadAsync(Session.product.link_image);
                                    }
                                }

                            }
                            else
                            {
                                pictureBox.Load("noimage.png");
                            }
                        }

                    }
                    else if (api_status.Equals("02"))
                    {
                        // Fixing
                        Session.productPos[key].link_image = (string)api_result["cover"];
                        Console.WriteLine("Cover is", (string)api_result["cover"]);
                        // Displayed in the user interface
                        if (Session.product.isbn != "")
                        {
                            foreach (PictureBox pictureBox_Items in ImageLayer.Controls.OfType<PictureBox>())
                            {
                                if (Session.productPos.Keys.Contains(pictureBox_Items.Name))
                                {
                                    pictureBox_Items.LoadAsync(Session.product.link_image);
                                }
                            }

                        }
                        else
                        {
                            pictureBox.Load("noimage.png");
                        }
                    }
                    else
                    {
                        Console.WriteLine(result);
                    }
                }
            }


            catch (Exception)
            {
                Console.WriteLine("Failed to get image - ApiGetImageLocal \n");

            }
        }

        private async Task<string> ApiGetImageLocal_ForGrid(string isbn)
        {
            try
            {

                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Session.address_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string json = "";

                json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    api_key = Session.api_key,
                    isbn = isbn
                }
                );


                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await api_client.PostAsync(Session.image_api_local, content);


                if (result.IsSuccessStatusCode)
                {
                    string resultContent = await result.Content.ReadAsStringAsync();
                    JObject api_result = JObject.Parse(resultContent);
                    api_message = (string)api_result["message"];
                    api_status = (string)api_result["code"];

                    if (api_status.Equals("00"))
                    {

                        string base64 = (string)api_result["base64"];
                        string cover = (string)api_result["cover"];
                        if (base64 == null && cover != null)
                        {
                            // Return data
                            return cover;
                        }
                        else
                        {
                            return base64;
                        }

                    }
                    else if (api_status.Equals("02"))
                    {
                        return "noimage.png";
                    }
                } else
                {
                    return "noimage.png";
                }
            } catch (Exception)
            {
                Console.WriteLine("Failed to get image \n");
                return "noimage.png";
            }
            return "noimage.png";
        }

        private async Task ApiGetImage()
        {
            try
            {
                pictureBox.Load("blank_background.png");
                barcode_state = false;
                Session.image_sub = "isbn=";
                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Session.image_api);
                var builder = new UriBuilder(Session.image_api);
                builder.Query = Session.image_sub + Session.product.isbn;
                var url = builder.ToString();
                var res = await api_client.GetAsync(url);
                var content = await res.Content.ReadAsStringAsync();

                // Extract value from response data
                JArray jsonArray = JArray.Parse(content);
                if (jsonArray[0].ToString() == "")
                {
                    if (Session.product.Jancode != "")
                    {
                        pictureBox.Load("noimage.png");
                    }
                    else
                    {
                        pictureBox.Load("blank_background.png");
                    }
                }
                dynamic data = JObject.Parse(jsonArray[0].ToString());

                if (content != "")
                {
                    if (data.summary.cover != "")
                    {
                        Session.product.link_image = data.summary.cover;
                        // Displayed in the user interface
                        if (Session.product.isbn != "")
                        {
                            pictureBox.LoadAsync(Session.product.link_image);
                        }
                        else
                        {
                            pictureBox.Load("noimage.png");
                        }
                    }
                    else
                    {
                        if (txtRfid.Text != "")
                        {
                            pictureBox.Load("noimage.png");
                        }
                        else
                        {
                            pictureBox.Load("blank_background.png");

                        }
                    }
                }

            }
            catch (Exception)
            {
                Console.WriteLine("Failed to get image - ApiGetImage \n");

            }
        }

        public static string ImageToBase64(string _imagePath)
        {
            string _base64String = null;

            using (System.Drawing.Image _image = System.Drawing.Image.FromFile(_imagePath))
            {
                using (MemoryStream _mStream = new MemoryStream())
                {
                    _image.Save(_mStream, _image.RawFormat);
                    byte[] _imageBytes = _mStream.ToArray();
                    _base64String = Convert.ToBase64String(_imageBytes);
                    return  _base64String;
                }
            }
        }

        public static string ImageToBase64_Online(string _imagePath)
        {
            string encodedUrl = Convert.ToBase64String(Encoding.Default.GetBytes(_imagePath));

            using (var client = new WebClient())
            {
                byte[] dataBytes = client.DownloadData(new Uri(_imagePath));
                string encodedFileAsBase64 = Convert.ToBase64String(dataBytes);
                return encodedFileAsBase64;
            }
            
        }

        private async Task ApiSetSmartShelfSetting(string dpp_shelf_name)
        {
            try
            {

                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Session.address_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string json = "";

                

                foreach (PictureBox pic in ImageLayer.Controls.OfType<PictureBox>())
                {
                    if (Session.productPos.Keys.Contains(pic.Name))
                    {
                        Console.WriteLine(pic.Name);
                        Console.WriteLine(Session.productPos);
                        Console.WriteLine("");
                        if (Session.productPos[pic.Name].link_image == "noimage.png")
                        {
                            Session.productPos[pic.Name].link_image = "";
                        }

                        string temp = Session.productPos[pic.Name].link_image;
                        Console.WriteLine("bug is", temp);
                        //Nếu base 64 OK
                        //Nếu link Online OK => Chuyển thành link local
                        //Nếu link local => Covert sang base64
                        string base64_covert = "";
                        if (CheckValidUrlNoLocal(Session.productPos[pic.Name].link_image))
                        {
                            base64_covert = ImageToBase64_Online(Session.productPos[pic.Name].link_image);
                        }
                        else if (Session.productPos[pic.Name].link_image != "")
                        {
                            base64_covert = ImageToBase64(Session.productPos[pic.Name].link_image);
                        }

                        //if (Session.productPos[pic.Name].link_image != "")
                        //{
                        //    base64_covert = ImageToBase64(Session.productPos[pic.Name].link_image);
                        //}

                        json = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            api_key = Session.api_key,
                            dpp_shelf_pos = Int32.Parse(Session.productPos[pic.Name].shelf_pos),
                            dpp_shelf_col_pos = Int32.Parse(Session.productPos[pic.Name].shelf_col_pos),
                            dpp_jan_cd = Session.productPos[pic.Name].Jancode,
                            dpp_rfid_cd = Session.productPos[pic.Name].RFIDcode,
                            dpp_isbn = Session.productPos[pic.Name].isbn,
                            dpp_product_name = Session.productPos[pic.Name].product_name,
                            dpp_scaner_name = txtScanner.Text,
                            dpp_shelf_name = dpp_shelf_name,
                            dpp_image_url = base64_covert
                        }
                        
                        );
                    }
                    else
                    {
                        json = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            api_key = Session.api_key,
                            dpp_shelf_pos = Int32.Parse(pic.Name.Substring(11, 1)),
                            dpp_shelf_col_pos = Int32.Parse(pic.Name.Substring(13, 1)),
                            dpp_jan_cd = "",
                            dpp_rfid_cd = "",
                            dpp_isbn = "",
                            dpp_product_name = "",
                            dpp_scaner_name = txtScanner.Text,
                            dpp_shelf_name = dpp_shelf_name,
                            dpp_image_url = ""
                        });
                    }

                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var result = await api_client.PostAsync(Session.set_smart_shelf_setting, content);


                    if (result.IsSuccessStatusCode)
                    {

                        string resultContent = await result.Content.ReadAsStringAsync();
                        JObject data = JObject.Parse(resultContent);
                        Console.WriteLine(resultContent);
                        api_message = (string)data["message"];
                        api_status = (string)data["code"];
                        Session.smart_shelf_names.Add(dpp_shelf_name);
                    }
                    else
                    {
                        Console.WriteLine(result);

                    }

                }
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to set Smart Self setting");
            }

        }


        private async Task ApiInsertMoreInfoSmartShelf()
        {
            try
            {

                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Session.address_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string json = "";
                foreach (PictureBox pic in ImageLayer.Controls.OfType<PictureBox>())
                {
                    if (Session.productPos.Keys.Contains(pic.Name))
                    {
                        if (Session.productPos[pic.Name].link_image == "noimage.png")
                        {
                            Session.productPos[pic.Name].link_image = "";
                        }
                        json = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            api_key = Session.api_key,
                            EPC = Session.productPos[pic.Name].RFIDcode,
                            jancode = Session.productPos[pic.Name].Jancode,
                            product_name = Session.productPos[pic.Name].product_name,
                            link_image = Session.productPos[pic.Name].link_image
                        }
                        );
                    }
                    else
                    {
                        //Do nothing
                    }


                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var result = await api_client.PostAsync(Session.insert_more_info_smartshelf, content);


                    if (result.IsSuccessStatusCode)
                    {

                        string resultContent = await result.Content.ReadAsStringAsync();
                        JObject data = JObject.Parse(resultContent);
                        Console.WriteLine(resultContent);
                        api_message = (string)data["message"];
                        api_status = (string)data["code"];
                    }
                    else
                    {
                        Console.WriteLine(result);

                    }

                }

            }
            catch (Exception)
            {
                Console.WriteLine("Failed to set Smart Self setting");
            }

        }



        private async Task ApiResetLocation()
        {
            try
            {

                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Session.address_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string json = "";

                json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    api_key = Session.api_key,
                }
                );
        

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await api_client.PostAsync(Session.sub_reset_smartshelf, content);


                if (result.IsSuccessStatusCode)
                {

                    string resultContent = await result.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(resultContent);
                    Console.WriteLine(resultContent);
                    api_message = (string)data["message"];
                    api_status = (string)data["code"];
                }
                else
                {
                    Console.WriteLine(result);
                }

                

            }
            catch (Exception)
            {
                Console.WriteLine("Failed to set Smart Self locations");
            }

        }



        private async Task ApiGetSmartShelfSetting(string dpp_shelf_name)
        {
            try
            {

                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Session.address_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    api_key = Session.api_key,
                    dpp_shelf_name = dpp_shelf_name
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await api_client.PostAsync(Session.get_smart_shelf_setting, content);

                if (result.IsSuccessStatusCode)
                {

                    string resultContent = await result.Content.ReadAsStringAsync();
                    JObject JsonData = JObject.Parse(resultContent);

                    foreach (var item in JsonData["data"])
                    {
                        int col = Int32.Parse(item["dpp_shelf_col_pos"].ToString());
                        int row = Int32.Parse(item["dpp_shelf_pos"].ToString());
                        ProductPos data = new ProductPos
                        {
                            Jancode = (string)item["dpp_jan_cd"],
                            RFIDcode = (string)item["dpp_rfid_cd"],
                            shelf_col_pos = col.ToString(),
                            shelf_pos = row.ToString(),
                            product_name = (string)item["dpp_product_name"],
                            shelf_name = (string)item["dpp_shelf_name"],
                            isbn = (string)item["dpp_isbn"],
                            link_image = (string)item["dpp_image_url"]
                        };

                        string name = Session.positionPos.FirstOrDefault(x => x.Value.col == col && x.Value.row == row).Key;
                        Session.productPos[name] = data;
                    }

                    api_message = (string)JsonData["message"];
                    api_status = (string)JsonData["code"];
                }
                else
                {
                    Console.WriteLine(result);
                }

            }
            catch (Exception)
            {
                Console.WriteLine("Failed to get Smart Self setting \n");
            }

        }

        private async Task ApiClearRawData()
        {
            try
            {
                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Session.address_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    api_key = Session.api_key
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await api_client.PostAsync(Session.sub_clear_raw_data, content);

                if (result.IsSuccessStatusCode)
                {
                    string resultContent = await result.Content.ReadAsStringAsync();
                    JObject JsonData = JObject.Parse(resultContent);
                    api_message = (string)JsonData["message"];
                    api_status = (string)JsonData["code"];
                }
                else
                {
                    Console.WriteLine(result);
                }

            }
            catch (Exception)
            {
                Console.WriteLine("Failed to clear raw data \n");
            }

        }

        private async Task ApiGetSmartShelfLocation(string dpp_shelf_name)
        {
            try
            {

                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Session.address_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    api_key = Session.api_key,
                    shelf_no = dpp_shelf_name
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await api_client.PostAsync(Session.get_smartshelf_location, content);

                if (result.IsSuccessStatusCode)
                {

                    string resultContent = await result.Content.ReadAsStringAsync();
                    JObject JsonData = JObject.Parse(resultContent);

                    foreach (var item in JsonData["data"])
                    {
                        int col = Int32.Parse(item["col"].ToString());
                        int row = Int32.Parse(item["row"].ToString());
                        ProductPos data = new ProductPos
                        {

                            Jancode = (string)item["jancode"],
                            RFIDcode = (string)item["EPC"],
                            shelf_col_pos = col.ToString(),
                            shelf_pos = row.ToString(),
                            product_name = (string)item["product_name"],
                            link_image = (string)item["link_image"]
                        };

                        string name = Session.positionPos.FirstOrDefault(x => x.Value.col == col && x.Value.row == row).Key;
                        Session.productPos[name] = data;
                    }

                    api_message = (string)JsonData["message"];
                    api_status = (string)JsonData["code"];
                }
                else
                {
                    Console.WriteLine(result);
                }

            }
            catch (Exception)
            {
                Console.WriteLine("Failed to get Smart Self location \n");
            }

        }

        private async Task<JObject> ApiGetSmartShelfLocationByCol(string dpp_shelf_name, int col, int row)
        {
            try
            {
                
                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Session.address_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    api_key = Session.api_key,
                    shelf_no = dpp_shelf_name,
                    col = col,
                    row = row
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await api_client.PostAsync(Session.get_smartshelf_location_by_col, content);

                if (result.IsSuccessStatusCode)
                {

                    string resultContent = await result.Content.ReadAsStringAsync();
                    JObject JsonData = JObject.Parse(resultContent);

                    api_message = (string)JsonData["message"];
                    api_status = (string)JsonData["code"];
                    return JsonData;
                }
                
                else
                {
                    Console.WriteLine(result);
                    return null;
                }

            }
            catch (Exception)
            {
                Console.WriteLine("Failed to get ApiGetSmartShelfLocationByCol \n");
                return null;
            }

        }


        private async Task ApiGetSmartShelfStatus(string name, string rfid)
        {
            try
            {
                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Session.address_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    api_key = Session.api_key,
                    rfid = rfid
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await api_client.PostAsync(Session.rfid_to_status_smartshelf, content);


                if (result.IsSuccessStatusCode)
                {

                    string resultContent = await result.Content.ReadAsStringAsync();
                    JObject JsonData = JObject.Parse(resultContent);

                    api_status = (string)JsonData["code"];

                    Session.productPos[name].status = api_status;
                    api_message = (string)JsonData["message"];

                }
                else
                {
                    Console.WriteLine(result);
                }

            }
            catch (Exception)
            {
                Console.WriteLine("Failed to get smart shelf status - ApiGetSmartShelfStatus\n");
            }

        }

        private async Task ApiGetSmartShelfNames()
        {
            try
            {
                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Session.address_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    api_key = Session.api_key
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await api_client.PostAsync(Session.get_smart_shelf_names, content);

                if (result.IsSuccessStatusCode)
                {

                    string resultContent = await result.Content.ReadAsStringAsync();
                    JObject JsonData = JObject.Parse(resultContent);

                    foreach (var item in JsonData["data"])
                    {
                        Session.smart_shelf_names.Add(item.ToString());
                    }
                    //Session.smart_shelf_names.ForEach(x => Console.WriteLine(x.ToString()));

                    api_message = (string)JsonData["message"];
                    api_status = (string)JsonData["code"];
                }
                else
                {
                    Console.WriteLine(result);


                }
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to get smart shelf names - ApiGetSmartShelfNames\n");
            }

        }



        // Search Jan1 from RFID
        private async Task ApiRFIDtoJan()
        {

            try
            {
                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Session.address_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var rfids = new string[] { Session.rfidcode };
                string json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    api_key = Session.api_key,
                    rfid = rfids

                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await api_client.PostAsync(Session.rfmaster_sub_rfids_to_jans, content);

                if (result.IsSuccessStatusCode)
                {

                    string resultContent = await result.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(resultContent);
                    Console.WriteLine(resultContent);
                    api_message = (string)data["message"];
                    api_status = (string)data["code"];
                    Session.barcode = (string)data["data"][0]["jancode_1"];
                } 
                else
                {
                    Console.WriteLine(result);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Connect to API Server Failed - ApiRFIDtoJan");
            }
        }

        private async Task<string> ApiRFIDtoJan_Scan(string rfids)
        {

            try
            {
                var array_rfids = new string[] { rfids };
                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Session.address_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    api_key = Session.api_key,
                    rfid = array_rfids

                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await api_client.PostAsync(Session.rfmaster_sub_rfids_to_jans, content);

                if (result.IsSuccessStatusCode)
                {

                    string resultContent = await result.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(resultContent);
                    Console.WriteLine(resultContent);
                    api_message = (string)data["message"];
                    api_status = (string)data["code"];
                    string jan_result = (string)data["data"][0]["jancode_1"];
                    return jan_result;
                }
                else
                {
                    Console.WriteLine(result);
                    return "";
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Connect to API Server Failed.");
                return "";
            }
        }

        private async Task ApiRFIDtoJan_Sync()
        {

            try
            {
                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Session.address_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //var arrayOfAllKeys = Session.productPos.Keys.ToArray();

                List<string> rfids = new List<string>();

                foreach (string key in Session.productPos.Keys)
                {
                    if (Session.productPos[key].RFIDcode != "")
                    {
                        rfids.Add(Session.productPos[key].RFIDcode);
                    
                    }
                }

                string json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    api_key = Session.api_key,
                    rfid = rfids,
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await api_client.PostAsync(Session.rfmaster_sub_rfids_to_jans, content);

                if (result.IsSuccessStatusCode)
                {

                    string resultContent = await result.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(resultContent);
                    Console.WriteLine(resultContent);
                    api_message = (string)data["message"];
                    api_status = (string)data["code"];

            
                    foreach (var item in data["data"])
                    {
                        //Handle here
                        foreach (string key in Session.productPos.Keys)
                        {
                            if (Session.productPos[key].RFIDcode == (string)item["rfid"])
                            {
                                Session.productPos[key].Jancode = (string)item["jancode_1"];
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine(result);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Connect to API Server Failed.");
            }
        }


        private Dictionary<string, string> getDictionaryConfig(string path)
        {
            Dictionary<string, string> Config = new Dictionary<string, string>();
            List<string> result = read_Fileini(path);
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
        private List<string> read_Fileini(string path)
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



        public string rfid_cd;
        public string jan_cd;
        private void Front_KeyDown(object sender, KeyEventArgs e)
        {

            //Session.rfidcode= "30838EFB95E27C028FA6AF78";
            //updateView();

            //if (e.KeyCode == Keys.Return && barcode_state)
            //{
            //    barcode = "";
            //    rfid_reading = false;

            //    Wait wait = new Wait();

            //    //Clear after test
            //    //wait.Visible = true;
            //    //Task.Run(() => ApiRFIDtoJan()).Wait();
            //    //Task.Run(() => ApiGetDataFromBQ()).Wait();
            //    //wait.Visible = false;
            //    //Task.Run(() => ApiGetImage()).Wait();
            //    //updateView();



            //    if (!Session.barcode.Equals("") && !Session.rfidcode.Equals(""))
            //    {

            //        rfid_cd = Session.rfidcode;
            //        jan_cd = Session.barcode;

            //    }
            //    rfid_reading = true;
            //}


        }

        private void Front_KeyPress(object sender, KeyPressEventArgs e)
        {
            //if (barcode_state)
            //{
            //    if (Char.IsDigit(e.KeyChar))
            //    {
            //        barcode += e.KeyChar;
            //    }
            //    if (barcode.Length == Session.JanLen)
            //    {
            //        if (barcode.Substring(0, 3).Equals("192") || barcode.Substring(0, 3).Equals("191"))
            //        {
            //            barcode = "";
            //            Session.barcode = "";
            //        }
            //        else
            //        {
            //            Session.barcode = barcode;

            //            barcode = "";
            //        }

            //        updateView();
            //    }
            //}
        }



        private void btnConnect_Click(object sender, EventArgs e)
        {
            btnConnect.Enabled = false;
            switch (btnConnect.Text)
            {
                case "CONNECT":
                    Session.OPOSRFID1.CreateControl();
                    int n = opos.OPOS_EnableDevice(Session.OPOSRFID1);
                    if (n == -1)
                    {
                        opos.setAtenaPower(Session.OPOSRFID1, Session.antenaPower);
                        opos.OPOS_StartReading(Session.OPOSRFID1);
                        //opos.OPOS_readSingleTag(Session.OPOSRFID1);
                        btnConnect.Text = "StopReading";
                        messageFromApp.Text += DateTime.Now.ToString("hh:mm:ss") + ": Connected to Device\n";
                    }
                    else
                    {
                        switch (n)
                        {
                            case 0:
                                messageFromApp.Text += DateTime.Now.ToString("hh:mm:ss") + ": Connect to Device Failed \n";
                                break;
                            case 1:
                                messageFromApp.Text += DateTime.Now.ToString("hh:mm:ss") + ": Claim Device Failed\n";
                                break;
                            case 2:
                                messageFromApp.Text += DateTime.Now.ToString("hh:mm:ss") + ": Enable Device Failed\n";
                                break;
                            default:
                                break;
                        };
                    }
                    break;
                case "StopReading":
                    opos.OPOS_StopReading(Session.OPOSRFID1);
                    //opos.OPOS_reset(Session.OPOSRFID1);
                    btnConnect.Text = "StartReading";
                    messageFromApp.Text += DateTime.Now.ToString("hh:mm:ss") + ": Device stopped Reading \n";
                    //resetLabel(1);
                    break;
                case "StartReading":
                    //opos.OPOS_readSingleTag(Session.OPOSRFID1);
                    opos.OPOS_StartReading(Session.OPOSRFID1);
                    btnConnect.Text = "StopReading";
                    messageFromApp.Text += DateTime.Now.ToString("hh:mm:ss") + ": Device started Reading \n";
                    //resetLabel(1);
                    break;
                default:
                    break;
            }
            btnConnect.Enabled = true;
        }
         


        private void btnClear_Click(object sender, EventArgs e)
        {
            DialogResult warningPopUp = MessageBox.Show("All data added, deleted, edited in the interface will be removed, are you sure?", "確認ダイアログ", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (warningPopUp == DialogResult.Yes)
            {
                cbShelf.Text = "";
                messageFromApp.Text = "";
                btnCheck.Text = "CHECK";
                checkTimer.Stop();
                btnCheck.BackColor = Color.RoyalBlue;
                btnLoad.BackColor = Color.RoyalBlue;
                resetLabel(1);

                if (btnConnect.Text == "StopReading") { 
                Task.Run(() => ApiRFIDtoJan()).Wait();
                Task.Run(() => ApiGetDataFromBQ()).Wait();
                Task.Run(() => ApiGetImage()).Wait();
                updateView();
                }
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }







        private void txtRfid_TextChanged(object sender, EventArgs e)
        {


            if (txtRfid.Text != rfid_cd) { 
            Wait wait = new Wait();
            wait.Visible = true;
            Task.Run(() => ApiRFIDtoJan()).Wait();
            Task.Run(() => ApiGetDataFromBQ()).Wait();
            wait.Visible = false;
            //Fix 1110
            //Task.Run(() => ApiGetImage()).Wait();
            

            //NEW
            string image = "";
            Task<string> result = Task.Run(() => ApiGetImageLocal_ForGrid(Session.barcode));
            image = result.Result;
            string temp;
            temp = image == null ? "noimage.png" : image;

            if (temp == "")
            {
                temp = "noimage.png";
            }

             Session.product.link_image = temp;





                if (CheckValidUrl(temp))
            {
                pictureBox.Load(temp);
            }
            else
            {
                Image base64_convert = LoadImage(temp);
                Bitmap objBitmap = new Bitmap(base64_convert, new Size(220, 300));
                var bmp = (Bitmap)base64_convert;
                pictureBox.Image = objBitmap;

                }





                updateView();

            } else
            {
                Console.WriteLine("Duplicate");
            }


        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
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
                this.media_name = "";
                this.artist_name = "";
                this.genreCD = "";
                this.ccode = "";
                price = 0;
                this.goods_name = "";
                this.rfid_goods_type = "";
                this.isbn = "";
                this.link_image = "";
            }
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

                this.shelf_pos = "";
                this.shelf_col_pos = "";
                this.isbn = "";
                this.product_name = "";
                this.shelf_name = "";
                this.link_image = "";
                this.status = "";
                this.picture_box = null;
            }
        }


        public class SettingAntena
        {
            public int row { get; set; }
            public int col { get; set; }
        }


        public class OPOS : System.Windows.Forms.UserControl
        {

            public OPOS()
            {
                Session.OPOSRFID1.DataEvent += OPOSRFID1_DataEvent;
                Session.OPOSRFID1.ErrorEvent += OPOSRFID1_ErrorEvent;
            }

            private void OPOSRFID1_ErrorEvent(Object sender, _DOPOSRFIDEvents_ErrorEventEvent e)
            {

            }


            private void OPOSRFID1_DataEvent(object sender, _DOPOSRFIDEvents_DataEventEvent e)
            {

                int TagCount;
                string UserData;

                TagCount = Session.OPOSRFID1.TagCount;


                for (int i = 0; i < TagCount; i++)
                {
                    UserData = " Userdata=" + Session.OPOSRFID1.CurrentTagUserData;

                    if (UserData == " Userdata=")
                    {
                        UserData = "";
                    }
                    var code_value = Session.OPOSRFID1.CurrentTagID + UserData;
                    string new_code = ConvertTagIDCode(code_value);
                    //Console.WriteLine(new_code);
                    if (!Session.rfidcode.Equals(new_code) && Session.front.rfid_reading)
                    {
                        Session.rfidcode = new_code;
                        Session.front.updateView();
                    }

                    Session.OPOSRFID1.NextTag();

                }


                Session.OPOSRFID1.DataEventEnabled = true;
            }

            //Enable Device
            public int OPOS_EnableDevice(AxOPOSRFID OPOSRFID1)
            {
                int Result;
                int phase;
                string strData;

                // Open Device
                string device_name = Session.device_name;
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
            //disable device
            private void OPOS_DisableDevice(AxOPOSRFID OPOSRFID1)
            {
            }

            //Scanning
            public void OPOS_StartReading(AxOPOSRFID OPOSRFID1)
            {
                int Result;
                //OPOSRFID1.ClearInputProperties();
                OPOSRFID1.ReadTimerInterval = Session.rT;
                OPOSRFID1.DataEventEnabled = true;

                if (Session.OPOSRFID1.TagCount > 0)
                {

                    Session.OPOSRFID1.ClearInputProperties();
                }

                PhaseChange(OPOSRFID1);
                Result = OPOSRFID1.StartReadTags(OposStatus.RfidRtId, "000000000000000000000000", "000000000000000000000000", 0, 0, 1000, "00000000");
                if (Result != OposStatus.OposSuccess)
                {
                    Console.WriteLine("read err");
                }

                //OPOSRFID1.DataEventEnabled = true;
                //Session.isReading = true;
            }

            //Set phase lenght of code
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

            //Stop read
            public void OPOS_StopReading(AxOPOSRFID OPOSRFID1)
            {
                int Result;
                Result = OPOSRFID1.StopReadTags("00000000");
                if (Result != OposStatus.OposSuccess)
                {
                    Console.WriteLine("Err Stop");
                }
            }



            //Converte List
            private string ConvertTagIDCode(string code_value)
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


            //Sendkeys to WEBPOS


            public void OPOS_Connector(AxOPOSRFID OPOSRFID1)
            {
                OPOS_EnableDevice(OPOSRFID1);
            }


            //private DataEvent

            private void InitializeComponent()
            {
                this.SuspendLayout();
                this.Name = "OPOS";
                this.Load += new System.EventHandler(this.OPOS_Load);
                this.ResumeLayout(false);
            }

            private void OPOS_Load(object sender, EventArgs e)
            {

            }



            public static void stopReading(AxOPOSRFID OPOSRFID1)
            {
                //Session.isReading = false;
                int Result;
                Result = OPOSRFID1.StopReadTags("00000000");
                if (Result != OposStatus.OposSuccess)
                {
                    Console.WriteLine("Err Stop");
                }
            }

            public void OPOS_readSingleTag(AxOPOSRFID OPOSRFID1)
            {
                //Session.isReading = false;

                OPOSRFID1.DataEventEnabled = true;

                PhaseChange(OPOSRFID1);
                if (Session.OPOSRFID1.TagCount > 0)
                {
                    Session.OPOSRFID1.ClearInputProperties();
                }

                OPOSRFID1.ReadTags(OposStatus.RfidRtId, "000000000000000000000000", "000000000000000000000000", 0, 0, 30000, "00000000");

            }

            public void OPOS_reset(AxOPOSRFID OPOSRFID1)
            {
                OPOSRFID1.DeviceEnabled = false;
                OPOSRFID1.DeviceEnabled = true;
            }

            // New range for power
            public void setAtenaPower(AxOPOSRFID OPOSRFID1, string strCSV)
            {
                //string strCSV = "";
                int lData;

                int lRet;
                OPOSRFID1.BinaryConversion = OposStatus.OposBcNone;

                lData = 2;
                lRet = OPOSRFID1.DirectIO(120, ref lData, ref strCSV);


                if (lRet != OposStatus.OposSuccess)
                {
                    Console.WriteLine("Setting atena power is failed!");
                }

                OPOSRFID1.BinaryConversion = OposStatus.OposBcNibble;
            }





        }
        public class OposStatus
        {
            public const int OposSuccess = 0;
            public const int OposEClosed = 101;
            public const int OposEClaimed = 102;
            public const int OposENotclaimed = 103;
            public const int OposENoservice = 104;
            public const int OposEDisabled = 105;
            public const int OposEIllegal = 106;
            public const int OposENohardware = 107;
            public const int OposEOffline = 108;
            public const int OposENoexist = 109;
            public const int OposEExists = 110;
            public const int OposEFailure = 111;
            public const int OposETimeout = 112;
            public const int OposEBusy = 113;
            public const int OposEExtended = 114;

            [System.Runtime.InteropServices.DllImport("AxInterop.OPOSRFIDLib.dll")]
            public static extern int Open(string deviceName);

            //[System.Runtime.InteropServices.DllImport("Interop.OPOSRFIDLib.dll")]
            //public static extern string Amethod(string s);

            // *///////////////////////////////////////////////////////////////////
            // * OPOS "OpenResult" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int Oposopenerr = 300;

            public const int OposOrAlreadyopen = 301;
            public const int OposOrRegbadname = 302;
            public const int OposOrRegprogid = 303;
            public const int OposOrCreate = 304;
            public const int OposOrBadif = 305;
            public const int OposOrFailedopen = 306;
            public const int OposOrBadversion = 307;

            public const int Oposopenerrso = 400;

            public const short OposOrsNoport = 401;
            public const short OposOrsNotsupported = 402;
            public const short OposOrsConfig = 403;
            public const short OposOrsSpecific = 450;


            // *///////////////////////////////////////////////////////////////////
            // * OPOS "BinaryConversion" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int OposBcNone = 0;
            public const int OposBcNibble = 1;
            public const int OposBcDecimal = 2;


            // *///////////////////////////////////////////////////////////////////
            // * "CheckHealth" Method: "Level" Parameter Constants
            // *///////////////////////////////////////////////////////////////////

            public const int OposChInternal = 1;
            public const int OposChExternal = 2;
            public const int OposChInteractive = 3;


            // *///////////////////////////////////////////////////////////////////
            // * OPOS "CapPowerReporting", "PowerState", "PowerNotify" Property
            // *   Constants
            // *///////////////////////////////////////////////////////////////////

            public const int OposPrNone = 0;
            public const int OposPrStandard = 1;
            public const int OposPrAdvanced = 2;

            public const int OposPnDisabled = 0;
            public const int OposPnEnabled = 1;

            public const int OposPsUnknown = 2000;
            public const int OposPsOnline = 2001;
            public const int OposPsOff = 2002;
            public const int OposPsOffline = 2003;
            public const int OposPsOffOffline = 2004;


            // *///////////////////////////////////////////////////////////////////
            // * "ErrorEvent" Event: "ErrorLocus" Parameter Constants
            // *///////////////////////////////////////////////////////////////////

            public const int OposElOutput = 1;
            public const int OposElInput = 2;
            public const int OposElInputData = 3;


            // *///////////////////////////////////////////////////////////////////
            // * "ErrorEvent" Event: "ErrorResponse" Constants
            // *///////////////////////////////////////////////////////////////////

            public const int OposErRetry = 11;
            public const int OposErClear = 12;
            public const int OposErContinueinput = 13;


            // *///////////////////////////////////////////////////////////////////
            // * "StatusUpdateEvent" Event: Common "Status" Constants
            // *///////////////////////////////////////////////////////////////////

            public const int OposSuePowerOnline = 2001;
            public const int OposSuePowerOff = 2002;
            public const int OposSuePowerOffline = 2003;
            public const int OposSuePowerOffOffline = 2004;


            // *///////////////////////////////////////////////////////////////////
            // * General Constants
            // *///////////////////////////////////////////////////////////////////

            public const int OposForever = -1;



            // *///////////////////////////////////////////////////////////////////
            // *
            // * OposBb.h
            // *
            // *   Bump Bar header file for OPOS Applications.
            // *
            // * Modification history
            // * ------------------------------------------------------------------
            // * 98-03-06 OPOS Release 1.3                                     BB
            // *
            // *///////////////////////////////////////////////////////////////////


            // *///////////////////////////////////////////////////////////////////
            // * "CurrentUnitID" and "UnitsOnline" Properties
            // *  and "Units" Parameter Constants
            // *///////////////////////////////////////////////////////////////////

            public const int BbUid1 = 0x1;
            public const int BbUid2 = 0x2;
            public const int BbUid3 = 0x4;
            public const int BbUid4 = 0x8;
            public const int BbUid5 = 0x10;
            public const int BbUid6 = 0x20;
            public const int BbUid7 = 0x40;
            public const int BbUid8 = 0x80;
            public const int BbUid9 = 0x100;
            public const int BbUid10 = 0x200;
            public const int BbUid11 = 0x400;
            public const int BbUid12 = 0x800;
            public const int BbUid13 = 0x1000;
            public const int BbUid14 = 0x2000;
            public const int BbUid15 = 0x4000;
            public const int BbUid16 = 0x8000;
            public const int BbUid17 = 0x10000;
            public const int BbUid18 = 0x20000;
            public const int BbUid19 = 0x40000;
            public const int BbUid20 = 0x80000;
            public const int BbUid21 = 0x100000;
            public const int BbUid22 = 0x200000;
            public const int BbUid23 = 0x400000;
            public const int BbUid24 = 0x800000;
            public const int BbUid25 = 0x1000000;
            public const int BbUid26 = 0x2000000;
            public const int BbUid27 = 0x4000000;
            public const int BbUid28 = 0x8000000;
            public const int BbUid29 = 0x10000000;
            public const int BbUid30 = 0x20000000;
            public const int BbUid31 = 0x40000000;
            public const int BbUid32 = int.MinValue + 0x00000000;


            // *///////////////////////////////////////////////////////////////////
            // * "DataEvent" Event: "Status" Parameter Constants
            // *///////////////////////////////////////////////////////////////////

            public const int BbDeKey = 0x1;



            // *///////////////////////////////////////////////////////////////////
            // *
            // * OposCash.h
            // *
            // *   Cash Drawer header file for OPOS Applications.
            // *
            // * Modification history
            // * ------------------------------------------------------------------
            // * 95-12-08 OPOS Release 1.0                                     CRM
            // * 98-03-06 OPOS Release 1.3                                     CRM
            // *
            // *///////////////////////////////////////////////////////////////////


            // *///////////////////////////////////////////////////////////////////
            // * "StatusUpdateEvent" Event Constants
            // *///////////////////////////////////////////////////////////////////

            public const int CashSueDrawerclosed = 0;
            public const int CashSueDraweropen = 1;



            // *///////////////////////////////////////////////////////////////////
            // *
            // * OposCAT.h
            // *
            // *   CAT header file for OPOS Applications.
            // *
            // * Modification history
            // * ------------------------------------------------------------------
            // * 98-09-23 OPOS Release 1.4                                 OPOS-J
            // * 00-09-24 OPOS Release 1.5                                    BKS
            // *   Add CAT_PAYMENT_DEBIT, CAT_MEDIA_UNSPECIFIED,
            // *   CAT_MEDIA_NONDEFINE, CAT_MEDIA_CREDIT, CAT_MEDIA_DEBIT
            // *
            // *///////////////////////////////////////////////////////////////////


            // *///////////////////////////////////////////////////////////////////
            // * Payment Condition Constants
            // *///////////////////////////////////////////////////////////////////

            public const int CatPaymentLump = 10;
            public const int CatPaymentBonus1 = 21;
            public const int CatPaymentBonus2 = 22;
            public const int CatPaymentBonus3 = 23;
            public const int CatPaymentBonus4 = 24;
            public const int CatPaymentBonus5 = 25;
            public const int CatPaymentInstallment1 = 61;
            public const int CatPaymentInstallment2 = 62;
            public const int CatPaymentInstallment3 = 63;
            public const int CatPaymentBonusCombination1 = 31;
            public const int CatPaymentBonusCombination2 = 32;
            public const int CatPaymentBonusCombination3 = 33;
            public const int CatPaymentBonusCombination4 = 34;
            public const int CatPaymentRevolving = 80;
            public const int CatPaymentDebit = 110;


            // *///////////////////////////////////////////////////////////////////
            // * Transaction Type Constants
            // *///////////////////////////////////////////////////////////////////

            public const int CatTransactionSales = 10;
            public const int CatTransactionVoid = 20;
            public const int CatTransactionRefund = 21;
            public const int CatTransactionVoidpresales = 29;
            public const int CatTransactionCompletion = 30;
            public const int CatTransactionPresales = 40;
            public const int CatTransactionCheckcard = 41;


            // *///////////////////////////////////////////////////////////////////
            // * "PaymentMedia" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int CatMediaUnspecified = 0;
            public const int CatMediaNondefine = 0;
            public const int CatMediaCredit = 1;
            public const int CatMediaDebit = 2;


            // *///////////////////////////////////////////////////////////////////
            // * ResultCodeExtended Constants
            // *///////////////////////////////////////////////////////////////////

            public const int OposEcatCentererror = 1;
            public const int OposEcatCommanderror = 90;
            public const int OposEcatReset = 91;
            public const int OposEcatCommunicationerror = 92;
            public const int OposEcatDailylogoverflow = 200;


            // *///////////////////////////////////////////////////////////////////
            // * "Daily Log" Property  & Argument Constants
            // *///////////////////////////////////////////////////////////////////

            public const int CatDlNone = 0; // None of them
            public const int CatDlReporting = 1; // Only Reporting
            public const int CatDlSettlement = 2; // Only Settlement
            public const int CatDlReportingSettlement = 3; // Both of them



            // *///////////////////////////////////////////////////////////////////
            // *
            // * OposChan.h
            // *
            // *   Cash Changer header file for OPOS Applications.
            // *
            // * Modification history
            // * ------------------------------------------------------------------
            // * 97-06-04 OPOS Release 1.2                                     CRM
            // * 00-09-24 OPOS Release 1.5                                  OPOS-J
            // *   Add DepositStatus Constants.
            // *   Add EndDeposit Constants.
            // *   Add PauseDeposit Constants.
            // *
            // *///////////////////////////////////////////////////////////////////


            // *///////////////////////////////////////////////////////////////////
            // * "DeviceStatus" and "FullStatus" Property Constants
            // * "StatusUpdateEvent" Event Constants
            // *///////////////////////////////////////////////////////////////////

            public const int ChanStatusOk = 0; // DeviceStatus, FullStatus

            public const int ChanStatusEmpty = 11; // DeviceStatus, StatusUpdateEvent
            public const int ChanStatusNearempty = 12; // DeviceStatus, StatusUpdateEvent
            public const int ChanStatusEmptyok = 13; // StatusUpdateEvent

            public const int ChanStatusFull = 21; // FullStatus, StatusUpdateEvent
            public const int ChanStatusNearfull = 22; // FullStatus, StatusUpdateEvent
            public const int ChanStatusFullok = 23; // StatusUpdateEvent

            public const int ChanStatusJam = 31; // DeviceStatus, StatusUpdateEvent
            public const int ChanStatusJamok = 32; // StatusUpdateEvent

            public const int ChanStatusAsync = 91; // StatusUpdateEvent


            // *///////////////////////////////////////////////////////////////////
            // * "DepositStatus" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int ChanStatusDepositStart = 1;
            public const int ChanStatusDepositEnd = 2;
            public const int ChanStatusDepositNone = 3;
            public const int ChanStatusDepositCount = 4;
            public const int ChanStatusDepositJam = 5;


            // *///////////////////////////////////////////////////////////////////
            // * "EndDeposit" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int ChanDepositChange = 1;
            public const int ChanDepositNochange = 2;
            public const int ChanDepositrepay = 3;


            // *///////////////////////////////////////////////////////////////////
            // * "PauseDeposit" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int ChanDepositPause = 1;
            public const int ChanDepositRestart = 2;


            // *///////////////////////////////////////////////////////////////////
            // * "ResultCodeExtended" Property Constants for Cash Changer
            // *///////////////////////////////////////////////////////////////////

            public const int OposEchanOverdispense = 201;



            // *///////////////////////////////////////////////////////////////////
            // *
            // * OposCoin.h
            // *
            // *   Coin Dispenser header file for OPOS Applications.
            // *
            // * Modification history
            // * ------------------------------------------------------------------
            // * 95-12-08 OPOS Release 1.0                                     CRM
            // *
            // *///////////////////////////////////////////////////////////////////


            // *///////////////////////////////////////////////////////////////////
            // * "DispenserStatus" Property Constants
            // * "StatusUpdateEvent" Event: "Data" Parameter Constants
            // *///////////////////////////////////////////////////////////////////

            public const int CoinStatusOk = 1;
            public const int CoinStatusEmpty = 2;
            public const int CoinStatusNearempty = 3;
            public const int CoinStatusJam = 4;



            // *///////////////////////////////////////////////////////////////////
            // *
            // * OposDisp.h
            // *
            // *   Line Display header file for OPOS Applications.
            // *
            // * Modification history
            // * ------------------------------------------------------------------
            // * 95-12-08 OPOS Release 1.0                                     CRM
            // * 96-03-18 OPOS Release 1.01                                    CRM
            // *   Add DISP_MT_INIT constant and MarqueeFormat constants.
            // * 96-04-22 OPOS Release 1.1                                     CRM
            // *   Add CapCharacterSet values for Kana and Kanji.
            // * 00-09-24 OPOS Release 1.5                                     BKS
            // *   Add CapCharacterSet and CharacterSet constants for UNICODE
            // * 01-07-15 OPOS Release 1.6                                     BKS
            // *   Add CapCursorType, CapCustomGlyph, CapReadBack, CapReverse,
            // *     CursorType property constants.
            // *   Add DefineGlyph, DisplayText and DisplayTextAt parameter
            // *     constants.
            // *
            // *///////////////////////////////////////////////////////////////////


            // *///////////////////////////////////////////////////////////////////
            // * "CapBlink" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int DispCbNoblink = 0;
            public const int DispCbBlinkall = 1;
            public const int DispCbBlinkeach = 2;


            // *///////////////////////////////////////////////////////////////////
            // * "CapCharacterSet" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int DispCcsNumeric = 0;
            public const int DispCcsAlpha = 1;
            public const int DispCcsAscii = 998;
            public const int DispCcsKana = 10;
            public const int DispCcsKanji = 11;
            public const int DispCcsUnicode = 997;


            // *///////////////////////////////////////////////////////////////////
            // * "CapCursorType" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int DispCctNone = 0x0;
            public const int DispCctFixed = 0x1;
            public const int DispCctBlock = 0x2;
            public const int DispCctHalfblock = 0x4;
            public const int DispCctUnderline = 0x8;
            public const int DispCctReverse = 0x10;
            public const int DispCctOther = 0x20;


            // *///////////////////////////////////////////////////////////////////
            // * "CapReadBack" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int DispCrbNone = 0x0;
            public const int DispCrbSingle = 0x1;


            // *///////////////////////////////////////////////////////////////////
            // * "CapReverse" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int DispCrNone = 0x0;
            public const int DispCrReverseall = 0x1;
            public const int DispCrReverseeach = 0x2;


            // *///////////////////////////////////////////////////////////////////
            // * "CharacterSet" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int DispCsUnicode = 997;
            public const int DispCsAscii = 998;
            public const int DispCsWindows = 999;


            // *///////////////////////////////////////////////////////////////////
            // * "CursorType" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int DispCtNone = 0;
            public const int DispCtFixed = 1;
            public const int DispCtBlock = 2;
            public const int DispCtHalfblock = 3;
            public const int DispCtUnderline = 4;
            public const int DispCtReverse = 5;
            public const int DispCtOther = 6;


            // *///////////////////////////////////////////////////////////////////
            // * "MarqueeType" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int DispMtNone = 0;
            public const int DispMtUp = 1;
            public const int DispMtDown = 2;
            public const int DispMtLeft = 3;
            public const int DispMtRight = 4;
            public const int DispMtInit = 5;


            // *///////////////////////////////////////////////////////////////////
            // * "MarqueeFormat" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int DispMfWalk = 0;
            public const int DispMfPlace = 1;


            // *///////////////////////////////////////////////////////////////////
            // * "DefineGlyph" Method: "GlyphType" Parameter Constants
            // *///////////////////////////////////////////////////////////////////

            public const int DispGtSingle = 1;


            // *///////////////////////////////////////////////////////////////////
            // * "DisplayText" Method: "Attribute" Property Constants
            // * "DisplayTextAt" Method: "Attribute" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int DispDtNormal = 0;
            public const int DispDtBlink = 1;
            public const int DispDtReverse = 2;
            public const int DispDtBlinkReverse = 3;


            // *///////////////////////////////////////////////////////////////////
            // * "ScrollText" Method: "Direction" Parameter Constants
            // *///////////////////////////////////////////////////////////////////

            public const int DispStUp = 1;
            public const int DispStDown = 2;
            public const int DispStLeft = 3;
            public const int DispStRight = 4;


            // *///////////////////////////////////////////////////////////////////
            // * "SetDescriptor" Method: "Attribute" Parameter Constants
            // *///////////////////////////////////////////////////////////////////

            public const int DispSdOff = 0;
            public const int DispSdOn = 1;
            public const int DispSdBlink = 2;



            // *///////////////////////////////////////////////////////////////////
            // *
            // * OposFptr.h
            // *
            // *   Fiscal Printer header file for OPOS Applications.
            // *
            // * Modification history
            // * ------------------------------------------------------------------
            // * 98-03-06 OPOS Release 1.3                                     PDU
            // * 00-09-24 OPOS Release 1.5                                     BKS
            // *   Change CountryCode constants and add code for Russia
            // * 01-07-15 OPOS Release 1.6                                     THH
            // *   Add values for all 1.6 added properties and method
            // *   parameters
            // *
            // *///////////////////////////////////////////////////////////////////


            // *///////////////////////////////////////////////////////////////////
            // * "ActualCurrency" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int FptrAcBrc = 1;
            public const int FptrAcBgl = 2;
            public const int FptrAcEur = 3;
            public const int FptrAcGrd = 4;
            public const int FptrAcHuf = 5;
            public const int FptrAcItl = 6;
            public const int FptrAcPlz = 7;
            public const int FptrAcRol = 8;
            public const int FptrAcRur = 9;
            public const int FptrAcTrl = 10;


            // *///////////////////////////////////////////////////////////////////
            // * "ContractorId" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int FptrCidFirst = 1;
            public const int FptrCidSecond = 2;
            public const int FptrCidSingle = 3;


            // *///////////////////////////////////////////////////////////////////
            // * Fiscal Printer Station Constants
            // *///////////////////////////////////////////////////////////////////

            public const int FptrSJournal = 1;
            public const int FptrSReceipt = 2;
            public const int FptrSSlip = 4;

            public const int FptrSJournalReceipt = FptrSJournal | FptrSReceipt;


            // *///////////////////////////////////////////////////////////////////
            // * "CountryCode" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int FptrCcBrazil = 1;
            public const int FptrCcGreece = 2;
            public const int FptrCcHungary = 4;
            public const int FptrCcItaly = 8;
            public const int FptrCcPoland = 16;
            public const int FptrCcTurkey = 32;
            public const int FptrCcRussia = 64;
            public const int FptrCcBulgaria = 128;
            public const int FptrCcRomania = 256;


            // *///////////////////////////////////////////////////////////////////
            // * "DateType" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int FptrDtConf = 1;
            public const int FptrDtEod = 2;
            public const int FptrDtReset = 3;
            public const int FptrDtRtc = 4;
            public const int FptrDtVat = 5;


            // *///////////////////////////////////////////////////////////////////
            // * "ErrorLevel" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int FptrElNone = 1;
            public const int FptrElRecoverable = 2;
            public const int FptrElFatal = 3;
            public const int FptrElBlocked = 4;


            // *///////////////////////////////////////////////////////////////////
            // * "ErrorState", "PrinterState" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int FptrPsMonitor = 1;
            public const int FptrPsFiscalReceipt = 2;
            public const int FptrPsFiscalReceiptTotal = 3;
            public const int FptrPsFiscalReceiptEnding = 4;
            public const int FptrPsFiscalDocument = 5;
            public const int FptrPsFixedOutput = 6;
            public const int FptrPsItemList = 7;
            public const int FptrPsLocked = 8;
            public const int FptrPsNonfiscal = 9;
            public const int FptrPsReport = 10;


            // *///////////////////////////////////////////////////////////////////
            // * "FiscalReceiptStation" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int FptrRsReceipt = 1;
            public const int FptrRsSlip = 2;


            // *///////////////////////////////////////////////////////////////////
            // * "FiscalReceiptType" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int FptrRtCashIn = 1;
            public const int FptrRtCashOut = 2;
            public const int FptrRtGeneric = 3;
            public const int FptrRtSales = 4;
            public const int FptrRtService = 5;
            public const int FptrRtSimpleInvoice = 6;


            // *///////////////////////////////////////////////////////////////////
            // * "MessageType" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int FptrMtAdvance = 1;
            public const int FptrMtAdvancePaid = 2;
            public const int FptrMtAmountToBePaid = 3;
            public const int FptrMtAmountToBePaidBack = 4;
            public const int FptrMtCard = 5;
            public const int FptrMtCardNumber = 6;
            public const int FptrMtCardType = 7;
            public const int FptrMtCash = 8;
            public const int FptrMtCashier = 9;
            public const int FptrMtCashRegisterNumber = 10;
            public const int FptrMtChange = 11;
            public const int FptrMtCheque = 12;
            public const int FptrMtClientNumber = 13;
            public const int FptrMtClientSignature = 14;
            public const int FptrMtCounterState = 15;
            public const int FptrMtCreditCard = 16;
            public const int FptrMtCurrency = 17;
            public const int FptrMtCurrencyValue = 18;
            public const int FptrMtDeposit = 19;
            public const int FptrMtDepositReturned = 20;
            public const int FptrMtDotLine = 21;
            public const int FptrMtDriverNumb = 22;
            public const int FptrMtEmptyLine = 23;
            public const int FptrMtFreeText = 24;
            public const int FptrMtFreeTextWithDayLimit = 25;
            public const int FptrMtGivenDiscount = 26;
            public const int FptrMtLocalCredit = 27;
            public const int FptrMtMileageKm = 28;
            public const int FptrMtNote = 29;
            public const int FptrMtPaid = 30;
            public const int FptrMtPayIn = 31;
            public const int FptrMtPointGranted = 32;
            public const int FptrMtPointsBonus = 33;
            public const int FptrMtPointsReceipt = 34;
            public const int FptrMtPointsTotal = 35;
            public const int FptrMtProfited = 36;
            public const int FptrMtRate = 37;
            public const int FptrMtRegisterNumb = 38;
            public const int FptrMtShiftNumber = 39;
            public const int FptrMtStateOfAnAccount = 40;
            public const int FptrMtSubscription = 41;
            public const int FptrMtTable = 42;
            public const int FptrMtThankYouForLoyalty = 43;
            public const int FptrMtTransactionNumb = 44;
            public const int FptrMtValidTo = 45;
            public const int FptrMtVoucher = 46;
            public const int FptrMtVoucherPaid = 47;
            public const int FptrMtVoucherValue = 48;
            public const int FptrMtWithDiscount = 49;
            public const int FptrMtWithoutUplift = 50;


            // *///////////////////////////////////////////////////////////////////
            // * "SlipSelection" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int FptrSsFullLength = 1;
            public const int FptrSsValidation = 2;


            // *///////////////////////////////////////////////////////////////////
            // * "TotalizerType" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int FptrTtDocument = 1;
            public const int FptrTtDay = 2;
            public const int FptrTtReceipt = 3;
            public const int FptrTtGrand = 4;


            // *///////////////////////////////////////////////////////////////////
            // * "GetData" Method Constants
            // *///////////////////////////////////////////////////////////////////

            public const int FptrGdCurrentTotal = 1;
            public const int FptrGdDailyTotal = 2;
            public const int FptrGdReceiptNumber = 3;
            public const int FptrGdRefund = 4;
            public const int FptrGdNotPaid = 5;
            public const int FptrGdMidVoid = 6;
            public const int FptrGdZReport = 7;
            public const int FptrGdGrandTotal = 8;
            public const int FptrGdPrinterId = 9;
            public const int FptrGdFirmware = 10;
            public const int FptrGdRestart = 11;
            public const int FptrGdRefundVoid = 12;
            public const int FptrGdNumbConfigBlock = 13;
            public const int FptrGdNumbCurrencyBlock = 14;
            public const int FptrGdNumbHdrBlock = 15;
            public const int FptrGdNumbResetBlock = 16;
            public const int FptrGdNumbVatBlock = 17;
            public const int FptrGdFiscalDoc = 18;
            public const int FptrGdFiscalDocVoid = 19;
            public const int FptrGdFiscalRec = 20;
            public const int FptrGdFiscalRecVoid = 21;
            public const int FptrGdNonfiscalDoc = 22;
            public const int FptrGdNonfiscalDocVoid = 23;
            public const int FptrGdNonfiscalRec = 24;
            public const int FptrGdSimpInvoice = 25;
            public const int FptrGdTender = 26;
            public const int FptrGdLinecount = 27;
            public const int FptrGdDescriptionLength = 28;

            public const int FptrPdlCash = 1;
            public const int FptrPdlCheque = 2;
            public const int FptrPdlChitty = 3;
            public const int FptrPdlCoupon = 4;
            public const int FptrPdlCurrency = 5;
            public const int FptrPdlDrivenOff = 6;
            public const int FptrPdlEftImprinter = 7;
            public const int FptrPdlEftTerminal = 8;
            public const int FptrPdlTerminalImprinter = 9;
            public const int FptrPdlFreeGift = 10;
            public const int FptrPdlGiro = 11;
            public const int FptrPdlHome = 12;
            public const int FptrPdlImprinterWithIssuer = 13;
            public const int FptrPdlLocalAccount = 14;
            public const int FptrPdlLocalAccountCard = 15;
            public const int FptrPdlPayCard = 16;
            public const int FptrPdlPayCardManual = 17;
            public const int FptrPdlPrepay = 18;
            public const int FptrPdlPumpTest = 19;
            public const int FptrPdlShortCredit = 20;
            public const int FptrPdlStaff = 21;
            public const int FptrPdlVoucher = 22;

            public const int FptrLcItem = 1;
            public const int FptrLcItemVoid = 2;
            public const int FptrLcDiscount = 3;
            public const int FptrLcDiscountVoid = 4;
            public const int FptrLcSurcharge = 5;
            public const int FptrLcSurchargeVoid = 6;
            public const int FptrLcRefund = 7;
            public const int FptrLcRefundVoid = 8;
            public const int FptrLcSubtotalDiscount = 9;
            public const int FptrLcSubtotalDiscountVoid = 10;
            public const int FptrLcSubtotalSurcharge = 11;
            public const int FptrLcSubtotalSurchargeVoid = 12;
            public const int FptrLcComment = 13;
            public const int FptrLcSubtotal = 14;
            public const int FptrLcTotal = 15;

            public const int FptrDlItem = 1;
            public const int FptrDlItemAdjustment = 2;
            public const int FptrDlItemFuel = 3;
            public const int FptrDlItemFuelVoid = 4;
            public const int FptrDlNotPaid = 5;
            public const int FptrDlPackageAdjustment = 6;
            public const int FptrDlRefund = 7;
            public const int FptrDlRefundVoid = 8;
            public const int FptrDlSubtotalAdjustment = 9;
            public const int FptrDlTotal = 10;
            public const int FptrDlVoid = 11;
            public const int FptrDlVoidItem = 12;


            // *///////////////////////////////////////////////////////////////////
            // * "GetTotalizer" Method Constants
            // *///////////////////////////////////////////////////////////////////

            public const int FptrGtGross = 1;
            public const int FptrGtNet = 2;
            public const int FptrGtDiscount = 3;
            public const int FptrGtDiscountVoid = 4;
            public const int FptrGtItem = 5;
            public const int FptrGtItemVoid = 6;
            public const int FptrGtNotPaid = 7;
            public const int FptrGtRefund = 8;
            public const int FptrGtRefundVoid = 9;
            public const int FptrGtSubtotalDiscount = 10;
            public const int FptrGtSubtotalDiscountVoid = 11;
            public const int FptrGtSubtotalSurcharges = 12;
            public const int FptrGtSubtotalSurchargesVoid = 13;
            public const int FptrGtSurcharges = 14;
            public const int FptrGtSSurchargesVoid = 15;
            public const int FptrGtVat = 16;
            public const int FptrGtVatCategory = 17;


            // *///////////////////////////////////////////////////////////////////
            // * "AdjustmentType" arguments in diverse methods
            // *///////////////////////////////////////////////////////////////////

            public const int FptrAtAmountDiscount = 1;
            public const int FptrAtAmountSurcharge = 2;
            public const int FptrAtPercentageDiscount = 3;
            public const int FptrAtPercentageSurcharge = 4;


            // *///////////////////////////////////////////////////////////////////
            // * "ReportType" argument in "PrintReport" method
            // *///////////////////////////////////////////////////////////////////

            public const int FptrRtOrdinal = 1;
            public const int FptrRtDate = 2;


            // *///////////////////////////////////////////////////////////////////
            // * "NewCurrency" argument in "SetCurrency" method
            // *///////////////////////////////////////////////////////////////////

            public const int FptrScEuro = 1;


            // *///////////////////////////////////////////////////////////////////
            // * "StatusUpdateEvent" Event: "Data" Parameter Constants
            // *///////////////////////////////////////////////////////////////////

            public const int FptrSueCoverOpen = 11;
            public const int FptrSueCoverOk = 12;

            public const int FptrSueJrnEmpty = 21;
            public const int FptrSueJrnNearempty = 22;
            public const int FptrSueJrnPaperok = 23;

            public const int FptrSueRecEmpty = 24;
            public const int FptrSueRecNearempty = 25;
            public const int FptrSueRecPaperok = 26;

            public const int FptrSueSlpEmpty = 27;
            public const int FptrSueSlpNearempty = 28;
            public const int FptrSueSlpPaperok = 29;

            public const int FptrSueIdle = 1001;


            // *///////////////////////////////////////////////////////////////////
            // * "ResultCodeExtended" Property Constants for Fiscal Printer
            // *///////////////////////////////////////////////////////////////////

            public const int OposEfptrCoverOpen = 201; // (Several)
            public const int OposEfptrJrnEmpty = 202; // (Several)
            public const int OposEfptrRecEmpty = 203; // (Several)
            public const int OposEfptrSlpEmpty = 204; // (Several)
            public const int OposEfptrSlpForm = 205; // EndRemoval
            public const int OposEfptrMissingDevices = 206; // (Several)
            public const int OposEfptrWrongState = 207; // (Several)
            public const int OposEfptrTechnicalAssistance = 208; // (Several)
            public const int OposEfptrClockError = 209; // (Several)
            public const int OposEfptrFiscalMemoryFull = 210; // (Several)
            public const int OposEfptrFiscalMemoryDisconnected = 211; // (Several)
            public const int OposEfptrFiscalTotalsError = 212; // (Several)
            public const int OposEfptrBadItemQuantity = 213; // (Several)
            public const int OposEfptrBadItemAmount = 214; // (Several)
            public const int OposEfptrBadItemDescription = 215; // (Several)
            public const int OposEfptrReceiptTotalOverflow = 216; // (Several)
            public const int OposEfptrBadVat = 217; // (Several)
            public const int OposEfptrBadPrice = 218; // (Several)
            public const int OposEfptrBadDate = 219; // (Several)
            public const int OposEfptrNegativeTotal = 220; // (Several)
            public const int OposEfptrWordNotAllowed = 221; // (Several)
            public const int OposEfptrBadLength = 222; // (Several)
            public const int OposEfptrMissingSetCurrency = 223; // (Several)




            // *///////////////////////////////////////////////////////////////////
            // *
            // * OposKbd.h
            // *
            // *   POS Keyboard header file for OPOS Applications.
            // *
            // * Modification history
            // * ------------------------------------------------------------------
            // * 96-04-22 OPOS Release 1.1                                     CRM
            // * 97-06-04 OPOS Release 1.2                                     CRM
            // *   Add "EventTypes" and "POSKeyEventType" values.
            // *
            // *///////////////////////////////////////////////////////////////////


            // *///////////////////////////////////////////////////////////////////
            // * "EventTypes" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int KbdEtDown = 1;
            public const int KbdEtDownUp = 2;


            // *///////////////////////////////////////////////////////////////////
            // * "POSKeyEventType" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int KbdKetKeydown = 1;
            public const int KbdKetKeyup = 2;



            // *///////////////////////////////////////////////////////////////////
            // *
            // * OposLock.h
            // *
            // *   Keylock header file for OPOS Applications.
            // *
            // * Modification history
            // * ------------------------------------------------------------------
            // * 95-12-08 OPOS Release 1.0                                     CRM
            // *
            // *///////////////////////////////////////////////////////////////////


            // *///////////////////////////////////////////////////////////////////
            // * "KeyPosition" Property Constants
            // * "WaitForKeylockChange" Method: "KeyPosition" Parameter
            // * "StatusUpdateEvent" Event: "Data" Parameter
            // *///////////////////////////////////////////////////////////////////

            public const int LockKpAny = 0; // WaitForKeylockChange Only
            public const int LockKpLock = 1;
            public const int LockKpNorm = 2;
            public const int LockKpSupr = 3;



            // *///////////////////////////////////////////////////////////////////
            // *
            // * OposMicr.h
            // *
            // *   MICR header file for OPOS Applications.
            // *
            // * Modification history
            // * ------------------------------------------------------------------
            // * 95-12-08 OPOS Release 1.0                                     CRM
            // *
            // *///////////////////////////////////////////////////////////////////


            // *///////////////////////////////////////////////////////////////////
            // * "CheckType" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int MicrCtPersonal = 1;
            public const int MicrCtBusiness = 2;
            public const int MicrCtUnknown = 99;


            // *///////////////////////////////////////////////////////////////////
            // * "CountryCode" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int MicrCcUsa = 1;
            public const int MicrCcCanada = 2;
            public const int MicrCcMexico = 3;
            public const int MicrCcUnknown = 99;


            // *///////////////////////////////////////////////////////////////////
            // * "ResultCodeExtended" Property Constants for MICR
            // *///////////////////////////////////////////////////////////////////

            public const int OposEmicrNocheck = 201; // EndInsertion
            public const int OposEmicrCheck = 202; // EndRemoval



            // *///////////////////////////////////////////////////////////////////
            // *
            // * OposMsr.h
            // *
            // *   Magnetic Stripe Reader header file for OPOS Applications.
            // *
            // * Modification history
            // * ------------------------------------------------------------------
            // * 95-12-08 OPOS Release 1.0                                     CRM
            // * 97-06-04 OPOS Release 1.2                                     CRM
            // *   Add ErrorReportingType values.
            // * 00-09-24 OPOS Release 1.5                                     BKS
            // *   Add constants relating to Track 4 Data
            // *
            // *///////////////////////////////////////////////////////////////////


            // *///////////////////////////////////////////////////////////////////
            // * "TracksToRead" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int MsrTr1 = 1;
            public const int MsrTr2 = 2;
            public const int MsrTr3 = 4;
            public const int MsrTr4 = 8;

            public const int MsrTr12 = MsrTr1 | MsrTr2;
            public const int MsrTr13 = MsrTr1 | MsrTr3;
            public const int MsrTr14 = MsrTr1 | MsrTr4;
            public const int MsrTr23 = MsrTr2 | MsrTr3;
            public const int MsrTr24 = MsrTr2 | MsrTr4;
            public const int MsrTr34 = MsrTr3 | MsrTr4;

            public const int MsrTr123 = MsrTr1 | MsrTr2 | MsrTr3;
            public const int MsrTr124 = MsrTr1 | MsrTr2 | MsrTr4;
            public const int MsrTr134 = MsrTr1 | MsrTr3 | MsrTr4;
            public const int MsrTr234 = MsrTr2 | MsrTr3 | MsrTr4;

            public const int MsrTr1234 = MsrTr1 | MsrTr2 | MsrTr3 | MsrTr4;


            // *///////////////////////////////////////////////////////////////////
            // * "ErrorReportingType" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int MsrErtCard = 0;
            public const int MsrErtTrack = 1;


            // *///////////////////////////////////////////////////////////////////
            // * "ErrorEvent" Event: "ResultCodeExtended" Parameter Constants
            // *///////////////////////////////////////////////////////////////////

            public const int OposEmsrStart = 201;
            public const int OposEmsrEnd = 202;
            public const int OposEmsrParity = 203;
            public const int OposEmsrLrc = 204;


            // *///////////////////////////////////////////////////////////////////
            // *
            // * OposPcrw.H
            // *
            // *   Point Card Reader Writer header file for OPOS Applications.
            // *
            // * Modification history
            // * ------------------------------------------------------------------
            // * 00-09-24 OPOS Release 1.5                                     BKS
            // *
            // *///////////////////////////////////////////////////////////////////


            // *///////////////////////////////////////////////////////////////////
            // * "CapCharacterSet" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int PcrwCcsAlpha = 1;
            public const int PcrwCcsAscii = 998;
            public const int PcrwCcsKana = 10;
            public const int PcrwCcsKanji = 11;
            public const int PcrwCcsUnicode = 997;


            // *///////////////////////////////////////////////////////////////////
            // * "CardState" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int PcrwStateNocard = 1;
            public const int PcrwStateRemaining = 2;
            public const int PcrwStateInrw = 4;


            // *///////////////////////////////////////////////////////////////////
            // * CapTrackToRead and TrackToWrite Property constants
            // *///////////////////////////////////////////////////////////////////

            public const int PcrwTrack1 = 0x1;
            public const int PcrwTrack2 = 0x2;
            public const int PcrwTrack3 = 0x4;
            public const int PcrwTrack4 = 0x8;
            public const int PcrwTrack5 = 0x10;
            public const int PcrwTrack6 = 0x20;


            // *///////////////////////////////////////////////////////////////////
            // * "CharacterSet" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int PcrwCsUnicode = 997;
            public const int PcrwCsAscii = 998;
            public const int PcrwCsWindows = 999;


            // *///////////////////////////////////////////////////////////////////
            // * "MappingMode" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int PcrwMmDots = 1;
            public const int PcrwMmTwips = 2;
            public const int PcrwMmEnglish = 3;
            public const int PcrwMmMetric = 4;


            // *///////////////////////////////////////////////////////////////////
            // * "ResultCodeExtended" Property Constants for PoinrCardR/W
            // *///////////////////////////////////////////////////////////////////

            public const int OposEpcrwRead = 201;
            public const int OposEpcrwWrite = 202;
            public const int OposEpcrwJam = 203;
            public const int OposEpcrwMotor = 204;
            public const int OposEpcrwCover = 205;
            public const int OposEpcrwPrinter = 206;
            public const int OposEpcrwRelease = 207;
            public const int OposEpcrwDisplay = 208;
            public const int OposEpcrwNocard = 209;


            // *///////////////////////////////////////////////////////////////////
            // * Magnetic read/write status Property Constants for PoinrCardR/W
            // *///////////////////////////////////////////////////////////////////

            public const int OposEpcrwStart = 211;
            public const int OposEpcrwEnd = 212;
            public const int OposEpcrwParity = 213;
            public const int OposEpcrwEncode = 214;
            public const int OposEpcrwLrc = 215;
            public const int OposEpcrwVerify = 216;


            // *///////////////////////////////////////////////////////////////////
            // * "RotatedPrint" Method: "Rotation" Parameter Constants
            // * "RotateSpecial" Property Constants (PCRWRPNORMALASYNC not legal)
            // *///////////////////////////////////////////////////////////////////

            public const int PcrwRpNormal = 0x1;
            public const int PcrwRpNormalasync = 0x2;

            public const int PcrwRpRight90 = 0x101;
            public const int PcrwRpLeft90 = 0x102;
            public const int PcrwRpRotate180 = 0x103;


            // *///////////////////////////////////////////////////////////////////
            // * "StatusUpdateEvent" "Status" Parameter Constants
            // *///////////////////////////////////////////////////////////////////

            public const int PcrwSueNocard = 1;
            public const int PcrwSueRemaining = 2;
            public const int PcrwSueInrw = 4;


            // *///////////////////////////////////////////////////////////////////
            // *
            // * OposPpad.h
            // *
            // *   PIN Pad header file for OPOS Applications.
            // *
            // * Modification history
            // * ------------------------------------------------------------------
            // * 98-03-06 OPOS Release 1.3                                     JDB
            // * 00-09-24 OPOS Release 1.5                                     BKS
            // *   Add PpadDispNone for devices with no display
            // *   Add OposEppadBadKey extended result code
            // *
            // *///////////////////////////////////////////////////////////////////


            // *///////////////////////////////////////////////////////////////////
            // * "CapDisplay" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int PpadDispUnrestricted = 1;
            public const int PpadDispPinrestricted = 2;
            public const int PpadDispRestrictedList = 3;
            public const int PpadDispRestrictedOrder = 4;
            public const int PpadDispNone = 5;


            // *///////////////////////////////////////////////////////////////////
            // * "AvailablePromptsList" and "Prompt" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int PpadMsgEnterpin = 1;
            public const int PpadMsgPleasewait = 2;
            public const int PpadMsgEntervalidpin = 3;
            public const int PpadMsgRetriesexceeded = 4;
            public const int PpadMsgApproved = 5;
            public const int PpadMsgDeclined = 6;
            public const int PpadMsgCanceled = 7;
            public const int PpadMsgAmountok = 8;
            public const int PpadMsgNotready = 9;
            public const int PpadMsgIdle = 10;
            public const int PpadMsgSlideCard = 11;
            public const int PpadMsgInsertcard = 12;
            public const int PpadMsgSelectcardtype = 13;


            // *///////////////////////////////////////////////////////////////////
            // * "CapLanguage" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int PpadLangNone = 1;
            public const int PpadLangOne = 2;
            public const int PpadLangPinrestricted = 3;
            public const int PpadLangUnrestricted = 4;


            // *///////////////////////////////////////////////////////////////////
            // * "TransactionType" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int PpadTransDebit = 1;
            public const int PpadTransCredit = 2;
            public const int PpadTransInq = 3;
            public const int PpadTransReconcile = 4;
            public const int PpadTransAdmin = 5;


            // *///////////////////////////////////////////////////////////////////
            // * "EndEFTTransaction" Method Completion Code Constants
            // *///////////////////////////////////////////////////////////////////

            public const int PpadEftNormal = 1;
            public const int PpadEftAbnormal = 2;


            // *///////////////////////////////////////////////////////////////////
            // * "DataEvent" Event Status Constants
            // *///////////////////////////////////////////////////////////////////

            public const int PpadSuccess = 1;
            public const int PpadCancel = 2;


            // *///////////////////////////////////////////////////////////////////
            // * "ResultCodeExtended" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const short OposEppadBadKey = 201;


            // *///////////////////////////////////////////////////////////////////
            // *
            // * OposPtr.h
            // *
            // *   POS Printer header file for OPOS Applications.
            // *
            // * Modification history
            // * ------------------------------------------------------------------
            // * 95-12-08 OPOS Release 1.0                                     CRM
            // * 96-04-22 OPOS Release 1.1                                     CRM
            // *   Add CapCharacterSet values.
            // *   Add ErrorLevel values.
            // *   Add TransactionPrint Control values.
            // * 97-06-04 OPOS Release 1.2                                     CRM
            // *   Remove PTR_RP_NORMAL_ASYNC.
            // *   Add more barcode symbologies.
            // * 98-03-06 OPOS Release 1.3                                     CRM
            // *   Add more PrintTwoNormal constants.
            // * 00-09-24 OPOS Release 1.5                                   EPSON
            // *   Add CapRecMarkFeed values and MarkFeed constants.
            // *   Add ChangePrintSide constants.
            // *   Add StatusUpdateEvent constants.
            // *   Add ResultCodeExtended values.
            // *   Add CapXxxCartridgeSensor and XxxCartridgeState values.
            // *   Add CartridgeNotify values.
            // * 00-09-24 OPOS Release 1.5                                     BKS
            // *   Add CapCharacterset and CharacterSet values for UNICODE.
            // *
            // *///////////////////////////////////////////////////////////////////


            // *///////////////////////////////////////////////////////////////////
            // * Printer Station Constants
            // *///////////////////////////////////////////////////////////////////

            public const int PtrSJournal = 1;
            public const int PtrSReceipt = 2;
            public const int PtrSSlip = 4;

            public const int PtrSJournalReceipt = PtrSJournal | PtrSReceipt;
            public const int PtrSJournalSlip = PtrSJournal | PtrSSlip;
            public const int PtrSReceiptSlip = PtrSReceipt | PtrSSlip;

            public const int PtrTwoReceiptJournal = 0x8000 + PtrSJournalReceipt;
            public const int PtrTwoSlipJournal = 0x8000 + PtrSJournalSlip;
            public const int PtrTwoSlipReceipt = 0x8000 + PtrSReceiptSlip;


            // *///////////////////////////////////////////////////////////////////
            // * "CapCharacterSet" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int PtrCcsAlpha = 1;
            public const int PtrCcsAscii = 998;
            public const int PtrCcsKana = 10;
            public const int PtrCcsKanji = 11;
            public const int PtrCcsUnicode = 997;


            // *///////////////////////////////////////////////////////////////////
            // * "CharacterSet" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int PtrCsUnicode = 997;
            public const int PtrCsAscii = 998;
            public const int PtrCsWindows = 999;


            // *///////////////////////////////////////////////////////////////////
            // * "ErrorLevel" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int PtrElNone = 1;
            public const int PtrElRecoverable = 2;
            public const int PtrElFatal = 3;


            // *///////////////////////////////////////////////////////////////////
            // * "MapMode" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int PtrMmDots = 1;
            public const int PtrMmTwips = 2;
            public const int PtrMmEnglish = 3;
            public const int PtrMmMetric = 4;


            // *///////////////////////////////////////////////////////////////////
            // * "CapXxxColor" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int PtrColorPrimary = 0x1;
            public const int PtrColorCustom1 = 0x2;
            public const int PtrColorCustom2 = 0x4;
            public const int PtrColorCustom3 = 0x8;
            public const int PtrColorCustom4 = 0x10;
            public const int PtrColorCustom5 = 0x20;
            public const int PtrColorCustom6 = 0x40;
            public const int PtrColorCyan = 0x100;
            public const int PtrColorMagenta = 0x200;
            public const int PtrColorYellow = 0x400;
            public const int PtrColorFull = int.MinValue + 0x00000000;

            // *///////////////////////////////////////////////////////////////////
            // * "CapXxxCartridgeSensor" and  "XxxCartridgeState" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int PtrCartUnknown = 0x10000000;
            public const int PtrCartOk = 0x0;
            public const int PtrCartRemoved = 0x1;
            public const int PtrCartEmpty = 0x2;
            public const int PtrCartNearend = 0x4;
            public const int PtrCartCleaning = 0x8;

            // *///////////////////////////////////////////////////////////////////
            // * "CartridgeNotify"  Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int PtrCnDisabled = 0x0;
            public const int PtrCnEnabled = 0x1;


            // *///////////////////////////////////////////////////////////////////
            // * "CutPaper" Method Constant
            // *///////////////////////////////////////////////////////////////////

            public const int PtrCpFullcut = 100;


            // *///////////////////////////////////////////////////////////////////
            // * "PrintBarCode" Method Constants:
            // *///////////////////////////////////////////////////////////////////

            // *   "Alignment" Parameter
            // *     Either the distance from the left-most print column to the start
            // *     of the bar code, or one of the following:

            public const int PtrBcLeft = -1;
            public const int PtrBcCenter = -2;
            public const int PtrBcRight = -3;

            // *   "TextPosition" Parameter

            public const int PtrBcTextNone = -11;
            public const int PtrBcTextAbove = -12;
            public const int PtrBcTextBelow = -13;

            // *   "Symbology" Parameter:

            // *     One dimensional symbologies
            public const int PtrBcsUpca = 101; // Digits
            public const int PtrBcsUpce = 102; // Digits
            public const int PtrBcsJan8 = 103; // = EAN 8
            public const int PtrBcsEan8 = 103; // = JAN 8 (added in 1.2)
            public const int PtrBcsJan13 = 104; // = EAN 13
            public const int PtrBcsEan13 = 104; // = JAN 13 (added in 1.2)
            public const int PtrBcsTf = 105; // (Discrete 2 of 5) Digits
            public const int PtrBcsItf = 106; // (Interleaved 2 of 5) Digits
            public const int PtrBcsCodabar = 107; // Digits, -, $, :, /, ., +;
                                                  // 4 start/stop characters
                                                  // (a, b, c, d)
            public const int PtrBcsCode39 = 108; // Alpha, Digits, Space, -, .,
                                                 // $, /, +, %; start/stop (*)
                                                 // Also has Full ASCII feature
            public const int PtrBcsCode93 = 109; // Same characters as Code 39
            public const int PtrBcsCode128 = 110; // 128 data characters
                                                  // *        (The following were added in Release 1.2)
            public const int PtrBcsUpcaS = 111; // UPC-A with supplemental
                                                // barcode
            public const int PtrBcsUpceS = 112; // UPC-E with supplemental
                                                // barcode
            public const int PtrBcsUpcd1 = 113; // UPC-D1
            public const int PtrBcsUpcd2 = 114; // UPC-D2
            public const int PtrBcsUpcd3 = 115; // UPC-D3
            public const int PtrBcsUpcd4 = 116; // UPC-D4
            public const int PtrBcsUpcd5 = 117; // UPC-D5
            public const int PtrBcsEan8S = 118; // EAN 8 with supplemental
                                                // barcode
            public const int PtrBcsEan13S = 119; // EAN 13 with supplemental
                                                 // barcode
            public const int PtrBcsEan128 = 120; // EAN 128
            public const int PtrBcsOcra = 121; // OCR "A"
            public const int PtrBcsOcrb = 122; // OCR "B"


            // *     Two dimensional symbologies
            public const int PtrBcsPdf417 = 201;
            public const int PtrBcsMaxicode = 202;

            // *     Start of Printer-Specific bar code symbologies
            public const int PtrBcsOther = 501;


            // *///////////////////////////////////////////////////////////////////
            // * "PrintBitmap" Method Constants:
            // *///////////////////////////////////////////////////////////////////

            // *   "Width" Parameter
            // *     Either bitmap width or:

            public const int PtrBmAsis = -11; // One pixel per printer dot

            // *   "Alignment" Parameter
            // *     Either the distance from the left-most print column to the start
            // *     of the bitmap, or one of the following:

            public const int PtrBmLeft = -1;
            public const int PtrBmCenter = -2;
            public const int PtrBmRight = -3;


            // *///////////////////////////////////////////////////////////////////
            // * "RotatePrint" Method: "Rotation" Parameter Constants
            // * "RotateSpecial" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int PtrRpNormal = 0x1;
            public const int PtrRpRight90 = 0x101;
            public const int PtrRpLeft90 = 0x102;
            public const int PtrRpRotate180 = 0x103;


            // *///////////////////////////////////////////////////////////////////
            // * "SetLogo" Method: "Location" Parameter Constants
            // *///////////////////////////////////////////////////////////////////

            public const int PtrLTop = 1;
            public const int PtrLBottom = 2;


            // *///////////////////////////////////////////////////////////////////
            // * "TransactionPrint" Method: "Control" Parameter Constants
            // *///////////////////////////////////////////////////////////////////

            public const int PtrTpTransaction = 11;
            public const int PtrTpNormal = 12;


            // *///////////////////////////////////////////////////////////////////
            // * "MarkFeed" Method: "Type" Parameter Constants
            // * "CapRecMarkFeed" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const short PtrMfToTakeup = 1;
            public const short PtrMfToCutter = 2;
            public const short PtrMfToCurrentTof = 4;
            public const short PtrMfToNextTof = 8;

            // *///////////////////////////////////////////////////////////////////
            // * "ChangePrintSide" Method: "Side" Parameter Constants
            // *///////////////////////////////////////////////////////////////////

            public const short PtrPsUnknown = 0;
            public const short PtrPsSide1 = 1;
            public const short PtrPsSide2 = 2;
            public const short PtrPsOpposite = 3;


            // *///////////////////////////////////////////////////////////////////
            // * "StatusUpdateEvent" Event: "Data" Parameter Constants
            // *///////////////////////////////////////////////////////////////////

            public const int PtrSueCoverOpen = 11;
            public const int PtrSueCoverOk = 12;

            public const int PtrSueJrnEmpty = 21;
            public const int PtrSueJrnNearempty = 22;
            public const int PtrSueJrnPaperok = 23;

            public const int PtrSueRecEmpty = 24;
            public const int PtrSueRecNearempty = 25;
            public const int PtrSueRecPaperok = 26;

            public const int PtrSueSlpEmpty = 27;
            public const int PtrSueSlpNearempty = 28;
            public const int PtrSueSlpPaperok = 29;

            public const short PtrSueJrnCartridgeEmpty = 41;
            public const short PtrSueJrnCartridgeNearempty = 42;
            public const short PtrSueJrnHeadCleaning = 43;
            public const short PtrSueJrnCartridgeOk = 44;

            public const short PtrSueRecCartridgeEmpty = 45;
            public const short PtrSueRecCartridgeNearempty = 46;
            public const short PtrSueRecHeadCleaning = 47;
            public const short PtrSueRecCartridgeOk = 48;

            public const short PtrSueSlpCartridgeEmpty = 49;
            public const short PtrSueSlpCartridgeNearempty = 50;
            public const short PtrSueSlpHeadCleaning = 51;
            public const short PtrSueSlpCartridgeOk = 52;

            public const int PtrSueIdle = 1001;


            // *///////////////////////////////////////////////////////////////////
            // * "ResultCodeExtended" Property Constants for Printer
            // *///////////////////////////////////////////////////////////////////

            public const int OposEptrCoverOpen = 201; // (Several)
            public const int OposEptrJrnEmpty = 202; // (Several)
            public const int OposEptrRecEmpty = 203; // (Several)
            public const int OposEptrSlpEmpty = 204; // (Several)
            public const int OposEptrSlpForm = 205; // EndRemoval
            public const int OposEptrToobig = 206; // PrintBitmap
            public const int OposEptrBadformat = 207; // PrintBitmap
            public const short OposEptrJrnCartridgeRemoved = 208; // (Several)
            public const short OposEptrJrnCartridgeEmpty = 209; // (Several)
            public const short OposEptrJrnHeadCleaning = 210; // (Several)
            public const short OposEptrRecCartridgeRemoved = 211; // (Several)
            public const short OposEptrRecCartridgeEmpty = 212; // (Several)
            public const short OposEptrRecHeadCleaning = 213; // (Several)
            public const short OposEptrSlpCartridgeRemoved = 214; // (Several)
            public const short OposEptrSlpCartridgeEmpty = 215; // (Several)
            public const short OposEptrSlpHeadCleaning = 216; // (Several)



            // *///////////////////////////////////////////////////////////////////
            // *
            // * OposPwr.h
            // *
            // *   POSPower header file for OPOS Applications.
            // *
            // * Modification history
            // * ------------------------------------------------------------------
            // * 99-02-22 OPOS Release 1.x                                     AL
            // * 99-09-13 OPOS Release 1.x                                     TH
            // *            ACCU -> UPS, FAN_ALARM and HEAT_ALARM added
            // * 99-12-06 OPOS Release 1.x                                     TH
            // *            FAN_ALARM and HEAT_ALARM changed to FAN_STOPPED,
            // *          FAN_RUNNING, TEMPERATURE_HIGH and TEMPERATURE_OK
            // * 00-09-24 OPOS Release 1.5                                     TH
            // *          SHUTDOWN added
            // *
            // *///////////////////////////////////////////////////////////////////

            // *///////////////////////////////////////////////////////////////////
            // * "UPSChargeState" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const short PwrUpsFull = 1;
            public const short PwrUpsWarning = 2;
            public const short PwrUpsLow = 4;
            public const short PwrUpsCritical = 8;


            // *///////////////////////////////////////////////////////////////////
            // * "StatusUpdateEvent" Event: "Status" Parameter
            // *///////////////////////////////////////////////////////////////////

            public const short PwrSueUpsFull = 11;
            public const short PwrSueUpsWarning = 12;
            public const short PwrSueUpsLow = 13;
            public const short PwrSueUpsCritical = 14;
            public const short PwrSueFanStopped = 15;
            public const short PwrSueFanRunning = 16;
            public const short PwrSueTemperatureHigh = 17;
            public const short PwrSueTemperatureOk = 18;
            public const short PwrSueShutdown = 19;



            // *///////////////////////////////////////////////////////////////////
            // *
            // * OposRod.h
            // *
            // *   Remote Order Display header file for OPOS Applications.
            // *
            // * Modification history
            // * ------------------------------------------------------------------
            // * 98-03-06 OPOS Release 1.3                                     BB
            // * 00-09-24 OPOS Release 1.5                                    BKS
            // *   Added CharacterSet value for UNICODE.
            // *
            // *///////////////////////////////////////////////////////////////////


            // *///////////////////////////////////////////////////////////////////
            // * "CurrentUnitID" and "UnitsOnline" Properties
            // *  and "Units" Parameter Constants
            // *///////////////////////////////////////////////////////////////////

            public const int RodUid1 = 0x1;
            public const int RodUid2 = 0x2;
            public const int RodUid3 = 0x4;
            public const int RodUid4 = 0x8;
            public const int RodUid5 = 0x10;
            public const int RodUid6 = 0x20;
            public const int RodUid7 = 0x40;
            public const int RodUid8 = 0x80;
            public const int RodUid9 = 0x100;
            public const int RodUid10 = 0x200;
            public const int RodUid11 = 0x400;
            public const int RodUid12 = 0x800;
            public const int RodUid13 = 0x1000;
            public const int RodUid14 = 0x2000;
            public const int RodUid15 = 0x4000;
            public const int RodUid16 = 0x8000;
            public const int RodUid17 = 0x10000;
            public const int RodUid18 = 0x20000;
            public const int RodUid19 = 0x40000;
            public const int RodUid20 = 0x80000;
            public const int RodUid21 = 0x100000;
            public const int RodUid22 = 0x200000;
            public const int RodUid23 = 0x400000;
            public const int RodUid24 = 0x800000;
            public const int RodUid25 = 0x1000000;
            public const int RodUid26 = 0x2000000;
            public const int RodUid27 = 0x4000000;
            public const int RodUid28 = 0x8000000;
            public const int RodUid29 = 0x10000000;
            public const int RodUid30 = 0x20000000;
            public const int RodUid31 = 0x40000000;
            public const int RodUid32 = int.MinValue + 0x00000000;


            // *///////////////////////////////////////////////////////////////////
            // * Broadcast Methods: "Attribute" Parameter Constants
            // *///////////////////////////////////////////////////////////////////

            public const int RodAttrBlink = 0x80;

            public const int RodAttrBgBlack = 0x0;
            public const int RodAttrBgBlue = 0x10;
            public const int RodAttrBgGreen = 0x20;
            public const int RodAttrBgCyan = 0x30;
            public const int RodAttrBgRed = 0x40;
            public const int RodAttrBgMagenta = 0x50;
            public const int RodAttrBgBrown = 0x60;
            public const int RodAttrBgGray = 0x70;

            public const int RodAttrIntensity = 0x8;

            public const int RodAttrFgBlack = 0x0;
            public const int RodAttrFgBlue = 0x1;
            public const int RodAttrFgGreen = 0x2;
            public const int RodAttrFgCyan = 0x3;
            public const int RodAttrFgRed = 0x4;
            public const int RodAttrFgMagenta = 0x5;
            public const int RodAttrFgBrown = 0x6;
            public const int RodAttrFgGray = 0x7;


            // *///////////////////////////////////////////////////////////////////
            // * "DrawBox" Method: "BorderType" Parameter Constants
            // *///////////////////////////////////////////////////////////////////

            public const int RodBdrSingle = 1;
            public const int RodBdrDouble = 2;
            public const int RodBdrSolid = 3;


            // *///////////////////////////////////////////////////////////////////
            // * "ControlClock" Method: "Function" Parameter Constants
            // *///////////////////////////////////////////////////////////////////

            public const int RodClkStart = 1;
            public const int RodClkPause = 2;
            public const int RodClkResume = 3;
            public const int RodClkMove = 4;
            public const int RodClkStop = 5;


            // *///////////////////////////////////////////////////////////////////
            // * "ControlCursor" Method: "Function" Parameter Constants
            // *///////////////////////////////////////////////////////////////////

            public const int RodCrsLine = 1;
            public const int RodCrsLineBlink = 2;
            public const int RodCrsBlock = 3;
            public const int RodCrsBlockBlink = 4;
            public const int RodCrsOff = 5;


            // *///////////////////////////////////////////////////////////////////
            // * "SelectCharacterSet" Method: "CharacterSet" Parameter Constants
            // *///////////////////////////////////////////////////////////////////

            public const int RodCsUnicode = 997;
            public const int RodCsAscii = 998;
            public const int RodCsWindows = 999;


            // *///////////////////////////////////////////////////////////////////
            // * "TransactionDisplay" Method: "Function" Parameter Constants
            // *///////////////////////////////////////////////////////////////////

            public const int RodTdTransaction = 11;
            public const int RodTdNormal = 12;


            // *///////////////////////////////////////////////////////////////////
            // * "UpdateVideoRegionAttribute" Method: "Function" Parameter Constants
            // *///////////////////////////////////////////////////////////////////

            public const int RodUaSet = 1;
            public const int RodUaIntensityOn = 2;
            public const int RodUaIntensityOff = 3;
            public const int RodUaReverseOn = 4;
            public const int RodUaReverseOff = 5;
            public const int RodUaBlinkOn = 6;
            public const int RodUaBlinkOff = 7;


            // *///////////////////////////////////////////////////////////////////
            // * "EventTypes" Property and "DataEvent" Event: "Status" Parameter Constants
            // *///////////////////////////////////////////////////////////////////

            public const int RodDeTouchUp = 0x1;
            public const int RodDeTouchDown = 0x2;
            public const int RodDeTouchMove = 0x4;


            // *///////////////////////////////////////////////////////////////////
            // * "ResultCodeExtended" Property Constants for Remote Order Display
            // *///////////////////////////////////////////////////////////////////

            public const int OposErodBadclk = 201; // ControlClock
            public const int OposErodNoclocks = 202; // ControlClock
            public const int OposErodNoregion = 203; // RestoreVideoRegion
            public const int OposErodNobuffers = 204; // SaveVideoRegion
            public const int OposErodNoroom = 205; // SaveVideoRegion



            // *///////////////////////////////////////////////////////////////////
            // *
            // * OposScal.h
            // *
            // *   Scale header file for OPOS Applications.
            // *
            // * Modification history
            // * ------------------------------------------------------------------
            // * 95-12-08 OPOS Release 1.0                                     CRM
            // *
            // *///////////////////////////////////////////////////////////////////


            // *///////////////////////////////////////////////////////////////////
            // * "WeightUnit" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int ScalWuGram = 1;
            public const int ScalWuKilogram = 2;
            public const int ScalWuOunce = 3;
            public const int ScalWuPound = 4;


            // *///////////////////////////////////////////////////////////////////
            // * "ResultCodeExtended" Property Constants for Scale
            // *///////////////////////////////////////////////////////////////////

            public const int OposEscalOverweight = 201; // ReadWeight



            // *///////////////////////////////////////////////////////////////////
            // *
            // * OposScan.h
            // *
            // *   Scanner header file for OPOS Applications.
            // *
            // * Modification history
            // * ------------------------------------------------------------------
            // * 95-12-08 OPOS Release 1.0                                     CRM
            // * 97-06-04 OPOS Release 1.2                                     CRM
            // *   Add "ScanDataType" values.
            // *
            // *///////////////////////////////////////////////////////////////////


            // *///////////////////////////////////////////////////////////////////
            // * "ScanDataType" Property Constants
            // *///////////////////////////////////////////////////////////////////

            // * One dimensional symbologies
            public const int ScanSdtUpca = 101; // Digits
            public const int ScanSdtUpce = 102; // Digits
            public const int ScanSdtJan8 = 103; // = EAN 8
            public const int ScanSdtEan8 = 103; // = JAN 8 (added in 1.2)
            public const int ScanSdtJan13 = 104; // = EAN 13
            public const int ScanSdtEan13 = 104; // = JAN 13 (added in 1.2)
            public const int ScanSdtTf = 105; // (Discrete 2 of 5) Digits
            public const int ScanSdtItf = 106; // (Interleaved 2 of 5) Digits
            public const int ScanSdtCodabar = 107; // Digits, -, $, :, /, ., +;
                                                   // 4 start/stop characters
                                                   // (a, b, c, d)
            public const int ScanSdtCode39 = 108; // Alpha, Digits, Space, -, .,
                                                  // $, /, +, %; start/stop (*)
                                                  // Also has Full ASCII feature
            public const int ScanSdtCode93 = 109; // Same characters as Code 39
            public const int ScanSdtCode128 = 110; // 128 data characters

            public const int ScanSdtUpcaS = 111; // UPC-A with supplemental
                                                 // barcode
            public const int ScanSdtUpceS = 112; // UPC-E with supplemental
                                                 // barcode
            public const int ScanSdtUpcd1 = 113; // UPC-D1
            public const int ScanSdtUpcd2 = 114; // UPC-D2
            public const int ScanSdtUpcd3 = 115; // UPC-D3
            public const int ScanSdtUpcd4 = 116; // UPC-D4
            public const int ScanSdtUpcd5 = 117; // UPC-D5
            public const int ScanSdtEan8S = 118; // EAN 8 with supplemental
                                                 // barcode
            public const int ScanSdtEan13S = 119; // EAN 13 with supplemental
                                                  // barcode
            public const int ScanSdtEan128 = 120; // EAN 128
            public const int ScanSdtOcra = 121; // OCR "A"
            public const int ScanSdtOcrb = 122; // OCR "B"

            // * Two dimensional symbologies
            public const int ScanSdtPdf417 = 201;
            public const int ScanSdtMaxicode = 202;

            // * Special cases
            public const int ScanSdtOther = 501; // Start of Scanner-Specific bar
                                                 // code symbologies
            public const int ScanSdtUnknown = 0; // Cannot determine the barcode
                                                 // symbology.



            // *///////////////////////////////////////////////////////////////////
            // *
            // * OposSig.h
            // *
            // *   Signature Capture header file for OPOS Applications.
            // *
            // * Modification history
            // * ------------------------------------------------------------------
            // * 95-12-08 OPOS Release 1.0                                     CRM
            // *
            // *///////////////////////////////////////////////////////////////////


            // * No definitions required for this version.



            // *///////////////////////////////////////////////////////////////////
            // *
            // * OposTone.h
            // *
            // *   Tone Indicator header file for OPOS Applications.
            // *
            // * Modification history
            // * ------------------------------------------------------------------
            // * 97-06-04 OPOS Release 1.2                                     CRM
            // *
            // *///////////////////////////////////////////////////////////////////


            // * No definitions required for this version.



            // *///////////////////////////////////////////////////////////////////
            // *
            // * OposTot.h
            // *
            // *   Hard Totals header file for OPOS Applications.
            // *
            // * Modification history
            // * ------------------------------------------------------------------
            // * 95-12-08 OPOS Release 1.0                                     CRM
            // *
            // *///////////////////////////////////////////////////////////////////


            // *///////////////////////////////////////////////////////////////////
            // * "ResultCodeExtended" Property Constants for Hard Totals
            // *///////////////////////////////////////////////////////////////////

            public const int OposEtotNoroom = 201; // Create, Write
            public const int OposEtotValidation = 202; // Read, Write

            // *///////////////////////////////////////////////////////////////////
            // *
            // * OposRfid.h
            // *
            // *   RFID Scanner header file for OPOS Applications.
            // *
            // * Modification history
            // * ------------------------------------------------------------------
            // * 07-08-17 OPOS Release 1.12                                     TEC
            // *
            // *///////////////////////////////////////////////////////////////////

            // *///////////////////////////////////////////////////////////////////
            // * "CapMultipleProtocols" & "ProtocolMask" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int RfidPrEpc0 = 0x1; // EPC Class 0 read-only passive tags
            public const int RfidPr0plus = 0x2; // Non-EPC Class “0+” write once passive tags
            public const int RfidPrEpc1 = 0x4; // EPC Class 1 write once passive tags
            public const int RfidPrEpc1g2 = 0x8; // EPC Class 1 Gen 2 write once passive tags
            public const int RfidPrEpc2 = 0x10; // EPC Class 2 rewritable tags
            public const int RfidPrIso14443A = 0x1000; // ISO 14443A HF tags
            public const int RfidPrIso14443B = 0x2000; // ISO 14443B HF tags
            public const int RfidPrIso15693 = 0x3000; // ISO 15693 HF tags
            public const int RfidPrIso180006B = 0x4000; // ISO 18000-6B UHF tags
            public const int RfidPrOther = 0x1000000; // A tag that does not fit into one of the defined protocols
            public const int RfidPrAll = 0x40000000; // (ProtocolMask only)

            // *///////////////////////////////////////////////////////////////////
            // * "CapWriteTag" Property Constants
            // *///////////////////////////////////////////////////////////////////

            public const int RfidCwtNone = 0; // No writable fields in the tag
            public const int RfidCwtId = 1; // The ID field in the tag is writable
            public const int RfidCwtUserData = 2; // The UserData field in the tag is writable
            public const int RfidCwtAll = 3; // All fields in the tag are writable

            // *///////////////////////////////////////////////////////////////////
            // * "ReadTags","StartReadTags" Methods Constants
            // *///////////////////////////////////////////////////////////////////

            public const int RfidRtId = 0x10; // Read only the ID data
            public const int RfidRtFullUserData = 0x1; // Read the full UserData
            public const int RfidRtPartialUserData = 0x2; // Read the defined partial UserData
            public const int RfidRtIdFullUserData = 0x11; // Read the ID and full UserData
            public const int RfidRtIdPartialUserData = 0x12; // Read the ID and 
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            // set the current caret position to the end
            messageFromApp.SelectionStart = messageFromApp.Text.Length;
            // scroll it automatically
            messageFromApp.ScrollToCaret();
        }

        PictureBox lastChoose = null;

        private void pictureBox_Click(object sender, EventArgs e)
        {


                PictureBox choosingImage = sender as PictureBox;
                ProductPos temp = new ProductPos();
                ProductPos data = new ProductPos               
                {
                    Jancode = Session.product.Jancode,
                    RFIDcode = txtRfid.Text,
                    shelf_col_pos = Session.positionPos[choosingImage.Name].col.ToString(), //choosingImage.Name.Substring(11, 1),
                    shelf_pos = Session.positionPos[choosingImage.Name].row.ToString(),
                    product_name = Session.product.goods_name,
                    shelf_name = cbShelf.Text,
                    isbn = Session.product.isbn,
                    link_image = Session.product.link_image,
                    picture_box = choosingImage
                };

                if (Session.product.link_image == "" && txtRfid.Text != "" && txtName.Text != "")
                {
                    Session.product.link_image = "noimage.png";
                }


                if (Session.productPos.Keys.Contains(choosingImage.Name) && Session.productPos[choosingImage.Name].RFIDcode != "" )
                {

                if (!Session.productPos.Keys.Contains("temp"))
                {

                    Session.productPos["temp"] = Session.productPos[choosingImage.Name];
                    Session.productPos["temp"].picture_box = choosingImage;

                    if (Session.productPos["temp"].link_image == "")
                    {
                        Session.productPos["temp"].link_image = "blank_background.png";
                    }



                    // Add focus to textBox                         ;
                    foreach (var txtBox_Items in ImageLayer.Controls.OfType<TextBox>())
                    {
                        string mapTextBox = Session.productPos["temp"].picture_box.Name.Replace("pictureBox", "textBox");
                        {
                            if (txtBox_Items.Name == mapTextBox)
                            {
                                txtBox_Items.BackColor = Color.Yellow;
                            }
                        }
                        if (lastChoose != null)
                        {
                            string mapLastTextBox = lastChoose.Name.Replace("pictureBox", "textBox");
                            {
                                if (txtBox_Items.Name == mapLastTextBox && Session.status_mode == false)
                                {
                                    txtBox_Items.BackColor = Color.PaleTurquoise;
                                }
                                else if (txtBox_Items.Name == mapLastTextBox)
                                {
                                    txtBox_Items.BackColor = Color.White;
                                }
                            }
                        }
                    }

                    lastChoose = choosingImage;
                    Console.WriteLine("Saved data to temp");
                }
                else
                {
                    //Swap function
                    PictureBox now = sender as PictureBox;
                    Console.WriteLine(now.Name);

                    if (now.Name != "" && now.Name != lastChoose.Name)
                    {
                        Console.WriteLine("Have image clicked!");

                        //Gán dữ liệu ảnh đang chọn vào biến datanew
                        //Lưu ý khi swap thì vị trí lưu phải là vị trí của thằng temp => Nếu ko chỉ dữ liệu thay đổi, vị trí không thay đổi
                        ProductPos datanew = new ProductPos
                        {
                            Jancode = Session.productPos[choosingImage.Name].Jancode,
                            RFIDcode = Session.productPos[choosingImage.Name].RFIDcode,
                            shelf_col_pos = Session.positionPos[Session.productPos["temp"].picture_box.Name].col.ToString(), //choosingImage.Name.Substring(11, 1),
                            shelf_pos = Session.positionPos[Session.productPos["temp"].picture_box.Name].row.ToString(),
                            product_name = Session.productPos[choosingImage.Name].product_name,
                            shelf_name = cbShelf.Text,
                            isbn = Session.productPos[choosingImage.Name].isbn,
                            link_image = Session.productPos[choosingImage.Name].link_image,
                            picture_box = choosingImage
                        };

                        //Lưu vị trí riêng so với data
                        Session.productPos["temp"].shelf_col_pos = Session.productPos[choosingImage.Name].shelf_col_pos;
                        Session.productPos["temp"].shelf_pos = Session.productPos[choosingImage.Name].shelf_pos;

                        //Gán dữ liệu của biến temp vào ảnh đang chọn
                        Session.productPos[choosingImage.Name] = Session.productPos["temp"];

                        //Lấy dữ liệu của biến datanew vào lại picturebox của temp
                        Session.productPos[Session.productPos["temp"].picture_box.Name] = datanew;
                        if (Session.scan_mode == true)
                        {
                            updateName_Scan();
                        }
                        else
                        {
                            updateName();
                        }
                        updatePictureBox(); ;
                        lastChoose = choosingImage;
                        Session.productPos.Remove("temp");
                    }
                }

            }
            if (Session.scan_mode == false)
            {
                // WORKING HERE 
                // IF NOT EXIST TEMP!
                if (!Session.productPos.Keys.Contains("temp"))
                {
                    if (lastChoose == null)
                    {
                        if (choosingImage.ImageLocation == "blank_background.png")
                        {
                            // Delete duplicate
                            string pictureBoxDuplicateName = Session.productPos.FirstOrDefault(duplicateItems => duplicateItems.Value.RFIDcode == txtRfid.Text).Key;
                            if (pictureBoxDuplicateName != null)
                            {
                                foreach (PictureBox duplicateItems in ImageLayer.Controls.OfType<PictureBox>())
                                {
                                    if (duplicateItems.Name == pictureBoxDuplicateName && Session.productPos[duplicateItems.Name].RFIDcode != "")
                                    {
                                        if (Session.status_mode == true)
                                        {
                                            duplicateItems.Load("blank_background.png");
                                            Image img = changeOpacity(new Bitmap(duplicateItems.Image), 100);
                                            duplicateItems.Image = img;
                                            string key_text = duplicateItems.Name.Replace("pictureBox", "textBox");
                                            foreach (var txtBox_Items in ImageLayer.Controls.OfType<TextBox>())
                                            {
                                                if (txtBox_Items.Name == key_text)
                                                {
                                                    txtBox_Items.BackColor = Color.White;
                                                }
                                            }
                                            Session.productPos.Remove(pictureBoxDuplicateName);
                                            break;
                                        }
                                        else
                                        {
                                            duplicateItems.Load("blank_background.png");
                                            Session.productPos.Remove(pictureBoxDuplicateName);
                                            break;
                                        }
                                    }
                                }
                            }

                            //Load data from panel to screen 
                            if (Session.product.link_image != "")
                            {
                                // Check base64
                                string url = GetImage(Session.product.link_image, Session.rfidcode);
                                Session.product.link_image = url;
                                choosingImage.Load(url);
                                Session.productPos[choosingImage.Name] = data;
                                lastChoose = choosingImage;
                            }
                            else
                            {
                                Console.WriteLine("Không có hình không làm gì cả");
                            }
                        }
                    }
                    // Trường hợp click lần thứ 2 trở đi
                    // Handle for scan mode = false 
                    else if (choosingImage.ImageLocation == "blank_background.png")
                    {
                        // Kiểm tra dữ liệu đã có chưa, có rồi thì xóa 
                        string pictureBoxDuplicateName = Session.productPos.FirstOrDefault(t => t.Value.RFIDcode == txtRfid.Text).Key;
                        if (pictureBoxDuplicateName != null)
                        {
                            foreach (PictureBox pic in ImageLayer.Controls.OfType<PictureBox>())
                            {
                                // Tìm thấy picturebox trùng data
                                if (pic.Name == pictureBoxDuplicateName)
                                {
                                    // Chỉnh giao diện tối trong chế độ scan và xóa thằng trước đó
                                    if (Session.status_mode == true)
                                    {
                                        pic.Load("blank_background.png");
                                        Image img = changeOpacity(new Bitmap(pic.Image), 100);
                                        pic.Image = img;
                                        string key_text = pic.Name.Replace("pictureBox", "textBox");
                                        foreach (var txtBox_Items in ImageLayer.Controls.OfType<TextBox>())
                                        {
                                            if (txtBox_Items.Name == key_text)
                                            {
                                                txtBox_Items.BackColor = Color.White;
                                            }
                                        }
                                        Session.productPos.Remove(pictureBoxDuplicateName);
                                        break;
                                    }
                                    else
                                    {
                                        //Xóa thằng trước đó 
                                        pic.Load("blank_background.png");
                                        Session.productPos.Remove(pictureBoxDuplicateName);
                                        break;
                                    }

                                }
                            }
                        }

                        if (Session.product.link_image != "")
                        {
                            //Check base64
                            string url = GetImage(Session.product.link_image, Session.rfidcode);
                            Session.product.link_image = url;
                            choosingImage.Load(url);
                            Session.productPos[choosingImage.Name] = data;
                            lastChoose = choosingImage;
                        }
                        else
                        {
                            Console.WriteLine("Không có hình không làm gì cả");
                        }
                    }
                    

                   
                    updateName();
                }
                //Handle for scan mode = true 
                else if (choosingImage.ImageLocation == "blank_background.png")
                {
                    // WORKING HERE 
                    // IF EXIST TEMP!               
                    choosingImage.Load(Session.productPos["temp"].link_image);
                    Session.productPos[choosingImage.Name] = Session.productPos["temp"];
                    //Continue handle duplicate image
                    Session.productPos["temp"].picture_box.Load("blank_background.png");
                    ////Load black screen for last choose
                    if (btnCheck.BackColor == Color.ForestGreen)
                    {
                        Image img = changeOpacity(new Bitmap(Session.productPos["temp"].picture_box.Image), 100);
                        Session.productPos["temp"].picture_box.Image = img;
                        string key_text = Session.productPos["temp"].picture_box.Name.Replace("pictureBox", "textBox");
                        foreach (var txtBox_Items in ImageLayer.Controls.OfType<TextBox>())
                        {
                            if (txtBox_Items.Name == key_text)
                            {
                                txtBox_Items.BackColor = Color.White;
                            }
                        }
                    }

                    //Add data to dictionary
                    data.Jancode = Session.productPos[choosingImage.Name].Jancode;
                    data.RFIDcode = Session.productPos[choosingImage.Name].RFIDcode;
                    data.product_name = Session.productPos[choosingImage.Name].product_name;
                    data.isbn = Session.productPos[choosingImage.Name].isbn;
                    data.link_image = Session.productPos[choosingImage.Name].link_image;

                    Session.productPos[choosingImage.Name] = data;
                    Session.productPos.Remove(Session.productPos["temp"].picture_box.Name);
                    Session.productPos.Remove("temp");
                    updateName();
                }
                else
                {
                }
            } //Fixing 1109 
            else if (Session.scan_mode == true)
            {
                // Handle for Scan mode
                if (Session.productPos.Keys.Contains("temp"))
                {
                    if (choosingImage.ImageLocation == "blank_background.png")
                    {
                        // WORKING HERE 
                        // IF EXIST TEMP
                        string url = GetImage(Session.product.link_image, Session.rfidcode);
                        Session.product.link_image = url;
                        choosingImage.Load(url);

                        //Continue handle duplicate image
                        Session.productPos["temp"].picture_box.Load("blank_background.png");

                        ////Load black screen for last choose
                        if (btnCheck.BackColor == Color.ForestGreen)
                        {
                            Image img = changeOpacity(new Bitmap(Session.productPos["temp"].picture_box.Image), 100);
                            Session.productPos["temp"].picture_box.Image = img;
                            string key_text = Session.productPos["temp"].picture_box.Name.Replace("pictureBox", "textBox");
                            foreach (var txtBox_Items in ImageLayer.Controls.OfType<TextBox>())
                            {
                                if (txtBox_Items.Name == key_text)
                                {
                                    txtBox_Items.BackColor = Color.White;
                                }
                            }
                        }

                        //Add data to dictionary
                        data.Jancode = Session.productPos[choosingImage.Name].Jancode;
                        data.RFIDcode = Session.productPos[choosingImage.Name].RFIDcode;
                        data.product_name = Session.productPos[choosingImage.Name].product_name;
                        data.isbn = Session.productPos[choosingImage.Name].isbn;
                        data.link_image = Session.productPos[choosingImage.Name].link_image;
                        Session.productPos[choosingImage.Name] = data;
                        Session.productPos.Remove(Session.productPos["temp"].picture_box.Name);
                        Session.productPos.Remove("temp");
                        updateName_Scan();                   

                        foreach (TextBox txtBox_Items in ImageLayer.Controls.OfType<TextBox>())
                        {
                            txtBox_Items.BackColor = Color.PaleTurquoise;
                        }

                        updatePictureBox();
                    } else
                    {
                        Console.WriteLine("Not handle");
                    }

                    }
                }
        }

        private void pictureBox_DoubleClick(object sender, EventArgs e)
        {
            PictureBox deleteImage = sender as PictureBox;
            if (deleteImage.ImageLocation != "blank_background.png")
            {
                DialogResult confirmResult = MessageBox.Show("これを削除してもよろしいですか", "確認ダイアログ", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

                if (confirmResult == DialogResult.Yes)
                {
                    //bugging
                    deleteImage.Load("blank_background.png");
                    Session.productPos.Remove(deleteImage.Name);
                    deleteName();
                    //ProductPos z = Session.productPos[deleteImage.Name];
                    //Session.productPos.Keys.ToList().ForEach(z => Console.WriteLine("Result after delete" + z.ToString()));


                    if (Session.productPos.Keys.Contains("temp"))
                    {
                        Session.productPos.Remove(Session.productPos["temp"].picture_box.Name);
                        Session.productPos.Remove("temp");
                    }
                }

            } else
            {
                Console.WriteLine("Nothing to delete");
            }
        }


        private void btnRegister_Click(object sender, EventArgs e)
        {

            //Session.productPos.Keys.ToList().ForEach(x => Console.WriteLine("Data is " + x.ToString()+ (Session.productPos[x] as ProductPos).Jancode));
            if (cbShelf.Text == "")
            {
                DialogResult infoPopUp = MessageBox.Show("棚名フィールドにデータがありません。入力または選択してください!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else 
            { 
            DialogResult warningPopUp = MessageBox.Show("この棚に他の棚の RFID コードが含まれている場合、他の棚の RFID コードは削除されます。登録してもよろしいですか？", "確認ダイアログ", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (warningPopUp == DialogResult.Yes)
                {
                    Console.WriteLine(Session.productPos);
                    Console.WriteLine("");
                    Wait wait = new Wait();
                    wait.Visible = true;
                    string shelfName = cbShelf.Text;
                    Task.Run(() => ApiSetSmartShelfSetting(shelfName)).Wait();
                    messageFromApp.Text += DateTime.Now.ToString("hh:mm:ss ") + api_message + "\n";
                    wait.Visible = false;
                    DialogResult confirmResult = MessageBox.Show(api_message, "結果", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
            }

        }

        private void btnLoad_Click(object sender, EventArgs e)
        {

            resetLabel(1);
            Task.Run(() => ApiGetImage()).Wait();
            Wait wait = new Wait();
            wait.Visible = true;
            string shelfName = cbShelf.Text;
            Task.Run(() => ApiGetSmartShelfSetting(shelfName)).Wait();
            updatePictureBox();
            updateName();
            wait.Visible = false;
            messageFromApp.Text += DateTime.Now.ToString("hh:mm:ss") + " Finish load image \n";
            btnLoad.BackColor = Color.ForestGreen;
            if (btnCheck.BackColor == Color.ForestGreen || btnScan.BackColor == Color.ForestGreen)
            {
                btnCheck.Text = "CHECK";
                btnCheck.BackColor = Color.RoyalBlue;
                checkTimer.Stop();
                btnScan.Text = "SCAN";
                btnScan.BackColor = Color.RoyalBlue;
                locationTimer.Stop();

            }
            //Session.productPos.Keys.ToList().ForEach(x => Console.WriteLine("Data is " + x.ToString()+ (Session.productPos[x] as ProductPos).isbn));

        }
        public Bitmap changeOpacity(Bitmap pic, int opacity)
        {
            for (int w = 0; w < pic.Width; w++)
            {
                for (int h = 0; h < pic.Height; h++)
                {
                    Color c = pic.GetPixel(w, h);
                    Color newC = Color.FromArgb(opacity, c);
                    pic.SetPixel(w, h, newC);
                }
            }
            return pic;
        }

        private void btnCheckInterval_Click(object sender, EventArgs e)
        {
            btnCheck.Text = btnCheck.Text == "CHECK" ? "CHECKING..." : "CHECK";
            if (btnCheck.Text == "CHECKING...")
            {
                Wait wait = new Wait();
                wait.Visible = true;
                updateStatus();
                wait.Visible = false;
                checkTimer.Start();
            }
            else
            {
                checkTimer.Stop();
            }
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            btnCheck.Text = btnCheck.Text == "CHECK" ? "CHECKING..." : "CHECK";
            if (btnCheck.Text == "CHECKING...")
            {
                Wait wait = new Wait();
                wait.Visible = true;
                updateStatus();
                wait.Visible = false;
                btnCheck.BackColor = Color.ForestGreen;

                if (btnLoad.BackColor == Color.ForestGreen || btnScan.BackColor == Color.ForestGreen)
                {
                    btnLoad.BackColor = Color.RoyalBlue;

                    btnScan.Text = "SCAN";
                    btnScan.BackColor = Color.RoyalBlue;
                    locationTimer.Stop();
                }
                checkTimer.Start();
            }
            else
            {
                checkTimer.Stop();
                btnCheck.Text = "CHECK";
                btnCheck.BackColor = Color.RoyalBlue;
            }
        }

        private PictureBox getPictureBoxByName(string name)
        {
            foreach (PictureBox pic in ImageLayer.Controls.OfType<PictureBox>())
            {
                if (pic.Name == name)
                {
                    return pic;
                }
            }
            return null;
        }

        private void cbShelf_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session.TcpHost = null;
            if (Session.TcpShelfHost_Dictionary.ContainsKey(cbShelf.Text.ToString()))
            {
                Session.TcpHost = Session.TcpShelfHost_Dictionary[cbShelf.Text.ToString()];
                Session.nameOfShelf = cbShelf.Text.ToString();
            }
            else
            {
                //Do nothing
            }
        }


        private void Front_FormClosed(object sender, FormClosedEventArgs e)
        {
            try {
                opos.OPOS_StopReading(Session.OPOSRFID1);
            }
            catch (Exception) {}
        }

        private void Check_timer(object sender, EventArgs e)
        {
            Session.time_check--;
            if(Session.time_check == 0)
            {
                updateStatus();
                Session.time_check = (int)Int64.Parse(txtInterval.Text);
                api_message = " Check status complete";
                messageFromApp.Text += DateTime.Now.ToString("hh:mm:ss") + api_message + "\n";
            }
            btnCheck.Text = "CHECKING..." + "\r\n" + Session.time_check.ToString() + "s";
        }

        private void txtInterval_TextChanged(object sender, EventArgs e)
        {
            if (txtInterval.Text != "")
            {
                Session.time_check = (int)Int64.Parse(txtInterval.Text);
            }
        }

        private void txtInterval_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Verify that the pressed key isn't CTRL or any non-numeric digit
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // If you want, you can allow decimal (float) numbers
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void txtJan_TextChanged(object sender, EventArgs e)
        {

        }

        public static List<SimpleTcpClient> connectedTcpHosts = new List<SimpleTcpClient>();
        static bool CreateConnect()
        {
            bool result = false;
            try
            {
                // instantiate
                // Stop scan another 

                int temp = Session.TcpHost.Count();
                for (int i = 0; i < temp; i++)
                {
                    multiConnections = new SimpleTcpClient(Session.TcpHost[i]);
                    multiConnections.Events.Connected += Connected;
                    //multiConnections.Events.Disconnected += Disconnected;
                    multiConnections.Events.DataReceived += DataReceived;
                    //multiConnections.Events.DataSent += Events_DataSent;
                    multiConnections.Connect();
                    multiConnections.Send("ACTION_REGISTER_SHELF_END");
                    Thread.Sleep(50);
                    multiConnections.Send("ACTION_REGISTER_SHELF_START");
                    connectedTcpHosts.Add(multiConnections);
                    
                }
                result = true;
            }
            catch (Exception )
            {
                Console.WriteLine("Bug is here");
                //log error
            }

            return result;
        }

        static bool CloseConnect()
        {
            bool result = false;
            try
            {
                // instantiate

                int temp = Session.TcpHost.Count();
                for (int i = 0; i < temp; i++)
                {
                    multiConnections = new SimpleTcpClient(Session.TcpHost[i]);
                    multiConnections.Events.Connected += Connected;
                    multiConnections.Events.DataReceived += DataReceived;
                    multiConnections.Connect();
                    multiConnections.Send("ACTION_REGISTER_SHELF_END");
                    multiConnections.Disconnect();

                }
                result = true;
            }
            catch (Exception)
            {
                Console.WriteLine("Bug is here");
                //log error
            }

            return result;
        }


        private static void Events_DataSent(object sender, DataSentEventArgs e)
        {
            
        }

        private static void DataReceived(object sender, SuperSimpleTcp.DataReceivedEventArgs e)
        {
            Console.WriteLine($"[{e.IpPort}] {Encoding.UTF8.GetString(e.Data)}");
         }

        private static void Disconnected(object sender, ConnectionEventArgs e)
        {
            Console.WriteLine($"*** Server {e.IpPort} disconnected");
        }

        private static void Connected(object sender, ConnectionEventArgs e)
        {
            Console.WriteLine($"*** Server {e.IpPort} connected");
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            // new 20220910: get product from local file
            Session.dataListLocal = ShelfLocal.ConvertCSVtoDataTable(Session.path_shelfpro_local);
            resetLabel(1);
            Task.Run(() => ApiClearRawData()).Wait();
            Session.scan_mode = true;
            btnScan.Text = btnScan.Text == "SCAN" ? "SCANNING..." : "SCAN";
            if (btnScan.Text == "SCANNING...")
            {
                Session.time_set_location = (int)Int64.Parse(txtLocation.Text);
                bool result = false;
                result = CreateConnect();
                btnScan.BackColor = Color.ForestGreen;
                if (btnLoad.BackColor == Color.ForestGreen || btnCheck.BackColor == Color.ForestGreen)
                {
                    btnLoad.BackColor = Color.RoyalBlue;
                    btnCheck.Text = "CHECK";
                    btnCheck.BackColor = Color.RoyalBlue;
                    checkTimer.Stop();
                }
                
                if (result == true)
                {
                    locationTimer.Start();
                    messageFromApp.Text += DateTime.Now.ToString("hh:mm:ss ") + "Success connected to SmartShelf scanning app" + "\n";
                } else
                {
                    messageFromApp.Text += DateTime.Now.ToString("hh:mm:ss ") + "Failed to connect to SmartShelf scanning app" + "\n";
                }


            }
            else
            {
                locationTimer.Stop();
                btnScan.Text = "SCAN";
                btnScan.BackColor = Color.RoyalBlue;

            }
        }

        private void locationTimer_Tick(object sender, EventArgs e)
        {
            Session.time_set_location--;
            btnScan.Text = "SCANNING..." + "\r\n" + Session.time_set_location.ToString() + "s";
            if (Session.time_set_location == 0)
            {
                //Send stop singal here => Need Handle exeption when not connect
                Wait wait = new Wait();
                wait.Visible = true;
                bool result = CloseConnect();

                if (result == true)
                {
                    messageFromApp.Text += String.Format("{0}Stop shelf complete \n", DateTime.Now.ToString("hh:mm:ss "));
                }
                else
                {
                    messageFromApp.Text += String.Format("{0}Failed to stop shelf \n", DateTime.Now.ToString("hh:mm:ss "));
                }
                locationTimer.Stop();
                btnScan.Text = "SCAN";
                btnScan.BackColor = Color.RoyalBlue;
                string shelfName = cbShelf.Text;
                Task.Run(() => ApiResetLocation()).Wait();
                Task.Run(() => ApiGetSmartShelfLocation(shelfName)).Wait(); //Get RFID row col
                Task.Run(() => ApiRFIDtoJan_Sync()).Wait(); // => Tìm JAN

                foreach (string key in Session.productPos.Keys)
                {
                    //new 20220910: get product from local file
                    ShelfProduct productLocal = ShelfLocal.getProductByRFID(Session.productPos[key].RFIDcode, Session.dataListLocal);

                    if (productLocal != null)
                    {
                        Session.productPos[key].link_image = productLocal.path_img;
                        Session.productPos[key].product_name = productLocal.goods_name;

                        if (Session.product.isbn != "")
                        {
                            foreach (PictureBox pictureBox_Items in ImageLayer.Controls.OfType<PictureBox>())
                            {
                                if (Session.productPos.Keys.Contains(pictureBox_Items.Name))
                                {
                                    pictureBox_Items.LoadAsync(Session.product.link_image);
                                }
                            }
                        }
                    }
                    else
                    {
                        Task.Run(() => ApiGetDataFromBQ_Sync(key, Session.productPos[key].Jancode)).Wait();
                        Task.Run(() => ApiGetImageLocal(key, Session.productPos[key].Jancode)).Wait();
                    }


                }
                updateView();
                //Insert data got
                Task.Run(() => ApiInsertMoreInfoSmartShelf()).Wait();
                //Reload data to screen
                //Task.Run(() => ApiGetSmartShelfLocation(shelfName)).Wait(); //Get RFID row col link image, pro....
                updateView();
                updatePictureBox();
                updateName_Scan();

                Session.time_set_location = (int)Int64.Parse(txtLocation.Text);
                api_message = " Set location complete";
                messageFromApp.Text += DateTime.Now.ToString("hh:mm:ss") + api_message + "\n";
                wait.Visible = false;
                Session.scan_mode = false;
            }
            

        }

        private void txtLocation_TextChanged(object sender, EventArgs e)
        {
            if (txtLocation.Text != "")
            {
                Session.time_check = (int)Int64.Parse(txtInterval.Text);
            }

        }

        private void txtLocation_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Verify that the pressed key isn't CTRL or any non-numeric digit
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // If you want, you can allow decimal (float) numbers
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }




        private void All_txt_click(object sender, EventArgs e)
        {
            if (btnLoad.BackColor != Color.ForestGreen)
            {
                TextBox txtBox_Items = (TextBox)sender;
                string shelfName = cbShelf.Text;
                int row = (int)Int64.Parse(txtBox_Items.Name.Substring(8, 1));
                int col = (int)Int64.Parse(txtBox_Items.Name.Substring(10, 1));
                Task<JObject> getData = Task.Run(() => ApiGetSmartShelfLocationByCol(shelfName, col, row));
                JObject JsonData = getData.Result;
                DataTable dt = new DataTable();
                DataColumn dc = new DataColumn();

                //dt.Columns.Add("link_image");
                dt.Columns.Add(new DataColumn("link_image", typeof(Bitmap)));
                dt.Columns.Add("jancode");
                dt.Columns.Add("product_name");
                dt.Columns.Add("EPC");

                DataRow NewRow;
                DataGridView gridView = new DataGridView();

                //gridView.modf(gridView.detailData);
                foreach (var item in JsonData["data"])
                {

                    //IMPORTANCE: GET DATA FROM BQ
                    //=======================================================================================//
                    Task<string> jan_task = Task.Run(() => ApiRFIDtoJan_Scan((string)item["EPC"]));
                    string jan_result = jan_task.Result;
                    ShelfProduct productLocal = ShelfLocal.getProductByRFID((string)item["EPC"], Session.dataListLocal);
                    string image = "";
                    string product_name_bq_result = "";
                    string isbn_bq_result = "";

                    if (productLocal != null)
                    {
                        image = productLocal.path_img;
                        product_name_bq_result = productLocal.goods_name;
                        isbn_bq_result = productLocal.jancode;
                    }
                    else
                    {
                        Task<(string, string, string)> bq_task = Task.Run(() => ApiGetDataFromBQ_Sync((string)item["EPC"], jan_result));
                        product_name_bq_result = bq_task.Result.Item2;
                        isbn_bq_result = bq_task.Result.Item3;
                        Task<string> image_task = Task.Run(() => ApiGetImageLocal_ForGrid(isbn_bq_result));
                        image = image_task.Result;
                    }

                    NewRow = dt.NewRow();
                    string temp;
                    temp = image == null ? "noimage.png" : image;
                    if (temp == "")
                    {
                        temp = "noimage.png";
                    }

                    if (CheckValidUrl(temp))
                    {
                        string url = GetImage(image, (string)item["EPC"]);
                        Session.product.link_image = url;
                        pictureBox.Load(url);
                        //pictureBox.Load(temp);
                        var bmp = (Bitmap)pictureBox.Image;
                        Bitmap objBitmap = new Bitmap(bmp, new Size(220, 300));
                        NewRow[0] = objBitmap;
                    }
                    else
                    {

                        Image base64_convert = LoadImage(temp);
                        Bitmap objBitmap = new Bitmap(base64_convert, new Size(220, 300));
                        var bmp = (Bitmap)base64_convert;
                        NewRow[0] = objBitmap;
                    }

                    NewRow[1] = jan_result;
                    NewRow[2] = product_name_bq_result;
                    NewRow[3] = (string)item["EPC"];
                    dt.Rows.Add(NewRow);

                }
                gridView.detailData.DataSource = dt;
                gridView.Show();
                gridView.detailData.ClearSelection();

            }

        }

        private void btnMore_Click(object sender, EventArgs e)
        {
            Setting settingForm = new Setting();
            settingForm.Show();
        }
    }
}
