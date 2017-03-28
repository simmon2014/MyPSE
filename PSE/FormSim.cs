using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PSE
{
    public partial class FormSim : Form
    {
        public FormSim()
        {
            InitializeComponent();
        }

        private void toolStripButtonTest_Click(object sender, EventArgs e)
        {
            AHP.MachineLine ml = new AHP.MachineLine();
            ml.Init(6,7,5);
           textBox1.Text = ml.LogToResult();
        }
    }
}
