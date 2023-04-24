using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Shelf_Register.Front;

namespace Shelf_Register
{
    public partial class Setting : Form
    {
        public Setting()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.Manual;
            this.CenterToScreen();
            init();           
        }

        public void init()
        {
            //Add textbox

            TextBox textBox1_1 = new TextBox();
            textBox1_1.Text = "";
            textBox1_1.Name = "1";
            settingLayer.Controls.Add(textBox1_1, 0, 0);


            TextBox textBox1_2 = new TextBox();
            textBox1_2.Text = "";
            textBox1_2.Name = "2";
            settingLayer.Controls.Add(textBox1_2, 8, 0);


            TextBox textBox2_1 = new TextBox();
            textBox2_1.Text = "";
            textBox2_1.Name = "3";
            settingLayer.Controls.Add(textBox2_1, 0, 1);


            TextBox textBox2_2 = new TextBox();
            textBox2_2.Text = "";
            textBox2_2.Name = "4";
            settingLayer.Controls.Add(textBox2_2, 8, 1);


            TextBox textBox3_1 = new TextBox();
            textBox3_1.Text = "";
            textBox3_1.Name = "5";
            settingLayer.Controls.Add(textBox3_1, 0, 2);


            TextBox textBox3_2 = new TextBox();
            textBox3_2.Text = "";
            textBox3_2.Name = "6";
            settingLayer.Controls.Add(textBox3_2, 8, 2);

            TextBox textBox4_1 = new TextBox();
            textBox4_1.Text = "";
            textBox4_1.Name = "7";
            settingLayer.Controls.Add(textBox4_1, 0, 3);


            TextBox textBox4_2 = new TextBox();
            textBox4_2.Text = "";
            textBox4_2.Name = "8";
            settingLayer.Controls.Add(textBox4_2, 8, 3);

            foreach (TextBox antenaNo in settingLayer.Controls.OfType<TextBox>())
            {
                //Handle textbox data
                antenaNo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txt_KeyPress);
                antenaNo.MaxLength = 2;
            }

            // Add checkbox to settingLayer - tableLayout
            CheckBox antenaNo1 = new CheckBox();
            antenaNo1.Text = "ANTENA";
            antenaNo1.Name = "1";
            settingLayer.Controls.Add(antenaNo1, 0, 0);

            CheckBox antenaNo2 = new CheckBox();
            antenaNo2.Text = "ANTENA";
            antenaNo2.Name = "2";
            settingLayer.Controls.Add(antenaNo2, 7, 0);

            CheckBox antenaNo3 = new CheckBox();
            antenaNo3.Text = "ANTENA";
            antenaNo3.Name = "3";
            settingLayer.Controls.Add(antenaNo3, 0, 1);

            CheckBox antenaNo4 = new CheckBox();
            antenaNo4.Text = "ANTENA";
            antenaNo4.Name = "4";
            settingLayer.Controls.Add(antenaNo4, 7, 1);

            CheckBox antenaNo5 = new CheckBox();
            antenaNo5.Text = "ANTENA";
            antenaNo5.Name = "5";
            settingLayer.Controls.Add(antenaNo5, 0, 2);

            CheckBox antenaNo6 = new CheckBox();
            antenaNo6.Text = "ANTENA";
            antenaNo6.Name = "6";
            settingLayer.Controls.Add(antenaNo6, 7, 2);

            CheckBox antenaNo7 = new CheckBox();
            antenaNo7.Text = "ANTENA";
            antenaNo7.Name = "7";
            settingLayer.Controls.Add(antenaNo7, 0, 3);

            CheckBox antenaNo8 = new CheckBox();
            antenaNo8.Text = "ANTENA";
            antenaNo8.Name = "8";
            settingLayer.Controls.Add(antenaNo8, 7, 3);

            foreach (CheckBox antenaIndex in settingLayer.Controls.OfType<CheckBox>())
            {
                //Handle checkbox event tick
                antenaIndex.CheckedChanged += new System.EventHandler(antenaIndexChecked);
            }

            // Add pictureBox to settingLayer
            // Solution 1: Loop all settingLayer, find position not contain checkbox and insert picturebox
            // Solution 2: Manual insert
            // Solution 3: Loop with location

            PictureBox pictureBoxSetting_1_1 = new PictureBox();
            pictureBoxSetting_1_1.Name = "1_1";
            settingLayer.Controls.Add(pictureBoxSetting_1_1, 1, 0);

