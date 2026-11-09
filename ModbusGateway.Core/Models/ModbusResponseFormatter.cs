public static class ModbusResponseFormatter
{
    //This function returns a list of formatted modbus data received from a read modbus function
    //The data type of the list is another class named ModbusValue which takes in a generic
    //data type so this function can be used with either a boolean response value or a numeric one
    
    //The way this function works is that it takes in the base address (given by the main program)
    //the starting address of the modbus value that was read and it creates a list of them using
    //the select function and a lambda
    public static List<ModbusValue<T>> Format<T>(
        int baseAddress,
        ushort startingAddress,
        T[] values)
    {
        return values
            .Select((value, index) => new ModbusValue<T>
            {
                Address = baseAddress + startingAddress + index,
                Value = value
            })
            .ToList();
    }
}