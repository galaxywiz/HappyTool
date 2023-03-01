using HappyTool.Stock.Calculate;
using HappyTool.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappyTool.Stock
{
    abstract class StockState
    {
        protected bool nextStep_ = false;
        private long dateTick_ = DateTime.Now.Ticks;
        protected long tick_ = 0;

        public virtual void process()
        {
            long now = DateTime.Now.Ticks;
            if (dateTick_ + TimeSpan.TicksPerSecond < now) {
                dateTick_ = now;
                tick_++;
            }
        }

        public bool nextStep()
        {
            return nextStep_;
        }

        public virtual void setNextStep()
        {
            nextStep_ = true;
        }
    }

    class LoadAccountState :StockState
    {
        public override void process()
        {
            StockEngine.getInstance.addOrder(new AccountStockStatement());
            StockEngine.getInstance.addOrder(new AccountMoneyStatement());
            nextStep_ = true;
        }
    }

    class ListUpStocksState :StockState
    {
        public override void process()
        {
            StockEngine.getInstance.addOrder(new ForeignerTradingSotck());
            //  StockEngine.getInstance.addOrder(new AgencyTradingStock());

            //   StockEngine.getInstance.addOrder(new YesterdayHighTradingStock());
            //StockEngine.getInstance.addOrder(new SuddenlyHighTradingStock());
            //StockEngine.getInstance.addOrder(new ApproachHighPriceTradingStock());
            //StockEngine.getInstance.addOrder(new NewHighPriceStock());

            nextStep_ = true;
        }
    }
    //---------------------------------------------------------------------
    // 리스트 업한 주식에서 실제 볼 주식들만 고르는 처리
    class SelectedPriceLoadStockState :StockState
    {
        bool init = false;
        PRICE_TYPE priceType_ = PRICE_TYPE.MIN15;
        public override void process()
        {
            base.process();

            StockBot bot = StockBot.getInstance;
            bot.loadStocksPriceData();
            if (init == false) {
                bot.drawStockView();
                init = true;
            }

            if (bot.checkFullLoadedStock(priceType_)) {
                priceType_++;
                if (priceType_ == PRICE_TYPE.MAX) {
                    nextStep_ = true;
                }
            }
        }
    }

    //---------------------------------------------------------------------
    // 리스트 업한 주식들을 관리 하고 팔고 / 사는 처리
    class WatchingStockState :StockState
    {
        bool init = false;
        public override void process()
        {
            base.process();

            StockBot bot = StockBot.getInstance;
            if (init == false) {
                bot.drawStockView();
                init = true;
            }
            bot.watchingForPick();
        }
    }

    class StockBotState
    {
        //---------------------------------------------------------------------
        // 상테 처리
        StockState state_ = null;

        public void start()
        {
            // 1. 계좌 리스트 업을 먼저 시킴
            state_ = new LoadAccountState();
        }

        public void changeState(StockState newState)
        {
            if (state_ != null) {
                state_ = null;
            }
            state_ = newState;
            char sp = '.';
            string[] className = state_.ToString().Split(sp);
            string stateName = "";
            foreach (string name in className) {
                stateName = name;
            }

            Logger.getInstance.consolePrint("==================================================================");
            Logger.getInstance.consolePrint("@ 주식 봇 상태 변경 : {0}", stateName);
            Logger.getInstance.print(KiwoomCode.Log.주식봇, "@ 주식 봇 상태 변경 : {0}", stateName);

            Program.happyTool_.setTitle(stateName);
        }

        public void process()
        {
            if (state_ == null) {
                return;
            }

            state_.process();

            //상태 변화 시킴
            if (state_.nextStep()) {
                Type stateType = state_.GetType();
                if (stateType == typeof(LoadAccountState)) {
                    this.changeState(new ListUpStocksState());
                } else if (stateType == typeof(ListUpStocksState)) {
                    this.changeState(new SelectedPriceLoadStockState());
                } else if (stateType == typeof(SelectedPriceLoadStockState)) {
                    // 4. 워칭 모드로 전환
                    this.changeState(new WatchingStockState());
                }
            }
        }
    }
}
