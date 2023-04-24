using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Shelf_Register
{
    public class API
    {

        public static async Task ApiGetSmartShelfNames()
        {
            try
            {
                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Config.address_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    api_key = Config.api_key
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await api_client.PostAsync(Config.get_smart_shelf_names, content);

                if (result.IsSuccessStatusCode)
                {

                    string resultContent = await result.Content.ReadAsStringAsync();
                    JObject JsonData = JObject.Parse(resultContent);

                    foreach (var item in JsonData["data"])
                    {
                        Config.smart_shelf_names.Add(item.ToString());
                    }

                    Global.apiMessage = (string)JsonData["message"];
                    Global.apiStatus = (string)JsonData["code"];
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
        public static async Task ApiGetDataFromBQ()
        {
            try
            {
                Global.product = new Global.ProductData();
                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Config.bquery_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    api_key = Config.api_key,
                    jancode = Global.barcode,
                    search_mode = 2,
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await api_client.PostAsync(Config.bquery_sub, content);
                if (result.IsSuccessStatusCode)
                {
                    string resultContent = await result.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(resultContent);
                    Global.apiStatus = (string)data["code"];
                    Global.apiMessage = (string)data["message"];
                    if (Global.apiStatus.Equals("00"))
                    {
                        Global.product.ccode = (string)data["data"]["c_code"] == null ? "//" : (string)data["data"]["c_code"];
                        Global.product.artist_name = (string)data["data"]["artist_name"] == "" ? "//" : (string)data["data"]["artist_name"];
                        Global.product.artist_kana = (string)data["data"]["artist_kana"] == "" ? "//" : (string)data["data"]["artist_kana"];
                        Global.product.Jancode = Global.barcode;
                        Global.product.goods_name = (string)data["data"]["goods_name"];
                        Global.product.goods_name_kana = (string)data["data"]["goods_name_kana"];
                        Global.product.media_cd = (string)data["data"]["media_cd"];
                        Global.product.genreCD = (string)data["data"]["genre_cd"];
                        Global.product.price = (int)data["data"]["price"];
                        Global.product.tax_rate = 1.1;
                        Global.product.price_intax = Convert.ToInt32(Math.Floor(Global.product.price * Global.product.tax_rate));
                        Global.product.cost_rate = (float)data["data"]["cost_rate"];
                        Global.product.makerCD = (string)data["data"]["publisher_cd"];
                        Global.product.maker_name = (string)data["data"]["publisher_name"];
                        Global.product.maker_name_kana = (string)data["data"]["publisher_name_kana"];
                        Global.product.selling_date = (string)data["data"]["selling_date"];
                        Global.product.isbn = (string)data["data"]["isbn"];
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

        public static async Task<bool> ApiLoadPositionMSTAntena(string nameOfShelf)
        {
            try
            {
                Config.antenaLoadList = new List<CheckBox>();
                Global.settingPosition = new List<Global.SettingAntena>();
                Config.antenaNoList = new List<string>();
                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Config.address_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    api_key = Config.api_key,
                    shelf_no = nameOfShelf
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await api_client.PostAsync(Config.load_position_mst_antena, content);

                if (result.IsSuccessStatusCode)
                {

                    string resultContent = await result.Content.ReadAsStringAsync();
                    JObject JsonData = JObject.Parse(resultContent);

                    foreach (var item in JsonData["data"])
                    {
                        int row_api = Int32.Parse(item["row"].ToString());
                        int col_api = Int32.Parse(item["scan_col_start"].ToString());
                        string antenaIndex = (string)item["antena_index"];
                        string antenaNo = (string)item["antena_no"];
                        Config.antenaNoList.Add(antenaIndex + "," + antenaNo);
                        Global.settingPosition.Add(new Global.SettingAntena { row = row_api, col = col_api });

                        //foreach (CheckBox checkItem in Setting.settingLayer.Controls.OfType<CheckBox>())
                        foreach (CheckBox checkItem in Global.settingForm.settingLayer.Controls.OfType<CheckBox>())
                        {
                            if (checkItem.Name == antenaIndex)
                            {
                                Config.antenaLoadList.Add(checkItem);
                            }
                        }
                    }
                    return true;
                }
                else
                {
                    Console.WriteLine(result);
                    return false;
                }

            }
            catch (Exception)
            {
                Console.WriteLine("Failed to load table MST Antena - ApiLoadPositionMSTAntena \n");
                return false;
            }
        }
        public static async Task<bool> ApiClearPositionMSTAntena(string nameOfShelf)
        {
            try
            {

                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Config.address_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string json = "";

                json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    api_key = Config.api_key,
                    shelf_no = nameOfShelf
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await api_client.PostAsync(Config.clear_position_mst_antena, content);
                if (result.IsSuccessStatusCode)
                {
                    string resultContent = await result.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(resultContent);
                }
                else
                {
                }
                return true;


            }
            catch (Exception)
            {
                Console.WriteLine("Failed to clear table MST Antena - ApiUpdatePositionMSTAntena");
                return false;
            }
        }

        public static async Task<bool> ApiUpdatePositionMSTAntena(string antenaIndex, int scan_col_start, string antenaNo)
        {
            //Hanle shelfNo
            string shelfNo = Config.nameOfShelf;

            //Handle row 
            int row = 0;

            row = Utilities.getRowbyAntenName(antenaIndex);

            //Handle col
            int col = 7;

            //Handle scan_col_end
            int scan_col_end = 7;

            try
            {

                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Config.address_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string json = "";

                json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    api_key = Config.api_key,
                    shelf_no = shelfNo,
                    antena_index = antenaIndex,
                    antena_no = antenaNo,
                    row = row,
                    col = col,
                    scan_col_start = scan_col_start,
                    scan_col_end = scan_col_end
                }

                );

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await api_client.PostAsync(Config.update_position_mst_antena, content);


                if (result.IsSuccessStatusCode)
                {

                    string resultContent = await result.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(resultContent);
                    Console.WriteLine(resultContent);
                    //Global.apiMessage = (string)data["message"];
                    //api_status = (string)data["code"];
                    return true;
                }
                else
                {
                    Console.WriteLine(result);
                    return false;
                }

            }
            catch (Exception)
            {
                Console.WriteLine("Failed to update table MST Antena - ApiUpdatePositionMSTAntena");
                return false;
            }

        }
        public static async Task ApiGetSmartShelfStatus(string name, string rfid)
        {
            try
            {
                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Config.address_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    api_key = Config.api_key,
                    rfid = rfid
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await api_client.PostAsync(Config.rfid_to_status_smartshelf, content);


                if (result.IsSuccessStatusCode)
                {
                    string resultContent = await result.Content.ReadAsStringAsync();
                    JObject JsonData = JObject.Parse(resultContent);
                    Global.apiStatus = (string)JsonData["code"];
                    Global.productPos[name].status = Global.apiStatus;
                    Global.apiMessage = (string)JsonData["message"];
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

        public static async Task<(string, string, string)> ApiGetDataFromBQ_Sync(string key, string jancode)
        {
            try
            {
                Global.product = new Global.ProductData();
                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Config.bquery_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    api_key = Config.api_key,
                    jancode = jancode,
                    search_mode = 2,
                });


                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await api_client.PostAsync(Config.bquery_sub, content);
                if (result.IsSuccessStatusCode)
                {
                    string resultContent = await result.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(resultContent);

                    Global.apiStatus = (string)data["code"];
                    Global.apiMessage = (string)data["message"];
                    if (Global.apiStatus.Equals("00"))
                    {
                        if (Global.productPos.Keys.Contains(key))
                        {
                            Global.productPos[key].isbn = (string)data["data"]["isbn"];
                            Global.productPos[key].product_name = (string)data["data"]["goods_name"];
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


                }
                else
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

        public static async Task ApiGetImageLocal(string key, string isbn)
        {
            try
            {

                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Config.address_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string json = "";

                json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    api_key = Config.api_key,
                    isbn = isbn
                }
                );

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await api_client.PostAsync(Config.image_api_local, content);


                if (result.IsSuccessStatusCode)
                {
                    string resultContent = await result.Content.ReadAsStringAsync();
                    JObject api_result = JObject.Parse(resultContent);

                    Global.apiMessage = (string)api_result["message"];
                    Global.apiStatus = (string)api_result["code"];

                    if (Global.apiStatus.Equals("00"))
                    {
                        string base64 = (string)api_result["base64"];
                        if (base64 != null)
                        {
                            //Session.productPos[key].link_image = base64;
                            //Handle get image link from BQ for Cuong
                            Global.productPos[key].link_image_bq = base64;
                            Image loadBase64 = Utilities.LoadImage(base64);

                            // new 20220910: get product from local file
                            // save base64 image to local
                            string name = Path.Combine(Config.static_img_folder, Global.productPos[key].RFIDcode + ".jpg");

                            File.WriteAllBytes(name, Convert.FromBase64String((string)api_result["base64"]));
                            ShelfProduct new_product = new ShelfProduct();
                            new_product.jancode = Global.productPos[key].Jancode;
                            new_product.goods_name = Global.productPos[key].product_name;
                            new_product.path_img = name;
                            new_product.rfid = Global.productPos[key].RFIDcode;


                            ShelfLocal.UpdateLocalCSV(Config.path_shelfpro_local, new_product);

                            Global.productPos[key].link_image = name;
                            // Displayed in the user interface
                            if (Global.product.isbn != "")
                            {
                                foreach (PictureBox pictureBox_Items in Global.mainForm.ImageLayer.Controls.OfType<PictureBox>())
                                {
                                    if (Global.productPos.Keys.Contains(pictureBox_Items.Name))
                                    {
                                        pictureBox_Items.Image = loadBase64;
                                    }
                                }
                            }
                        }
                        else
                        {
                            Global.productPos[key].link_image = (string)api_result["cover"];
                            Console.WriteLine("Cover is", (string)api_result["cover"]);
                            // Download image to local and update CSV file
                            string url = (string)api_result["cover"];
                            string name = Path.Combine(Config.static_img_folder, string.Format("{0}.jpg", Global.productPos[key].RFIDcode));
                            bool result_download = ShelfLocal.DownloadImage(url, name, ImageFormat.Jpeg);
                            Console.WriteLine("Download image is sucess: " + result_download);
                            ShelfProduct new_product = new ShelfProduct();
                            new_product.jancode = Global.productPos[key].Jancode;
                            new_product.goods_name = Global.productPos[key].product_name;
                            new_product.path_img = name;
                            new_product.rfid = Global.productPos[key].RFIDcode;


                            ShelfLocal.UpdateLocalCSV(Config.path_shelfpro_local, new_product);

                            if (Global.product.isbn != "")
                            {
                                foreach (PictureBox pictureBox_Items in Global.mainForm.ImageLayer.Controls.OfType<PictureBox>())
                                {
                                    if (Global.productPos.Keys.Contains(pictureBox_Items.Name))
                                    {
                                        pictureBox_Items.LoadAsync(Global.product.link_image);
                                    }
                                }

                            }
                            else
                            {
                                Global.mainForm.pictureBox.Load(Const.no_image);
                            }
                        }

                    }
                    else if (Global.apiStatus.Equals("02"))
                    {
                        Global.productPos[key].link_image = (string)api_result["cover"];
                        // Displayed in the user interface
                        if (Global.product.isbn != "")
                        {
                            foreach (PictureBox pictureBox_Items in Global.mainForm.ImageLayer.Controls.OfType<PictureBox>())
                            {
                                if (Global.productPos.Keys.Contains(pictureBox_Items.Name))
                                {
                                    pictureBox_Items.LoadAsync(Global.product.link_image);
                                }
                            }

                        }
                        else
                        {
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

        public static async Task<string> ApiGetImageLocal_ForGrid(string isbn)
        {
            try
            {

                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Config.address_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string json = "";

                json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    api_key = Config.api_key,
                    isbn = isbn
                }
                );


                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await api_client.PostAsync(Config.image_api_local, content);


                if (result.IsSuccessStatusCode)
                {
                    string resultContent = await result.Content.ReadAsStringAsync();
                    JObject api_result = JObject.Parse(resultContent);
                    Global.apiMessage = (string)api_result["message"];
                    Global.apiStatus = (string)api_result["code"];

                    if (Global.apiStatus.Equals("00"))
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
                    else if (Global.apiStatus.Equals("02"))
                    {
                        return Const.no_image;
                    }
                }
                else
                {
                    return Const.no_image;
                }
            }
            catch (Exception) 
            {
                Console.WriteLine("Failed to get image \n");
                return Const.no_image;
            }
            return Const.no_image;
        }

        public static async Task ApiGetImage()
        {
            try
            {
                Global.mainForm.pictureBox.Load(Const.blank_image);
                Global.barcode_state = false;
                Config.image_sub = "isbn=";
                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Config.image_api);
                var builder = new UriBuilder(Config.image_api);
                builder.Query = Config.image_sub + Global.product.isbn;
                var url = builder.ToString();
                var res = await api_client.GetAsync(url);
                var content = await res.Content.ReadAsStringAsync();

                // Extract value from response data
                JArray jsonArray = JArray.Parse(content);
                if (jsonArray[0].ToString() == "")
                {
                    if (Global.product.Jancode != "")
                    {
                        Global.mainForm.pictureBox.Load(Const.no_image);
                    }
                    else
                    {
                        Global.mainForm.pictureBox.Load(Const.blank_image);
                    }
                }
                dynamic data = JObject.Parse(jsonArray[0].ToString());

                if (content != "")
                {
                    if (data.summary.cover != "")
                    {
                        Global.product.link_image = data.summary.cover;
                        // Displayed in the user interface
                        if (Global.product.isbn != "")
                        {
                            Global.mainForm.pictureBox.LoadAsync(Global.product.link_image);
                        }
                        else
                        {
                            Global.mainForm.pictureBox.Load(Const.no_image);
                        }
                    }
                    else
                    {
                        if (Global.mainForm.txtRfid.Text != "")
                        {
                            Global.mainForm.pictureBox.Load(Const.no_image);
                        }
                        else
                        {
                            Global.mainForm.pictureBox.Load(Const.blank_image);

                        }
                    }
                }

            }
            catch (Exception)
            {
                Console.WriteLine("Failed to get image - ApiGetImage \n");

            }
        }

        public static async Task ApiSetSmartShelfSetting(string dpp_shelf_name)
        {
            try
            {

                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Config.address_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string json = "";

                foreach (PictureBox pic in Global.mainForm.ImageLayer.Controls.OfType<PictureBox>())
                {
                    if (Global.productPos.Keys.Contains(pic.Name))
                    {

                        if (Global.productPos[pic.Name].link_image == Const.no_image)
                        {
                            Global.productPos[pic.Name].link_image = "";
                        }

                        string temp = Global.productPos[pic.Name].link_image;
                        //Nếu base 64 OK
                        //Nếu link Online OK => Chuyển thành link local
                        //Nếu link local => Covert sang base64
                        string imageToDatabase = "";
                        if (Utilities.CheckValidUrlNoLocal(Global.productPos[pic.Name].link_image))
                        {
                            imageToDatabase = Utilities.ImageToBase64_Online(Global.productPos[pic.Name].link_image);
                        }
                        else if (Global.productPos[pic.Name].link_image != "")
                        {
                            // Check phải là base64 chưa => Nếu chưa mới gọi hàm ImageToBase64

                            if (!Utilities.IsBase64(Global.productPos[pic.Name].link_image))
                            {
                                imageToDatabase = Utilities.ImageToBase64(Global.productPos[pic.Name].link_image);
                            } else
                            {
                                imageToDatabase = Global.productPos[pic.Name].link_image;
                            }
                        }

                        json = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            api_key = Config.api_key,
                            dpp_shelf_pos = Int32.Parse(Global.productPos[pic.Name].shelf_pos),
                            dpp_shelf_col_pos = Int32.Parse(Global.productPos[pic.Name].shelf_col_pos),
                            dpp_jan_cd = Global.productPos[pic.Name].Jancode,
                            dpp_rfid_cd = Global.productPos[pic.Name].RFIDcode,
                            dpp_isbn = Global.productPos[pic.Name].isbn,
                            dpp_product_name = Global.productPos[pic.Name].product_name,
                            dpp_scaner_name = Global.mainForm.txtScanner.Text,
                            dpp_shelf_name = dpp_shelf_name,
                            dpp_image_url = imageToDatabase
                        }

                        );
                    }
                    else
                    {
                        json = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            api_key = Config.api_key,
                            dpp_shelf_pos = Int32.Parse(pic.Name.Substring(11, 1)),
                            dpp_shelf_col_pos = Int32.Parse(pic.Name.Substring(13, 1)),
                            dpp_jan_cd = "",
                            dpp_rfid_cd = "",
                            dpp_isbn = "",
                            dpp_product_name = "",
                            dpp_scaner_name = Global.mainForm.txtScanner.Text,
                            dpp_shelf_name = dpp_shelf_name,
                            dpp_image_url = ""
                        });
                    }

                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var result = await api_client.PostAsync(Config.set_smart_shelf_setting, content);
                    Console.WriteLine(json);

                    if (result.IsSuccessStatusCode)
                    {

                        string resultContent = await result.Content.ReadAsStringAsync();
                        JObject data = JObject.Parse(resultContent);
                        Global.apiMessage = (string)data["message"];
                        Global.apiStatus = (string)data["code"];
                        Config.smart_shelf_names.Add(dpp_shelf_name);
                    }
                    else
                    {

                    }

                }
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to set Smart Self setting");
            }

        }

        public static async Task ApiInsertMoreInfoSmartShelf()
        {
            try
            {

                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Config.address_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string json = "";
                foreach (PictureBox pic in Global.mainForm.ImageLayer.Controls.OfType<PictureBox>())
                {
                    if (Global.productPos.Keys.Contains(pic.Name))
                    {
                        if (Global.productPos[pic.Name].link_image == Const.no_image)
                        {
                            Global.productPos[pic.Name].link_image = "";
                        }
                        json = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            api_key = Config.api_key,
                            EPC = Global.productPos[pic.Name].RFIDcode,
                            jancode = Global.productPos[pic.Name].Jancode,
                            product_name = Global.productPos[pic.Name].product_name,
                            link_image = Global.productPos[pic.Name].link_image
                        }
                        );
                    }
                    else
                    {
                        //Do nothing
                    }


                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var result = await api_client.PostAsync(Config.insert_more_info_smartshelf, content);


                    if (result.IsSuccessStatusCode)
                    {

                        string resultContent = await result.Content.ReadAsStringAsync();
                        JObject data = JObject.Parse(resultContent);
                        Console.WriteLine(resultContent);
                        Global.apiMessage = (string)data["message"];
                        Global.apiStatus = (string)data["code"];
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

        public static async Task<string> ApiRFIDtoJan_Scan(string rfids)
        {

            try
            {
                var array_rfids = new string[] { rfids };
                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Config.address_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    api_key = Config.api_key,
                    rfid = array_rfids

                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await api_client.PostAsync(Config.rfmaster_sub_rfids_to_jans, content);

                if (result.IsSuccessStatusCode)
                {

                    string resultContent = await result.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(resultContent);
                    Console.WriteLine(resultContent);
                    Global.apiMessage = (string)data["message"];
                    Global.apiStatus = (string)data["code"];
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

        public static async Task ApiResetLocation()
        {
            try
            {

                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Config.address_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string json = "";

                json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    api_key = Config.api_key,
                }
                );


                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await api_client.PostAsync(Config.sub_reset_smartshelf, content);


                if (result.IsSuccessStatusCode)
                {

                    string resultContent = await result.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(resultContent);
                    Console.WriteLine(resultContent);
                    Global.apiMessage = (string)data["message"];
                    Global.apiStatus = (string)data["code"];
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

        public static async Task ApiGetSmartShelfLocation(string dpp_shelf_name)
        {
            try
            {

                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Config.address_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    api_key = Config.api_key,
                    shelf_no = dpp_shelf_name
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await api_client.PostAsync(Config.get_smartshelf_location, content);

                if (result.IsSuccessStatusCode)
                {

                    string resultContent = await result.Content.ReadAsStringAsync();
                    JObject JsonData = JObject.Parse(resultContent);

                    foreach (var item in JsonData["data"])
                    {
                        int col = Int32.Parse(item["col"].ToString());
                        int row = Int32.Parse(item["row"].ToString());
                        Global.ProductPos data = new Global.ProductPos
                        {

                            Jancode = (string)item["jancode"],
                            RFIDcode = (string)item["EPC"],
                            shelf_col_pos = col.ToString(),
                            shelf_pos = row.ToString(),
                            product_name = (string)item["product_name"],
                            link_image = (string)item["link_image"]
                        };

                        string name = Config.positionPos.FirstOrDefault(x => x.Value.col == col && x.Value.row == row).Key;
                        Global.productPos[name] = data;
                    }

                    Global.apiMessage = (string)JsonData["message"];
                    Global.apiStatus = (string)JsonData["code"];
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

        public static async Task ApiRFIDtoJan_Sync()
        {

            try
            {
                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Config.address_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //var arrayOfAllKeys = Session.productPos.Keys.ToArray();

                List<string> rfids = new List<string>();

                foreach (string key in Global.productPos.Keys)
                {
                    if (Global.productPos[key].RFIDcode != "")
                    {
                        rfids.Add(Global.productPos[key].RFIDcode);

                    }
                }

                string json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    api_key = Config.api_key,
                    rfid = rfids,
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await api_client.PostAsync(Config.rfmaster_sub_rfids_to_jans, content);

                if (result.IsSuccessStatusCode)
                {

                    string resultContent = await result.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(resultContent);
                    Console.WriteLine(resultContent);
                    Global.apiMessage = (string)data["message"];
                    Global.apiStatus = (string)data["code"];


                    foreach (var item in data["data"])
                    {
                        //Handle here
                        foreach (string key in Global.productPos.Keys)
                        {
                            if (Global.productPos[key].RFIDcode == (string)item["rfid"])
                            {
                                Global.productPos[key].Jancode = (string)item["jancode_1"];
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

        public static async Task ApiGetSmartShelfSetting(string dpp_shelf_name)
        {
            try
            {

                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Config.address_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    api_key = Config.api_key,
                    dpp_shelf_name = dpp_shelf_name
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await api_client.PostAsync(Config.get_smart_shelf_setting, content);

                if (result.IsSuccessStatusCode)
                {

                    string resultContent = await result.Content.ReadAsStringAsync();
                    JObject JsonData = JObject.Parse(resultContent);

                    foreach (var item in JsonData["data"])
                    {
                        int col = Int32.Parse(item["dpp_shelf_col_pos"].ToString());
                        int row = Int32.Parse(item["dpp_shelf_pos"].ToString());
                        Global.ProductPos data = new Global.ProductPos
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

                        string name = Config.positionPos.FirstOrDefault(x => x.Value.col == col && x.Value.row == row).Key;
                        Global.productPos[name] = data;
                    }

                    Global.apiMessage = (string)JsonData["message"];
                    Global.apiStatus = (string)JsonData["code"];
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


        public static async Task ApiClearRawData()
        {
            try
            {
                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Config.address_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    api_key = Config.api_key
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await api_client.PostAsync(Config.sub_clear_raw_data, content);

                if (result.IsSuccessStatusCode)
                {
                    string resultContent = await result.Content.ReadAsStringAsync();
                    JObject JsonData = JObject.Parse(resultContent);
                    Global.apiMessage = (string)JsonData["message"];
                    Global.apiStatus = (string)JsonData["code"];
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

        public static async Task<JObject> ApiGetSmartShelfLocationByCol(string dpp_shelf_name, int col, int row)
        {
            try
            {

                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Config.address_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    api_key = Config.api_key,
                    shelf_no = dpp_shelf_name,
                    col = col,
                    row = row
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await api_client.PostAsync(Config.get_smartshelf_location_by_col, content);

                if (result.IsSuccessStatusCode)
                {

                    string resultContent = await result.Content.ReadAsStringAsync();
                    JObject JsonData = JObject.Parse(resultContent);

                    Global.apiMessage = (string)JsonData["message"];
                    Global.apiStatus = (string)JsonData["code"];
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

        public static async Task ApiRFIDtoJan()
        {

            try
            {
                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Config.address_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var rfids = new string[] { Global.rfidcode };
                string json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    api_key = Config.api_key,
                    rfid = rfids

                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await api_client.PostAsync(Config.rfmaster_sub_rfids_to_jans, content);

                if (result.IsSuccessStatusCode)
                {

                    string resultContent = await result.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(resultContent);
                    Console.WriteLine(resultContent);
                    Global.apiMessage = (string)data["message"];
                    Global.apiStatus = (string)data["code"];
                    Global.barcode = (string)data["data"][0]["jancode_1"];
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

        public static async Task ApiInsertRawData()
        {

            try
            {
                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Config.address_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var rfids = new string[] { Global.rfidcode };
                string json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    api_key = Config.api_key,
                    data = Global.rawDataList

                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await api_client.PostAsync(Config.insert_raw_data, content);

                if (result.IsSuccessStatusCode)
                {

                    string resultContent = await result.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(resultContent);
                    Console.WriteLine(resultContent);
                    Global.apiMessage = (string)data["message"];
                    Global.apiStatus = (string)data["code"];
                }
                else
                {
                    Console.WriteLine(result);
                    Global.rawDataList.Clear();
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Connect to API Server Failed - ApiInsertRawData");
            }
        }


    }
}
