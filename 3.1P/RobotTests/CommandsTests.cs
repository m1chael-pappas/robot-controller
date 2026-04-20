using Xunit;
using RobotController.Commands;
using System.Collections.Generic;
using RobotController.States;
using RobotController;

namespace RobotTests
{
    public class CommandsTests
    {
        public static IEnumerable<object[]> GetFacingChangingCommands()
        {
            yield return new object[] { new LeftCommand() };
            yield return new object[] { new RightCommand() };
            yield return new object[] { new PlaceCommand(0, 0, Direction.North) };
        }

        public static IEnumerable<object[]> GetNonPlacementCommands()
        {
            yield return new object[] { new LeftCommand() };
            yield return new object[] { new RightCommand() };
            yield return new object[] { new ReportCommand() };
            yield return new object[] { new MoveCommand() };

        }

        [Theory]
        [MemberData(nameof(GetFacingChangingCommands))]
        public void TestActiveRobotReactingToFacingChangingCommands(
            ICommand sut)
        {
            var facing = Direction.East;
            var robot = new Robot(new Map(5, 5));
            robot.CurrentPosition = new Coordinate(0, 0);
            robot.Facing = facing;
            robot.CurrentState = new ActiveState(robot);

            robot.ExecuteCommand(sut);

            Assert.NotEqual(facing, robot.Facing);
        }

        [Theory]
        [MemberData(nameof(GetNonPlacementCommands))]
        public void TestIdleRobotIgnoringNonPlaceCommands(
            ICommand sut)
        {
            var robot = new RobotController.Robot();
            robot.CurrentState = new IdleState(robot);

            robot.ExecuteCommand(sut);

            Assert.IsType<IdleState>(robot.CurrentState);
            Assert.Null(robot.CurrentPosition);
        }

        [Theory]
        [InlineData(11, 11, Direction.North, 10)]
        [InlineData(11, 11, Direction.East, 5)]
        [InlineData(11, 11, Direction.South, 11)]
        [InlineData(-1, 5, Direction.South, 11)]
        [InlineData(5, -1, Direction.West, 11)]
        public void TestIdleRobotIgnoringInvalidPlaceCommands(
            int x, int y, Direction facing, int mapSize)
        {
            var robot = new RobotController.Robot(new Map(mapSize, mapSize));
            robot.CurrentState = new IdleState(robot);
            var sut = new PlaceCommand(x, y, facing);
            robot.ExecuteCommand(sut);

            Assert.IsType<IdleState>(robot.CurrentState);
            Assert.Null(robot.CurrentPosition);
        }

        [Theory]
        [InlineData(0, 0, Direction.North, 5)]
        [InlineData(4, 4, Direction.East, 5)]
        [InlineData(0, 4, Direction.South, 5)]
        [InlineData(4, 0, Direction.West, 5)]
        [InlineData(24, 24, Direction.West, 25)]
        public void TestIdleRobotReactingToValidPlaceCommands(
            int x, int y, Direction facing, int mapSize)
        {
            var robot = new RobotController.Robot(new Map(mapSize, mapSize));
            robot.CurrentState = new IdleState(robot);

            var sut = new PlaceCommand(x, y, facing);
            robot.ExecuteCommand(sut);

            Assert.IsType<ActiveState>(robot.CurrentState);
            Assert.NotNull(robot.CurrentPosition);
        }

        [Theory]
        [InlineData(4, 4, Direction.West, 5, 3, 3, Direction.East)]
        [InlineData(2, 2, Direction.South, 5, 1, 1, Direction.West)]
        [InlineData(0, 0, Direction.West, 5, 3, 3, Direction.East)]
        public void TestActiveRobotReactingToValidPlaceCommands(
            int x, int y, Direction facing, int mapSize, int prevX, int prevY, Direction prevFacing)
        {
            var robot = new RobotController.Robot(new Map(mapSize, mapSize));
            robot.CurrentState = new ActiveState(robot);
            robot.Facing = prevFacing;
            robot.CurrentPosition = new Coordinate(prevX, prevY);

            var sut = new PlaceCommand(x, y, facing);
            robot.ExecuteCommand(sut);

            Assert.IsType<ActiveState>(robot.CurrentState);
            Assert.NotNull(robot.CurrentPosition);
        }

        [Theory]
        [InlineData(5, 3, 3, Direction.North)]
        [InlineData(5, 3, 3, Direction.South)]
        [InlineData(5, 3, 3, Direction.East)]
        [InlineData(5, 3, 3, Direction.West)]
        public void TestActiveRobotReactingToValidMoveCommands(
            int mapSize, int prevX, int prevY, Direction prevFacing)
        {
            var robot = new RobotController.Robot(new Map(mapSize, mapSize));
            robot.CurrentState = new ActiveState(robot);
            robot.Facing = prevFacing;
            robot.CurrentPosition = new Coordinate(prevX, prevY);

            var sut = new MoveCommand();
            robot.ExecuteCommand(sut);

            Assert.IsType<ActiveState>(robot.CurrentState);
            Assert.NotEqual(robot.CurrentPosition.X + robot.CurrentPosition.Y, prevX + prevY);
            Assert.Equal(robot.Facing, prevFacing);
        }
    }
}
