using RobotController;

namespace RobotController.Commands
{
    /// <summary>
    /// The command that moves robot 1 position forward in the direction it is currently facing.
    /// </summary>
    internal class MoveCommand : ICommand
    {
        /// <summary>
        /// Command's name.
        /// </summary>
        public string Name => "MOVE";

        /// <summary>
        /// Command's description.   
        /// </summary>
        public string Description => "Moves robot 1 cell in the direction it is facing.";

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
            if (robot.CanMove)
            {
                switch (robot.Facing)
                {
                    case Direction.North: robot.CurrentPosition.Y++; break;
                    case Direction.East: robot.CurrentPosition.X++; break;
                    case Direction.South: robot.CurrentPosition.Y--; break;
                    case Direction.West: robot.CurrentPosition.X--; break;
                }
                Success = true;
            }

            return Success;
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
