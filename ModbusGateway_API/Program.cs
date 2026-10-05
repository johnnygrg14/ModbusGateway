var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//Main MODBUS client object
//It is used for communication with the modbus device
ModbusClient? client = null;

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

//@todo check if i need to implement the broadcast case

//Endpoint for establishing a client connection
app.MapPost(
    "/connect",
    (ModbusConnectionOptions options) =>
    {
        //We first check if a connection with a client 
        //has already been established
        if(client != null)
        {
            return Results.Conflict("A Modbus client is already connected.");
        }

        //Then we create a new Modbus client object with the connection options 
        //we received from the front end 
        //and we call the client's connect function to establish a connection
        //We use an exception here to handle the cases of failed connection so that the program doesn't crash
        //and we get a normal connection refused error on the front end
        try
        {
            client = new ModbusClient(options);

            client.Connect();
        }
        catch (Exception ex)
        {
            client = null;

            return Results.BadRequest($"Connection failed: {ex.Message}");
        }

        //Because in the exception case the endpoint terminates if the code
        //reaches this part we can assume that everything worked correctly 
        //and we can return an Ok response to the front end
        return Results.Ok("Connected successfully.");
    });

//Endpoint for terminating a client connection
app.MapPost(
    "/disconnect",
    () =>
    {
        //We check if the client has an established connection
        //If there is no connection there is nothing to disconnect
        if(client == null)
        {
            return Results.Conflict("No Modbus client is connected.");
        }

        //Using an exception we call the client's disconnect function to release 
        //the connection. The exception here is good practice in case some of the 
        //Dispose functions throw some unexpected exception we can prevent a crash.
        try
        {
            client.Disconnect();
            client = null;
        }
        catch(Exception ex)
        {
            //We log the error to the console 
            Console.WriteLine(ex);

            //And null the client so another connection can be established.
            client = null;

            return Results.BadRequest("Failed to disconnect.");
        }
        
        return Results.NoContent();
    });

//This endpoint returns a boolean based on if a client exists
//it is used to let the front end know if a connection exists
//so that the right html elements can be shown.
//This exists because when the page reloads the client does not 
//get disconnected so we need to check if there is an active connection
//to enable the right HTML elements
app.MapGet(
    "/connection-status", 
    () =>
    {
        bool connected = (client != null);

        return Results.Ok(new { connected });
    });

//Endpoint for Read Coils (0x01)
app.MapPost(
    "/read-coils",
    (ReadCoilsRequest request) =>
    {
        string error = "";

        //Validate received data
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
        if(client == null)
        {
            return Results.Conflict("Not connected.");
        }

        bool[] result;

        try
        {
            //Send the request to the client 
            result =
                client!.ReadCoils(
                    request.SlaveId,
                    request.Address,
                    request.Count);
        }
        catch(NModbus.SlaveException ex)
        {
            return Results.BadRequest(
                ModbusExceptionFormatter
                .Format(ex)
            );
        }

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
app.MapPost(
    "/read-discrete-inputs",
    (ReadInputsRequest request) =>
    {
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

        //Check if the client is connected
        if (client == null)
        {
            return Results.Conflict("Not connected.");
        }

        bool[] result;

        try
        {
            //Send the request to the client
            result = client.ReadDiscreteInputs(
                request.SlaveId,
                request.Address,
                request.Count);
        }
        catch (NModbus.SlaveException ex)
        {
            return Results.BadRequest(
                ModbusExceptionFormatter
                .Format(ex)
            );
        }

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
app.MapPost(
    "/read-holding-registers",
    (ReadHoldingRegistersRequest request) =>
    {

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

        //Check if the client is connected
        if (client == null)
        {
            return Results.Conflict("Not connected.");
        }

        ushort[] result;

        try
        {
            //Send the request to the client
            result =
                client.ReadHoldingRegisters(
                    request.SlaveId,
                    request.Address,
                    request.Count);
        }
        catch(NModbus.SlaveException ex)
        {
            return Results.BadRequest(
                ModbusExceptionFormatter
                .Format(ex)
            );
        }

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
app.MapPost(
    "/read-input-registers",
    (ReadInputRegistersRequest request) =>
    {
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

        //Check if the client is connected
        if (client == null)
        {
            return Results.Conflict("Not connected.");
        }

        ushort[] result;

        try
        {
            //Send the request to the client
            result =
                client.ReadInputRegisters(
                    request.SlaveId,
                    request.Address,
                    request.Count);
        }
        catch (NModbus.SlaveException ex)
        {
            return Results.BadRequest(
                ModbusExceptionFormatter
                .Format(ex)
            );
        }

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
app.MapPost(
    "/write-single-coil",
    (WriteSingleCoilRequest request) =>
    {
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

        //Check if the client is connected
        if (client == null)
        {
            return Results.Conflict("Not connected.");
        }

        try
        {
            //Send the request to the client
            client.WriteSingleCoil(
                request.SlaveId,
                request.Address,
                request.Value);
        }
        catch(NModbus.SlaveException ex)
        {
            return Results.BadRequest(
                ModbusExceptionFormatter
                .Format(ex)
            );
        }

        //Return the request to the front end
        return Results.NoContent();
    });

//Endpoint for Write Single Register (0x06)
app.MapPost(
    "/write-single-register",
    (WriteSingleRegisterRequest request) =>
    {
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

        //Check if the client is connected
        if (client == null)
        {
            return Results.Conflict("Not connected.");
        }

        try
        {
            //Send the request
            client.WriteSingleRegister(
                request.SlaveId,
                request.Address,
                request.Value);
        }
        catch(NModbus.SlaveException ex)
        {
            return Results.BadRequest(
                ModbusExceptionFormatter
                .Format(ex)
                );
        }

        //Return success to the front end
        return Results.NoContent();
    });

//Endpoint for Write Multiple Coils (0x0F)
app.MapPost(
    "/write-multiple-coils",
    (WriteMultipleCoilsRequest request) =>
    {
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

        //Check if the client is connected
        if (client == null)
        {
            return Results.Conflict("Not connected.");
        }

        try
        {
            //Send the request to the client
            client.WriteMultipleCoils(
                request.SlaveId,
                request.Address,
                request.Values!);
        }
        catch (NModbus.SlaveException ex) 
        {
            return Results.BadRequest(
                ModbusExceptionFormatter
                .Format(ex)
                );
        }

        //Return the response to the front end
        return Results.NoContent();
    });

//Endpoint for Write Multiple Registers (0x10)
app.MapPost(
    "/write-multiple-registers",
    (WriteMultipleRegistersRequest request) =>
    {
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

        //Check if the client is connected
        if (client == null)
        {
            return Results.Conflict("Not connected.");
        }

        try
        {
            //Call the Modbus function Write Multiple Registers
            client.WriteMultipleRegisters(
                request.SlaveId,
                request.Address,
                request.Values!);
        }
        catch (NModbus.SlaveException ex)
        {
            return Results.BadRequest(
                ModbusExceptionFormatter
                .Format(ex)
                );
        }

        //Return the result to the front end
        return Results.NoContent();
    });


app.Run();