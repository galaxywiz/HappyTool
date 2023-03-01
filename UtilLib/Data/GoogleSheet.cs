using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace UtilLibrary.Data
{
    public class GoogleSheet
    {
        readonly string authJsonPath_;
        readonly string appName_;
        readonly string spreadsheetId_;
        readonly string sheetName_;

        public GoogleSheet(string authJson, string appName, string spreadsheetId, string sheetName)
        {
            // appName = "HappyFuture";
            // spreadsheetId = "1VOCbWUPhCAab5yDsceBusgR2FdIHx32SohS1vJREXFA";
            // sheetName = "매매기록";
            this.authJsonPath_ = authJson;
            this.appName_ = appName;
            this.spreadsheetId_ = spreadsheetId;
            this.sheetName_ = sheetName;
        }
        //****************************************************************
        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };

        public bool update(IList<IList<Object>> objNeRecords)
        {
            try {
                var service = this.authorizeGoogleApp();
                string newRange = this.getRange(service);

                this.UpdatGoogleSheetinBatch(objNeRecords, this.spreadsheetId_, newRange, service);
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "구글 시트에 데이터 입력중 에러, {0}, {1}", e.Message, e.StackTrace);
                return false;
            }
            return true;
        }

        //****************************************************************
        //OpenSheet() - json 인증파일은 여기서 받기
        // https://console.developers.google.com/apis/credentials?project=valid-flow-222223
        //****************************************************************
        SheetsService authorizeGoogleApp()
        {
            UserCredential credential;
            using (var stream =
                new FileStream(this.authJsonPath_, FileMode.Open, FileAccess.Read)) {
                string credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/sheets.googleapis.com-dotnet-quickstart.json");
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }
            // Create Google Sheets API service.
            var service = new SheetsService(new BaseClientService.Initializer() {
                HttpClientInitializer = credential,
                ApplicationName = appName_,
            });
            return service;
        }

        // 다음에 추가할 열을 찾는다.
        private string getRange(SheetsService service)
        {
            // Define request parameters.
            String spreadsheetId = this.spreadsheetId_;
            String range = String.Format("{0}!A:A", this.sheetName_);
            SpreadsheetsResource.ValuesResource.GetRequest getRequest =
                       service.Spreadsheets.Values.Get(spreadsheetId, range);
            ValueRange getResponse = getRequest.Execute();
            if (getResponse == null) {
                return string.Empty;
            }

            IList<IList<Object>> getValues = getResponse.Values;
            int currentCount = getValues.Count() + 1;
            String newRange = "A" + currentCount + ":A";
            return newRange;
        }

        // 이런시식으로 데이터를 만들어야 함. (예시)
        public virtual IList<IList<Object>> genrateRecodes()
        {
            List<IList<Object>> objNewRecords = new List<IList<Object>>();
            IList<Object> obj = new List<Object>();
            obj.Add("Column - 1");
            obj.Add("Column - 2");
            obj.Add("Column - 3");
            objNewRecords.Add(obj);
            return objNewRecords;
        }

        // 실제 업데이트
        private void UpdatGoogleSheetinBatch(IList<IList<Object>> values, string spreadsheetId, string newRange, SheetsService service)
        {
            SpreadsheetsResource.ValuesResource.AppendRequest request =
               service.Spreadsheets.Values.Append(new ValueRange() { Values = values }, spreadsheetId, newRange);
            request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var response = request.Execute();
        }
    }
}
