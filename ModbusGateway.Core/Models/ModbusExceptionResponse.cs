public class ModbusExceptionResponse
{
    public int FunctionCode { get; set; }
    public int ExceptionCode { get; set; }
    public string Message { get; set; } = "";
}