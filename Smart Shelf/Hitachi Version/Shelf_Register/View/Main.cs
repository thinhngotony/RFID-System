using Newtonsoft.Json.Linq;
using Rfid.Helper.Common;
using Rfid.Helper.Extensions;
using Rfid.Helper.Services.Mq;
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
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Shelf_Register
{
    public partial class Front : Form
    {
        public static SimpleTcpClient multiConnections;
        public string barcode = "";
        public Front()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.Manual;
            this.CenterToScreen();
            //KillProcessRFID.ControlKill.KillRFIDProcess_diffCurrent();
            init();


            txtRfid.Text = Global.rfidcode;
            txtJan.Text = Global.barcode;
            txtScanner.Text = Config.device_name;
            Global.SetForm(this);
        }

        private void init()
        {
            Config.readConfig();
            Global.dataRegister = new MqClient(Rfid.Helper.Enums.MqClientAppName.REGISTER_MASTER_APP, Config.rabbitMQ);
            Global.dataLocation = new MqClient(Rfid.Helper.Enums.MqClientAppName.LOCATION_SHELF_APP, Config.rabbitMQ);
            foreach (PictureBox pictureBox_Items in ImageLayer.Controls.OfType<PictureBox>())
            {
                pictureBox_Items.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox_Items.Load(Const.blank_image);
            }
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox.Load(Const.blank_image);

            Task.Run(() => API.ApiGetSmartShelfNames()).Wait();
            cbShelf.DataSource = Config.smart_shelf_names;
            cbShelf.SelectedItem = "SHELF 1";
            txtInterval.Text = Config.time_check.ToString();
            txtLocation.Text = Config.time_set_location.ToString();

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
            if (!Directory.Exists(Config.static_img_folder))
            {
                Directory.CreateDirectory(Config.static_img_folder);
            }

            if (Config.TcpShelfHost_Dictionary.ContainsKey(cbShelf.Text.ToString()))
            {
                Config.TcpHost = Config.TcpShelfHost_Dictionary[cbShelf.Text.ToString()];
            }
            else
            {
                messageFromApp.Text += DateTime.Now.ToString("hh:mm:ss") + ": Can't find IP for TcpHosts \n";
            }

            Global.TimerRegister.Enabled = false;
            Global.EthernetForm.UpdateViewTimerRegister();
            Connect_Ethernet.Register();
            Connect_Ethernet.Location();


        }

        public void updateView()
        {

            //txtRfid.Invoke((MethodInvoker)(() => txtRfid.Text = Global.rfidcode));
            txtRfid.PerformSafely(() =>
            {
                txtRfid.Text = Global.rfidcode;

            });

            if (txtRfid.Text != "")
            {
                txtJan.PerformSafely(() =>
                {
                    txtJan.Text = Global.barcode;

                });
                txtName.PerformSafely(() =>
                {
                    txtName.Text = Global.product.goods_name;

                });
            }

            
        }

        public void updateName()
        {
            foreach (string key in Global.productPos.Keys)
            {
                if (Global.productPos[key].RFIDcode != "")
                {
                    PictureBox pic = getPictureBoxByName(key);
                    string key_text = key.Replace("pictureBox", "textBox");
                    foreach (var txtBox_Items in ImageLayer.Controls.OfType<TextBox>())
                    {
                        if (txtBox_Items.Name == key_text && txtBox_Items.Text == "")
                        {
                            txtBox_Items.BackColor = Color.PaleTurquoise;
                            txtBox_Items.Text += Global.productPos[key].Jancode;
                            txtBox_Items.Text += "\r\n";
                            txtBox_Items.Text += Global.productPos[key].product_name;
                            deleteName();
                        }
                        //Handle for swap data
                        else if (txtBox_Items.Name == key_text && txtBox_Items.Text != "")
                        {
                            txtBox_Items.Text = "";
                            txtBox_Items.BackColor = Color.PaleTurquoise;
                            txtBox_Items.Text += Global.productPos[key].Jancode;
                            txtBox_Items.Text += "\r\n";
                            txtBox_Items.Text += Global.productPos[key].product_name;
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
            foreach (string key in Global.productPos.Keys)
            {
                if (Global.productPos[key].RFIDcode != "")
                {
                    PictureBox pic = getPictureBoxByName(key);
                    string key_text = key.Replace("pictureBox", "textBox");
                    foreach (var txtBox_Items in ImageLayer.Controls.OfType<TextBox>())
                    {
                        if (txtBox_Items.Name == key_text)
                        {
                            txtBox_Items.BackColor = Color.PaleTurquoise;
                            txtBox_Items.Text += "\r\n";
                            txtBox_Items.Text += Global.productPos[key].Jancode;
                            txtBox_Items.Text += "\r\n";
                            txtBox_Items.Text += Global.productPos[key].product_name;
                            deleteName_Scan();
                        }
                        //Handle for swap data
                        else if (txtBox_Items.Name == key_text && txtBox_Items.Text != "")
                        {
                            txtBox_Items.Text = "";
                            txtBox_Items.BackColor = Color.PaleTurquoise;
                            txtBox_Items.Text += Global.productPos[key].Jancode;
                            txtBox_Items.Text += "\r\n";
                            txtBox_Items.Text += Global.productPos[key].product_name;
                            deleteName();
                        }
                    }
                }
            }

        }

        public void updateStatus()
        {

            foreach (string key in Global.productPos.Keys)
            {
                //PictureBox pic = Image_Items as PictureBox;
                Task.Run(() => API.ApiGetSmartShelfStatus(key, Global.productPos[key].RFIDcode)).Wait();

                if (Global.productPos[key].status == "00")
                {
                    string key_text = key.Replace("pictureBox", "textBox");
                    foreach (var txtBox_Items in ImageLayer.Controls.OfType<TextBox>())
                    {
                        if (txtBox_Items.Name == key_text)
                        {
                            txtBox_Items.BackColor = Color.LightGreen;
                        }
                        Config.status_mode = true;
                    }
                }
                else
                {
                    //Bugging
                    PictureBox pic = getPictureBoxByName(key);
                    if (pic != null)
                    {
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
                        Config.status_mode = true;
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
                if (Global.productPos.Keys.Contains(pic.Name))
                {
                    if (string.IsNullOrEmpty(Global.productPos[pic.Name].link_image))
                    {

                        if (Global.productPos[pic.Name].RFIDcode != "")
                        {
                            Global.productPos[pic.Name].link_image = Const.no_image;
                            pic.Load(Const.no_image);
                        }
                        else
                        {
                            pic.Load(Const.blank_image);
                        }
                    }
                    else
                    {
                        string url = GetImage(Global.productPos[pic.Name].link_image, Global.productPos[pic.Name].RFIDcode);
                        Global.productPos[pic.Name].link_image = url;
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



        public void deleteName()
        {
            foreach (var txtBox_Items in ImageLayer.Controls.OfType<TextBox>())
            {
                foreach (var picBox_Items in ImageLayer.Controls.OfType<PictureBox>())
                {
                    if (!Global.productPos.Keys.Contains(picBox_Items.Name) && Config.mappingTextBox[picBox_Items.Name] == txtBox_Items.Name && txtBox_Items.Text != "")
                    {
                        txtBox_Items.Text = "";
                        if (Config.status_mode == false)
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
                    if (!Global.productPos.Keys.Contains(picBox_Items.Name) && Config.mappingTextBox[picBox_Items.Name] == txtBox_Items.Name && txtBox_Items.Text != "")
                    {
                        txtBox_Items.Text = "";
                        if (txtBox_Items.Text == "")
                        {

                            txtBox_Items.Text = txtBox_Items.Name.Substring(8, 3);
                        }
                        if (Config.status_mode == false)
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
                Global.lastRFID = "";
                Global.viewDataList = null;

                Global.product = new Global.ProductData();
                Global.productPos = new Dictionary<string, Global.ProductPos>();
                Global.barcode = "";
                Global.rfidcode = "";
                txtRfid.Text = "";
                txtJan.Text = "";
                txtName.Text = "";
                pictureBox.Load(Const.blank_image);

                foreach (PictureBox pictureBox_Items in ImageLayer.Controls.OfType<PictureBox>())
                {
                    pictureBox_Items.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox_Items.Load(Const.blank_image);
                }
                foreach (TextBox txtBox_Items in ImageLayer.Controls.OfType<TextBox>())
                {
                    txtBox_Items.Text = "";
                    txtBox_Items.BackColor = Color.PaleTurquoise;
                }
                Config.status_mode = false;
                Config.scan_mode = false;

            }
            else if (mode == 2)
            {
                foreach (PictureBox pictureBox_Items in ImageLayer.Controls.OfType<PictureBox>())
                {
                    pictureBox_Items.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox_Items.Load(Const.blank_image);
                }
                foreach (TextBox txtBox_Items in ImageLayer.Controls.OfType<TextBox>())
                {
                    txtBox_Items.Text = "";
                    txtBox_Items.BackColor = Color.PaleTurquoise;
                }
                Config.status_mode = false;
            }
        }




        private static Boolean isBase64(String str)
        {
            String patern = "^([A-Za-z0-9+/]{4})*([A-Za-z0-9+/]{3}=|[A-Za-z0-9+/]{2}==)?$";
            return Regex.IsMatch(str, patern);
        }



        public static string GetImage(string data, string rfid)
        {
            //Check is base64
            try
            {
                if (!isBase64(data))
                {
                    if (data == Const.no_image || data == Const.blank_image)
                    {
                        return Const.no_image;
                    }
                    return data;
                }

                byte[] bytes = Convert.FromBase64String(data);

                Image image;
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    image = Image.FromStream(ms);
                    string name = Path.Combine(Config.static_img_folder, rfid + ".jpg");
                    if (File.Exists(name))
                    {
                        return name;
                    } else
                    {
                        image.Save(name, ImageFormat.Jpeg);
                        return name;
                    }

            }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                MessageBox.Show(e.Message);

            }
            return data;

        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (btnScan.BackColor != Color.ForestGreen)
            {
                btnConnect.Text = btnConnect.Text == "READ TAG" ? "READING..." : "READ TAG";
                if (btnConnect.Text == "READING...")
                {
                    Global.readedTagsRepo.Clear();
                    Global.dataRegister.EnableReceivedData = true;
                    Global.TimerRegister.Enabled = true;
                    btnConnect.BackColor = Color.ForestGreen;
                }
                else
                {
                    Global.dataRegister.EnableReceivedData = false;
                    Global.TimerRegister.Enabled = false;
                    btnConnect.Text = "READ TAG";
                    btnConnect.BackColor = Color.RoyalBlue;

                }
            }


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

                if (btnConnect.Text == "StopReading")
                {
                    Task.Run(() => API.ApiRFIDtoJan()).Wait();
                    Task.Run(() => API.ApiGetDataFromBQ()).Wait();
                    Task.Run(() => API.ApiGetImage()).Wait();
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
            if (txtRfid.Text!= "")
            {
                Wait wait = new Wait();
                wait.Visible = true;
                Task.Run(() => API.ApiRFIDtoJan()).Wait();
                Task.Run(() => API.ApiGetDataFromBQ()).Wait();
                wait.Visible = false;
                string image = "";
                Task<string> result = Task.Run(() => API.ApiGetImageLocal_ForGrid(Global.barcode));
                image = result.Result;
                string temp;
                temp = image == null ? Const.no_image : image;

                if (temp == "")
                {
                    temp = Const.no_image;
                }

                Global.product.link_image = temp;

                if (CheckValidUrl(temp))
                {
                    pictureBox.Load(temp);
                }
                else
                {
                    Image base64_convert = Utilities.LoadImage(temp);
                    Bitmap objBitmap = new Bitmap(base64_convert, new Size(220, 300));
                    var bmp = (Bitmap)base64_convert;
                    pictureBox.Image = objBitmap;

                }
                updateView();
            } else
            {
                txtJan.Text = "";
                txtName.Text = "";
                pictureBox.Load(Const.blank_image);
                Global.product = null;
                Global.product = new Global.ProductData();
            }




        }

        private void messageFromApp_onChanged(object sender, EventArgs e)
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
            Global.ProductPos temp = new Global.ProductPos();
            Global.ProductPos data = new Global.ProductPos
            {
                Jancode = Global.product.Jancode,
                RFIDcode = txtRfid.Text,
                shelf_col_pos = Config.positionPos[choosingImage.Name].col.ToString(), //choosingImage.Name.Substring(11, 1),
                shelf_pos = Config.positionPos[choosingImage.Name].row.ToString(),
                product_name = Global.product.goods_name,
                shelf_name = cbShelf.Text,
                isbn = Global.product.isbn,
                link_image = Global.product.link_image,
                picture_box = choosingImage
            };

            if (Global.product.link_image == "" && txtRfid.Text != "" )
            {
                Global.product.link_image = Const.no_image;
            }


            if (Global.productPos.Keys.Contains(choosingImage.Name) && Global.productPos[choosingImage.Name].RFIDcode != "")
            {

                if (!Global.productPos.Keys.Contains("temp"))
                {

                    Global.productPos["temp"] = Global.productPos[choosingImage.Name];
                    Global.productPos["temp"].picture_box = choosingImage;

                    if (Global.productPos["temp"].link_image == "")
                    {
                        Global.productPos["temp"].link_image = Const.blank_image;
                    }

                    // Add focus to textBox                         ;
                    foreach (var txtBox_Items in ImageLayer.Controls.OfType<TextBox>())
                    {
                        string mapTextBox = Global.productPos["temp"].picture_box.Name.Replace("pictureBox", "textBox");
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
                                if (txtBox_Items.Name == mapLastTextBox && Config.status_mode == false)
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

                }
                else
                {
                    //Swap function
                    PictureBox now = sender as PictureBox;

                    if (now.Name != "" && now.Name != lastChoose.Name)
                    {
                        //Gán dữ liệu ảnh đang chọn vào biến datanew
                        //Lưu ý khi swap thì vị trí lưu phải là vị trí của thằng temp => Nếu ko chỉ dữ liệu thay đổi, vị trí không thay đổi
                        Global.ProductPos datanew = new Global.ProductPos
                        {
                            Jancode = Global.productPos[choosingImage.Name].Jancode,
                            RFIDcode = Global.productPos[choosingImage.Name].RFIDcode,
                            shelf_col_pos = Config.positionPos[Global.productPos["temp"].picture_box.Name].col.ToString(), //choosingImage.Name.Substring(11, 1),
                            shelf_pos = Config.positionPos[Global.productPos["temp"].picture_box.Name].row.ToString(),
                            product_name = Global.productPos[choosingImage.Name].product_name,
                            shelf_name = cbShelf.Text,
                            isbn = Global.productPos[choosingImage.Name].isbn,
                            link_image = Global.productPos[choosingImage.Name].link_image,
                            picture_box = choosingImage
                        };

                        //Lưu vị trí riêng so với data
                        Global.productPos["temp"].shelf_col_pos = Global.productPos[choosingImage.Name].shelf_col_pos;
                        Global.productPos["temp"].shelf_pos = Global.productPos[choosingImage.Name].shelf_pos;

                        //Gán dữ liệu của biến temp vào ảnh đang chọn
                        Global.productPos[choosingImage.Name] = Global.productPos["temp"];

                        //Lấy dữ liệu của biến datanew vào lại picturebox của temp
                        Global.productPos[Global.productPos["temp"].picture_box.Name] = datanew;
                        if (Config.scan_mode == true)
                        {
                            updateName_Scan();
                        }
                        else
                        {
                            updateName();
                        }
                        updatePictureBox(); ;
                        lastChoose = choosingImage;
                        Global.productPos.Remove("temp");
                    }
                }

            }
            if (Config.scan_mode == false)
            {
                // WORKING HERE 
                // IF NOT EXIST TEMP!
                if (!Global.productPos.Keys.Contains("temp"))
                {
                    if (lastChoose == null)
                    {
                        if (choosingImage.ImageLocation == Const.blank_image)
                        {
                            // Delete duplicate
                            string pictureBoxDuplicateName = Global.productPos.FirstOrDefault(duplicateItems => duplicateItems.Value.RFIDcode == txtRfid.Text).Key;
                            if (pictureBoxDuplicateName != null)
                            {
                                foreach (PictureBox duplicateItems in ImageLayer.Controls.OfType<PictureBox>())
                                {
                                    if (duplicateItems.Name == pictureBoxDuplicateName && Global.productPos[duplicateItems.Name].RFIDcode != "")
                                    {
                                        if (Config.status_mode == true)
                                        {
                                            duplicateItems.Load(Const.blank_image);
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
                                            Global.productPos.Remove(pictureBoxDuplicateName);
                                            break;
                                        }
                                        else
                                        {
                                            duplicateItems.Load(Const.blank_image);
                                            Global.productPos.Remove(pictureBoxDuplicateName);
                                            break;
                                        }
                                    }
                                }
                            }


                        }

                        //Load data from panel to screen 
                        if (Global.product.link_image != "")
                        {
                            // Check base64
                            string url = GetImage(Global.product.link_image, Global.rfidcode);
                            Global.product.link_image = url;
                            choosingImage.Load(Global.product.link_image);
                            url = "";
                            Global.productPos[choosingImage.Name] = data;
                            lastChoose = choosingImage;

                        }
                        else
                        {
                            Console.WriteLine("Không có hình không làm gì cả");
                        }
                    }
                    // Trường hợp click lần thứ 2 trở đi
                    // Handle for scan mode = false 
                    else if (choosingImage.ImageLocation == Const.blank_image)
                    {
                        // Kiểm tra dữ liệu đã có chưa, có rồi thì xóa 
                        string pictureBoxDuplicateName = Global.productPos.FirstOrDefault(t => t.Value.RFIDcode == txtRfid.Text).Key;
                        if (pictureBoxDuplicateName != null)
                        {
                            foreach (PictureBox pic in ImageLayer.Controls.OfType<PictureBox>())
                            {
                                // Tìm thấy picturebox trùng data
                                if (pic.Name == pictureBoxDuplicateName)
                                {
                                    // Chỉnh giao diện tối trong chế độ scan và xóa thằng trước đó
                                    if (Config.status_mode == true)
                                    {
                                        pic.Load(Const.blank_image);
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
                                        Global.productPos.Remove(pictureBoxDuplicateName);
                                        break;
                                    }
                                    else
                                    {
                                        //Xóa thằng trước đó 
                                        pic.Load(Const.blank_image);
                                        Global.productPos.Remove(pictureBoxDuplicateName);
                                        break;
                                    }

                                }
                            }
                        }

                        if (Global.product.link_image != "")
                        {
                            string url = GetImage(Global.product.link_image, Global.rfidcode);
                            Global.product.link_image = url;
                            choosingImage.Load(Global.product.link_image);
                            url = "";
                            Global.productPos[choosingImage.Name] = data;
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
                else if (choosingImage.ImageLocation == Const.blank_image)
                {
                    string url = GetImage(Global.productPos["temp"].link_image, Global.productPos["temp"].RFIDcode);
                    Global.productPos["temp"].link_image = url;
                    choosingImage.Load(url);
                    Global.productPos[choosingImage.Name] = Global.productPos["temp"];
                    //Continue handle duplicate image
                    Global.productPos["temp"].picture_box.Load(Const.blank_image);
                    ////Load black screen for last choose
                    if (btnCheck.BackColor == Color.ForestGreen)
                    {
                        Image img = changeOpacity(new Bitmap(Global.productPos["temp"].picture_box.Image), 100);
                        Global.productPos["temp"].picture_box.Image = img;
                        string key_text = Global.productPos["temp"].picture_box.Name.Replace("pictureBox", "textBox");
                        foreach (var txtBox_Items in ImageLayer.Controls.OfType<TextBox>())
                        {
                            if (txtBox_Items.Name == key_text)
                            {
                                txtBox_Items.BackColor = Color.White;
                            }
                        }
                    }

                    //Add data to dictionary
                    data.Jancode = Global.productPos[choosingImage.Name].Jancode;
                    data.RFIDcode = Global.productPos[choosingImage.Name].RFIDcode;
                    data.product_name = Global.productPos[choosingImage.Name].product_name;
                    data.isbn = Global.productPos[choosingImage.Name].isbn;
                    data.link_image = Global.productPos[choosingImage.Name].link_image;

                    Global.productPos[choosingImage.Name] = data;
                    Global.productPos.Remove(Global.productPos["temp"].picture_box.Name);
                    Global.productPos.Remove("temp");
                    updateName();
                }
                else
                {
                }
            }
            else if (Config.scan_mode == true)
            {
                // Handle for Scan mode
                if (Global.productPos.Keys.Contains("temp"))
                {
                    if (choosingImage.ImageLocation == Const.blank_image)
                    {
                        // WORKING HERE 
                        // IF EXIST TEMP
                        string url = GetImage(Global.product.link_image, Global.rfidcode);
                        Global.product.link_image = url;
                        choosingImage.Load(Global.product.link_image);
                        url = "";

                        //Continue handle duplicate image
                        Global.productPos["temp"].picture_box.Load(Const.blank_image);

                        ////Load black screen for last choose
                        if (btnCheck.BackColor == Color.ForestGreen)
                        {
                            Image img = changeOpacity(new Bitmap(Global.productPos["temp"].picture_box.Image), 100);
                            Global.productPos["temp"].picture_box.Image = img;
                            string key_text = Global.productPos["temp"].picture_box.Name.Replace("pictureBox", "textBox");
                            foreach (var txtBox_Items in ImageLayer.Controls.OfType<TextBox>())
                            {
                                if (txtBox_Items.Name == key_text)
                                {
                                    txtBox_Items.BackColor = Color.White;
                                }
                            }
                        }

                        //Add data to dictionary
                        data.Jancode = Global.productPos[choosingImage.Name].Jancode;
                        data.RFIDcode = Global.productPos[choosingImage.Name].RFIDcode;
                        data.product_name = Global.productPos[choosingImage.Name].product_name;
                        data.isbn = Global.productPos[choosingImage.Name].isbn;
                        data.link_image = Global.productPos[choosingImage.Name].link_image;
                        Global.productPos[choosingImage.Name] = data;
                        Global.productPos.Remove(Global.productPos["temp"].picture_box.Name);
                        Global.productPos.Remove("temp");
                        updateName_Scan();

                        foreach (TextBox txtBox_Items in ImageLayer.Controls.OfType<TextBox>())
                        {
                            txtBox_Items.BackColor = Color.PaleTurquoise;
                        }

                        updatePictureBox();
                    }
                    else
                    {
                        Console.WriteLine("Not handle");
                    }

                }
            }
        }

        private void pictureBox_DoubleClick(object sender, EventArgs e)
        {
            PictureBox deleteImage = sender as PictureBox;
            if (deleteImage.ImageLocation != Const.blank_image)
            {
                Console.WriteLine(Const.blank_image);
                DialogResult confirmResult = MessageBox.Show("これを削除してもよろしいですか", "確認ダイアログ", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

                if (confirmResult == DialogResult.Yes)
                {
                    deleteImage.Load(Const.blank_image);
                    Global.productPos.Remove(deleteImage.Name);
                    deleteName();
                    //ProductPos z = Session.productPos[deleteImage.Name];
                    //Session.productPos.Keys.ToList().ForEach(z => Console.WriteLine("Result after delete" + z.ToString()));
                    if (Global.productPos.Keys.Contains("temp"))
                    {
                        Global.productPos.Remove(Global.productPos["temp"].picture_box.Name);
                        Global.productPos.Remove("temp");
                    }
                }

            }
            else
            {
                Console.WriteLine("Nothing to delete");
            }
        }


        private void btnRegister_Click(object sender, EventArgs e)
        {
            Global.readedTagsRepo.Clear();
            Global.dataRegister.EnableReceivedData = false;
            Global.productPos.Keys.ToList().ForEach(x => Console.WriteLine("Data is " + x.ToString()+ (Global.productPos[x] as Global.ProductPos).Jancode));
            if (cbShelf.Text == "")
            {
                DialogResult infoPopUp = MessageBox.Show("棚名フィールドにデータがありません。入力または選択してください!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                DialogResult warningPopUp = MessageBox.Show("この棚に他の棚の RFID コードが含まれている場合、他の棚の RFID コードは削除されます。登録してもよろしいですか？", "確認ダイアログ", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (warningPopUp == DialogResult.Yes)
                {
                    Wait wait = new Wait();
                    wait.Visible = true;
                    string shelfName = cbShelf.Text;
                    Task.Run(() => API.ApiSetSmartShelfSetting(shelfName)).Wait();
                    messageFromApp.Text += DateTime.Now.ToString("hh:mm:ss ") + Global.apiMessage + "\n";
                    Network.CreateConnect();
                    Network.CloseConnect();
                    wait.Visible = false;
                    DialogResult confirmResult = MessageBox.Show(Global.apiMessage, "結果", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);

                }
            }
            if (btnConnect.BackColor ==  Color.ForestGreen)
            Global.dataRegister.EnableReceivedData = true;
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {

            resetLabel(1);
            Task.Run(() => API.ApiGetImage()).Wait();
            Wait wait = new Wait();
            wait.Visible = true;
            string shelfName = cbShelf.Text;
            Task.Run(() => API.ApiGetSmartShelfSetting(shelfName)).Wait();
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
            Config.TcpHost = null;
            if (Config.TcpShelfHost_Dictionary.ContainsKey(cbShelf.Text.ToString()))
            {
                Config.TcpHost = Config.TcpShelfHost_Dictionary[cbShelf.Text.ToString()];
                Config.nameOfShelf = cbShelf.Text.ToString();
            }
            else
            {
                //Do nothing
            }
        }


        private void Front_FormClosed(object sender, FormClosedEventArgs e)
        {
            //try
            //{
            //    Network.CloseConnect();

            //}
            //catch (Exception) { }
        }

        private void Check_timer(object sender, EventArgs e)
        {
            Config.time_check--;
            if (Config.time_check == 0)
            {
                updateStatus();
                Config.time_check = (int)Int64.Parse(txtInterval.Text);
                Global.apiMessage = " Check status complete";
                messageFromApp.Text += DateTime.Now.ToString("hh:mm:ss") + Global.apiMessage + "\n";
            }
            btnCheck.Text = "CHECKING..." + "\r\n" + Config.time_check.ToString() + "s";
        }

        private void txtInterval_TextChanged(object sender, EventArgs e)
        {
            if (txtInterval.Text != "")
            {
                Config.time_check = (int)Int64.Parse(txtInterval.Text);
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

        private void btnScan_Click(object sender, EventArgs e)
        {
            Global.rawDataList.Clear();
            Config.dataListLocal = ShelfLocal.ConvertCSVtoDataTable(Config.path_shelfpro_local);
            resetLabel(1);
            Task.Run(() => API.ApiClearRawData()).Wait();
            Global.dataLocation.EnableReceivedData = true;
            Config.scan_mode = true;
            btnScan.Text = btnScan.Text == "SCAN" ? "SCANNING..." : "SCAN";
            if (btnScan.Text == "SCANNING...")
            {
                Config.time_set_location = (int)Int64.Parse(txtLocation.Text);
                btnScan.BackColor = Color.ForestGreen;
                if (btnLoad.BackColor == Color.ForestGreen || btnCheck.BackColor == Color.ForestGreen || btnConnect.BackColor == Color.ForestGreen)
                {
                    btnLoad.BackColor = Color.RoyalBlue;
                    btnCheck.Text = "CHECK";
                    btnCheck.BackColor = Color.RoyalBlue;
                    checkTimer.Stop();

                    btnConnect.Text = "READ TAG";
                    btnConnect.BackColor = Color.RoyalBlue;

                    Global.dataRegister.EnableReceivedData = false;
                    Global.TimerRegister.Enabled = false;

                }

                locationTimer.Start();
                messageFromApp.Text += DateTime.Now.ToString("hh:mm:ss ") + "Success connected to scanning antena" + "\n";
            }
            else
            {
                    Global.dataLocation.EnableReceivedData = false;
                    locationTimer.Stop();
                    btnScan.Text = "SCAN";
                    btnScan.BackColor = Color.RoyalBlue;
            }
        }

        private void locationTimer_Tick(object sender, EventArgs e)
        {
            Config.time_set_location--;
            btnScan.Text = "SCANNING..." + "\r\n" + Config.time_set_location.ToString() + "s";
            if (Config.time_set_location == 0)
            {
                Wait wait = new Wait();
                wait.Visible = true;
                locationTimer.Stop();
                btnScan.Text = "SCAN";
                btnScan.BackColor = Color.RoyalBlue;

                Global.dataLocation.EnableReceivedData = false;
                string shelfName = cbShelf.Text;
                Task.Run(() => API.ApiInsertRawData()).Wait();
                Task.Run(() => API.ApiResetLocation()).Wait();
                Task.Run(() => API.ApiGetSmartShelfLocation(shelfName)).Wait(); 
                Task.Run(() => API.ApiRFIDtoJan_Sync()).Wait();

                foreach (string key in Global.productPos.Keys)
                {
                    ShelfProduct productLocal = ShelfLocal.getProductByRFID(Global.productPos[key].RFIDcode, Config.dataListLocal);

                    if (productLocal != null)
                    {
                        Global.productPos[key].link_image = productLocal.path_img;
                        Global.productPos[key].product_name = productLocal.goods_name;

                        if (Global.product.isbn != "")
                        {
                            foreach (PictureBox pictureBox_Items in ImageLayer.Controls.OfType<PictureBox>())
                            {
                                if (Global.productPos.Keys.Contains(pictureBox_Items.Name))
                                {
                                    pictureBox_Items.LoadAsync(Global.product.link_image);
                                }
                            }
                        }
                    }
                    else
                    {
                        Task.Run(() => API.ApiGetDataFromBQ_Sync(key, Global.productPos[key].Jancode)).Wait();
                        Task.Run(() => API.ApiGetImageLocal(key, Global.productPos[key].Jancode)).Wait();
                    }

                }
                //updateView();
                Task.Run(() => API.ApiInsertMoreInfoSmartShelf()).Wait();
                //updateView();
                updatePictureBox();
                updateName_Scan();

                Config.time_set_location = (int)Int64.Parse(txtLocation.Text);
                Global.apiMessage = " Set location complete";
                messageFromApp.Text += DateTime.Now.ToString("hh:mm:ss") + Global.apiMessage + "\n";
                wait.Visible = false;
                Config.scan_mode = false;
            }


        }

        private void txtLocation_TextChanged(object sender, EventArgs e)
        {
            if (txtLocation.Text != "")
            {
                Config.time_check = (int)Int64.Parse(txtInterval.Text);
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
            if (btnLoad.BackColor != Color.ForestGreen && btnConnect.BackColor != Color.ForestGreen)
            {
                TextBox txtBox_Items = (TextBox)sender;
                string shelfName = cbShelf.Text;
                int row = (int)Int64.Parse(txtBox_Items.Name.Substring(8, 1));
                int col = (int)Int64.Parse(txtBox_Items.Name.Substring(10, 1));
                Task<JObject> getData = Task.Run(() => API.ApiGetSmartShelfLocationByCol(shelfName, col, row));
                JObject JsonData = getData.Result;
                DataTable dt = new DataTable();
                DataColumn dc = new DataColumn();

                dt.Columns.Add(new DataColumn("link_image", typeof(Bitmap)));
                dt.Columns.Add("jancode");
                dt.Columns.Add("product_name");
                dt.Columns.Add("EPC");

                DataRow NewRow;
                DataGridView gridView = new DataGridView();

                foreach (var item in JsonData["data"])
                {

                    //IMPORTANCE: GET DATA FROM BQ
                    //=======================================================================================//
                    Task<string> jan_task = Task.Run(() => API.ApiRFIDtoJan_Scan((string)item["EPC"]));
                    string jan_result = jan_task.Result;
                    ShelfProduct productLocal = ShelfLocal.getProductByRFID((string)item["EPC"], Config.dataListLocal);
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
                        Task<(string, string, string)> bq_task = Task.Run(() => API.ApiGetDataFromBQ_Sync((string)item["EPC"], jan_result));
                        product_name_bq_result = bq_task.Result.Item2;
                        isbn_bq_result = bq_task.Result.Item3;
                        Task<string> image_task = Task.Run(() => API.ApiGetImageLocal_ForGrid(isbn_bq_result));
                        image = image_task.Result;
                    }

                    NewRow = dt.NewRow();
                    string temp;
                    temp = image == null ? Const.no_image : image;
                    if (temp == "")
                    {
                        temp = Const.no_image;
                    }

                    if (CheckValidUrl(temp))
                    {
                        string url = GetImage(image, (string)item["EPC"]);
                        Global.product.link_image = url;
                        pictureBox.Load(Global.product.link_image);
                        url = "";
                        //pictureBox.Load(temp);
                        var bmp = (Bitmap)pictureBox.Image;
                        Bitmap objBitmap = new Bitmap(bmp, new Size(220, 300));
                        NewRow[0] = objBitmap;
                    }
                    else
                    {

                        Image base64_convert = Utilities.LoadImage(temp);
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

        private void Front_Load(object sender, EventArgs e)
        {

        }

        private void txtJan_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox_3_1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox_1_1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
