using RobotController.CommandProviders;
using RobotController.States;
using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("RobotTests")]
namespace RobotController
{
    class Program
    {
        static void Main(string[] args)
        {
            /// Future improvements:
            /// TODO: Use DI container.
            /// TODO: Move literals to app.config
            /// TODO: Use System.CommandLine to parse args passed into Main.
            /// TODO: Use automocking library to manage mocks in tests.
            /// TODO: Add DI GUI interface to be able to switch between different GUIs:
            /// Console result only, Console draw map, Web app, mobile app
            
            ICommandProvider cp;
            if (args.Length == 0)
            {
                cp = new ConsoleCommandProvider();
                Console.WriteLine("Robot is not placed on the map and is waiting for commands.");
                Console.WriteLine("Please type one of the supported commands from the robot manual.");
            }
            else
            {
                cp = new FileCommandProvider();
            }

            var map = new Map(10, 10);
            var robot = new Robot(map);
            robot.CurrentState = new IdleState(robot);

            foreach (var command in cp.GetCommands(args))
            {
                robot.ExecuteCommand(command);
            }
        }
    }
}
