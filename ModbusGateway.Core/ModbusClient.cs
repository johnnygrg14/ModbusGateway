using NModbus;
using NModbus.Data;
using NModbus.IO;
using NModbus.Message;
using NModbus.Serial;
using System.IO.Ports;

public class ModbusClient 
{
    private IModbusMaster? _master = null;

    private SerialPort? _serialPort = null;

    private ModbusConnectionOptions? _options = null;
    


    public ModbusClient(ModbusConnectionOptions options)
    {
        _options = options;
    }

    public void Connect()
    {
        var factory = new ModbusFactory();

        //Using an exception for safety we try to establish a connection with
        //the provided connection options 
        try
        {
            //This switch statement creates a modbus client based on the protocol set
            switch (_options!.Protocol)
            {
                case ModbusProtocol.RTU:
                    _serialPort = new SerialPort(
                        _options.ComPort!,
                        _options.BaudRate,
                        Parity.None,
                        8,
                        StopBits.One);

                    if (!_serialPort.IsOpen)
                    {
                        _serialPort.Open();
                    }

                    _master = factory.CreateRtuMaster(_serialPort);
                    break;

                case ModbusProtocol.ASCII:
                    _serialPort = new SerialPort(
                       _options.ComPort!,
                       _options.BaudRate,
                       Parity.Even,
                       7,
                       StopBits.One);

                    if (!_serialPort.IsOpen)
                    {
                        _serialPort.Open();
                    }

                    _master = factory.CreateAsciiMaster(_serialPort);
                    break;
            }
        }
        catch
        {
            //In case the connection fails, we call disconnect() because 
            //disconnect nulls every field of the object.
            //We also throw the error so the endpoint can show it to the user
            Disconnect();
            throw;
        }
    }

    /*
     * This function releases an active connection
     * It checks if a master, a tcp client or a serial port are active
     * and disconnects them if applicable
     */
    public void Disconnect()
    {
        if(_master != null)
        {
            _master.Dispose();
            _master = null;
        }

        if(_serialPort != null)
        {
            _serialPort.Dispose();
            _serialPort = null;
        }

        if(_options != null)
        {
            _options = null;
        }
    }

    //Wrapper function for NModbus ReadCoils
    public bool[] ReadCoils(
        byte slaveId,
        ushort startAddress,
        ushort count)
    {
        return _master!.ReadCoils(
            slaveId, 
            startAddress,
            count);
    }

    //Wrapper function for NModbus ReadInputs
    public bool[] ReadDiscreteInputs(
        byte slaveId,
        ushort startAddress,
        ushort count)
    {
        return _master!.ReadInputs(
            slaveId,
            startAddress,
            count);
    }

    //Wrapper function for NModbus ReadHoldingRegisters
    public ushort[] ReadHoldingRegisters(
        byte slaveId,
        ushort startAddress,
        ushort count)
    {
        return _master!.ReadHoldingRegisters(
            slaveId,
            startAddress,
            count);
    }

    //Wrapper function for NModbus WriteSingleRegister
    public void WriteSingleRegister(
        byte slaveId,
        ushort address,
        ushort value)
    {
        //NModbus provides a broadcast function for WriteSingleRegister 
        if(slaveId == 0)
        {
            _master!.BroadcastWriteSingleRegister(address, value);

            return;
        }

        _master!.WriteSingleRegister(
            slaveId,
            address,
            value);
    }

    //Wrapper function for NModbus ReadInputRegisters
    public ushort[] ReadInputRegisters(
        byte slaveId,
        ushort startAddress,
        ushort count)
    {
        return _master!.ReadInputRegisters(
            slaveId,
            startAddress,
            count);
    }

    //Wrapper function for NModbus WriteSingleCoil
    public void WriteSingleCoil(
        byte slaveId,
        ushort coilAddress,
        bool value)
    {
        //NModbus doesn't provide a broadcast function for WriteSingleCoil
        //so we implement a specialization case for broadcast here, 
        //using NModbus' transport layer.
        if(slaveId == 0)
        {
            var request = new 
                WriteSingleCoilRequestResponse(
                slaveId, coilAddress, value);

            _master!.Transport.BroadcastWrite(request);

            return;
        }

        _master!.WriteSingleCoil(
            slaveId,
            coilAddress,
            value);
    }

    //Wrapper function for NModbus WriteMultipleCoils
    public void WriteMultipleCoils(
        byte slaveId,
        ushort startAddress,
        bool[] data)
    {
        if(slaveId == 0)
        {
            //In the NModbus implementation of WriteMultipleCoils there is a data validation
            //instruction but since that validation function is a private member of ModbusMaster
            //we cannot use it here. This does not matter much though since these functions already 
            //receive validated data from RequestValidator.

            var request = new NModbus.Message.WriteMultipleCoilsRequest(
                    slaveId,
                    startAddress,
                    new DiscreteCollection(data));

            _master!.Transport.BroadcastWrite(request);
            return;
        }

        _master!.WriteMultipleCoils(
            slaveId,
            startAddress,
            data);
    }

    //Wrapper function for NModbus WriteMultipleRegisters
    public void WriteMultipleRegisters(
        byte slaveId,
        ushort startAddress,
        ushort[] data)
    {
        //Note: Modbus Write functions require the broadcast case 
        //NModbus does not have an implementation for a broadcast
        //WriteMultipleRegisters so the following code is a modified 
        //version of the its implementation but using the function
        //BroadcastWrite in order to support the broadcast case.
        //Similar implementations will be found for any other 
        //Broadcast Functions that NModbus doesn't support
        if (slaveId == 0)
        {
           var request = new 
                NModbus.Message.WriteMultipleRegistersRequest(
                    0,
                    startAddress,
                    new RegisterCollection(data));

            _master!.Transport.BroadcastWrite(request);
            return;
        }

        _master!.WriteMultipleRegisters(
            slaveId,
            startAddress,
            data);
    }
}