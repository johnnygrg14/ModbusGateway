/// <summary>
/// This class makes sure the data the server received to establish a 
/// serial connection is valid.
/// </summary>
public class SerialConnectionValidator
{
    /// <summary>
    /// Main function used for connection options validation
    /// Returns true if all the data is valid
    /// Returns false along with an error that will be shown 
    /// to the user.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static bool Validate(
        ModbusConnectionOptions options, out string error)
    {
        //Check if the protocol exists
        if(options.Protocol != ModbusProtocol.RTU &&
            options.Protocol != ModbusProtocol.ASCII)
        {
            error = "Unsupported protocol";
            return false;
        }

        //Check if the COM port name is valid
        if(string.IsNullOrWhiteSpace(options.ComPort) ||
            !System.Text.RegularExpressions.Regex.IsMatch(
                options.ComPort,
                @"^COM[1-9][0-9]*$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase))
        {
            error = "Invalid COM port name.";
            return false;
        }

        //Check if the baud rate is supported 
        if(!_supportedBaudRates.Contains(options.BaudRate))
        {
            error = "Unsupported baud rate.";
            return false;
        }

        error = "";
        return true;
    }

    /// <summary>
    /// The supported baud rate values were based on the emulator ModRSsim2
    /// </summary>
    private static readonly int[] _supportedBaudRates =
    {
        110,    300,    600, 
        1200,   2400,   4800,
        9600,   14400,  19200, 
        38400,  56000,  57600,
        115200, 128000, 25600
    };
}