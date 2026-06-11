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
        private readonly object _lock = new object();

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

            if (machineIDBitLength < 0)
            {
                throw new InvalidBitLengthException("MachineIDBitLength cannot be negative.");
            }

            if (machineIDBitLength + sequenceBitLength > 22)
            {
                throw new InvalidBitLengthException($"The sum of machine ID bit length and sequence bit length cannot exceed 22. Given machine ID bit length: {machineIDBitLength}, sequence bit length: {sequenceBitLength}.");
            }

            _bitLenMachineID = machineIDBitLength;
            _bitLenSequence = sequenceBitLength;

            if (machineID > (1u << _bitLenMachineID) - 1)
            {
                throw new InvalidMachineIdException($"Machine ID must be between 0 and {(1u << _bitLenMachineID) - 1}, but received {machineID}.");
            }

            _machineID = machineID;

            if (settings?.CustomEpoch != null && settings.CustomEpoch.Value.UtcDateTime >= DateTimeOffset.UtcNow)
            {
                throw new InvalidCustomEpochException($"Custom epoch must be earlier than the start time. Provided custom epoch: {settings.CustomEpoch}, start time: {DateTimeOffset.UtcNow}.");
            }

            _customEpoch = settings?.CustomEpoch?.ToUnixTimeMilliseconds() ?? 0;

            // Sentinel meaning "no ID generated yet" so the first NextID() of this millisecond starts the sequence at 0.
            _lastTimestamp = -1L;

            _sequenceMask = (1u << _bitLenSequence) - 1;

            _timestampShift = _bitLenMachineID + _bitLenSequence;

            _machineIDShift = _bitLenSequence;

            _maxTimestamp = (1L << (63 - _timestampShift)) - 1;
        }

        /// <summary>
        /// Generates a new unique ID.
        /// </summary>
        /// <returns>A new unique ID as a 64-bit signed integer.</returns>
        /// <remarks>
        /// This method is thread-safe; concurrent callers are serialized so that no two IDs are ever identical.
        /// To preserve that guarantee the system clock is never allowed to move backwards: if the clock is stepped
        /// back (for example by an aggressive NTP adjustment) the generator continues from the last observed
        /// timestamp. If the per-millisecond sequence is also exhausted during such a regression, the call spins
        /// until real time advances past the last observed timestamp — never issuing a duplicate.
        /// </remarks>
        /// <exception cref="TimestampOverflowException">Thrown when the timestamp has exceeded the range representable by the ID layout.</exception>
        public long NextID()
        {
            lock (_lock)
            {
                long elapsedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _customEpoch;

                // Guard against the system clock moving backwards (e.g. NTP adjustment).
                if (elapsedTime < _lastTimestamp)
                {
                    elapsedTime = _lastTimestamp;
                }

                if (elapsedTime == _lastTimestamp)
                {
                    _sequence = (int)((_sequence + 1) & _sequenceMask);

                    // Sequence exhausted for this millisecond; wait for the next one.
                    if (_sequence == 0)
                    {
                        SpinWait spinWait = new SpinWait();
                        do
                        {
                            spinWait.SpinOnce();
                            elapsedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _customEpoch;
                        } while (elapsedTime <= _lastTimestamp);
                    }
                }
                else
                {
                    _sequence = 0;
                }

                // Check if the timestamp has exceeded its limit
                if (elapsedTime >= _maxTimestamp)
                {
                    throw new TimestampOverflowException("The timestamp has exceeded its limit. Unable to generate a new Snowflake ID.");
                }

                _lastTimestamp = elapsedTime;

                // Construct the ID from the timestamp, machine ID, and sequence number
                return (elapsedTime << _timestampShift) |
                       ((long)_machineID << _machineIDShift) |
                       (uint)_sequence;
            }
        }

        /// <summary>
        /// Decodes a Snowflake ID into its constituent parts.
        /// </summary>
        /// <param name="id">The Snowflake ID to decode.</param>
        /// <returns>
        /// A tuple containing the epoch-relative timestamp (in milliseconds since the configured custom epoch),
        /// the machine ID, and the sequence number encoded in the ID.
        /// </returns>
        public (long Timestamp, uint MachineID, uint Sequence) DecodeID(long id)
        {
            long timestampMask = (1L << (63 - _timestampShift)) - 1;
            long machineIDMask = (1L << _bitLenMachineID) - 1;
            long sequenceMask = (1L << _bitLenSequence) - 1;

            long timestamp = (id >> _timestampShift) & timestampMask;
            uint machineID = (uint)((id >> _machineIDShift) & machineIDMask);
            uint sequence = (uint)(id & sequenceMask);

            return (timestamp, machineID, sequence);
        }
    }
}