            PictureBox pictureBoxSetting_1_2 = new PictureBox();
            pictureBoxSetting_1_2.Name = "1_2";
            settingLayer.Controls.Add(pictureBoxSetting_1_2, 2, 0);

            PictureBox pictureBoxSetting_1_3 = new PictureBox();
            pictureBoxSetting_1_3.Name = "1_3";
            settingLayer.Controls.Add(pictureBoxSetting_1_3, 3, 0);


            PictureBox pictureBoxSetting_1_4 = new PictureBox();
            pictureBoxSetting_1_4.Name = "1_4";
            settingLayer.Controls.Add(pictureBoxSetting_1_4, 4, 0);


            PictureBox pictureBoxSetting_1_5 = new PictureBox();
            pictureBoxSetting_1_5.Name = "1_5";
            settingLayer.Controls.Add(pictureBoxSetting_1_5, 5, 0);

            PictureBox pictureBoxSetting_1_6 = new PictureBox();
            pictureBoxSetting_1_6.Name = "1_6";
            settingLayer.Controls.Add(pictureBoxSetting_1_6, 6, 0);

            PictureBox pictureBoxSetting_2_1 = new PictureBox();
            pictureBoxSetting_2_1.Name = "2_1";
            settingLayer.Controls.Add(pictureBoxSetting_2_1, 1, 1);

            PictureBox pictureBoxSetting_2_2 = new PictureBox();
            pictureBoxSetting_2_2.Name = "2_2";
            settingLayer.Controls.Add(pictureBoxSetting_2_2, 2, 1);

            PictureBox pictureBoxSetting_2_3 = new PictureBox();
            pictureBoxSetting_2_3.Name = "2_3";
            settingLayer.Controls.Add(pictureBoxSetting_2_3, 3, 1);

            PictureBox pictureBoxSetting_2_4 = new PictureBox();
            pictureBoxSetting_2_4.Name = "2_4";
            settingLayer.Controls.Add(pictureBoxSetting_2_4, 4, 1);

            PictureBox pictureBoxSetting_2_5 = new PictureBox();
            pictureBoxSetting_2_5.Name = "2_5";
            settingLayer.Controls.Add(pictureBoxSetting_2_5, 5, 1);

            PictureBox pictureBoxSetting_2_6 = new PictureBox();
            pictureBoxSetting_2_6.Name = "2_6";
            settingLayer.Controls.Add(pictureBoxSetting_2_6, 6, 1);

            PictureBox pictureBoxSetting_3_1 = new PictureBox();
            pictureBoxSetting_3_1.Name = "3_1";
            settingLayer.Controls.Add(pictureBoxSetting_3_1, 1, 2);

            PictureBox pictureBoxSetting_3_2 = new PictureBox();
            pictureBoxSetting_3_2.Name = "3_2";
            settingLayer.Controls.Add(pictureBoxSetting_3_2, 2, 2);

            PictureBox pictureBoxSetting_3_3 = new PictureBox();
            pictureBoxSetting_3_3.Name = "3_3";
            settingLayer.Controls.Add(pictureBoxSetting_3_3, 3, 2);

            PictureBox pictureBoxSetting_3_4 = new PictureBox();
            pictureBoxSetting_3_4.Name = "3_4";
            settingLayer.Controls.Add(pictureBoxSetting_3_4, 4, 2);

            PictureBox pictureBoxSetting_3_5 = new PictureBox();
            pictureBoxSetting_3_5.Name = "3_5";
            settingLayer.Controls.Add(pictureBoxSetting_3_5, 5, 2);

            PictureBox pictureBoxSetting_3_6 = new PictureBox();
            pictureBoxSetting_3_6.Name = "3_6";
            settingLayer.Controls.Add(pictureBoxSetting_3_6, 6, 2);

            PictureBox pictureBoxSetting_4_1 = new PictureBox();
            pictureBoxSetting_4_1.Name = "4_1";
            settingLayer.Controls.Add(pictureBoxSetting_4_1, 1, 3);

            PictureBox pictureBoxSetting_4_2 = new PictureBox();
            pictureBoxSetting_4_2.Name = "4_2";
            settingLayer.Controls.Add(pictureBoxSetting_4_2, 2, 3);

            PictureBox pictureBoxSetting_4_3 = new PictureBox();
            pictureBoxSetting_4_3.Name = "4_3";
            settingLayer.Controls.Add(pictureBoxSetting_4_3, 3, 3);

