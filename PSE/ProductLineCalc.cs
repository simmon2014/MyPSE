using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Diagnostics;
using PSE.DES;

namespace PSE
{
    public class S2Line
    {
        public double p011;   
        public double p001;
        public double pn11;
        public double pn10;
        public double tp1;
        public double tp2;
        public S2Line()
        {
            this.p001 = 1;
            this.p011 = 1;
            this.pn11 = 1;
            this.pn10 = 1;
            this.tp1 = 1;
        }
        public double RateC(double c)
        {
        //    this.p001 = this.p001 * c;
        //    this.p011 = this.p011 * c;
        //    this.pn11 = this.pn11 * c;
        //    this.pn10 = this.pn10 * c;
            this.tp1 = this.tp1 * c;
            this.tp2 = this.tp2 * c;
            return 1;
        }
    }
    public class SMachine
    {
       public double r;
       public double u;
       public double c;
       public SMachine()
       {
           r = 0.1;
           u = 0.01;
           c = 1;
       }
       public SMachine(double r,double u ,double c)
       {
           this.r = r;
           this.c = c;
           this.u = u;
       }

       public int Set(double r, double u, double c)
       {
           this.r = r;
           this.c = c;
           this.u = u;
           return 1;
       }

       public double getMTTF()
       {
           return 1 / r;
       }
       public double getMTTR()
       {
           return 1 / u;
       }
       public double getProcessTime()
       {
           return 1 / c;
       }
       public double getE()
       {
           return u/ (u+r);
       }
       public double getRealC()
       {
           return c*u / (u + r);
       }
       
        public SMachine Clone()
       {
           SMachine MM = new SMachine(this.r, this.u, this.c);
           return MM;
       }
      
  
    }

   public class ProductLineCalc
    {


        /*****************************************************************************************/
        /*
         * 
         */
        /***************************************************************************************/
           public int m_M;
           public double[] m_r;
           public double[] m_u;
           public double[] m_c;
           public double[] m_N;
           public double m_TotalN;
           public double m_D, m_xmax, m_xmin;
           static Random rand = new Random();//用于生成随机数
           public double m_epsilon = 1E-6;

           public int m_stateU=0;////0,C  1,U  2,R  3,UR

           public int m_minBuffer = 5;


        /// <summary>
        /// /
        /// </summary>
           public int m_ClimbNum=0;
           public int m_CalNum=0;
           public int m_BCNum=0;
           public int m_NeighborNum = 0;///邻域搜索次数
           public ArrayList m_TPArray=new ArrayList();

        /// <summary>
        /// /
           public string m_strClimb="";


        /// <summary>
        /// //pso参数
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        
        int m_psoNum, m_psoDim,m_psoMaxS;
        double m_psoC1, m_psoC2, m_psoW1,  m_psoW2,  m_psoXmin,  m_psoXmax;
        double m_minRealC;


        public int Mix(SMachine A,SMachine B,double N)
        {
            double r, u, c;
            c = A.c; if (B.c < c) c = B.c;
            u = A.u; if (B.u < u) u = B.u;

            double tp = CalcTP(A.r, A.u, A.c, B.r, B.u, B.c, N);
            r = c * u / tp - u;
          

            A.Set(r,u,c);
            return 1;
        }

        public double CalTwoMachine(SMachine A, SMachine B, double N)
        {
            double tp = CalcTP(A.r, A.u, A.c, B.r, B.u, B.c, N);
            return tp;
        }


        public SMachine MixMulMachine(double[] r, double[] u, double[] c, double[] N, int M,string ori,string type)
        {

            SMachine A = new SMachine(r[0],u[0],c[0]);
            if(M==1)
            {
                return A;
            }

            double tp = CalcMulTPRate(r, u, c, N, M);
            int index=0;
            if(ori=="left")
            {   
                index=0;
            }else
            {
                index = M - 1;
            }
            if(type=="R")
            {
                A.c = c[index];
                A.u = u[index];
                A.r = A.c * A.u / tp - A.u;
            }else if(type=="U")
            {
                A.c = c[index];
                A.r = r[index];
                A.u = A.r * tp / (A.c - tp);
            }else if(type=="C")
            {
                
                A.r = r[index];
                A.u = u[index];
                A.c = (1 + A.r/A.u) * tp; 
            }
           


            return A;
        }

        public string CalMulMachineArrayStr(string lambda, string stru, string strc, string strN, string strM)
        {
            int M = int.Parse(strM);
            if (M <= 0 || M > 1000) return "机器数量太大";

            double[] r = new double[M];
            double[] u = new double[M];
            double[] c = new double[M];
            double[] N = new double[M - 1];

            string blank = " \t";
            string[] rSplit = lambda.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string[] uSplit = stru.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string[] cSplit = strc.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string[] NSplit = strN.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (rSplit.Length < M || uSplit.Length < M || cSplit.Length < M || NSplit.Length < M - 1) return "数据中机器数量与其他数据不符";

            for (int i = 0; i < M; i++)
            {
                r[i] = GetDoubleValue(rSplit[i]);
                u[i] = GetDoubleValue(uSplit[i]);
                c[i] = GetDoubleValue(cSplit[i]);
                if (i < M - 1)
                    N[i] = GetDoubleValue(NSplit[i]);
            }

            double [] resultarray=CalMulMachineArray(r, u, c, N, M) ;

            int start = 0;
            int end = M;
            int num = 0;
            int length = 50;
            if(M>length)
            {
                start = (M - length) / 2;
                end = start + length;
            }

            string result = "";
            double total = 0;
            double logtotal = 0;
            for(int i=start;i<end;i++)
            {
                num++;
                result = result + "\t" + resultarray[i].ToString();
                total = total + resultarray[i];
                logtotal = logtotal + Math.Log(resultarray[i]);
            }

            double average = total / num;
            double lnavg = Math.Exp(logtotal / num);
            result = result + "\r\n算术平均值是：\t" + average.ToString()+"\r\n";
            result = result + "\r\n几何平均值是：\t" + lnavg.ToString() + "\r\n";
        
      
            return result;
        }

          public double CalMulMachineRate(double[] r, double[] u, double[] c, double[] N, int M)
          {
              double[] resultarray = CalMulMachineArray(r, u, c, N, M);
              int start = 0;
              int end = M;
              int num = 0;
              int length = 50;
              if (M > length)
              {
                  start = (M - length) / 2;
                  end = start + length;
              }

              string result = "";
              double total = 0;
              double logtotal = 0;
              for (int i = start; i < end; i++)
              {
                  num++;
                  result = result + "\t" + resultarray[i].ToString();
                  total = total + resultarray[i];
                  logtotal = logtotal + Math.Log(resultarray[i]);
              }

              double average = total / num;
              double lnavg = Math.Exp(logtotal / num);
              return average;

          }


       public double[] CalMulMachineArray(double[] oldr, double[] oldu, double[] oldc, double[] oldN, int M)
        {
            double[] result = new double[M];
            
            for(int t=0;t<M-1;t++)
            {
                int ldim = t + 1;
                double [] lr=new double[ldim];
                double[] lu = new double[ldim];
                double[] lc = new double[ldim];
                double[] lN = new double[ldim];

                int rdim = M - (t + 1);
                double[] rr = new double[rdim];
                double[] ru = new double[rdim];
                double[] rc = new double[rdim];
                double[] rN = new double[rdim];
                for(int i=0;i<M;i++)
                {
                    if(i<ldim)
                    {
                        lr[i] = oldr[i];
                        lu[i] = oldu[i];
                        lc[i] = oldc[i];
                        lN[i] = oldN[i];

                    }else if(i>=ldim)
                    {
                        rr[i-ldim] = oldr[i];
                        ru[i-ldim] = oldu[i];
                        rc[i-ldim] = oldc[i];
                        if(i<M-1)
                        rN[i-ldim] = oldN[i];
                    }
                }

                string type = "U";
              
                    SMachine leftM=MixMulMachine(lr,lu,lc,lN,ldim,"right",type);
                    SMachine rightM = MixMulMachine(rr,ru,rc,rN,rdim,"left",type);
                    result[t] = CalTwoMachine(leftM,rightM,oldN[t]);
                   

                  

            }
            result[M - 1] = CalcMulTPRate(oldr,oldu,oldc,oldN,M);

            return result;
        }


        public double GetDoubleValue(string str)
        {
            if (str.IndexOf("/") > 0)
            {
                string fenzi = str.Substring(0, str.IndexOf('/'));
                string fenmu = str.Split('/')[1];
                return Convert.ToDouble(fenzi) / Convert.ToDouble(fenmu);
            }
            else
                return Convert.ToDouble(str);
        }



        /***************************************************************************************/

        public string CalcMulMachineStr(double[] r, double[] u, double[] c, double[] N, int M)
        {

            SMachine B = new SMachine();
            SMachine A = new SMachine(r[0], u[0], c[0]);

            for (int i = 1; i < M; i++)
            {
                B.Set(r[i], u[i], c[i]);
                Mix(A, B, N[i - 1]);
            }

            SMachine C = new SMachine(r[M - 1], u[M - 1], c[M - 1]);
           
            for (int i = 1; i < M; i++)
            {
                int j = M-1 - i;
                B.Set(r[j], u[j], c[j]);
                Mix(C, B, N[j]);
            }

           

          



            string totalResult="";

            totalResult = totalResult + "r:\tu:\tc:\te:\t" +"\r\n";

            totalResult = totalResult + A.r.ToString() + "\t" + A.u.ToString() + "\t" + A.c.ToString() + "\t" + A.getE().ToString() + "\r\n";

            totalResult = totalResult + C.r.ToString() + "\t" + C.u.ToString() + "\t" + C.c.ToString() + "\t" + C.getE().ToString() + "\r\n";
            totalResult = totalResult + "TP1:" + A.getRealC().ToString() + "\r\n";
            totalResult = totalResult + "TP2:" + C.getRealC().ToString() + "\r\n";
           
            

            return totalResult;

        }


       public string TestModal()
       {
           int total = (int)(m_TotalN - 3);
           string result="",temp="";
           double rate;

            //for(int i=0;i<=total;i++)
            //{
            //    for(int j=total-i;j>=0;j--)
            //    {
            //        m_N[1]=i+1;
            //        m_N[2]=j+1;
            //        m_N[3]=total-i-j+1;
            //        rate=CalcMulTPRate(m_r,m_u,m_c,m_N,4);
            //        temp=(i+1).ToString()+"\t"+(j+1).ToString()+"\t"+rate.ToString();
            //        result=result+temp+"\r\n";

            //    }
            //}

           for (int i = 0; i <= total; i++)
           {
               temp = "";
               for (int j =0 ; j <= total - i; j++)
               {
                   m_N[0] = i + 1;
                   m_N[1] = j + 1;
                   m_N[2] = total - i - j + 1;
                   rate = CalcMulTPRate(m_r, m_u, m_c, m_N, 4);
                   temp = temp+rate.ToString() + "\t";
               }
               result = result + temp + "\r\n";
           }


           return result;
       }
            








           public double ObjectFuncEx(double[] x)
           {
               for (int i = 0; i < m_N.Length; i++)
               {
                   m_N[i] = x[i];
               }

               double result = CalcMulTPRate(m_r, m_u, m_c, m_N, m_M);

               return result;
           }
         

            public double  ObjectFunc(double [] x)
            {
                 for (int i = 0; i < m_N.Length;i++ )
                 {
                     m_N[i]=1;
                 }
                int t=0;
                for (int i = 0; i < x.Length;i++ )
                {
                    t = (int) Math.Floor(x[i])-1;
                    if (t < 0) continue;
                    m_N[t]=m_N[t]+1;
                }

                double result = CalcMulTPRate(m_r, m_u, m_c, m_N, m_M);

                    return result;
            }
            
            /// 为粒子群算法做准备
            /// </summary>
            /// <param name="lambda"></param>
            /// <param name="stru"></param>
            /// <param name="strc"></param>
            /// <param name="strN"></param>
            /// <param name="strM"></param>
            /// <returns></returns>
            public int InitSolution(string lambda, string stru, string strc, string strN, string strM)
            {
                int M = int.Parse(strM);
                if (M <= 0 || M > 1000) return 0;
                
                 m_M = M;
                 m_r = new double[M];
                 m_u = new double[M];
                 m_c = new double[M];
                 m_N = new double[M - 1];

                string blank = " \t";
                string[] rSplit = lambda.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string[] uSplit = stru.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string[] cSplit = strc.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string[] NSplit = strN.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (rSplit.Length < M || uSplit.Length < M || cSplit.Length < M || NSplit.Length < M - 1) return 0;

                m_TotalN = 0;
                for (int i = 0; i < M; i++)
                {
                    m_r[i] = GetDoubleValue(rSplit[i]);
                    m_u[i] = GetDoubleValue(uSplit[i]);
                    m_c[i] = GetDoubleValue(cSplit[i]);
                    if (i < M - 1)
                    {
                        m_N[i] = GetDoubleValue(NSplit[i]);
                        m_TotalN = m_TotalN + m_N[i];

                    }
                       
                }

                m_xmin = 0;
                m_xmax = M-(1E-6);
                m_D = m_TotalN - (M - 1);
                

                
                return 1;
            }


            public int InitSolutionEx(string lambda, string stru, string strc,  string strM)
            {
                int M = int.Parse(strM);
                if (M <= 0 || M > 1000) return 0;

                m_M = M;
                m_r = new double[M];
                m_u = new double[M];
                m_c = new double[M];
                m_N = new double[M - 1];

                string blank = " \t";
                string[] rSplit = lambda.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string[] uSplit = stru.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string[] cSplit = strc.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (rSplit.Length < M || uSplit.Length < M || cSplit.Length < M ) return 0;


                m_minRealC = double.MaxValue;
                double temp;
                for (int i = 0; i < M; i++)
                {
                    m_r[i] = GetDoubleValue(rSplit[i]);
                    m_u[i] = GetDoubleValue(uSplit[i]);
                    m_c[i] = GetDoubleValue(cSplit[i]);

                    temp = m_c[i] * m_u[i] / (m_r[i] + m_u[i]);
                    if (temp < m_minRealC) m_minRealC = temp;
                }



                return 1;
            }

            
             public int GetMinNForTP(double[] r, double[] u, double[] c, double[] N, int M,double rate,double[] outN)
            {
                double minRealC=double.MaxValue;
                double temp;
                for (int i = 0; i < M;i++ )
                {
                    temp = c[i] * u[i] / (r[i] + u[i]);
                    if (temp < minRealC) minRealC = temp;
                }
                double minTP = minRealC * rate;
                double TP;
                 int smax=1000;

                 TP = CalcMulTPRate(r, u, c, N, M);
                 if (TP >= minTP)
                 {
                     Array.Copy(outN, N, M - 1);
                     return 1;
                 }

          
                 double maxtp=TP;
                 int maxi;
                 for (int s = 0; s < smax;s++ )
                 {
                     maxi = -1;
                     for(int i=0;i<M-1;i++)
                     {
                         N[i] = N[i] + 1;
                         TP = CalcMulTPRate(r, u, c, N, M);
                         if(TP>maxtp)
                         {
                             maxi = i;
                             maxtp = TP;
                         }

                         N[i] = N[i] - 1;
                     }
                     if(maxi>-1)
                     {
                         N[maxi] = N[maxi] + 1;
                         if(maxtp>=minTP)
                         {
                             Array.Copy(outN, N, M - 1);
                             return 1;
                         }
                     }
                 }
                    
                    return 0;
            }

            /// <summary>
            /// /返回考虑故障率后实际的利用率
            /// </summary>
            /// <param name="r"></param>
            /// <param name="u"></param>
            /// <param name="c"></param>
            /// <param name="N"></param>
            /// <param name="M"></param>
            /// <returns></returns>
            public double CalcMulTPRate(double[] r, double[] u, double[] c, double[] N, int M)
            {
                if(m_stateU==1)
                {
                   return CalcMulTPRateU(r, u, c, N, M);
                }else if(m_stateU==2)
                {
                 return CalcMulTPRateR(r, u, c, N, M);
                }
                else if (m_stateU == 3)
                {
                    return CalcMulTPRateUR(r, u, c, N, M);
                }
             
                m_CalNum++;
                double TP1 = 0;
                double TP2 = 0;
                double rate = 0;
                double[] e = new double[M];

                for (int i = 0; i < M; i++)
                {
                    e[i] = u[i] / (u[i] + r[i]);
                }

                double[] oldcf = new double[M];
                double[] oldcb = new double[M];
                double[] oldbl = new double[M];
                double[] oldst = new double[M];


                double[] cf = new double[M];
                double[] cb = new double[M];
                double[] bl = new double[M];
                double[] st = new double[M];


                Array.Copy(c, oldcf, M);

                int sMaxCount = 1000;
                double epsilon = 1E-6;
                int s = 0;
                for (s = 0; s < sMaxCount; s++)
                {
                    //////迭代开始
                    cf[0] = c[0];
                    cb[M - 1] = c[M - 1];
                    for (int i = M - 2; i >= 0; i--)
                    {
                        bl[i] = (e[i] * oldcf[i] - CalcTP(r[i], u[i], oldcf[i], r[i + 1], u[i + 1], cb[i + 1], N[i])) / (e[i] * oldcf[i]);
                        cb[i] = c[i] * (1 - bl[i]);
                    }
                    for (int i = 1; i < M; i++)
                    {
                        st[i] = (e[i] * cb[i] - CalcTP(r[i - 1], u[i - 1], cf[i - 1], r[i], u[i], cb[i], N[i - 1])) / (e[i] * cb[i]);
                        cf[i] = c[i] * (1 - st[i]);
                    }

                    Array.Copy(cf, oldcf, M);

                    TP1 = cf[M - 1] * e[M - 1];
                    TP2 = cb[0] * e[0];
                    rate = TP1 - TP2;
                    if (Math.Abs(rate) < epsilon) break;

                } 

               
               double result = TP1;
              // m_TPArray.Add(TP1);
                return result;

            }




