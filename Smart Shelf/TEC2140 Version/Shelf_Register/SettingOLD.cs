using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Shelf_Register
{
    public partial class SettingOLD : Form
    {
        public SettingOLD()
        {
            InitializeComponent();
            initSetting();
            this.StartPosition = FormStartPosition.Manual;
            this.CenterToScreen();

        }

        private void initSetting()
        {
            setBlankForMultiLayer(new List<Panel> { settingLayer_1, settingLayer_2, settingLayer_3, settingLayer_4 });
        }

        private void setBlankForSingleLayer(Panel panel)
        {
            foreach (PictureBox pictureBox_Items in panel.Controls.OfType<PictureBox>())
            {
                pictureBox_Items.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox_Items.Load("blank_background.png");
            }
        }

        private void setBlankForMultiLayer(List<Panel> panels)
        {
            foreach (var panel in panels)
            {
                setBlankForSingleLayer(panel);
            }
        }
    }
}
