using System;
using System.Threading;
using SnowflakeGenerator.Exceptions;

namespace SnowflakeGenerator
{
    /// <summary>
    /// Generates unique, time-ordered identifiers based on the Snowflake algorithm.
    /// </summary>
    public class Snowflake
    {
        private const int BitLenMachineID = 10;
        private const int BitLenSequence = 12;

        private readonly long _customEpoch;
        private readonly uint _machineID;

        private long _lastTimestamp;
        private int _sequence;

        private const uint _sequenceMask = (1 << BitLenSequence) - 1;
        private const int _timestampShift = BitLenMachineID + BitLenSequence;
        private const int _machineIDShift = BitLenSequence;

        /// <summary>
        /// Initializes a new instance of the Snowflake class with the specified settings.
        /// </summary>
        /// <param name="settings">The settings used to configure the Snowflake instance. If not provided, default settings will be used.</param>
        public Snowflake(Settings settings = null)
        {
            uint machineID = settings?.MachineID ?? 0;

            if (machineID > (1 << BitLenMachineID) - 1)
            {
                throw new InvalidMachineIdException($"Machine ID must be between 0 and {(1 << BitLenMachineID) - 1}, but received {machineID}.");
            }

            _machineID = machineID;

            if (settings?.CustomEpoch != null && settings.CustomEpoch.Value.UtcDateTime >= DateTimeOffset.UtcNow)
            {
                throw new InvalidCustomEpochException($"Custom epoch must be earlier than the start time. Provided custom epoch: {settings.CustomEpoch}, start time: {DateTimeOffset.UtcNow}.");
            }

            _customEpoch = settings?.CustomEpoch?.ToUnixTimeMilliseconds() ?? 0;

            _lastTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _customEpoch;

        }

        /// <summary>
        /// Generates a new unique ID.
        /// </summary>
        /// <returns>A new unique ID as a 64-bit unsigned integer.</returns>
        /// <exception cref="SnowflakeException">Thrown when an ID generation error occurs.</exception>
        public long NextID()
        {
            long elapsedTime;
            uint sequence;

            elapsedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _customEpoch;

            // Wait for the next millisecond if the current timestamp is the same as the last timestamp
            if (elapsedTime == _lastTimestamp)
            {
                SpinWait spinWait = new SpinWait();
                while (elapsedTime == _lastTimestamp)
                {
                    spinWait.SpinOnce();
                    elapsedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _customEpoch;
                }
            }

            // Atomically increment and get the sequence value
            sequence = (uint)Interlocked.Add(ref _sequence, 1) & _sequenceMask;

            // If the sequence has reached its maximum value, wait for the next millisecond
            if (sequence == _sequenceMask)
            {
                SpinWait spinWait = new SpinWait();
                while (true)
                {
                    long nextNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    long nextElapsedTime = nextNow - _customEpoch;

                    if (nextElapsedTime > elapsedTime)
                    {
                        elapsedTime = nextElapsedTime;
                        break;
                    }
                    spinWait.SpinOnce();
                }
            }

            Interlocked.Add(ref _lastTimestamp, elapsedTime - _lastTimestamp);

            // Construct the ID from the timestamp, machine ID, and sequence number
            ulong id = ((ulong)elapsedTime << _timestampShift) |
                       ((ulong)_machineID << _machineIDShift) |
                       sequence;

            // Apply a mask to unset the most significant bit
            return (long)(id & 0x7FFFFFFFFFFFFFFF);
        }
    }
}