public class ModbusConnectionOptions
{
    public ModbusProtocol Protocol { get; set; }

    //Serial (RTU/ASCII)
    public string? ComPort { get; set; }
    public int BaudRate { get; set; }
}