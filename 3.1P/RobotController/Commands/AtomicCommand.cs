using System.Collections.Generic;
using System.Linq;

namespace RobotController.Commands
{
    /// <summary>
    /// All-or-nothing workflow. Clones the robot, dry-runs every inner command on the clone
    /// (suppressing REPORT during simulation), and only commits on the real robot if the
    /// simulation reported no failures.
    /// </summary>
    internal class AtomicCommand : ICommand
    {
        private readonly List<ICommand> commands;

        public AtomicCommand(IEnumerable<ICommand> commands)
        {
            this.commands = commands.ToList();
        }

        public string Name => "WORKFLOW";

        public string Description => "All-or-nothing workflow execution.";

        public bool Executed { get; set; }

        public bool Success { get; set; }

        public bool Execute(IRobot robot)
        {
            Executed = true;

            var realRobot = robot as AdvancedRobot;
            if (realRobot == null)
                return Success = false;

            var simRobot = AdvancedRobot.Clone(realRobot);

            foreach (var command in commands.Where(c => !(c is ReportCommand)))
            {
                simRobot.ExecuteCommand(command);
            }

            if (simRobot.CommandHistory.Any(c => !c.Success))
                return Success = false;

            foreach (var command in commands)
            {
                command.Executed = false;
                command.Success = false;
                realRobot.ExecuteCommand(command);
                if (!command.Success)
                    return Success = false;
            }

            return Success = true;
        }
    }
}
