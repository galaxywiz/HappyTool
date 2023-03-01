using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using UtilLibrary;

namespace HappyFuture.DialogControl.FutureDlg
{
    class OrderRecoderView: QuickDataViewCtl
    {
        internal enum COLUMNS_NAME
        {
            종목, 코드, buyModule, sellModule, 포지션, 주문시간, 보유시간, 구입가, 청산가, 최저가, 최고가, 예상이익, 이익금, 이유
        };

        DateTimePicker startTime_;
        DateTimePicker endTime_;

        public override void setup(QuickDataGridView dataView)
        {
            base.setup(dataView);
            var dlg = Program.happyFuture_.futureDlg_;
            this.startTime_ = dlg.dateTimePicker_OrderStart;
            this.endTime_ = dlg.dateTimePicker_OrderEnd;

            DateTime now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 6, 0, 0);
            this.startTime_.Value = now.AddDays(-1);
            this.endTime_.Value = now;
        }

        protected override void decorationView()
        {
            this.dataGridView_.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            int maxRow = Math.Max(this.dataGridView_.RowCount - 1, 0);
            for (int rowIdx = 0; rowIdx < maxRow; ++rowIdx) {
                double price = 0.0f;
                if (double.TryParse(this.dataGridView_.Rows[rowIdx].Cells[(int) COLUMNS_NAME.이익금].Value.ToString(), out price) == false) {
                    continue;
                }
                if (price > 0) {
                    this.dataGridView_.Rows[rowIdx].DefaultCellStyle.BackColor = Color.LightPink;
                }
                else if (price < 0) {
                    this.dataGridView_.Rows[rowIdx].DefaultCellStyle.BackColor = Color.SkyBlue;
                }
            }
        }

        public override DataTable makeNewTable()
        {
            var bot = ControlGet.getInstance.futureBot();
            DataTable dt;

            if (bot.loadOrderRecodes(this.startTime_.Value, this.endTime_.Value, out dt)) {
                return dt;
            }

            return null;
        }

        public override void update()
        {
            this.dataTable_ = this.makeNewTable();
            this.print();
        }

        public void exportExcel()
        {
            var bot = ControlGet.getInstance.futureBot();
            if (bot.futureDBHandler_ == null) {
                return;
            }
            string excelFileName = "";
            if (bot.futureDBHandler_.selectTradeRecode(this.startTime_.Value, this.endTime_.Value, out excelFileName)) {
                MessageBox.Show(string.Format("{0} 위치에 파일 저장", excelFileName));
            }
        }
    }
}
