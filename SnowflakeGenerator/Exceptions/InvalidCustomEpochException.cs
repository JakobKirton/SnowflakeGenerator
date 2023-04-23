namespace SnowflakeGenerator.Exceptions
{
    public class InvalidCustomEpochException : SnowflakeException
    {
        public InvalidCustomEpochException(string message) : base(message) { }
    }
}