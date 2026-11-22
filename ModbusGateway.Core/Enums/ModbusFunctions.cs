namespace ModbusGateway.Core.Enums;

public enum ModbusFunctions
{
    ReadCoils = 0x01,
    ReadDescreteInputs = 0x02,
    ReadHoldingRegisters = 0x03,
    ReadInputRegisers = 0x04,
    WriteSingleCoil = 0x05,
    WriteSingleRegister = 0x06,
    WriteMultipleCoils = 0x15,
    WriteMultipleRegisters = 0x16
}