using SnowflakeGenerator.Exceptions;
using System.Collections.Concurrent;
using Xunit;

namespace SnowflakeGenerator.Tests
{
    public class SnowflakeTests
    {
        [Fact]
        [Trait("Category", "Unit")]
        public void NextID_GeneratesUniqueIDs()
        {
            // Arrange
            var snowflake = new Snowflake();

            // Act
            var id1 = snowflake.NextID();
            var id2 = snowflake.NextID();

            // Assert
            Assert.NotEqual(id1, id2);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void NextID_WithCustomMachineID_GeneratesUniqueIDs()
        {
            // Arrange
            var settings = new Settings { MachineID = 1 };
            var snowflake = new Snowflake(settings);

            // Act
            var id1 = snowflake.NextID();
            var id2 = snowflake.NextID();

            // Assert
            Assert.NotEqual(id1, id2);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void NextID_WithCustomEpoch_GeneratesUniqueIDs()
        {
            // Arrange
            var settings = new Settings { CustomEpoch = DateTimeOffset.UtcNow.AddMinutes(-1) };
            var snowflake = new Snowflake(settings);

            // Act
            var id1 = snowflake.NextID();
            var id2 = snowflake.NextID();

            // Assert
            Assert.NotEqual(id1, id2);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void NextID_WithCustomMachineIDBitLength_GeneratesUniqueIDs()
        {
            // Arrange
            var settings = new Settings { MachineIDBitLength = 8};
            var snowflake = new Snowflake(settings);

            // Act
            var id1 = snowflake.NextID();
            var id2 = snowflake.NextID();

            // Assert
            Assert.NotEqual(id1, id2);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void NextID_WithCustomSequenceBitLength_GeneratesUniqueIDs()
        {
            // Arrange
            var settings = new Settings {SequenceBitLength = 8};
            var snowflake = new Snowflake(settings);

            // Act
            var id1 = snowflake.NextID();
            var id2 = snowflake.NextID();

            // Assert
            Assert.NotEqual(id1, id2);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Constructor_WithInvalidMachineID_ThrowsInvalidMachineIdException()
        {
            // Arrange
            var settings = new Settings { MachineID = 1 << 10 };

            // Act & Assert
            Assert.Throws<InvalidMachineIdException>(() => new Snowflake(settings));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Constructor_WithInvalidCustomEpoch_ThrowsInvalidCustomEpochException()
        {
            // Arrange
            var settings = new Settings { CustomEpoch = DateTimeOffset.UtcNow.AddMinutes(1) };

            // Act & Assert
            Assert.Throws<InvalidCustomEpochException>(() => new Snowflake(settings));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Constructor_WithInvalidSequenceBitLength_ThrowsInvalidBitLengthException()
        {
            // Arrange
            var settings = new Settings { MachineIDBitLength = 8, SequenceBitLength = 0 };

            // Act & Assert
            Assert.Throws<InvalidBitLengthException>(() => new Snowflake(settings));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Constructor_WithInvalidMachineIDBitLength_ThrowsInvalidBitLengthException()
        {
            // Arrange
            var settings = new Settings { MachineIDBitLength = 25, SequenceBitLength = 2 };

            // Act & Assert
            Assert.Throws<InvalidBitLengthException>(() => new Snowflake(settings));
        }

        [Fact]
        [Trait("Category", "Stress")]
        public void MaximumMachineIdTest()
        {
            var settings = new Settings { MachineID = (1 << 10) - 1 };
            Snowflake snowflake = null;
            Exception exception = Record.Exception(() => snowflake = new Snowflake(settings));

            Assert.Null(exception);
            Assert.NotNull(snowflake);
        }

        [Fact]
        [Trait("Category", "Stress")]
        public async Task MaximumSequenceNumberTest()
        {
            var settings = new Settings();
            var snowflake = new Snowflake(settings);

            var ids = new HashSet<long>();
            int idCount = (1 << 12) + 100;

            for (int i = 0; i < idCount; i++)
            {
                long id = snowflake.NextID();
                Assert.DoesNotContain(id, ids);
                ids.Add(id);
            }
        }

        [Fact]
        [Trait("Category", "Stress")]
        public void RapidIdGenerationTest()
        {
            var settings = new Settings();
            var snowflake = new Snowflake(settings);

            int idCount = 10_000;
            var ids = new HashSet<long>();

            for (int i = 0; i < idCount; i++)
            {
                long id = snowflake.NextID();
                Assert.DoesNotContain(id, ids);
                ids.Add(id);
            }
        }

        [Fact]
        [Trait("Category", "Stress")]
        public async Task TimestampOrderTest()
        {
            var settings = new Settings();
            var snowflake = new Snowflake(settings);
            var ids = new List<long>();
            int idCount = 100;

            for (int i = 0; i < idCount; i++)
            {
                long id = snowflake.NextID();
                ids.Add(id);
                await Task.Delay(1);
            }

            var sortedIds = ids.OrderBy(id => id).ToList();
            Assert.Equal(ids, sortedIds);
        }

        [Fact]
        [Trait("Category", "Stress")]
        public async Task MultiThreadedUniquenessTest()
        {
            var settings = new Settings();
            var snowflake = new Snowflake(settings);
            var ids = new ConcurrentBag<long>();
            int idCount = 1000;
            int threadCount = 10;

            Task GenerateIds()
            {
                for (int i = 0; i < idCount; i++)
                {
                    ids.Add(snowflake.NextID());
                }
                return Task.CompletedTask;
            }

            var tasks = new List<Task>();

            for (int i = 0; i < threadCount; i++)
            {
                tasks.Add(GenerateIds());
            }

            await Task.WhenAll(tasks);

            // Check uniqueness
            var uniqueIds = new HashSet<long>(ids);
            Assert.Equal(ids.Count, uniqueIds.Count);
        }
    }
}