namespace FutureEngine
{
    public abstract class Statement
    {
        // 화면번호 생산
        public string screenNo_ { get; set; }
        protected string requestName_ { get; set; }
        public long receivedTick_ { get; set; }

        public Statement()
        {
            receivedTick_ = 0;
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
