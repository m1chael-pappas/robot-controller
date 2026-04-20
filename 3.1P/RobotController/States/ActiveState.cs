using RobotController.Commands;

namespace RobotController.States
{
    /// <summary>
    /// Robot's state <see cref="ActiveState"/> when it is placed on the map and can execute any valid command.
    /// </summary>
    internal class ActiveState : IState
    {
        IRobot _robot;

        /// <summary>
        /// Initializes a new <see cref="ActiveState"/> for a robot.
        /// </summary>
        /// <param name="robot"><see cref="IRobot"/> that the state belongs to.</param>
        public ActiveState(IRobot robot)
        {
            _robot = robot;
        }

        /// <summary>
        /// Executes <see cref="ICommand"/> on the <see cref="IRobot"/>.
        /// </summary>
        /// <param name="command"><see cref="ICommand"/> that need to be executed on the robot.</param>
        public void ExecuteCommand(ICommand command)
        {
            command.Execute(_robot);
        }
    }
}
