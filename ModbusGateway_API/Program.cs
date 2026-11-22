var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<ModbusExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();

ModbusConnectionManager connectionManager =
    new ModbusConnectionManager();

//These constants are the starting addresses for each modbus register type
//They are used in order to return the full address of each memory location 
//to the front end
//Note: these addresses were based on the ModRSSim2 emulator.
const int CoilBase = 000001;
const int DigitalInputBase = 100001;
const int AnalogInputBase = 300001;
const int HoldingRegisterBase = 400001;

//Note: The modbus implementation guide (linked below)
//clarifies that slave address (called slaveId in this codebase) 
//uses a number between 1 and 247 with 248-255 
//being reserved by the protocol and 0 being the broadcast address.
//For this implementation we only care about the addresses between 1 and 247,
//therefore the following code only checks for these limits.
//*https://www.modbus.org/file/secure/modbusoverserial.pdf

//Endpoint for establishing a client connection
app.MapPost(
    "/connections",
    (ModbusConnectionOptions options) =>
    {
        string error = "";

        //Check if the data to configure a serial connection makes sense
        if(!SerialConnectionValidator
            .Validate(options, out error))
        {
            return Results.BadRequest(error);
        }

        //Create a new connection using the connection manager and save its id
        string connectionId = connectionManager.Connect(options);

        var response = new
        {
            connectionId,
            protocol = options.Protocol,
            comPort  = options.ComPort,
            baudRate = options.BaudRate,
            parity   = options.Parity,
            dataBits = options.DataBits,
            stopBits = options.StopBits
        };
        return Results.Created($"/connections/{connectionId}", response);
    });

//Endpoint for terminating a client connection
app.MapDelete(
    "/connections/{connectionId}",
    (string connectionId) =>
    {
        //Ask the connection manager to release the connection
        bool disconnected = connectionManager.Disconnect(connectionId);

        //Check if the operation succeeded 
        if(!disconnected)
        {
            //Return 404 not found if the operation failed
            return Results.NotFound("Connection was not found.");
        }

        //Return 204 no content if successful
        return Results.NoContent();
    });

//Endpoint for releasing all open connections
app.MapDelete(
    "/connections", () =>
    {
        connectionManager.DisconnectAll();
        return Results.NoContent();
    });

//Endpint that returns a list of active connections
app.MapGet(
    "/connections", 
    () =>
    {
        return Results.Ok(connectionManager.GetConnectionInfo());
    });


//Endpoint that returns the state of a connection
app.MapGet("/connections/{connectionId}",
    (string connectionId) =>
    {
        ModbusClient? client = connectionManager.GetConnection(connectionId);

        if (client == null)
        {
            return Results.NotFound("Connection not found.");
        }

        return Results.Ok(new
        {
            connectionId,
            comPort = client.ComPort,
            isConnected = client.isConnected
        });
    });

//Endpoint for Read Coils (0x01)
app.MapGet(
    "/connections/{connectionId}" +
    "/devices/{slaveId}/coils/",
    (string connectionId, 
    byte slaveId, ushort address, ushort count) =>
    {
        ModbusClient? client = connectionManager.GetConnection(connectionId);

        //Create a request from the received data
        var request = new ReadCoilsRequest
        {
            SlaveId = slaveId,
            Address = address,
            Count = count
        };

        //Validate request
        string error = "";
        if(!RequestValidator
        .ValidateReadCoils(request, out error))
        {
            return Results.BadRequest(new
            {
                error = "Invalid request",
                message = error
            });
        }

        //Check if the client is connected
        if(client == null || !client.isConnected)
        {
            return Results.Conflict("Not connected.");
        }

        //Send the request to the client 
        bool[] result =
            client!.ReadCoils(
                request.SlaveId,
                request.Address,
                request.Count);

        //Format the response
        var response = 
        ModbusResponseFormatter.Format(
                CoilBase,
                request.Address,
                result);

        //Return the response to the front end
        return Results.Ok(response);
    });

