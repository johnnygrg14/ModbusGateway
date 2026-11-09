public class WriteMultipleCoilsRequest : ModbusHttpRequest
{
    //1 byte
    public byte SlaveId { get; set; }
    //2 bytes
    public ushort Address { get; set; }
    //Array of booleans (length is unknown before a request is sent)
    public bool[] Values { get; set; } = Array.Empty<bool>();
}