using RobotController;

namespace RobotController.Commands
{
    /// <summary>
    /// The command that moves the robot 1 cell backwards (opposite of the direction it is facing).
    /// </summary>
    internal class StepBackCommand : ICommand
    {
        public string Name => "STEP_BACK";

        public string Description => "Moves robot 1 cell backwards, opposite to the direction it is facing.";

        public bool Executed { get; set; }

        public bool Success { get; set; }

        public bool Execute(IRobot robot)
        {
            Executed = true;

            if (robot.CurrentMap == null || robot.CurrentPosition == null)
            {
                return Success = false;
            }

            int newX = robot.CurrentPosition.X;
            int newY = robot.CurrentPosition.Y;

            switch (robot.Facing)
            {
                case Direction.North: newY -= 1; break;
                case Direction.East: newX -= 1; break;
                case Direction.South: newY += 1; break;
                case Direction.West: newX += 1; break;
            }

            if (!robot.CurrentMap.IsOnMap(newX, newY))
            {
                return Success = false;
            }

            robot.CurrentPosition.X = newX;
            robot.CurrentPosition.Y = newY;
            return Success = true;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
