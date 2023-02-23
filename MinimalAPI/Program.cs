using System;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

public class Program
{
    private static void Main(string[] args)
    {
        string fileName = "dataLog.json";
        string userDirectory = Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.MapPost("/", (Clima clima) =>
            {
                if (!Directory.Exists(userDirectory + "/logs"))
                {
                    DirectoryInfo di = Directory.CreateDirectory(userDirectory + "/logs");
                }
                string? fullFileName = userDirectory + "/logs/" + GetDate(clima) + "_" + fileName;
                DeleteLastLine(fullFileName);
                WriteToFile(fullFileName, clima);
                using StreamWriter file = new(fullFileName, append: true);
                file.WriteLine("]");
                return Results.Created("/Ok", clima);
            });

        app.MapPost("/MultipleData", ([FromBody] List<Clima> clima) =>
        {
            DeleteLastLine(fileName);
            foreach (var item in clima)
            {
                WriteToFile(fileName, item);
            }
            using StreamWriter file = new(fileName, append: true);
            file.WriteLine("]");
            return Results.Created("/Ok", clima);
        });
        app.Run();
    }
    public static void WriteToFile(string fileName, Clima climaObject)
    {
        using StreamWriter file = new(fileName, append: true);
        string jsonData = JsonSerializer.Serialize(climaObject);
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
    public static void WriteToFile(string fileName)
    {
        using StreamWriter file = new(fileName, append: true);
        file.Write("");
    }
    public static void DeleteLastLine(string filepath)
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

    public static string GetDate(Clima clima)
    {
        if (clima is null)
        {
            return "";
        }
        return clima.Date.ToString("ddMMyy");
    }
}