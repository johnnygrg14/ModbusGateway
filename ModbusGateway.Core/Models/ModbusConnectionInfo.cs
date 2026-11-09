public class ModbusConnectionInfo
{
    public string ConnectionId { get; set; } = string.Empty;
    public ModbusProtocol? Protocol { get; set; }
    public string? ComPort { get; set; }
    public int? BaudRate { get; set; }
    public bool IsConnected { get; set; }
}