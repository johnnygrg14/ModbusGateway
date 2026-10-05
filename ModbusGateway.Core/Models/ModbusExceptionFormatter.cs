public static class ModbusExceptionFormatter
{
    //This class creates a json response for the javascript to 
    //display in case of an exception
    //We subtract 0x80 from the function code because when an exception is 
    //returned by the modbus machine then 0x80 is added to the function code
    //The above is described in the specification
    public static ModbusExceptionResponse Format(
        NModbus.SlaveException ex)
    {
        return new ModbusExceptionResponse
        {
            FunctionCode = ex.FunctionCode - 0x80,
            ExceptionCode = ex.SlaveExceptionCode,
            Message = GetExceptionDescription(ex.SlaveExceptionCode)
        };
    }

    //The exception descriptions are based on the specification document at
    //https://www.modbus.org/file/secure/modbusprotocolspecification.pdf
    public static string GetExceptionDescription(byte code)
    {
        return code switch
        {
            0x01 => "Illegal Function",
            0x02 => "Illegal Data Address",
            0x03 => "Illegal Data Value",
            0x04 => "Server Device Failure",
            0x05 => "Acknowledge",
            0x06 => "Server Device Busy",
            0x08 => "Memory Parity Error",
            0x0A => "Gateway Path Unavailable",
            0x0B => "Gateway Target Device Failed To Respond",
            _ => "Unknown Modbus Exception"
        };
    }
}