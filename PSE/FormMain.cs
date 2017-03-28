using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms.DataVisualization.Charting;
using PSE.DES;

namespace PSE
{
    public partial class FormMain : Form
    {
         ProductLineCalc plc=new ProductLineCalc();
         public double m_time=0;
         public bool simstate = false;
         public bool climbstate = false;
         
        public FormMain()
        {
            InitializeComponent();
        }

        private void toolStripButtonLoadFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog fdlg = new OpenFileDialog();

            fdlg.InitialDirectory = @"D:\workdir\vs2013\other";
            fdlg.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            fdlg.FilterIndex = 2;
            fdlg.RestoreDirectory = true;
            if (fdlg.ShowDialog() != DialogResult.OK) return;
            string pathname = fdlg.FileName;
            try
            {
                StreamReader sr = new StreamReader(pathname);
                string sLine = "";
                string strLeft="",strRight = "";

                string blank = ":";
               
                while (sLine != null)
                {
                    sLine = sr.ReadLine();
                    string[] rSplit = sLine.Split(blank.ToCharArray(), 2, StringSplitOptions.RemoveEmptyEntries);
                    if (rSplit.Length < 2) continue;
                    strLeft = rSplit[0].Trim();
                    strRight= rSplit[1].Trim();
                    if (strLeft == "M") textBox1.Text = strRight;
                    if (strLeft == "r") textBox2.Text = strRight;
                    if (strLeft == "u") textBox3.Text = strRight;
                    if (strLeft == "c") textBox4.Text = strRight;
                    if (strLeft == "N") textBox5.Text = strRight;

                }
                sr.Close();
            }catch(Exception excep)
            {
                Console.WriteLine("Exception: " + excep.Message);
            }

           


        }
        private void toolStripButton_SavePara_Click(object sender, EventArgs e)
        {
            string strText="";
            strText = strText + "M:\t" + textBox1.Text+"\r\n";
            strText = strText + "r:\t" + textBox2.Text + "\r\n";
            strText = strText + "u:\t" + textBox3.Text + "\r\n";
            strText = strText + "c:\t" + textBox4.Text + "\r\n";
            strText = strText + "N:\t" + textBox5.Text + "\r\n";


            SaveFileDialog fdlg = new SaveFileDialog();
            fdlg.InitialDirectory = @"D:\workdir\vs2013\other";
            fdlg.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            fdlg.FilterIndex = 2;
            fdlg.RestoreDirectory = true;
            if (fdlg.ShowDialog() != DialogResult.OK) return;
            string pathname = fdlg.FileName;
            try
            {
                StreamWriter sw = new StreamWriter(pathname);
                sw.Write(strText);
                sw.Close();
            }
            catch (Exception excep)
            {
                Console.WriteLine("Exception: " + excep.Message);
            }
        }


        private void toolStripButton_TP_Click(object sender, EventArgs e)
        {
            string blank = " \t";
            string[] rSplit = textBoxPara2.Text.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (rSplit.Length < 6)
            {
                textBoxResult.Text = "参数不全";
                timer1.Stop();
                climbstate = false;
                return;
            }


            double Tmax = double.Parse(rSplit[0]);//
            double Tmin = double.Parse(rSplit[1]);//
            double maxCool = double.Parse(rSplit[2]);//
            double KBeta = double.Parse(rSplit[3]);//
            int maxS = int.Parse(rSplit[4]);//最大迭代次数
            int xdelta = int.Parse(rSplit[5]);//


            Stopwatch sw = new Stopwatch();
            sw.Start();
            textBoxResult.Text = plc.CalcMulTPStrEx(textBox2.Text, textBox3.Text, textBox4.Text, textBox5.Text, textBox1.Text);
             plc.m_stateU = maxS;
            textBoxResult2.Text = plc.CalMulMachineArrayStr(textBox2.Text, textBox3.Text, textBox4.Text, textBox5.Text, textBox1.Text);

            textBoxResult3.Text = plc.CalcMulTPStrExU(textBox2.Text, textBox3.Text, textBox4.Text, textBox5.Text, textBox1.Text);
            sw.Stop();
            textBoxResult2.Text = textBoxResult2.Text + "\r\n\r\n" + "总运行时间毫秒：\t" + sw.ElapsedMilliseconds.ToString();


        }

