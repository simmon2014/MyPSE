using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSE.DES
{

    class IEvent
    {
        public int priority;
        public double t;
        public int mid;////机器编号 
        public EventName FuncName;///事件名 arrive ， start  ，end ， terminate，breakdown，repairup
        ///
        public IEvent()
        {

        }
        public IEvent(double t, int mid, int priority, EventName FuncName)
        {
            this.t = t;
            this.mid = mid;
            this.priority = priority;
            this.FuncName = FuncName;
        }

        public int CompareTo(IEvent ev2)
        {
            if (this.t < ev2.t) return -1;
            if (this.t > ev2.t) return 1;
            if (this.priority < ev2.priority) return -1;
            if (this.priority > ev2.priority) return 1;
            return 0;
        }

        public int CopyFrom(IEvent ev2)
        {
            this.t = ev2.t;
            this.mid = ev2.mid;
            this.priority = ev2.priority;
            this.FuncName = ev2.FuncName;
            return 1;

        }
        // public delegate int DoEvent( int i );
    }

   class IEventList
    {
        public List<IEvent> list;

        public IEventList()
        {
            list = new List<IEvent>();
        }

        public int GetCount()
        {
            return list.Count;
        }


        private static bool isRemoved(IEvent ev)
        {
            return true;
        }

        public int Reset()
        {
            list.RemoveRange(0, list.Count);
            //list.RemoveAll(isRemoved);
            return 0;
        }
        public int AddEvent(double t, int mid, int priority, EventName FuncName)
        {
            IEvent ev = new IEvent(t, mid, priority, FuncName);
            for (int i = 0; i < list.Count; i++)
            {
                if (ev.CompareTo(list[i]) == -1)
                {
                    list.Insert(i, ev);
                    return 1;
                }
            }
            list.Add(ev);
            return 1;
        }
        public int PopEvent(IEvent ev)
        {
            if (list.Count > 0)
            {
                ev.CopyFrom(list[0]);
                list.RemoveAt(0);
                return 1;
            }

            return 0;
        }

        public double CancelEvent(int mid, EventName funcname)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if ((list[i].mid == mid) && (list[i].FuncName == funcname))
                {
                    double tt = list[i].t;
                    list.RemoveAt(i);
                    return tt;
                }
            }
            return 0;
        }


    }

     class BaseSim
    {
        public IEventList evList = new IEventList();
        public List<Entity> EntList=new List<Entity>();


        //public string m_strResult;
       // public Random random;
       // public int mseed;
       /// public int TotalS;////爬山法迭代次数
        public double batchtime;
        public double currentTime = 0;
        public int RunningState = 0;

        public int InitEntityList()
        {
             
             return 1;
        }
        


        public int AddEvent(int id,EventName ev,double t)
        {
            evList.AddEvent(t,id,1,ev);
            return 1;
        }

        public int RunBatch(double terminateTime)
        {
            batchtime = terminateTime;
            IEvent ev = new IEvent();
            while ((RunningState == 1) && (evList.GetCount() > 0) && (currentTime < terminateTime))
            {
                if (evList.PopEvent(ev) == 0)
                {
                    return 0;
                }
                ExecuteEvent(ev);

            }

            return 1;
        }


        public int ExecuteEvent(IEvent ev)
        {
            currentTime = ev.t;
            EntList[ev.mid].Execute(ev.FuncName);

            return 1;
        }



    }

    public enum EventName
    {
        InFrom = 1,
        OutTo = 2,
        BeInFrom = 3,
        BeOutTo = 4,
        Start = 5,
        End = 6,
        Breakdown = 7,
        Repairup
    }

    public enum MState
    {
        idle = 1,
        running = 2,
        block = 3,
        breakdown = 4
    }


   

    public class Entity
    {
        //属性
        public int bufInMax { get; set; }///最大入口缓冲
        public int bufOutMax { get; set; }///最大出口缓冲
        public MState state { get; set; }  ///系统状态  空闲  运行   block  失败
        public bool isInActive { get; set; } ////入口是否主动
        public bool isOutActive { get; set; } ////出口是否主动


        public int BeInNum { get; set; }///被要求的数量
        public int BeOutNum { get; set; }///被要求的数量
                                         ///
                                         ///
        public int id { get; set; }///实体id，父项来指定
        //属性
        BaseSim parentSim { get; set; }////父项传递过来

        public Entity()
        {
            Init();
        }

        //内部状态
       public List<Entity> InEntList;
       public List<Entity> OutEntList;
       public List<int> InNumList;
       public List<int> OutNumList;



       public int bufS{ get; set; } ////实际缓冲 


       public virtual int Init()
       {
           bufInMax = 1;
           bufOutMax = 1;
           bufS = 0;
           InEntList=new List<Entity>();
           OutEntList=new List<Entity>();
           InNumList=new List<int>();
           OutNumList=new List<int>();
           state = MState.idle;
           isInActive = false;
           isOutActive = false;
            return 1;
       }

        ////调度程序执行
       public virtual int Execute(EventName en)
        {
            switch (en)
            {
                case EventName.InFrom:
                    InFrom();
                    break;
                case EventName.OutTo:
                    OutTo();
                    break;
                case EventName.BeInFrom:
                    BeInFrom();
                    break;
                case EventName.BeOutTo:
                    BeOutTo();
                    break;
                case EventName.Start:
                    Start();
                    break;
                case EventName.End:
                    End();
                    break;

                default:
                    break;
            }

            return 1;
        }


        //主动事件
       public virtual int InFrom()
        {
           ///状态改变
           ///引发前面实体变化
            for (int i = 0; i < InEntList.Count; i++)
            {
                InEntList[i].tryAddBeOutTo(InNumList[i]);
                bufS = bufS + InNumList[i];
                
            }
            tryAddStart();
            

            return 1;
        }

       public virtual int OutTo()
        {
           int status=0; 
           
           if(bufS==bufOutMax) status=1;////刚刚可能block过
            for (int i = 0; i < OutEntList.Count; i++)
            {
                OutEntList[i].tryAddBeInFrom(OutNumList[i]);
                bufS = bufS - OutNumList[i];
            }

          ///////发送消息给前面
            SendPrevMessage(status);
            return 1;
        }

        //被动事件
       public virtual int BeInFrom()
        {
            bufS = bufS + BeInNum;
            tryAddStart();
            return 1;
        }
       public virtual int BeOutTo()
        {
           ///改变自身状态
            int status = 0;
            if (bufS == bufOutMax) status = 1;////刚刚可能block过
            bufS = bufS - BeOutNum;
           //提示前面消息
            SendPrevMessage(status);

           
            return 1;
        }


       //事件
       public virtual int Start()
       {
           return 1;
       }
       public virtual int End()
       {
           return 1;
       }

      





        //方法
        public virtual int isCanBeInFrom(int num)
        {
            if (state == MState.breakdown) return 0;
            if (state == MState.running) return 0;
            if ( bufS + num > bufInMax) return 0;
            return 1;
        }
        public virtual int isCanBeToOut(int num)
        {
            if (state == MState.breakdown) return 0;
            if (state == MState.running) return 0;
            if ( bufS - num < 0) return 0;
            return 1;
        }

        ////
        public virtual int tryAddInFrom()
        {
            for (int i = 0; i < InEntList.Count; i++)
            {
                if (InEntList[i].isCanBeToOut(InNumList[i]) == 0)
                {
                    return 0;
                }
            }

            parentSim.AddEvent(id,EventName.InFrom,0);

            return 1;
        }

        public virtual int tryAddOutTo()
        {
            for (int i = 0; i < OutEntList.Count; i++)
            {
                if (OutEntList[i].isCanBeInFrom(OutNumList[i]) == 0)
                {
                    return 0;
                }
            }

            parentSim.AddEvent(id, EventName.OutTo, 0);

            return 1;
        }

        ///
        public virtual int tryAddBeOutTo(int num)
        {
            //更新状态
            BeOutNum = num;
            parentSim.AddEvent(id, EventName.BeOutTo, 0);



            return 1;
        }
        public virtual int tryAddBeInFrom(int num)
        {
            BeInNum = num;
            parentSim.AddEvent(id, EventName.BeInFrom, 0);
            return 1;
        }


        ////
        public virtual int tryAddStart()
        {
            parentSim.AddEvent(id, EventName.Start, 0);
            return 1;
        }

        public int tryAddEnd()
        {
            return 1;
        }



      //////辅助函数
       public int SendPrevMessage(int status)
        {
            if (isInActive)
            {
                tryAddInFrom();
                return 1;
            }
            else if (status == 1)/////刚刚可能block过
            {
                for (int i = 0; i < InEntList.Count; i++)
                {
                    if (InEntList[i].isOutActive)
                    {
                        OutEntList[i].tryAddOutTo();
                        return 1;
                    }


                }
            }
            return 0;
        }
       

    
    }




    public class Machine : Entity
    {




    }

    public class Buffer : Entity
    {


    }
    public class Conveyer : Entity
    {




    }

    public class World : Entity
    {

        //public virtual int isCanBeToOut(int num)
        //{
        //    if (state == MState.breakdown) return 0;
        //    if (state == MState.running) return 0;
        //    if (bufS - num < 0) return 0;
        //    return 1;
        //}


        //public virtual int tryAddBeOutTo(int num)
        //{
        //    //更新状态
        //    BeOutNum = num;
        //    parentSim.AddEvent(id, EventName.BeOutTo, 0);



        //    return 1;
        //}

    }


}
