using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSE
{
    class part
    {//定义一个粒子
        public int D = 9;
        public double[] x;//当前位置矢量
        public double f1;//当前适应度1
        public double f2;//当前

        public part(int D)
        {
            this.D = D;
            x = new double[D];
        }

        public part Clone()
        {
            part newp = new part(D);
            x.CopyTo(newp.x, 0);
            newp.f1 = f1;
            newp.f2 = f2;
            return newp;
        }
    }

    class MOSA
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
        double JBeta=3;//搜索次数
        int maxMONum=100;///最大解集数量

        public  double rbeta1=1000;
        public  double rbeta2=1;
        public List<part>  gbestx=new List<part>();//全局最优位置

        public part bestx;//最优位置
     
        static Random rand = new Random();//用于生成随机数

        public MOSA()
         {
             this.DIM = 9;//维数
             this.TMax = 6000;//最高温度
             this.TMin = 2000;//最低温度
             this.maxCool = 20;//冷却次数
             this.KBeta = 30;//搜索次数
             this.maxS = 2000;//最大迭代次数
             this.xdelta = 32;//最大迭代次数
         }


        public MOSA(int DIM, double TMax, double TMin, double maxcool, double KBeta, int xdelta, int maxS, double xmin, double xmax)
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

            return total;
        }


        
        /// <summary>
        /// /适应度函数
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        double getf(part p)
        {
            double[] x = p.x;
            p.f2= changeX(x);
            p.f1=plc.ObjectFuncEx(x);
            return 1;
        }

        int compare(part a,part b)
        {
           double temp1 = (a.f1 - b.f1);
           double temp2 = (a.f2 - b.f2);
            if(temp1==0&&temp2==0)
            {
                return 2;
            }

           if((temp1>=0)&&(temp2<=0))
           {
               return 1;
           }
           if ((temp1 <= 0) && (temp2 >= 0))
           {
               return -1;
           }

           return 0;
        }

        int AddToGlobal(part a)
        {
            int status;
            List<int> intlist=new List<int>() ;
            for (int i = 0; i < gbestx.Count;i++)
            {
                part b = gbestx[i];
                status = compare(a, b);
                if (status == -1||status==2) return 0;
                else if (status == 0) continue;
                else if(status==1)
                {
                    intlist.Add(i);
                }
            }

            for(int i=intlist.Count-1;i>=0;i--)
            {
                gbestx.RemoveAt(intlist[i]);
            }

            gbestx.Add(a);
            if(gbestx.Count>maxMONum)
            {
                gbestx.RemoveAt(rand.Next(maxMONum));
            }
                return 1;
        }


        double GetDeltaT(part b)
        {
            double temp=rbeta1*(b.f1-bestx.f1)+rbeta2*(bestx.f2-b.f2);
            temp = Math.Exp(temp / T);
            return temp;
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
            bestx = new part(DIM);
            return 1;
        }

        public int Search()
        {
            part newp=FindNeighborhood(xdelta);
            getf(newp);
            

            if(compare(newp,bestx)==1)
            {
                bestx = newp;
                AddToGlobal(newp);
            }else
            {
                double ra = rand.NextDouble();
                double temp = GetDeltaT(newp);
                if(ra<=temp)
                {
                   bestx = newp;
                }
                
            }

            return 1;
        }


        public int RandInit()
        {
            double totalN = plc.m_TotalN;
            totalN=DIM+rand.Next((int)(totalN-DIM));

            double temp1 = totalN - DIM;
            double temp = temp1;
             temp = temp1;
            for (int j = 0; j < DIM; j++)
            {
                if (j == DIM - 1)
                {
                    bestx.x[j] = temp + 1;
                    continue;
                }
                bestx.x[j] = (int)(temp * rand.NextDouble()) + 1;
                temp = temp - (bestx.x[j] - 1);
            } 
               getf(bestx);
           
            return 1;
        }

         public int RandReInit()
        {
          if(gbestx.Count==0) return RandInit();
           int r=rand.Next(gbestx.Count);
           bestx=gbestx[r];
           
            return 1;
        }





        public part FindNeighborhood(double delta)
        {

            part newp = bestx.Clone();

            //int left, right;
            //left = 0 + rand.Next(DIM);
            //int status = -1;

            //for (int j = 0; j < DIM;j++ )
            //{
            //    left = (left + j) % DIM;
            //   if( bestx.x[left] - delta>=1)
            //   {
            //       status = 1; break;
            //   }
            //}
            //if (status == -1)
            //{
            //    delta = (int)(delta/2);
            //    if (delta < 1) delta = 1;
            //    return FindNeighborhood(delta);
            //} 

            //right = (left + 1 + rand.Next(DIM - 1)) % DIM;

            //newp.x[left] = newp.x[left] - delta;
            //newp.x[right] = newp.x[right] + delta;

            double ss;
            for (int j = 0; j < DIM; j++)
            {
               // ss = rand.Next(3)-1;
                ss = rand.NextDouble() * 2 - 1;
                newp.x[j] = newp.x[j] + ss * delta;
            }


            return newp;

        }



       

        public double RunOnce()
        {
            Init();
            int oldxdelta = xdelta;
            double drate;
      
            for (int s = 0; s < maxS; s++)
            {
               // xdelta = oldxdelta;
                //RandInit();
                for( int i=0;i<maxCool;i++)
                {
                    T=TMax-(TMax-TMin)*i/(maxCool-1);


                    drate = oldxdelta- (oldxdelta - 0) * i / (maxCool - 1);

                    xdelta = (int)(drate);

                    if (xdelta < 1) xdelta = 1;

                    for(int j=0;j<JBeta;j++)
                    {
                         RandInit();
                     for(int k=0;k<KBeta;k++)
                    {
                        Search();
                        if(T==TMin)
                        {
                            Search();
                        }
                    }
                    }
                   
                }
               
               
            }
          
            return 1;
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

          

            result = result + "最佳粒子：" + "\r\n";

            string temp;
            for (int i = 0; i < gbestx.Count;i++ )
            {
                part a = gbestx[i];
                temp="";
                for(int j=0;j<DIM;j++)
                {
                    temp=temp+"\t"+a.x[j];
                }
                result = result + a.f1+"\t"+a.f2+temp+"\r\n";
            }

                return result;


        }
    }
}