        private void toolStripButton_WIP_Click(object sender, EventArgs e)
        {

            Stopwatch sw = new Stopwatch();
            sw.Start();
            TextBox tb = textBoxResult3;
            tb.Text = plc.TestTPStr(textBox2.Text, textBox3.Text, textBox4.Text, textBox5.Text, textBox1.Text);

             tb = textBoxResult2;
            tb.Text = plc.DDXCalcMulTPStr(textBox2.Text, textBox3.Text, textBox4.Text, textBox5.Text, textBox1.Text);

            sw.Stop();
            tb.Text = tb.Text + "\r\n\r\n" + "总运行时间毫秒：\t" + sw.ElapsedMilliseconds.ToString();
        }

        private void toolStripButton_MulWIP_Click(object sender, EventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            textBoxResult.Text=plc.CalcMulTPStrExU(textBox2.Text,textBox3.Text,textBox4.Text,textBox5.Text,textBox1.Text);
            sw.Stop();
            textBoxResult.Text = textBoxResult.Text + "\r\n\r\n" + "总运行时间毫秒：\t" + sw.ElapsedMilliseconds.ToString();
        }

        private void toolStripButton_CycleOpt_Click(object sender, EventArgs e)
        {
           // backgroundWorker2.RunWorkerAsync();

            if (climbstate == false)
            {
                climbstate = true;
                m_time = 0;
                timer1.Start();
                backgroundWorker2.RunWorkerAsync();

            }
            else
            {
                backgroundWorker2.CancelAsync();
                timer1.Stop();
                climbstate = false;

            }
        }

