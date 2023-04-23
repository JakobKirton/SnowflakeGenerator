using System;

namespace SnowflakeGenerator.Exceptions
{
    /// <summary>
    /// Represents errors that occur during Snowflake ID generation.
    /// </summary>
    public class SnowflakeException : Exception
    {
        /// <summary>
        /// Represents errors that occur during Snowflake ID generation.
        /// </summary>
        public SnowflakeException(string message) : base(message) { }
    }
}