            public double CalcMulTPRateEx(double[] r, double[] u, double[] c, double[] N, int M)
            {
                m_CalNum++;
                double TP1 = 0;
                double TP2 = 0;
                double rate = 0;
                double[] e = new double[M];

                for (int i = 0; i < M; i++)
                {
                    e[i] = u[i] / (u[i] + r[i]);
                }

              
                double[] cf = new double[M];
                double[] cb = new double[M];
           

                c.CopyTo(cf, 0);
                c.CopyTo(cb, 0);

                int sMaxCount = 1000;
                double epsilon = 1E-6;
                int s = 0;
                for (s = 0; s < sMaxCount; s++)
                {
                    //////迭代开始
                    cf[0] = c[0];
                    cb[M - 1] = c[M - 1];

                    for (int i = 1; i < M; i++)
                    {
                       // cf[i] = (c[i] / (e[i] * cb[i])) * (CalcTP(r[i - 1], u[i - 1], cf[i - 1], r[i], u[i], cb[i], N[i - 1]));
                        int j = M - 1 - i;
                        cb[j] = (c[j] / (e[j] * cf[j])) * CalcTP(r[j], u[j], cf[j], r[j + 1], u[j + 1], cb[j + 1], N[j]);
                    }

                    for (int i = 1; i < M; i++)
                    {
                        cf[i] = (c[i] / (e[i] * cb[i]) )* (CalcTP(r[i - 1], u[i - 1], cf[i - 1], r[i], u[i], cb[i], N[i - 1]));
                      //  int j = M - 1 - i;
                      //  cb[j] = (c[j] / (e[j] * cf[j])) * CalcTP(r[j], u[j], cf[j], r[j + 1], u[j + 1], cb[j + 1], N[j]);
                    }

                  

                    TP1 = cf[M - 1] * e[M - 1];
                    TP2 = cb[0] * e[0];
                    rate = TP1 - TP2;
                    if (Math.Abs(rate) < epsilon) break;

                }

             
                double result = (TP1+TP2)/2;
           
                return result;

            }



            public string BCImproveStr(string lambda, string stru, string strc, string strN, string strM)
            {
                m_BCNum = 0;
                int M = int.Parse(strM);
                if (M <= 0 || M > 1000) return "机器数量太大";

                double[] r = new double[M];
                double[] u = new double[M];
                double[] c = new double[M];
                double[] N = new double[M - 1];

                string blank = " \t";
                string[] rSplit = lambda.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string[] uSplit = stru.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string[] cSplit = strc.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string[] NSplit = strN.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (rSplit.Length < M || uSplit.Length < M || cSplit.Length < M || NSplit.Length < M - 1) return "数据中机器数量与其他数据不符";

                for (int i = 0; i < M; i++)
                {
                    r[i] = GetDoubleValue(rSplit[i]);
                    u[i] = GetDoubleValue(uSplit[i]);
                    c[i] = GetDoubleValue(cSplit[i]);
                    if (i < M - 1)
                        N[i] = GetDoubleValue(NSplit[i]);
                }

               
                double[] newN = BCImprove(r,u,c,N,M);

                string result = CalcMulTPStr(r, u, c, newN, M);
                
                return result;
            }

              /// <summary>
            /// /持续改进，根据 wip[i] - (N[i + 1] - wip[i + 1]) 的值，对N进行重新分配。
            /// 如果为正 从N[i]到N[i+1]分配一个单位。如果为负 从N[i]到N[i+1]分配一个单位
              /// </summary>
              /// <param name="r"></param>
              /// <param name="u"></param>
              /// <param name="c"></param>
              /// <param name="N"></param>
              /// <param name="M"></param>
              /// <param name="oldTP"></param>
              /// <returns></returns>
            public double[] BCImprove(double[] r, double[] u, double[] c, double[] N, int M)
            {
                m_CalNum = 0;
                double oldTP = 0;
                double TP1 = 0;
                double TP2 = 0;
                double rate = 0;
                double[] e = new double[M];

                for (int i = 0; i < M; i++)
                {
                    e[i] = u[i] / (u[i] + r[i]);
                }

                double[] oldcf = new double[M];
                double[] oldcb = new double[M];
                double[] oldbl = new double[M];
                double[] oldst = new double[M];


                double[] cf = new double[M];
                double[] cb = new double[M];
                double[] bl = new double[M];
                double[] st = new double[M];
                double[] wip = new double[M];


                for (int iter = 0; iter < 2000; iter++)
                {
                    m_BCNum++;
                    Array.Copy(c, oldcf, M);

                    int sMaxCount = 500;
                    double epsilon = 1E-6;
                    int s = 0;
                    for (s = 0; s < sMaxCount; s++)
                    {
                        //////迭代开始
                        cf[0] = c[0];
                        cb[M - 1] = c[M - 1];
                        for (int i = M - 2; i >= 0; i--)
                        {
                            bl[i] = (e[i] * oldcf[i] - CalcTP(r[i], u[i], oldcf[i], r[i + 1], u[i + 1], cb[i + 1], N[i])) / (e[i] * oldcf[i]);
                            cb[i] = c[i] * (1 - bl[i]);
                        }
                        for (int i = 1; i < M; i++)
                        {
                            st[i] = (e[i] * cb[i] - CalcTP(r[i - 1], u[i - 1], cf[i - 1], r[i], u[i], cb[i], N[i - 1])) / (e[i] * cb[i]);
                            cf[i] = c[i] * (1 - st[i]);
                        }

                        Array.Copy(cf, oldcf, M);

                        TP1 = cf[M - 1] * e[M - 1];
                        TP2 = cb[0] * e[0];
                        rate = TP1 - TP2;
                        if (Math.Abs(rate) < epsilon) break;

                    }

             //       if (Math.Abs(TP1 - oldTP) < epsilon) return N;

                    //////////前面存储每个缓冲的在制品wip，最后一个存储总的WIP
                    
                    for (int i = 0; i < M - 1; i++)
                    {
                        wip[i] = CalcWIP(r[i], u[i], cf[i], r[i + 1], u[i + 1], cb[i + 1], N[i]);
                        
                    }

                    double tempmax = -1, tempcurrent = 0;
                    double zz;
                    int bcindex = -1;
                    for (int i = 0; i < M - 2; i++)
                    {
                        zz = wip[i] - (N[i + 1] - wip[i + 1]);
                        if (Math.Abs(zz) > tempmax)
                        {
                            tempmax = Math.Abs(zz);
                            tempcurrent = zz;
                            bcindex = i;
                        }
                    }

                    oldTP = TP1;
                    if (bcindex > -1)
                    {
                        if (tempcurrent > 0)
                        {
                          //  if (N[bcindex] == 1) continue;
                            N[bcindex] = N[bcindex] - 1;
                            N[bcindex + 1] = N[bcindex + 1] + 1;

                        }
                        else if (tempcurrent < 0)
                        {
                           // if (N[bcindex+1] == 1) continue;
                            N[bcindex] = N[bcindex] + 1;
                            N[bcindex + 1] = N[bcindex + 1] - 1;
                        }else if(tempcurrent==0)
                        {
                            break;
                        }
                    }
                }

                return N;

            }



            public string ADDXCalcMulTPStr(double[] r, double[] u, double[] c, double[] N, int M)
            {

                m_CalNum++;
                double TP1 = 0;
                double TP2 = 0;
                double rate = 0;
                double[] e = new double[M];

                for (int i = 0; i < M; i++)
                {
                    e[i] = u[i] / (u[i] + r[i]);
                }


                double[] ur = new double[M - 1];
                double[] uu = new double[M - 1];
                double[] uc = new double[M - 1];

                double[] dr = new double[M - 1];
                double[] du = new double[M - 1];
                double[] dc = new double[M - 1];


                for (int i = 0; i < M - 1; i++)
                {
                    uc[i] = c[i];
                    ur[i] = r[i];
                    uu[i] = u[i];
                    dc[i] = c[i + 1];
                    dr[i] = r[i + 1];
                    du[i] = u[i + 1];

                }

                double K1, K2, K3;
                S2Line line = new S2Line();

                string calresult="";

                int sMaxCount = 1000;
                double epsilon = 1E-6;
                int s = 0;
                for (s = 0; s < sMaxCount; s++)
                {
                    //////迭代开始
                    rate = 0;

                    for (int i = 1; i < M - 1; i++)
                    {
                        calresult=CalcFullTP(ur[i - 1], uu[i - 1], uc[i - 1], dr[i - 1], du[i - 1], dc[i - 1], N[i - 1], line);
                        //calresult = CalcFullTP(dr[i - 1], du[i - 1], dc[i - 1], ur[i - 1], uu[i - 1], uc[i - 1], N[i - 1], line);

                        if(calresult=="0")
                        {
                             string tempresult="有值小于0错误,发生up\r\n"+"S="+s.ToString()+"\t i="+(i-1).ToString()+"\t N="+N[i-1].ToString() + "\r\n";
                             return tempresult;
                        }


                        K1 = r[i] * ( (line.p011  / line.tp1) * (uc[i - 1] / dc[i - 1] - 1)) + (line.p001  / line.tp1) * uu[i - 1];
                        K2 = (uu[i - 1] - u[i]) * (line.p011 / line.tp1);
                        K3 =  1 / (1 / line.tp1 + 1 / (e[i] * c[i]) - (du[i - 1] + dr[i - 1]) / (du[i - 1] * dc[i - 1]));

                        ur[i]=(r[i]*K2*K3+u[i]*r[i]+u[i]*K1*K3)/(u[i]+K2*K3-K1*K3);
                        uu[i] = (r[i] * K2 * K3 + u[i] * r[i] + u[i] * K1 * K3) / (r[i] + K1* K3 - K2 * K3);
                        uc[i] = ((r[i] + u[i]) * K3) / (u[i] + K2 * K3 - K1 * K3);



                        if (i == 1)
                        {
                            TP1 = line.tp1;
                        }
                        else
                        {
                            if (Math.Abs(line.tp1 - TP1) > rate)
                            {
                                rate = Math.Abs(line.tp1 - TP1);
                            }
                        }

                    }

                    for (int i = 1; i < M - 1; i++)
                    {
                        int j = M - 1 - i;
                        j = i;

                        calresult=CalcFullTP(ur[j], uu[j], uc[j], dr[j], du[j], dc[j], N[j], line);
                        //calresult = CalcFullTP(dr[j], du[j], dc[j], ur[j], uu[j], uc[j], N[j], line);

                        if (calresult == "0")
                        {
                            string tempresult = "有值小于0错误,发生down\r\n" + "S=" + s.ToString() + "\t i=" + j.ToString() + "\t N=" + N[j].ToString() + "\r\n";
                            return tempresult;
                        }

                        K1= r[j] * ((line.pn11 / line.tp1) * (dc[j] / uc[j] - 1)) + (line.pn10  / line.tp1) * du[j];
                        K2 =(du[j]-u[j])*(line.pn10  / line.tp1);
                        K3 = 1 / (1 / line.tp1 + 1 / (e[j] * c[j]) - (uu[j] + ur[j]) / (uu[j] * uc[j]));


                       dr[j-1]=(r[j]*K2*K3+u[j]*r[j]+u[j]*K1*K3)/(u[j]+K2*K3-K1*K3);
                       du[j-1] = (r[j] * K2 * K3 + u[j] * r[j] + u[j] * K1 * K3) / (r[j] + K1 * K3 - K2 * K3);
                       dc[j-1] = ((r[j] + u[j]) * K3) / (u[j] + K2 * K3 - K1 * K3);
                        
                        
                        if (j == 1)
                        {
                            TP2 = line.tp1;
                        }

                        if (Math.Abs(line.tp1 - TP1) > rate)
                        {
                            rate = Math.Abs(line.tp1 - TP1);
                        }
                    }

                    //rate = TP1 - TP2;

                    if (Math.Abs(rate) < epsilon) break;

                }


                //////////////输出计算结果
                string[] result = new string[6] { "r", "u", "e", "c", "N", "TP" };
                result[result.Length - 1] = result[result.Length - 1] + "\t" + TP1.ToString() + " TP2\t" + TP2.ToString();

                string spec = "F2";
                for (int i = 0; i < M; i++)
                {
                    int j = 0;
                    result[j] = result[j++] + "\t" + r[i].ToString(spec);
                    result[j] = result[j++] + "\t" + u[i].ToString(spec);
                    result[j] = result[j++] + "\t" + e[i].ToString(spec);
                    result[j] = result[j++] + "\t" + c[i].ToString(spec);
                    if (i < M - 1) result[j] = result[j] + "\t" + ((int)N[i]).ToString(); j++;

                }
                string totalResult = "";
                for (int i = 0; i < result.Length; i++)
                {
                    totalResult = totalResult + result[i] + "\r\n";
                }

                ///////输出比率
                result[0] = "MaxTP";
                result[1] = "TP比率";
                for (int i = 0; i < M; i++)
                {
                    int j = 0;
                    result[j] = result[j++] + "\t" + (c[i] * e[i]).ToString("F2");
                    result[j] = result[j++] + "\t" + (TP1 / (c[i] * e[i])).ToString("P2");
                }
                totalResult = totalResult + result[0] + "\r\n";
                totalResult = totalResult + result[1] + "\r\n";
                totalResult = totalResult + "迭代次数：" + s.ToString() + "\r\n";

                totalResult = totalResult + "line:\t" + line.p011 + "\t" + line.p001 + "\t" + line.pn10 + "\t" + line.pn11 + "\t" + "\r\n";
                totalResult = totalResult + "\r\n"+calresult;

                return totalResult;

            }

       /// <summary>
       /// /
       /// </summary>
       /// <param name="lambda"></param>
       /// <param name="stru"></param>
       /// <param name="strc"></param>
       /// <param name="strN"></param>
       /// <param name="strM"></param>
       /// <returns></returns>

            public string DDXCalcMulTPStr(string lambda, string stru, string strc, string strN, string strM)
            {
                int M = int.Parse(strM);
                if (M <= 0 || M > 1000) return "机器数量太大";

                double[] r = new double[M];
                double[] u = new double[M];
                double[] c = new double[M];
                double[] N = new double[M - 1];

                string blank = " \t";
                string[] rSplit = lambda.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string[] uSplit = stru.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string[] cSplit = strc.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string[] NSplit = strN.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (rSplit.Length < M || uSplit.Length < M || cSplit.Length < M || NSplit.Length < M - 1) return "数据中机器数量与其他数据不符";

                for (int i = 0; i < M; i++)
                {
                    r[i] = GetDoubleValue(rSplit[i]);
                    u[i] = GetDoubleValue(uSplit[i]);
                    c[i] = GetDoubleValue(cSplit[i]);
                    if (i < M - 1)
                        N[i] = GetDoubleValue(NSplit[i]);
                }

             
                string result = DDXCalcMulTPStr(r, u, c, N, M) + "\r\n\r\n" + ADDXCalcMulTPStr(r, u, c, N, M);
                return result;
            }

