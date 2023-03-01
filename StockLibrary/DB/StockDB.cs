using UtilLibrary;

namespace StockLibrary.DB
{
    public class BotSqliteDB: BaseSqliteDB
    {
        public BotSqliteDB(string dbFile) : base(dbFile)
        {
        }
        protected override void createTable()
        {
            throw new System.NotImplementedException();
        }
        protected override string tableName()
        {
            throw new System.NotImplementedException();
        }
    }

    public abstract class StockDataSqliteDB: BaseSqliteDB
    {
        public StockDataSqliteDB(string dbFile) : base(dbFile)
        {
        }

        public abstract bool select(ref StockData StockData);
        public abstract void update(StockData StockData);
        public abstract void delete(StockData StockData);
    }
}
