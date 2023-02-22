#include "Arduino.h"
#include "DHT.h"
#include "DHT_U.h"
#include "myWifi.h"
#include "SendHttpRequest.h"
#include "ArduinoJson.h"
#include "SPI.h"
#include "SD.h"
#include "sdDataDeleter.h"
#include "NTPClient.h"
#include "WiFiUdp.h"

struct LinesAndPositions
{
  int NumberOfLines; // number of lines in file
  int SOL[250];      // start of line in file
  int EOL[250];      // end of line in file
};

String serverName = "http://api.crondust.com";
String PostData;
String EmptyData;
String fileName = "/dataLog.json";

const long utcOffsetInSeconds = 3600;
unsigned long lastTime = 0;
unsigned long timerDelay = 5000; // alter wert 10000, zukünftiger wert 900000
unsigned int dataLength = PostData.length();
const int chipSelect = 5;

int linesInFile;
int httpResponseCode;
int dhtPin = 3;

float humidity, temperature;

DHT dht(dhtPin, DHT22);
File storedData;
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
  if ((millis() - lastTime) > timerDelay)
  {
    if (wifiIsConnected())
    {
      temperature = dht.readTemperature();
      humidity = dht.readHumidity();
      float tempRound = truncf(temperature * 10.0) / 10;
      float humiRound = truncf(humidity * 10.0) / 10;

      DynamicJsonDocument dataJSON(1024);
      dataJSON["Date"] = timeClient.getFormattedDate();
      dataJSON["Temperature"] = tempRound;
      dataJSON["Humidity"] = humiRound;
      serializeJson(dataJSON, PostData);

      storedData = SD.open("/dataLog.json");
      if (!(storedData.size() <= 0))
      {
        LinesAndPositions x = FindLinesAndPositions("/dataLog.json");
        linesInFile = x.NumberOfLines;
        DeleteLineFromFile("/dataLog.json", linesInFile - 1); // delete last line with "]"
        storedData = SD.open("/dataLog.json", FILE_APPEND);
        storedData.println(",\n" + PostData + "\n]");
        storedData.close();
        Serial.println("Daten angefügt");

        storedData = SD.open("/dataLog.json");
        while (storedData.size() > 8)
        {
          storedData = SD.open("/dataLog.json", FILE_READ);
          String oldestData;
          for (int i = 1; i < 3; i++)
          {
            oldestData = storedData.readStringUntil('\n');
            if (i == 2)
            {
              Serial.print("Zu sendenen Daten: ");
              Serial.println(oldestData);
              httpResponseCode = PostSingleJsonData(serverName, "", oldestData);
            }
          }
          storedData.close();
          if (httpResponseCode != 201)
          {
            Serial.println("Daten konnten nicht gesendet werden! API down?");
            break;
          }
          DeleteMultipleLinesFromFile("/dataLog.json", 2, 3);
          Serial.print("Deletet: ");
          Serial.println(oldestData);
          storedData = SD.open("/dataLog.json");
        }
      }
      else
      {
        httpResponseCode = PostSingleJsonData(serverName, "", PostData);
        if (httpResponseCode != 201)
        {
          storedData = SD.open("/dataLog.json", FILE_WRITE);
          storedData.println("[\n" + PostData + "\n]");
          storedData.close();
        }
        Serial.println("In neue Datei geschrieben.");
      }
      PostData = EmptyData;

      Serial.print("Response Code: ");
      Serial.println(httpResponseCode);
    }
    // dataLength = PostData.length();
    lastTime = millis();
  }
}
