using RobotController.Commands;
using RobotController;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Text.RegularExpressions;

namespace RobotController.CommandProviders
{
    /// <summary>
    /// <see cref="FileCommandProvider"/> provides continuous input of robot commands from the file.
    /// </summary>
    internal class FileCommandProvider : IMetaCommandProvider
    {
        IFileSystem _fileSystem;

        public FileCommandProvider(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public FileCommandProvider() : this(new FileSystem())
        {

        }

        public string MetaCommand => ":file";

        const string PLACE_REGEX = @"^PLACE[\s　-[\r\n]][0-4],[0-4],(NORTH|EAST|WEST|SOUTH)$";

        /// <summary>
        /// Gets continuous input of robot commands from the file. Colon-prefixed lines are
        /// emitted as <see cref="UnknownCommand"/> so a file can route to other providers
        /// via <see cref="DynamicCommandProvider"/>; other unrecognised lines are silently
        /// skipped (preserves legacy file-ignore semantics asserted by existing tests).
        /// </summary>
        public IEnumerable<ICommand> GetCommands(string[] args)
        {
            var fileName = args[0];

            var lines = _fileSystem.File.ReadLines(fileName);
            foreach (var line in lines)
            {
                string command = line;

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
                else if (!string.IsNullOrEmpty(command) && command[0] == ':')
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
