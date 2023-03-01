using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ExcelDataReader;

namespace UtilLibrary
{
    public class Parser
    {
        protected DataSet dataSet_ = new DataSet();

        protected string filePath_;
        protected string filePathOrg_;

        ~Parser()
        {
            if (this.dataSet_ == null) {
                return;
            }
            this.dataSet_.Dispose();
        }

        public void resetDataSet()
        {
            this.dataSet_ = null;
        }

        public DataSet table()
        {
            return this.dataSet_;
        }
    }

    public class CsvParser: Parser
    {
        public static DataTable getDataTableFromCsv(string strFilePath)
        {
            DataTable dt = new DataTable();
            using (StreamReader sr = new StreamReader(strFilePath)) {
                string[] headers = sr.ReadLine().Split(',');
                foreach (string header in headers) {
                    dt.Columns.Add(header.Replace("\"", ""));
                }
                while (!sr.EndOfStream) {
                    string[] rows = sr.ReadLine().Split(',');
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < headers.Length; i++) {
                        dr[i] = rows[i].Replace("\"", "");
                    }
                    dt.Rows.Add(dr);
                }

            }
            return dt;
        }

        public static void writeDataTable(DataTable sourceTable, TextWriter writer, bool includeHeaders)
        {
            if (includeHeaders) {
                IEnumerable<String> headerValues = sourceTable.Columns
                    .OfType<DataColumn>()
                    .Select(column => quoteValue(column.ColumnName));

                writer.WriteLine(String.Join(",", headerValues));
            }

            IEnumerable<String> items = null;

            foreach (DataRow row in sourceTable.Rows) {
                items = row.ItemArray.Select(o => quoteValue(o?.ToString() ?? String.Empty));
                writer.WriteLine(String.Join(",", items));
            }

            writer.Flush();
        }

        private static string quoteValue(string value)
        {
            return String.Concat("\"",
            value.Replace("\"", "\"\""), "\"");
        }
    }

    public class ExcelParser: Parser
    {
        public ExcelParser(string filePath)
        {
            this.filePath_ = filePath;
            this.filePathOrg_ = filePath.ToLower();
        }

        //-------------------------------------------------------------------------------//
        public void read()
        {
            FileStream stream = File.Open(filePath_, FileMode.Open, FileAccess.Read);
            IExcelDataReader excelReader = null;
            if (filePathOrg_.Contains("xlsx")) {
                excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            }
            else {
                excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
            }

            var result = excelReader.AsDataSet(new ExcelDataSetConfiguration() {
                ConfigureDataTable = (_) => new ExcelDataTableConfiguration() {
                    UseHeaderRow = true
                }
            });

            //var tables = result.Tables
            //                   .Cast<DataTable>()
            //                   .Select(t => new {
            //                       TableName = t.TableName,
            //                       Columns = t.Columns
            //                                                .Cast<DataColumn>()
            //                                                .Select(x => x.ColumnName)
            //                                                .ToList()
            //                   });
            var table = result.Tables[0].Copy();
            this.dataSet_.Tables.Add(table);
            excelReader.Close();
        }

        public void save(DataTable dt)
        {
            var lines = new List<string>();

            string[] columnNames = dt.Columns.Cast<DataColumn>().
                                              Select(column => column.ColumnName).
                                              ToArray();

            var header = string.Join(",", columnNames);
            lines.Add(header);

            var valueLines = dt.AsEnumerable()
                               .Select(row => string.Join(",", row.ItemArray));
            lines.AddRange(valueLines);

            int euckrCodepage = 51949;
            System.Text.Encoding euckr = System.Text.Encoding.GetEncoding(euckrCodepage);
            File.WriteAllLines(filePath_ + ".csv", lines, euckr);
        }
    }
}