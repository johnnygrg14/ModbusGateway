namespace ModbusGateway.Core.Models;

using System.IO.Ports;
using ModbusGateway.Core.Enums;

public class ModbusConnectionOptions
{
    public ModbusProtocol Protocol { get; set; }
    public string? ComPort { get; set; }
    public int BaudRate { get; set; }
    public Parity Parity { get; set; }
    public int DataBits { get; set; }
    public StopBits StopBits { get; set; }
}