            public string DDXCalcMulTPStr(double[] r, double[] u, double[] c, double[] N, int M)
            {

                m_CalNum++;
                double TP1 = 0;
                double TP2 = 0;
                double rate = 0;
                double[] e = new double[M];

                for (int i = 0; i < M; i++)
                {
                    e[i] = u[i] / (u[i] + r[i]);
                }


                double[] ur = new double[M - 1];
                double[] uu = new double[M - 1];
                double[] uc = new double[M - 1];

                double[] dr = new double[M - 1];
                double[] du = new double[M - 1];
                double[] dc = new double[M - 1];


                for (int i = 0; i < M-1; i++)
                {
                    uc[i] = c[i];
                    ur[i] = r[i];
                    uu[i] = u[i];
                    dc[i] = c[i+1];
                    dr[i] = r[i+1];
                    du[i] = u[i+1];

                }
                string calresult="";

                S2Line line = new S2Line();

                int sMaxCount = 1000;
                double epsilon = 1E-6;
                int s = 0;
                for (s = 0; s < sMaxCount; s++)
                {
                    //////迭代开始
                    rate=0;
                  //  double temp1, temp2, temp3;
                    for (int i = 1; i < M-1; i++)
                    {
                        calresult = CalcFullTP(ur[i - 1], uu[i - 1], uc[i - 1], dr[i - 1], du[i - 1], dc[i - 1], N[i - 1], line);
                       // calresult = CalcFullTP(dr[i - 1], du[i - 1], dc[i - 1], ur[i - 1], uu[i - 1], uc[i - 1], N[i - 1], line);
                        if (calresult == "0")
                        {
                            string tempresult = "有值小于0错误,发生up\r\n" + "S=" + s.ToString() + "\t i=" + (i - 1).ToString() + "\t N=" + N[i - 1].ToString() + "\r\n";
                            return tempresult;
                        }
  
                        ur[i] = r[i] * (1 + ((line.p011 * uc[i] / line.tp1) * (uc[i - 1] / dc[i - 1] - 1))) + (line.p001 * uc[i] / line.tp1) * uu[i - 1];
                        uu[i] = uu[i - 1] * (line.p001 * uu[i] * uc[i]) / (ur[i] * line.tp1) + u[i] * (1 - (line.p001 * uu[i] * uc[i]) / (ur[i] * line.tp1));
                        uc[i] = (uu[i] + ur[i]) / uu[i] * (1 / (1 / line.tp1 + 1 / (e[i] * c[i]) - (du[i - 1] + dr[i - 1]) / (du[i - 1] * dc[i - 1])));

                        
                        if (i == 1)
                        {
                            TP1 = line.tp1;
                        }else
                        {
                            if(Math.Abs(line.tp1-TP1)>rate)
                            {
                                rate = Math.Abs(line.tp1 - TP1);
                            }
                        }


                    }

                    for (int i = 1; i < M - 1; i++)
                    {
                        calresult = CalcFullTP(ur[i], uu[i], uc[i], dr[i], du[i], dc[i], N[i], line);
                        //calresult = CalcFullTP(dr[i], du[i], dc[i], ur[i], uu[i], uc[i], N[i], line);

                        if (calresult == "0")
                        {
                            string tempresult = "有值小于0错误,发生down\r\n" + "S=" + s.ToString() + "\t i=" + (i).ToString() + "\t N=" + N[i].ToString() + "\r\n";
                            return tempresult;
                        }
                     


                        dr[i - 1] = r[i] * (1 + ((line.pn11 * dc[i - 1] / line.tp1) * (dc[i] / uc[i] - 1))) + (line.pn10 * dc[i] / line.tp1) * du[i];
                        du[i - 1] = du[i] * (line.pn10 * du[i - 1] * dc[i - 1]) / (dr[i - 1] * line.tp1) + u[i] * (1 - (line.pn10 * du[i - 1] * dc[i - 1]) / (dr[i - 1] * line.tp1));
                        dc[i - 1] = (du[i - 1] + dr[i - 1]) / du[i - 1] * (1 / (1 / line.tp1 + 1 / (e[i] * c[i]) - (uu[i] + ur[i]) / (uu[i] * uc[i])));

                      
                        
                        
                        
                        if (i == M - 2)
                        {
                            TP2 = line.tp1;
                        }
                        if (Math.Abs(line.tp1 - TP1) > rate)
                        {
                            rate = Math.Abs(line.tp1 - TP1);
                        }
                    }

                  

                    if (Math.Abs(rate) < epsilon) break;

                }


                //////////////输出计算结果
                string[] result = new string[6] { "r", "u", "e", "c", "N", "TP" };
                result[result.Length - 1] = result[result.Length - 1] + "\t" + TP1.ToString() + " TP2\t" + TP2.ToString();

                string spec = "F2";
                for (int i = 0; i < M; i++)
                {
                    int j = 0;
                    result[j] = result[j++] + "\t" + r[i].ToString(spec);
                    result[j] = result[j++] + "\t" + u[i].ToString(spec);
                    result[j] = result[j++] + "\t" + e[i].ToString(spec);
                    result[j] = result[j++] + "\t" + c[i].ToString(spec);
                    if (i < M - 1) result[j] = result[j] + "\t" + ((int)N[i]).ToString(); j++;
                  
                }
                string totalResult = "";
                for (int i = 0; i < result.Length; i++)
                {
                    totalResult = totalResult + result[i] + "\r\n";
                }

                ///////输出比率
                result[0] = "MaxTP";
                result[1] = "TP比率";
                for (int i = 0; i < M; i++)
                {
                    int j = 0;
                    result[j] = result[j++] + "\t" + (c[i] * e[i]).ToString("F2");
                    result[j] = result[j++] + "\t" + (TP1 / (c[i] * e[i])).ToString("P2");
                }
                totalResult = totalResult + result[0] + "\r\n";
                totalResult = totalResult + result[1] + "\r\n";
                totalResult = totalResult + "迭代次数：" + s.ToString() + "\r\n";

                totalResult = totalResult +"line:\t"+ line.p011 + "\t" + line.p001 + "\t" + line.pn10 + "\t" + line.pn11 + "\t" + "\r\n";

                totalResult = totalResult + calresult+"\r\n";
                return totalResult;

            }
       /// <summary>
       /// /
       /// </summary>
       /// <param name="r"></param>
       /// <param name="u"></param>
       /// <param name="c"></param>
       /// <param name="N"></param>
       /// <param name="M"></param>
       /// <returns></returns>

            public double DDXCalcTPRate(double[] r, double[] u, double[] c, double[] N, int M)
            {
                m_CalNum++;
                double TP1 = 0;
                double TP2 = 0;
                double rate = 0;
                double[] e = new double[M];

                for (int i = 0; i < M; i++)
                {
                    e[i] = u[i] / (u[i] + r[i]);
                }


                double[] ur = new double[M - 1];
                double[] uu = new double[M - 1];
                double[] uc = new double[M - 1];

                double[] dr = new double[M - 1];
                double[] du = new double[M - 1];
                double[] dc = new double[M - 1];


                for (int i = 0; i < M - 1; i++)
                {
                    uc[i] = c[i];
                    ur[i] = r[i];
                    uu[i] = u[i];
                    dc[i] = c[i + 1];
                    dr[i] = r[i + 1];
                    du[i] = u[i + 1];

                }


                S2Line line = new S2Line();

                int sMaxCount = 1000;
                double epsilon = 1E-6;
                int s = 0;
                for (s = 0; s < sMaxCount; s++)
                {
                    //////迭代开始
                    rate = 0;

                    for (int i = 1; i < M - 1; i++)
                    {
                        CalcFullTP(ur[i - 1], uu[i - 1], uc[i - 1], dr[i - 1], du[i - 1], dc[i - 1], N[i - 1], line);
                        ur[i] = r[i] * (1 + ((line.p011 * uc[i] / line.tp1) * (uc[i - 1] / dc[i - 1] - 1))) + (line.p001 * uc[i] / line.tp1) * uu[i - 1];
                        uu[i] = uu[i - 1] * (line.p001 * uu[i] * uc[i]) / (ur[i] * line.tp1) + u[i] * (1 - (line.p001 * uu[i] * uc[i]) / (ur[i] * line.tp1));
                        uc[i] = (uu[i] + ur[i]) / ur[i] * (1 / (1 / line.tp1 + 1 / (e[i] * c[i]) - (du[i - 1] + dr[i - 1]) / (du[i - 1] * dc[i - 1])));
                        if (i == 1)
                        {
                            TP1 = line.tp1;
                        }
                        else
                        {
                            if (Math.Abs(line.tp1 - TP1) > rate)
                            {
                                rate = Math.Abs(line.tp1 - TP1);
                            }
                        }


                        //CalcFullTP(ur[i], uu[i], uc[i], dr[i], du[i], dc[i], N[i], line);
                        //dr[i - 1] = r[i] * (1 + ((line.pn11 * dc[i - 1] / line.tp1) * (dc[i] / uc[i] - 1))) + (line.pn10 * dc[i] / line.tp1) * du[i];
                        //du[i - 1] = du[i] * (line.pn10 * du[i - 1] * dc[i - 1]) / (dr[i - 1] * line.tp1) + u[i] * (1 - (line.pn10 * du[i - 1] * dc[i - 1]) / (dr[i - 1] * line.tp1));
                        //dc[i - 1] = (du[i - 1] + dr[i - 1]) / dr[i - 1] * (1 / (1 / line.tp1 + 1 / (e[i] * c[i]) - (uu[i] + ur[i]) / (uu[i] * uc[i])));
                        //if (i == M - 2)
                        //{
                        //    TP2 = line.tp1;
                        //}

                    }

                    for (int i = 1; i < M - 1; i++)
                    {
                        CalcFullTP(ur[i], uu[i], uc[i], dr[i], du[i], dc[i], N[i], line);
                        dr[i - 1] = r[i] * (1 + ((line.pn11 * dc[i - 1] / line.tp1) * (dc[i] / uc[i] - 1))) + (line.pn10 * dc[i] / line.tp1) * du[i];
                        du[i - 1] = du[i] * (line.pn10 * du[i - 1] * dc[i - 1]) / (dr[i - 1] * line.tp1) + u[i] * (1 - (line.pn10 * du[i - 1] * dc[i - 1]) / (dr[i - 1] * line.tp1));
                        dc[i - 1] = (du[i - 1] + dr[i - 1]) / dr[i - 1] * (1 / (1 / line.tp1 + 1 / (e[i] * c[i]) - (uu[i] + ur[i]) / (uu[i] * uc[i])));
                        if (i == M - 2)
                        {
                            TP2 = line.tp1;
                        }
                    }

                    //rate = TP1 - TP2;

                    if (Math.Abs(rate) < epsilon) break;

                }

                double result = TP1;

                return result;

            }



            public string SolveBAP2PSOStr(string lambda, string stru, string strc, string strN, string strM,string strPSO)
            {
                //////////
                /////////
                InitSolutionEx(lambda, stru, strc, strM);
                ////

                string blank = " \t";
                string[] psoSplit = strPSO.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                if (psoSplit.Length < 5)
                {
                    return "PSO参数不全";
                }

                m_psoNum = int.Parse(psoSplit[0]);//粒子数
                m_psoC1 = GetDoubleValue(psoSplit[1]);//学习因子c1
                m_psoC2 = GetDoubleValue(psoSplit[2]);//学习因子c22
                m_psoW1 = GetDoubleValue(psoSplit[3]);//惯性权重
                m_psoW2 = GetDoubleValue(psoSplit[4]);//惯性权重
                m_psoMaxS = int.Parse(psoSplit[5]);//最大迭代次数

                m_psoDim = m_M - 1;
                m_psoXmin = 1;
               
                double[] para = new double[2];
                string[] NSplit = strN.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (NSplit.Length < 2) return "数据中机器数量与其他数据不符";

                for (int i = 0; i < 2; i++)
                {
                    para[i] = GetDoubleValue(NSplit[i]);
                }

                m_TotalN = para[1];
                double rate = para[0];
                m_psoXmax = m_TotalN - m_psoDim + 1;


                double[] newN;
                newN = SolveBAP2PSO(rate, (int)m_TotalN);

                double totalN = 0;
                for (int i = 0; i < m_M - 1; i++)
                {
                    totalN = totalN + newN[i];
                }

                string result = totalN.ToString("F0") + "\r\n";
                result = result + CalcMulTPStr(m_r, m_u, m_c, newN, m_M);
              
               

                return result;
            }


            /// <summary>
            /// /
            /// </summary>
            /// <param name="r"></param>
            /// <param name="u"></param>
            /// <param name="c"></param>
            /// <param name="M"></param>
            /// <param name="S"></param>
            /// <param name="rate"></param>
            /// <param name="startN"></param>
            /// <returns></returns>

            public double[] SolveBAP2PSO(double rate, int startN)
            {

               // Stopwatch sw = new Stopwatch();
              //  sw.Start();

                double minRealC = m_minRealC;
                int M = m_M;
              


                double[] N = new double[M - 1];

                for (int i = 0; i < M - 1; i++) N[i] = 1;

                double TPRate = CalcMulTPRate(m_r, m_u, m_c, N, m_M) / minRealC;

                if (TPRate >= rate) return N;

                N[0] = startN - (M - 1);

                double  bestf;

                double[] newN = CalPSO(startN, out bestf); 


                double[] highN = newN;

                TPRate = bestf / minRealC;

                double LOW = M-1;
                double HIGH = startN ;
                double MID;

                if (TPRate == rate)
                {
                    return newN;
                }
                while (TPRate < rate)
                {
                    LOW = HIGH;
                    HIGH = HIGH * 2;
                   
                    newN = CalPSO((int)HIGH, out bestf); 
                    TPRate = bestf / minRealC;
                    highN = newN;
                }

                while (HIGH - LOW > 1)
                {
                    MID = (int)((LOW + HIGH) / 2);
                    newN = CalPSO((int)MID, out bestf); 
                    TPRate = bestf / minRealC;
                    if (TPRate > rate)
                    {
                        HIGH = MID;
                        highN = newN;
                    }
                    else if (TPRate < rate)
                    {
                        LOW = MID;
                    }
                    else if (TPRate == rate)
                    {
                        return newN;
                    }
                }

              //  sw.Stop();

                return highN;

            }



           public double[] CalPSO(int startN,out double bestf)
           {
                 m_TotalN = startN;
                 m_psoXmax = m_TotalN - m_psoDim + 1;
                // FirstPSOEx pso = new FirstPSOEx(m_psoNum, m_psoDim, m_psoC1, m_psoC2, m_psoW1, m_psoW2, m_psoMaxS, m_psoXmin, m_psoXmax);
                 SecondPSO pso = new SecondPSO(m_psoNum, m_psoDim, m_psoC1, m_psoC2, m_psoW1, m_psoW2, m_psoMaxS, m_psoXmin, m_psoXmax);
                 pso.plc = this;
                 pso.RunOnce();
                 bestf = pso.gbestf;
                 return pso.gbestx;
           }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="lambda"></param>
        /// <param name="stru"></param>
        /// <param name="strc"></param>
        /// <param name="strN"></param>
        /// <param name="strM"></param>
        /// <param name="strS"></param>
        /// <returns></returns>
            public string SolveBAP2ClimbStr(string lambda, string stru, string strc, string strN, string strM, string strS)
            {
                int M = int.Parse(strM);
                if (M <= 0 || M > 1000) return "机器数量太大";

                double[] r = new double[M];
                double[] u = new double[M];
                double[] c = new double[M];
                double[] N = new double[2];

                string blank = " \t";
                string[] rSplit = lambda.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string[] uSplit = stru.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string[] cSplit = strc.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string[] NSplit = strN.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (rSplit.Length < M || uSplit.Length < M || cSplit.Length < M || NSplit.Length < 2) return "数据中机器数量与其他数据不符";

                for (int i = 0; i < M; i++)
                {
                    r[i] = GetDoubleValue(rSplit[i]);
                    u[i] = GetDoubleValue(uSplit[i]);
                    c[i] = GetDoubleValue(cSplit[i]);
                }
                for (int i = 0; i < 2; i++)
                {
                    N[i] = GetDoubleValue(NSplit[i]);
                }


                double[] newN;
                newN = SolveBAP2Climb(r,u,c,M,int.Parse(strS),N[0],(int)N[1]);

                double totalN=0;
                for(int i=0;i<M-1;i++)
                {
                    totalN = totalN + newN[i];
                }

                string result = totalN.ToString("F0")+"\r\n";
                result = result+CalcMulTPStrExU(r,u,c,newN,M);
               


                return result;
            }

        /// <summary>
        /// //
        /// </summary>
        /// <param name="r"></param>
        /// <param name="u"></param>
        /// <param name="c"></param>
        /// <param name="M"></param>
        /// <param name="S"></param>
        /// <param name="rate"></param>
        /// <param name="startN"></param>
        /// <returns></returns>
            public double [] SolveBAP2Climb(double[] r, double[] u, double[] c, int M, int S,double rate,int startN)
            {
                int step = 0;
                Stopwatch sw = new Stopwatch();
                sw.Start();

                double minRealC = double.MaxValue;
                double temp;
                for (int i = 0; i < M; i++)
                {
                    temp = c[i] * u[i] / (r[i] + u[i]);
                    if (temp < minRealC) minRealC = temp;
                }

            
                double[] N = new double[M - 1];

                for (int i = 0; i < M - 1; i++) N[i] = m_minBuffer;
            
                double TPRate = CalcMulTPRate(r, u, c, N, M) / minRealC;

                if (TPRate >= rate) return N;

                N[0] = startN - (M - 1)*m_minBuffer;

                double[] newN = QuickHillClimbingN(r, u, c, N, M, S,step);

                double[] highN=newN;

                TPRate = CalcMulTPRate(r, u, c, newN, M) / minRealC;

                double LOW = m_minBuffer;
                double HIGH = startN - (M - 1)*m_minBuffer;
                double MID;

                 if (TPRate == rate)
                 {
                     return newN;
                 }
                  while (TPRate < rate)
                  {
                        LOW = HIGH;
                        HIGH = HIGH * 2;
                        N[0] = HIGH;
                        newN = QuickHillClimbingN(r, u, c, N, M, S,step);
                        TPRate = CalcMulTPRate(r, u, c, newN, M) / minRealC;
                        highN = newN;
                  }

                  while(HIGH-LOW>1)
                  {
                        MID = (int)((LOW + HIGH) / 2);
                        N[0] = MID;
                        newN = QuickHillClimbingN(r, u, c, N, M, S,step);
                        TPRate = CalcMulTPRate(r, u, c, newN, M) / minRealC;
                        if(TPRate>rate)
                        {
                            HIGH = MID;
                            highN = newN;
                        }else if(TPRate<rate)
                        {
                            LOW = MID;
                        }else if(TPRate==rate)
                        {
                            return newN;
                        }
                    }

                  sw.Stop();

                    return highN;

            }



        /***
         * 
         * 
         * 
         * */
            public string SolveBAP1Str(string lambda, string stru, string strc, string strN, string strM, string strS)
            {
                int M = int.Parse(strM);
                if (M <= 0 || M > 1000) return "机器数量太大";

                double[] r = new double[M];
                double[] u = new double[M];
                double[] c = new double[M];
                int [] N = new int[3];

                string blank = " \t";
                string[] rSplit = lambda.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string[] uSplit = stru.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string[] cSplit = strc.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string[] NSplit = strN.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (rSplit.Length < M || uSplit.Length < M || cSplit.Length < M || NSplit.Length <3) return "数据中机器数量与其他数据不符";

                for (int i = 0; i < M; i++)
                {
                    r[i] = GetDoubleValue(rSplit[i]);
                    u[i] = GetDoubleValue(uSplit[i]);
                    c[i] = GetDoubleValue(cSplit[i]);
                }
                for (int i = 0; i < 3; i++)
                {
                    N[i] = int.Parse(NSplit[i]);
                }

                string result="";
                for(int i=N[0];i<=N[1];i=i+N[2])
                {
                   result=result+SolveBAP1(r, u, c, i, M, int.Parse(strS))+"\r\n";
                }

              

                return result;
            }


