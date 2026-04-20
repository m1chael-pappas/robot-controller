using RobotController;

namespace RobotController.Commands
{
    /// <summary>
    /// The command that jumps the robot <c>steps</c> cells forward atomically —
    /// either the full jump succeeds or nothing happens. Contrasts with MOVE expansion
    /// (N independent MoveCommands) where partial success is allowed.
    /// </summary>
    internal class JumpForwardCommand : ICommand
    {
        private readonly int steps;

        public JumpForwardCommand(int steps)
        {
            this.steps = steps < 1 ? 1 : steps;
        }

        public string Name => "JUMP_FORWARD";

        public string Description => string.Format("Jumps forward {0} cells if possible.", steps);

        public bool Executed { get; set; }

        public bool Success { get; set; }

        public bool Execute(IRobot robot)
        {
            Executed = true;

            if (!(robot is Robot concreteRobot))
            {
                return Success = false;
            }

            if (!concreteRobot.CanMoveSteps(steps))
            {
                return Success = false;
            }

            switch (robot.Facing)
            {
                case Direction.North: robot.CurrentPosition.Y += steps; break;
                case Direction.East: robot.CurrentPosition.X += steps; break;
                case Direction.South: robot.CurrentPosition.Y -= steps; break;
                case Direction.West: robot.CurrentPosition.X -= steps; break;
            }

            return Success = true;
        }

        public override string ToString()
        {
            return Name + " " + steps;
        }
    }
}
