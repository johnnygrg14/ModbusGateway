public class ModbusConnectionOptions
{
    public ModbusProtocol Protocol { get; set; }

    //TCP
    public string? IpAddress { get; set; }
    public int Port { get; set; }


    //Serial (RTU/ASCII)
    public string? ComPort { get; set; }
    public int BaudRate { get; set; }
}