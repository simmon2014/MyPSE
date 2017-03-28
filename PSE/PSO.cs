using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSE
{
    class particle
    {//定义一个粒子
        public int D = 10;
        public double[] x;//当前位置矢量
        public double[] bestx;//历史最优位置
        public double[] v;//当前速度矢量
        public double f;//当前适应度
        public double bestf;//历史最优适应度
        public particle(int D)
        {
            this.D = D;
            x = new double[D];//当前位置矢量
            bestx = new double[D];//历史最优位置
            v = new double[D];//当前速度矢量
        }
    }

    class FirstPSO
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


         double[] gbestx;//全局最优位置
         double gbestf;//全局最优适应度
         particle[] swarm;//定义粒子群
         static Random rand = new Random();//用于生成随机数

       

       
        

        public FirstPSO()
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

       

        public FirstPSO(int num,int dim,double c1,double c2,double w1,double w2,int maxS,double xmin,double xmax)
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
        }


        /// <summary>
        /// /适应度函数
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        double f1(double[] x)
        {//测试函数：超球函数
            //return x.Sum(a => a * a);
            return plc.ObjectFunc(x);
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
            gbestx = new double[DIM];//全局最优位置
            //gbestf;//全局最优适应度
            swarm = new particle[NUM];//定义粒子群


            for (int i = 0; i < DIM; i++)//初始化全局最优
                gbestx[i] = rand.NextDouble() * (xmax - xmin) + xmin;
            gbestf = double.MinValue;
            for (int i = 0; i < NUM; i++)
            {//初始化粒子群
                particle p1 = new particle(DIM);
                for (int j = 0; j < DIM; j++)
                    p1.x[j] = rand.NextDouble() * (xmax - xmin) + xmin;
                p1.f = f1(p1.x);
                //p1.bestf = double.MaxValue;
                p1.bestf = p1.f;
                swarm[i] = p1;
                if(gbestf<p1.f)
                {
                    gbestf = p1.f;
                }
            }
            return 1;
        }
        public int UpdateSwarm()
        {
            double v;
            for (int i = 0; i < NUM; i++)
            {
                particle p1 = swarm[i];
                for (int j = 0; j < DIM; j++)//进化方程
                {
                   v=w*p1.x[j]+c1 * rand.NextDouble() * (p1.bestx[j] - p1.x[j])
                          + c2 * rand.NextDouble() * (gbestx[j] - p1.x[j]);
                   v = v1(v);
                   p1.x[j] += v;
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
                w = w1 + (w2 - w1) * t / maxS;
                UpdateSwarm();
            }

            double best = f1(gbestx);
            return gbestf;
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
            result = result + "最大迭代次数：" + maxS.ToString() + "\r\n";
            result = result + "粒子位置最小值：" + xmin.ToString() + "\r\n";
            result = result + "粒子位置最大值：" + xmax.ToString() + "\r\n";
            result = result + "最佳适应值：" + gbestf.ToString() + "\r\n";

            result = result + "最佳粒子：" + "\r\n";



            //for (int i = 0; i < DIM; i++)
            //{
            //    result = result + "\t" + gbestx[i].ToString("F6");
            //}
            return result;
                
            
        }


       
    }




    class FirstPSOEx
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


      
        

        public FirstPSOEx()
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

       

        public FirstPSOEx(int num,int dim,double c1,double c2,double w1,double w2,int maxS,double xmin,double xmax)
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
           // if (value >= vmax) return vmax;
           // if (value <= vmin) return vmin;
          
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
            {//初始化粒子群
                temp = temp1;
                particle p1 = new particle(DIM);
                for (int j = 0; j < DIM; j++)
                {
                    if(j==DIM-1)
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
            double v;
            for (int i = 0; i < NUM; i++)
            {
                particle p1 = swarm[i];
                for (int j = 0; j < DIM; j++)//进化方程
                {
                   v=w*p1.x[j]+c1 * rand.NextDouble() * (p1.bestx[j] - p1.x[j])
                          + c2 * rand.NextDouble() * (gbestx[j] - p1.x[j]);
                   v = v1(v);
                   p1.x[j] += v;
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

                   // HillClimbSwarm(10);

                  //  for (int j = 0; j < DIM; j++)//把当前全局最优的粒子随机放到另一位置
                       // p1.x[j] = gbestx[j];

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
               // if(t%40==0)
               // m_Result = m_Result + t.ToString() + "\t" + gbestf.ToString() + "\r\n";
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
}