//Endpoint for Read Discrete Inputs (0x02)
app.MapGet(
    "/connections/{connectionId}" +
    "/devices/{slaveId}/discrete-inputs",
    (string connectionId,
    byte slaveId, ushort address, ushort count) =>
    {
        //Get the correct client based on the id given in the url
        ModbusClient? client = connectionManager.GetConnection(connectionId);

        //Check if the client is connected
        if (client == null || !client.isConnected)
        {
            return Results.Conflict("Not connected.");
        }

        //Create a request object from the received data
        var request = new ReadInputsRequest
        {
            SlaveId = slaveId,
            Address = address,
            Count = count
        };

        //Validate request
        string error = "";
        if(!RequestValidator
        .ValidateReadDiscreteInputs(
            request, out error))
        {
            return Results.BadRequest(new
            {
                error = "Invalid Request.",
                message = error
            });
        }


        //Send the request to the client
        bool[] result = client.ReadDiscreteInputs(
                request.SlaveId,
                request.Address,
                request.Count);

        //Format the response
        var response =
            ModbusResponseFormatter.Format(
                DigitalInputBase,
                request.Address,
                result);

        //Return the response to the front end
        return Results.Ok(response);
    });

//Endpoint for Read Holding Registers (0x03)
app.MapGet(
    "/connections/{connectionId}" +
    "/devices/{slaveId}/holding-registers",
    (string connectionId,
    byte slaveId, ushort address, ushort count) =>
    {
        //Get the correct client based on the id given in the url
        ModbusClient? client = connectionManager.GetConnection(connectionId);

        //Check if the client is connected
        if (client == null || !client.isConnected)
        {
            return Results.Conflict("Not connected.");
        }

        //Create a request object from the received data
        var request = new ReadHoldingRegistersRequest
        {
            SlaveId = slaveId,
            Address = address,
            Count = count
        };

        //Validate received data 
        string error = "";
        if (!RequestValidator
        .ValidateReadHoldingRegisters(
            request, out error))
        {
            return Results.BadRequest(new
            {
                error = "Invalid Request.",
                message = error
            });
        }

        //Send the request to the client
        ushort[] result = 
            client.ReadHoldingRegisters(
                request.SlaveId,
                request.Address,
                request.Count);

        //Format the response
        var response =
            ModbusResponseFormatter
            .Format(
                HoldingRegisterBase,
                request.Address,
                result);

        //Return the response to the front end
        return Results.Ok(response);
    });

//Endpoint for Read Input Registers (0x04)
app.MapGet(
    "/connections/{connectionId}" +
    "/devices/{slaveId}/input-registers",
    (string connectionId, 
    byte slaveId, ushort address, ushort count) =>
    {
        //Get the correct client based on the id given in the url
        ModbusClient? client = connectionManager.GetConnection(connectionId);

        //Check if the client is connected
        if (client == null || !client.isConnected)
        {
            return Results.Conflict("Not connected.");
        }

        //Create a request object from the received data
        var request = new ReadInputRegistersRequest
        {
            SlaveId = slaveId,
            Address = address,
            Count = count
        };

        //Validate the request
        string error = "";
        if(!RequestValidator
        .ValidateReadInputRegisters(
            request,
            out error))
        {
            return Results.BadRequest(new
            {
                error = "Invalid Request.",
                message = error
            });
        }

        //Send the request to the client
        ushort[] result =
            client.ReadInputRegisters(
                request.SlaveId,
                request.Address,
                request.Count);

        //Format the response 
        var response =
            ModbusResponseFormatter
            .Format(
                AnalogInputBase,
                request.Address,
                result);

        //Return the response to the front end
        return Results.Ok(response);
    });

