/*
   Develop by         : Nurman Hariyanto
   Email              : nurman.hariyanto13@gmail.com
   Project            : PT.LSKK - PPTIK Homeautomation
   Version            : 1.0
   Description        : Control and Monitor device
*/


//HARDWARE
/*
   Arduino             : NodeMCU ESP8266
   Device              : Dispenser
   Type Device         : LSKK-HA-Dispenser
   Version             : 1.0
   Minor               : 1.a
*/

//SOFTWARE
/*
   Library            :  PubSubSlient <2.8.0>
                         ESP8266WiFi
*/


//=================================================================================================

/*
   Include Library
*/
#include <PubSubClient.h>
#include <ESP8266WiFi.h>


/*
   Wifi Variable
*/
const char* ssid = "LSKK_Lantai2";
const char* password = "lskk12345";


/*
   MQTT Variable
*/
const char* mqtt_server     = "192.168.4.62";
const char* mqtt_user       = "/Homeauto:homeauto";
const char* mqtt_pass       = "homeauto12345!";
const char* CL              = "IoT-Local-1";
const char* mqtt_keywords1  = "Aktuator"; //Subscribe
const char* mqtt_keywords2  = "Log"; //Publish


/*
   Device Variable
*/
String statusDevice[3] = {"0", "0", "0"};
String statusDeviceInput[3] = {"0" "0" "0"};
int relay1        = D1 ;
int relay2        = D2 ;
int relay3        = D3 ;
int InputWater    = D4 ;
int InputHot      = D5 ;
int InputCool     = A0 ;



/*
   Message Content Variable
*/
const char* device_guid = "ec8a2bef-95ff-4d52-88a7-e43e863c3aea";
String output_value;
char msg[100];
char sPayload[100];
char message [40] ;
char address[40];


/*
   initiate function wifi and pubsub mqtt
*/
WiFiClient espClient;
PubSubClient client(espClient);
byte mac[6];
String MACAddress;


/*
   Setup PIN and try to connect wifi
*/
void setup()
{
  pinMode(D1, OUTPUT);
  pinMode(D2, OUTPUT);
  pinMode(D3, OUTPUT);
  pinMode(D4, INPUT_PULLUP);
  pinMode(D5, INPUT_PULLUP);
  pinMode(A0, INPUT);
  digitalWrite(D1, HIGH);
  digitalWrite(D2, HIGH);
  digitalWrite(D3, HIGH);

  //pinMode(input, INPUT);
  Serial.begin(115200);
  setup_wifi();
  printMACAddress();
  client.setServer(mqtt_server, 1883);
  client.setCallback(callback);
}


/*
   Set up wifi connection
*/
void setup_wifi() {
  delay(10);
  // We start by connecting to a WiFi network
  Serial.println();
  Serial.print("Connecting to ");
  Serial.println(ssid);
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  Serial.println("");
  Serial.println("WiFi connected");
  Serial.println("IP address: ");
  Serial.println(WiFi.localIP());
}


/*
   Convert mac value from byte to string
*/
String mac2String(byte ar[]) {
  String s;
  for (byte i = 0; i < 6; ++i)
  {
    char buf[3];
    sprintf(buf, "%2X", ar[i]);
    s += buf;
    if (i < 5) s += ':';
  }
  return s;
}


/*
   Print Mac Address
*/
void printMACAddress() {
  WiFi.macAddress(mac);
  MACAddress = mac2String(mac);
  Serial.println(MACAddress);
}



/*
   Callback
*/
void callback(char* topic, byte* payload, unsigned int length) {
  memcpy(sPayload, payload, length);
  memcpy(address, payload, 36);
  memcpy(message, &payload[37], length - 37);
  Serial.print("Message arrived [");
  Serial.print(sPayload);
  Serial.println("] ");

  Serial.println(device_guid);
  Serial.println(address);
  if (String((char *)address) == String((char *)device_guid))
  {
    Serial.println("address sama");
  }
  else
  {
    Serial.println("address berbeda");
    return;
  }

  Serial.println(message);

  if (message[0] == '1') {
    digitalWrite(relay1, HIGH);
    Serial.println("relay 1 on");
    statusDevice[0] = "1";

  }
  if (message[0] == '0') {
    digitalWrite(relay1, LOW);
    Serial.println("relay 1 off");
    statusDevice[0] = "0";
  }

  //relay 2
  if (message[1] == '1') {
    digitalWrite(relay2, HIGH);
    Serial.println("relay 2 on");
    statusDevice[1] = "1";

  }
  if (message[1] == '0') {
    digitalWrite(relay2, LOW);
    Serial.println("relay 2 off");
    statusDevice[1] = "0";
  }

  //relay 3
  if (message[2] == '1') {
    digitalWrite(relay3, HIGH);
    Serial.println("relay 3 on");
    statusDevice[2] = "1";

  }
  if (message[2] == '0') {
    digitalWrite(relay3, LOW);
    Serial.println("relay 3 off");
    statusDevice[2] = "0";
  }

}


/*
   Reconncet MQTT
*/
void reconnect() {
  // Loop until we're reconnected
  printMACAddress();
  const char* CL;
  CL = MACAddress.c_str();
  Serial.println(CL);
  while (!client.connected()) {
    Serial.print("Attempting MQTT connection...");
    // Attempt to connect
    if (client.connect(CL, mqtt_user, mqtt_pass)) {
      Serial.println("connected");
      client.subscribe(mqtt_keywords1);
    } else {
      Serial.print("failed, rc=");
      Serial.print(client.state());
      Serial.println(" try again in 5 seconds");
      ESP.restart();
      delay(5000);

    }
  }
}

/*
   Loop program, check connection and send final status led empty,hot and cool
*/
void loop() {
  if (!client.connected()) {
    reconnect();
  }
  client.loop();
  int oldstateInput[3] = {0, 0, 0};
  int statWater = digitalRead(InputWater);
  int statHot = digitalRead(InputHot);
  int statCool = analogRead(InputCool);
  String dataled4 = "0";
  String dataled5 = "0";

  if (statWater == '1') {
    statusDeviceInput[0] = "0";

  }

  if (statWater == '0') {
    statusDeviceInput[0] = "1";
  }

  if (statHot == '1') {
    statusDeviceInput[1] = "1";

  }

  if (statHot == '0') {
    statusDeviceInput[1] = "0";
  }

  if (statCool >= 0 && statCool <= 620) {
    statusDeviceInput[2] = "0";
  }

  if (statCool >= 620 && statCool <= 1024) {
    statusDeviceInput[2] = "1";
  }

  String dataSend = String( device_guid) + "#" + String(statusDeviceInput[0] + statusDeviceInput[1] + statusDeviceInput[2]);
  char dataStatusInput[300];
  dataSend.toCharArray(dataStatusInput, sizeof(dataStatusInput));
  Serial.println(dataStatusInput);
  if (client.publish(mqtt_keywords2, dataStatusInput) == true) {
    Serial.println("Success sending message");
    Serial.println(dataStatusInput);
  } else {
    Serial.println("Error sending message");
  }
  delay(3000);


}
