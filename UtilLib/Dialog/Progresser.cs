using System;
using System.Windows.Forms;

namespace UtilLibrary
{
    public class ProgressBarInfo
    {
        private ProgressBar bar_;

        void init()
        {
            this.bar_.Style = ProgressBarStyle.Continuous;
            this.bar_.Minimum = 0;
            this.bar_.Maximum = 100;
            this.bar_.Step = 1;
            this.bar_.Value = 0;
        }
        public void setup(ProgressBar bar)
        {
            this.bar_ = bar;
            if (this.bar_.InvokeRequired) {
                this.bar_.BeginInvoke(new Action(() => this.init()));
            }
            else {
                this.init();
            }
        }

        public void setInit()
        {
            if (this.bar_.InvokeRequired) {
                this.bar_.BeginInvoke(new Action(() => this.bar_.Value = 0));
            }
            else {
                this.bar_.Value = 0;
            }
        }

        public void setMax(int max)
        {
            int maxStep = Math.Min(max, Int32.MaxValue);
            if (this.bar_.InvokeRequired) {
                this.bar_.BeginInvoke(new Action(() => this.bar_.Maximum = maxStep));
            }
            else {
                this.bar_.Maximum = maxStep;
            }
        }

        public void performStep()
        {
            if (this.bar_.InvokeRequired) {
                this.bar_.BeginInvoke(new Action(() => this.bar_.PerformStep()));
            }
            else {
                this.bar_.PerformStep();
            }
        }
    }

    public class Progresser: SingleTon<Progresser>
    {
        readonly ProgressBarInfo info_ = new ProgressBarInfo();

        public void setup(ProgressBar bar)
        {
            this.info_.setup(bar);
        }

        public void setInit()
        {
            this.info_.setInit();
        }

        public void setMax(int max)
        {
            this.info_.setMax(max);
        }

        public void performStep()
        {
            this.info_.performStep();
        }
    }
}
