public class ModbusExceptionResponse
{
    public readonly string Error = "Modbus exception";
    public int FunctionCode { get; set; }
    public int ExceptionCode { get; set; }
    public string Message { get; set; } = "";
}