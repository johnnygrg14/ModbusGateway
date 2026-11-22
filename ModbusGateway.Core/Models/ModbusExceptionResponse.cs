namespace ModbusGateway.Core.Models;

public class ModbusExceptionResponse
{
    public string Error { get; } = "Modbus exception";
    public int FunctionCode { get; set; }
    public int ExceptionCode { get; set; }
    public string Message { get; set; } = "";
}