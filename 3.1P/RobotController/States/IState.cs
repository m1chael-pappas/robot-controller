using RobotController.Commands;

namespace RobotController.States
{
    /// <summary>
    /// Robot's state interface.
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// Executes <see cref="ICommand"/> on the robot.
        /// </summary>
        /// <param name="command"><see cref="ICommand"/> that need to be executed on the robot.</param>
        void ExecuteCommand(ICommand command);
    }
}
