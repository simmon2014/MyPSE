using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace PSE
{
    class QuantumPSO
    {
        public ProductLineCalc plc;
        int NUM;//粒子数
        int DIM;//维数
        double beta;//惯性权重
        double beta1;//惯性权重最小值
        double beta2;//惯性权重最大值
        int maxS;//最大迭代次数

        double xmin;//位置下限
        double xmax;//位置上限
        double vmin;//速度下限
        double vmax;//速度上限



        public double[] abestx;//个体最优位置平均数
        public double[] gbestx;//全局最优位置
        public double gbestf;//全局最优适应度
        particle[] swarm;//定义粒子群
        static Random rand = new Random();//用于生成随机数

        public string m_Result = "";
        public string gbestxStr = "";





        public QuantumPSO()
        {
            NUM = 40;//粒子数
            DIM = 10;//维数
            beta = 0.8;//惯性权重
            maxS = 5000;//最大迭代次数
            xmin = -10.0;//位置下限
            xmax = 10.0;//位置上限
            vmin = xmin;
            vmax = xmax;

        }



        public QuantumPSO(int num, int dim,  double beta1, double beta2, int maxS, double xmin, double xmax)
        {
            this.NUM = num;//粒子数
            this.DIM = dim;//维数
            this.beta1 = beta1;//beta最小值
            this.beta2 = beta2;//beta最大值
            this.maxS = maxS;//最大迭代次数
            this.xmin = xmin;//位置下限
            this.xmax = xmax;//位置上限
        }




        double changeX(double[] x)
        {
            /////增加约束条件
            int[] intx = new int[DIM];//全局最优位置
            int total = 0;
            for (int j = 0; j < DIM; j++)
            {
                intx[j] = (int)x[j];
                if (intx[j] < 1) intx[j] = 1;
                total = total + intx[j];
            }

            int gap = total - (int)(plc.m_TotalN);
            if (gap > 0)
            {
                for (int j = 0; j < DIM; j++)
                {
                    if (intx[j] - gap >= 1)
                    {
                        intx[j] = intx[j] - gap;
                        break;
                    }
                    else if (intx[j] - 1 >= 1)
                    {
                        gap = gap - (intx[j] - 1);
                        intx[j] = 1;
                        continue;
                    }
                }
            }


            for (int j = 0; j < DIM; j++)
            {
                x[j] = intx[j];
            }



            return 1;
        }
        /// <summary>
        /// /适应度函数
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        double f1(double[] x)
        {
            changeX(x);
            return plc.ObjectFuncEx(x);
        }

        double x1(double value)
        {
            if (value > xmax)
            {
                // value = xmin + (( value-xmax) % (xmax - xmin));

                value = xmax - ((value - xmax) % (xmax - xmin));


            }
            if (value < xmin)
            {
                //value = xmax - ((xmin - value) % (xmax - xmin));

                value = xmin + ((xmin - value) % (xmax - xmin));

            }


            return value;
        }

        double v1(double value)
        {
            // if (value >= vmax) return vmax;
            // if (value <= vmin) return vmin;

            return value;
        }

        public int InitSwarm()
        {
            abestx = new double[DIM];//个体最优位置平均值
            gbestx = new double[DIM];//全局最优位置
            //gbestf;//全局最优适应度
            swarm = new particle[NUM];//定义粒子群


            double totalN = plc.m_TotalN;

            double temp,temp1 = totalN - DIM;
            for (int i = 0; i < NUM; i++)
            {//初始化粒子群
                temp = temp1;
                particle p1 = new particle(DIM);
                for (int j = 0; j < DIM; j++)
                {
                    if (j == DIM - 1)
                    {
                        p1.x[j] = temp;
                        continue;
                    }
                    p1.x[j] = (int)(temp * rand.NextDouble()) + 1;
                    temp = temp - (p1.x[j] - 1);
                }
                p1.f = f1(p1.x);
                p1.bestf = p1.f;
                swarm[i] = p1;
                if (gbestf < p1.f)
                {
                    gbestf = p1.f;
                    p1.x.CopyTo(gbestx, 0);
                }
            }

            return 1;
        }
        public int UpdateSwarm()
        {
          
            for (int j = 0; j < DIM; j++)
            {
                for (int i = 0; i < NUM; i++)
                {
                    abestx[j] = abestx[j] + swarm[i].bestx[j]/NUM;
                }
            }


            double phai, mu,sign;
            //double v;
            for (int i = 0; i < NUM; i++)
            {
                particle p1 = swarm[i];
                for (int j = 0; j < DIM; j++)//进化方程
                {
                    phai = rand.NextDouble();
                    mu = rand.NextDouble();
                    if (mu <= 0.5) sign = 1; else sign = -1;
                    p1.x[j] = phai * p1.bestx[j] + (1 - phai) * gbestx[j] + sign * beta * (abestx[j] - p1.x[j]) * Math.Log(1 / mu);
                    p1.x[j] = x1(p1.x[j]);
                }
                p1.f = f1(p1.x);
                if (p1.f > p1.bestf)
                {//改变历史最优
                    p1.x.CopyTo(p1.bestx, 0);
                    p1.bestf = p1.f;
                }
                if (p1.f > gbestf)
                {//改变全局最优
                    p1.x.CopyTo(gbestx, 0);
                    for (int j = 0; j < DIM; j++)//把当前全局最优的粒子随机放到另一位置
                        p1.x[j] = rand.NextDouble() * (xmax - xmin) + xmin;
                    gbestf = p1.f;
                }
            }
            return 1;
        }


       

        /// <summary>
        /// 运行一次
        /// </summary>
        /// <returns></returns>
        public double RunOnce()
        {
            InitSwarm();
            for (int t = 0; t < maxS; t++)
            {
                beta = beta1 + (beta2 - beta1) * t / maxS;
                UpdateSwarm();
               
            }

            double best = f1(gbestx);
            return best;
        }

        public string RunToStr()
        {
            RunOnce();

            string result = "";
            result = result + "粒子数N：" + NUM.ToString() + "\r\n";
            result = result + "维数D：" + DIM.ToString() + "\r\n";
            result = result + "惯性权重beta：" + beta.ToString() + "\r\n";
            result = result + "最大迭代次数：" + maxS.ToString() + "\r\n";
            result = result + "粒子位置最小值：" + xmin.ToString() + "\r\n";
            result = result + "粒子位置最大值：" + xmax.ToString() + "\r\n";
            result = result + "最佳适应值：" + gbestf.ToString() + "\r\n";
            result = result + "最佳粒子：" + "\r\n";

            gbestxStr = "";
            for (int i = 0; i < DIM; i++)
            {
                gbestxStr = gbestxStr + "\t" + gbestx[i].ToString("F6");
            }
            return result;


        }



    }



    ///////////////////
    /////////////////////////
    ////////////////////////
     class SecondPSO
    {
         public ProductLineCalc plc;
         int NUM ;//粒子数
         int DIM ;//维数
         double c1 ;//学习因子c1
         double c2 ;//学习因子c22
         double w;//惯性权重
         double w1;//惯性权重最小值
         double w2;//惯性权重最大值
         int maxS;//最大迭代次数

         double xmin;//位置下限
         double xmax;//位置上限
         double vmin;//速度下限
         double vmax;//速度上限




        public double[] gbestx;//全局最优位置
        public double gbestf;//全局最优适应度
         particle[] swarm;//定义粒子群
         static Random rand = new Random();//用于生成随机数

         public string m_Result="";
         public string gbestxStr="";


      
        

        public SecondPSO()
        {
            NUM = 40;//粒子数
            DIM = 10;//维数
            c1 = 1.8;//学习因子
            c2 = 1.8;//学习因子
            w = 0.9;//惯性权重
            maxS = 5000;//最大迭代次数
            xmin = -10.0;//位置下限
            xmax = 10.0;//位置上限
            vmin = xmin;
            vmax = xmax;
            
        }

       

        public SecondPSO(int num,int dim,double c1,double c2,double w1,double w2,int maxS,double xmin,double xmax)
        {
            this.NUM = num;//粒子数
            this.DIM = dim;//维数
            this.c1 = c1;//学习因子
            this.c2 = c2;//学习因子
            this.w1 = w1;//惯性权重最小值
            this.w2 = w2;//惯性权重最大值
            this.maxS = maxS;//最大迭代次数
            this.xmin = xmin;//位置下限
            this.xmax = xmax;//位置上限
            this.vmax=xmax-xmin;
            this.vmin=xmin-xmax;
        }




         double changeX(double[] x)
        {
            /////增加约束条件
            int[] intx = new int[DIM];//全局最优位置
            int total = 0;
            for (int j = 0; j < DIM; j++)
            {
                intx[j] = (int)x[j];
                if (intx[j] < 1) intx[j] = 1;
                total = total + intx[j];
            }

            int gap = total-(int)(plc.m_TotalN);
            if(gap>0)
            {
                for (int j = 0; j < DIM; j++)
                {
                    if (intx[j] - gap >= 1)
                    {
                        intx[j] = intx[j] - gap;
                        break;
                    }
                    else if (intx[j] - 1 >= 1)
                    {
                        gap = gap - (intx[j] - 1);
                        intx[j] = 1;
                        continue;
                    }
                }
            }
           

            for (int j = 0; j < DIM; j++)
            {
                x[j] = intx[j];
            }



            return 1;
        }
        /// <summary>
        /// /适应度函数
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        double f1(double[] x)
        {
             changeX(x);
             return plc.ObjectFuncEx(x);
        }

        double x1(double value)
        {
            if (value > xmax)
            {  
               // value = xmin + (( value-xmax) % (xmax - xmin));

                value = xmax - ((value - xmax) % (xmax - xmin));


            }
            if (value < xmin) 
            {
                //value = xmax - ((xmin - value) % (xmax - xmin));

                value = xmin + ((xmin - value) % (xmax - xmin));
               
            } 


            return value;
        }

        double v1(double value)
        {
            if (value >= vmax) return vmax;
            if (value <= vmin) return vmin;
          
            return value;
        }

        public int InitSwarm()
        {
            gbestx = new double[DIM];//全局最优位置
            //gbestf;//全局最优适应度
            swarm = new particle[NUM];//定义粒子群


            double totalN = plc.m_TotalN;

            double temp1 = totalN - DIM;
            double temp = temp1;
            for (int i = 0; i < NUM; i++)
            {//初始化粒子群
                temp = temp1;
                particle p1 = new particle(DIM);
                for (int j = 0; j < DIM; j++)
                {
                    if(j==DIM-1)
                    {
                        p1.x[j] = temp+1;
                        continue;
                    }
                    p1.x[j] = (int)(temp * rand.NextDouble()) + 1;
                    temp = temp - (p1.x[j] - 1);
                }
                p1.x.CopyTo(p1.v, 0);
                p1.f = f1(p1.x);
                p1.bestf = p1.f;
                swarm[i] = p1;
                if (gbestf < p1.f)
                {
                    gbestf = p1.f;
                    p1.x.CopyTo(gbestx, 0);
                }
            }
          
            return 1;
        }
        public int UpdateSwarm()
        {
            //double v;
            for (int i = 0; i < NUM; i++)
            {
                particle p1 = swarm[i];
                for (int j = 0; j < DIM; j++)//进化方程
                {
                 
                   p1.v[j] = w * p1.v[j] + c1 * rand.NextDouble() * (p1.bestx[j] - p1.x[j])
                          + c2 * rand.NextDouble() * (gbestx[j] - p1.x[j]);
                   p1.v[j] = v1(p1.v[j]);
                   p1.x[j] = p1.x[j] + p1.v[j];
                   p1.x[j] = x1(p1.x[j]);

                }
                p1.f = f1(p1.x);
                if (p1.f > p1.bestf)
                {//改变历史最优
                    p1.x.CopyTo(p1.bestx, 0);
                    p1.bestf = p1.f;
                }
                if (p1.f > gbestf)
                {//改变全局最优
                    p1.x.CopyTo(gbestx, 0);

                    for (int j = 0; j < DIM; j++)//把当前全局最优的粒子随机放到另一位置
                    {
                        p1.x[j] = rand.NextDouble() * (xmax - xmin) + xmin;
                    }
                       
                    gbestf = p1.f;

                

                }
            }
            return 1;
        }


        public int HillClimbSwarm(int S)
        {
            
            double[] N = new double[DIM];

            gbestx.CopyTo(N,0);

            //int S = 10;
            double max = gbestf;
        

            double current;
            // double epsilon = 1E-8;
            int state = 0;

            int maxi = -1, maxj = -1;
            int tempmaxi = -1, tempmaxj = -1;

            for (int t = 0; t < S; t++)
            {
                state = 0;
                for (int i = 0; i < DIM; i++)
                    for (int j = 0; j <DIM; j++)
                    {
                        if (j != i)
                        {
                            if (N[i] == 1) continue;
                            if ((i == maxj) || (j == maxi)) continue;
                            N[i] = N[i] - 1;
                            N[j] = N[j] + 1;
                            current = f1(N); 
                            if (current > max)
                            {
                                N.CopyTo(gbestx, 0);
                                gbestf = current;

                                state = 1;
                           
                                tempmaxi = i;
                                tempmaxj = j;

                            }
                            N[i] = N[i] + 1;
                            N[j] = N[j] - 1;

                        }

                    }
                if (state == 0)
                {
                    return 1;
                }
                else
                {
                    gbestx.CopyTo(N, 0);
                    maxi = tempmaxi;
                    maxj = tempmaxj;
                }

            }
            return 0;
        }


        /// <summary>
        /// 运行一次
        /// </summary>
        /// <returns></returns>
        public double RunOnce()
        {
            InitSwarm();
            for (int t = 0; t < maxS; t++)
            {
                w = w1 + (w2 - w1) * t / maxS;
                UpdateSwarm();
                //if(t%(maxS/100)==0)
                //{
                //    m_Result = m_Result + t.ToString() + "\t" + gbestf.ToString() ;
                //    for(int i=0;i<DIM;i++)
                //    {
                //        m_Result = m_Result + "\t" + gbestx[i].ToString(); 
                //    }
                //    m_Result = m_Result + "\r\n";
                //}
               
            }


           




            double best = f1(gbestx);
            return best;
        }

        public string RunToStr()
        {
            RunOnce();


            string result = "";
            result = result + "粒子数N：" + NUM.ToString() + "\r\n";
            result = result + "维数D：" + DIM.ToString() + "\r\n";
            result = result + "学习因子c1：" + c1.ToString() + "\r\n";
            result = result + "学习因子c2：" + c2.ToString() + "\r\n";
            result = result + "惯性权重w：" + w.ToString() + "\r\n";
            result = result + "惯性权重w1：" + w1.ToString() + "\r\n";
            result = result + "惯性权重w2：" + w2.ToString() + "\r\n";
            result = result + "最大迭代次数：" + maxS.ToString() + "\r\n";
            result = result + "粒子位置最小值：" + xmin.ToString() + "\r\n";
            result = result + "粒子位置最大值：" + xmax.ToString() + "\r\n";
            result = result + "最佳适应值：" + gbestf.ToString() + "\r\n";

            result = result + "最佳粒子：" + "\r\n";


            gbestxStr = "";
            for (int i = 0; i < DIM; i++)
            {
                gbestxStr = gbestxStr + "\t" + gbestx[i].ToString("F6");
            }
            return result;
                
            
        }


       
    }




     ///////////////////thirdPSO
     /////////////////////////
     ////////////////////////
     class ThirdPSO
     {
         public ProductLineCalc plc;
         int NUM;//粒子数
         int DIM;//维数
         double c1;//学习因子c1
         double c2;//学习因子c22
         double w;//惯性权重
         double w1;//惯性权重最小值
         double w2;//惯性权重最大值
         int maxS;//最大迭代次数

         double xmin;//位置下限
         double xmax;//位置上限
         double vmin;//速度下限
         double vmax;//速度上限
         double gamma;///罚函数惩罚系数



         public double[] gbestx;//全局最优位置
         public double gbestf;//全局最优适应度
         particle[] swarm;//定义粒子群
         static Random rand = new Random();//用于生成随机数

         public string m_Result = "";
         public string gbestxStr = "";

       





         public ThirdPSO()
         {
             NUM = 40;//粒子数
             DIM = 10;//维数
             c1 = 1.8;//学习因子
             c2 = 1.8;//学习因子
             w = 0.9;//惯性权重
             maxS = 5000;//最大迭代次数
             xmin = -10.0;//位置下限
             xmax = 10.0;//位置上限
             vmin = xmin;
             vmax = xmax;
             gamma = 0.01;///惩罚系数

         }



         public ThirdPSO(int num, int dim, double c1, double c2, double w1, double w2, int maxS, double xmin, double xmax,double gamma)
         {
             this.NUM = num;//粒子数
             this.DIM = dim;//维数
             this.c1 = c1;//学习因子
             this.c2 = c2;//学习因子
             this.w1 = w1;//惯性权重最小值
             this.w2 = w2;//惯性权重最大值
             this.maxS = maxS;//最大迭代次数
             this.xmin = xmin;//位置下限
             this.xmax = xmax;//位置上限
             this.vmax = xmax - xmin;
             this.vmin = xmin - xmax;
             this.gamma = gamma;
         }




         double changeX(double[] x)
         {
             /////增加约束条件
             int[] intx = new int[DIM];//全局最优位置
             int total = 0;
             for (int j = 0; j < DIM; j++)
             {
                 intx[j] = (int)x[j];
                 if (intx[j] < 1) intx[j] = 1;
                 total = total + intx[j];
             }

             int gap = total - (int)(plc.m_TotalN);
             if(gap<=0)
             {
                 for (int j = 0; j < DIM; j++)
                 {
                     x[j] = intx[j];
                 }
             }else
             {
                 double temp = (int)(plc.m_TotalN) - DIM;
                 for (int j = 0; j < DIM; j++)
                 {
                     if (j == DIM - 1)
                     {
                         x[j] = temp+1;
                         continue;
                     }
                     x[j] = (int)(temp * rand.NextDouble()) + 1;
                     temp = temp - (x[j] - 1);
                 }
             }
            


             return gap;
         }
         /// <summary>
         /// /适应度函数
         /// </summary>
         /// <param name="x"></param>
         /// <returns></returns>
         double f1(double[] x)
         {
             gamma = 0;
             double gap=changeX(x);
             double fit1 = plc.ObjectFuncEx(x);
             double result = fit1 - gamma*gap;

             return result;
         }

         double x1(double value)
         {
             if (value > xmax)
             {
                 // value = xmin + (( value-xmax) % (xmax - xmin));

                 value = xmax - ((value - xmax) % (xmax - xmin));


             }
             if (value < xmin)
             {
                 //value = xmax - ((xmin - value) % (xmax - xmin));

                 value = xmin + ((xmin - value) % (xmax - xmin));

             }


             return value;
         }

         double v1(double value)
         {
             if (value >= vmax) return vmax;
             if (value <= vmin) return vmin;

             return value;
         }

         public int InitSwarm()
         {
             
             gbestx = new double[DIM];//全局最优位置
             //gbestf;//全局最优适应度
             swarm = new particle[NUM];//定义粒子群


             double totalN = plc.m_TotalN;

             double temp,temp1 = totalN - DIM;
          
             for (int i = 0; i < NUM; i++)
             {
            
                 //初始化粒子群
                 particle p1 = new particle(DIM);
                 temp = temp1;
                 for (int j = 0; j < DIM; j++)
                 {
                     if (j == DIM - 1)
                     {
                         p1.x[j] = temp;
                         continue;
                     }
                     p1.x[j] = (int)(temp * rand.NextDouble()) + 1;
                     temp = temp - (p1.x[j] - 1);
                 }
                 p1.x.CopyTo(p1.v, 0);
                 p1.f = f1(p1.x);
                 p1.bestf = p1.f;
                 swarm[i] = p1;
                 if (gbestf < p1.f)
                 {
                     gbestf = p1.f;
                     p1.x.CopyTo(gbestx, 0);
                 }
             }

             return 1;
         }
         public int UpdateSwarm()
         {
             //double v;
             for (int i = 0; i < NUM; i++)
             {
                 particle p1 = swarm[i];
                 for (int j = 0; j < DIM; j++)//进化方程
                 {

                     p1.v[j] = w * p1.v[j] + c1 * rand.NextDouble() * (p1.bestx[j] - p1.x[j])
                            + c2 * rand.NextDouble() * (gbestx[j] - p1.x[j]);
                     p1.v[j] = v1(p1.v[j]);
                     p1.x[j] = p1.x[j] + p1.v[j];
                     p1.x[j] = x1(p1.x[j]);

                 }
                 p1.f = f1(p1.x);
                 if (p1.f > p1.bestf)
                 {//改变历史最优
                     p1.x.CopyTo(p1.bestx, 0);
                     p1.bestf = p1.f;
                 }
                 if (p1.f > gbestf)
                 {//改变全局最优
                     p1.x.CopyTo(gbestx, 0);
                     for (int j = 0; j < DIM; j++)//把当前全局最优的粒子随机放到另一位置
                         p1.x[j] = rand.NextDouble() * (xmax - xmin) + xmin;
                     gbestf = p1.f;

                  

                     //  for (int j = 0; j < DIM; j++)//把当前全局最优的粒子随机放到另一位置
                     // p1.x[j] = gbestx[j];

                 }
             }
             return 1;
         }


          public void SingleUpdateSwarm()
          {
              object obj = (object)this;
              int n = 0;
              n = int.Parse(Thread.CurrentThread.Name);
              particle p1 = swarm[n];
                for (int j = 0; j < DIM; j++)//进化方程
                {

                    p1.v[j] = w * p1.v[j] + c1 * rand.NextDouble() * (p1.bestx[j] - p1.x[j])
                            + c2 * rand.NextDouble() * (gbestx[j] - p1.x[j]);
                    p1.v[j] = v1(p1.v[j]);
                    p1.x[j] = p1.x[j] + p1.v[j];
                    p1.x[j] = x1(p1.x[j]);

                }
                p1.f = f1(p1.x);
                if (p1.f > p1.bestf)
                {//改变历史最优
                    p1.x.CopyTo(p1.bestx, 0);
                    p1.bestf = p1.f;
                }

                      //    try
                      //    {
                      //    Monitor.Enter(obj);

                      //if (p1.f > gbestf)
                      //{
                      //    gbestf = p1.bestf;
                      //    p1.x.CopyTo(gbestx, 0);
                      //}  
                      
                      //    }
                      //    finally
                      //    {
                      //        Monitor.Exit(obj);
                      //    }
              return ;
          }



         public int MultiThreadUpdateSwarm()
         {
             Thread[] Threads = new Thread[NUM];
             for (int i = 0; i < NUM;i++)
             {
                 Threads[i] = new Thread(new ThreadStart(this.SingleUpdateSwarm));
                 Threads[i].Name = i.ToString();
             }
             foreach(Thread t in Threads) t.Start();
             for (int i = 0; i < NUM; i++)
             {
                 Threads[i].Join();
             }
                 return 1;
         }

         public double RunMultiThreads()
         {
             InitSwarm();

             for (int t = 0; t < maxS; t++)
             {
                 w = w1 + (w2 - w1) * t / maxS;
                 MultiThreadUpdateSwarm();
                 for(int i=0;i<NUM;i++)
                 {
                    if (swarm[i].f > gbestf)
                      {
                          gbestf = swarm[i].bestf;
                          swarm[i].x.CopyTo(gbestx, 0);
                      }  
                 }
                
             }

            

             double best = f1(gbestx);
             
             return best;
         }
        

         /// <summary>
         /// 运行一次
         /// </summary>
         /// <returns></returns>
         public double RunOnce()
         {
             InitSwarm();
             for (int t = 0; t < maxS; t++)
             {
                 w = w1 + (w2 - w1) * t / maxS;
                 UpdateSwarm();
               
             }

             double best = f1(gbestx);
             return best;
         }

         public string RunToStr()
         {
             RunOnce();

             string result = "";
             result = result + "粒子数N：" + NUM.ToString() + "\r\n";
             result = result + "维数D：" + DIM.ToString() + "\r\n";
             result = result + "学习因子c1：" + c1.ToString() + "\r\n";
             result = result + "学习因子c2：" + c2.ToString() + "\r\n";
             result = result + "惯性权重w：" + w.ToString() + "\r\n";
             result = result + "惯性权重w1：" + w1.ToString() + "\r\n";
             result = result + "惯性权重w2：" + w2.ToString() + "\r\n";
             result = result + "最大迭代次数：" + maxS.ToString() + "\r\n";
             result = result + "粒子位置最小值：" + xmin.ToString() + "\r\n";
             result = result + "粒子位置最大值：" + xmax.ToString() + "\r\n";
             result = result + "最佳适应值：" + gbestf.ToString() + "\r\n";

          


             //gbestxStr = "";
             //for (int i = 0; i < DIM; i++)
             //{
             //    gbestxStr = gbestxStr + "\t" + gbestx[i].ToString("F6");
             //}

           //  result = result + "最佳粒子：" + gbestxStr+"\r\n";

             result = result  + plc.CalcMulTPStr() + "\r\n";


             return result;


         }


         public string RunMultiThreadsToStr()
         {
             RunMultiThreads();


             string result = "";
             result = result + "粒子数N：" + NUM.ToString() + "\r\n";
             result = result + "维数D：" + DIM.ToString() + "\r\n";
             result = result + "学习因子c1：" + c1.ToString() + "\r\n";
             result = result + "学习因子c2：" + c2.ToString() + "\r\n";
            // result = result + "惯性权重w：" + w.ToString() + "\r\n";
             result = result + "惯性权重w1：" + w1.ToString() + "\r\n";
             result = result + "惯性权重w2：" + w2.ToString() + "\r\n";
             result = result + "最大迭代次数：" + maxS.ToString() + "\r\n";
             result = result + "粒子位置最小值：" + xmin.ToString() + "\r\n";
             result = result + "粒子位置最大值：" + xmax.ToString() + "\r\n";
             result = result + "最佳适应值：" + gbestf.ToString() + "\r\n";

             result = result + "最佳粒子：" + "\r\n";


             gbestxStr = "";
             for (int i = 0; i < DIM; i++)
             {
                 gbestxStr = gbestxStr + "\t" + gbestx[i].ToString("F6");
             }
             return result;


         }



     }



     ///////////////////
     /////////////////////////
     ////////////////////////
     class FourthPSO
     {
         public ProductLineCalc plc;
         int NUM;//粒子数
         int DIM;//维数
         double c1;//学习因子c1
         double c2;//学习因子c22
         double w;//惯性权重
         double w1;//惯性权重最小值
         double w2;//惯性权重最大值
         int maxS;//最大迭代次数

         double xmin;//位置下限
         double xmax;//位置上限
         double vmin;//速度下限
         double vmax;//速度上限




         public double[] gbestx;//全局最优位置
         public double gbestf;//全局最优适应度
         particle[] swarm;//定义粒子群
         static Random rand = new Random();//用于生成随机数

         public string m_Result = "";
         public string gbestxStr = "";

         int beta=1;//
         int maxbeta = 16;





         public FourthPSO()
         {
             NUM = 40;//粒子数
             DIM = 10;//维数
             c1 = 1.8;//学习因子
             c2 = 1.8;//学习因子
             w = 0.9;//惯性权重
             maxS = 5000;//最大迭代次数
             xmin = -10.0;//位置下限
             xmax = 10.0;//位置上限
             vmin = xmin;
             vmax = xmax;

         }



         public FourthPSO(int num, int dim, double c1, double c2, double w1, double w2, int maxS, double xmin, double xmax)
         {
             this.NUM = num;//粒子数
             this.DIM = dim;//维数
             this.c1 = c1;//学习因子
             this.c2 = c2;//学习因子
             this.w1 = w1;//惯性权重最小值
             this.w2 = w2;//惯性权重最大值
             this.maxS = maxS;//最大迭代次数
             this.xmin = xmin;//位置下限
             this.xmax = xmax;//位置上限
             this.vmax = xmax - xmin;
             this.vmin = xmin - xmax;
         }




         double changeX(double[] x)
         {
             /////增加约束条件
             int[] intx = new int[DIM];//全局最优位置
             int total = 0;
             for (int j = 0; j < DIM; j++)
             {
                 intx[j] = (int)x[j];
                 if (intx[j] < 1) intx[j] = 1;
                 total = total + intx[j];
             }

             int gap = total - (int)(plc.m_TotalN);
             if (gap > 0)
             {
                 for (int j = 0; j < DIM; j++)
                 {
                     if (intx[j] - gap >= 1)
                     {
                         intx[j] = intx[j] - gap;
                         break;
                     }
                     else if (intx[j] - 1 >= 1)
                     {
                         gap = gap - (intx[j] - 1);
                         intx[j] = 1;
                         continue;
                     }
                 }
             }


             for (int j = 0; j < DIM; j++)
             {
                 x[j] = intx[j];
             }

             return 1;
         }
         /// <summary>
         /// /适应度函数
         /// </summary>
         /// <param name="x"></param>
         /// <returns></returns>
         double f1(double[] x)
         {
             changeX(x);
             return plc.ObjectFuncEx(x);
         }

         double x1(double value)
         {
             if (value > xmax)
             {
                 // value = xmin + (( value-xmax) % (xmax - xmin));

                 value = xmax - ((value - xmax) % (xmax - xmin));


             }
             if (value < xmin)
             {
                 //value = xmax - ((xmin - value) % (xmax - xmin));

                 value = xmin + ((xmin - value) % (xmax - xmin));

             }


             return value;
         }

         double v1(double value)
         {
             if (value >= vmax) return vmax;
             if (value <= vmin) return vmin;

             return value;
         }

         public int InitSwarm()
         {
             gbestx = new double[DIM];//全局最优位置
             //gbestf;//全局最优适应度
             swarm = new particle[NUM];//定义粒子群


             double totalN = plc.m_TotalN;

             double temp1 = totalN - DIM;
             double temp = temp1;
             for (int i = 0; i < NUM; i++)
             {//初始化粒子群
                 temp = temp1;
                 particle p1 = new particle(DIM);
                 for (int j = 0; j < DIM; j++)
                 {
                     if (j == DIM - 1)
                     {
                         p1.x[j] = temp + 1;
                         continue;
                     }
                     p1.x[j] = (int)(temp * rand.NextDouble()) + 1;
                     temp = temp - (p1.x[j] - 1);
                 }
                 p1.x.CopyTo(p1.v, 0);
                 p1.f = f1(p1.x);
                 p1.bestf = p1.f;
                 swarm[i] = p1;
                 if (gbestf < p1.f)
                 {
                     gbestf = p1.f;
                     p1.x.CopyTo(gbestx, 0);
                 }
             }

             return 1;
         }
         public int UpdateSwarm()
         {
             double r1 ;
             int signv;
             int rj;
             for (int i = 0; i < NUM; i++)
             {
                 particle p1 = swarm[i];
                 for (int j = 0; j < DIM; j++)//进化方程
                 {
                     r1 = rand.NextDouble();
                     if (p1.v[j] > 0) signv = 1;else if (p1.v[j] < 0) signv = -1; else signv = 0;
                     rj = rand.Next(NUM);
                     p1.v[j] = w*Math.Abs(p1.bestf-swarm[rj].bestf)*signv+  r1 * (p1.bestx[j] - p1.x[j])
                            + (1-r1) * (gbestx[j] - p1.x[j]);
                    // p1.v[j] = v1(p1.v[j]);
                    
                     if (p1.v[j] > 0) signv = 1; else if (p1.v[j] < 0) signv = -1; else signv = rand.Next(3)-1;

                     p1.x[j] = p1.x[j] + signv*beta;
                     p1.x[j] = x1(p1.x[j]);

                 }
                 p1.f = f1(p1.x);
                 if (p1.f > p1.bestf)
                 {//改变历史最优
                     p1.x.CopyTo(p1.bestx, 0);
                     p1.bestf = p1.f;
                 }
                 if (p1.f > gbestf)
                 {//改变全局最优
                     p1.x.CopyTo(gbestx, 0);

                     for (int j = 0; j < DIM; j++)//把当前全局最优的粒子随机放到另一位置
                     {
                         p1.x[j] = rand.NextDouble() * (xmax - xmin) + xmin;
                     }

                     gbestf = p1.f;



                 }
             }
             return 1;
         }




         /// <summary>
         /// 运行一次
         /// </summary>
         /// <returns></returns>
         public double RunOnce()
         {
             InitSwarm();
             for (int t = 0; t < maxS; t++)
             {
                 w = w1 + (w2 - w1) * t / maxS;
                 beta = 1 + (maxbeta - 1) *(maxS-t) / maxS;
                 UpdateSwarm();
                 //if(t%(maxS/100)==0)
                 //{
                 //    m_Result = m_Result + t.ToString() + "\t" + gbestf.ToString() ;
                 //    for(int i=0;i<DIM;i++)
                 //    {
                 //        m_Result = m_Result + "\t" + gbestx[i].ToString(); 
                 //    }
                 //    m_Result = m_Result + "\r\n";
                 //}

             }







             double best = f1(gbestx);
             return best;
         }

         public string RunToStr()
         {
             RunOnce();


             string result = "";
             result = result + "粒子数N：" + NUM.ToString() + "\r\n";
             result = result + "维数D：" + DIM.ToString() + "\r\n";
             result = result + "学习因子c1：" + c1.ToString() + "\r\n";
             result = result + "学习因子c2：" + c2.ToString() + "\r\n";
             result = result + "惯性权重w：" + w.ToString() + "\r\n";
             result = result + "惯性权重w1：" + w1.ToString() + "\r\n";
             result = result + "惯性权重w2：" + w2.ToString() + "\r\n";
             result = result + "最大迭代次数：" + maxS.ToString() + "\r\n";
             result = result + "粒子位置最小值：" + xmin.ToString() + "\r\n";
             result = result + "粒子位置最大值：" + xmax.ToString() + "\r\n";
             result = result + "最佳适应值：" + gbestf.ToString() + "\r\n";

             result = result + "最佳粒子：" + "\r\n";


             //gbestxStr = "";
             //for (int i = 0; i < DIM; i++)
             //{
             //    gbestxStr = gbestxStr + "\t" + gbestx[i].ToString("F6");
             //}


             result = result + plc.CalcMulTPStr() + "\r\n";


             return result;


         }



     }



}





