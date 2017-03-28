using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSE
{
    public class Chromosome 
    {
       
        public int DIM = 10;//维度
        public double[] x;//当前位置矢量
        public double f;//当前适应度
        public int totalN=200;///总的缓冲总量约束
        static Random rand = new Random();//用于生成随机数        
        public int maxMutate = 5;//变异最大次数  
        public int maxCrossOver = 3;//交叉最大次数    
     


        public ProductLineCalc plc;

        public Chromosome(int DIM, int totalN, ProductLineCalc pl)
        {
            this.DIM = DIM;
            x = new double[DIM];//当前位置矢量
            this.totalN = totalN;
            this.plc = pl;
        }
     
      public  void Generate()
        {
            double temp = totalN - DIM;
            for (int j = 0; j < DIM; j++)
            {
                if (j == DIM - 1)
                {
                    x[j] = temp + 1;
                    continue;
                }
                x[j] = (int)(temp * rand.NextDouble()) + 1;
                temp = temp - (x[j] - 1);
            }
            Evaluate();
        }

       
      public  Chromosome CreateNew()
        {
            Chromosome newchromo = new Chromosome(this.DIM,this.totalN,this.plc);
            newchromo.Generate();
            return newchromo;
        }


       
      public  Chromosome Clone()
        {
            Chromosome newchromo = new Chromosome(this.DIM, this.totalN, this.plc);
            for (int j = 0; j < DIM; j++)
            {
                newchromo.x[j] = this.x[j];
            }
            newchromo.f = this.f;
            return newchromo;
        }


      public  void Mutate()
        {
          
          //  int left, right;
            // double temp;
            //for (int i = 0; i < maxMutate; i++)
            //{
            //    left = 0 + rand.Next(DIM);
            //    right = (left + 1 + rand.Next(DIM - 1)) % DIM;

            //    temp = x[left];
            //    x[left] = x[right];
            //    x[right] = temp;
            //}

            int left;
            for (int i = 0; i < maxMutate; i++)
            {
                left = 0 + rand.Next(DIM);
                x[left] = rand.Next(totalN - DIM);
            }



            Evaluate();
        }


      public  Chromosome Crossover(Chromosome pair)
        {
            Chromosome newchromo = new Chromosome(this.DIM, this.totalN, this.plc);
            double temp;
            int itemp;
            for (int i = 0; i < maxMutate; i++)
            {
                for (int j = 0; j < DIM; j++)
                {
                    temp = (this.x[j] + pair.x[j]) / 2;
                    itemp = (int)temp;
                    if (temp - itemp < 0.5) temp = itemp;
                    else if (temp - itemp >= 0.5) temp = itemp + 1;
                    newchromo.x[j] = temp;
                }

            }

            newchromo.Evaluate();

            return newchromo;
        }

      public  void Evaluate()
        {
            changeX();
            f=plc.ObjectFuncEx(x);
        }

       public double changeX()
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

    }






    class FirstGE
    {
        public ProductLineCalc plc;
        public int DIM = 10;//维度
        public int totalN = 200;///总的缓冲总量约束
        int NUM;//粒子
        int maxS;//最大迭代次数
        public Chromosome[] population;//定义种群数量
        static Random rand = new Random();//用于生成随机数     
        public double mutaterate = 0.5;////变异率
        public double crossrate = 0.5;////交叉率

        public Chromosome bestchrome;


        public FirstGE(int num,int dim,int maxS,int totalN)
        {
            this.NUM = num;//粒子数
            this.DIM = dim;//维数
            this.maxS = maxS;//最大迭代次数
            this.totalN = totalN;///最大缓冲容量
          
        }


        public Chromosome[] PopClone()
        {
            Chromosome[] newpop = new Chromosome[NUM];
            for (int i = 0; i < NUM; i++)
            {
                newpop[i]=population[i] ;
            }
            return newpop;
        }






        public bool isEnd()
         {
             return true;
         }

        public double Init()
        {
            population = new Chromosome[NUM];//定义人口
            for (int i = 0; i < NUM; i++)
            {//初始化种群
                Chromosome chromo = new Chromosome(DIM, totalN, plc);
                chromo.Generate();
                population[i] = chromo;
            }

            return 0;
        }

        public int findsel(double [] sel,int num,double selvalue)
        {
            
            int low = 0; int high = num- 1;
            int mid;
            int thesel = low;
            if (sel[low] > selvalue)
            { 
                thesel = low;
                return thesel;
            }
           // else if (selvalue > sel[high - 1]) thesel = high;
            while (high - low > 1)
            {
                mid = (low + high) / 2;
                if (sel[mid] < selvalue) low = mid;
                else if (sel[mid] > selvalue)
                { high = mid; thesel = mid; }
                else if (sel[mid] == selvalue)
                {
                    thesel = mid; break;
                }

            }

            return thesel;
        }


        public double Select()
        {
            double maxfit=double.MinValue;
            double minfit = double.MaxValue;

            double[] sel = new double[NUM];
            double totalfit=0;
            int maxi=0,mini=0;

            for (int i = 0; i < NUM; i++)
            {
                if (population[i].f < maxfit)
                {
                    minfit = population[i].f;
                }
            }



            for (int i = 0; i < NUM; i++)
            {
                Chromosome chromo = population[i];
                if (chromo.f > maxfit) 
                {
                    maxfit = chromo.f;
                    maxi=i;
                }
                    
               // totalfit = totalfit + chromo.f-(i+1)*(minfit-(1E-11));

                totalfit = totalfit + Math.Pow((chromo.f /minfit ),2.5);

                sel[i] = totalfit;
            }

            bestchrome = population[maxi];
            Chromosome [] newpop=new Chromosome[NUM];
            int thesel;
           
            for (int i = 0; i < NUM; i++)
            {
                double selvalue = rand.NextDouble() * totalfit;
                thesel = findsel(sel, NUM, selvalue);
                newpop[i]=population[thesel].Clone();
                if(population[thesel].f<minfit)
                {
                    minfit = population[thesel].f;
                    mini = i;

                }
            }

            newpop[mini] = population[maxi].Clone();

            for (int i = 0; i < NUM; i++)
            {
                population[i] = newpop[i];
            }
        
             return 0;
        }
        public double CrossOver()
        {
            Chromosome[] oldpop = this.PopClone();
            int left, right;
            for (int i = 0; i < NUM*crossrate; i++)
            {
                left = 0 + rand.Next(NUM);
                right = (left + 1 + rand.Next(NUM- 1)) % NUM;

                for (int j = 0; j < DIM;j++)
                {
                    double u = rand.NextDouble(); if (u >= 0.5) u = 1 - u;
                    double beta=Math.Pow(2*u,1/(crossrate+1));
                    //double beta = 2*u;

                    population[left].x[j] = 0.5 * ((1 - beta) * oldpop[left].x[j] + (1 + beta) * oldpop[right].x[j]);
                    population[right].x[j] = 0.5 * ((1 + beta) * oldpop[left].x[j] + (1 - beta) * oldpop[right].x[j]);
                }
                population[left].Evaluate();
                population[right].Evaluate();
                   
            }
            return 0;
        }
        public double Mutate()
        {
            for (int i = 0; i < NUM*mutaterate; i++)
            {
                int left = rand.Next(NUM);
                population[left].Mutate();
            }
            return 0;
        }

        public double Recombine()
        {
            int mini = 0;
            double minx = double.MaxValue;
            for (int i = 0; i < NUM ; i++)
            {
              if(population[i].f<minx)
              {
                  minx = population[i].f;
                  mini = i;
              }
            }
            population[mini] = bestchrome;
            return 0;
        }


        public double RunOnce()
        {
            Init();
            for (int t = 0; t < maxS; t++)
            {
                Select();
                CrossOver();
                Mutate();
                Recombine();
            }

             bestchrome.Evaluate();
             double best = bestchrome.f;
            return best;
        }

        public string RunToStr()
        {
            RunOnce();


            string result = "";
            result = result + "人口N：" + NUM.ToString() + "\r\n";
            result = result + "维数D：" + DIM.ToString() + "\r\n";
          
            result = result + "最大迭代次数：" + maxS.ToString() + "\r\n";
           
            result = result + "最佳适应值：" + bestchrome.f.ToString() + "\r\n";

          


            result = result + plc.CalcMulTPStr() + "\r\n";


            return result;


        }


    }
}
