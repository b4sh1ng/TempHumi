#include "WiFi.h"

const char *ssid = "DrWho";
const char *password = "Q5cb1hn0hve1)";

void connectToWifi()
{
    WiFi.mode(WIFI_STA);
    WiFi.begin(ssid, password);

    Serial.print("Wifi Verbindung wird aufgebaut...");
    while (WiFi.waitForConnectResult() != WL_CONNECTED) // Warten auf Wifi Verbindung.
    {
        delay(500);
        Serial.print(".");
    }
    Serial.println("Verbunden!");
    Serial.print("IP Adresse: ");
    Serial.println(WiFi.localIP());
}

bool wifiIsConnected()
{
    return (WiFi.status() == WL_CONNECTED);
}