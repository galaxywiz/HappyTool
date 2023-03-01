using System;

namespace StockLibrary
{
    public abstract class Phase
    {
        protected bool nextStep_ = false;
        DateTime timeTicker_ = DateTime.Now;

        public virtual void process()
        {
            var now = DateTime.Now;
            // 0.5초마다
            if (this.timeTicker_.AddMilliseconds(500) < now) {
                this.timeTicker_ = now;
            }
            else {
                return;
            }
        }

        public bool nextStep()
        {
            return this.nextStep_;
        }

        public virtual void setNextStep()
        {
            this.nextStep_ = true;
        }
    }

    public class BotState
    {
        //---------------------------------------------------------------------
        // 상테 처리
        protected Phase phase_ = null;

        public virtual void start()
        {
        }

        public virtual void changePhase(Phase newPhase)
        {
            if (this.phase_ != null) {
                this.phase_ = null;
            }
            this.phase_ = newPhase;
            string phaseName = this.phase_.GetType().Name;
        }

        public virtual void process()
        {
        }

        public string nowPhaseName()
        {
            return phase_.GetType().Name;
        }
    }
}