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

        /// <summary>
        /// Gets or sets the number of bits allocated to the Machine ID part of the Snowflake ID.
        /// The default value is null, which means the default Machine ID bit length (10 bits) will be used.
        /// </summary>
        public int? MachineIDBitLength { get; set; }

        /// <summary>
        /// Gets or sets the number of bits allocated to the Sequence part of the Snowflake ID.
        /// The default value is null, which means the default Sequence bit length (12 bits) will be used.
        /// </summary>
        public int? SequenceBitLength { get; set; }
    }
}
