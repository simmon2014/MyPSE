using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.IO;

namespace PSE.DES
{
    /// <summary>
    /// 
    /// </summary>
    class EGSim
    {
        public string m_strResult;
        public Random random ;
        public int mseed;
        public int TotalS;////爬山法迭代次数
        public double batchtime;
        public EventList evList=new EventList();
        public double currentTime=0;
        public int RunningState = 0;
        public int MachineNum=5;
        //input
        public double[] c;
        public int[] b;
        public double[] mttf;
        public double[] mttr;

        //////记录中间过程
        public double m_MaxMTTF=0;
        public double m_MTTFInterval=1000;
        public int m_MTTFNum = 0;
        public double m_OldWip= 0;
        public List<double> m_TPList = new List<double>();//全局最优位置


      


        ////中间变量机器状态变量
        public int[] ms;////ms[i]=0,1,2,3,4,5  0--空闲，1--加工,2--堵塞,3---空闲故障,4---加工故障,5----堵塞故障
        public double[] remain;
       
        //output
        public double[] wip;
        public int[] tp,tp2;

        
       //记录特殊算法
        public double repairmax=0;
        public double w1max = 0;
        public double w1time = 0;
        public double w1ul = 0;
        public int LogW1()
        {
            string str;
            str = currentTime.ToString("F2") + "," + w1max.ToString("F2") +","+ repairmax.ToString("F2") + "\r\n";
            StreamWriter sw = new StreamWriter("w1max.txt", true);
            sw.Write(str);
            sw.Close();
            w1time = currentTime;
            return 1;
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

        public EGSim(string lambda, string stru, string strc, string strN, string strM)
        {
            InitStr(lambda,stru,strc,strN,strM);
        }

        public EGSim(int MachineNum,int seed)
        {
           
            InitSim(MachineNum,seed);
         

        }


        public EGSim(int MachineNum,double []oldc,int [] oldb,int seed )
        {
            InitSim(MachineNum, seed);
           
            Array.Copy(oldc,c,MachineNum);
            Array.Copy(oldb,b,MachineNum);

        }

        public int InitSim(int MachineNum,int seed)
        {
            mseed = seed;
            
             this.MachineNum = MachineNum;
            wip = new double[MachineNum];
            ms = new int[MachineNum];
            b = new int[MachineNum];
            c = new double[MachineNum];
            tp = new int[MachineNum];
            tp2 = new int[MachineNum];
            remain = new double[MachineNum];
            mttf = new double[MachineNum];
            mttr= new double[MachineNum];
            for (int i = 0; i < MachineNum; i++)
            {
                ms[i] = 0;
                wip[i] = 0;
                tp[i] = 0;
                tp2[i] = 0;
                remain[i] = -1;
                mttf[i] = 0;
                mttr[i] = 0;
            }

            return 1;
        }


        public string InitStr(string lambda, string stru, string strc, string strN, string strM)
        {
            int M = int.Parse(strM);
            if (M <= 0 || M > 1000) return "机器数量太多";
           // InitSim(M);
           
            string blank = " \t";
            string[] rSplit = lambda.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string[] uSplit = stru.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string[] cSplit = strc.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string[] NSplit = strN.Split(blank.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (rSplit.Length < M || uSplit.Length < M || cSplit.Length < M || NSplit.Length < M-1 ) return "参数不符合机器";

            for (int i = 0; i < M; i++)
            {
                mttf[i] = 1 / GetDoubleValue(rSplit[i]);
                mttr[i] = 1 / GetDoubleValue(uSplit[i]);
                c[i] = 1 / GetDoubleValue(cSplit[i]);
                if(i<M-1)
                b[i] = int.Parse(NSplit[i]);

                if (mttf[i] > m_MaxMTTF) m_MaxMTTF = mttf[i];
            }

           
            return "成功初始化";
        }


        public string InitArray(double [] rr, double [] uu ,double [] tao, double [] theN, int theM)
        {
            int M = theM;
            if (M <= 0 || M > 1000) return "机器数量太多";
            // InitSim(M);

         
            for (int i = 0; i < M; i++)
            {
                mttf[i] =1/rr[i];
                mttr[i] = 1/uu[i];
                c[i] = 1/tao[i];
                if (i < M - 1)
                    b[i] = (int)theN[i];

                if (mttf[i] > m_MaxMTTF) m_MaxMTTF = mttf[i];
            }


            return "成功初始化";
        }


        public int LogMTTFTP()
        {
            double thetime=(m_MTTFNum+1)*m_MTTFInterval * m_MaxMTTF;
            if(currentTime>=thetime)
            {
                m_MTTFNum++;
                double newWip = wip[MachineNum - 1];
                double aTP = (newWip - m_OldWip) / (m_MTTFInterval * m_MaxMTTF);
                m_TPList.Add(aTP);
                m_OldWip = newWip;
            }
            return 1;
        }



        public string GetResult()
        {
            //////////////输出计算结果
            string[] result = new string[9] { "c", "b","mttf","mttr", "wip","tp","tp2","e","pe"};
            

            string spec = "F2";
            double value,eff;
            for (int i = 0; i < MachineNum; i++)
            {
                int j = 0;
                result[j] = result[j++] + "\t" + c[i].ToString(spec);
                result[j] = result[j++] + "\t" + b[i].ToString();
                result[j] = result[j++] + "\t" + mttf[i].ToString(spec);
                result[j] = result[j++] + "\t" + mttr[i].ToString(spec);
                result[j] = result[j++] + "\t" + wip[i].ToString();
                result[j] = result[j++] + "\t" + tp[i].ToString();
                result[j] = result[j++] + "\t" + tp2[i].ToString();
                if(mttf[i]==0) eff=1;
                else  eff=mttf[i]/(mttf[i]+mttr[i]);
                result[j] = result[j++] + "\t" + eff.ToString(spec);
                value = tp[i] / (currentTime / c[i]);
                result[j] = result[j++] + "\t" + value.ToString(spec);
                value = tp[i] / (currentTime / c[i] * eff);
              //  value = tp[i] / (currentTime / c[i] * eff);
              //  result[j] = result[j++] + "\t" + value.ToString(spec);
                

            }
            string totalResult = "";
            for (int i = 0; i < result.Length; i++)
            {
                totalResult = totalResult + result[i] + "\r\n";
            }

            totalResult = totalResult + "currentTime:"+currentTime.ToString(spec) + "\r\n";

            if(batchtime>0)
            {
                totalResult = totalResult + "PR:" + (wip[MachineNum - 1] / batchtime).ToString() + "\r\n";
            }

            double avgTP=0;
            for (int i = 0; i < m_TPList.Count;i++ )
            {
                int seq = i + 1;
                double seqtime = seq * m_MTTFInterval * m_MaxMTTF;
                totalResult=totalResult+seq.ToString()+":\t"+seqtime.ToString()+"\t"+m_TPList[i].ToString()+"\r\n";
                if(i>1)
                {
                    avgTP = avgTP + m_TPList[i] / (m_TPList.Count - 2);
                }
            }

             totalResult=totalResult+"去除前两个阶段平均值:\t"+avgTP.ToString()+"\r\n";
                m_strResult = totalResult;
            return totalResult;
        }



        public int Reset()
        {
           // StreamWriter sw = new StreamWriter("w1max.txt", false);
           // sw.Write("");
          // sw.Close();

           // repairmax = 0;
          //   w1max = 0;
           //  w1time = 0;
           //  w1ul = 0;

            random = new Random(mseed);

            for (int i = 0; i < MachineNum; i++)
            {
                ms[i] = 0;
                wip[i] = 0;
                tp[i] = 0;
                tp2[i] = 0;
                remain[i] = -1;
            }

            currentTime = 0;
            RunningState = 1;
            evList.Reset();
            evList.AddEvent(0, 0, 1, "start");
            for (int i = 0; i < MachineNum;i++ )
            {
                tryAddBreakDown(i);
            }


             
            m_MTTFNum = 0;
             m_OldWip= 0;
             int count = m_TPList.Count;
             m_TPList.RemoveRange(0,count);

                return 1;
        }

        public int Reset(double tTime)
        {
            Reset();
            evList.AddEvent(tTime,0,1,"terminate");
            return 1;
        }




        public int  Run()
        {
            Event  ev=new Event();
            while((RunningState==1)&&(evList.GetCount()>0))
            {
                if(evList.PopEvent(ev)==0)
                {
                    return 0;
                }
                ExecuteEvent(ev);

            }
            return 1;
        }

        public int RunBatch(double terminateTime)
        {
            batchtime = terminateTime;
            Event ev=new Event();
            while ((RunningState == 1) && (evList.GetCount() > 0)&&(currentTime<terminateTime))
            {
               
                if (evList.PopEvent(ev) == 0)
                {
                    return 0;
                }
                ExecuteEvent(ev);
                LogMTTFTP();
            }

            w1ul = wip[0];
            return 1;
        }





        public int Pause()
        {
            if (RunningState == 1) RunningState = 0;
            else RunningState = 1;
            return 1;
        }

        public int ExecuteEvent(Event ev)
        {
            string funcname = ev.FuncName;
            int mid = ev.mid;
            currentTime = ev.t;
            if(funcname=="start")
            {
                Start(mid);
            }
            else if (funcname == "end")
            {
                End(mid);
            }
            else if (funcname == "terminate")
            {
                Terminate(mid);
            }
            else if (funcname == "breakdown")
            {
                BreakDown(mid);
            }
            else if (funcname == "repairup")
            {
                Repairup(mid);
            }
            else if (funcname == "arrive")
            {
                Arrive(mid);
            }
          
            return 1;
        }

        public int Start(int mid)
        {
            double nextTime;
            nextTime=currentTime+c[mid];
            ms[mid] = 1;////运行态
            tp[mid]=tp[mid]+1;
            evList.AddEvent(nextTime, mid, 1, "end");

            return 1;
        }
      


        public int End(int mid)
        {
            if(tryAddQueue(mid)==0)
            {
                ms[mid] = 2;///堵塞状态
                return 1;
            }
            ms[mid] = 0;////设为空闲
            tryAddStart(mid + 1,0);
            tryAddStart(mid,1E-10);
            unBlock(mid - 1,1E-9);

       


            return 1;
        }




        public int Terminate(int mid)
        {
            RunningState = 0;
            return 1;
        }
        public int BreakDown(int mid)
        {
            if(ms[mid]==1)
            {
                double tt = evList.CancelEvent(mid, "end");
                remain[mid] = tt - currentTime;
            }else if(ms[mid]==2)
            {
                remain[mid]=-0.5;
            }else
            {
                remain[mid] = -1;
            }
            ms[mid] = 3;///设置为故障状态
                        ///
            tryAddRepairUp(mid);
            
                        
            return 1;
        }
        public int Repairup(int mid)
        {
            if(remain[mid]>=0)
            {
                evList.AddEvent(currentTime+remain[mid], mid, 1, "end");
                ms[mid] = 1;
              
            }
            else if (remain[mid] == -0.5)
            {
                ms[mid] = 2;
                unBlock(mid,0);
      
            }else
            {
                ms[mid] = 0;
                tryAddStart(mid,0);
                unBlock(mid-1,1E-9);
            }

            tryAddBreakDown(mid);
            return 1;
        }
        public int Arrive(int mid)
        {

            return 1;
        }


        //////辅助函数
        public int tryAddStart(int mid,double delta)
        {
            if (mid < 0 || mid >= MachineNum) return 0;
            if (ms[mid] > 0) return 0;
            if(mid==0)
            {
                evList.AddEvent(currentTime+delta, mid , 1, "start");
                return 1;
            }else
            {
                if(wip[mid-1]>0)
                {
                    wip[mid - 1] = wip[mid - 1] - 1;

                    evList.AddEvent(currentTime+delta, mid, 1, "start");
                    return 1;
                }
            }
            return 0;
        }

        public int tryAddQueue(int mid)
        {
            if (mid < 0 || mid >= MachineNum) return 0;
            if(mid==MachineNum-1)
            {
                wip[mid] = wip[mid] + 1;
                tp2[mid] = tp2[mid] + 1;
                return 1;
            }
            else
            {
                if (wip[mid]<b[mid])
                {
                    wip[mid] = wip[mid] + 1;
                    tp2[mid] = tp2[mid] + 1;
                   

                    if(mid==0)
                    {
                        if(wip[mid]>w1max)
                        {
                            w1max = wip[mid];
                            //w1time = currentTime;
                            //LogW1();
                        }
                    }
                    return 1;
                }
            }
            return 0;
        }

        public int unBlock(int mid,double delta)
        {
            if (mid < 0 || mid >= MachineNum) return 0;
            if(ms[mid]==2)
            {
                evList.AddEvent(currentTime+delta, mid, 1, "end");
                return 1;
            }

            return 0;
        }

        public int tryAddBreakDown(int mid)
        {
            if (mid < 0 || mid >= MachineNum) return 0;
            if (mttf[mid] <= 0) return 0;//////设定为不发生故障
            if (ms[mid] == 3) return 0;/////处于故障状态，不再发生故障
            double nexttime;
            double r,y;
            r=mttf[mid];
            y = random.NextDouble();
            nexttime = -r * Math.Log(1 - y);
            nexttime = currentTime + nexttime;
            evList.AddEvent(nexttime, mid, 1, "breakdown");
             return 1;
        }

        public int tryAddRepairUp(int mid)
        {
            if (mid < 0 || mid >= MachineNum) return 0;
            if (mttr[mid] <= 0) return 0;//////设定为不发生故障
            if (ms[mid] != 3) return 0;/////不处于故障状态
            double nexttime;
            double r, y;
            r = mttr[mid];
            y = random.NextDouble();
            nexttime = -r * Math.Log(1 - y);

            if (nexttime > repairmax) repairmax = nexttime;

            nexttime = currentTime + nexttime;
            evList.AddEvent(nexttime, mid, 1, "repairup");

           
            return 1;
        }


        public double GetTotalTP()
        {
            Reset();
            RunBatch(batchtime);
            double max = wip[MachineNum - 1] / batchtime;
            TotalS++;
            return max;

        }


        public int ResetAvgN(int [] N, int M)
        {
            int totalN = 0;
            for (int i = 0; i < M - 1; i++)
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


        public double QuickHillClimbingN(double batchtime,int step)
        {
            this.batchtime = batchtime;
            //TotalS = 0;
            //TotalS++;
            int M = MachineNum;

            if (step <= 0)
            {
                TotalS = 0;
                ResetAvgN(b, M);
                step = 32;
            }

            int [] newN = new int[M];

            double max = GetTotalTP();

            Array.Copy(b, newN, M);

            double current;
            // double epsilon = 1E-8;
            int state = 0;
            int S = 100;////最大迭代次数

            int maxi=-1,maxj=-1;
            int tempmaxi = -1, tempmaxj = -1;

            for (int t = 0; t < S; t++)
            {
                state = 0;
                for (int i = 0; i < M - 1; i++)
                    for (int j = 0; j < M - 1; j++)
                    {
                        if (j != i)
                        {
                            if (b[i] < 1+step) continue;
                            if ((i == maxj) || (j == maxi)) continue;
                            b[i] = b[i] - step;
                            b[j] = b[j] + step;
                            current = GetTotalTP();
                            if (current > max)
                            {
                                state = 1;
                                max = current;
                                Array.Copy(b, newN, M);
                                tempmaxi = i;
                                tempmaxj = j;
                                // if (1 - max < epsilon) return newN;
                            }
                            b[i] = b[i] + step;
                            b[j] = b[j] - step;

                        }

                    }
                if (state == 0)
                {
                    if (step <= 1)
                    {
                        GetTotalTP();
                        return 1;
                    }
                    else
                    {
                        step = step / 2;
                        return QuickHillClimbingN(batchtime, step);
                    }
                   
                }
                else
                {
                    Array.Copy(newN, b, M);
                    maxi = tempmaxi;
                    maxj = tempmaxj;
                }

            }

            return 1;
        }

        ///////
       

        ///////

    }

    class Event
    {
       public int priority;
       public double t;
       public int mid;////机器编号 
       public string FuncName;///事件名 arrive ， start  ，end ， terminate，breakdown，repairup
                              ///
        public Event()
       {

       }
        public Event(double t,int mid,int priority,string FuncName)
        {
            this.t=t;
            this.mid=mid;
            this.priority=priority;
            this.FuncName=FuncName;
        }

        public int CompareTo(Event ev2)
        {
            if (this.t < ev2.t) return -1;
            if (this.t > ev2.t) return 1;
            if (this.priority < ev2.priority) return -1;
            if (this.priority > ev2.priority) return 1;
            return 0;
        }

        public int CopyFrom(Event ev2)
        {
            this.t = ev2.t;
            this.mid = ev2.mid;
            this.priority = ev2.priority;
            this.FuncName = ev2.FuncName;
            return 1;

        }
       // public delegate int DoEvent( int i );
    }

    class EventList
    {
        public List<Event> list;

        public EventList ()
        {
            list=new List<Event> ();
        }

        public int GetCount()
        {
            return list.Count;
        }


        private static bool isRemoved(Event ev)
        {
            return true;
        }

        public int Reset()
        {
            list.RemoveRange(0,list.Count);
            //list.RemoveAll(isRemoved);
            return 0;
        }
        public int AddEvent(double t,int mid,int priority,string FuncName)
        {
            Event ev=new Event(t,mid,priority,FuncName);
            for (int i = 0; i < list.Count;i++ )
            {
                if(ev.CompareTo(list[i])==-1)
                {
                    list.Insert(i,ev);
                    return 1;
                }
            }
            list.Add(ev);
                return 1;
        }
         public int PopEvent( Event ev)
        {
             if(list.Count>0)
             {
                 ev.CopyFrom(list[0]);
                 list.RemoveAt(0);
                 return 1;
             }
            
            return 0;
        }

        public double CancelEvent(int mid,string funcname)
        {
            for (int i = 0; i < list.Count; i++)
            {
               if((list[i].mid==mid)&&(list[i].FuncName==funcname))
               {
                   double tt = list[i].t;
                   list.RemoveAt(i);
                   return tt;
               }
            }
            return 0;
        }


    }


    

   



}
