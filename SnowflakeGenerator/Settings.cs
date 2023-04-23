using System;

namespace SnowflakeGenerator
{
    /// <summary>
    /// Represents settings for configuring a Snowflake instance.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Gets or sets the machine ID for the Snowflake instance. This value must be unique across all machines generating IDs concurrently.
        /// </summary>
        public uint? MachineID { get; set; }

        /// <summary>
        /// Gets or sets the custom epoch for the Snowflake instance. The custom epoch is used as a reference point for the timestamp portion of the generated ID.
        /// </summary>
        public DateTimeOffset? CustomEpoch { get; set; }
    }
}
