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
        private readonly int _bitLenMachineID;
        private readonly int _bitLenSequence;

        private readonly long _customEpoch;
        private readonly uint _machineID;

        private long _lastTimestamp;
        private int _sequence;

        private readonly uint _sequenceMask;
        private readonly int _timestampShift;
        private readonly int _machineIDShift;

        private readonly long _maxTimestamp;

        /// <summary>
        /// Initializes a new instance of the Snowflake class with the specified settings.
        /// </summary>
        /// <param name="settings">The settings used to configure the Snowflake instance. If not provided, default settings will be used.</param>
        public Snowflake(Settings settings = null)
        {
            uint machineID = settings?.MachineID ?? 0;

            int machineIDBitLength = settings?.MachineIDBitLength ?? 10;
            int sequenceBitLength = settings?.SequenceBitLength ?? 12;

            if (sequenceBitLength < 1)
            {
                throw new InvalidBitLengthException("SequenceBitLength must be at least 1.");
            }

            if (machineIDBitLength + sequenceBitLength > 22)
            {
                throw new InvalidBitLengthException($"The sum of machine ID bit length and sequence bit length cannot exceed 22. Given machine ID bit length: {machineIDBitLength}, sequence bit length: {sequenceBitLength}.");
            }

            _bitLenMachineID = machineIDBitLength;
            _bitLenSequence = sequenceBitLength;

            if (machineID > (1u << _bitLenMachineID) - 1)
            {
                throw new InvalidMachineIdException($"Machine ID must be between 0 and {(1 << _bitLenMachineID) - 1}, but received {machineID}.");
            }

            _machineID = machineID;

            if (settings?.CustomEpoch != null && settings.CustomEpoch.Value.UtcDateTime >= DateTimeOffset.UtcNow)
            {
                throw new InvalidCustomEpochException($"Custom epoch must be earlier than the start time. Provided custom epoch: {settings.CustomEpoch}, start time: {DateTimeOffset.UtcNow}.");
            }

            _customEpoch = settings?.CustomEpoch?.ToUnixTimeMilliseconds() ?? 0;

            _lastTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _customEpoch;

            _sequenceMask = (1u << _bitLenSequence) - 1;

            _timestampShift = _bitLenMachineID + _bitLenSequence;

            _machineIDShift = _bitLenSequence;

            try
            {
                checked
                {
                    _maxTimestamp = (1L << (64 - _timestampShift)) - 1 + _customEpoch;
                }
            }
            catch (OverflowException)
            {
                throw new InvalidCustomEpochException("The custom epoch is too large, causing the maximum timestamp to overflow. Please provide a smaller custom epoch.");
            }
        }

        /// <summary>
        /// Generates a new unique ID.
        /// </summary>
        /// <returns>A new unique ID as a 64-bit signed integer.</returns>
        /// <exception cref="SnowflakeException">Thrown when an ID generation error occurs.</exception>
        public long NextID()
        {
            long elapsedTime;
            uint sequence;

            elapsedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _customEpoch;

            // Atomically attempt to reset the sequence value to 0
            if (elapsedTime != _lastTimestamp && _sequence != 0)
            {
                sequence = (uint)Interlocked.CompareExchange(ref _sequence, 0, _sequence);

                if (sequence != 0u)
                {
                    // Atomically increment and get the sequence value
                    sequence = (uint)Interlocked.Add(ref _sequence, 1) & _sequenceMask;
                }
            }
            else
            {
                sequence = (uint)Interlocked.Add(ref _sequence, 1) & _sequenceMask;
            }

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

            // Check if the timestamp has exceeded its limit
            if (elapsedTime >= _maxTimestamp)
            {
                throw new TimestampOverflowException("The timestamp has exceeded its limit. Unable to generate a new Snowflake ID.");
            }

            Interlocked.Add(ref _lastTimestamp, elapsedTime - _lastTimestamp);

            // Construct the ID from the timestamp, machine ID, and sequence number
            ulong id = ((ulong)elapsedTime << _timestampShift) |
                       ((ulong)_machineID << _machineIDShift) |
                       sequence;

            // Apply a mask to unset the most significant bit
            return (long)(id & 0x7FFFFFFFFFFFFFFF);
        }

        public (long Timestamp, uint MachineID, uint Sequence) DecodeID(long id)
        {
            long timestampMask = (1L << (64 - _timestampShift)) - 1;
            long machineIDMask = (1L << _bitLenMachineID) - 1;
            long sequenceMask = (1L << _bitLenSequence) - 1;

            long timestamp = (id >> _timestampShift) & timestampMask;
            uint machineID = (uint)((id >> _machineIDShift) & machineIDMask);
            uint sequence = (uint)(id & sequenceMask);

            return (timestamp, machineID, sequence);
        }
    }
}