            PictureBox pictureBoxSetting_4_4 = new PictureBox();
            pictureBoxSetting_4_4.Name = "4_4";
            settingLayer.Controls.Add(pictureBoxSetting_4_4, 4, 3);

            PictureBox pictureBoxSetting_4_5 = new PictureBox();
            pictureBoxSetting_4_5.Name = "4_5";
            settingLayer.Controls.Add(pictureBoxSetting_4_5, 5, 3);

            PictureBox pictureBoxSetting_4_6 = new PictureBox();
            pictureBoxSetting_4_6.Name = "4_6";
            settingLayer.Controls.Add(pictureBoxSetting_4_6, 6, 3);

            //Set event onclick for all picture box
            foreach (PictureBox pic in settingLayer.Controls.OfType<PictureBox>())
            {
                pic.Dock = DockStyle.Fill;
                pic.Size = MaximumSize;
                pic.SizeMode = PictureBoxSizeMode.StretchImage;
                pic.Load("blank_background.png");
                pic.Click += new System.EventHandler(picOnClick);
            }

            // Add button REGISTER settingLayer - tableLayout
            Button btnRegister = new Button();
            btnRegister.Text = "REGISTER";
            btnRegister.Height = 50;
            btnRegister.Width = 100;
            btnRegister.Click += new System.EventHandler(btnRegisterOnClick);
            settingLayer.Controls.Add(btnRegister, 6, 4);

            // Add button LOAD settingLayer - tableLayout
            Button btnLoad = new Button();
            btnLoad.Text = "LOAD";
            btnLoad.Height = 50;
            btnLoad.Width = 100;
            btnLoad.Click += new System.EventHandler(btnLoadOnClick);
            settingLayer.Controls.Add(btnLoad, 5, 4);

