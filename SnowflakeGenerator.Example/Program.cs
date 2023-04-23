using SnowflakeGenerator;
using SnowflakeGenerator.Exceptions;

var settings = new Settings
{
    MachineID = 0,
    CustomEpoch = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
    MachineIDBitLength = 16,
    SequenceBitLength = 6
};
Snowflake sonyflake = new(settings);

Console.WriteLine("Generating 10 unique ID values.\n");

for (int i = 0; i < 10; i++)
{
    long uniqueId;
    long timeStamp;
    long machineId;
    long sequence;
    try
    {
        uniqueId = sonyflake.NextID();
        (timeStamp, machineId, sequence) = sonyflake.DecodeID(uniqueId);
    }
    catch (SnowflakeException ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        continue;
    }

    Console.WriteLine($"Generated ID: {uniqueId}");

    DateTimeOffset unixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

    long unixTimestamp = timeStamp + settings.CustomEpoch?.ToUnixTimeMilliseconds() ?? 0;
    DateTimeOffset generatedTime = unixEpoch.AddMilliseconds(unixTimestamp);

    Console.WriteLine($"Generated TimeStamp: {generatedTime:yyyy-MM-ddTHH:mm:ss.fffZ}");
    Console.WriteLine($"Generated MachineID: {machineId}");
    Console.WriteLine($"Generated Sequence: {sequence}");
}

Console.WriteLine("\nEnter any key to Exit.");
Console.ReadLine();
