namespace ModbusGateway.Core;

public class ReadCoilsRequest : ModbusHttpRequest
{
    //1 byte
    public byte SlaveId { get; set; }

    //2 bytes positive integer
    public ushort Address {  get; set; }

    //2 byte positive integer
    public ushort Count { get; set; }
}