            // Add button CLEAR settingLayer - tableLayout
            Button btnClear = new Button();
            btnClear.Text = "CLEAR";
            btnClear.Height = 50;
            btnClear.Width = 100;
            btnClear.Click += new System.EventHandler(btnClearOnClick);
            settingLayer.Controls.Add(btnClear, 4, 4);
        }

        private void setPositionForRow(int row, int antena)
        {
            foreach (PictureBox pic in settingLayer.Controls.OfType<PictureBox>())
            {
                {
                    if (int.Parse(pic.Name.Substring(0, 1)) == row)
                    {
                        if (antena % 2 == 0)
                        {
                            pic.Click += new System.EventHandler(pictureBoxOnClick_Right);
                        }
                        else
                        {
                            pic.Click += new System.EventHandler(pictureBoxOnClick_Left);
                        }
                    }
                }

            }
        }

        private void unSetPositionForRow(int row, int antena)
        {
            foreach (PictureBox pic in settingLayer.Controls.OfType<PictureBox>())
            {
                {
                    if (int.Parse(pic.Name.Substring(0, 1)) == row)
                    {
                        if (antena % 2 == 0)
                        {
                            pic.Click -= new System.EventHandler(pictureBoxOnClick_Right);
                        }
                        else
                        {
                            pic.Click -= new System.EventHandler(pictureBoxOnClick_Left);
                        }
                    }
                }

            }
        }

        private void setDefaultAntenaNo(string antenaIndex, bool antenaIsSelected)
        {

            int max_antenno = 0;
            TextBox antenaNoCurent = new TextBox();
            foreach (TextBox antenaNo in settingLayer.Controls.OfType<TextBox>())
            {
                if (antenaNo.Text != "")
                {
                    if (max_antenno <= int.Parse(antenaNo.Text))
                    {
                        max_antenno = int.Parse(antenaNo.Text);
                    }
                }
                if (antenaNo.Name == antenaIndex)
                {
                    antenaNoCurent = antenaNo;
                }
            }

            if (antenaIsSelected)
            {
                if (antenaNoCurent.Text == "")
                {
                    max_antenno += 1;
                    antenaNoCurent.Text = max_antenno.ToString();         
                }
            } else
            {
                antenaNoCurent.Text = "";
            }

        }

        //EventOnClick for checkbox
        private void antenaIndexChecked(object sender, EventArgs e)
        {
            if (Session.isLoadSetting)
            {
                return;
            }
            foreach (CheckBox checkItem in settingLayer.Controls.OfType<CheckBox>())
            {
                setDefaultAntenaNo(checkItem.Name, checkItem.Checked);
                if (checkItem.Checked)
                {
                    //setDefaultAntenaNo(checkItem.Name, checkItem.Checked);
                    //switch (checkItem.Name)
                    //{
                    //    case "1":
                    //        // Make picture box in row 1 can be clicked
                    //        setPositionForRow(1, 1);
                    //        break;
                    //    case "2":
                    //        // code block
                    //        setPositionForRow(1, 2);
                    //        break;
                    //    case "3":
                    //        setPositionForRow(2, 3);
                    //        // code block
                    //        break;
                    //    case "4":
                    //        setPositionForRow(2, 4);
                    //        // code block
                    //        break;
                    //    case "5":
                    //        setPositionForRow(3, 5);
                    //        // code block
                    //        break;
                    //    case "6":
                    //        setPositionForRow(3, 6);
                    //        // code block
                    //        break;
                    //    case "7":
                    //        setPositionForRow(4, 7);
                    //        // code block
                    //        break;
                    //    case "8":
                    //        setPositionForRow(4, 8);
                    //        // code block
                    //        break;
                    //    default:
                    //        // code block
                    //        break;
                    //}
                } else //Handle unclick check box 
                {
                    //setDefaultAntenaNo(checkItem.Name, false);
                    //switch (checkItem.Text)
                    //{
                    //    case "1":
                    //        // Make picture box in row 1 cannot be clicked
                    //        unSetPositionForRow(1, 1);
                    //        break;
                    //    case "2":
                    //        unSetPositionForRow(1, 2);
                    //        break;
                    //    case "3":
                    //        unSetPositionForRow(2, 3);
                    //        break;
                    //    case "4":
                    //        unSetPositionForRow(2, 4);
                    //        break;
                    //    case "5":
                    //        unSetPositionForRow(3, 5);
                    //        break;
                    //    case "6":
                    //        unSetPositionForRow(3, 6);
                    //        break;
                    //    case "7":
                    //        unSetPositionForRow(4, 7);
                    //        break;
                    //    case "8":
                    //        unSetPositionForRow(4, 8);
                    //        break;
                    //    default:
                    //        break;
                    //}
                }
            }
        }

        private void pictureBoxOnClick_Left(object sender, EventArgs e)
        {
            PictureBox pictureBoxClicked = sender as PictureBox;
            pictureBoxClicked.Load("selected.png");
            if (pictureBoxClicked.ImageLocation == "selected.png")
            {
                pictureBoxClicked.Load("blank_background.png");
            }
            //foreach (PictureBox pic in settingLayer.Controls.OfType<PictureBox>())
            //{
            //    if (int.Parse(pic.Name.Substring(0, 1)) == int.Parse(pictureBoxClicked.Name.Substring(0, 1)))
            //    {
            //        pic.Load("blank_background.png");
            //        if (int.Parse(pic.Name.Substring(2, 1)) >= int.Parse(pictureBoxClicked.Name.Substring(2, 1)))
            //        {
            //            pic.Load("selected.png");
            //            //Call API                       
            //        }
            //    }
            //}
        }


        private void pictureBoxOnClick_Right(object sender, EventArgs e)
        {
            PictureBox pictureBoxClicked = sender as PictureBox;
            pictureBoxClicked.Load("selected.png");
            if (pictureBoxClicked.ImageLocation == "selected.png")
            {
                pictureBoxClicked.Load("blank_background.png");
            }
            //foreach (PictureBox pic in settingLayer.Controls.OfType<PictureBox>())
            //{
            //    if (int.Parse(pic.Name.Substring(0, 1)) == int.Parse(pictureBoxClicked.Name.Substring(0, 1)))
            //    {
            //        pic.Load("blank_background.png");
            //        if (int.Parse(pic.Name.Substring(2, 1)) <= int.Parse(pictureBoxClicked.Name.Substring(2, 1)))
            //        {
            //            pic.Load("selected.png");
            //        }
            //    }
            //}
        }

        private int getRowbyAntenName(string antena)
        {
            int row = 0 ;
            if (antena == "1" || antena == "2")
            {
                row = 1;
            }
            else if (antena == "3" || antena == "4")
            {
                row = 2;
            }
            else if (antena == "5" || antena == "6")
            {
                row = 3;
            }
            else if (antena == "7" || antena == "8")
            {
                row = 4;
            }

            return row;
        }

        private string getAntenaNobyAntenaIndex(string antenaIndex)
        {
            string antenaNo = "";
            foreach (TextBox antena_no in settingLayer.Controls.OfType<TextBox>())
            {
                if (antena_no.Name == antenaIndex)
                {
                    return antena_no.Text;
                }    
            }
            return antenaNo;
        }
    
        private (int, string, string) getValueToInsertMST_Working(CheckBox antenaIndex)
        {
            int scan_col_start = 0;
            int left_col = 1;
            int right_col = 6;
            int CONST_MAX_RIGHT_COL = 6;
            int CONST_MIN_LEFT_COL = 1;
            string antenaNo = getAntenaNobyAntenaIndex(antenaIndex.Name);
            int antenaRow = getRowbyAntenName(antenaIndex.Name);
            Boolean isSelectedLocation = false;

            foreach (PictureBox pic in settingLayer.Controls.OfType<PictureBox>())
            {
                int picRow = int.Parse(pic.Name.Substring(0, 1));
                int picCol = int.Parse(pic.Name.Substring(2, 1));
                if (pic.ImageLocation == "selected.png")
                {
                    if (picRow == antenaRow){
                        isSelectedLocation = true;
                        // All antena in the right
                        if (int.Parse(antenaIndex.Name) % 2 == 0)
                        {
                            if (picCol >= left_col)
                            {
                                left_col = int.Parse(pic.Name.Substring(2, 1));
                            }
                        }
                        // All antena in the left
                        else
                        {
                            if (picCol <= right_col)
                            {
                                right_col = int.Parse(pic.Name.Substring(2, 1));
                            }
                        }
                    }
                }
            }

            if (int.Parse(antenaIndex.Name) % 2 == 0)
            {
               if (isSelectedLocation == false )
               {
                    scan_col_start = CONST_MAX_RIGHT_COL;
               }
               else
               {
                    scan_col_start = left_col;
               }
                
            }
            else
            {
                if (isSelectedLocation == false)
                {
                    scan_col_start = CONST_MIN_LEFT_COL;
                }
                else
                {
                    scan_col_start = right_col;
                }
                
            }

            return (scan_col_start, antenaIndex.Name, antenaNo);
        }

        private void btnRegisterOnClick(object sender, EventArgs e)
        {
            Task<bool> resultClear = Task.Run(() => ApiClearPositionMSTAntena(Session.nameOfShelf));
            bool isSuccessClear = resultClear.Result;
            if (isSuccessClear)
            {
                bool isSuccessUpdate = false;
                foreach (CheckBox checkItem in settingLayer.Controls.OfType<CheckBox>())
                {

                    if (checkItem.Checked)
                    {
                        var (scancolstart, antenaIndex, antenaNo) = getValueToInsertMST_Working(checkItem);
                        Task<bool> resultUpdate = Task.Run(() => ApiUpdatePositionMSTAntena(antenaIndex, scancolstart, antenaNo));
                        isSuccessUpdate = resultUpdate.Result;
                    }
                }
                if (isSuccessUpdate)
                {
                    DialogResult confirmResult = MessageBox.Show("登録完了", "結果", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    DialogResult confirmResult = MessageBox.Show("登録失敗", "結果", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
            } else
            {
                DialogResult confirmResult = MessageBox.Show("登録失敗", "結果", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        private void LoadDataToScreen()
        {
            string[] antenaIndexAndNo;
            foreach (TextBox antenaNo in settingLayer.Controls.OfType<TextBox>())
            {
                foreach (string loadAntena in Session.antenaNoList)
                {
                    antenaIndexAndNo = loadAntena.Split(',');
                    if (antenaNo.Name == antenaIndexAndNo[0])
                    {
                        antenaNo.Text = antenaIndexAndNo[1];
                    }
                }
            }

            foreach (CheckBox antenaIndex in settingLayer.Controls.OfType<CheckBox>())
            {
                foreach (CheckBox loadAntena in Session.antenaLoadList)
                {
                    if (loadAntena.Name == antenaIndex.Name)
                    {
                        if (antenaIndex.Checked == false)
                        {
                            antenaIndex.Checked = true;
                        }
                    }
                }
            }

            foreach (PictureBox pic in settingLayer.Controls.OfType<PictureBox>())
            {
                foreach(var item in Session.settingPosition)
                {
                    if (int.Parse(pic.Name.Substring(2, 1)) == item.col && int.Parse(pic.Name.Substring(0, 1)) == item.row)
                    {
                        pic.Load("selected.png");
                    }
                }               
            }

        }

        private async Task<bool> ApiClearPositionMSTAntena(string nameOfShelf)
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
                    shelf_no = nameOfShelf
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await api_client.PostAsync(Session.clear_position_mst_antena, content);
                if (result.IsSuccessStatusCode)
                {
                    string resultContent = await result.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(resultContent);
                }
                else
                {
                    Console.WriteLine(result);
                }
                return true;


            }
            catch (Exception)
            {
                Console.WriteLine("Failed to clear table MST Antena - ApiUpdatePositionMSTAntena");
                return false;
            }
        }

        private async Task<bool> ApiLoadPositionMSTAntena(string nameOfShelf)
        {
            try
            {
                Session.antenaLoadList = new List<CheckBox>();
                Session.settingPosition = new List<SettingAntena>();
                Session.antenaNoList = new List<string>();
                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Session.address_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    api_key = Session.api_key,
                    shelf_no = nameOfShelf
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await api_client.PostAsync(Session.load_position_mst_antena, content);

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
                        Session.antenaNoList.Add(antenaIndex + "," + antenaNo);
                        Session.settingPosition.Add(new SettingAntena { row = row_api, col = col_api });

                        foreach (CheckBox checkItem in settingLayer.Controls.OfType<CheckBox>())
                        {
                            if (checkItem.Name == antenaIndex)
                            {
                                Session.antenaLoadList.Add(checkItem);
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

        private async Task<bool> ApiUpdatePositionMSTAntena(string antenaIndex, int scan_col_start, string antenaNo)
        {
            //Hanle shelfNo
            string shelfNo = Session.nameOfShelf;

            //Handle row 
            int row = 0;

            row = getRowbyAntenName(antenaIndex);

            //Handle col
            int col = 7;

            //Handle scan_col_end
            int scan_col_end = 7;

            try
            {

                HttpClient api_client = new HttpClient();
                api_client.BaseAddress = new Uri(Session.address_api);
                api_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string json = "";

                json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    api_key = Session.api_key,
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
                var result = await api_client.PostAsync(Session.update_position_mst_antena, content);


                if (result.IsSuccessStatusCode)
                {

                    string resultContent = await result.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(resultContent);
                    Console.WriteLine(resultContent);
                    //api_message = (string)data["message"];
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


        private void picOnClick(object sender, EventArgs e)
        {
            PictureBox pictureBoxClicked = sender as PictureBox;

            if (pictureBoxClicked.ImageLocation == "selected.png")
            {
                pictureBoxClicked.Load("blank_background.png");
            } else
            {
                pictureBoxClicked.Load("selected.png");

            }
        }

        private void btnClearOnClick(object sender, EventArgs e)
        {
            DialogResult warningPopUp = MessageBox.Show("画面上のすべてのデータが削除されます。よろしいですか?", "確認ダイアログ", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (warningPopUp == DialogResult.Yes)
            {
                reset();
            }
        }

        private void reset()
        {
            foreach (PictureBox pic in settingLayer.Controls.OfType<PictureBox>())
            {
                pic.Dock = DockStyle.Fill;
                pic.Size = MaximumSize;
                pic.SizeMode = PictureBoxSizeMode.StretchImage;
                pic.Load("blank_background.png");
            }

            foreach (CheckBox antenaIndex in settingLayer.Controls.OfType<CheckBox>())
            {
                if (antenaIndex.Checked == true)
                {
                    antenaIndex.Checked = false;
                }
            }

            foreach (TextBox antenaNo in settingLayer.Controls.OfType<TextBox>())
            {
                if (antenaNo.Text != "")
                {
                    antenaNo.Text = "";
                }
            }
        }
        private void btnLoadOnClick(object sender, EventArgs e)
        {
            reset();
            Session.isLoadSetting = true;
            Task<bool> result = Task.Run(() => ApiLoadPositionMSTAntena((Session.nameOfShelf)));
            bool isSuccess = result.Result;
            if (isSuccess)
            {
                LoadDataToScreen();
                DialogResult confirmResult = MessageBox.Show("データを正常にロード", "結果", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            } else
            {
                DialogResult confirmResult = MessageBox.Show("データの読み込みに失敗しました", "結果", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
            Session.isLoadSetting = false;

        }

        private void settingLayer_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Setting_Load(object sender, EventArgs e)
        {

        }

        private void txt_KeyPress(object sender, KeyPressEventArgs e)
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
    }
}
