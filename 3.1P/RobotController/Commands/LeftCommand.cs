using RobotController;

namespace RobotController.Commands
{
    /// <summary>
    /// The command that turns robot left.
    /// </summary>
    internal class LeftCommand : ICommand
    {
        /// <summary>
        /// Command's name.
        /// </summary>
        public string Name => "LEFT";

        /// <summary>
        /// Command's description.
        /// </summary>
        public string Description => "Turns robot left.";

        /// <summary>
        /// Shows whether the command has been executed.
        /// </summary>
        public bool Executed { get; set; }

        /// <summary>
        /// Shows whether the command has run successfully.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <returns>True if the command has run successfully.</returns>
        public bool Execute(IRobot robot)
        {
            Executed = true;
            switch (robot.Facing)
            {
                case Direction.North:
                    robot.Facing = Direction.West;
                    break;
                case Direction.East:
                    robot.Facing = Direction.North;
                    break;
                case Direction.South:
                    robot.Facing = Direction.East;
                    break;
                case Direction.West:
                    robot.Facing = Direction.South;
                    break;
            }

            return Success = true;
        }

        /// <summary>
        /// String presentation of <see cref="LeftCommand"/>.
        /// </summary>
        /// <returns><see cref="Name"/></returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
