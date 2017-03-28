using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace PSE.AHP
{
   
    class Attribute
    {
       public double value;
       public double xmin;
       public double xmax;
       public double w;

       public Attribute()
       {
        
       }

       public Attribute(double xmin,double xmax)
       {
           this.xmin = xmin;
           this.xmax = xmax;
       }

    }
    class Machine
    {
       public ArrayList X;/////属性数组
       public double u;//////////
    }
    class MachineArray
    {
        public ArrayList M;/////机器备选数组
        public MachineArray()
        {
            M = new ArrayList();
         }
    }

    class MachineLine
    {
        public ArrayList ALLMachines;
        public Random random=new Random();
        public int m_XNum;
        public int m_MNum;


        public string LogToResult()
        {
            string result="序号";
            for (int i = 0; i < m_XNum;i++ )
            {
                result = result + "\t参数" + (i + 1).ToString("F0");
            }
            

                for (int i = 0; i < ALLMachines.Count; i++)
                {
                    MachineArray ma = (MachineArray)ALLMachines[i];
                    
                    for (int j = 0; j < ma.M.Count; j++)
                    {
                        Machine mm = (Machine)((ma.M)[j]);
                        result = result +"\r\n"+ (i + 1).ToString("F0") + (j + 1).ToString("F0"); 
                        for(int k=0;k<m_XNum;k++)
                        {
                            Attribute attr = (Attribute)mm.X[k];
                            result = result + "\t" + attr.value.ToString("F2");
                        }
                     
                    }
                    result = result + "\r\n";
                }
            return result;
        }

        public ArrayList changeXWeight(ArrayList X,double w)
        {
            ArrayList Y=new ArrayList();
            double xmin;
            double xmax;
            double value;
            for(int i=0;i<X.Count;i++)
            {
                Attribute attr = new Attribute();
             
             
                xmin=w*((Attribute)X[i]).xmin;
                xmax=w*((Attribute)X[i]).xmax;
                attr.w = ((Attribute)X[i]).w;
                value = xmin + (xmax - xmin) * random.NextDouble();
                attr.xmin=xmin;
                attr.xmax=xmax;
                attr.value =value;
                
                Y.Add(attr);
            }
            return Y;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public double [] InitWeight(int num)
        {
            double [] w=new double[num];
            double total=0;
            for(int i=0;i<num;i++)
            {
                w[i]=random.NextDouble();
                total=total+w[i];
            }
            for (int i = 0; i < num; i++)
            {
                w[i] = w[i] / total;
            }
            return w;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="numM"></param>
        /// <param name="numX"></param>
        /// <param name="numLine"></param>
        /// <returns></returns>
        public int Init(int numM,int numX,int numLine)
        {
            m_XNum = numX;
            m_MNum = numM;
            ALLMachines = new ArrayList();
            ArrayList X=new ArrayList();
            double xmin;
          

            double[] w = InitWeight(numX);
            for (int i = 0; i < numX;i++ )
            {
                xmin = random.NextDouble()*10000;
                Attribute arr = new Attribute(xmin,xmin*3);
                arr.w=w[i];
                X.Add(arr);
            }


            double[] mw = InitWeight(numM);
            for (int i = 0; i < numM;i++ )
            {
                MachineArray ma = new MachineArray();
                double n=1+(numLine-1)*random.NextDouble();
                for(int j=0;j<n;j++)
                {
                    Machine mm = new Machine();
                    mm.X = changeXWeight(X, mw[i]);
                    mm.u = mw[i];
                    ma.M.Add(mm);
                }
                ALLMachines.Add(ma);
            }
                return 1;
        }

    }
}
