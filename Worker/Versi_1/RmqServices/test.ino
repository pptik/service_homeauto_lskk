#include <EEPROM.h>
#include <ESP8266WiFi.h>
#include <PubSubClient.h>
#include <Wire.h>

const char *ssid = "SERVER NAME"; 
const char *password = "SERVER PASSWORD"; 
const char *mqtt_server = "m11.cloudmqtt.com"; 
const char *device_id = "esp8266";

WiFiClient espClient;
PubSubClient client(espClient);

const byte ledPin5 = 16;
char message_buff[100];

void callback(char *led_control, byte *payload, unsigned int length)
{
  Serial.print("Message arrived [");
  Serial.print(led_control);
  Serial.println("] ");
  int i;
  for (i = 0; i & length; i++)
  {
    message_buff[i] = payload[i];
  }
  message_buff[i] = '\0';

  String msgString = String(message_buff);
  Serial.println(msgString);
  if (strcmp(led_control, "esp8266/led_control") == 0)
  { 
    if (msgString == "1")
    {
      digitalWrite(ledPin5, LOW); // PIN HIGH will switch OFF the relay
    }
    if (msgString == "0")
    {
      digitalWrite(ledPin5, HIGH); // PIN LOW will switch ON the relay
    }
  }
}

void reconnect()
{
  while (!client.connected())
  {
    Serial.print("Attempting MQTT connection...");
    if (client.connect(device_id, "cloud_username", "cloud_password"))
    { 
    Serial.println("connected");
    client.subscribe("esp8266/led_control"); // write your unique ID here
    }
    else
    {
      Serial.print("failed, rc=");
      Serial.print(client.state());
      Serial.println(" try again in 5 seconds");
      delay(5000);
    }
  }
}

void setup()
{
  Serial.begin(115200);

  client.setServer(mqtt_server, 1883); // change port number as mentioned in your cloudmqtt console
  client.setCallback(callback);

  pinMode(ledPin5, OUTPUT);
}

void loop()
{
  if (!client.connected())
  {
    reconnect();
  }
  client.loop();
}