using SnowflakeGenerator;
using SnowflakeGenerator.Exceptions;
using System.Xml;

var settings = new Settings
{
    MachineID = 0,
    CustomEpoch = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero)
};
Snowflake sonyflake = new(settings);

Console.WriteLine("Generating 10 unique ID values.\n");

for (int i = 0; i < 10; i++)
{
    ulong uniqueId;
    try
    {
        uniqueId = sonyflake.NextID();
    }
    catch (SnowflakeException ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        continue;
    }

    Console.WriteLine($"Generated ID: {uniqueId}");
}

Console.WriteLine("\nEnter any key to Exit.");
Console.ReadLine();