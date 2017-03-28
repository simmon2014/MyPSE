using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace PSE
{
    public partial class LineSim : Form
    {
        public LineSim()
        {
            InitializeComponent();
        }

        public double[] m_ResultList;
        public string m_strChartName;


        private void toolStripButtonTest_Click(object sender, EventArgs e)
        {
            double yValue = 50.0; 
            Random random = new Random();
            double x, xmax, xmin;
            xmax = 30000;
            xmin = 1;
            //double k=3;
            int maxcount = 20000;

            for (int pointIndex = 1; pointIndex < maxcount; pointIndex++) 
            { 

                //yValue = yValue + (random.NextDouble() * 10.0 - 5.0);
                x = xmin + pointIndex*(xmax - xmin) / maxcount;
               // yValue=(Math.Exp((x-xmin)/(xmax-xmin)*k)-1)/(Math.Exp(k)-1);

              //  yValue = (Math.Log((x - xmin) / (xmax - xmin) * k) - 1) / (Math.Log(k) - 1);
               yValue = Math.Log((pointIndex + 1) * 10);

                chart1.Series["Series1"].Points.AddY(yValue); 
            }         
            // Set fast line chart type         
            chart1.Series["Series1"].ChartType = SeriesChartType.FastLine;
        }

        private void toolStripButtonShow_Click(object sender, EventArgs e)
        {
            chart1.Series[0].Points.Clear();

            double yValue;
            for(int i=0;i<m_ResultList.Length;i++)
            {
                yValue = m_ResultList[i];
                chart1.Series[0].Points.AddY(yValue); 
            }

        }




    }
}
