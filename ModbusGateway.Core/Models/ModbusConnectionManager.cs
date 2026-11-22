using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public class ModbusConnectionManager
{
    //Main container for the list of connections
    private readonly Dictionary<string, ModbusClient> _connections = new();

    //Lock for only allowing one thread to use connection or modbus operations
    private readonly object _lock = new();

    /// <summary>
    /// Creates a serial connection and returns an id for it. 
    /// The front end will use this id to use the connection.
    /// </summary>
    /// <param name="options">Connection congiguration of type ModbusConnectionOptions</param>
    /// <returns>A string containing the Id of the new connection.</returns>
    /// <exception cref="InvalidOperationException">In case the requested serial port is not available</exception>
    public string Connect(ModbusConnectionOptions options)
    {
        lock (_lock)
        {
            //Check if the requested com port already has a connection
            bool portInUse = _connections.Values.Any(
                client => string.Equals(
                    client.ComPort,
                    options.ComPort,
                    StringComparison.OrdinalIgnoreCase));

            if (portInUse) 
            {
                throw new InvalidOperationException(
                    "This COM port is already in use.");
            }

            //Create a client and try to establish a serial connection
            ModbusClient client = new ModbusClient(options);

            try
            {
                client.Connect();

                //Create the connection id and add the connection to the dictionary
                string connectionId = Guid.NewGuid().ToString("N"); //.ToString("N") is used to return a number
                _connections.Add(connectionId, client);

                return connectionId;
            }
            catch
            {
                //If something goes wrong call client.Disconnect()
                //because it releases the resources used for establishing a connection
                client.Disconnect();
                throw;
            }
        }
    }

    /// <summary>
    /// Searches for a connection based on its id
    /// </summary>
    /// <param name="connectionId"></param>
    /// <returns>A modbus client object with the requested id if found</returns>
    public ModbusClient? GetConnection(string connectionId)
    {
        lock (_lock)
        {
            _connections.TryGetValue(connectionId, out ModbusClient? client);
            return client;
        }
    }

    /// <summary>
    /// Returns an enumerable list of all the active connections.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetConnectionIds()
    {
        lock( _lock)
        {
            return _connections.Keys.ToList();
        }
    }

    /// <summary>
    /// Returns an enumerable list with details for all active connections
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ModbusConnectionInfo> GetConnectionInfo()
    {
        lock (_lock)
        {
            return _connections.Select(entry => new ModbusConnectionInfo
            {
                ConnectionId   = entry.Key,
                Protocol       = entry.Value.Protocol,
                ComPort        = entry.Value.ComPort,
                BaudRate       = entry.Value.BaudRate,
                Parity         = entry.Value.Parity,
                DataBits       = entry.Value.DataBits,
                StopBits       = entry.Value.StopBits,
                IsConnected    = entry.Value.isConnected
            }).ToList();
        }
    }

    /// <summary>
    /// Removes a client from the list of connections and then releases the connection using the client's disconnect function
    /// </summary>
    /// <param name="connectionId"></param>
    /// <returns></returns>
    public bool Disconnect(string connectionId)
    {
        lock(_lock)
        {
            if(!_connections.Remove(connectionId, out ModbusClient? client))
            {
                return false;
            }

            client.Disconnect();
            return true;
        }
    }

    /// <summary>
    /// Releases all connections.
    /// </summary>
    public void DisconnectAll()
    {
        foreach (string connectionId in _connections.Keys.ToList())
        {
            Disconnect(connectionId);
        }
    }
}