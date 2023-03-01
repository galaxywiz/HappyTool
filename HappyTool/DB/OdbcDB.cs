using HappyTool.Dlg;
using HappyTool.Stock;
using StockLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using UtilLibrary;

// SQLite 심각한 속도 문제가 있음. 그냥 MSSQL 쓰자
namespace HappyTool
{
    class OdbcDB
    {
        public OdbcDB()
        {
            this.open();
        }

        ~OdbcDB()
        {
            if (sqlConn_ != null) {
                sqlConn_.Close();
                sqlConn_ = null;
            }
        }

        protected SqlConnection sqlConn_ = null;
        string dbName = "StockData";

        protected void open()
        {
            if (PublicVar.dbUser.Length == 0) {
                return;
            }

            string provide = string.Format("server = {0}; uid = {1}; pwd = {2}; database = {3};",
                PublicVar.dbIp, PublicVar.dbUser, PublicVar.dbPw, dbName);

            //sqlConn_ = new SqlConnection(provide);
            //if (sqlConn_.State != ConnectionState.Closed) {
            //    sqlConn_.Close();
            //}
        }
    }

    class StockDataOdbcDB :OdbcDB
    {

        void createTableQuery(ref StockData stockData)
        {
            if (sqlConn_ == null) {
                return;
            }
            if (sqlConn_.State != ConnectionState.Closed) {
                sqlConn_.Close();
            }
            try {
                SqlCommand cmd = new SqlCommand();

                cmd.Connection = sqlConn_;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "dbo.stock_makeTable";

                cmd.Parameters.Add("@stockCode", SqlDbType.Int);
                cmd.Parameters["@stockCode"].Value = stockData.code_;

                sqlConn_.Open();
                cmd.ExecuteNonQuery();
                sqlConn_.Close();
            } catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, e.Message);
            }
        }

        public bool selectData(ref StockData stockData)
        {
            if (sqlConn_ == null) {
                return false;
            }
            if (sqlConn_.State != ConnectionState.Closed) {
                sqlConn_.Close();
            }
            foreach (PRICE_TYPE type in Enum.GetValues(typeof(PRICE_TYPE))) {
                if (type == PRICE_TYPE.MAX) {
                    continue;
                }
                try {
                    //SqlCommand cmd = new SqlCommand();

                    //cmd.Connection = sqlConn_;
                    //cmd.CommandType = CommandType.StoredProcedure;
                    //cmd.CommandText = "dbo.stock_loadPrice";

                    //cmd.Parameters.Add("@stockCode", SqlDbType.Int);
                    //cmd.Parameters.Add("@priceType", SqlDbType.Int);
                    //cmd.Parameters["@stockCode"].Value = stockData.code_;
                    //cmd.Parameters["@priceType"].Value = (int) type;

                    //sqlConn_.Open();
                    //cmd.ExecuteNonQuery();

                    //SqlDataReader dataReader = cmd.ExecuteReader();
                    //if (dataReader.HasRows == false) {
                    //    sqlConn_.Close();
                    //    return false;
                    //}
                    //// 첫행으로 기존 데이터 삭제할지등을 봐야 함.
                    //dataReader.Read();
                    //string lastDateTime = dataReader["dateStr"].ToString();
                    //DateTime lastDate = CandleData.strToDateTime(lastDateTime);

                    //// 이미 갖고 있는 주식 가격표가 더 최신것이면,
                    //if (stockData.priceTable_.Count > 0)
                    //    if (lastDate <= stockData.priceTable_[0].date_) {
                    //        sqlConn_.Close();
                    //        continue;
                    //    } else {
                    //        // 이럴리는 없겠지만...
                    //        stockData.clearPrice();
                    //    }
                    //stockData.dbLoadedLastDate_ = lastDate;

                    //// 위에서 dataRead를 먼저 했으니 do~while 임
                    //do {
                    //    string dateTime = dataReader["dateStr"].ToString();
                    //    double price = double.Parse(dataReader["price"].ToString());
                    //    double startPrice = double.Parse(dataReader["startPrice"].ToString());
                    //    double highPrice = double.Parse(dataReader["highPrice"].ToString());
                    //    double lowPrice = double.Parse(dataReader["lowPrice"].ToString());
                    //    UInt64 volume = UInt64.Parse(dataReader["volume"].ToString());
                    //    CandleData priceData = new CandleData(dateTime, price, startPrice, highPrice, lowPrice, volume);
                    //    stockData.updatePrice(priceData);
                    //} while (dataReader.Read());

                    //dataReader.Close();
                    //sqlConn_.Close();

                    //stockData.receivedDataProcess();
                } catch (Exception e) {
                    Logger.getInstance.print(KiwoomCode.Log.에러, e.Message);
                    return false;
                }
            }
            StockDlg dlg = Program.happyTool_.stockDlg_;
            dlg.printStatus(string.Format("{0} 가격표 db에서 로딩, {1}/{2}", stockData.name_, ControlGet.getInstance.stockBot().stockPoolIndex_++, ControlGet.getInstance.stockBot().stockPoolCount()));
            return true;
        }

        public void insertData(ref StockData stockData, PRICE_TYPE priceType)
        {
            if (sqlConn_ == null) {
                return;
            }
            if (sqlConn_.State != ConnectionState.Closed) {
                sqlConn_.Close();
            }

            //int typeIdx = (int) priceType;
            //List<CandleData> priceTable = stockData.priceTable_;
            //DateTime lastDate = stockData.dbLoadedLastDate_;

            //this.createTableQuery(ref stockData);

            //foreach (CandleData priceDate in priceTable) {
            //    Int64 dateInt = Int64.Parse(priceDate.dateStr_);

            //    if (lastDate == priceDate.date_) {
            //        return;
            //    }

            //    try {

            //        SqlCommand cmd = new SqlCommand();

            //        cmd.Connection = sqlConn_;
            //        cmd.CommandType = CommandType.StoredProcedure;
            //        cmd.CommandText = "dbo.stock_insertRow";

            //        cmd.Parameters.Add("@stockCode", SqlDbType.Int);
            //        cmd.Parameters.Add("@priceType", SqlDbType.Int);
            //        cmd.Parameters.Add("@date", SqlDbType.BigInt);
            //        cmd.Parameters.Add("@price", SqlDbType.Int);
            //        cmd.Parameters.Add("@startPrice", SqlDbType.Int);
            //        cmd.Parameters.Add("@highPrice", SqlDbType.Int);
            //        cmd.Parameters.Add("@lowPrice", SqlDbType.Int);
            //        cmd.Parameters.Add("@volume", SqlDbType.BigInt);

            //        cmd.Parameters["@stockCode"].Value = stockData.code_;
            //        cmd.Parameters["@priceType"].Value = typeIdx;
            //        cmd.Parameters["@date"].Value = dateInt;
            //        cmd.Parameters["@price"].Value = priceDate.price_;
            //        cmd.Parameters["@startPrice"].Value = priceDate.startPrice_;
            //        cmd.Parameters["@highPrice"].Value = priceDate.highPrice_;
            //        cmd.Parameters["@lowPrice"].Value = priceDate.lowPrice_;
            //        cmd.Parameters["@volume"].Value = priceDate.volume_;

            //        sqlConn_.Open();
            //        cmd.ExecuteNonQuery();
            //        sqlConn_.Close();
            //    } catch (Exception e) {
            //        Logger.getInstance.print(KiwoomCode.Log.에러, e.Message);
            //        return;
            //    }
            //}
        }
    }
}
