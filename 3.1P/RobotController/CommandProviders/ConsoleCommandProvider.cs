using RobotController.Commands;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RobotController.CommandProviders
{
    /// <summary>
    /// <see cref="ConsoleCommandProvider"/> provides continuous input of robot commands from the user in the console.
    /// </summary>
    internal class ConsoleCommandProvider : IMetaCommandProvider
    {
        const string PLACE_REGEX = @"^PLACE[\s　-[\r\n]][0-4],[0-4],(NORTH|EAST|WEST|SOUTH)$";

        public string MetaCommand => ":console";

        /// <summary>
        /// Gets continuous input of robot commands from the user in the console.
        /// Unrecognised input is emitted as <see cref="UnknownCommand"/> so
        /// <see cref="DynamicCommandProvider"/> can route provider-switch directives
        /// like <c>:api &lt;url&gt;</c> or <c>:quit</c>.
        /// </summary>
        public IEnumerable<ICommand> GetCommands(string[] args = null)
        {
            string command;

            while (null != (command = Console.ReadLine()))
            {
                if (command == "MOVE")
                    yield return new MoveCommand();
                else if (command == "LEFT")
                    yield return new LeftCommand();
                else if (command == "RIGHT")
                    yield return new RightCommand();
                else if (command == "REPORT")
                    yield return new ReportCommand();
                else if (Regex.IsMatch(command, PLACE_REGEX))
                    yield return Deserialize(command);
                else
                    yield return new UnknownCommand(command);
            }
        }

        /// <summary>
        /// Deserializes <see cref="PlaceCommand" from the <see cref="string"/>/>
        /// </summary>
        /// <param name="command">Serialized <see cref="PlaceCommand"/> presentation.</param>
        /// <returns><see cref="PlaceCommand"/></returns>
        public static PlaceCommand Deserialize(string command)
        {
            string[] placeParams = command.Substring(6).Split(',');
            var x = int.Parse(placeParams[0]);
            var y = int.Parse(placeParams[1]);
            Direction facing = Direction.North;

            switch (placeParams[2])
            {
                case "NORTH": facing = Direction.North; break;
                case "EAST": facing = Direction.East; break;
                case "SOUTH": facing = Direction.South; break;
                case "WEST": facing = Direction.West; break;
                default: return null;
            }

            return new PlaceCommand(x, y, facing, command);
        }
    }
}
