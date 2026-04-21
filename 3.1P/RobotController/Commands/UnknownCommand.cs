namespace RobotController.Commands
{
    /// <summary>
    /// Carries raw unrecognised input through the command pipeline. DynamicCommandProvider
    /// inspects its <see cref="Input"/> to decide whether the line is a provider-switch
    /// directive (starts with ':') or genuine garbage; either way, executing the command
    /// itself is a no-op that records Success=false.
    /// </summary>
    internal class UnknownCommand : ICommand
    {
        public UnknownCommand(string input)
        {
            Input = input ?? string.Empty;
        }

        public string Input { get; }

        public string Name => "UNKNOWN";

        public string Description => "Unrecognised input preserved for routing or reporting.";

        public bool Executed { get; set; }

        public bool Success { get; set; }

        public bool Execute(IRobot robot)
        {
            Executed = true;
            return Success = false;
        }

        public override string ToString()
        {
            return Input;
        }
    }
}
