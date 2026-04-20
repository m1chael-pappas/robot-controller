using Xunit;
using RobotController.Commands;
using System.Collections.Generic;
using RobotController.States;
using RobotController;

namespace RobotTests
{
    public class StatesTests
    {
        public static IEnumerable<object[]> GetInvalidCommandsForIdleState()
        {
            yield return new object[] { new LeftCommand() };
            yield return new object[] { new RightCommand() };
            yield return new object[] { new ReportCommand() };
            yield return new object[] { new MoveCommand() };
            yield return new object[] { new PlaceCommand(-1, -1, Direction.North) };
        }

        [Fact]
        public void TestStateTransitionAfterValidCommand()
        {
            int x = 1, y = 1;
            var command = new PlaceCommand(x, y, Direction.North);
            var robot = new Robot(new Map(x+1, y+1));
            
            var sut = new IdleState(robot);

            sut.ExecuteCommand(command);

            Assert.IsType<ActiveState>(robot.CurrentState); 
        }

        [Theory]
        [MemberData(nameof(GetInvalidCommandsForIdleState))]
        public void TestStateNotChangingForInvalidCommands(
            ICommand invalidCommand)
        {
            var robot = new Robot(new Map(5, 5));

            var sut = new IdleState(robot);

            sut.ExecuteCommand(invalidCommand);

            Assert.IsType<IdleState>(robot.CurrentState);
        }
    }
}
