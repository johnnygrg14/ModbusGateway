public class WriteMultipleCoilsRequest : ModbusHttpRequest
{
    public byte SlaveId { get; set; }

    public ushort Address { get; set; }

    //@todo check if this has to  be nullable or whatever
    public bool[] Values { get; set; } = Array.Empty<bool>();
}