            /***
             解决BAP1型问题，缓冲总量确定，如何确定各个缓冲区容量的分配，使得产出最大。
             
             
             *******/


             public string SolveBAP1 (double[] r, double[] u, double[] c, double NTotal, int M,int S)
            {
                int step = 0;
                Stopwatch sw = new Stopwatch();
                sw.Start();
                if (NTotal < M-1) return "缓冲总量太小";
                double[] N = new double[M - 1];

                N[0] = NTotal - (M - 2);
                for (int i = 1; i < M - 1; i++) N[i] = 1;

                double[] newN = QuickHillClimbingN(r, u, c, N, M, S,step);


                double minRealC = double.MaxValue;
                double temp;
                for (int i = 0; i < M; i++)
                {
                    temp = c[i] * u[i] / (r[i] + u[i]);
                    if (temp < minRealC) minRealC = temp;
                }

                // double totalmillsecond=sw.ElapsedMilliseconds;

                double TPRate = CalcMulTPRate(r,u,c,newN,M)/minRealC;
                string spec = "F0";
                string strN = newN[0].ToString(spec);
                 for(int i=1;i<M-1;i++)
                 {
                     strN = strN + "\t" + newN[i].ToString(spec);
                 }
                string strResult;
                sw.Stop();
                strResult = TPRate.ToString() + "\t" + sw.ElapsedMilliseconds.ToString() + "\t" + NTotal.ToString(spec) + "\t" + strN;

                return strResult;
            }



            /// <summary>
            /// 爬山法求解缓冲
            /// </summary>
            /// <param name="lambda"></param>
            /// <param name="stru"></param>
            /// <param name="strc"></param>
            /// <param name="strN"></param>
            /// <param name="strM"></param>
            /// <param name="strS"></param>
            /// <param name="strType">"C"表示优化加工能力，"N"表示缓冲区容量</param>
            /// <returns></returns>
            public string HillClimbingStr(string lambda, string stru, string strc, string strN, string strM,string strS,string strType)
            {
                int M = int.Parse(strM);
                if (M <= 0 || M > 1000) return "机器数量太大";

                double[] r = new double[M];
                double[] u = new double[M];
                double[] c = new double[M];
                double[] N = new double[M - 1];

                string blank = " \t";
                string[] rSplit = lambda.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string[] uSplit = stru.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string[] cSplit = strc.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string[] NSplit = strN.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (rSplit.Length < M || uSplit.Length < M || cSplit.Length < M || NSplit.Length < M - 1) return "数据中机器数量与其他数据不符";

                for (int i = 0; i < M; i++)
                {
                    r[i] = GetDoubleValue(rSplit[i]);
                    u[i] = GetDoubleValue(uSplit[i]);
                    c[i] = GetDoubleValue(cSplit[i]);
                    if (i < M - 1)
                        N[i] = GetDoubleValue(NSplit[i]);
                }


                string result;
                if(strType=="N")
                {
                    double[] newN = HillClimbingN(r, u, c, N, M, int.Parse(strS));
                     result = CalcMulTPStr(r, u, c, newN, M);
                }else
                {
                    double[] newC = HillClimbingC(r, u, c, N, M, int.Parse(strS));
                    result = CalcMulTPStr(r, u, newC, N, M);
                }
               
                return result;
            }

           public int ResetAvgN(double [] N,int M)
            {
                int totalN = 0;
                for (int i = 0; i < M-1; i++)
                {
                    totalN = totalN + (int)N[i];
                }

                for (int i = 0; i < M - 1; i++)
                {
                    N[i] = (int)(totalN / (M - 1));
                }
                N[M - 2] = N[M - 2] + (totalN - (int)(totalN / (M - 1)) * (M - 1));

             
                return 1;
            }

           public int ResetRandN(double[] N, int M)
           {
               int totalN = 0;
               for (int i = 0; i < M - 1; i++)
               {
                   totalN = totalN + (int)N[i];
               }

               int DIM = M - 1;
               double temp = totalN - DIM;
               for (int j = 0; j < DIM; j++)
               {
                   if (j == DIM - 1)
                   {
                       N[j] = temp + 1;
                       continue;
                   }
                   N[j] = (int)(temp * rand.NextDouble()) + 1;
                   temp = temp - (N[j] - 1);
               }
               return 1;
           }

            /// <summary>
            /// 爬山法，根据初始给的N组合，M个机器两两组合，互相交换一次，计算。
            /// 保留最佳结果。
            /// </summary>
            /// <param name="r"></param>
            /// <param name="u"></param>
            /// <param name="c"></param>
            /// <param name="N"></param>
            /// <param name="M"></param>
            /// <returns></returns>
            public double[] QuickHillClimbingN(double[] r, double[] u, double[] c, double[] oldN, int M, int S,int step)
            {
                

                //////////
                m_ClimbNum++;

              
               
                double[] N = new double[M - 1];
                Array.Copy(oldN, N, M - 1);

                if (step <= 0)
                {
                    ResetAvgN(N, M);
                    //step = 16;
                    step = 32;
                }

                m_strClimb = m_strClimb + "step=" + step.ToString() + "\r\n";

                double[] newN = new double[M - 1];

                double max = CalcMulTPRate(r, u, c, N, M);
                Array.Copy(N, newN, M - 1);

                double current;
                // double epsilon = 1E-8;
                int state = 0;

                int maxi = -1, maxj = -1;
                int tempmaxi = -1, tempmaxj = -1;

                for (int t = 0; t < S; t++)
                {
                    state = 0;
                    for (int i = 0; i < M - 1; i++)
                        for (int j = 0; j < M - 1; j++)
                        {
                            if (j != i)
                            {
                                //if (N[i] == 0) continue;
                                if (N[i] < m_minBuffer+step) continue;
                                if ((i == maxj) || (j == maxi)) continue;
                                N[i] = N[i] - step;
                                N[j] = N[j] + step;
                                current = CalcMulTPRate(r, u, c, N, M);
                                if (current > max)
                                {
                                    state = 1;
                                    max = current;
                                    Array.Copy(N, newN, M - 1);
                                    // if (1 - max < epsilon) return newN;
                                    tempmaxi = i;
                                    tempmaxj = j;

                                }
                                N[i] = N[i] + step;
                                N[j] = N[j] - step;

                            }

                        }
                    if (state == 0)
                    {
                        if(step<=1)
                        {
                            return newN;
                        }
                        else
                        {
                            step = step / 2;
                            return QuickHillClimbingN(r, u, c, newN, M, S, step);
                        }
                    }
                    else
                    {
                        Array.Copy(newN, N, M - 1);
                        maxi = tempmaxi;
                        maxj = tempmaxj;
                        ////////tempstr
                        //if(t%10==0)
                        //{
                        //    m_strClimb = m_strClimb + t.ToString() + "\t" + max.ToString() + "\r\n";
                        //}
                        m_NeighborNum++;
                        for(int k=0;k<M-1;k++)
                        {
                            m_strClimb = m_strClimb + newN[k].ToString()+"\t";
                        }
                        m_strClimb = m_strClimb + "\r\n";

                    }

                }



                return newN;
            }



            public string QuickHillClimbingStr(string lambda, string stru, string strc, string strN, string strM, string strS, string strType)
            {
                int M = int.Parse(strM);
                if (M <= 0 || M > 1000) return "机器数量太大";

                double[] r = new double[M];
                double[] u = new double[M];
                double[] c = new double[M];
                double[] N = new double[M - 1];

                string blank = " \t";
                string[] rSplit = lambda.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string[] uSplit = stru.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string[] cSplit = strc.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string[] NSplit = strN.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (rSplit.Length < M || uSplit.Length < M || cSplit.Length < M || NSplit.Length < M - 1) return "数据中机器数量与其他数据不符";

                int totalN=0;
                for (int i = 0; i < M; i++)
                {
                    r[i] = GetDoubleValue(rSplit[i]);
                    u[i] = GetDoubleValue(uSplit[i]);
                    c[i] = GetDoubleValue(cSplit[i]);
                    if (i < M - 1)
                    {
                        N[i] = GetDoubleValue(NSplit[i]);
                        totalN = totalN + (int)N[i];
                    }
                        
                }

                string result;
                int step=0;
                double[] newN;
                m_strClimb = "";
                if (strType == "N")
                {
                    step = 0;
                     newN = QuickHillClimbingN(r, u, c, N, M, int.Parse(strS), step);
                    result = CalcMulTPStr(r, u, c, newN, M);
                }
                else
                {
                    step = 16;
                    newN = QuickHillClimbingN(r, u, c, N, M, int.Parse(strS), step);
                    result = CalcMulTPStr(r, u, c, newN, M);
                }  

                return result;
            }



            public string QuickCompareClimbingStr(string lambda, string stru, string strc, string strN, string strM, string strS, string strType)
            {
                int M = int.Parse(strM);
                if (M <= 0 || M > 1000) return "机器数量太大";

                double[] r = new double[M];
                double[] u = new double[M];
                double[] c = new double[M];
                double[] N = new double[M - 1];

                string blank = " \t";
                string[] rSplit = lambda.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string[] uSplit = stru.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string[] cSplit = strc.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string[] NSplit = strN.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (rSplit.Length < M || uSplit.Length < M || cSplit.Length < M || NSplit.Length < M - 1) return "数据中机器数量与其他数据不符";

                int totalN = 0;
                for (int i = 0; i < M; i++)
                {
                    r[i] = GetDoubleValue(rSplit[i]);
                    u[i] = GetDoubleValue(uSplit[i]);
                    c[i] = GetDoubleValue(cSplit[i]);
                    if (i < M - 1)
                    {
                        N[i] = GetDoubleValue(NSplit[i]);
                        totalN = totalN + (int)N[i];
                    }

                }

                string result="",temp="";
                int step = 0;
                double[] newN;
                double[] oldN = new double[M - 1];
                double rate;
                step = 16;

                for (int i = 0; i < 20;i++ )
                {
                    N.CopyTo(oldN, 0);
                    ResetRandN(oldN, M);
                    newN = QuickHillClimbingN(r, u, c, oldN, M, int.Parse(strS), step);
                    rate = CalcMulTPRate(r, u, c, newN, M);
                    temp = rate.ToString();
                    for(int j=0;j<M-1;j++)
                    {
                        temp = temp + "\t" + newN[j].ToString();
                    }
                    result = result + temp + "\r\n";
                }
                   
              

                return result;
            }


            /// <summary>
            /// 爬山法，根据初始给的N组合，M个机器两两组合，互相交换一次，计算。
            /// 保留最佳结果。
            /// </summary>
            /// <param name="r"></param>
            /// <param name="u"></param>
            /// <param name="c"></param>
            /// <param name="N"></param>
            /// <param name="M"></param>
            /// <returns></returns>
            public double[] HillClimbingN(double[] r, double[] u, double[] c, double[] oldN, int M,int S)
            {
                m_ClimbNum++;
                m_strClimb = "";
                double[] N = new double[M - 1];
                Array.Copy(oldN, N, M - 1);
                double[] newN = new double[M - 1];

                double max = CalcMulTPRate(r,u,c,N,M);
                Array.Copy(N, newN, M - 1);
                
                double current;
               // double epsilon = 1E-8;
                int state = 0;

                int maxi = -1, maxj = -1;
                int tempmaxi = -1, tempmaxj = -1;

                for (int t = 0; t < S;t++ )
                {
                        state = 0;
                    for (int i = 0; i < M - 1; i++)
                        for (int j = 0; j < M - 1; j++)
                        {
                            if (j != i)
                            {
                                //if (N[i] == 0) continue;
                                if (N[i] == 1) continue;
                                if ((i == maxj) || (j == maxi)) continue;
                                N[i] = N[i] - 1;
                                N[j] = N[j] + 1;
                                current = CalcMulTPRate(r, u, c, N, M);
                                if (current > max)
                                {
                                    state = 1;
                                    max = current;
                                    Array.Copy(N, newN, M - 1);
                                   // if (1 - max < epsilon) return newN;
                                    tempmaxi = i;
                                    tempmaxj = j;

                                }
                                N[i] = N[i] + 1;
                                N[j] = N[j] - 1;

                            }

                        }
                    if(state==0) 
                    {
                        return newN;
                    }else
                    {
                        Array.Copy(newN, N, M - 1);
                        maxi = tempmaxi;
                        maxj = tempmaxj;
                        ////////tempstr
                        //if(t%10==0)
                        //{
                        //    m_strClimb = m_strClimb + t.ToString() + "\t" + max.ToString() + "\r\n";
                        //}

                    }

                }



                        return newN;
            }



            public double[] HillClimbingC(double[] r, double[] u, double[] c, double[] N, int M, int S)
            {
                double sum = 0;
                for(int i=0;i<c.Length;i++)
                {
                    sum = sum + c[i];
                }

                double lower = sum / M * 0.85;
                double mmsize = sum / M * 0.001;

                double[] newC = new double[M];

                double max = CalcMulTPRate(r, u, c, N, M);
                Array.Copy(c, newC, M);

                double current;
               // double epsilon = 1E-8;
                int state = 0;

                for (int t = 0; t < S; t++)
                {
                    state = 0;
                    for (int i = 0; i < M; i++)
                        for (int j = 0; j < M; j++)
                        {
                            if (j != i)
                            {
                                if (c[i] <= lower) continue;
                                c[i] = c[i] - mmsize;
                                c[j] = c[j] + mmsize;
                                current = CalcMulTPRate(r, u, c, N, M);
                                if (current > max)
                                {
                                    state = 1;
                                    max = current;
                                    Array.Copy(c, newC, M);
                                   // if (1 - max < epsilon) return newC;
                                }
                                c[i] = c[i] + mmsize;
                                c[j] = c[j] - mmsize;

                            }

                        }
                    if (state == 0)
                    {
                        return newC;
                    }
                    else
                    {
                        Array.Copy(newC, c, M);
                    }

                }



                return newC;
            }

        /// <summary>
        /// ///
        /// </summary>
        /// <returns></returns>

        public string CalcMulTPStr()////string lambda,string stru,string strc,string strN,int M
        {
            //const int M = 5;
            //double[] r = new double[M] { 0.07, 0.05, 0.07, 0.1, 0.06 };
            //double[] u = new double[M] { 0.5, 0.7, 0.6, 0.9, 0.75 };
            //double[] c = new double[M] { 1.25, 1.2, 1, 1.2, 1 };
            //double[] N = new double[M - 1] { 3, 2, 2, 3 };
            string result = CalcMulTPStr(m_r, m_u, m_c, m_N, m_M);
            return result;

        }
        /// <summary>
        /// //参数是字符串，进行转换称数组
        /// </summary>
        /// <returns>返回结果作为字符串</returns>
        public string CalcMulTPStr(string lambda,string stru,string strc,string strN,string strM)
        {
              int M = int.Parse(strM);
            if (M <= 0 || M > 1000) return "机器数量太大";

            double[] r = new double[M];
            double[] u = new double[M];
            double[] c = new double[M];
            double[] N = new double[M - 1];
            
            string blank=" \t";
            string[] rSplit=lambda.Split(blank.ToCharArray(),StringSplitOptions.RemoveEmptyEntries);
            string[] uSplit=stru.Split(blank.ToCharArray(),StringSplitOptions.RemoveEmptyEntries);
            string[] cSplit=strc.Split(blank.ToCharArray(),StringSplitOptions.RemoveEmptyEntries);
            string[] NSplit=strN.Split(blank.ToCharArray(),StringSplitOptions.RemoveEmptyEntries);
            if(rSplit.Length<M||uSplit.Length<M||cSplit.Length<M||NSplit.Length<M-1) return "数据中机器数量与其他数据不符";

            for(int i=0;i<M;i++)
            {
                r[i]= GetDoubleValue(rSplit[i]);
                u[i] = GetDoubleValue(uSplit[i]);
                c[i] = GetDoubleValue(cSplit[i]);
                if (i < M - 1)
                    N[i] = GetDoubleValue(NSplit[i]);
            }

            string result = CalcMulTPStr(r, u, c, N, M);
           // string result = CalcMulTPStrExUR(r, u, c, N, M);
            return result;
        }

        /*****************************************************************************************/
        /* 已知有数组r,u,c,N.数组长度为M。求tp
         * 数组cb,cf,bl,st数组长度为M
         * 另有迭代次数s。s从0到无穷大，直至收敛。s可以为一个类数组。
         * s0.cf=c  si.c1f
         * 
         * 
         */
        /***************************************************************************************/

