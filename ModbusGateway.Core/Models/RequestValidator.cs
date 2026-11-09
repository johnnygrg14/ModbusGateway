/// <summary>
/// This class implements functions that check the validity
/// of the data a request contains.
/// They all work very similarly but each one is dedicated to each 
/// corresponding Modbus function.
/// The data we check are the slave ID, the Starting Address when needed,
/// the count of registers and if the address plus the count will exceed 
/// the memory limits of a Modbus device as specified in the specification.
/// 
/// Each function performs a series of if statements and will return false 
/// if an issue is found along with an error that will be shown to the user.
/// They are all structured in a way where if the end of the function is reached
/// we can assume that all of the data is valid and we can return true.
/// </summary>

public class RequestValidator
{ 
    public static bool ValidateReadCoils(
        ReadCoilsRequest request, 
        out string error)
    {
        //Check if broadcast case was requested from a "Read" function
        if(request.SlaveId == 0)
        {
            error = "Broadcast case is only supported for \"Write\" Modbus functions.";
            return false;
        }

        //Check if slave address is valid
        if (request.SlaveId < 1 || request.SlaveId > 247)
        {
            error = "Slave ID must be between 1 and 247";
            return false;
        }

        //Since the Starting Address is a ushort, the type limits match the limits 
        //specified by Modbus therefore we do not need to perform a check for
        //Address validity here

        //Check if the quantity of the coils is valid
        if (request.Count < 1 || request.Count > 2000)
        {
            error = "Count must be between 1 and 2000.";
            return false;
        }

        //Check if the starting address and the quantity exceed the modbus limits
        if ((uint)request.Address + request.Count > 65536)
        {
            error = "Requested address range exceeds Modbus limits.";
            return false;
        }

        error = "";
        return true;
    }

    public static bool ValidateReadDiscreteInputs(
        ReadInputsRequest request, 
        out string error)
    {
        //Check if broadcast case was requested from a "Read" function
        if (request.SlaveId == 0)
        {
            error = "Broadcast case is only supported for \"Write\" Modbus functions.";
            return false;
        }

        //Check if slave address is valid
        if (request.SlaveId < 1 || request.SlaveId > 247)
        {
            error = "Slave ID must be between 1 and 247";
            return false;
        }
            
        //Since the Starting Address is a ushort, the type limits match the limits 
        //specified by modbus therefore we do not need to perform a check for
        //Address validity here

        //Check if the quantity of the coils is valid
        if (request.Count < 1 || request.Count > 2000)
        {
            error = "Count must be between 1 and 2000.";
            return false;
        }

        //Check if the starting address and the quantity exceed the modbus limits
        if ((uint)request.Address + request.Count > 65536)
        {
            error = "Requested address range exceeds Modbus limits.";
            return false;
        }

        error = "";
        return true;
    }

    public static bool ValidateReadHoldingRegisters(
        ReadHoldingRegistersRequest request,
        out string error)
    {
        //Check if broadcast case was requested from a "Read" function
        if (request.SlaveId == 0)
        {
            error = "Broadcast case is only supported for \"Write\" Modbus functions.";
            return false;
        }

        //Check if slave id is between 1 and 247
        if (request.SlaveId < 1 || request.SlaveId > 247)
        {
            error = "Slave ID must be between 1 and 247.";
            return false;
        }

        //Since the Starting Address is a ushort, the variable type limits match
        //the limits specified by modbus therefore we do not need to perform
        //a check for Starting Address validity here

        //Check if the Quantity of Registers is between 1 and 125
        if (request.Count < 1 || request.Count > 125)
        {
            error = "Count must be between 1 and 125.";
            return false;
        }

        //Check if the address plus the count exceed modbus limits
        if ((uint)request.Address + request.Count > 65536)
        {
            error = "Requested address range exceeds Modbus limits.";
            return false;
        }

        error = "";
        return true;
    }

    public static bool ValidateReadInputRegisters(
        ReadInputRegistersRequest request,
        out string error)
    {
        //Check if broadcast case was requested from a "Read" function
        if (request.SlaveId == 0)
        {
            error = "Broadcast case is only supported for \"Write\" Modbus functions.";
            return false;
        }

        //Check if slave address is valid
        if (request.SlaveId < 1 || request.SlaveId > 247)
        {
            error = "Slave ID must be between 1 and 247.";
            return false;
        }

        //Check if input register quantity is valid
        if (request.Count < 1 || request.Count > 125)
        {
            error = "Count must be between 1 and 125.";
            return false;
        }

        //Check if address + count exceeds modbus limits
        if ((uint)request.Address + request.Count > 65536)
        {
            error = "Requested address range exceeds Modbus limits.";
            return false;
        }
 
        error = "";
        return true;
    }

    public static bool ValidateWriteSingleCoil(
        WriteSingleCoilRequest request,
        out string error)
    {
        //Check if slave address is valid
        if (request.SlaveId < 1 || request.SlaveId > 247)
        {
            error = "Slave ID must be between 1 and 247";
            return false;
        }
        
        //No need to check Output Address as the variable is a ushort 
        //and it has the same range as the specification sets.

        //Modbus specification uses 0x0000 and 0xff00 for ON and OFF states of the coil
        //NModbus4 uses a simple boolean so we do not need to check if it is within range 
        //Since this endpoint receives a boolean there is no need to validate it further 
        //as a C# boolean cannot have any other value besides true or false.

        error = "";
        return true;
    }

    public static bool ValidateWriteSingleRegister(
        WriteSingleRegisterRequest request,
        out string error)
    {
        //Check if the slave ID is within valid modbus range
        if (request.SlaveId < 1 || request.SlaveId > 247)
        {
            error = "Slave ID must be between 1 and 247";
            return false;
        }

        //No need to check Register Address and Register Value
        //as they will always be in the right range since they are 
        //of type ushort which is a positive 2 byte integer

        error = "";
        return true;
    }

    public static bool ValidateWriteMultipleCoils(
        WriteMultipleCoilsRequest request, 
        out string error)
    {
        //Check if slave id is between 1 and 247
        if (request.SlaveId < 1 || request.SlaveId > 247)
        {
            error = "Slave ID must be between 1 and 247.";
            return false;
        }

        //Since Address is of type ushort we dont need to check if it is larger than 65535 
        //because thats the limit of the ushort type

        //Check if the quantity of outputs is between 1 and 1968 (0x07B0)
        if (request.Values.Length < 1 || request.Values.Length > 1968)
        {
            error = "Values count must be between 1 and 1968.";
            return false;
        }

        //Check if the address plus the count of values exceed the modbus limit
        if (request.Address + request.Values.Length > 65536)
        {
            error = "Address + count exceeds modbus range.";
            return false;
        }

        error = "";
        return true;
    }

    public static bool ValidateWriteMultipleRegisters(
        WriteMultipleRegistersRequest request,
        out string error)
    {
        //Check if slave id is between 1 and 247
        if (request.SlaveId < 0 || request.SlaveId > 247)
        {
            error = "Slave ID must be between 1 and 247.";
            return false;
        }

        //Check if the number of values is between 1 and 123
        if (request.Values.Length < 1 || request.Values.Length > 123)
        {
            error = "Count must be between 1 and 123.";
            return false;
        }

        //Check if the address + the number of values exceed the modbus limit
        if ((uint)request.Address + request.Values.Length > 65536)
        {
            error = "Requested address range exceeds Modbus limits.";
            return false;
        }

        error = "";
        return true;
    }
}
















