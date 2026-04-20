using RobotController.Commands;
using RobotController.States;
using System.Collections.Generic;

namespace RobotController
{
    /// <summary>
    /// Toy <see cref="Robot"/> that waits for commands and interacts with the <see cref="Map"/>.
    /// </summary>
    public class Robot : IRobot
    {
        private List<ICommand> _commands = new List<ICommand>();

        /// <summary>
        /// The <see cref="Map"/> the robot is currently placed on.
        /// </summary>
        public Map CurrentMap { get; set; }

        /// <summary>
        /// Current robot's <see cref="Coordinate"/> on the <see cref="Map"/>.
        /// </summary>
        public Coordinate CurrentPosition { get; set; }

        /// <summary>
        /// <see cref="Direction"/> the robot is currently facing. 
        /// </summary>
        public Direction Facing { get; set; }

        /// <summary>
        /// <see cref="IState"/> the robot is currently in.
        /// </summary>
        public IState CurrentState { get; set; }

        /// <summary>
        /// Returns true if robot can move 1 <see cref="CurrentPosition"/> forward.
        /// </summary>
        public bool CanMove
        {
            get
            {
                if (CurrentMap == null)
                {
                    return false;
                }

                switch (Facing)
                {
                    case Direction.North:
                        return IsOnMap(CurrentPosition.X, CurrentPosition.Y + 1);
                    case Direction.East:
                        return IsOnMap(CurrentPosition.X + 1, CurrentPosition.Y);
                    case Direction.South:
                        return IsOnMap(CurrentPosition.X, CurrentPosition.Y - 1);
                    case Direction.West:
                        return IsOnMap(CurrentPosition.X - 1, CurrentPosition.Y);
                    default: return false;
                }
            }
        }

        public Robot()
        {

        }

        /// <summary>
        /// <see cref="Robot"/> constructor that initializes the <see cref="Map"/> for the robot to interact.
        /// </summary>
        /// <param name="map"></param>
        public Robot(Map map)
        {
            CurrentState = new IdleState(this);
            CurrentMap = map;
        }

        /// <summary>
        /// Executes <see cref="ICommand"/> on the robot.
        /// </summary>
        /// <param name="command"><see cref="ICommand"/> that need to be executed on the robot.</param>
        public void ExecuteCommand(ICommand command)
        {
            CurrentState.ExecuteCommand(command);
            _commands.Add(command);
        }

        /// <summary>
        /// Checks whether a certain coordinate is on the <see cref="Map"/>. 
        /// </summary>
        /// <param name="X">X coordinate.</param>
        /// <param name="Y">Y coordinate.</param>
        /// <returns>True if the a coordinate is on the <see cref="Map"/>.</returns>
        private bool IsOnMap(int X, int Y)
        {
            if (this.CurrentMap == null)
            {
                return false;
            }

            return X >= 0 && Y >= 0 && X < CurrentMap.Rows && Y < CurrentMap.Columns;
        }
    }
}
