 

/*
    Develop by      : Nurman Hariyanto
    Email           : nurman.hariyanto13@gmail.com
    Project         : Homeautomation
    Version         : 3.0
*/

#include <SDM.h>                                                                //import SDM library
#include <SoftwareSerial.h>                                                     //import SoftwareSerial library

SoftwareSerial swSerSDM(D2,D3);                                               //config SoftwareSerial (rx->D7 / tx->D8)
SDM sdm(swSerSDM, 9600, D5);                                                   //config SDM

int loop_count  = 0 ; //loop count loop
/*
   Setup pin and load config file
*/

void setup() {
  Serial.begin(115200);
                                                        //initialize serial
  sdm.begin();
}

void watchdogSetup(void) {
  cli();
  ESP.wdtDisable();
}



//loop publish
void loop() {
char bufout[10];
  sprintf(bufout, "%c[1;0H", 27);
  Serial.print(bufout);

  Serial.print("Voltage:   ");
  Serial.print(sdm.readVal(SDM220T_VOLTAGE), 2);                                //display voltage
  Serial.println("V");

  delay(50);

  Serial.print("Current:   ");
  Serial.print(sdm.readVal(SDM220T_CURRENT), 2);                                //display current  
  Serial.println("A");

  delay(50);

  Serial.print("Power:     ");
  Serial.print(sdm.readVal(SDM220T_POWER), 2);                                  //display power
  Serial.println("W");

  delay(50);

  Serial.print("Frequency: ");
  Serial.print(sdm.readVal(SDM220T_FREQUENCY), 2);                              //display frequency
  Serial.println("Hz");   

  delay(1000);             


}
