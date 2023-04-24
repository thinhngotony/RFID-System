
namespace Shelf_Register
{
    partial class DataGridView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.detailData = new System.Windows.Forms.DataGridView();
            this.RfidGV = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ProductNameGV = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.JancodeGV = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.imageGV = new System.Windows.Forms.DataGridViewImageColumn();
            ((System.ComponentModel.ISupportInitialize)(this.detailData)).BeginInit();
            this.SuspendLayout();
            // 
            // detailData
            // 
            this.detailData.AllowUserToOrderColumns = true;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.detailData.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.detailData.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.detailData.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.detailData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.detailData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.detailData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.imageGV,
            this.JancodeGV,
            this.ProductNameGV,
            this.RfidGV});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.detailData.DefaultCellStyle = dataGridViewCellStyle3;
            this.detailData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.detailData.Location = new System.Drawing.Point(0, 0);
            this.detailData.Name = "detailData";
            this.detailData.Size = new System.Drawing.Size(984, 461);
            this.detailData.TabIndex = 0;
            this.detailData.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.detailData_CellContentClick);
            // 
            // RfidGV
            // 
            this.RfidGV.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.RfidGV.DataPropertyName = "EPC";
            this.RfidGV.HeaderText = "RFID";
            this.RfidGV.Name = "RfidGV";
            // 
            // ProductNameGV
            // 
            this.ProductNameGV.DataPropertyName = "product_name";
            this.ProductNameGV.HeaderText = "Product Name";
            this.ProductNameGV.Name = "ProductNameGV";
            this.ProductNameGV.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // JancodeGV
            // 
            this.JancodeGV.DataPropertyName = "jancode";
            this.JancodeGV.HeaderText = "Jancode";
            this.JancodeGV.Name = "JancodeGV";
            // 
            // imageGV
            // 
            this.imageGV.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.imageGV.DataPropertyName = "link_image";
            this.imageGV.FillWeight = 50F;
            this.imageGV.HeaderText = "Image";
            this.imageGV.Name = "imageGV";
            this.imageGV.Width = 200;
            // 
            // DataGridView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 461);
            this.Controls.Add(this.detailData);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "DataGridView";
            this.Text = "Detail Data";
            ((System.ComponentModel.ISupportInitialize)(this.detailData)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.DataGridView detailData;
        private System.Windows.Forms.DataGridViewImageColumn imageGV;
        private System.Windows.Forms.DataGridViewTextBoxColumn JancodeGV;
        private System.Windows.Forms.DataGridViewTextBoxColumn ProductNameGV;
        private System.Windows.Forms.DataGridViewTextBoxColumn RfidGV;
    }
}