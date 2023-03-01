namespace Statisitsche_Temp_Erfassung.Models
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
            return sensorDaten;
        }
        public List<List<List<List<SensorDaten>>>> GetMeasurementData()
        {
            // Die äusserste Liste beschreibt die Liste selbst darin gibt es eine Liste mit den KWs
            // und in den KWs sind nochmal die einzelnen Tage der Woche
            // und darin sind die Messwerte nochmal nach der Uhrzeit sortiert.

            List<List<List<List<SensorDaten>>>> messwerte = new();
            string directory = logDirectory;
            string[] files = Directory.GetFiles(directory);

            // Messwerte aus den JSON-Dateien auslesen und in die verschachtelte Liste einfügen
            foreach (string file in files)
            {
                string jsonData = File.ReadAllText(file);
                List<SensorDaten>? measurements = JsonSerializer.Deserialize<List<SensorDaten>>(jsonData);
                if (measurements is null) break; 
                foreach (var measurement in measurements)
                {
                    DateTime timepoint = measurement.Date;
                    int cw = GetCalendarweek(timepoint);
                    int day = (int)timepoint.DayOfWeek;
                    int hour = timepoint.Hour;
                    // Liste für die Woche anlegen
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
                    messwerte[cw - 1][day][hour].Add(measurement);
                }
            }
            return messwerte;
        }
        public List<List<List<List<SensorDaten>>>> RemoveEmptyLists(List<List<List<List<SensorDaten>>>> data)
        {
            List<List<List<List<SensorDaten>>>> cleanedData = new List<List<List<List<SensorDaten>>>>();
            foreach (var week in data)
            {
                List<List<List<SensorDaten>>> cleanedWeek = new List<List<List<SensorDaten>>>();
                foreach (var day in week)
                {
                    List<List<SensorDaten>> cleanedDay = new List<List<SensorDaten>>();
                    foreach (var hour in day)
                    {
                        if (hour.Count > 0)
                        {
                            cleanedDay.Add(hour);
                        }
                    }
                    if (cleanedDay.Count > 0)
                    {
                        cleanedWeek.Add(cleanedDay);
                    }
                }
                if (cleanedWeek.Count > 0)
                {
                    cleanedData.Add(cleanedWeek);
                }
            }
            return cleanedData;
        }
        public static int GetCalendarweek(DateTime dateTime)
        {
            return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }
        private string[] GetFiles()
        {
            return Directory.GetFiles(logDirectory, "*.json", SearchOption.TopDirectoryOnly);
        }
    }

}
