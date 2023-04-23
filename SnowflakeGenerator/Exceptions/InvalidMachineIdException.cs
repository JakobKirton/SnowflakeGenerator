namespace SnowflakeGenerator.Exceptions
{
    public class InvalidMachineIdException : SnowflakeException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidMachineIdException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public InvalidMachineIdException(string message) : base(message) { }
    }
}