        public string CalcMulTPStr(double [] r,double[] u ,double[] c,double[] N ,  int  M)
        {
            if (m_stateU == 1)
            {
                return CalcMulTPStrExU(r, u, c, N, M);
            }
            double TP1 = 0;
            double TP2 = 0;
            double rate = 0;
            double[] e = new double[M];

            for (int i = 0; i < M; i++)
            {
                e[i] = u[i] / (u[i] + r[i]);
            }

            double[] oldcf = new double[M];
            double[] oldcb = new double[M];
            double[] oldbl = new double[M];
            double[] oldst = new double[M];


            double[] cf = new double[M];
            double[] cb = new double[M];
            double[] bl = new double[M];
            double[] st = new double[M];


            Array.Copy(c, oldcf, M);

            int sMaxCount = 1000;
            double epsilon = 1E-6;
            int s = 0;
            for ( s = 0; s < sMaxCount; s++)
            {
                //////迭代开始
                cf[0] = c[0];
                cb[M - 1] = c[M - 1];
                for (int i = M - 2; i >= 0; i--)
                {
                    bl[i] = (e[i] * oldcf[i] - CalcTP(r[i], u[i], oldcf[i], r[i + 1], u[i + 1], cb[i + 1], N[i])) / (e[i] * oldcf[i]);
                    cb[i] = c[i] * (1 - bl[i]);
                }
                for (int i = 1; i < M; i++)
                {
                    st[i] = (e[i] * cb[i] - CalcTP(r[i - 1], u[i - 1], cf[i - 1], r[i], u[i], cb[i], N[i - 1])) / (e[i] * cb[i]);
                    cf[i] = c[i] * (1 - st[i]);
                }

                Array.Copy(cf, oldcf, M);

                 TP1 = cf[M - 1] * e[M - 1];
                 TP2 = cb[0] * e[0];
                 rate = TP1 - TP2;
                 if (Math.Abs(rate) < epsilon) break;


            }

            //////////前面存储每个缓冲的在制品wip，最后一个存储总的WIP
            double[] wip = new double[M];
            for (int i = 0; i < M - 1; i++)
            {
                wip[i] = CalcWIP(r[i], u[i], cf[i], r[i + 1], u[i + 1], cb[i + 1], N[i]);
                wip[M-1] = wip[M-1] + wip[i];

            }

            ///////////
            //double TP1 = cf[M - 1] * e[M - 1];
            //double TP2 = cb[0] * e[0];
            //double rate = TP1 - TP2;

            //////////////输出计算结果
            string[] result = new string[11] { "r", "u", "e", "c", "N", "WIP","cf", "cb", "st", "bl", "TP" };
            result[result.Length - 1] = result[result.Length - 1] + "\t" + TP1.ToString() + " TP2\t" + TP2.ToString();
            
            string spec = "F2";
            for (int i = 0; i < M; i++)
            {
                int j = 0;
                result[j] = result[j++] + "\t" + r[i].ToString(spec); 
                result[j] = result[j++] + "\t" + u[i].ToString(spec); 
                result[j] = result[j++] + "\t" + e[i].ToString(spec); 
                result[j] = result[j++] + "\t" + c[i].ToString(spec); 
                if (i < M - 1) result[j] = result[j] + "\t" + ((int)N[i]).ToString(); j++;
                result[j] = result[j++] + "\t" + wip[i].ToString(spec); 
                result[j] = result[j++] + "\t" + cf[i].ToString(spec); 
                result[j] = result[j++] + "\t" + cb[i].ToString(spec); 
                result[j] = result[j++] + "\t" + st[i].ToString(spec); 
                result[j] = result[j++] + "\t" + bl[i].ToString(spec); 
            }
            string totalResult = "";
            for (int i = 0; i < result.Length; i++)
            {
                totalResult = totalResult + result[i] + "\r\n";
            }

            ///////输出比率
            result[0] = "MaxTP";
            result[1] = "TP比率";
            result[2] = "WIP比率";
            for (int i = 0; i < M;i++ )
            {
                int j = 0;
                result[j] = result[j++] + "\t" + (c[i]*e[i]).ToString("F2");
                result[j] = result[j++] + "\t" + (TP1/(c[i] * e[i])).ToString("P2");
                if(i<M-2)
                    result[j] = result[j++] + "\t" + (wip[i] - (N[i + 1] - wip[i + 1])).ToString("F2"); 
            }
            totalResult = totalResult + result[0] + "\r\n";
            totalResult = totalResult + result[1] + "\r\n";
            totalResult = totalResult + result[2] + "\r\n";
            totalResult = totalResult + "迭代次数："+s.ToString() + "\r\n";

                return totalResult;

        }

        /// <summary>
        /// //参数是字符串，进行转换称数组
        /// </summary>
        /// <returns>返回结果作为字符串</returns>
        public string CalcMulTPStrEx(string lambda, string stru, string strc, string strN, string strM)
        {
            int M = int.Parse(strM);
            if (M <= 0 || M > 1000) return "机器数量太大";

            double[] r = new double[M];
            double[] u = new double[M];
            double[] c = new double[M];
            double[] N = new double[M - 1];

            string blank = " \t";
            string[] rSplit = lambda.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string[] uSplit = stru.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string[] cSplit = strc.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string[] NSplit = strN.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (rSplit.Length < M || uSplit.Length < M || cSplit.Length < M || NSplit.Length < M - 1) return "数据中机器数量与其他数据不符";

            for (int i = 0; i < M; i++)
            {
                r[i] = GetDoubleValue(rSplit[i]);
                u[i] = GetDoubleValue(uSplit[i]);
                c[i] = GetDoubleValue(cSplit[i]);
                if (i < M - 1)
                    N[i] = GetDoubleValue(NSplit[i]);
            }

           // string result = CalcMulTPStrEx(r, u, c, N, M);
           // string result = CalcMulMachineStr(r, u, c, N, M);
            string result = CalcMulTPStrExR(r, u, c, N, M);
            return result;
        }


        /// <summary>
        /// //参数是字符串，进行转换称数组
        /// </summary>
        /// <returns>返回结果作为字符串</returns>
        public string CalcMulTPStrExU(string lambda, string stru, string strc, string strN, string strM)
        {
            int M = int.Parse(strM);
            if (M <= 0 || M > 1000) return "机器数量太大";

            double[] r = new double[M];
            double[] u = new double[M];
            double[] c = new double[M];
            double[] N = new double[M - 1];

            string blank = " \t";
            string[] rSplit = lambda.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string[] uSplit = stru.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string[] cSplit = strc.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string[] NSplit = strN.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (rSplit.Length < M || uSplit.Length < M || cSplit.Length < M || NSplit.Length < M - 1) return "数据中机器数量与其他数据不符";

            for (int i = 0; i < M; i++)
            {
                r[i] = GetDoubleValue(rSplit[i]);
                u[i] = GetDoubleValue(uSplit[i]);
                c[i] = GetDoubleValue(cSplit[i]);
                if (i < M - 1)
                    N[i] = GetDoubleValue(NSplit[i]);
            }

            // string result = CalcMulTPStrEx(r, u, c, N, M);
            // string result = CalcMulMachineStr(r, u, c, N, M);
            string result = CalcMulTPStrExU(r, u, c, N, M);
            return result;
        }

        public string CalcMulTPStrEx(double[] r, double[] u, double[] c, double[] N, int M)
        {

            double TP1 = 0;
            double TP2 = 0;
            double rate = 0;
            double[] e = new double[M];

            for (int i = 0; i < M; i++)
            {
                e[i] = u[i] / (u[i] + r[i]);
            }

            double[] cf = new double[M];
            double[] cb = new double[M];

            c.CopyTo(cf, 0);
            c.CopyTo(cb, 0);

            int sMaxCount = 1000;
            double epsilon = m_epsilon;
            int s = 0;
            for (s = 0; s < sMaxCount; s++)
            {
                //////迭代开始
                cf[0] = c[0];
                cb[M - 1] = c[M - 1];


                for (int i = 1; i < M; i++)
                {
                    cf[i] = (c[i] / (e[i] * cb[i])) * CalcTP(r[i - 1], u[i - 1], cf[i - 1], r[i], u[i], cb[i], N[i - 1]);
                    int j = M - 1 - i;
                    cb[j] = (c[j] / (e[j] * cf[j])) * CalcTP(r[j], u[j], cf[j], r[j + 1], u[j + 1], cb[j + 1], N[j]);
                }

                TP1 = cf[M - 1] * e[M - 1];
                TP2 = cb[0] * e[0];
                rate = TP1 - TP2;
                if (Math.Abs(rate) < epsilon) break;


            }

            //////////前面存储每个缓冲的在制品wip，最后一个存储总的WIP
            double[] wip = new double[M];
            for (int i = 0; i < M - 1; i++)
            {
                wip[i] = CalcWIP(r[i], u[i], cf[i], r[i + 1], u[i + 1], cb[i + 1], N[i]);
                wip[M - 1] = wip[M - 1] + wip[i];

            }

        

            //////////////输出计算结果
            string[] result = new string[9] { "r", "u", "e", "c", "N", "WIP", "cf", "cb", "TP" };
            result[result.Length - 1] = result[result.Length - 1] + "\t" + TP1.ToString() + " TP2\t" + TP2.ToString();

            string spec = "F2";
            for (int i = 0; i < M; i++)
            {
                int j = 0;
                result[j] = result[j++] + "\t" + r[i].ToString(spec);
                result[j] = result[j++] + "\t" + u[i].ToString(spec);
                result[j] = result[j++] + "\t" + e[i].ToString(spec);
                result[j] = result[j++] + "\t" + c[i].ToString(spec);
                if (i < M - 1) result[j] = result[j] + "\t" + ((int)N[i]).ToString(); j++;
                result[j] = result[j++] + "\t" + wip[i].ToString(spec);
                result[j] = result[j++] + "\t" + cf[i].ToString(spec);
                result[j] = result[j++] + "\t" + cb[i].ToString(spec);
            }
            string totalResult = "";
            for (int i = 0; i < result.Length; i++)
            {
                totalResult = totalResult + result[i] + "\r\n";
            }

            ///////输出比率
            result[0] = "MaxTP";
            result[1] = "TP比率";
            result[2] = "WIP比率";
            for (int i = 0; i < M; i++)
            {
                int j = 0;
                result[j] = result[j++] + "\t" + (c[i] * e[i]).ToString("F2");
                result[j] = result[j++] + "\t" + (TP1 / (c[i] * e[i])).ToString("P2");
                if (i < M - 2)
                    result[j] = result[j++] + "\t" + (wip[i] - (N[i + 1] - wip[i + 1])).ToString("F2");
            }
            totalResult = totalResult + result[0] + "\r\n";
            totalResult = totalResult + result[1] + "\r\n";
            totalResult = totalResult + result[2] + "\r\n";
            totalResult = totalResult + "迭代次数：" + s.ToString() + "\r\n";

            return totalResult;

        }



        public string CalcMulTPStrExR(double[] r, double[] u, double[] c, double[] N, int M)
        {

            double TP1 = 0;
            double TP2 = 0;
            double rate = 0;

            double[] e = new double[M];

            for (int i = 0; i < M; i++)
            {
                e[i] = u[i] / (u[i] + r[i]);
            }

            double[] rf = new double[M];
            double[] rb = new double[M];

            r.CopyTo(rf, 0);
            r.CopyTo(rb, 0);


            int sMaxCount = 1000;
            double epsilon = m_epsilon;
            int s = 0;
            for (s = 0; s < sMaxCount; s++)
            {

                rf[0] = r[0];
                rb[M - 1] = r[M - 1];
                for (int i = 1; i < M; i++)
                {
                    rf[i]=(r[i]/rb[i])*(c[i]*u[i]/CalcTP(rf[i - 1], u[i - 1], c[i - 1], rb[i], u[i], c[i], N[i - 1])-u[i]); 
                    int j = M - 1 - i;
                    rb[j] = (r[j] / rf[j]) * (c[j] * u[j] / CalcTP(rf[j], u[j], c[j], rb[j + 1], u[j + 1], c[j + 1], N[j])-u[j]);
                }


                TP1 = c[M - 1] * (u[M-1]/(u[M-1]+rf[M-1]));
                TP2 = c[0] * (u[0] / (u[0] + rb[0]));


                rate = TP1 - TP2;
                if (Math.Abs(rate) < epsilon) break;


            }

            //////////前面存储每个缓冲的在制品wip，最后一个存储总的WIP
            double[] wip = new double[M];
            for (int i = 0; i < M - 1; i++)
            {
                wip[i] = CalcWIP(rf[i], u[i], c[i], rb[i + 1], u[i + 1], c[i + 1], N[i]);
                wip[M - 1] = wip[M - 1] + wip[i];

            }



            //////////////输出计算结果
            string[] result = new string[9] { "r", "u", "e", "c", "N", "WIP", "rf", "rb", "TP" };
            result[result.Length - 1] = result[result.Length - 1] + "\t" + TP1.ToString() + " TP2\t" + TP2.ToString();

            string spec = "F2";
            for (int i = 0; i < M; i++)
            {
                int j = 0;
                result[j] = result[j++] + "\t" + r[i].ToString(spec);
                result[j] = result[j++] + "\t" + u[i].ToString(spec);
                result[j] = result[j++] + "\t" + e[i].ToString(spec);
                result[j] = result[j++] + "\t" + c[i].ToString(spec);
                if (i < M - 1) result[j] = result[j] + "\t" + ((int)N[i]).ToString(); j++;
                result[j] = result[j++] + "\t" + wip[i].ToString(spec);
                result[j] = result[j++] + "\t" + rf[i].ToString(spec);
                result[j] = result[j++] + "\t" + rb[i].ToString(spec);
            }
            string totalResult = "";
            for (int i = 0; i < result.Length; i++)
            {
                totalResult = totalResult + result[i] + "\r\n";
            }

            ///////输出比率
            result[0] = "MaxTP";
            result[1] = "TP比率";
            result[2] = "WIP比率";
            for (int i = 0; i < M; i++)
            {
                int j = 0;
                result[j] = result[j++] + "\t" + (c[i] * e[i]).ToString("F2");
                result[j] = result[j++] + "\t" + (TP1 / (c[i] * e[i])).ToString("P2");
                if (i < M - 2)
                    result[j] = result[j++] + "\t" + (wip[i] - (N[i + 1] - wip[i + 1])).ToString("F2");
            }
            totalResult = totalResult + result[0] + "\r\n";
            totalResult = totalResult + result[1] + "\r\n";
            totalResult = totalResult + result[2] + "\r\n";
            totalResult = totalResult + "迭代次数：" + s.ToString() + "\r\n";

            return totalResult;

        }



        public double CalcMulTPRateR(double[] r, double[] u, double[] c, double[] N, int M)
        {

            double TP1 = 0;
            double TP2 = 0;
            double rate = 0;

            double[] e = new double[M];

            for (int i = 0; i < M; i++)
            {
                e[i] = u[i] / (u[i] + r[i]);
            }

            double[] rf = new double[M];
            double[] rb = new double[M];

            r.CopyTo(rf, 0);
            r.CopyTo(rb, 0);


            int sMaxCount = 1000;
            double epsilon = m_epsilon;
            int s = 0;
            for (s = 0; s < sMaxCount; s++)
            {

                rf[0] = r[0];
                rb[M - 1] = r[M - 1];
                for (int i = 1; i < M; i++)
                {
                    rf[i] = (r[i] / rb[i]) * (c[i] * u[i] / CalcTP(rf[i - 1], u[i - 1], c[i - 1], rb[i], u[i], c[i], N[i - 1]) - u[i]);
                    int j = M - 1 - i;
                    rb[j] = (r[j] / rf[j]) * (c[j] * u[j] / CalcTP(rf[j], u[j], c[j], rb[j + 1], u[j + 1], c[j + 1], N[j]) - u[j]);
                }


                TP1 = c[M - 1] * (u[M - 1] / (u[M - 1] + rf[M - 1]));
                TP2 = c[0] * (u[0] / (u[0] + rb[0]));


                rate = TP1 - TP2;
                if (Math.Abs(rate) < epsilon) break;


            }



            return (TP1 + TP2) / 2;

        }


        public double CalcMulTPRateU(double[] r, double[] u, double[] c, double[] N, int M)
        {

            double TP1 = 0;
            double TP2 = 0;
            double rate = 0;

            double[] e = new double[M];

            for (int i = 0; i < M; i++)
            {
                e[i] = u[i] / (u[i] + r[i]);
            }

            double[] uf = new double[M];
            double[] ub = new double[M];

            u.CopyTo(uf, 0);
            u.CopyTo(ub, 0);


            int sMaxCount = 1000;
            double epsilon = m_epsilon;
            int s = 0;
            for (s = 0; s < sMaxCount; s++)
            {

                uf[0] = u[0];
                ub[M - 1] = u[M - 1];
                for (int i = 1; i < M; i++)
                {
                  


                    uf[i] = (u[i] / ub[i]) * (r[i] / (c[i] / CalcTP(r[i - 1], uf[i - 1], c[i - 1], r[i], ub[i], c[i], N[i - 1]) - 1));
                    int j = M - 1 - i;
                    ub[j] = (u[j] / uf[j]) * (r[j] / (c[j] / CalcTP(r[j], uf[j], c[j], r[j + 1], ub[j + 1], c[j + 1], N[j]) - 1));
                }


                TP1 = c[M - 1] * (uf[M - 1] / (uf[M - 1] + r[M - 1]));
                TP2 = c[0] * (ub[0] / (ub[0] + r[0]));


                rate = TP1 - TP2;
                if (Math.Abs(rate) < epsilon) break;


            }
            return (TP1 + TP2) / 2;
        }