        private void toolStripButton_BufferOpt_Click(object sender, EventArgs e)
        {
            if (climbstate == false)
            {
                climbstate = true;
                m_time = 0;
                timer1.Start();
                backgroundWorker1.RunWorkerAsync();

            }
            else
            {
                backgroundWorker1.CancelAsync();
                timer1.Stop();
                climbstate = false;

            }


        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            string strType = "N";

            if (checkBox1.Checked == true)
            {
                strType = "C";

            }
            else
            {
                strType = "N";

            }

            TextBox tb = textBoxResult3;

            string blank = " \t";
            string[] rSplit = textBoxPara.Text.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (rSplit.Length < 5)
            {
                tb.Text = "参数不全";
                return;
            }

            int NUM = int.Parse(rSplit[0]);//粒子数
            //  int DIM = int.Parse(rSplit[1]);//维数
            double c1 = double.Parse(rSplit[1]);//学习因子c1
            double c2 = double.Parse(rSplit[2]);//学习因子c22
            double w1 = double.Parse(rSplit[3]);//惯性权重
            double w2 = double.Parse(rSplit[4]);//惯性权重
            int maxS = int.Parse(rSplit[5]);//最大迭代次数
            ProductLineCalc pl = new ProductLineCalc();
            pl.InitSolution(textBox2.Text, textBox3.Text, textBox4.Text, textBox5.Text, textBox1.Text);
            int DIM = (int)pl.m_M - 1;
            double xmin = 1;
            double xmax = pl.m_TotalN - DIM + 1;

            if (strType == "C")
            {
                pl.m_stateU = 1;
                strType = "N";
                tb = textBoxResult2;
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();

            //QuantumPSO pso = new QuantumPSO(NUM, DIM, w1, w2, maxS, xmin, xmax);
            SecondPSO pso = new SecondPSO(NUM, DIM,c1,c2, w1, w2, maxS, xmin, xmax);
            pso.plc = pl;

           

            tb.Text = pso.RunToStr();
            tb.Text = tb.Text + "\r\n\r\n" + pl.CalcMulTPStr();

            tb.Text = tb.Text + "\r\n\r\n" + "总运行时间毫秒：\t" + sw.ElapsedMilliseconds.ToString();
            tb.Text = tb.Text + "\r\n函数计算次数：\t" + pl.m_CalNum.ToString();
          //  textBoxResult2.Text = pso.m_Result;
            sw.Stop();
            timer1.Stop();
            climbstate = false;





        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            

            string strType = "N";

            if (checkBox1.Checked == true)
            {
                strType = "C";

            }
            else
            {
                strType = "N";


            }

            TextBox tb = textBoxResult2;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            ProductLineCalc pl = new ProductLineCalc();
            if(strType=="C")
            {
                pl.m_stateU = 1;
                strType = "N";
                tb = textBoxResult;
            }

            pl.m_stateU = 1;
            tb.Text = pl.QuickHillClimbingStr(textBox2.Text, textBox3.Text, textBox4.Text, textBox5.Text, textBox1.Text, textBoxS.Text,strType);
            //textBoxResult2.Text = pl.QuickCompareClimbingStr(textBox2.Text, textBox3.Text, textBox4.Text, textBox5.Text, textBox1.Text, textBoxS.Text, strType);
            sw.Stop();
            tb.Text = tb.Text + "\r\n爬山耗用时间：\t" + sw.ElapsedMilliseconds.ToString();
            tb.Text = tb.Text + "\r\n函数计算次数：\t" + pl.m_CalNum.ToString();
            tb.Text = tb.Text + "\r\n爬山法计算次数：\t" + pl.m_ClimbNum.ToString();
            tb.Text = tb.Text + "\r\n爬山法邻域搜索次数：\t" + pl.m_NeighborNum.ToString();
            tb.Text = tb.Text + "\r\n爬山法邻域搜索结果：\r\n" + pl.m_strClimb; 

            timer1.Stop();
            climbstate = false;


        }

        private void backgroundWorker3_DoWork(object sender, DoWorkEventArgs e)
        {
           // string strType = "N";
           //// string strChartName = "";
           // if (checkBox1.Checked == true)
           // {
           //     strType = "C";
           //    // strChartName = "HillClimbingC";
           // }
           // else
           // {
           //     strType = "N";
           //    // strChartName = "HillClimbingN";

           // }
           // Stopwatch sw = new Stopwatch();
           // sw.Start();
           // ProductLineCalc pl = new ProductLineCalc();
           // textBoxResult.Text = pl.HillClimbingStr(textBox2.Text, textBox3.Text, textBox4.Text, textBox5.Text, textBox1.Text, textBoxS.Text, strType);
           // sw.Stop();
           // textBoxResult.Text = textBoxResult.Text + "\r\n爬山耗用时间：\t" + sw.ElapsedMilliseconds.ToString();
           // textBoxResult.Text = textBoxResult.Text + "\r\n函数计算次数：\t" + pl.m_CalNum.ToString();




            Stopwatch sw = new Stopwatch();
            sw.Start();

            TextBox tb = textBoxResult;
            tb.Text = "";

            ProductLineCalc pl = new ProductLineCalc();
            pl.m_stateU = 1;
            tb.Text = pl.SolveBAP2ClimbStr(textBox2.Text, textBox3.Text, textBox4.Text, textBox5.Text, textBox1.Text, textBoxS.Text);
            sw.Stop();
            tb.Text = tb.Text + "\r\n爬山耗用时间：\t" + sw.ElapsedMilliseconds.ToString();
            tb.Text = tb.Text + "\r\n函数计算次数：\t" + pl.m_CalNum.ToString();


            //Random rand = new Random();
            //int Mmax = 30;
            //int tmax = 1;
            //int seedmax = 20000;
            //double batchtime = 80000;


            ////double cmax = 3, cmin = 1;
            //double cmax = 1.2, cmin = 0.8;
            //double emin = 0.85, emax = 0.999;
            ////  double mtbfmin = 10, mtbfmax = 200;

            //double mtbfmin = 50, mtbfmax = 2 * mtbfmin;

            //int nmin = 5, nmax = 2 * nmin;
            //double MTTF, MTBF, MTTR, ee;

            //string strResult = "";

            //for (int M = 5; M <= Mmax; M = M + 5)
            //{
            //    double[] r = new double[M];
            //    double[] u = new double[M];
            //    double[] c = new double[M];
            //    double[] N = new double[M];
            //    strResult = "";
            //    ProductLineCalc pl = new ProductLineCalc();
            //    EGSim egsim = new EGSim(M, seedmax);
            //    egsim.batchtime = batchtime;
            //    strResult = strResult + "M=" + M.ToString() + "\r\n";
            //    strResult = strResult + "sim=" + "\t";
            //    strResult = strResult + "C=" + "\t";
            //    strResult = strResult + "U=" + "\t";
            //    strResult = strResult + "R=" + "\t";
            //    strResult = strResult + "UR=" + "\t";
            //    strResult = strResult + "MACH=" + "\r\n";

            //    for (int t = 0; t < tmax; t++)
            //    {
            //        for (int i = 0; i < M; i++)
            //        {
            //            //ee = emin + (emax - emin) * rand.NextDouble();
            //            //MTBF = mtbfmin + (mtbfmax - mtbfmin) * rand.NextDouble();
            //            //c[i] = cmin + (cmax - cmin) * rand.NextDouble();
            //            //N[i] = rand.Next(nmin, nmax);

            //            //MTTF = MTBF * ee;
            //            //MTTR = MTBF * (1 - ee);
            //            //r[i] = 1 / MTTF;
            //            //u[i] = 1 / MTTR;

            //            c[i] = 1;
            //            r[i] = 0.01;
            //            u[i] = 0.1;
            //            N[i] = 10;
            //        }

            //        egsim.InitArray(r, u, c, N, M);
            //        double tpsim = egsim.GetTotalTP();
            //        double tpC = plc.CalcMulTPRateEx(r, u, c, N, M);
            //        double tpU = plc.CalcMulTPRateU(r, u, c, N, M);
            //        double tpR = plc.CalcMulTPRateR(r, u, c, N, M);
            //        double tpUR = plc.CalcMulTPRateUR(r, u, c, N, M);
            //        double tpMach = plc.CalMulMachineRate(r, u, c, N, M);
            //        strResult = strResult + tpsim.ToString() + "\t";
            //        strResult = strResult + tpC.ToString() + "\t";
            //        strResult = strResult + tpU.ToString() + "\t";
            //        strResult = strResult + tpR.ToString() + "\t";
            //        strResult = strResult + tpUR.ToString() + "\t";
            //        strResult = strResult + tpMach.ToString() + "\r\n";

            //    }


            //    tb.Text = tb.Text + strResult;


            //}




            sw.Stop();
            tb.Text = tb.Text + "\r\n耗用时间(ms)：\t" + sw.ElapsedMilliseconds.ToString();
            timer1.Stop();




            timer1.Stop();
            climbstate = false;
          

        }

        private void toolStripButtonHill_Click(object sender, EventArgs e)
        {

            if (climbstate == false)
            {
                climbstate = true;
                m_time = 0;
                timer1.Start();
                backgroundWorker3.RunWorkerAsync();

            }
            else
            {
                backgroundWorker3.CancelAsync();
                timer1.Stop();
                climbstate = false;

            }
        }

        private void toolStripButtonInitText_Click(object sender, EventArgs e)
        {

          //  int M = int.Parse(textBox1.Text);
          //  if (M <= 0) return;

          //  Random rand=new Random();
          //  double r, u, c;
          //  int N;

          //  //double cmax = 3, cmin = 1;
          //  double cmax = 1.2, cmin =0.8;
          //  double emin = 0.85, emax = 0.999;
          ////  double mtbfmin = 10, mtbfmax = 200;

          //  double mtbfmin = 50, mtbfmax = 2*mtbfmin;

          //  double nmin = 5, nmax = 2*nmin;
          //  double MTTF, MTBF, MTTR, ee;


          //  textBox2.Text = "";
          //  textBox3.Text = "";
          //  textBox4.Text = "";
          //  textBox5.Text = "";
          //  for (int i = 0; i < M; i++)
          //  {
          //      ee = emin + (emax - emin) * rand.NextDouble();
          //      MTBF = mtbfmin + (mtbfmax - mtbfmin) * rand.NextDouble();
          //      c = cmin + (cmax - cmin) * rand.NextDouble();
          //      N = (int)(nmin + (nmax - nmin) * rand.NextDouble());

          //      MTTF = MTBF * ee;
          //      MTTR = MTBF * (1 - ee);
          //      r = 1 / MTTF;
          //      u = 1 / MTTR;
          //      textBox2.Text = textBox2.Text  + r.ToString()+" ";
          //      textBox3.Text = textBox3.Text  + u.ToString()+" ";
          //      textBox4.Text = textBox4.Text  + c.ToString()+" ";
          //      textBox5.Text = textBox5.Text +  N.ToString()+" ";

          //  }

            ///////////////////
            ///////////////////

            if (climbstate == false)
            {
                climbstate = true;
                m_time = 0;
                timer1.Start();
                backgroundWorker3.RunWorkerAsync();

            }
            else
            {
                backgroundWorker3.CancelAsync();
                timer1.Stop();
                climbstate = false;

            }



        }

        private void toolStripButtonSim_Click(object sender, EventArgs e)
        {
           if(simstate==false)
           {
               simstate = true;
               m_time = 0;
               timer1.Start();
               backgroundWorker4.RunWorkerAsync();

           }else
           {
               backgroundWorker4.CancelAsync();
               timer1.Stop();
               simstate = false;

           }
          
          
        }

        private void backgroundWorker4_DoWork(object sender, DoWorkEventArgs e)
        {
           
            Stopwatch sw = new Stopwatch();
            sw.Start();

            string blank = " \t";
            string[] rSplit = textBoxSeed.Text.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (rSplit.Length <1)
            {
                textBoxResult.Text = "参数不全";
                timer1.Stop();
                simstate = false;
                return;
            }

            int seedmax,seednum=0;
             seedmax = int.Parse(rSplit[0]);//
            if(rSplit.Length>1)
            {
               seednum = int.Parse(rSplit[1]);// 
            }
           
          
            //int seed = int.Parse(textBoxSeed.Text);

            EGSim egsim = new EGSim(int.Parse(textBox1.Text), seedmax);
           string message= egsim.InitStr(textBox2.Text, textBox3.Text, textBox4.Text, textBox5.Text, textBox1.Text);
           double batchtime = double.Parse(textBoxS.Text);

           egsim.Reset();
           egsim.RunBatch(batchtime);
            textBoxResult.Text = egsim.GetResult();


           Random rand = new Random();
           double totaltp = 0;
           string temp = "";
           for (int i = 0; i <seednum;i++ )
           {
               egsim.mseed = rand.Next(seedmax);
               double tp = egsim.GetTotalTP();
               totaltp = totaltp + tp;
               temp = tp.ToString() + "\t";
               textBoxResult.Text = textBoxResult.Text + temp;
           }
           if(seednum>0)
           {
               totaltp = totaltp / seednum;
               temp =  "\r\n" + "平均值：" + totaltp.ToString();
               textBoxResult.Text = textBoxResult.Text + temp;

           }
           

               sw.Stop();
            textBoxResult.Text = textBoxResult.Text + "\r\n事件图仿真耗用时间(ms)：\t" + sw.ElapsedMilliseconds.ToString();
            timer1.Stop();
            simstate = false;
        }

        private void toolStripButtonEClimb_Click(object sender, EventArgs e)
        {
           
            if (climbstate == false)
            {
                climbstate = true;
                m_time = 0;
                timer1.Start();
                backgroundWorker5.RunWorkerAsync();

            }
            else
            {
                backgroundWorker5.CancelAsync();
                timer1.Stop();
                climbstate = false;

            }

           
        }

        private void backgroundWorker5_DoWork(object sender, DoWorkEventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            ProductLineCalc pl = new ProductLineCalc();
            textBoxResult2.Text = pl.SolveBAP2ClimbStr(textBox2.Text, textBox3.Text, textBox4.Text, textBox5.Text, textBox1.Text, textBoxS.Text);
            //textBoxResult2.Text = pl.SolveBAP1Str(textBox2.Text, textBox3.Text, textBox4.Text, textBox5.Text, textBox1.Text, textBoxS.Text);
            sw.Stop();
            textBoxResult2.Text = textBoxResult2.Text + "\r\n耗用时间：\t" + sw.ElapsedMilliseconds.ToString();
            textBoxResult2.Text = textBoxResult2.Text + "\r\n函数计算次数：\t" + pl.m_CalNum.ToString();

            textBoxResult2.Text = textBoxResult2.Text + "\r\n爬山法计算次数：\t" + pl.m_ClimbNum.ToString();


    

            timer1.Stop();
            climbstate = false;
          


        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            m_time = m_time + 0.1;
            labeltime.Text = m_time.ToString("F2");
        }

        private void toolStripButtonGene_Click(object sender, EventArgs e)
        {
            if (climbstate == false)
            {
                climbstate = true;
                m_time = 0;
                timer1.Start();
                backgroundWorker7.RunWorkerAsync();

            }
            else
            {
                backgroundWorker7.CancelAsync();
                timer1.Stop();
                climbstate = false;

            }
        }

        private void toolStripButtonPSO2_Click(object sender, EventArgs e)
        {
            if (climbstate == false)
            {
                climbstate = true;
                m_time = 0;
                timer1.Start();
                backgroundWorker6.RunWorkerAsync();

            }
            else
            {
                backgroundWorker6.CancelAsync();
                timer1.Stop();
                climbstate = false;

            }
        }

        private void backgroundWorker6_DoWork(object sender, DoWorkEventArgs e)
        {
            string blank = " \t";
            string[] rSplit = textBoxPara2.Text.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (rSplit.Length < 6)
            {
                textBoxResult.Text = "参数不全";
                timer1.Stop();
                climbstate = false;
                return;
            }

           
            double Tmax = double.Parse(rSplit[0]);//
            double Tmin = double.Parse(rSplit[1]);//
            double maxCool= double.Parse(rSplit[2]);//
            double KBeta = double.Parse(rSplit[3]);//
            int maxS = int.Parse(rSplit[4]);//最大迭代次数
            int xdelta = int.Parse(rSplit[5]);//

            // double gamma= double.Parse(rSplit[6]);//最大迭代次数
            ProductLineCalc pl = new ProductLineCalc();
            pl.InitSolution(textBox2.Text, textBox3.Text, textBox4.Text, textBox5.Text, textBox1.Text);
            int DIM = (int)pl.m_M - 1;
            double xmin = 1;
            double xmax = pl.m_TotalN - DIM + 1;

            Stopwatch sw = new Stopwatch();
            sw.Start();


              //Metropolis pso = new Metropolis( DIM,Tmax,Tmin,maxCool,KBeta,xdelta, maxS,xmin,xmax);
              MOSA pso = new MOSA(DIM, Tmax, Tmin, maxCool, KBeta, xdelta, maxS, xmin, xmax);
              pso.plc = pl;
              textBoxResult.Text = pso.RunToStr();
         
            textBoxResult.Text = textBoxResult.Text + "\r\n" + "总运行时间毫秒：\t" + sw.ElapsedMilliseconds.ToString();
            textBoxResult.Text = textBoxResult.Text + "\r\n函数计算次数：\t" + pl.m_CalNum.ToString();

            sw.Stop();
            timer1.Stop();
            climbstate = false;


            //string strType = "N";
            //if (checkBox1.Checked == true)
            //{
            //    strType = "C";
               
            //}
            //else
            //{
            //    strType = "N";
            //}

            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            //ProductLineCalc pl = new ProductLineCalc();
            //// textBoxResult2.Text = pl.QuickHillClimbingStr(textBox2.Text, textBox3.Text, textBox4.Text, textBox5.Text, textBox1.Text, textBoxS.Text,strType);
            //textBoxResult3.Text = pl.QuickCompareClimbingStr(textBox2.Text, textBox3.Text, textBox4.Text, textBox5.Text, textBox1.Text, textBoxS.Text, strType);
            //sw.Stop();
            //textBoxResult3.Text = textBoxResult3.Text + "\r\n爬山耗用时间：\t" + sw.ElapsedMilliseconds.ToString();
            //textBoxResult3.Text = textBoxResult3.Text + "\r\n函数计算次数：\t" + pl.m_CalNum.ToString();
            //textBoxResult3.Text = textBoxResult3.Text + "\r\n爬山法计算次数：\t" + pl.m_ClimbNum.ToString();

            //timer1.Stop();
            //climbstate = false;

        }

        private void backgroundWorker7_DoWork(object sender, DoWorkEventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            ProductLineCalc pl = new ProductLineCalc();
          
            textBoxResult.Text = pl.SolveBAP2PSOStr(textBox2.Text, textBox3.Text, textBox4.Text, textBox5.Text, textBox1.Text, textBoxPara.Text);
            sw.Stop();
            textBoxResult.Text = textBoxResult.Text + "\r\n耗用时间：\t" + sw.ElapsedMilliseconds.ToString();
            textBoxResult.Text = textBoxResult.Text + "\r\n函数计算次数：\t" + pl.m_CalNum.ToString();

            textBoxResult.Text = textBoxResult.Text + "\r\n爬山法计算次数：\t" + pl.m_ClimbNum.ToString();


            timer1.Stop();
            climbstate = false;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (climbstate == false)
            {
                climbstate = true;
                m_time = 0;
                timer1.Start();
                backgroundWorker8.RunWorkerAsync();

            }
            else
            {
                backgroundWorker8.CancelAsync();
                timer1.Stop();
                climbstate = false;

            }
        }

        private void backgroundWorker8_DoWork(object sender, DoWorkEventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int seed = int.Parse(textBoxSeed.Text);
            EGSim egsim = new EGSim(int.Parse(textBox1.Text), int.Parse(textBoxSeed.Text));
            string message = egsim.InitStr(textBox2.Text, textBox3.Text, textBox4.Text, textBox5.Text, textBox1.Text);
            double batchtime = double.Parse(textBoxS.Text);

            egsim.Reset();
            egsim.QuickHillClimbingN(batchtime, 0);

            //egsim.Reset();
            //egsim.RunBatch(batchtime);
          

            textBoxResult3.Text = egsim.GetResult();
            sw.Stop();
            textBoxResult3.Text = textBoxResult3.Text + "\r\n事件图仿真耗用时间(ms)：\t" + sw.ElapsedMilliseconds.ToString();
            textBoxResult3.Text = textBoxResult3.Text + "\r\n仿真计算次数：\t" + egsim.TotalS.ToString();
            timer1.Stop();
            climbstate = false;
        }

        private void toolStripContainer1_TopToolStripPanel_Click(object sender, EventArgs e)
        {

        }

       
    }
}
