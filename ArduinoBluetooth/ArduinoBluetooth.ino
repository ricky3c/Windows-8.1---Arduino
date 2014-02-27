//Librerias
#include <Servo.h>
#include "SoftwareSerial.h"
//Variables
Servo myservo;
int Luz = 13;
const int TX_BT = 10;
const int RX_BT = 11;
SoftwareSerial BluetoothSerial(TX_BT, RX_BT);

void setup() {
  // put your setup code here, to run once:
    pinMode(Luz, OUTPUT);
    Serial.begin(9600);
	Serial.println("Serial Iniciado");
	BluetoothSerial.begin(9600);
	Serial.println("Bluetooth Iniciado");
}

void loop() {
  // put your main code here, to run repeatedly:
    LeerBluetooth();
}
//Metodo para recibir el mensaje enviado desde nuestra aplicacion 
void LeerBluetooth()
{
	if (BluetoothSerial.available())
	{
		int commandSize = (int) BluetoothSerial.read();
		char command[commandSize];
		int commandPos = 0;
		while (commandPos < commandSize)
		{
			if (BluetoothSerial.available())
			{
				command[commandPos] = (char) BluetoothSerial.read();
				commandPos++;
			}
		}
		command[commandPos] = 0;
		Eventos(command);
	}
}
//En este metodo se procesa el mensaje
//y dependiendo del mensaje enviado desde la aplicacion 
//realiza una funcion
void Eventos(char* message)
{
	Serial.println(message);
		if ((String) message == "Encender_Luz")
		{
				digitalWrite(Luz, HIGH);
				EnviarMensaje("Luz Encendida");
		}
		if ((String) message == "Apagar_Luz")
		{
				digitalWrite(Luz, LOW);
				EnviarMensaje("Luz Apagada");
		}
			
		if ((String) message == "Mover_Motor")
		{
				Mover180();
				EnviarMensaje("Servo a 180 Grados");
				delay(2000);
				Mover0();
				EnviarMensaje("Servo a 0 Grados");
				delay(2000);
		} 
	 myservo.detach();
	
}
//Envia mensaje a la aplicacion del proceso que esta realizando en el arduino
void EnviarMensaje(char* message)
{
	Serial.print("> ");
	Serial.println(message);
	int messageLen = strlen(message);
	if (messageLen < 256)
	{
		BluetoothSerial.write(messageLen);
		BluetoothSerial.print(message);
	}
}
//Metodos para mover los servos
void Mover180()
{
	myservo.attach(12);
	myservo.write(180);
	delay(15);
}
void Mover0()
{
	myservo.attach(12);
	myservo.write(0);
	delay(15);
}
