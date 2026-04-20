using Xunit;
using RobotController.CommandProviders;
using RobotController.Commands;
using System.IO.Abstractions.TestingHelpers;
using System.Collections.Generic;

namespace RobotTests
{
    public class CommandProvidersTests
    {
        [Theory]
        [InlineData(0, 0, "NORTH")]
        [InlineData(1, 1, "SOUTH")]
        [InlineData(2, 2, "WEST")]
        [InlineData(3, 3, "EAST")]
        [InlineData(4, 4, "NORTH")]
        [InlineData(-5, 777, "SOUTH")]
        public void ValidFormatPlaceCommandDeserializationInConsoleCommandProviderTest(int x, int y, string facing)
        {
            var command = $"PLACE {x},{y},{facing}";
            var placementCommand = ConsoleCommandProvider.Deserialize(command);
            Assert.NotNull(placementCommand);
            Assert.IsType<PlaceCommand>(placementCommand);
        }

        [Theory]
        [InlineData(0, 0, "NORTH")]
        [InlineData(1, 1, "SOUTH")]
        [InlineData(2, 2, "WEST")]
        [InlineData(3, 3, "EAST")]
        [InlineData(4, 4, "NORTH")]
        [InlineData(-5, 777, "SOUTH")]
        public void ValidFormatPlaceCommandDeserializationInFileCommandProviderTest(int x, int y, string facing)
        {
            var command = $"PLACE {x},{y},{facing}";
            var placementCommand = FileCommandProvider.Deserialize(command);
            Assert.NotNull(placementCommand);
            Assert.IsType<PlaceCommand>(placementCommand);
        }

        [Theory]
        [InlineData("HELLO")]
        [InlineData("WORLD")]
        [InlineData(" PLACE ")]
        [InlineData("PLACE 1,0")]
        [InlineData("PLACE 1,0,N")]
        public void InvalidCommandsInFileIgnoredTest(string fileData)
        {
            var args = new string[] { @"./commands.txt" };
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { args[0], new MockFileData(fileData) }
            });
            var fcp = new FileCommandProvider(fileSystem);

            Assert.Empty(fcp.GetCommands(args));
        }

        [Theory]
        [InlineData("MOVE")]
        [InlineData("REPORT")]
        [InlineData("LEFT")]
        [InlineData("RIGHT")]
        [InlineData("PLACE 0,0,NORTH")]
        [InlineData("PLACE 4,4,SOUTH")]
        public void ValidCommandsInFileGetThroughTest(string fileData)
        {
            var args = new string[] { @"./commands.txt" };
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { args[0], new MockFileData(fileData) }
            });
            var fcp = new FileCommandProvider(fileSystem);

            Assert.NotEmpty(fcp.GetCommands(args));
            Assert.All(fcp.GetCommands(args), x => Assert.IsAssignableFrom<ICommand>(x));
        }
    }
}
