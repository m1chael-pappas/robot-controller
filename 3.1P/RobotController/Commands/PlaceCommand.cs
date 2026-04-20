using RobotController;

namespace RobotController.Commands
{
    /// <summary>
    /// The command that places the robot on a certan <see cref="Coordinate"/> within the <see cref="Map"/> facing a certain <see cref="Direction"/>.
    /// </summary>
    internal class PlaceCommand : ICommand
    {
        private readonly int _x;
        private readonly int _y;
        private readonly string _commandInput;
        private readonly Direction _facing;

        /// <summary>
        /// <see cref="PlaceCommand"/> constructor.
        /// </summary>
        /// <param name="x">Robot's <see cref="Coordinate.X"/>.</param>
        /// <param name="y">Robot's <see cref="Coordinate.Y"/></param>
        /// <param name="facing"><see cref="Direction"/> the report is facing.</param>
        public PlaceCommand(int x, int y, Direction facing)
        {
            _x = x;
            _y = y;
            _facing = facing;
        }

        /// <summary>
        /// <see cref="PlaceCommand"/> constructor.
        /// </summary>
        /// <param name="x">Robot's <see cref="Coordinate.X"/>.</param>
        /// <param name="y">Robot's <see cref="Coordinate.Y"/></param>
        /// <param name="facing"><see cref="Direction"/> the robot is facing.</param>
        /// <param name="commandInput">Original input used to create <see cref="PlaceCommand"/>.</param>
        public PlaceCommand(int x, int y, Direction facing, string commandInput)
        {
            _x = x;
            _y = y;
            _facing = facing;
            _commandInput = commandInput;
        }

        /// <summary>
        /// Command's name.
        /// </summary>
        public string Name => "PLACE";

        /// <summary>
        /// Command's description.   
        /// </summary>
        public string Description => "Places the robot on a specific coordinate facing either NORTH, EAST, SOUTH or WEST.";

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

            if (robot.CurrentMap == null || !robot.CurrentMap.IsOnMap(_x, _y))
            {
                return false;
            }

            robot.CurrentPosition = new Coordinate(_x, _y);
            robot.Facing = _facing;
            return Success = true;
        }

        /// <summary>
        /// String presentation of <see cref="LeftCommand"/>.
        /// </summary>
        /// <returns><see cref="Name"/></returns>
        public override string ToString()
        {
            return string.IsNullOrEmpty(_commandInput) ? Name : _commandInput;
        }
    }
}