//Endpoint for Write Single Coil (0x05)
app.MapPut(
    "/connections/{connectionId}" +
    "/devices/{slaveId}" +
    "/coils/single",
    (string connectionId,
    byte slaveId, 
    ushort address, 
    WriteSingleCoilRequest request) =>
    {
        //Get the correct client based on the id given in the url
        ModbusClient? client = connectionManager.GetConnection(connectionId);

        //Check if the client is connected
        if (client == null || !client.isConnected)
        {
            return Results.Conflict("Not connected.");
        }

        //Create request from the data
        request.SlaveId = slaveId;
        request.Address = address;

        //Validate request
        string error = "";
        if(!RequestValidator
        .ValidateWriteSingleCoil(
            request, out error))
        {
            return Results.BadRequest(new
            {
                error = "Invalid Request.",
                message = error
            });
        }

        //Send the request to the client
        client.WriteSingleCoil(
            request.SlaveId,
            request.Address,
            request.Value);

        //Return the request to the front end
        return Results.NoContent();
    });

//Endpoint for Write Single Register (0x06)
app.MapPut(
    "/connections/{connectionId}" +
    "/devices/{slaveId}" +
    "/holding-registers/single",
    (string connectionId, 
    byte slaveId, 
    ushort address,
    WriteSingleRegisterRequest request) =>
    {
        //Get the correct client based on the id given in the url
        ModbusClient? client = connectionManager.GetConnection(connectionId);

        //Check if the client is connected
        if (client == null || !client.isConnected)
        {
            return Results.Conflict("Not connected.");
        }

        //Add the data from the url to the request object
        request.SlaveId = slaveId;
        request.Address = address;

        //Validate request object
        string error = "";
        if(!RequestValidator
        .ValidateWriteSingleRegister(
            request, out error))
        {
            return Results.BadRequest(new
            {
                error = "Invalid Request.",
                message = error
            });
        }

        //Send the request
        client.WriteSingleRegister(
            request.SlaveId,
            request.Address,
            request.Value);

        //Return success to the front end
        return Results.NoContent();
    });

//Endpoint for Write Multiple Coils (0x0F)
app.MapPut(
    "/connections/{connectionId}" +
    "/devices/{slaveId}" +
    "/coils/multiple",
    (string connectionId,
    byte slaveId, 
    ushort address,
    WriteMultipleCoilsRequest request) =>
    {
        //Get the correct client based on the id given in the url
        ModbusClient? client = connectionManager.GetConnection(connectionId);

        //Check if the client is connected
        if (client == null || !client.isConnected)
        {
            return Results.Conflict("Not connected.");
        }

        //Add the values from the url to the request obj
        request.SlaveId = slaveId;
        request.Address = address;

        //Validate the request data
        string error = "";
        if(!RequestValidator
        .ValidateWriteMultipleCoils(
            request, out error))
        {
            return Results.BadRequest(new
            {
                error = "Invalid Request.",
                message = error
            });
        }

        //Send the request to the client
        client.WriteMultipleCoils(
            request.SlaveId,
            request.Address,
            request.Values!);

        //Return the response to the front end
        return Results.NoContent();
    });

//Endpoint for Write Multiple Registers (0x10)
app.MapPut(
    "/connections/{connectionId}" +
    "/devices/{slaveId}" +
    "/holding-registers/multiple",
    (string connectionId,
    byte slaveId,
    ushort address,
    WriteMultipleRegistersRequest request) =>
    {
        //Get the correct client based on the id given in the url
        ModbusClient? client = connectionManager.GetConnection(connectionId);

        //Check if the client is connected
        if (client == null || !client.isConnected)
        {
            return Results.Conflict("Not connected.");
        }

        //Add the values from the url to the request object
        request.SlaveId = slaveId;
        request.Address = address;

        //Validate request
        string error = "";
        if(!RequestValidator
        .ValidateWriteMultipleRegisters(
            request, out error))
        {
            return Results.BadRequest(new
            {
                error = "Invalid Request.",
                message = error
            });
        }

        //Call the Modbus function Write Multiple Registers
        client.WriteMultipleRegisters(
            request.SlaveId,
            request.Address,
            request.Values!);

        //Return the result to the front end
        return Results.NoContent();
    });


app.Run();