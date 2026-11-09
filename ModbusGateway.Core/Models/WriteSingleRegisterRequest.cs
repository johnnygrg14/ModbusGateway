public class WriteSingleRegisterRequest : ModbusHttpRequest
{
    //1 byte
    public byte SlaveId { get; set; }
    //2 bytes
    public ushort Address {  get; set; }
    //2 byte value that will be written 
    public ushort Value {  get; set; }
}