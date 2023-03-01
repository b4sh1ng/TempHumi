namespace Statisitsche_Temp_Erfassung.Handlers
{
    public class HttpRequestHandler
    {
        private string fileName = "dataLog.json";
        private string userDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        private int responseCode;
        private HttpResponseMessage? response;
        private SensorDaten? sensorDaten;
        readonly HttpClient client = new();

        public async Task<int> StartGetData()
        {
            do
            {
                try
                {
                    await GetHttpDataBody(client);
                }
                catch
                {
                    responseCode = 1;
                }
                if (responseCode == 200)
                {
                    if (!Directory.Exists(userDirectory + "/logs"))
                    {
                        DirectoryInfo di = Directory.CreateDirectory(userDirectory + "/logs");
                    }
                    string? fullFileName = userDirectory + "/logs/" + GetDate(sensorDaten) + "_" + fileName;
                    DeleteLastLine(fullFileName);
                    WriteToFile(fullFileName, sensorDaten);
                    using StreamWriter file = new(fullFileName, append: true);
                    file.WriteLine("]");
                }
            } while (responseCode == 200);
            return responseCode;
        }
        private async Task GetHttpDataBody(HttpClient client)
        {
            response = await client.GetAsync("http://192.168.2.183:5000/");             
            responseCode = (int)response.StatusCode;
            string responseData = await response.Content.ReadAsStringAsync();
            if(responseCode == 200)
            { 
            sensorDaten = JsonSerializer.Deserialize<SensorDaten>(responseData);
            }
            //if (sensorDaten is null) throw new Exception();          
            
        }
        private static void WriteToFile(string fileName, SensorDaten data)
        {
            using StreamWriter file = new(fileName, append: true);
            string jsonData = JsonSerializer.Serialize(data);
            if (new FileInfo(fileName).Length == 0)
            {
                file.WriteLine("[\n" + jsonData);
            }
            else
            {
                file.WriteLine(",");
                file.WriteLine(jsonData);
            }
        }
        private static void WriteToFile(string fileName)
        {
            using StreamWriter file = new(fileName, append: true);
            file.Write("");
        }
        private static void DeleteLastLine(string filepath)
        {
            try
            {
                var lines = File.ReadAllLines(filepath);
                File.WriteAllLines(filepath, lines.Take(lines.Length - 1).ToArray());
            }
            catch (Exception)
            {
                WriteToFile(filepath);
            }
        }
        private static string GetDate(SensorDaten data)
        {
            if (data is null)
            {
                return "";
            }
            return data.Date.ToString("yyMMdd");
        }
    }
}
