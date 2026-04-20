using RobotController;
using System;

namespace RobotController.Commands
{
    /// <summary>
    /// The command that reports robot's current <see cref="Coordinate"/> and the first letter of the <see cref="Direction"/> it is facing.
    /// </summary>
    internal class ReportCommand : ICommand
    {
        /// <summary>
        /// Command's name.
        /// </summary>
        public string Name => "REPORT";

        /// <summary>
        /// Command's description.   
        /// </summary>
        public string Description => "Reports robot's current position and direction.";

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
            Console.WriteLine
                ($"{robot.CurrentPosition.X},{robot.CurrentPosition.Y},{robot.Facing.ToString().ToUpperInvariant()}");
            Success = true;

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
