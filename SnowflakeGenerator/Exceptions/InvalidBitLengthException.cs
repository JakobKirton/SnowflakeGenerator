namespace SnowflakeGenerator.Exceptions
{
    public class InvalidBitLengthException : SnowflakeException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidBitLengthException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public InvalidBitLengthException(string message) : base(message) { }
    }
}