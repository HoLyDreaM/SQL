using SQL.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi.Core.Web;
using Tweetinvi;
using Tweetinvi.Models;
using System.Net.Http;
using RestSharp.Authenticators;
using DevExpress.Utils.Drawing.Helpers;
using System.Net;

namespace SQL
{
    public static class SQL
    {
        public static async Task<(bool Process, string Message)> TelegramSinyal(string strUrl, string strText)
        {
            bool blnResult = false;
            string strMessage = "";

            try
            {
                var options = new RestClientOptions()
                {
                    MaxTimeout = -1,
                };
                var client = new RestClient(options);
                var request = new RestRequest(strUrl, Method.Post);
                request.AddHeader("Content-Type", "text/plain");
                var body = strText;
                request.AddParameter("text/plain", body, ParameterType.RequestBody);
                RestResponse response = client.ExecuteAsync(request).Result;
                dynamic jsonResponseText = response.Content.ToString();
                Root jsonResult = JsonConvert.DeserializeObject<Root>(jsonResponseText);

                if (jsonResult == null)
                {
                    strMessage = "";
                    blnResult = false;
                }
                else
                {
                    strMessage = "";
                    blnResult = jsonResult.ok;

                }
            }
            catch (Exception ex)
            {
                strMessage = "Telegrama Sinyal Gönderilemedi.Detay:\n" + ex.Message.ToString();
                blnResult = false;
            }

            return (blnResult, strMessage);
        }
        public static async Task<TwitDataResponse> TwiterSendMessage(string strText, string ApiKey, string ApiSecret, string AccesToken, string AccesTokenSecret, string Proxy = "")
        {
            TwitDataResponse TResponse = new TwitDataResponse();
            try
            {
                var client = new TwitterClient(ApiKey, ApiSecret, AccesToken, AccesTokenSecret);

                if(!string.IsNullOrEmpty(Proxy))
                {
                    client.Config.ProxyConfig = new ProxyConfig(Proxy);
                }

                var poster = new TweetsV2Poster(client);

                var authenticatedUser = await client.Users.GetAuthenticatedUserAsync();
                ITwitterResult result = await poster.PostTweet(new TweetV2PostRequest { Text = strText });

                dynamic jsonResponseText = result.Content.ToString();
                TwitRoot jsonResult = JsonConvert.DeserializeObject<TwitRoot>(jsonResponseText);


                TResponse.ScreenName = authenticatedUser.ScreenName.ToString();
                TResponse.id = jsonResult.data.id.ToString();
                TResponse.Url = "https://twitter.com/" + authenticatedUser.ScreenName.ToString() + "/status/" + jsonResult.data.id.ToString();

                return TResponse;
            }
            catch (Exception ex)
            {
                Log.Error("Twit Atılamadı.Detay: {Exception}", ex.Message.ToString());
            }

            return TResponse;
        }
        public static async Task<(bool Process, string Content)> AICreateMessage(string Msg, string API)
        {
            bool blnResult = false;
            string strMessage = "";

            try
            {
                List<Messagex> MData = new List<Messagex>();
                var MsgData = new Messagex
                {
                    content = Msg,
                    role = "user"
                };
                MData.Add(MsgData);

                var MSGS = new AIMsg()
                {
                    model = "gpt-3.5-turbo",
                    messages = MData
                };

                var options = new RestClientOptions()
                {
                    MaxTimeout = -1,
                };
                var client = new RestClient(options);
                var request = new RestRequest("https://api.openai.com/v1/chat/completions", Method.Post);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", $"Bearer {API}");
                var body = JsonConvert.SerializeObject(MSGS);
                //request.AddJsonBody(JsonConvert.SerializeObject(MSGS));
                request.AddParameter("text/plain", body, ParameterType.RequestBody);
                RestResponse response = client.ExecuteAsync(request).Result;
                dynamic jsonResponseText = response.Content.ToString();
                GPT jsonResult = JsonConvert.DeserializeObject<GPT>(jsonResponseText);

                if (jsonResult == null)
                {
                    strMessage = "";
                    blnResult = false;
                }
                else
                {
                    foreach (var item in jsonResult.choices)
                    {
                        strMessage = item.message.content.ToString();
                        blnResult = true;
                    }
                }
            }
            catch (Exception ex)
            {
                strMessage = "Telegrama Sinyal Gönderilemedi.Detay:\n" + ex.Message.ToString();
                blnResult = false;
            }

            return (blnResult, strMessage);
        }
        public static DataTable ToDataTable<T>(this IList<T> data)
        {
            PropertyDescriptorCollection props =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }
        public static DataTable ConvertToDataTable<T>(IList<T> data)
        {
            DataTable table = new DataTable();

            //special handling for value types and string
            if (typeof(T).IsValueType || typeof(T).Equals(typeof(string)))
            {

                DataColumn dc = new DataColumn("Value", typeof(T));
                table.Columns.Add(dc);
                foreach (T item in data)
                {
                    DataRow dr = table.NewRow();
                    dr[0] = item;
                    table.Rows.Add(dr);
                }
            }
            else
            {
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
                foreach (PropertyDescriptor prop in properties)
                {
                    table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                }
                foreach (T item in data)
                {
                    DataRow row = table.NewRow();
                    foreach (PropertyDescriptor prop in properties)
                    {
                        try
                        {
                            row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                        }
                        catch (Exception ex)
                        {
                            row[prop.Name] = DBNull.Value;
                        }
                    }
                    table.Rows.Add(row);
                }
            }
            return table;
        }
        public static List<T> DataTableToList<T>(this DataTable dataTable) where T : new()
        {
            var dataList = new List<T>();
            const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance;
            var objFieldNames = typeof(T).GetProperties(flags).Cast<System.Reflection.PropertyInfo>().
                Select(item => new
                {
                    Name = item.Name,
                    Type = Nullable.GetUnderlyingType(item.PropertyType) ?? item.PropertyType
                }).ToList();
            var dtlFieldNames = dataTable.Columns.Cast<DataColumn>().
                Select(item => new
                {
                    Name = item.ColumnName,
                    Type = item.DataType
                }).ToList();

            foreach (DataRow dataRow in dataTable.AsEnumerable().ToList())
            {
                var classObj = new T();

                foreach (var dtField in dtlFieldNames)
                {
                    System.Reflection.PropertyInfo propertyInfos = classObj.GetType().GetProperty(dtField.Name);

                    var field = objFieldNames.Find(x => x.Name == dtField.Name);

                    if (field != null)
                    {

                        if (propertyInfos.PropertyType == typeof(DateTime))
                        {
                            propertyInfos.SetValue
                            (classObj, convertToDateTime(dataRow[dtField.Name]), null);
                        }
                        else if (propertyInfos.PropertyType == typeof(Nullable<DateTime>))
                        {
                            propertyInfos.SetValue
                            (classObj, convertToDateTime(dataRow[dtField.Name]), null);
                        }
                        else if (propertyInfos.PropertyType == typeof(int))
                        {
                            propertyInfos.SetValue
                            (classObj, ConvertToInt(dataRow[dtField.Name]), null);
                        }
                        else if (propertyInfos.PropertyType == typeof(long))
                        {
                            propertyInfos.SetValue
                            (classObj, ConvertToLong(dataRow[dtField.Name]), null);
                        }
                        else if (propertyInfos.PropertyType == typeof(decimal))
                        {
                            propertyInfos.SetValue
                            (classObj, ConvertToDecimal(dataRow[dtField.Name]), null);
                        }
                        else if (propertyInfos.PropertyType == typeof(String))
                        {
                            if (dataRow[dtField.Name].GetType() == typeof(DateTime))
                            {
                                propertyInfos.SetValue
                                (classObj, ConvertToDateString(dataRow[dtField.Name]), null);
                            }
                            else
                            {
                                propertyInfos.SetValue
                                (classObj, ConvertToString(dataRow[dtField.Name]), null);
                            }
                        }
                        else
                        {
                            propertyInfos.SetValue
                                (classObj, Convert.ChangeType(dataRow[dtField.Name], propertyInfos.PropertyType), null);
                        }
                    }
                }
                dataList.Add(classObj);
            }
            return dataList;
        }
        private static string ConvertToDateString(object date)
        {
            if (date == null)
                return string.Empty;

            return date == null ? string.Empty : Convert.ToDateTime(date).ConvertDate();
        }
        private static string ConvertToString(object value)
        {
            return Convert.ToString(ReturnEmptyIfNull(value));
        }
        private static int ConvertToInt(object value)
        {
            return Convert.ToInt32(ReturnZeroIfNull(value));
        }
        private static long ConvertToLong(object value)
        {
            return Convert.ToInt64(ReturnZeroIfNull(value));
        }
        private static decimal ConvertToDecimal(object value)
        {
            return Convert.ToDecimal(ReturnZeroIfNull(value));
        }
        private static DateTime convertToDateTime(object date)
        {
            return Convert.ToDateTime(ReturnDateTimeMinIfNull(date));
        }
        public static object ReturnEmptyIfNull(this object value)
        {
            if (value == DBNull.Value)
                return string.Empty;
            if (value == null)
                return string.Empty;
            return value;
        }
        public static object ReturnZeroIfNull(this object value)
        {
            if (value == DBNull.Value)
                return 0;
            if (value == null)
                return 0;
            return value;
        }
        public static object ReturnDateTimeMinIfNull(this object value)
        {
            if (value == DBNull.Value)
                return DateTime.MinValue;
            if (value == null)
                return DateTime.MinValue;
            return value;
        }
        public static string ConvertDate(this DateTime datetTime, bool excludeHoursAndMinutes = false)
        {
            if (datetTime != DateTime.MinValue)
            {
                if (excludeHoursAndMinutes)
                    return datetTime.ToString("yyyy-MM-dd");
                return datetTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
            }
            return null;
        }
        public static string MSSQLConnectionString()
        {
            string strAppPath = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            string strPath = strAppPath + "\\parameters.json";
            var json = System.IO.File.ReadAllText(strPath);
            var jObject = JObject.Parse(json);
            string result = "";
            result = string.Format("Server={0};Database={1};User Id={2};Password ='{3}'; Trusted_Connection=False; MultipleActiveResultSets=true", new object[]
                {
                    jObject["Server"].ToString(),
                    jObject["Database"].ToString(),
                    jObject["UserName"].ToString(),
                    jObject["Password"].ToString()
                });

            return result;
        }
        public static void LogSettings(string strLogFileName = "")
        {
            string strAppFolder = AppDomain.CurrentDomain.BaseDirectory;
            string strLogFolder = strAppFolder + @"\Logs"; //Application.StartupPath + @"\Logs";
            string strConfigFilePath = strAppFolder + @"\App.cfg";
            SharpConfig.Configuration cfgApp;
            Serilog.Events.LogEventLevel lelMain;

            #region Konfigurasyon Islemleri
            if (!System.IO.File.Exists(strConfigFilePath))
            {
                //Dosya yok ondegerlerle olusturalim
                lelMain = Serilog.Events.LogEventLevel.Error;
                cfgApp = new SharpConfig.Configuration();
                SharpConfig.Section secDebug = cfgApp["Debug Config"];
                secDebug["MinDebugLevel"].SetValue<Serilog.Events.LogEventLevel>(lelMain);

                //Firma no icin konfigurasyon ekleyelim 
                SharpConfig.Section secRunTime = cfgApp["RunTime Config"];

                cfgApp.Save(strConfigFilePath);
            }
            else
            {
                //CFG Dosyasi var, okuyalim
                cfgApp = SharpConfig.Configuration.LoadFromFile(strConfigFilePath);
                SharpConfig.Section secDebug = cfgApp["Debug Config"];
                lelMain = secDebug["MinDebugLevel"].GetValue<Serilog.Events.LogEventLevel>();

                SharpConfig.Section secRunTime = cfgApp["RunTime Config"];

            }
            #endregion

            #region Loglama Ayarlari
            if (!System.IO.Directory.Exists(strLogFolder))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(strLogFolder);
                }
                catch (Exception excMain)
                {
                    throw new Exception(String.Format("{0} klasörü oluşturulamadı!", strLogFolder), excMain);
                }
            }
            Log.Logger = new LoggerConfiguration()
                            .Enrich.WithProcessId()
                            .Enrich.WithProperty("UserName", System.Security.Principal.WindowsIdentity.GetCurrent().Name)
                            .WriteTo
                            .RollingFile(strLogFolder + @"\" + strLogFileName + "_Logs_" + Environment.MachineName + "_" + Environment.UserName + @"_{Date}.txt", outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] ({UserName})({ProcessId}) {Message}{NewLine}{Exception}", fileSizeLimitBytes: 50000000)
                            .MinimumLevel.Is(lelMain)
                            .CreateLogger();
            #endregion
        }
        public static void ToCSV(this DataTable dtDataTable, string strFilePath)
        {
            StreamWriter sw = new StreamWriter(strFilePath, false);
            //headers    
            for (int i = 0; i < dtDataTable.Columns.Count; i++)
            {
                sw.Write(dtDataTable.Columns[i]);
                if (i < dtDataTable.Columns.Count - 1)
                {
                    sw.Write(",");
                }
            }
            sw.Write(sw.NewLine);
            foreach (DataRow dr in dtDataTable.Rows)
            {
                for (int i = 0; i < dtDataTable.Columns.Count; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                    {
                        string value = dr[i].ToString();
                        if (value.Contains(','))
                        {
                            value = String.Format("{0}", value.Replace(',', '.'));
                            sw.Write(value);
                        }
                        else
                        {
                            sw.Write(dr[i].ToString());
                        }
                    }
                    if (i < dtDataTable.Columns.Count - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write(sw.NewLine);
            }
            sw.Close();
        }
        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            DateTime unixEpoch = DateTime.ParseExact("1970-01-01", "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            DateTime convertedTime = unixEpoch.AddMilliseconds(unixTimeStamp);

            return convertedTime;
        }
        public static bool HaftaGunleri(string periyot)
        {
            bool blnReturn = false;

            switch (periyot)
            {
                case "Pazartesi":
                    blnReturn = true;
                    break;
                case "Salı":
                    blnReturn = true;
                    break;
                case "Çarşamba":
                    blnReturn = true;
                    break;
                case "Perşembe":
                    blnReturn = true;
                    break;
                case "Cuma":
                    blnReturn = true;
                    break;
                case "Cumartesi":
                    blnReturn = false;
                    break;
                case "Pazar":
                    blnReturn = false;
                    break;
            }

            return blnReturn;
        }
        public static string Currency(string Currency)
        {
            string strReturn = "";
            switch (Currency)
            {
                case "AED":
                    strReturn = "د.إ.";
                    break;
                case "AFN":
                    strReturn = "؋";
                    break;
                case "ALL":
                    strReturn = "Lekë";
                    break;
                case "AMD":
                    strReturn = "֏";
                    break;
                case "ANG":
                    strReturn = "NAf.";
                    break;
                case "AOA":
                    strReturn = "Kz";
                    break;
                case "ARS":
                    strReturn = "$";
                    break;
                case "AUD":
                    strReturn = "$";
                    break;
                case "AWG":
                    strReturn = "Afl.";
                    break;
                case "AZN":
                    strReturn = "₼";
                    break;
                case "BAM":
                    strReturn = "КМ";
                    break;
                case "BBD":
                    strReturn = "$";
                    break;
                case "BDT":
                    strReturn = "৳";
                    break;
                case "BGN":
                    strReturn = "лв.";
                    break;
                case "BHD":
                    strReturn = "د.ب.";
                    break;
                case "BIF":
                    strReturn = "FBu";
                    break;
                case "BMD":
                    strReturn = "$";
                    break;
                case "BND":
                    strReturn = "$";
                    break;
                case "BOB":
                    strReturn = "Bs";
                    break;
                case "BRL":
                    strReturn = "R$";
                    break;
                case "BSD":
                    strReturn = "$";
                    break;
                case "BTN":
                    strReturn = "Nu.";
                    break;
                case "BWP":
                    strReturn = "P";
                    break;
                case "BYN":
                    strReturn = "Br";
                    break;
                case "BZD":
                    strReturn = "$";
                    break;
                case "CAD":
                    strReturn = "$";
                    break;
                case "CDF":
                    strReturn = "FC";
                    break;
                case "CHF":
                    strReturn = "CHF";
                    break;
                case "CLP":
                    strReturn = "$";
                    break;
                case "CNY":
                    strReturn = "¥";
                    break;
                case "COP":
                    strReturn = "$";
                    break;
                case "CRC":
                    strReturn = "₡";
                    break;
                case "CUP":
                    strReturn = "$";
                    break;
                case "CVE":
                    strReturn = "​";
                    break;
                case "CZK":
                    strReturn = "Kč";
                    break;
                case "DJF":
                    strReturn = "Fdj";
                    break;
                case "DKK":
                    strReturn = "kr.";
                    break;
                case "DOP":
                    strReturn = "$";
                    break;
                case "DZD":
                    strReturn = "د.ج.";
                    break;
                case "EGP":
                    strReturn = "ج.م.";
                    break;
                case "ERN":
                    strReturn = "Nfk";
                    break;
                case "ETB":
                    strReturn = "Br";
                    break;
                case "EUR":
                    strReturn = "€";
                    break;
                case "FJD":
                    strReturn = "$";
                    break;
                case "FKP":
                    strReturn = "£";
                    break;
                case "GBP":
                    strReturn = "£";
                    break;
                case "GEL":
                    strReturn = "₾";
                    break;
                case "GHS":
                    strReturn = "GH₵";
                    break;
                case "GIP":
                    strReturn = "£";
                    break;
                case "GMD":
                    strReturn = "D";
                    break;
                case "GNF":
                    strReturn = "FG";
                    break;
                case "GTQ":
                    strReturn = "Q";
                    break;
                case "GYD":
                    strReturn = "$";
                    break;
                case "HKD":
                    strReturn = "$";
                    break;
                case "HNL":
                    strReturn = "L";
                    break;
                case "HRK":
                    strReturn = "kn";
                    break;
                case "HTG":
                    strReturn = "G";
                    break;
                case "HUF":
                    strReturn = "Ft";
                    break;
                case "IDR":
                    strReturn = "Rp";
                    break;
                case "ILS":
                    strReturn = "₪";
                    break;
                case "INR":
                    strReturn = "₹";
                    break;
                case "IQD":
                    strReturn = "د.ع.";
                    break;
                case "IRR":
                    strReturn = "ريال";
                    break;
                case "ISK":
                    strReturn = "kr";
                    break;
                case "JMD":
                    strReturn = "$";
                    break;
                case "JOD":
                    strReturn = "د.ا.";
                    break;
                case "JPY":
                    strReturn = "¥";
                    break;
                case "KES":
                    strReturn = "Ksh";
                    break;
                case "KGS":
                    strReturn = "сом";
                    break;
                case "KHR":
                    strReturn = "៛";
                    break;
                case "KMF":
                    strReturn = "CF";
                    break;
                case "KPW":
                    strReturn = "₩";
                    break;
                case "KRW":
                    strReturn = "₩";
                    break;
                case "KWD":
                    strReturn = "د.ك.";
                    break;
                case "KYD":
                    strReturn = "$";
                    break;
                case "KZT":
                    strReturn = "₸";
                    break;
                case "LAK":
                    strReturn = "₭";
                    break;
                case "LBP":
                    strReturn = "ل.ل.";
                    break;
                case "LKR":
                    strReturn = "රු.";
                    break;
                case "LRD":
                    strReturn = "$";
                    break;
                case "LYD":
                    strReturn = "د.ل.";
                    break;
                case "MAD":
                    strReturn = "د.م.";
                    break;
                case "MDL":
                    strReturn = "L";
                    break;
                case "MGA":
                    strReturn = "Ar";
                    break;
                case "MKD":
                    strReturn = "ден";
                    break;
                case "MMK":
                    strReturn = "K";
                    break;
                case "MNT":
                    strReturn = "₮";
                    break;
                case "MOP":
                    strReturn = "MOP$";
                    break;
                case "MRU":
                    strReturn = "MRU";
                    break;
                case "MUR":
                    strReturn = "Rs";
                    break;
                case "MVR":
                    strReturn = "ރ.";
                    break;
                case "MWK":
                    strReturn = "MK";
                    break;
                case "MXN":
                    strReturn = "$";
                    break;
                case "MYR":
                    strReturn = "RM";
                    break;
                case "MZN":
                    strReturn = "MTn";
                    break;
                case "NAD":
                    strReturn = "$";
                    break;
                case "NGN":
                    strReturn = "₦";
                    break;
                case "NIO":
                    strReturn = "C$";
                    break;
                case "NOK":
                    strReturn = "kr";
                    break;
                case "NPR":
                    strReturn = "रु";
                    break;
                case "NZD":
                    strReturn = "$";
                    break;
                case "OMR":
                    strReturn = "ر.ع.";
                    break;
                case "PAB":
                    strReturn = "B";
                    break;
                case "PEN":
                    strReturn = "S";
                    break;
                case "PGK":
                    strReturn = "K";
                    break;
                case "PHP":
                    strReturn = "₱";
                    break;
                case "PKR":
                    strReturn = "Rs";
                    break;
                case "PLN":
                    strReturn = "zł";
                    break;
                case "PYG":
                    strReturn = "₲";
                    break;
                case "QAR":
                    strReturn = "ر.ق.";
                    break;
                case "RON":
                    strReturn = "lei";
                    break;
                case "RSD":
                    strReturn = "дин.";
                    break;
                case "RUB":
                    strReturn = "₽";
                    break;
                case "RWF":
                    strReturn = "RF";
                    break;
                case "SAR":
                    strReturn = "ر.س.";
                    break;
                case "SBD":
                    strReturn = "$";
                    break;
                case "SCR":
                    strReturn = "SR";
                    break;
                case "SDG":
                    strReturn = "ج.س.";
                    break;
                case "SEK":
                    strReturn = "kr";
                    break;
                case "SGD":
                    strReturn = "$";
                    break;
                case "SHP":
                    strReturn = "£";
                    break;
                case "SLL":
                    strReturn = "Le";
                    break;
                case "SOS":
                    strReturn = "S";
                    break;
                case "SRD":
                    strReturn = "$";
                    break;
                case "SSP":
                    strReturn = "£";
                    break;
                case "STN":
                    strReturn = "Db";
                    break;
                case "SYP":
                    strReturn = "ل.س.";
                    break;
                case "SZL":
                    strReturn = "E";
                    break;
                case "THB":
                    strReturn = "฿";
                    break;
                case "TJS":
                    strReturn = "смн";
                    break;
                case "TMT":
                    strReturn = "m.";
                    break;
                case "TND":
                    strReturn = "د.ت.";
                    break;
                case "TOP":
                    strReturn = "T$";
                    break;
                case "TRY":
                    strReturn = "₺";
                    break;
                case "TTD":
                    strReturn = "$";
                    break;
                case "TWD":
                    strReturn = "NT$";
                    break;
                case "TZS":
                    strReturn = "TSh";
                    break;
                case "UAH":
                    strReturn = "₴";
                    break;
                case "UGX":
                    strReturn = "USh";
                    break;
                case "USD":
                    strReturn = "$";
                    break;
                case "UYU":
                    strReturn = "$";
                    break;
                case "UZS":
                    strReturn = "сўм";
                    break;
                case "VES":
                    strReturn = "Bs.S";
                    break;
                case "VND":
                    strReturn = "₫";
                    break;
                case "VUV":
                    strReturn = "VT";
                    break;
                case "WST":
                    strReturn = "WS$";
                    break;
                case "XAF":
                    strReturn = "FCFA";
                    break;
                case "XCD":
                    strReturn = "EC$";
                    break;
                case "XDR":
                    strReturn = "XDR";
                    break;
                case "XOF":
                    strReturn = "CFA";
                    break;
                case "XPF":
                    strReturn = "FCFP";
                    break;
                case "YER":
                    strReturn = "ر.ي.";
                    break;
                case "ZAR":
                    strReturn = "R";
                    break;
                case "ZMW":
                    strReturn = "K";
                    break;
                case "TL":
                    strReturn = "₺";
                    break;
                case "USDT":
                    strReturn = "$";
                    break;
                case "BTC":
                    strReturn = "BTC";
                    break;
                case "ETH":
                    strReturn = "ETH";
                    break;
                case "BNB":
                    strReturn = "BNB";
                    break;
            }

            return strReturn;
        }
        public static DataTable IslemSonucu(string strSorgu)
        {
            DataTable dtReturn = new DataTable();

            try
            {
                using (SqlConnection cnn = new SqlConnection(MSSQLConnectionString()))
                {
                    cnn.Open();

                    dtReturn.Load(cnn.ExecuteReader(strSorgu, (object)null, (IDbTransaction)null, 2500, new CommandType?()));

                    cnn.Close();
                }
            }
            catch
            {
                Environment.Exit(0);
            }

            return dtReturn;
        }
    }
    public class Chat
    {
        public string id { get; set; }
        public string title { get; set; }
        public string type { get; set; }
    }
    public class Entity
    {
        public int offset { get; set; }
        public int length { get; set; }
        public string type { get; set; }
    }
    public class Result
    {
        public int message_id { get; set; }
        public SenderChat sender_chat { get; set; }
        public Chat chat { get; set; }
        public long date { get; set; }
        public string text { get; set; }
        public List<Entity> entities { get; set; }
    }
    public class Root
    {
        public bool ok { get; set; }
        public Result result { get; set; }
    }
    public class SenderChat
    {
        public string id { get; set; }
        public string title { get; set; }
        public string type { get; set; }
    }
    public class TwitData
    {
        public string id { get; set; }
        public string text { get; set; }
    }
    public class TwitRoot
    {
        public TwitData data { get; set; }
    }
    public class TweetsV2Poster
    {
        // ----------------- Fields ----------------

        private readonly ITwitterClient client;

        // ----------------- Constructor ----------------

        public TweetsV2Poster(ITwitterClient client)
        {
            this.client = client;
        }

        public Task<ITwitterResult> PostTweet(TweetV2PostRequest tweetParams)
        {
            return this.client.Execute.AdvanceRequestAsync(
                (ITwitterRequest request) =>
                {
                    var jsonBody = this.client.Json.Serialize(tweetParams);

                    // Technically this implements IDisposable,
                    // but if we wrap this in a using statement,
                    // we get ObjectDisposedExceptions,
                    // even if we create this in the scope of PostTweet.
                    //
                    // However, it *looks* like this is fine.  It looks
                    // like Microsoft's HTTP stuff will call
                    // dispose on requests for us (responses may be another story).
                    // See also: https://stackoverflow.com/questions/69029065/does-stringcontent-get-disposed-with-httpresponsemessage
                    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                    request.Query.Url = "https://api.twitter.com/2/tweets";
                    request.Query.HttpMethod = Tweetinvi.Models.HttpMethod.POST;
                    request.Query.HttpContent = content;
                }
            );
        }
    }
    public class TweetV2PostRequest
    {
        /// <summary>
        /// The text of the tweet to post.
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; } = string.Empty;
    }
    public class TwitDataResponse
    {
        public string ScreenName { get; set; }
        public string id { get; set; }
        public string Url { get; set; }
    }
    public class Choice
    {
        public int index { get; set; }
        public Message message { get; set; }
        public object logprobs { get; set; }
        public string finish_reason { get; set; }
    }
    public class Message
    {
        public string role { get; set; }
        public string content { get; set; }
    }
    public class GPT
    {
        public string id { get; set; }
        public string @object { get; set; }
        public int created { get; set; }
        public string model { get; set; }
        public List<Choice> choices { get; set; }
        public Usage usage { get; set; }
        public object system_fingerprint { get; set; }
    }
    public class Usage
    {
        public int prompt_tokens { get; set; }
        public int completion_tokens { get; set; }
        public int total_tokens { get; set; }
    }
    //
    public class Messagex
    {
        public string role { get; set; }
        public string content { get; set; }
    }
    public class AIMsg
    {
        public string model { get; set; }
        public List<Messagex> messages { get; set; }
    }
}
