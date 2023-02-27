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
    public partial class DataGridView : Form
    {
        public DataGridView()
        {
            InitializeComponent();
            this.CenterToScreen();
        }

        public void modf(System.Windows.Forms.DataGridView d1)
        {
            d1.AutoGenerateColumns = false;
        }

        public void clsGridView(System.Windows.Forms.DataGridView d1)
        {
            detailData.DataSource = null;
            detailData.Rows.Clear();
            detailData.Columns.Clear();
            detailData.Refresh();

        }


        private void detailData_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
