#include "HTTPClient.h"

int httpResponse;

WiFiClient client;
HTTPClient http;


int PostSingleJsonData(String serverName,String Path, String jsonData)
{    
    http.begin(serverName);
    http.addHeader("Content-Type", "application/json");
    httpResponse = http.POST(jsonData);
    http.end();
    return httpResponse;
}