using System.IO.Ports;

public class ModbusConnectionInfo
{
    public string ConnectionId { get; set; } = string.Empty;
    public ModbusProtocol? Protocol { get; set; }
    public string? ComPort { get; set; }
    public int? BaudRate { get; set; }
    public Parity? Parity { get; set; }
    public int? DataBits { get; set; }
    public StopBits? StopBits { get; set; }
    public bool? IsConnected { get; set; }
}