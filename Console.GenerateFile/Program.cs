using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Console;
using Console.GenerateFile.Properties;
using System.Globalization;
using log4net;

namespace Console.GenerateFile
{
    class Program
    {
        const string _controller = "/PrintOnServer";
        private static ILog _logger = LogManager.GetLogger("EInvoiceConsole");

        static void Main(string[] args)
        {
            try
            {
                CustomWriteLine("Start processing...");
                var baseDirectory = Settings.Default.BaseDirectory;

                if (Settings.Default.DevelopmentMode)
                {
                    var defaultArguments = new List<string>() { NoteType.RecoveryNote.ToString(), "20200101", "20200131" };
                    args = defaultArguments.ToArray();
                }

                if (args == null || args.Length != 3)
                {
                    CustomWriteLine("Invalid arguments. Please retry.");
                    return;
                }

                var noteType = args[0];
                var fromDate = args[1];
                var toDate = args[2];

                CustomWriteLine($"Arg1:{noteType} Arg2:{fromDate} Arg3:{toDate}");
                var passValidation = CheckArguments(fromDate, toDate);

                if (passValidation)
                {
                    var noteIDs = GetNoteIDs(noteType, fromDate, toDate);
                    var currentDate = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    var fullDirectory = $"{baseDirectory}\\{currentDate}";

                    if (noteIDs != null && noteIDs.Count > 0)
                    {
                        if (noteIDs.First() == -1)
                            CustomWriteLine("API hit exception.");
                        else
                            Directory.CreateDirectory(fullDirectory);
                    }
                    else
                        CustomWriteLine("Transfer ID list is empty.");

                    var actionWithParameter = GetActionWithParameter(noteType);
                    foreach (var noteID in noteIDs)
                    {
                        CustomWriteLine($"Start calling {noteType} with ID: {noteID}");

                        WebRequest request = WebRequest.Create($"{Settings.Default.BaseUrl}{_controller}{actionWithParameter}{noteID}");
                        WebResponse response = request.GetResponse();

                        var fileName = response.Headers["Content-Disposition"].Substring(response.Headers["Content-Disposition"].LastIndexOf("=") + 1).Replace("\"", "");
                        Stream streamWithFileBody = response.GetResponseStream();

                        using (Stream output = File.OpenWrite($"{fullDirectory}\\{fileName}"))
                        {
                            streamWithFileBody.CopyTo(output);
                        }

                        CustomWriteLine($"Finish calling {noteType} with ID: {noteID}");
                    }
                }
            }
            catch (Exception ex)
            {
                CustomWriteLine("Unable to Download");
                CustomWriteLine(ex.ToString());
            }
            finally
            {
                CustomWriteLine("Stop processing...");
                CustomWriteLine("Finish");
                Read();
            }
        }

        private static List<int> GetNoteIDs(string noteType, string fromDate, string toDate)
        {
            using (WebClient wc = new WebClient())
            {
                var json = wc.DownloadString($"{Settings.Default.BaseUrl}{_controller}/PrintOnServerInfo?noteType={noteType}&fromDate={fromDate}&toDate={toDate}");
                return JsonConvert.DeserializeObject<List<int>>(json);
            }
        }

        private static string GetActionWithParameter(string noteType)
        {
            if (noteType == NoteType.SignedDONote.ToString())
            {
                return "/SignedDONote?deliveryID=";
            }
            else if (noteType == NoteType.PlannedPickupNote.ToString())
            {
                return "/PlannedPickupNote?transferID=";
            }
            else //noteType == NoteType.RecoveryNote.ToString()
            {
                return "/RecoveryNote?transferID=";
            }
        }

        private static bool CheckArguments(string fromDate, string toDate)
        {
            DateTime fromDT, toDT;
            if (!DateTime.TryParseExact(fromDate, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDT))
            {
                CustomWriteLine($"FromDate:{fromDate} is invalid");
                return false;
            }

            if (!DateTime.TryParseExact(toDate, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out toDT))
            {
                CustomWriteLine($"ToDate: {toDate} is invalid");
                return false;
            }

            var totalDays = (toDT - fromDT).TotalDays;
            if (-1 >= totalDays || totalDays > 31)
            {
                CustomWriteLine($"Invalid date range for FromDate:{fromDate} and ToDate:{toDate}");
                return false;
            }

            return true;
        }

        private static void CustomWriteLine(string text)
        {
            WriteLine(text);
            _logger.Debug(text);
        }
    }
}
