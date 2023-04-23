namespace SnowflakeGenerator.Exceptions
{
    public class InvalidCustomEpochException : SnowflakeException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidCustomEpochException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public InvalidCustomEpochException(string message) : base(message) { }
    }
}