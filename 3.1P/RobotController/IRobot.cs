using RobotController.Commands;
using RobotController.States;
using RobotController;

namespace RobotController
{
    /// <summary>
    /// Common robot interface.
    /// </summary>
    public interface IRobot
    {
        /// <summary>
        /// <see cref="IState"/> the robot is currently in.
        /// </summary>
        IState CurrentState {  get; set; }

        /// <summary>
        /// The <see cref="Map"/> the robot is currently placed on.
        /// </summary>
        Map CurrentMap { get; set; }

        /// <summary>
        /// Current robot's <see cref="Coordinate"/> on the <see cref="Map"/>.
        /// </summary>
        Coordinate CurrentPosition {  get; set; }

        /// <summary>
        /// <see cref="Direction"/> the robot is currently facing. 
        /// </summary>
        Direction Facing { get; set; }

        /// <summary>
        /// Returns true if robot can move.
        /// </summary>
        bool CanMove {  get; }

        /// <summary>
        /// Executes <see cref="ICommand"/> on the robot.
        /// </summary>
        /// <param name="command"><see cref="ICommand"/> that need to be executed on the robot.</param>
        void ExecuteCommand(ICommand command);
    }
}