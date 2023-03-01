using System;

namespace KiwoomEngine
{
    public abstract class Statement
    {
        // 화면번호 생산
        public string screenNo_ { get; set; }
        public string requestName_ { get; set; }
        public long receivedTick_ { get; set; }
        public DateTime orderTime_ { get; }

        public Statement()
        {
            receivedTick_ = 0;
            orderTime_ = DateTime.Now;
        }
               
        protected abstract bool setInput();

        // 주식 모듈로 명령을 내리는 부분
        public abstract void request();

        public virtual bool isTradingOrder()
        {
            return false;
        }
    }
}
