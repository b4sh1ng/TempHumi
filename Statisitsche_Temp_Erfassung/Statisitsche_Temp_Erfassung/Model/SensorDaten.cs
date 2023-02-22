using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Text.Json;
using System.IO;

namespace Statisitsche_Temp_Erfassung
{
    public class SensorDaten
    {
        public DateTime Date { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }

        public List<SensorDaten> AktuellsteDatenAuslesen()
        {
            string logDirectory = Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) + "/logs";
            string[] filePaths = Directory.GetFiles(logDirectory, "*.json", SearchOption.TopDirectoryOnly);
            string sensor = File.ReadAllText(filePaths.Last());

            var sensorDaten = JsonSerializer.Deserialize<List<SensorDaten>>(sensor)!;
            return sensorDaten;
        }
    }

}
