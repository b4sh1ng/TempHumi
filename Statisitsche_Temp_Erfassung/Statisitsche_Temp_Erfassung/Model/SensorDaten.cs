using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Text.Json;
using System.IO;
using System.Globalization;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

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
            var sensorDaten = System.Text.Json.JsonSerializer.Deserialize<List<SensorDaten>>(sensor)!;
            return sensorDaten;
        }

        public List<List<List<List<SensorDaten>>>> GetMeasurementData()
        {
            // Die äusserste Liste beschreibt die Liste selbst darin gibt es eine Liste mit den KWs
            // und in den KWs sind nochmal die einzelnen Tagen in der Woche
            // und darin sind die Messwerte nochmal so strukturiert das diese alle Messwerte die in
            // einer Stunde getätigt wurden sich darin befinden.

            List<List<List<List<SensorDaten>>>> messwerte = new List<List<List<List<SensorDaten>>>>();
            string directory = "C:\\Users\\Bash\\logs";
            string[] files = Directory.GetFiles(directory);

            // Messwerte aus den JSON-Dateien auslesen und in die verschachtelte Liste einfügen
            foreach (string file in files)
            {
                string json = File.ReadAllText(file);
                List<SensorDaten> measurements = JsonConvert.DeserializeObject<List<SensorDaten>>(json);
                if (measurements is null)
                {
                    break;
                }
                foreach (SensorDaten measurement in measurements)
                {
                    DateTime timepoint = measurement.Date;
                    int cw = GetCalendarweek(timepoint);
                    int day = (int)timepoint.DayOfWeek;
                    int hour = timepoint.Hour;
                    // Liste für die WeekTree anlegen
                    while (messwerte.Count < cw)
                    {
                        messwerte.Add(new List<List<List<SensorDaten>>>());
                    }
                    // Liste für den Tag anlegen
                    while (messwerte[cw - 1].Count < 7)
                    {
                        messwerte[cw - 1].Add(new List<List<SensorDaten>>());
                    }
                    // Liste für die Stunde anlegen
                    while (messwerte[cw - 1][day].Count < 24)
                    {
                        messwerte[cw - 1][day].Add(new List<SensorDaten>());
                    }
                    // SensorDaten hinzufügen
                    measurement.Date = timepoint;
                    messwerte[cw - 1][day][hour].Add(measurement);
                }
            }
            return messwerte;
        }
        public static int GetCalendarweek(DateTime dateTime)
        {
            return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

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
