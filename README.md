## Modbus Gateway

### Summary:
Modbus Gateway is an ASP.Net Core application that allows communication between an HTTP clients and a Modbus compliant device through a REST API.

It allows clients to establish serial connections to Modbus devices and send read 
and write operation requests.

### Main features:
Modbus RTU communication

Modbus ASCII communication

Management of multiple simultaneous serial connections

REST API for Modbus operations

Web based user interface for managing connections and sending requests

### Supported Modbus Functions:
01 Read Coils

02 Read Discrete Inputs

03 Read Holding Registers

04 Read Input Registers

05 Write Single Coil

06 Write Single Register

15 Write Multiple Coils

16 Write Multiple Registers

### Technologies used:
C#

ASP.NET Core

[NModbus](https://github.com/NModbus/NModbus)

JavaScript

HTML & CSS

### Requirements:
.NET 10.0

A Modbus-compliant device with a compatible serial interface and connection.
Alternatively, a Modbus emulator and a virtual serial port pair.


### Testing Tools:
#### Modbus Emulators:
[ModRSsim2](https://www.support.aceautomation.eu/knowledge-base/modrssim2-modbus-slave-server-rtu-tcp-and-df1-simulator/)

[ModbusTools](https://github.com/serhmarch/ModbusTools)

#### Virtual Serial Ports Emulators
[VSPE](https://eterlogic.com/products.vspe.html)

[Virtual Serial Ports](https://freevirtualserialports.com/)

### Known Limitations:
The application currently only supports the Modbus functions listed above.

Modbus communication is performed synchronously rather than asynchronously. 