using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Text.Json;
using System.IO;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Statisitsche_Temp_Erfassung
{
    public class SensorDaten
    {
        public DateTime Date { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }

        private string logDirectory;

        public SensorDaten()
        {
            logDirectory = Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) + "\\logs";
        }
        public List<SensorDaten> AktuellsteDatenAuslesen()
        {
            string[] filePaths = GetFiles();
            string sensor = File.ReadAllText(filePaths.Last());
            var sensorDaten = JsonSerializer.Deserialize<List<SensorDaten>>(sensor)!;
            /*            List<DateTime> dates = new();
                        foreach (string item in filePaths)
                        {
                            string temp;
                            temp = Path.GetFileName(item);
                            temp = temp.Substring(0, 6);
                            DateTime date;
                            if (DateTime.TryParseExact(temp, "ddMMyy", null, System.Globalization.DateTimeStyles.AssumeLocal, out date))
                            {
                                dates.Add(date);
                            }
                        }
                        Calendar calendar = CultureInfo.InvariantCulture.Calendar;
                        var weeks = dates.GroupBy(d => calendar.GetWeekOfYear(d, CalendarWeekRule.FirstDay, DayOfWeek.Monday));
                        List<List<List<SensorDaten>>> list = new();

                        foreach (var week in weeks)
                        {
                            List<List<SensorDaten>> weekData = new List<List<SensorDaten>>();
                            foreach (var day in week)
                            {
                                List<SensorDaten> dayData = new List<SensorDaten>();
                                foreach (var hour in Enumerable.Range(0, 24))
                                {
                                    var startTime = new DateTime(day.Year, day.Month, day.Day, hour, 0, 0);
                                    var endTime = startTime.AddHours(1);
                                    var hourMeasurements = sensorDaten.Where(m => m.Date >= startTime && m.Date < endTime); // Filtern welche Daten in eine Stunde sind                        
                                    dayData.AddRange(hourMeasurements);
                                }
                                weekData.Add(dayData); // Die Täglichen Daten in die Wöchentliche Liste eintragen
                            }
                            list.Add(weekData); // Alle Daten in die Liste eintragen
                        }*/
            return sensorDaten;
        }

        public List<List<List<SensorDaten>>> GetDataa()
        {
            string[] filePaths = GetFiles();
            string sensor = File.ReadAllText(filePaths.Last());
            var sensorDaten = JsonSerializer.Deserialize<List<SensorDaten>>(sensor)!;
            List<DateTime> dates = new();
            foreach (string item in filePaths)
            {
                string temp;
                temp = Path.GetFileName(item);
                temp = temp.Substring(0, 6);
                DateTime date;
                if (DateTime.TryParseExact(temp, "ddMMyy", null, System.Globalization.DateTimeStyles.AssumeLocal, out date))
                {
                    dates.Add(date);
                }
            }
            Calendar calendar = CultureInfo.InvariantCulture.Calendar;
            var weeks = dates.GroupBy(d => calendar.GetWeekOfYear(d, CalendarWeekRule.FirstDay, DayOfWeek.Monday));
            List<List<List<SensorDaten>>> list = new();

            foreach (var week in weeks)
            {
                List<List<SensorDaten>> weekData = new List<List<SensorDaten>>();
                foreach (var day in week)
                {
                    List<SensorDaten> dayData = new List<SensorDaten>();
                    foreach (var hour in Enumerable.Range(0, 24))
                    {
                        var startTime = new DateTime(day.Year, day.Month, day.Day, hour, 0, 0);
                        var endTime = startTime.AddHours(1);
                        var hourMeasurements = sensorDaten.Where(m => m.Date >= startTime && m.Date < endTime); // Filtern welche Daten in eine Stunde sind                        
                        dayData.AddRange(hourMeasurements);
                    }
                    weekData.Add(dayData); // Die Täglichen Daten in die Wöchentliche Liste eintragen
                }
                list.Add(weekData); // Alle Daten in die Liste eintragen
            }
            return list;
        }

        /* public List<SensorDaten> HourlyData()
         {
             string[] filePaths = GetFiles();
             List<DateTime> dates = new();
             foreach (string item in filePaths)
             {
                 string temp;
                 temp = Path.GetFileName(item);
                 temp = temp.Substring(0, 6);
                 DateTime date;
                 if (DateTime.TryParseExact(temp, "ddMMyy", null, System.Globalization.DateTimeStyles.AssumeLocal, out date))
                 {
                     dates.Add(date);
                 }
             }
             Calendar calendar = CultureInfo.InvariantCulture.Calendar;
             var weeks = dates.GroupBy(d => calendar.GetWeekOfYear(d, CalendarWeekRule.FirstDay, DayOfWeek.Monday));
             List<List<DateTime>> list = new();
             List<string> calendarWeek = new();

             foreach (var week in weeks)
             {                
                 List<DateTime> weekDates = new();
                 foreach (var day in week)
                 {
                     weekDates.Add(day);
                 }
                 list.Add(weekDates);
             }
             return new List<SensorDaten>();
         }*/


        public List<SensorDaten> DailyData()
        {
            string[] filePaths = GetFiles();

            return new List<SensorDaten>();
        }
        public List<SensorDaten> WeeklyData()
        {
            string[] filePaths = GetFiles();

            return new List<SensorDaten>();
        }

        private string[] GetFiles()
        {
            return Directory.GetFiles(logDirectory, "*.json", SearchOption.TopDirectoryOnly);
        }
    }

}