        public string CalcMulTPStrExU(double[] r, double[] u, double[] c, double[] N, int M)
        {

            double TP1 = 0;
            double TP2 = 0;
            double rate = 0;

            double[] e = new double[M];

            for (int i = 0; i < M; i++)
            {
                e[i] = u[i] / (u[i] + r[i]);
            }

            double[] uf = new double[M];
            double[] ub = new double[M];

            u.CopyTo(uf, 0);
            u.CopyTo(ub, 0);


            int sMaxCount = 1000;
            double epsilon = m_epsilon;
            int s = 0;
            for (s = 0; s < sMaxCount; s++)
            {

                uf[0] = u[0];
                ub[M - 1] = u[M - 1];
                for (int i = 1; i < M; i++)
                {
                    uf[i] = (u[i] / ub[i]) * (r[i] / (c[i]/CalcTP(r[i - 1], uf[i - 1], c[i - 1], r[i], ub[i], c[i], N[i - 1]) - 1));
                    int j = M - 1 - i;
                    ub[j] = (u[j] / uf[j]) * (r[j] /(c[j]/ CalcTP(r[j], uf[j], c[j], r[j + 1], ub[j + 1], c[j + 1], N[j]) - 1));
                }


                TP1 = c[M - 1] * (uf[M - 1] / (uf[M - 1] + r[M - 1]));
                TP2 = c[0] * (ub[0] / (ub[0] + r[0]));


                rate = TP1 - TP2;
                if (Math.Abs(rate) < epsilon) break;


            }

            //////////前面存储每个缓冲的在制品wip，最后一个存储总的WIP
            double[] wip = new double[M];
            for (int i = 0; i < M - 1; i++)
            {
                wip[i] = CalcWIP(r[i], uf[i], c[i], r[i + 1], ub[i + 1], c[i + 1], N[i]);
                wip[M - 1] = wip[M - 1] + wip[i];

            }



            //////////////输出计算结果
            string[] result = new string[9] { "r", "u", "e", "c", "N", "WIP", "uf", "ub", "TP" };
            result[result.Length - 1] = result[result.Length - 1] + "\t" + TP1.ToString() + " TP2\t" + TP2.ToString();

            string spec = "F2";
            for (int i = 0; i < M; i++)
            {
                int j = 0;
                result[j] = result[j++] + "\t" + r[i].ToString(spec);
                result[j] = result[j++] + "\t" + u[i].ToString(spec);
                result[j] = result[j++] + "\t" + e[i].ToString(spec);
                result[j] = result[j++] + "\t" + c[i].ToString(spec);
                if (i < M - 1) result[j] = result[j] + "\t" + ((int)N[i]).ToString(); j++;
                result[j] = result[j++] + "\t" + wip[i].ToString(spec);
                result[j] = result[j++] + "\t" + uf[i].ToString(spec);
                result[j] = result[j++] + "\t" + ub[i].ToString(spec);
            }
            string totalResult = "";
            for (int i = 0; i < result.Length; i++)
            {
                totalResult = totalResult + result[i] + "\r\n";
            }

            ///////输出比率
            result[0] = "MaxTP";
            result[1] = "TP比率";
            result[2] = "WIP比率";
            for (int i = 0; i < M; i++)
            {
                int j = 0;
                result[j] = result[j++] + "\t" + (c[i] * e[i]).ToString("F2");
                result[j] = result[j++] + "\t" + (TP1 / (c[i] * e[i])).ToString("P2");
                if (i < M - 2)
                    result[j] = result[j++] + "\t" + (wip[i] - (N[i + 1] - wip[i + 1])).ToString("F2");
            }
            totalResult = totalResult + result[0] + "\r\n";
            totalResult = totalResult + result[1] + "\r\n";
            totalResult = totalResult + result[2] + "\r\n";
            totalResult = totalResult + "迭代次数：" + s.ToString() + "\r\n";

            return totalResult;

        }


       /// <summary>
       /// //U,R一起变 
       /// </summary>
       /// <param name="r"></param>
       /// <param name="u"></param>
       /// <param name="c"></param>
       /// <param name="N"></param>
       /// <param name="M"></param>
       /// <returns></returns>
        public string CalcMulTPStrExUR(double[] r, double[] u, double[] c, double[] N, int M)
        {

            double TP1 = 0;
            double TP2 = 0;
            double rate = 0;

            double[] e = new double[M];

            for (int i = 0; i < M; i++)
            {
                e[i] = u[i] / (u[i] + r[i]);
            }

            double[] uf = new double[M];
            double[] ub = new double[M];

            u.CopyTo(uf, 0);
            u.CopyTo(ub, 0);

            double[] rf = new double[M];
            double[] rb = new double[M];

            r.CopyTo(rf, 0);
            r.CopyTo(rb, 0);

            double RUratio;
            double MTBF1 ;
            double MTBF2;
            double MTBF;
            double rrate ;
            double urate ;


            int sMaxCount = 1000;
            double epsilon = m_epsilon;
            int s = 0;
            for (s = 0; s < sMaxCount; s++)
            {
                

                uf[0] = u[0];
                ub[M - 1] = u[M - 1];
                rf[0] = r[0];
                rb[M - 1] = r[M - 1];
                for (int i = 1; i < M; i++)
                {
                   
                    RUratio=(c[i] / CalcTP(rf[i - 1], uf[i - 1], c[i - 1], rb[i], ub[i], c[i], N[i - 1]) - 1);
                    MTBF1=(1/rf[i-1]+1/uf[i-1]);
                    MTBF2=(1/rb[i]+1/ub[i]);
                    MTBF=MTBF1;if(MTBF<MTBF2) MTBF=MTBF2;
                    rrate=(1+RUratio)/MTBF;
                    urate = (1 + 1 / RUratio) / MTBF;
                    rf[i] = (r[i] / rb[i]) * rrate;
                    uf[i] = (u[i] / ub[i]) * urate;


                    int j = M - 1 - i;

                    RUratio = (c[j] / CalcTP(rf[j], uf[j], c[j], rb[j + 1], ub[j + 1], c[j + 1], N[j]) - 1);
                    MTBF1 = (1 / rf[j] + 1 / uf[j]);
                    MTBF2 = (1 / rb[j+1] + 1 / ub[j+1]);
                    MTBF = MTBF1; if (MTBF < MTBF2) MTBF = MTBF2;
                    rrate = (1 + RUratio) / MTBF;
                    urate = (1 + 1 / RUratio) / MTBF;
                    rb[j] = (r[j] / rf[j]) * rrate;
                    ub[j] = (u[j] / uf[j]) * urate;

                }


                TP1 = c[M - 1] * (uf[M - 1] / (uf[M - 1] + rf[M - 1]));
                TP2 = c[0] * (ub[0] / (ub[0] + rb[0]));


                rate = TP1 - TP2;
                if (Math.Abs(rate) < epsilon) break;


            }

            //////////前面存储每个缓冲的在制品wip，最后一个存储总的WIP
            double[] wip = new double[M];
            for (int i = 0; i < M - 1; i++)
            {
                wip[i] = CalcWIP(r[i], uf[i], c[i], r[i + 1], ub[i + 1], c[i + 1], N[i]);
                wip[M - 1] = wip[M - 1] + wip[i];

            }



            //////////////输出计算结果
            string[] result = new string[9] { "r", "u", "e", "c", "N", "WIP", "uf", "ub", "TP" };
            result[result.Length - 1] = result[result.Length - 1] + "\t" + TP1.ToString() + " TP2\t" + TP2.ToString();

            string spec = "F2";
            for (int i = 0; i < M; i++)
            {
                int j = 0;
                result[j] = result[j++] + "\t" + r[i].ToString(spec);
                result[j] = result[j++] + "\t" + u[i].ToString(spec);
                result[j] = result[j++] + "\t" + e[i].ToString(spec);
                result[j] = result[j++] + "\t" + c[i].ToString(spec);
                if (i < M - 1) result[j] = result[j] + "\t" + ((int)N[i]).ToString(); j++;
                result[j] = result[j++] + "\t" + wip[i].ToString(spec);
                result[j] = result[j++] + "\t" + uf[i].ToString(spec);
                result[j] = result[j++] + "\t" + ub[i].ToString(spec);
            }
            string totalResult = "";
            for (int i = 0; i < result.Length; i++)
            {
                totalResult = totalResult + result[i] + "\r\n";
            }

            ///////输出比率
            result[0] = "MaxTP";
            result[1] = "TP比率";
            result[2] = "WIP比率";
            for (int i = 0; i < M; i++)
            {
                int j = 0;
                result[j] = result[j++] + "\t" + (c[i] * e[i]).ToString("F2");
                result[j] = result[j++] + "\t" + (TP1 / (c[i] * e[i])).ToString("P2");
                if (i < M - 2)
                    result[j] = result[j++] + "\t" + (wip[i] - (N[i + 1] - wip[i + 1])).ToString("F2");
            }
            totalResult = totalResult + result[0] + "\r\n";
            totalResult = totalResult + result[1] + "\r\n";
            totalResult = totalResult + result[2] + "\r\n";
            totalResult = totalResult + "迭代次数：" + s.ToString() + "\r\n";

            return totalResult;

        }


        public double CalcMulTPRateUR(double[] r, double[] u, double[] c, double[] N, int M)
        {

            double TP1 = 0;
            double TP2 = 0;
            double rate = 0;

            double[] e = new double[M];

            for (int i = 0; i < M; i++)
            {
                e[i] = u[i] / (u[i] + r[i]);
            }

            double[] uf = new double[M];
            double[] ub = new double[M];

            u.CopyTo(uf, 0);
            u.CopyTo(ub, 0);

            double[] rf = new double[M];
            double[] rb = new double[M];

            r.CopyTo(rf, 0);
            r.CopyTo(rb, 0);

            double RUratio;
            double MTBF1;
            double MTBF2;
            double MTBF;
            double rrate;
            double urate;


            int sMaxCount = 1000;
            double epsilon = m_epsilon;
            int s = 0;
            for (s = 0; s < sMaxCount; s++)
            {


                uf[0] = u[0];
                ub[M - 1] = u[M - 1];
                rf[0] = r[0];
                rb[M - 1] = r[M - 1];
                for (int i = 1; i < M; i++)
                {

                    RUratio = (c[i] / CalcTP(rf[i - 1], uf[i - 1], c[i - 1], rb[i], ub[i], c[i], N[i - 1]) - 1);
                    MTBF1 = (1 / rf[i - 1] + 1 / uf[i - 1]);
                    MTBF2 = (1 / rb[i] + 1 / ub[i]);
                    MTBF = MTBF1; if (MTBF < MTBF2) MTBF = MTBF2;


                    MTBF = ((1 / r[i - 1] + 1 / u[i - 1]) + (1 / r[i] + 1 / u[i])) / 2;
                    rrate = (1 + RUratio) / MTBF;
                    urate = (1 + 1 / RUratio) / MTBF;
                    rf[i] = (r[i] / rb[i]) * rrate;
                    uf[i] = (u[i] / ub[i]) * urate;


                    int j = M - 1 - i;

                    RUratio = (c[j] / CalcTP(rf[j], uf[j], c[j], rb[j + 1], ub[j + 1], c[j + 1], N[j]) - 1);
                    MTBF1 = (1 / rf[j] + 1 / uf[j]);
                    MTBF2 = (1 / rb[j + 1] + 1 / ub[j + 1]);
                    MTBF = MTBF1; if (MTBF < MTBF2) MTBF = MTBF2;

                    MTBF = ((1 / r[j + 1] + 1 / u[j + 1]) + (1 / r[j] + 1 / u[j])) / 2;
                    rrate = (1 + RUratio) / MTBF;
                    urate = (1 + 1 / RUratio) / MTBF;
                    rb[j] = (r[j] / rf[j]) * rrate;
                    ub[j] = (u[j] / uf[j]) * urate;

                }


                TP1 = c[M - 1] * (uf[M - 1] / (uf[M - 1] + rf[M - 1]));
                TP2 = c[0] * (ub[0] / (ub[0] + rb[0]));


                rate = TP1 - TP2;
                if (Math.Abs(rate) < epsilon) break;


            }

        

            return (TP1+TP2)/2;

        }

        /************************************************************************/
        /* c是加工能力是加工周期的倒数，r是平均故障发生时间的倒数，u是平均维修时间的倒数，
         * N是缓冲区容量，以单位时间为计量单位，WIP是缓冲区的平均占用量 */
        /************************************************************************/
        public double CalcWIP(double r1, double u1, double c1, double r2, double u2, double c2, double N)
        {
            if (r1 <= 0 || u1 <= 0 || r2 <= 0 || u2 <= 0 || N < 0 || c1 <= 0 || c2 <= 0)
            {
                return 0;
            }

            if (c1 == c2)
            {
                double PR = CalcSyncWIP(r1 / c1, u1 / c1, r2 / c2, u2 / c2, N);
                return c1 * PR;
            }
            double E1 = (r2 * (c1 * u1 + c2 * u2) - u2 * (c2 - c1) * (u1 + u2 + r1 + r2)) / (c1 * (c2 - c1) * (u1 + u2));
            double E2 = (-r2 * (c1 * u1 + c2 * u2)) / (c1 * (c2 - c1) * (u1 + u2));
            double E3 = (-r1 * (c1 * u1 + c2 * u2)) / (c2 * (c2 - c1) * (u1 + u2));
            double E4 = (r1 * (c1 * u1 + c2 * u2) + u1 * (c2 - c1) * (u1 + u2 + r1 + r2)) / (c2 * (c2 - c1) * (u1 + u2));
            // if (c1 < c2) { E1 = -E1; E4 = -E4; }

            double K1 = (E1 + E4) / 2 + 0.5 * Math.Sqrt(Math.Pow((E1 - E4), 2) + 4 * E2 * E3);
            double K2 = (E1 + E4) / 2 - 0.5 * Math.Sqrt(Math.Pow((E1 - E4), 2) + 4 * E2 * E3);
            
            /***   计算F1到F11   ***/
            double F1;
            if (c1 < c2)
            {
                F1 = (c2 * (u1 + u2 + r1) * (E1 - E4 - K1 + K2) + 2 * r1 * c1 * E2) / (c2 * (u1 + u2 + r1) * (E1 - E4 + K1 - K2) + 2 * r1 * c1 * E2);
            }
            else
            {
                F1 = (c2 * r2 * (E1 - E4 - K1 + K2) + 2 * (u1 + u2 + r2) * c1 * E2) / (c2 * r2 * (E1 - E4 - K1 - K2) + 2 * (u1 + u2 + r2) * c1 * E2);
            }
            double F2 = E1 - E4 - (K1 - K2);
            double F3 = E1 - E4 + (K1 - K2);
            double F4 = r1 / (u1 + u2) + c2 / (c2 - c1);
            double F5 = r2 / (u1 + u2) - c1 / (c2 - c1);

            ///////////////////
            double F6 = 0;
            if (c1 < c2)
            {
                F6 = (2 * E2 * c1 * (r1 + u1) * (r2 + u1 + u2) * (1 - F1 * Math.Exp((K1 - K2) * N)) + c2 * r2 * (r2 + u2) * (-F2 + F1 * F3 * Math.Exp((K1 - K2) * N))) / (2 * E2 * r2 * u1 * (r1 + r2 + u1 + u2));
            }
            else
            {
                F6 = (c2 * (F1 * F3 - F2)) / (2 * u1 * E2);
            }
            ////////////////////////////
            double F7 = 0;
            if (c1 < c2)
            {
                F7 = c1 * (1 - F1) * Math.Exp(K1 * N) / u2;
            }
            else
            {
                F7 = (Math.Exp(K1 * N) * (c2 * (r2 + u2) * (r1 + u1 + u2) * (-F2 + F1 * F3 * Math.Exp((K2 - K1) * N)) + 2 * E2 * c1 * r1 * (r1 + u1) * (1 - F1 * Math.Exp((K2 - K1) * N)))) / (2 * E2 * r1 * u2 * (r1 + r2 + u1 + u2));
            }
            ///////////////////////////////////////////////////////
            double F8 = 0;
            if (c1 < c2)
            {
                F8 = (Math.Exp(K1 * N) - 1) / K1 - ((Math.Exp(K2 * N) - 1) / K2) * F1 * Math.Exp((K1 - K2) * N);
            }
            else
            {
                F8 = (Math.Exp(K1 * N) - 1) / K1 - ((Math.Exp(K2 * N) - 1) / K2) * F1;
            }

            //////////////////////
            double F9 = 0;
            if (c1 < c2)
            {
                F9 = ((1 - Math.Exp(K1 * N)) / K1) * (F2 / (2 * E2)) + ((Math.Exp(K2 * N) - 1) / K2) * (F1 * F3 / (2 * E2)) * Math.Exp(-(K2 - K1) * N);
            }
            else
            {
                F9 = ((1 - Math.Exp(K1 * N)) / K1) * (F2 / (2 * E2)) + ((Math.Exp(K2 * N) - 1) / K2) * (F1 * F3 / (2 * E2));
            }

            ////////////////////////////////
            double F10 = 0;
            if (c1 < c2)
            {
                F10 = (1 + (K1 * N - 1) * Math.Exp(K1 * N)) / (K1 * K1) - (1 + (K2 * N - 1) * Math.Exp(K2 * N)) / (K2 * K2) * F1 * Math.Exp((K1 - K2) * N);
            }
            else
            {
                F10 = (1 + (K1 * N - 1) * Math.Exp(K1 * N)) / (K1 * K1) - (1 + (K2 * N - 1) * Math.Exp(K2 * N)) / (K2 * K2) * F1;
            }

            /////////////////////////////////////////
            double F11 = 0;
            if (c1 < c2)
            {
                F11 = -(F2 * (1 + (K1 * N - 1) * (Math.Exp(K1 * N)))) / (2 * E2 * K1 * K1) + ((1 + (K2 * N - 1) * Math.Exp(K2 * N)) * F1 * F3 * Math.Exp((K1 - K2) * N)) / (2 * E2 * K2 * K2);
            }
            else
            {
                F11 = -(F2 * (1 + (K1 * N - 1) * (Math.Exp(K1 * N)))) / (2 * E2 * K1 * K1) + ((1 + (K2 * N - 1) * Math.Exp(K2 * N)) * F1 * F3) / (2 * E2 * K2 * K2);
            }

            ///////////////计算WIP
            double WIP = 0;
            WIP = (F4 * F10 + F5 * F11 + F7 * N) / (F4 * F8 + F5 * F9 + F6 + F7);

            return WIP;
        }


