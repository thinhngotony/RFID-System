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
    
    public partial class Wait : Form
    {
        //public Wait(Action worker)
        //{
        //    InitializeComponent();
        //    if (worker == null)
        //        throw new ArgumentException();
        //    Worker = worker;
        //}
        public Wait()
        {

            InitializeComponent();
            this.CenterToScreen();
        }
        
        protected override void OnLoad(EventArgs e)
        {
            
            base.OnLoad(e);
            //Task.Factory.StartNew(Worker).ContinueWith(t => { this.Close(); }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public Action Worker { get; set; }

        private void progressBar1_Click(object sender, EventArgs e)
        {
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Wait_Load(object sender, EventArgs e)
        {

        }
    }
}
