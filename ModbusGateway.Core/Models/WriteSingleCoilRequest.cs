namespace ModbusGateway.Core;

public class WriteSingleCoilRequest : ModbusHttpRequest
{
    //1 byte
    public byte SlaveId { get; set; }
    //2 bytes
    public ushort Address { get; set; }
    //Boolean value to represent an ON or OFF state 
    public bool Value { get; set; }
}