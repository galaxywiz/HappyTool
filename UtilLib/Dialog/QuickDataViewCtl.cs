using System;
using System.Data;
using System.Windows.Forms;

namespace UtilLibrary
{
    public class QuickDataViewCtl
    {
        protected QuickDataGridView dataGridView_;
        protected DataTable dataTable_;

        protected virtual void initScreen()
        {
        }
        void init(QuickDataGridView dataView)
        {
            this.dataGridView_.DataSource = this.dataTable_;
            this.dataGridView_.ReadOnly = true;
        }

        public virtual void setup(QuickDataGridView dataView)
        {
            this.dataGridView_ = dataView;

            if (this.dataGridView_.InvokeRequired) {
                this.dataGridView_.BeginInvoke(new Action(() => this.init(dataView)));
                this.dataGridView_.BeginInvoke(new Action(() => this.initScreen()));
            }
            else {
                this.init(dataView);
                this.initScreen();
            }
            this.dataGridView_.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
        }

        // 특정 줄 / 컬럼 색같은거 처리 하고 싶을때
        protected virtual void decorationView()
        {
            this.dataGridView_.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }

        // 테이블 첫 컬럼 만드는 작업을 해주는곳
        public virtual DataTable makeNewTable()
        {
            DataTable dt = new DataTable();
            return dt;
        }

        void refreshData()
        {
            this.dataGridView_.DataSource = null;
            this.dataGridView_.DataSource = this.dataTable_;

            if (this.dataGridView_.DataSource == null) {
                return;
            }
            this.decorationView();
        }

        protected void printList(DataTable dt)
        {
            this.dataTable_ = dt;
            this.refreshData();
        }

        public virtual void print(DataTable dt)
        {
            if (this.dataGridView_ == null) {
                return;
            }
            if (this.dataGridView_.InvokeRequired) {
                this.dataGridView_.BeginInvoke(new Action(() => this.printList(dt)));
            }
            else {
                this.printList(dt);
            }
        }
        public virtual void print()
        {
            this.print(this.dataTable_);
        }

        // 화면 갱신 요청
        public virtual void update()
        {
        }

        public string captureView(string fileName)
        {
            return this.dataGridView_.captureView(fileName);
        }
    }
}
