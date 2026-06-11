namespace SnowflakeGenerator.Exceptions
{
    /// <summary>
    /// The exception that is thrown when the timestamp exceeds the range representable by the ID layout.
    /// </summary>
    public class TimestampOverflowException : SnowflakeException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimestampOverflowException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public TimestampOverflowException(string message) : base(message) { }
    }
}