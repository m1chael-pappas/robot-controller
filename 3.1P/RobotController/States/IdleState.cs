using RobotController.Commands;

namespace RobotController.States
{
    /// <summary>
    /// Robot's state when it is not placed on the map and can execute <see cref="PlaceCommand"/> only.
    /// </summary>
    internal class IdleState : IState
    {
        IRobot _robot;

        /// <summary>
        /// Initializes a new <see cref="IdleState"/> for a robot.
        /// </summary>
        /// <param name="robot"><see cref="IRobot"/> that the state belongs to.</param>
        public IdleState(IRobot robot)
        {
            _robot = robot;
        }

        /// <summary>
        /// Executes <see cref="ICommand"/> on the <see cref="IRobot"/>.
        /// </summary>
        /// <param name="command"><see cref="ICommand"/> that need to be executed on the robot.</param>
        public void ExecuteCommand(ICommand command)
        {
            if (!(command is PlaceCommand)) return;
            
            command.Execute(_robot);
            if (command.Success)
                _robot.CurrentState = new ActiveState(_robot);
        }
    }
}