        /************************************************************************/
        /* c是加工能力是加工周期的倒数，r是平均故障发生时间的倒数，u是平均维修时间的倒数，
         * N是缓冲区容量，以单位时间为计量单位，TP是最后一台机器，平均单位时间内生产的产品数量 */
        /************************************************************************/
        public double CalcTP(double r1, double u1, double c1, double r2, double u2, double c2, double N)
        {
            if (r1 <= 0 || u1 <= 0 || r2 <= 0 || u2 <= 0 || N < 0 || c1 <= 0 || c2 <= 0)
            {
                return 0;
            }

            if (c1 == c2)
            {
                double PR = CalcPR(r1 / c1, u1 / c1, r2 / c2, u2 / c2, N);
                return c1 * PR;
            }
            double E1 = (r2 * (c1 * u1 + c2 * u2) - u2 * (c2 - c1) * (u1 + u2 + r1 + r2)) / (c1 * (c2 - c1) * (u1 + u2));
            double E2 = (-r2 * (c1 * u1 + c2 * u2)) / (c1 * (c2 - c1) * (u1 + u2));
            double E3 = (-r1 * (c1 * u1 + c2 * u2)) / (c2 * (c2 - c1) * (u1 + u2));
            double E4 = (r1 * (c1 * u1 + c2 * u2) + u1 * (c2 - c1) * (u1 + u2 + r1 + r2)) / (c2 * (c2 - c1) * (u1 + u2));

         
           
           

            double K1 = (E1 + E4) / 2 + 0.5 * Math.Sqrt(Math.Pow((E1 - E4), 2) + 4 * E2 * E3);
            double K2 = (E1 + E4) / 2 - 0.5 * Math.Sqrt(Math.Pow((E1 - E4), 2) + 4 * E2 * E3);


            double e1 = u1 / (u1 + r1);
            double e2 = u2 / (u2 + r2);
            double G0 = Math.Sqrt(Math.Pow((c1 * (u1 + u2 + r2) - c2 * (u1 + u2 + r1)), 2) + 4 * c1 * c2 * r1 * r2);
            double G1 = 0;
            if (c1 < c2)
            {
                G1 = u1 * G0 * (G0 + c1 * (u1 + u2 + r2) - c2 * (u1 + u2 + r1));
            }
            else
            {
                G1 = u2 * G0 * (G0 + c1 * (u1 + u2 + r2) - c2 * (u1 + u2 + r1));
            }
            ////////////////////////////////////////
            double G2 = 0;
            if (c1 < c2)
            {
                G2 = u2 * r1 * c2 * ((c1 - c2) * (u1 - u2) - (c2 * r1 + c1 * r2) - G0);
            }
            else
            {
                G2 = u1 * r2 * c1 * ((c1 - c2) * (u1 - u2) - (c2 * r1 + c1 * r2) + G0);
            }
            //////////////////////////////////////////////
            double G3 = 0;
            if (c1 < c2)
            {
                G3 = (e2 * (c2 - c1 * e1) * G1 + c1 * e1 * (1 - e2) * G2) / (c1 * e1 * (e2 - 1));
            }
            else
            {
                G3 = (e1 * (c1 - c2 * e2) * G1 + c2 * e2 * (1 - e1) * G2) / (c2 * e2 * (e1 - 1));
            }
            ///////////////////////////////////////////////////////
            double G4 = 0;
            if (c1 < c2) G4 = c2 * e2 * G1; else G4 = c1 * e1 * G1;

            double G5 = 0;
            if (c1 < c2) G5 = c1 * e1 * G2; else G5 = c2 * e2 * G2;

            double G6 = 0;
            if (c1 < c2) G6 = c1 * e1 * G3; else G6 = c2 * e2 * G3;

            ////////////////////////////////////////////////////////////
            double TP = 0;
            if (c1 < c2)
            {
                TP = (G4 + G5 * Math.Exp(-K2 * N) + G6 * Math.Exp(-K1 * N)) / (G1 + G2 * Math.Exp(-K2 * N) + G3 * Math.Exp(-K1 * N));
            }
            else
            {
                TP = (G4 + G5 * Math.Exp(K2 * N) + G6 * Math.Exp(K1 * N)) / (G1 + G2 * Math.Exp(K2 * N) + G3 * Math.Exp(K1 * N));

            }

            return TP;
        }


        /************************************************************************/
        /* r是平均故障间隔时间的倒数，u是平均维修时间的倒数，N是缓冲区容量，以周期为计量单位 
         WIP是缓冲区的平均占用量*/
        /************************************************************************/
        public double CalcSyncWIP(double r1, double u1, double r2, double u2, double N)
        {
          
           double K=((u1+u2+r1+r2)*(r2*u1-r1*u2))/((u1+u2)*(r1+r2));
  


            ///////计算D1到D5
            double D1= (u1+u2)/(r1+r2);
            double D2 = 0;
            if (K==0)
            {
                D2 = (2 + D1 + 1 / D1) * N;
            }else
            {
                D2 = (2 + D1 + 1 / D1) * ((Math.Exp(K*N)-1)/K);
            }

            double D3 = ((r1 + r2 + u1 + u2) * (r2 + u1) + r1 * u2 - r2 * u1) / (r2 * u1 * (r1 + r2 + u1 + u2));

            double D4 = ((r1 + r2 + u1 + u2) * (r1 + u2) + r2 * u1 - r1 * u2) / (r1 * u2 * (r1 + r2 + u1 + u2))*Math.Exp(K*N);

            double D5=(D2*(1+(K*N-1)*Math.Exp(K*N)))/(K*(Math.Exp(K*N)-1))+D4*N;

            //////////////////计算WIP
            double WIP=0;
            if(K==0)
            {
                WIP=(D2/2+D4)*N/(D2+D3+D4);
            }else
            {
                WIP=D5/(D2+D3+D4);
            }


            return WIP;
        }



        /************************************************************************/
        /* r是平均故障间隔时间的倒数，u是平均维修时间的倒数，N是缓冲区容量，以周期为计量单位 
         PR是最后一台机器，平均单位周期内生产的产品数量*/
        /************************************************************************/
        public double CalcPR(double r1, double u1, double r2, double u2, double N)
        {
            double PR1, PR2;
            double e1 = u1 / (r1 + u1);
            double e2 = u2 / (r2 + u2);
            PR1 = e2 * (1 - CalcQ(r1, u1, r2, u2, N));
            PR2 = e1 * (1 - CalcQ(r2, u2, r1, u1, N));
            double rate = PR1 / PR2;
            return PR1;
        }

        /************************************************************************/
        /* r是平均故障间隔时间的倒数，u是平均维修时间的倒数，N是缓冲区容量，以周期为计量单位 */
        /************************************************************************/

        public double CalcQ(double r1, double u1, double r2, double u2, double N)////这里的r1，u1等都是对周期来说的，
        {
            double e1 = u1 / (r1 + u1);
            double e2 = u2 / (r2 + u2);
            double psi = (e1 * (1 - e2)) / (e2 * (1 - e1));
            double beta = ((r1 + r2 + u1 + u2) * (r1 * u2 - r2 * u1)) / ((r1 + r2) * (u1 + u2));
            double Q;
            if (beta == 0)
            {
                Q = (r1 * (r1 + r2) * (u1 + u2)) / ((r1 + u1) * ((r1 + r2) * (u1 + u2) + r2 * u1 * (r1 + r2 + u1 + u2) * N));
            }
            else
            {
                Q = ((1 - e1) * (1 - psi)) / (1 - psi * Math.Exp(-beta * N));
            }

            return Q;

        }



        public string TestTPStr(string lambda, string stru, string strc, string strN, string strM)
        {
            int M = int.Parse(strM);
            if (M <= 0 || M > 1000) return "机器数量太大";

            double[] r = new double[M];
            double[] u = new double[M];
            double[] c = new double[M];
            double[] N = new double[M - 1];

            string blank = " \t";
            string[] rSplit = lambda.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string[] uSplit = stru.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string[] cSplit = strc.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string[] NSplit = strN.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (rSplit.Length < M || uSplit.Length < M || cSplit.Length < M || NSplit.Length < M - 1) return "数据中机器数量与其他数据不符";

            for (int i = 0; i < M; i++)
            {
                r[i] = GetDoubleValue(rSplit[i]);
                u[i] = GetDoubleValue(uSplit[i]);
                c[i] = GetDoubleValue(cSplit[i]);
                if (i < M - 1)
                    N[i] = GetDoubleValue(NSplit[i]);
            }

            S2Line line = new S2Line();

            string result=CalcFullTP(r[0], u[0], c[0], r[1], u[1], c[1], N[0], line);


           
            result =result+"\r\n\r\n"+ "line:\t" + "p011:" + line.p011 + "\t" + "p001:" + line.p001 + "\t" + "pn10:" + line.pn10 + "\t" + "pn11:" + line.pn11 + "\r\n";
            return result;
        }


        /************************************************************************/
        /* c是加工能力是加工周期的倒数，r是平均故障发生时间的倒数，u是平均维修时间的倒数，
         * N是缓冲区容量，以单位时间为计量单位，TP是最后一台机器，平均单位时间内生产的产品数量 */
        /************************************************************************/
        public string CalcFullTP(double r1, double u1, double c1, double r2, double u2, double c2, double N,S2Line line)
        {
            if (r1 <= 0 || u1 <= 0 || r2 <= 0 || u2 <= 0 || N < 0 || c1 <= 0 || c2 <= 0)
            {
                string tempresult = "有值小于0\t" + r1.ToString() + "\t" + u1.ToString() + "\t" + c1.ToString() + "\t" + r2.ToString() + "\t" + u2.ToString() + "\t" + c2.ToString() + "\t" + N.ToString() + "\r\n";
                return 0.ToString() ;
            }

            if (c1 == c2)
            {
                 double PR=CalcFullPR(r1 / c1, u1 / c1, r2 / c2, u2 / c2, N,line);

                line.tp1 = line.tp1 * c1;
                return (PR*c1).ToString();
            }
            double E1 = (r2 * (c1 * u1 + c2 * u2) - u2 * (c2 - c1) * (u1 + u2 + r1 + r2)) / (c1 * (c2 - c1) * (u1 + u2));
            double E2 = (-r2 * (c1 * u1 + c2 * u2)) / (c1 * (c2 - c1) * (u1 + u2));
            double E3 = (-r1 * (c1 * u1 + c2 * u2)) / (c2 * (c2 - c1) * (u1 + u2));
            double E4 = (r1 * (c1 * u1 + c2 * u2) + u1 * (c2 - c1) * (u1 + u2 + r1 + r2)) / (c2 * (c2 - c1) * (u1 + u2));


            double K1 = (E1 + E4) / 2 + 0.5 * Math.Sqrt(Math.Pow((E1 - E4), 2) + 4 * E2 * E3);
            double K2 = (E1 + E4) / 2 - 0.5 * Math.Sqrt(Math.Pow((E1 - E4), 2) + 4 * E2 * E3);


            double e1 = u1 / (u1 + r1);
            double e2 = u2 / (u2 + r2);
            double G0 = Math.Sqrt(Math.Pow((c1 * (u1 + u2 + r2) - c2 * (u1 + u2 + r1)), 2) + 4 * c1 * c2 * r1 * r2);
            double G1 = 0;
            if (c1 < c2)
            {
                G1 = u1 * G0 * (G0 + c1 * (u1 + u2 + r2) - c2 * (u1 + u2 + r1));
            }
            else
            {
                G1 = u2 * G0 * (G0 + c1 * (u1 + u2 + r2) - c2 * (u1 + u2 + r1));
            }
            ////////////////////////////////////////
            double G2 = 0;
            if (c1 < c2)
            {
                G2 = u2 * r1 * c2 * ((c1 - c2) * (u1 - u2) - (c2 * r1 + c1 * r2) - G0);
            }
            else
            {
                G2 = u1 * r2 * c1 * ((c1 - c2) * (u1 - u2) - (c2 * r1 + c1 * r2) + G0);
            }
            //////////////////////////////////////////////
            double G3 = 0;
            if (c1 < c2)
            {
                G3 = (e2 * (c2 - c1 * e1) * G1 + c1 * e1 * (1 - e2) * G2) / (c1 * e1 * (e2 - 1));
            }
            else
            {
                G3 = (e1 * (c1 - c2 * e2) * G1 + c2 * e2 * (1 - e1) * G2) / (c2 * e2 * (e1 - 1));
            }
            ///////////////////////////////////////////////////////
            double G4 = 0;
            if (c1 < c2) G4 = c2 * e2 * G1; else G4 = c1 * e1 * G1;

            double G5 = 0;
            if (c1 < c2) G5 = c1 * e1 * G2; else G5 = c2 * e2 * G2;

            double G6 = 0;
            if (c1 < c2) G6 = c1 * e1 * G3; else G6 = c2 * e2 * G3;

            ////////////////////////////////////////////////////////////
            double TP = 0;
            if (c1 < c2)
            {
                TP = (G4 + G5 * Math.Exp(-K2 * N) + G6 * Math.Exp(-K1 * N)) / (G1 + G2 * Math.Exp(-K2 * N) + G3 * Math.Exp(-K1 * N));
            }
            else
            {
                TP = (G4 + G5 * Math.Exp(K2 * N) + G6 * Math.Exp(K1 * N)) / (G1 + G2 * Math.Exp(K2 * N) + G3 * Math.Exp(K1 * N));

            }

            double F1;
            if (c1 < c2)
            {
                F1 = (c2 * (u1 + u2 + r1) * (E1 - E4 - K1 + K2) + 2 * r1 * c1 * E2) / (c2 * (u1 + u2 + r1) * (E1 - E4 + K1 - K2) + 2 * r1 * c1 * E2);
            }
            else
            {
               // F1 = (c2 * r2 * (E1 - E4 - K1 + K2) + 2 * (u1 + u2 + r2) * c1 * E2) / (c2 * r2 * (E1 - E4 - K1 - K2) + 2 * (u1 + u2 + r2) * c1 * E2);
                F1 = (c2 * r2 * (E1 - E4 - K1 + K2) + 2 * (u1 + u2 + r2) * c1 * E2) / (c2 * r2 * (E1 - E4 + K1 - K2) + 2 * (u1 + u2 + r2) * c1 * E2);
            }
            double F2 = E1 - E4 - (K1 - K2);
            double F3 = E1 - E4 + (K1 - K2);
            double F4 = r1 / (u1 + u2) + c2 / (c2 - c1);
            double F5 = r2 / (u1 + u2) - c1 / (c2 - c1);
            ///////////////////
            double F6 = 0;
            if (c1 < c2)
            {
                F6 = (2 * E2 * c1 * (r1 + u1) * (r2 + u1 + u2) * (1 - F1 * Math.Exp((K1 - K2) * N)) + c2 * r2 * (r2 + u2) * (-F2 + F1 * F3 * Math.Exp((K1 - K2) * N))) / (2 * E2 * r2 * u1 * (r1 + r2 + u1 + u2));
            }
            else
            {
                F6 = (c2 * (F1 * F3 - F2)) / (2 * u1 * E2);
            }
            ////////////////////////////
            double F7 = 0;
            if (c1 < c2)
            {
                F7 = c1 * (1 - F1) * Math.Exp(K1 * N) / u2;
            }
            else
            {
              //  F7 = (Math.Exp(K1 * N) * (c2 * (r2 + u2) * (r1 + u1 + u2) * (-F2 + F1 * F3 * Math.Exp((K2 - K1) * N)) + 2 * E2 * c1 * r1 * (r1 + u1) * (1 - F1 * Math.Exp((K2 - K1) * N)))) / (2 * E2 * r1 * u2 * (r1 + r2 + u1 + u2));
                F7 = (Math.Exp(K1 * N) * (c2 * (r2 + u2) * (r1 + u1 + u2) * (-F2 + F1 * F3 * Math.Exp((-K2 + K1) * N)) + 2 * E2 * c1 * r1 * (r1 + u1) * (1 - F1 * Math.Exp((-K2 + K1) * N)))) / (2 * E2 * r1 * u2 * (r1 + r2 + u1 + u2));
            }
            ///////////////////////////////////////////////////////
            double F8 = 0;
            if (c1 < c2)
            {
                F8 = (Math.Exp(K1 * N) - 1) / K1 - ((Math.Exp(K2 * N) - 1) / K2) * F1 * Math.Exp((K1 - K2) * N);
            }
            else
            {
                F8 = (Math.Exp(K1 * N) - 1) / K1 - ((Math.Exp(K2 * N) - 1) / K2) * F1;
            }

            //////////////////////
            double F9 = 0;
            if (c1 < c2)
            {
               F9 = ((1 - Math.Exp(K1 * N)) / K1) * (F2 / (2 * E2)) + ((Math.Exp(K2 * N) - 1) / K2) * (F1 * F3 / (2 * E2)) * Math.Exp(-(K2 - K1) * N);
                //F9 = ((1 - Math.Exp(K1 * N)) / K1) * (F2 / (2 * E2)) + ((Math.Exp(K2 * N) - 1) / K2) * (F1 * F3 / (2 * E2)) * Math.Exp((K2 - K1) * N);
            }
            else
            {
                F9 = ((1 - Math.Exp(K1 * N)) / K1) * (F2 / (2 * E2)) + ((Math.Exp(K2 * N) - 1) / K2) * (F1 * F3 / (2 * E2));
            }

         

            //double C0=1/(F4*F8+F5*F9+F6+F7);
            double C0 = 1 / (F4 * F8 + F5 * F9 + F6 * F7);

            double f010, f001, fn10, fn01;
            f010 = C0 * (1 - F1 * Math.Exp((K1 - K2) * N));
            f001 = C0 * (-F2 / (2 * E2) + F1 * F3 / (2 * E2) * Math.Exp((K1 - K2) * N));
            fn10 = C0 * (Math.Exp(K1 * N) - F1 * Math.Exp((K1 - K2) * N) * Math.Exp(K2 * N));
            fn01 = C0 * (-F2 / (2 * E2) * Math.Exp(K1 * N) + F1 * F3 / (2 * E2) * Math.Exp((K1 - K2) * N) * Math.Exp(K2 * N));

            double p011, p001, pn11, pn10;
            if (c1 < c2)
            {
                p011=(c1*(r2+u1+u2)*f010-c2*r2*f001)/(r2*(r1+u1+r2+u2));
                p001=((u1+u2)*(c1*r1*f010+c2*r2*f001))/(r2*u1*(r1+u1+r2+u2));
                pn11 = 0;
                pn10 = (c1 * fn10 - c2 * fn01) / u2;

            }
            else  
            {
                p011 = 0;
                p001=(-c1*f010+c2*f001)/u1;
                pn11 = (c2 * (r1 + u1 + u2) * fn01 - c1 * r1 * fn10) / (r1 * (r1 + u1 + r2 + u2));
                pn10 = ((u1 + u2) * (c1 * r1 * fn10 + c2 * r2 * fn01)) / (r1 * u2 * (r1 + u1 + r2 + u2));

            }

            line.tp1 = TP;
            line.p001 = p001;
            line.p011 = p011;
            line.pn10 = pn10;
            line.pn11 = pn11;

            string result = "";
            result = result + "\r\n" + "K1\t:" + K1.ToString();
            result = result + "\r\n" + "K2\t:" + K2.ToString();
            result = result + "\r\n" + "E1\t:" + E1.ToString();
            result = result + "\r\n" + "E2\t:" + E2.ToString();
            result = result + "\r\n" + "E3\t:" + E3.ToString();
            result = result + "\r\n" + "E4\t:" + E4.ToString();
            result = result + "\r\n" + "F1\t:" + F1.ToString();
            result = result + "\r\n" + "F2\t:" + F2.ToString();
            result = result + "\r\n" + "F3\t:" + F3.ToString();
            result = result + "\r\n" + "F4\t:" + F4.ToString();
            result = result + "\r\n" + "F5\t:" + F5.ToString();
            result = result + "\r\n" + "F6\t:" + F6.ToString();
            result = result + "\r\n" + "F7\t:" + F7.ToString();
            result = result + "\r\n" + "F8\t:" + F8.ToString();
            result = result + "\r\n" + "F9\t:" + F9.ToString();
            result = result + "\r\n" + "C0\t:" + C0.ToString();
            result = result + "\r\n" + "1C0\t:" + (1/C0).ToString();
            result = result + "\r\n" + "f010\t:" + f010.ToString();
            result = result + "\r\n" + "f001\t:" + f001.ToString();
            result = result + "\r\n" + "fn10\t:" + fn10.ToString();
            result = result + "\r\n" + "fn01\t:" + fn01.ToString();

            result = result + "\r\n" + "p001\t:" + p001.ToString();
            result = result + "\r\n" + "p011\t:" + p011.ToString();
            result = result + "\r\n" + "pn10\t:" + pn10.ToString();
            result = result + "\r\n" + "pn11\t:" + pn11.ToString();

            result = result + "\r\n" + "TP\t:" + TP.ToString();
          



            return result;
        }

