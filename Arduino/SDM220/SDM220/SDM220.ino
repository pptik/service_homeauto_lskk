
/*
    Develop by      : Nurman Hariyanto
    Email           : nurman.hariyanto13@gmail.com
    Project         : Homeautomation
    Version         : 3.0
*/

#include <SDM.h>                                                                //import SDM library
#include <SoftwareSerial.h>                                                     //import SoftwareSerial library
#include <PubSubClient.h>
#include <ESP8266WiFi.h>

SoftwareSerial swSerSDM(D7, D8);                                              //config SoftwareSerial (rx->D7 / tx->D8)
SDM sdm(swSerSDM, 9600, D5);                                                   //config SDM


const char* ssid = "LSKKHomeAuto";
const char* password = "1234567890";
const char* mqtt_server = "167.205.7.226";
const char* mqtt_user = "/kwhmeter:kwhmeter";
const char* mqtt_pass = "!!_kwhmeter";
const char* CL = "IoT-Local-1";
const char* mqtt_topic_pub = "Log";
const char* mqtt_topic_sub = "Aktuator";
int loop_count  = 0 ; //loop count loop
String messageSend = "";

String statusDevice = "0";
int relay1 = D6 ;

const char* device_guid = "dbcc7974-87cf-427c-915e-02f0df2c38e1";
String output_value;

char msg[100];
WiFiClient espClient;
PubSubClient client(espClient);

byte mac[6];
String MACAddress;


/*
   Setup pin and load config file
*/
void watchdogSetup(void) {
  ESP.wdtDisable();
}


void setup() {
  Serial.begin(115200);
  //initialize serial
  
  sdm.begin();
  setup_wifi();
  printMACAddress();
  client.setServer(mqtt_server, 1883);
  client.setCallback(callback);
  pinMode(D6, OUTPUT);
  digitalWrite(D6, HIGH);
  delay(1000);
  watchdogSetup();
}




void setup_wifi() {
  delay(10);
  // We start by connecting to a WiFi network
  Serial.println();
  Serial.print("Connecting to ");
  Serial.println(ssid);
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED) {
    delay(2000);
    Serial.print(".");
  }
  Serial.println("");
  Serial.println("WiFi connected");
  Serial.println("IP address: ");
  Serial.println(WiFi.localIP());
}

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

void printMACAddress() {
  WiFi.macAddress(mac);
  MACAddress = mac2String(mac);
  Serial.println(MACAddress);
}

char sPayload[100];
char message [40] ;
char address[40];
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
    statusDevice = "1";

  }
  if (message[0] == '0') {
    digitalWrite(relay1, LOW);
    Serial.println("relay 1 off");
    statusDevice = "0";
  }

}


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
      client.subscribe(mqtt_topic_sub);
    } else {
      Serial.print("failed, rc=");
      Serial.print(client.state());
      Serial.println(" try again in 5 seconds");
      ESP.restart();
      delay(5000);

    }
  }
}




//loop publish
void loop() {
  for (int i = 0; i <= loop_count; i++) {
    if (!client.connected()) {
      reconnect();
    }

    client.loop();
  }

  loop_count++;
  ESP.wdtFeed();
  Serial.print(loop_count);
  Serial.print(". Watchdog fed in approx. ");
  Serial.print(loop_count * 500);
  Serial.println(" milliseconds.");

  float dataVoltage = (sdm.readVal(SDM220T_VOLTAGE));
  float dataCurrent = (sdm.readVal(SDM220T_CURRENT));
  float dataPower   = (sdm.readVal(SDM220T_POWER));
  float dataActiveApparentPower = (sdm.readVal(SDM220T_ACTIVE_APPARENT_POWER));
  float dataReactiveApparentPower = (sdm.readVal(SDM220T_REACTIVE_APPARENT_POWER));
  float dataPowerFactor = (sdm.readVal(SDM220T_POWER_FACTOR));
  float dataPhaseAngle = (sdm.readVal(SDM220T_PHASE_ANGLE));
  float dataFrequency = (sdm.readVal(SDM220T_FREQUENCY));
  float dataImportActiveEnergy = (sdm.readVal(SDM220T_IMPORT_ACTIVE_ENERGY));
  float dataExportActiveEnergy = (sdm.readVal(SDM220T_EXPORT_ACTIVE_ENERGY));
  float dataImportReactiveEnergy = (sdm.readVal(SDM220T_IMPORT_REACTIVE_ENERGY));
  float dataExportReactiveEnergy = (sdm.readVal(SDM220T_EXPORT_REACTIVE_ENERGY));
  float dataTotalActiveEnergy = (sdm.readVal(SDM220T_TOTAL_ACTIVE_ENERGY));
  float dataTotalReactiveEnergy = (sdm.readVal(SDM220T_TOTAL_REACTIVE_ENERGY));


  messageSend = String(device_guid) + "#" + dataVoltage + "#" + dataCurrent + "#" + dataPower + "#" + dataActiveApparentPower + "#" + dataReactiveApparentPower + "#" + dataPowerFactor + "#" + dataPhaseAngle + "#" + dataFrequency + "#" + dataImportActiveEnergy + "#" + dataExportActiveEnergy + "#" + dataImportReactiveEnergy + "#" + dataExportReactiveEnergy + "#" + dataTotalActiveEnergy + "#" + dataTotalReactiveEnergy; 
  
  if (client.publish(mqtt_topic_pub,messageSend.c_str()) == true){
       Serial.println("Success sending message");
  } 
  else {
    Serial.println("Error sending message");
  }
    
  
  
  Serial.println(messageSend);
  delay(500);


}
