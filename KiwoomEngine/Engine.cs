using StockLibrary;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using UtilLibrary;

namespace KiwoomEngine
{
    public class Engine
    {
        protected Thread thread_ = null;
        protected Bot bot_ = null;

        const Int16 KH_LIMIT_ORDER = 2000;       // 키움증권 제한된 키값 갯수
        // 
        protected ConcurrentQueue<Statement> statmentOrderPool_ = new ConcurrentQueue<Statement>();       //주문을 쌓는 pool

        // 매매 이외의 주문은 모두 이쪽으로 처리 receive 처리함.
        private ConcurrentDictionary<string, Statement> statmentReceivePool_ = new ConcurrentDictionary<string, Statement>(); // 매매 이외의 주문 관리

        DateTime[] screenNumPool_ = new DateTime[KH_LIMIT_ORDER];

        public void setup(Bot bot)
        {
            bot_ = bot;

            for (int i = 0; i < KH_LIMIT_ORDER; ++i) {
                screenNumPool_[i] = DateTime.MinValue;
            }
            thread_ = new Thread(this.run);
            thread_.Priority = ThreadPriority.Highest;
            thread_.Start();
        }

        //---------------------------------------------------------------------
        // 쓰레드 실행 루프
        private bool runLoop_ = true;
        public bool runLoop()
        {
            return runLoop_;
        }

        public void shutdown()
        {
            runLoop_ = false;
        }

        private void run()
        {
            // 요청과 상태 쓰레드를 동시에 가지고 간다. 
            // 분리하면 주식 모듈로의 send /recv 가 꼬일 수 있음.. orz (그래서 MFC가 실패..)
            while (this.runLoop()) {
                Thread.Sleep(10);

                this.processStockOrder();
                bot_.process();
            }
        }

        protected string getScreenNum()
        {
            Int16 index = 0;
            var now = DateTime.Now;
            for (; index < KH_LIMIT_ORDER; ++index) {
                //  5분안에 못받으면 무시해버림
                if (screenNumPool_[index].AddMinutes(5) < now) {
                    screenNumPool_[index] = now;
                    break;
                }
            }
            if (index == KH_LIMIT_ORDER) {
                return "";
            }
            return makeScreenNum(index);
        }

        public void removeScreenNum(string key)
        {
            if (key.Length == 0) {
                return;
            }
            string temp = key.Substring(3);
            int index = int.Parse(temp);
            screenNumPool_[index] = DateTime.MinValue;
        }

        protected string makeScreenNum(Int16 key)
        {
            return string.Format("{0}", key + 1000);
        }

        protected virtual int apiRequestDelay()
        {
            return 1000 / 1;
        }

        ProgressBarInfo engineBar_ = null;
        public void setupProgressBar(ProgressBar bar)
        {
            engineBar_ = new ProgressBarInfo();
            engineBar_.setup(bar);
        }
        //---------------------------------------------------------------------
        //주식 실제 주문 명령 실행 부분
        private void processStockOrder()
        {
            if (statmentOrderPool_.Count == 0) {
                return;
            }

            int count = 0;
            foreach (Statement statement in statmentOrderPool_) {
                string screenNum = this.getScreenNum();
                if (screenNum.Length == 0) {
                    break;
                }
                // 키값이 성공적으로 가져 오면, 실행하고
                statement.screenNo_ = screenNum;
                count++;
            }

            if (engineBar_ != null) {
                engineBar_.setInit();
                engineBar_.setMax(count);
            }
            while (count > 0) {
                try {
                    Statement statement;
                    if (statmentOrderPool_.TryDequeue(out statement)) {
                        if (engineBar_ != null) {
                            engineBar_.performStep();
                        }
                        count--;
                        if (statement.screenNo_ == null) {
                            continue;
                        }
                        if (statement.screenNo_.Length == 0) {
                            continue;
                        }
                        statement.request();

                        this.addStatmentReceive(statement);
                        // 키움증권 제한 1초에 1번만 요청 가능 (나쁜놈들)
                        Thread.Sleep(this.apiRequestDelay());
                    }
                }
                catch (Exception e) {
                    Logger.getInstance.print(KiwoomCode.Log.에러, "쿼리 요청중 에러 {0}/{1}", e.Message, e.StackTrace);
                }
            }
            if (engineBar_ != null) {
                engineBar_.setInit();
            }
        }

        public virtual void addOrder(Statement statement)
        {
            statmentOrderPool_.Enqueue(statement);
        }

        public int orderCount()
        {
            return statmentOrderPool_.Count;
        }

        private void addStatmentReceive(Statement statement)
        {
            statmentReceivePool_[statement.screenNo_] = statement;
        }

        protected Statement getStockStatement(string key)
        {
            foreach (KeyValuePair<string, Statement> data in statmentReceivePool_) {
                if (data.Key == key) {
                    return data.Value;
                }
            }
            return null;
        }

        public virtual void requestStockData(string code, PRICE_TYPE priceType, bool forceBuy = false)
        {
        }

        //---------------------------------------------------------------------
        // 주문등의 주식 관련 명령 실행
        protected static string accountNum_;

        public virtual bool loadAccountInfo()
        {
            return false;
        }

        public static string accountNumber()
        {
            return accountNum_;
        }

        public virtual string userId()
        {
            // 하위 참조
            return "";
        }
    }
}