        public double CalcFullPR(double r1, double u1, double r2, double u2, double N, S2Line line)
        {
            double PR1, PR2;
            double e1 = u1 / (r1 + u1);
            double e2 = u2 / (r2 + u2);
            PR1 = e2 * (1 - CalcQ(r1, u1, r2, u2, N));
            PR2 = e1 * (1 - CalcQ(r2, u2, r1, u1, N));

            double beta = r1 * u2 - r2 * u1;
            double K;
            if(beta!=0)
            {
                K=(u1+u2+r1+r2)*(r2*u1-r1*u2)/((u1+u2)*(r1+r2));
            }else
            {
                K = 0;
            }
            double D1 = (u1 + u2) / (r1 + r2);
            double D2;
            if(beta!=0)
            {
                D2 = (2 + D1 + 1 / D1) * (Math.Exp(K * N)-1) / K;
            }else
            {
                D2 = (2 + D1 + 1 / D1) * N;
            }

            double D3=((r1+r2+u1+u2)*(r2+u1)+r1*u2-r2*u1)/(r2*u1*(r1+r2+u1+u2));
            double D4 = ((r1 + r2 + u1 + u2) * (r1 + u2) + r2 * u1 - r1 * u2) / (r1 * u2 * (r1 + r2 + u1 + u2)) * Math.Exp(K * N);
            double C0=1/(D2+D3+D4);

            double p011, p001, pn11, pn10;
            p011=(u1+u2)*C0/(r2*(r1+u1+r2+u2));
            p001 = ((r1+r2)*(u1 + u2) * C0) / (r2 * u1*(r1 + u1 + r2 + u2));
            pn11 = ((u1 + u2) * C0) / (r1 * (r1 + u1 + r2 + u2)) * Math.Exp(K * N);
            pn10 = ((r1 + r2) * (u1 + u2) * C0) / (r1 * u2 * (r1 + u1 + r2 + u2)) * Math.Exp(K * N);

            line.tp1 = PR1;
            line.p001 = p001;
            line.p011 = p011;
            line.pn10 = pn10;
            line.pn11 = pn11;

            return PR1;
        }



        public string CalcMyTPStr(string lambda, string stru, string strc, string strN, string strM)
        {
            int M = int.Parse(strM);
            if (M <= 0 || M > 1000) return "机器数量太大";

            double[] r = new double[M];
            double[] u = new double[M];
            double[] c = new double[M];
            double[] N = new double[M - 1];

            string blank = " \t";
            string[] rSplit = lambda.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string[] uSplit = stru.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string[] cSplit = strc.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string[] NSplit = strN.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (rSplit.Length < M || uSplit.Length < M || cSplit.Length < M || NSplit.Length < M - 1) return "数据中机器数量与其他数据不符";

            for (int i = 0; i < M; i++)
            {
                r[i] = GetDoubleValue(rSplit[i]);
                u[i] = GetDoubleValue(uSplit[i]);
                c[i] = GetDoubleValue(cSplit[i]);
                if (i < M - 1)
                    N[i] = GetDoubleValue(NSplit[i]);
            }

            // string result = CalcMulTPStr(r, u, c, N, M);
            S2Line line = new S2Line();
            string result1 = CalcMyTP(r[0], u[0], c[0],r[1], u[1], c[1], N[0],line);
            string result2 = CalcFullTP(r[0], u[0], c[0], r[1], u[1], c[1], N[0], line);
            return result1+"\r\n\r\n"+result2;
        }


         /************************************************************************/
        /* c是加工能力是加工周期的倒数，r是平均故障发生时间的倒数，u是平均维修时间的倒数，
         * N是缓冲区容量，以单位时间为计量单位，TP是最后一台机器，平均单位时间内生产的产品数量 */
        /************************************************************************/
        public string CalcMyTP(double p1, double r1, double t1, double p2, double r2, double t2, double N, S2Line line)
        {
            if (p1 <= 0 || r1 <= 0 || t2 <= 0 || p2 <= 0 || N < 0 || r2 <= 0 || t2 <= 0)
            {
                // string tempresult = "有值小于0\t" + r1.ToString() + "\t" + u1.ToString() + "\t" + c1.ToString() + "\t" + r2.ToString() + "\t" + u2.ToString() + "\t" + c2.ToString() + "\t" + N.ToString() + "\r\n";
                return 0.ToString();
            }

            if (t1 == t2)
            {
                double PR = CalcFullPR(p1 / t1, r1 / t1, p2 / t2, r2 / t2, N, line);

                line.tp1 = line.tp1 * t1;
                return (PR * t1).ToString();
            }
            if(t1>t2)
            {
                return CalcMyTP(p2,r2,t2,p1,r1,t1,N,line);
            }

            double K11 = (p1 + r2) / (-t1) + p2 / (t2 - t1) + (r1 / t1) * p1 / (r1 + r2);
            double K22 = (p2 + r1) / (t2) + p1 / (t2 - t1) + (r2 / -t2) * p2 / (r1 + r2);
            double K12 = (p2 / t1) * t2 / (t1 - t2) + (r1 / t1) * p2 / (r1 + r2);
            double K21 = (p1 / -t2) * (-t1) / (t1 - t2) + (r2 / -t2) * p1 / (r1 + r2);
            double K1 = ((K11 + K22) + Math.Sqrt(K11 * K11 - 2 * K11 * K22 + K22 * K22 + 4 * K12 * K21)) / 2;
            double K2 = ((K11 + K22) - Math.Sqrt(K11 * K11 - 2 * K11 * K22 + K22 * K22 + 4 * K12 * K21)) / 2;
            double K3 = ((K11 - K22) + Math.Sqrt(K11 * K11 - 2 * K11 * K22 + K22 * K22 + 4 * K12 * K21)) / (-2 * K12);
            double K4 = ((K11 - K22) - Math.Sqrt(K11 * K11 - 2 * K11 * K22 + K22 * K22 + 4 * K12 * K21)) / (-2 * K12);
            double D1 = t2 * (r1 + r2 + p1) * K3 * Math.Exp(K2 * N) - p1 * t1 * Math.Exp(K1 * N);
            double D2 = -t2 * (r1 + r2 + p1) * K4 * Math.Exp(K1 * N) + p1 * t1 * Math.Exp(K2 * N);
            double D3 = ((p1 * (t1 - t2) - (r1 + r2) * t2) * K2 * (Math.Exp(K1 * N) - 1) + ((p2 * (t1 - t2) + (r1 + r2) * t1) * K1 * K3 * (Math.Exp(K2 * N) - 1))) / ((r1 + r2) * (t1 - t2) * K1 * K2) + (K3 * (r2 + p2) * p2 * t2 + (r1 + r2 + p2) * t1 * (p1 + r1)) / (p2 * r1 * (r1 + r2 + p1 + p2)) + (t1 * Math.Exp(K1 * N)) / (r2);

            double D4 = ((p1 * (t1 - t2) - (r1 + r2) * t2) * K1 * (Math.Exp(K2 * N) - 1) + ((p2 * (t1 - t2) + (r1 + r2) * t1) * K2 * K4 * (Math.Exp(K1 * N) - 1))) / ((r1 + r2) * (t1 - t2) * K1 * K2) + (K4 * (r2 + p2) * p2 * t2 + (r1 + r2 + p2) * t1 * (p1 + r1)) / (p2 * r1 * (r1 + r2 + p1 + p2)) + (t1 * Math.Exp(K2 * N)) / (r2);


          //  double D3 = ((p1 * (t1 - t2) - (r1 + r2) * t2) * K2 * (Math.Exp(K1 * N) - 1) + ((p2 * (t1 - t2) + (r1 + r2) * t1) * K1 * K3 * (Math.Exp(K2 * N) - 1))) / ((r1 + r2) * (t1 - t2) * K1 * K2) + (K3 * (r2 + p2) * p2 * t2 + (r1 + r2 + p2) * t1 * (p1 + r1)) / (p2 * r1 * (r1 + r2 + p1 + p2)) + (t1 * Math.Exp(K1 * N)-t2*K3*Math.Exp(K2*N))*(r1+r2+p1) / ((r1+r2)*r2);

          //  double D4 = ((p1 * (t1 - t2) - (r1 + r2) * t2) * K1 * (Math.Exp(K2 * N) - 1) + ((p2 * (t1 - t2) + (r1 + r2) * t1) * K2 * K4 * (Math.Exp(K1 * N) - 1))) / ((r1 + r2) * (t1 - t2) * K1 * K2) + (K4 * (r2 + p2) * p2 * t2 + (r1 + r2 + p2) * t1 * (p1 + r1)) / (p2 * r1 * (r1 + r2 + p1 + p2)) + (t1 * Math.Exp(K2 * N) - t2 * K4 * Math.Exp(K1 * N)) * (r1 + r2 + p1) / ((r1 + r2) * r2);



            double C1 = D2 / (D2 * D3 + D1 * D4);
            double C2 = D1 / (D2 * D3 + D1 * D4);

            double TP = (t2 * t1 / (t1 - t2)) * ((((C2 * K4 - C1) / K1) * (Math.Exp(K1 * N) - 1)) + (((C1 * K3 - C2) / K2) * (Math.Exp(K2 * N) - 1))) + t1 * ((-t2 * p2 * (C2 * K4 + C1 * K3) + (r1 + r2 + p2) * t1 * (C1 + C2)) / (p2 * (r1 + r2 + p1 + p2)));


            double p011, p001, pn11, pn10;
            p001=((r1+r2)*(p1*t1*(C1+C2)+p2*t2*(C1*K3+C2*K4)))/(p2*r1*(r1+r2+p1+p2));

            p011 = ( ((r1+r2+p2) * t1 * (C1 + C2) - p2 * t2 * (C1 * K3 + C2 * K4))) / (p2  * (r1 + r2 + p1 + p2));

            pn11 = 0;
            pn10 = (-1 / r2) * ((t2 * C2 * K4 - t1 * C1)  * Math.Exp(K1 * N) + (t2 * C1 * K3- t1 * C2)  * Math.Exp(K1 * N));


            double fh10 = C1 * 1 / K1 * (Math.Exp(K1 * N) - 1) + C2 * 1 / K2 * (Math.Exp(K2 * N) - 1);
            double fh01 = C1 * K3 * 1 / K2 * (Math.Exp(K2 * N) - 1) + C2 * K4 * 1 / K1 * (Math.Exp(K1 * N) - 1);
            double fh00 = p1 / (r1 + r2) * fh10 + p2 / (r1 + r2) * fh01;
            double fh11 = t2 / (t1 - t2) * fh01 - t1 / (t1 - t2) * fh10;

            double p010 = 0;
            double p000 = (p2) / (r1 + r2) * p001;
            double pn00 = p1 / (r1 + r2) * pn10;
            double pn01 = 0;
            double pn00ratio=t2/r2*( C1 * K3  * (Math.Exp(K2 * N)) + C2 * K4  * (Math.Exp(K1 * N)))-pn00;
            double cdratio = C1 * D3 + C2 * D4;

          
            double aaa = p000 + p001 + p010 + p011;
            double bbb = pn00 + pn01 + pn10 + pn11;
            double ccc = fh01 + fh00 + fh10 + fh11;
            double ddd = aaa + bbb + ccc;


            line.tp1 = TP;
            line.p001 = p001;
            line.p011 = p011;
            line.pn10 = pn10;
            line.pn11 = pn11;

            


            string result = "";
            result = result + "\r\n" + "K11\t:" + K11.ToString();
            result = result + "\r\n" + "K22\t:" + K22.ToString();
            result = result + "\r\n" + "K12\t:" + K12.ToString();
            result = result + "\r\n" + "K21\t:" + K21.ToString();
            result = result + "\r\n" + "K1\t:" + K1.ToString();
            result = result + "\r\n" + "K2\t:" + K2.ToString();
            result = result + "\r\n" + "K3\t:" + K3.ToString();
            result = result + "\r\n" + "K4\t:" + K4.ToString();
            result = result + "\r\n" + "D1\t:" + D1.ToString();
            result = result + "\r\n" + "D2\t:" + D2.ToString();
            result = result + "\r\n" + "D3\t:" + D3.ToString();
            result = result + "\r\n" + "D4\t:" + D4.ToString();
            result = result + "\r\n" + "C1\t:" + C1.ToString();
            result = result + "\r\n" + "C2\t:" + C2.ToString();
            result = result + "\r\n" + "TP\t:" + TP.ToString();

            result = result + "\r\n" + "p001\t:" + p001.ToString();
            result = result + "\r\n" + "p000\t:" + p000.ToString();
            result = result + "\r\n" + "p011\t:" + p011.ToString();
            result = result + "\r\n" + "p010\t:" + p010.ToString();
            result = result + "\r\n" + "aaa\t:" + aaa.ToString();


            result = result + "\r\n" + "pn00\t:" + pn00.ToString();
            result = result + "\r\n" + "pn01\t:" + pn01.ToString();
            result = result + "\r\n" + "pn10\t:" + pn10.ToString();
            result = result + "\r\n" + "pn11\t:" + pn11.ToString();

            result = result + "\r\n" + "bbb\t:" + bbb.ToString();

            result = result + "\r\n" + "fh01\t:" + fh01.ToString();
            result = result + "\r\n" + "fh00\t:" + fh00.ToString();
            result = result + "\r\n" + "fh11\t:" + fh11.ToString();
            result = result + "\r\n" + "fh10\t:" + fh10.ToString();

            result = result + "\r\n" + "ccc\t:" +ccc.ToString();
            result = result + "\r\n" + "ddd\t:" + ddd.ToString();
            result = result + "\r\n" + "pn00ratio\t:" + pn00ratio.ToString();
            result = result + "\r\n" + "cdratio\t:" + cdratio.ToString();


           
          


            return result;
        }
        //////
       
    }
}
