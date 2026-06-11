namespace SnowflakeGenerator.Exceptions
{
    /// <summary>
    /// The exception that is thrown when the configured machine ID or sequence bit lengths are invalid.
    /// </summary>
    public class InvalidBitLengthException : SnowflakeException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidBitLengthException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public InvalidBitLengthException(string message) : base(message) { }
    }
}