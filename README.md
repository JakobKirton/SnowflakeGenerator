# SnowflakeGenerator
SnowflakeGenerator is a unique ID generator based on [Twitter's Snowflake](https://blog.twitter.com/engineering/en_us/a/2010/announcing-snowflake "Twitter Snowflake Blog"). It generates 64-bit, time-ordered, unique IDs based on the Snowflake algorithm. It is written in C# and is compatible with .NET Standard 2.0.

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
```powershell
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

## Settings

The `Settings` class allows you to customize the Snowflake instance:

-   `MachineID`: A unique identifier for the machine or instance generating the IDs.
-   `CustomEpoch`: A custom epoch or reference point for the timestamp portion of the generated ID.

## License

This project is licensed under the [MIT License](https://github.com/JakobKirton/SnowflakeGenerator/blob/main/LICENSE).
## Contributing

Contributions are welcome. Please submit a pull request or create an issue to discuss your proposed changes.
