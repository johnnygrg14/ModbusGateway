namespace ModbusGateway.Core.Models;

using ModbusGateway.Core.Enums;

public class ModbusHttpRequest
{
    public ModbusProtocol Protocol { get; set; }

    public int Port { get; set; }

    public string? ComPort { get; set; }

    public int BaudRate { get; set; }
}