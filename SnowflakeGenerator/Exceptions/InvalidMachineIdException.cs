namespace SnowflakeGenerator.Exceptions
{
    public class InvalidMachineIdException : SnowflakeException
    {
        public InvalidMachineIdException(string message) : base(message) { }
    }
}