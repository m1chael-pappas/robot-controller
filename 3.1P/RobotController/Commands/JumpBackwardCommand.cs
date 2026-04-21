using RobotController;

namespace RobotController.Commands
{
    /// <summary>
    /// Antipode of <see cref="JumpForwardCommand"/>: jumps the robot <c>steps</c> cells
    /// opposite to its facing atomically. Used by the server-generated rollback workflow
    /// to compensate a forward jump from the original execution.
    /// </summary>
    internal class JumpBackwardCommand : ICommand
    {
        private readonly int steps;

        public JumpBackwardCommand(int steps)
        {
            this.steps = steps < 1 ? 1 : steps;
        }

        public int Steps => steps;

        public string Name => "JUMP_BACKWARD";

        public string Description => string.Format("Jumps backward {0} cells if possible.", steps);

        public bool Executed { get; set; }

        public bool Success { get; set; }

        public bool Execute(IRobot robot)
        {
            Executed = true;

            if (robot.CurrentMap == null || robot.CurrentPosition == null)
            {
                return Success = false;
            }

            int newX = robot.CurrentPosition.X;
            int newY = robot.CurrentPosition.Y;

            switch (robot.Facing)
            {
                case Direction.North: newY -= steps; break;
                case Direction.East: newX -= steps; break;
                case Direction.South: newY += steps; break;
                case Direction.West: newX += steps; break;
            }

            if (!robot.CurrentMap.IsOnMap(newX, newY))
            {
                return Success = false;
            }

            robot.CurrentPosition.X = newX;
            robot.CurrentPosition.Y = newY;
            return Success = true;
        }

        public override string ToString()
        {
            return Name + " " + steps;
        }
    }
}
