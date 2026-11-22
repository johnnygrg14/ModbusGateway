namespace ModbusGateway.Core;

using NModbus;
using NModbus.Data;
using NModbus.Message;
using NModbus.Serial;
using System.IO.Ports;
using ModbusGateway.Core.Enums;
using ModbusGateway.Core.Models;

/// <summary>
/// This is the class that represents a connection to a modbus device (client)
/// It has the ability to create and release a serial connection and implements 
/// all of the Modbus operations this application supports.
/// </summary>
public class ModbusClient 
{
    private IModbusMaster? _master = null;

    private SerialPort? _serialPort = null;


    private ModbusConnectionOptions? _options = null;

    public string? ComPort => _options?.ComPort;
    public ModbusProtocol? Protocol => _options?.Protocol;
    public int? BaudRate => _options?.BaudRate;
    public Parity? Parity => _options?.Parity;
    public int? DataBits => _options?.DataBits;
    public StopBits? StopBits => _options?.StopBits;
    
    private readonly object _lock = new object();
    public bool isConnected
    {
        get
        {
            lock (_lock)
            {
                return _master != null &&
                    _serialPort != null && 
                    _serialPort.IsOpen;
            }
        }
    }

    public ModbusClient(ModbusConnectionOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// Creates a connection with a client with the given configuration.
    /// If connection creation fails it calls the Disconnect function to 
    /// free up the resources.
    /// </summary>
    public void Connect()
    {
        lock (_lock)
        {
            var factory = new ModbusFactory();

            try
            {
                //Create a serial port with the received configuration
                _serialPort 
                    = new SerialPort(
                        _options!.ComPort,
                        _options.BaudRate,
                        _options.Parity,
                        _options.DataBits,
                        _options.StopBits);

                if (!_serialPort.IsOpen)
                {
                    _serialPort.Open();
                }

                //Create the appropriate Modbus client based on the protocol
                switch (_options!.Protocol)
                {
                    case ModbusProtocol.RTU:
                        _master = factory.CreateRtuMaster(_serialPort);
                        break;

                    case ModbusProtocol.ASCII:
                        _master = factory.CreateAsciiMaster(_serialPort);
                        break;
                }

                //10 seconds timeouts for read and write operations
                _master!.Transport.ReadTimeout  = 10000;
                _master!.Transport.WriteTimeout = 10000;
                _master!.Transport.Retries = 0;
            }
            catch
            {
                //If connection fails we call Disconnect() because it releases the resources
                //used for creating a connection 
                //We also throw the error so the endpoint can show it to the user
                Disconnect();
                throw;
            }
        }
    }

    /// <summary>
    /// This function releases an active connection
    /// It checks if a master or a serial port are active
    /// and disconnects them if applicable
    /// </summary>
    public void Disconnect()
    {
        lock (_lock)
        {
            if (_master != null)
            {
                _master.Dispose();
                _master = null;
            }

            if (_serialPort != null)
            {
                _serialPort.Dispose();
                _serialPort = null;
            }

            if (_options != null)
            {
                _options = null;
            }
        }
    }

    //Wrapper function for NModbus ReadCoils
    public bool[] ReadCoils(
        byte slaveId,
        ushort startAddress,
        ushort count)
    {
        lock(_lock)
        {
            return _master!.ReadCoils(
                slaveId, 
                startAddress,
                count);
        }
    }

    //Wrapper function for NModbus ReadInputs
    public bool[] ReadDiscreteInputs(
        byte slaveId,
        ushort startAddress,
        ushort count)
    {
        lock (_lock)
        {
            return _master!.ReadInputs(
                slaveId,
                startAddress,
                count);
        }
    }

    //Wrapper function for NModbus ReadHoldingRegisters
    public ushort[] ReadHoldingRegisters(
        byte slaveId,
        ushort startAddress,
        ushort count)
    {
        lock (_lock)
        {
            return _master!.ReadHoldingRegisters(
                slaveId,
                startAddress,
                count);
        }
    }

    //Wrapper function for NModbus ReadInputRegisters
    public ushort[] ReadInputRegisters(
        byte slaveId,
        ushort startAddress,
        ushort count)
    {
        lock (_lock)
        {
            return _master!.ReadInputRegisters(
                slaveId,
                startAddress,
                count);
        }
    }

    //Wrapper function for NModbus WriteSingleRegister
    public void WriteSingleRegister(
        byte slaveId,
        ushort address,
        ushort value)
    {
        lock (_lock)
        {
            //NModbus provides a broadcast function for WriteSingleRegister 
            if (slaveId == 0)
            {
                _master!.BroadcastWriteSingleRegister(address, value);

                return;
            }

            _master!.WriteSingleRegister(
                slaveId,
                address,
                value);
        }
    }

    //Note: Modbus Write functions require the broadcast case 
    //NModbus does not have an implementation for a broadcast
    //WriteMultipleRegisters, WriteMultipleCoils and WriteSingleCoil
    //so the following code is a modified version 
    //of the its implementation but using the function
    //BroadcastWrite in order to support the broadcast case.

    //Wrapper function for NModbus WriteSingleCoil
    public void WriteSingleCoil(
        byte slaveId,
        ushort coilAddress,
        bool value)
    {
        lock (_lock)
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
    }

    //Wrapper function for NModbus WriteMultipleCoils
    public void WriteMultipleCoils(
        byte slaveId,
        ushort startAddress,
        bool[] data)
    {
        lock(_lock)
        {
            if (slaveId == 0)
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
    }

    //Wrapper function for NModbus WriteMultipleRegisters
    public void WriteMultipleRegisters(
        byte slaveId,
        ushort startAddress,
        ushort[] data)
    {
        lock (_lock)
        {
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
}