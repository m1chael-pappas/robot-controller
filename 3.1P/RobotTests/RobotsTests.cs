using Xunit;
using System.Collections.Generic;
using RobotController.States;
using RobotController;

namespace RobotTests
{
    public class RobotsTests
    {
        public static IEnumerable<object[]> GetPositionsFacingWalls()
        {
            yield return new object[] { 5, 1, 4, Direction.North };
            yield return new object[] { 5, 4, 4, Direction.East };
            yield return new object[] { 5, 0, 0, Direction.South };
            yield return new object[] { 5, 0, 0, Direction.West };
            yield return new object[] { 5, 0, 1, Direction.West };
        }

        public static IEnumerable<object[]> GetPositionsFacingFreeCoordinate()
        {
            yield return new object[] { 5, 1, 4, Direction.West };
            yield return new object[] { 5, 1, 4, Direction.East };
            yield return new object[] { 5, 0, 0, Direction.North };
            yield return new object[] { 5, 4, 4, Direction.South };
            yield return new object[] { 5, 4, 4, Direction.West };
        }

        [Theory]
        [MemberData(nameof(GetPositionsFacingWalls))]
        public void RobotCannotMoveAgainstTheWall(
            int mapSize, int x, int y, Direction facing)
        {
            var robot = new RobotController.Robot(new Map(mapSize, mapSize));
            robot.CurrentState = new ActiveState(robot);
            robot.CurrentPosition = new Coordinate(x, y);
            robot.Facing = facing;

            Assert.False(robot.CanMove);
        }

        [Theory]
        [MemberData(nameof(GetPositionsFacingFreeCoordinate))]
        public void RobotCanMoveFacingFreeCoordinate(
            int mapSize, int x, int y, Direction facing)
        {
            var robot = new RobotController.Robot(new Map(mapSize, mapSize));
            robot.CurrentState = new ActiveState(robot);
            robot.CurrentPosition = new Coordinate(x, y);
            robot.Facing = facing;

            Assert.True(robot.CanMove);
        }
    }
}
