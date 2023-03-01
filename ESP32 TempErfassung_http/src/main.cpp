#include "Arduino.h"
#include "DHT.h"
#include "DHT_U.h"
#include "myWifi.h"
#include "ArduinoJson.h"
#include "SPI.h"
#include "SD.h"
#include "sdDataDeleter.h"
#include "NTPClient.h"
#include "WiFiUdp.h"
#include "WiFiServer.h"

void HttpHandler();
void MeasurementHandler();

struct LinesAndPositions
{
  int NumberOfLines; // number of lines in file
  int SOL[250];      // start of line in file
  int EOL[250];      // end of line in file
};

String PostData;
String EmptyData;
String fileName = "/dataLog.json";

const long utcOffsetInSeconds = 3600;
unsigned long lastTime = 0;
unsigned long timerDelay = 30000; // alter wert 10000, zukünftiger wert 600000 => 10min
const int chipSelect = 5;
int dhtPin = 3;
int linesInFile;
double humidity, temperature;

DHT dht(dhtPin, DHT22);
File storedData;
WiFiServer server(5000);
WiFiUDP ntpUDP;
NTPClient timeClient(ntpUDP, "de.pool.ntp.org", utcOffsetInSeconds);

void setup()
{
  Serial.begin(115200);
  while (!Serial)
    ;
  connectToWifi(); // Eigene Methode um mit dem Wifi zu verbinden
  dht.begin();     // DHT22 Sensor initialisieren
  pinMode(chipSelect, OUTPUT);
  server.begin();
  if (!SD.begin(chipSelect))
  {
    Serial.println("SD Card initialization failed!");
  }
  delay(500);
  timeClient.begin();
  timeClient.update();
}

void loop()
{
  HttpHandler();
  MeasurementHandler();
}
void HttpHandler()
{
  WiFiClient client = server.available();
  if (client)
  {
    while (client.available())
    {
      String request = client.readStringUntil('\r');
      String requestPath = request.substring(4, request.indexOf(" ", 4));
      if (requestPath == "/")
      {
        storedData = SD.open("/dataLog.json", FILE_READ);
        if (storedData.size() < 1)
        {
          Serial.println("Daten wurden angefragt, aber keine vorhanden. Sende 404!");
          client.println("HTTP/1.1 404 No Data");
          client.println("Content-Type: application/json");
          client.println("");
          client.println("");
          client.stop();          
        }
        else
        {
          String oldestData;
          for (int i = 1; i < 2; i++)
          {
            oldestData = storedData.readStringUntil('\n');
            storedData.close();
            if (i == 1)
            {
              Serial.print("Zu sendenen Daten: ");
              Serial.println(oldestData);
            }
          }
          Serial.println(request);
          client.println("HTTP/1.1 200 OK");
          client.println("Content-Type: application/json");
          client.println("");
          client.println(oldestData);
          client.stop();
          DeleteLineFromFile("/dataLog.json", 1);
        }
      }
    }
  }
}

void MeasurementHandler()
{
  if ((millis() - lastTime) > timerDelay)
  {
    temperature = dht.readTemperature();
    humidity = dht.readHumidity();

    DynamicJsonDocument dataJSON(1024);
    dataJSON["Date"] = timeClient.getFormattedDate();
    dataJSON["Temperature"] = temperature;
    dataJSON["Humidity"] = humidity;
    serializeJson(dataJSON, PostData);

    storedData = SD.open("/dataLog.json");
    if (!(storedData.size() <= 0))
    {
      storedData = SD.open("/dataLog.json", FILE_APPEND);
      storedData.println(PostData);
      storedData.close();
      Serial.println("Daten angefügt");
      PostData = EmptyData;
    }
    else
    {
      storedData = SD.open("/dataLog.json", FILE_WRITE);
      storedData.println(PostData);
      storedData.close();
      Serial.println("Neue Datei erstellt und Daten angefügt");
      PostData = EmptyData;
    }
    lastTime = millis();
  }
}
