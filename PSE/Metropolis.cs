using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSE
{
    class Metropolis
    {
        public ProductLineCalc plc;
      
        int DIM;//维数
        double T;//当前温度
        double TMax;//最高温度
        double TMin;//最低温度
        double maxCool;//冷却次数
        double KBeta;//搜索次数
        int maxS;//最大迭代次数
        int xdelta;///领域大小

        double xmin;//位置下限
        double xmax;//位置上限


        public double[] gbestx;//全局最优位置
        public double gbestf=double.MinValue;//全局最优适应度
        public double[] bestx;//全局最优位置
        public double bestf;//全局最优适应度

        public double[] newx;//搜索最优位置

        static Random rand = new Random();//用于生成随机数

        public Metropolis()
         {
             this.DIM = 9;//维数
             this.TMax = 6000;//最高温度
             this.TMin = 2000;//最低温度
             this.maxCool = 20;//冷却次数
             this.KBeta = 30;//搜索次数
             this.maxS = 2000;//最大迭代次数
             this.xdelta = 32;//最大迭代次数
         }


        public Metropolis(int DIM, double TMax, double TMin, double maxcool, double KBeta, int xdelta, int maxS, double xmin, double xmax)
         {
  
             this.DIM = DIM;//维数
             this.TMax = TMax;//最高温度
             this.TMin = TMin;//最低温度
             this.maxCool = maxcool;//冷却次数
             this.KBeta = KBeta;//搜索次数
             this.xdelta = xdelta;/////领域大小
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
                value = xmax - ((value - xmax) % (xmax - xmin));
            }
            if (value < xmin)
            {
                value = xmin + ((xmin - value) % (xmax - xmin));
            }

            return value;
        }


        public int Init()
        {
            gbestx = new double[DIM];//全局最优位置
            bestx = new double[DIM];//全局最优位置
            newx = new double[DIM];//全局最优位置

            return 1;
        }

        public int Search()
        {
            FindNeighborhood(xdelta);
            double newf = f1(newx);
            if(newf>bestf)
            {
               newx.CopyTo(bestx, 0);
               
                bestf = newf;
                if (newf > gbestf)
                {
                    newx.CopyTo(gbestx, 0);
                    gbestf = newf;
                }
            }else
            {
                double ra = rand.NextDouble();
                double temp = Math.Exp((newf - bestf) / T);
                if(ra<=temp)
                {
                    newx.CopyTo(bestx, 0);
                    //bestx = newx;
                    bestf = newf;
                }
                
            }

            return 1;
        }


        public int RandInit()
        {
            double totalN = plc.m_TotalN;
            double temp1 = totalN - DIM;
            double temp = temp1;
             temp = temp1;
            for (int j = 0; j < DIM; j++)
            {
                if (j == DIM - 1)
                {
                    bestx[j] = temp + 1;
                    continue;
                }
                bestx[j] = (int)(temp * rand.NextDouble()) + 1;
                temp = temp - (bestx[j] - 1);
            } 
                bestf = f1(bestx);
           
            return 1;
        }


        public double [] FindNeighborhood(double delta)
        {
            
            bestx.CopyTo(newx, 0);

            int left, right;
            left = 0 + rand.Next(DIM);
            int status = -1;

            for (int j = 0; j < DIM;j++ )
            {
                left = (left + j) % DIM;
               if( bestx[left] - delta>=1)
               {
                   status = 1; break;
               }
            }
            if (status == -1)
            {
                delta = (int)(delta/2);
                if (delta < 1) delta = 1;
                return FindNeighborhood(delta);
            } 

            right = (left + 1 + rand.Next(DIM - 1)) % DIM;

            newx[left] = newx[left] - delta;
            newx[right] = newx[right] + delta;

            return newx;

        }



        public int SearchEx(double delta)
        {

            int left, right;
            left = 0 + rand.Next(DIM);
            int status = -1;

            for (int j = 0; j < DIM; j++)
            {
                left = (left + j) % DIM;
                if (bestx[left] - delta >= 1)
                {
                    status = 1; break;
                }
            }
            if (status == -1)
            {
                delta = (int)(delta / 2);
                if (delta < 1) delta = 1;
                return SearchEx(delta);
            }

            right = (left + 1 + rand.Next(DIM - 1)) % DIM;

            bestx[left] = bestx[left] - delta;
            bestx[right] = bestx[right] + delta;

            double newf = f1(bestx);
            if (newf > bestf)
            {
                bestf = newf;
                if (newf > gbestf)
                {
                    bestx.CopyTo(gbestx, 0);
                    gbestf = newf;
                }
            }
            else
            {
                double ra = rand.NextDouble();
                double temp = Math.Exp((newf - bestf) / T);
                if (ra <= temp)
                {
                    bestf = newf;
                }else
                {
                    bestx[left] = bestx[left] + delta;
                    bestx[right] = bestx[right] - delta;
                }

            }

            return 1;
        }


        public double RunOnce()
        {
            Init();
            int oldxdelta = xdelta;
            double drate;
      
            for (int s = 0; s < maxS; s++)
            {
               // xdelta = oldxdelta;
                RandInit();
                for( int i=0;i<maxCool;i++)
                {
                    T=TMax-(TMax-TMin)*i/(maxCool-1);


                    drate = oldxdelta- (oldxdelta - 0) * i / (maxCool - 1);

                    xdelta = (int)(drate);

                    if (xdelta < 1) xdelta = 1;
                    for(int k=0;k<KBeta;k++)
                    {
                        //Search();
                        SearchEx(xdelta);
                        if(T==TMin)
                        {
                           // Search();
                            SearchEx(xdelta);
                        }
                    }
                }
                if(bestf>gbestf)
                {
                    bestx.CopyTo(gbestx, 0);
                    gbestf = bestf;
                }
               
            }
           
            double best = f1(gbestx);
            return best;
        }


        public string RunToStr()
        {
            RunOnce();

            string result = "";
            result = result + "维数D：" + DIM.ToString() + "\r\n";
            result = result + "当前温度：" + T.ToString() + "\r\n";
            result = result + "最高温度：" + TMax.ToString() + "\r\n";
            result = result + "最低温度：" + TMin.ToString() + "\r\n";
            result = result + "冷却次数：" + maxCool.ToString() + "\r\n";
            result = result + "搜索次数：" + KBeta.ToString() + "\r\n";
            result = result + "最大迭代次数：" + maxS.ToString() + "\r\n";
            result = result + "邻域大小：" + xdelta.ToString() + "\r\n";

            result = result + "最佳适应值：" + gbestf.ToString() + "\r\n";

            result = result + "最佳粒子：" + "\r\n";


            result = result + plc.CalcMulTPStr() + "\r\n";

            return result;


        }



    }
}
