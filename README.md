## Modbus Gateway

### Summary:
 - Modbus Gateway is an ASP.NET Core application that allows communication between HTTP clients and a Modbus compliant device through a REST API.
 - It allows clients to establish serial connections to Modbus devices and send read and write operation requests.

### Main features:
 - Modbus RTU communication
 - Modbus ASCII communication
 - Management of multiple simultaneous serial connections
 - REST API for Modbus operations
 - Web based user interface for managing connections and sending requests

### Supported Modbus Functions:
 - 01 Read Coils
 - 02 Read Discrete Inputs
 - 03 Read Holding Registers
 - 04 Read Input Registers
 - 05 Write Single Coil
 - 06 Write Single Register
 - 15 Write Multiple Coils
 - 16 Write Multiple Registers

### Technologies used:
 - C#
 - ASP.NET Core

#### Libraries:
 - [NModbus](https://github.com/NModbus/NModbus)
   
#### Frontend:
 - JavaScript
 - HTML & CSS


### Testing Tools:
#### Modbus Emulators:
 - [ModRSsim2](https://www.support.aceautomation.eu/knowledge-base/modrssim2-modbus-slave-server-rtu-tcp-and-df1-simulator/)
 - [ModbusTools](https://github.com/serhmarch/ModbusTools)

#### Virtual Serial Ports Emulators
 - [VSPE](https://eterlogic.com/products.vspe.html)
 - [Virtual Serial Ports](https://freevirtualserialports.com/)

### Known Limitations:
 - The application currently only supports the Modbus functions listed above.
 - Modbus communication is performed synchronously rather than asynchronously. 

### Installation and Usage

#### Prerequisites

Before running the application, make sure you have:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- A Modbus-compliant device with a compatible serial interface, or:
  - A Modbus emulator
  - A virtual serial port pair (e.g. [VSPE](https://eterlogic.com/products.vspe.html))

#### Installation

1. Clone the repository:

   ```bash
   git clone &lt;repository-url&gt;
   ```

2. Navigate to the API project directory:

   ```bash
   cd ModbusGateway_API
   ```

3. Restore the project dependencies:

   ```bash
   dotnet restore
   ```

#### Running the Application

Run the application using the .NET CLI:

```bash
dotnet run
```

Once the application has started, open your browser and navigate to:

- HTTP: `http://localhost:5254`
- HTTPS: `https://localhost:7171`

#### Using the Application

1. Connect your Modbus device or configure your Modbus emulator.
2. Select the desired serial protocol (RTU or ASCII).
3. Configure the serial communication parameters to match your device.
4. Establish a connection.
5. Select a Modbus function and provide the required parameters.
6. Send the request and inspect the response.
