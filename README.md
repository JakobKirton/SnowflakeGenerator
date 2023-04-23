# SnowflakeGenerator
SnowflakeGenerator is a unique ID generator based on [Twitter's Snowflake](https://blog.twitter.com/engineering/en_us/a/2010/announcing-snowflake "Twitter Snowflake Blog"). It generates 64-bit, time-ordered, unique IDs based on the Snowflake algorithm. It is written in C# and is compatible with .NET Standard 2.0.

The default bit assignment for this Snowflake implementation is:
```
42 bits for the TimeStamp value
10 bits for the MachineID value
12 bits for Sequence value
```
This provides by default:

 - A time range of approximately 139 years (2^42 milliseconds).
 - Use of 1024 (2^10) unique MachineIDs across a distributed deployment.
 - Generation for a maximum of 4096 (2^12) IDs per ms from a single Snowflake instance.
 
If you require a higher generation rate or large range of MachineID's these values can be customised by the Settings used to initialize a Snowflake instance.

**_NOTE:_** 42 bits is always reserved for the TimeStamp value. Therefore, the sum of the MachineIDBitLength and SequenceBitLength cannot exceed 22. With the SequenceBitLength being at least equal to 1.

# Features

 - Generates 64-bit unique IDs, which are time-ordered.
 - Customizable machine ID and custom epoch settings.
 - Thread-safe ID generation.
 - High-performance and low-latency.

# Installation

To install the SnowflakeGenerator library, you can use the following command in the Package Manager Console:
```powershell
Install-Package SnowflakeGenerator
```
Alternatively, you can use the .NET CLI:
```
dotnet add package SnowflakeGenerator
```

# Usage

First, import the SnowflakeGenerator namespace:
```csharp
using SnowflakeGenerator;
```

Create a new instance of the `Snowflake` class with optional settings:

```csharp
Settings settings = new Settings 
{ 
    MachineID = 1,
    CustomEpoch = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero) 
};
    
Snowflake snowflake = new Snowflake(settings);
```    
Generate a new unique ID:

```csharp
ulong id = snowflake.NextID();
```

Decode ID content:
```csharp
var (timeStamp, machineId, sequence) = sonyflake.DecodeID(uniqueId);
```

`NextID()` will throw a `TimestampOverflowException` once it reaches the limit of the TimeStamp.

## Settings

The `Settings` class allows you to customize the Snowflake instance:

-   `MachineID`: A unique identifier for the machine or instance generating the IDs.
	- Defaults to: 0
-   `CustomEpoch`: A custom epoch or reference point for the timestamp portion of the generated ID.
	- Defaults to: 1970-01-01T00:00:00.000Z
- `MachineIDBitLength`: Sets the number of bits allocated to the Machine ID part of the Snowflake ID.
	- Defaults to: 10
- `SequenceBitLength`: Sets the number of bits allocated to the Sequence part of the Snowflake ID.
	- Defaults to: 12

## License

This project is licensed under the [MIT License](https://github.com/JakobKirton/SnowflakeGenerator/blob/main/LICENSE).
## Contributing

Contributions are welcome. Please submit a pull request or create an issue to discuss your proposed changes.
