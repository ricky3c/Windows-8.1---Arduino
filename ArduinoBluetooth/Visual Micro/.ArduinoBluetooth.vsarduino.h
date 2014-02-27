#ifndef _VSARDUINO_H_
#define _VSARDUINO_H_
//Board = Arduino Leonardo
#define __AVR_ATmega32u4__
#define __AVR_ATmega32U4__
#define USB_VID 0x2341
#define USB_PID 0x8036
#define USB_MANUFACTURER 
#define USB_PRODUCT "\"Arduino Leonardo\""
#define ARDUINO 150
#define ARDUINO_MAIN
#define __AVR__
#define __avr__
#define F_CPU 16000000L
#define __cplusplus
#define __inline__
#define __asm__(x)
#define __extension__
#define __ATTR_PURE__
#define __ATTR_CONST__
#define __inline__
#define __asm__ 
#define __volatile__

#define __builtin_va_list
#define __builtin_va_start
#define __builtin_va_end
#define __DOXYGEN__
#define __attribute__(x)
#define NOINLINE __attribute__((noinline))
#define prog_void
#define PGM_VOID_P int
            
typedef unsigned char byte;
extern "C" void __cxa_pure_virtual() {;}

//
//
void LeerBluetooth();
void Eventos(char* message);
void EnviarMensaje(char* message);
void Mover180();
void Mover0();

#include "C:\Program Files (x86)\Arduino\hardware\arduino\avr\cores\arduino\arduino.h"
#include "C:\Program Files (x86)\Arduino\hardware\arduino\avr\variants\leonardo\pins_arduino.h" 
#include "C:\Users\ricardo\Documents\Arduino\ArduinoBluetooth\ArduinoBluetooth.ino"
#endif
