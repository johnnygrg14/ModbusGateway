public class WriteSingleRegisterRequest : ModbusHttpRequest
{
    public byte SlaveId { get; set; }

    public ushort Address {  get; set; }

    public ushort Value {  get; set; }
}