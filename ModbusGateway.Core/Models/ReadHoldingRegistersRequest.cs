public class ReadHoldingRegistersRequest
{
    public ModbusProtocol Protocol { get; set; }

    public byte SlaveId { get; set; }

    public ushort Address { get; set; }

    public ushort Count { get; set; }

    public string? IpAddress { get; set; }

    public int Port { get; set; }

    public string? ComPort { get; set; }

    public int BaudRate { get; set; }
}