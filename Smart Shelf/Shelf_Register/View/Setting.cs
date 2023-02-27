using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            Global.SetForm2(this);

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

            CheckBox antenaNo3 = new CheckBox();
            antenaNo3.Text = "ANTENA";
            antenaNo3.Name = "3";
            settingLayer.Controls.Add(antenaNo3, 0, 1);

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
            
            TextBox textBox5_1 = new TextBox();
            textBox5_1.Text = "";
            textBox5_1.Name = "9";
            settingLayer.Controls.Add(textBox5_1, 0, 4);


            TextBox textBox5_2 = new TextBox();
            textBox5_2.Text = "";
            textBox5_2.Name = "10";
            settingLayer.Controls.Add(textBox5_2, 8, 4);


            CheckBox antenaNo11 = new CheckBox();
            antenaNo11.Text = "ANTENA";
            antenaNo11.Name = "11";
            settingLayer.Controls.Add(antenaNo11, 0, 5);

            TextBox textBox6_1 = new TextBox();
            textBox6_1.Text = "";
            textBox6_1.Name = "11";
            settingLayer.Controls.Add(textBox6_1, 0, 5);


            TextBox textBox6_2 = new TextBox();
            textBox6_2.Text = "";
            textBox6_2.Name = "12";
            settingLayer.Controls.Add(textBox6_2, 8, 5);

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

            CheckBox antenaNo9 = new CheckBox();
            antenaNo9.Text = "ANTENA";
            antenaNo9.Name = "9";
            settingLayer.Controls.Add(antenaNo9, 0, 4);

            CheckBox antenaNo10 = new CheckBox();
            antenaNo10.Text = "ANTENA";
            antenaNo10.Name = "10";
            settingLayer.Controls.Add(antenaNo10, 7, 4);

            CheckBox antenaNo12 = new CheckBox();
            antenaNo12.Text = "ANTENA";
            antenaNo12.Name = "12";
            settingLayer.Controls.Add(antenaNo12, 7, 5);

            foreach (CheckBox antenaIndex in settingLayer.Controls.OfType<CheckBox>())
            {
                //Handle checkbox event tick
                antenaIndex.CheckedChanged += new System.EventHandler(antenaIndexChecked);
            }

            // Add pictureBox to settingLayer
            // Solution 1: Loop all settingLayer, find position not contain checkbox and insert picturebox
            // Solution 2: Manual insert
            // Solution 3: Loop with location

            //=======================ROW 1======================//

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

            //=======================ROW 2======================//

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

            //=======================ROW 3======================//

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

            //=======================ROW 4======================//

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

            //=======================ROW 5======================//

            PictureBox pictureBoxSetting_5_1 = new PictureBox();
            pictureBoxSetting_5_1.Name = "5_1";
            settingLayer.Controls.Add(pictureBoxSetting_5_1, 1, 4);

            PictureBox pictureBoxSetting_5_2 = new PictureBox();
            pictureBoxSetting_5_2.Name = "5_2";
            settingLayer.Controls.Add(pictureBoxSetting_5_2, 2, 4);

            PictureBox pictureBoxSetting_5_3 = new PictureBox();
            pictureBoxSetting_5_3.Name = "5_3";
            settingLayer.Controls.Add(pictureBoxSetting_5_3, 3, 4);

            PictureBox pictureBoxSetting_5_4 = new PictureBox();
            pictureBoxSetting_5_4.Name = "5_4";
            settingLayer.Controls.Add(pictureBoxSetting_5_4, 4, 4);

            PictureBox pictureBoxSetting_5_5 = new PictureBox();
            pictureBoxSetting_5_5.Name = "5_5";
            settingLayer.Controls.Add(pictureBoxSetting_5_5, 5, 4);

            PictureBox pictureBoxSetting_5_6 = new PictureBox();
            pictureBoxSetting_5_6.Name = "5_6";
            settingLayer.Controls.Add(pictureBoxSetting_5_6, 6, 4);

            //=======================ROW 6======================//

            PictureBox pictureBoxSetting_6_1 = new PictureBox();
            pictureBoxSetting_6_1.Name = "6_1";
            settingLayer.Controls.Add(pictureBoxSetting_6_1, 1, 5);

            PictureBox pictureBoxSetting_6_2 = new PictureBox();
            pictureBoxSetting_6_2.Name = "6_2";
            settingLayer.Controls.Add(pictureBoxSetting_6_2, 2, 5);

            PictureBox pictureBoxSetting_6_3 = new PictureBox();
            pictureBoxSetting_6_3.Name = "6_3";
            settingLayer.Controls.Add(pictureBoxSetting_6_3, 3, 5);

            PictureBox pictureBoxSetting_6_4 = new PictureBox();
            pictureBoxSetting_6_4.Name = "6_4";
            settingLayer.Controls.Add(pictureBoxSetting_6_4, 4, 5);

            PictureBox pictureBoxSetting_6_5 = new PictureBox();
            pictureBoxSetting_6_5.Name = "6_5";
            settingLayer.Controls.Add(pictureBoxSetting_6_5, 5, 5);

            PictureBox pictureBoxSetting_6_6 = new PictureBox();
            pictureBoxSetting_6_6.Name = "6_6";
            settingLayer.Controls.Add(pictureBoxSetting_6_6, 6, 5);

            //Set event onclick for all picture box
            foreach (PictureBox pic in settingLayer.Controls.OfType<PictureBox>())
            {
                pic.Dock = DockStyle.Fill;
                pic.Size = MaximumSize;
                pic.SizeMode = PictureBoxSizeMode.StretchImage;
                pic.Load(Const.blank_image);
                pic.Click += new System.EventHandler(picOnClick);
            }

            // Add button REGISTER settingLayer - tableLayout
            Button btnRegister = new Button();
            btnRegister.Text = "REGISTER";
            btnRegister.Height = 50;
            btnRegister.Width = 100;
            btnRegister.Click += new System.EventHandler(btnRegisterOnClick);
            settingLayer.Controls.Add(btnRegister, 6, 6);

            // Add button LOAD settingLayer - tableLayout
            Button btnLoad = new Button();
            btnLoad.Text = "LOAD";
            btnLoad.Height = 50;
            btnLoad.Width = 100;
            btnLoad.Click += new System.EventHandler(btnLoadOnClick);
            settingLayer.Controls.Add(btnLoad, 5, 6);

            // Add button CLEAR settingLayer - tableLayout
            Button btnClear = new Button();
            btnClear.Text = "CLEAR";
            btnClear.Height = 50;
            btnClear.Width = 100;
            btnClear.Click += new System.EventHandler(btnClearOnClick);
            settingLayer.Controls.Add(btnClear, 4, 6);
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
            }
            else
            {
                antenaNoCurent.Text = "";
            }

        }

        //EventOnClick for checkbox
        private void antenaIndexChecked(object sender, EventArgs e)
        {
            if (Config.isLoadSetting)
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
                }
                else //Handle unclick check box 
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
            pictureBoxClicked.Load(Const.selected);
            if (pictureBoxClicked.ImageLocation == Const.selected)
            {
                pictureBoxClicked.Load(Const.blank_image);
            }

        }


        private void pictureBoxOnClick_Right(object sender, EventArgs e)
        {
            PictureBox pictureBoxClicked = sender as PictureBox;
            pictureBoxClicked.Load(Const.selected);
            if (pictureBoxClicked.ImageLocation == Const.selected)
            {
                pictureBoxClicked.Load(Const.blank_image);
            }

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
            int antenaRow = Utilities.getRowbyAntenName(antenaIndex.Name);
            bool isSelectedLocation = false;

            foreach (PictureBox pic in settingLayer.Controls.OfType<PictureBox>())
            {
                int picRow = int.Parse(pic.Name.Substring(0, 1));
                int picCol = int.Parse(pic.Name.Substring(2, 1));
                if (pic.ImageLocation == Const.selected)
                {
                    if (picRow == antenaRow)
                    {
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
                if (isSelectedLocation == false)
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
            Task<bool> resultClear = Task.Run(() => API.ApiClearPositionMSTAntena(Config.nameOfShelf));
            bool isSuccessClear = resultClear.Result;
            if (isSuccessClear)
            {
                bool isSuccessUpdate = false;
                foreach (CheckBox checkItem in settingLayer.Controls.OfType<CheckBox>())
                {

                    if (checkItem.Checked)
                    {
                        var (scancolstart, antenaIndex, antenaNo) = getValueToInsertMST_Working(checkItem);
                        Task<bool> resultUpdate = Task.Run(() => API.ApiUpdatePositionMSTAntena(antenaIndex, scancolstart, antenaNo));
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
            }
            else
            {
                DialogResult confirmResult = MessageBox.Show("登録失敗", "結果", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        private void LoadDataToScreen()
        {
            string[] antenaIndexAndNo;
            foreach (TextBox antenaNo in settingLayer.Controls.OfType<TextBox>())
            {
                foreach (string loadAntena in Config.antenaNoList)
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
                foreach (CheckBox loadAntena in Config.antenaLoadList)
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
                foreach (var item in Global.settingPosition)
                {
                    if (int.Parse(pic.Name.Substring(2, 1)) == item.col && int.Parse(pic.Name.Substring(0, 1)) == item.row)
                    {
                        pic.Load(Const.selected);
                    }
                }
            }

        }








        private void picOnClick(object sender, EventArgs e)
        {
            PictureBox pictureBoxClicked = sender as PictureBox;

            if (pictureBoxClicked.ImageLocation == Const.selected)
            {
                pictureBoxClicked.Load(Const.blank_image);
            }
            else
            {
                pictureBoxClicked.Load(Const.selected);

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
                pic.Load(Const.blank_image);
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
            Config.isLoadSetting = true;
            Task<bool> result = Task.Run(() => API.ApiLoadPositionMSTAntena((Config.nameOfShelf)));
            bool isSuccess = result.Result;
            if (isSuccess)
            {
                LoadDataToScreen();
                DialogResult confirmResult = MessageBox.Show("データを正常にロード", "結果", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
            else
            {
                DialogResult confirmResult = MessageBox.Show("データの読み込みに失敗しました", "結果", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
            Config.isLoadSetting = false;

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
