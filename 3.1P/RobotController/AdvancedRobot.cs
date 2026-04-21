using RobotController.Commands;
using RobotController.States;
using System.Collections.Generic;

namespace RobotController
{
    /// <summary>
    /// <see cref="AdvancedRobot"/> extends the robot model with a read-only command history
    /// and cloning support, which AtomicCommand uses to dry-run workflows before committing.
    /// </summary>
    public class AdvancedRobot : IRobot
    {
        private readonly List<ICommand> commandHistory = new List<ICommand>();

        public IReadOnlyList<ICommand> CommandHistory
        {
            get { return commandHistory.AsReadOnly(); }
        }

        public Map CurrentMap { get; set; }

        public Coordinate CurrentPosition { get; set; }

        public Direction Facing { get; set; }

        public IState CurrentState { get; set; }

        public bool CanMove
        {
            get
            {
                if (CurrentMap == null || CurrentPosition == null)
                    return false;

                switch (Facing)
                {
                    case Direction.North:
                        return CurrentMap.IsOnMap(CurrentPosition.X, CurrentPosition.Y + 1);
                    case Direction.East:
                        return CurrentMap.IsOnMap(CurrentPosition.X + 1, CurrentPosition.Y);
                    case Direction.South:
                        return CurrentMap.IsOnMap(CurrentPosition.X, CurrentPosition.Y - 1);
                    case Direction.West:
                        return CurrentMap.IsOnMap(CurrentPosition.X - 1, CurrentPosition.Y);
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// Returns true if the robot can move <paramref name="steps"/> cells forward.
        /// Named <c>CanMoveSteps</c> rather than overloading <c>CanMove</c> because C# does not allow
        /// a property and a method to share a name.
        /// </summary>
        public bool CanMoveSteps(int steps)
        {
            if (steps < 1) steps = 1;
            if (CurrentMap == null || CurrentPosition == null) return false;

            switch (Facing)
            {
                case Direction.North:
                    return CurrentMap.IsOnMap(CurrentPosition.X, CurrentPosition.Y + steps);
                case Direction.East:
                    return CurrentMap.IsOnMap(CurrentPosition.X + steps, CurrentPosition.Y);
                case Direction.South:
                    return CurrentMap.IsOnMap(CurrentPosition.X, CurrentPosition.Y - steps);
                case Direction.West:
                    return CurrentMap.IsOnMap(CurrentPosition.X - steps, CurrentPosition.Y);
                default:
                    return false;
            }
        }

        public AdvancedRobot(Map map)
        {
            CurrentMap = map;
            CurrentState = new IdleState(this);
        }

        public void ExecuteCommand(ICommand command)
        {
            // Workflow wrappers (AtomicCommand) and transport sentinels (PostExecutionLogCommand)
            // bypass the state machine — they are infrastructure, not robot actions, and they
            // must run even when the robot is in IdleState (e.g. dry-run failed before PLACE).
            // AtomicCommand's inner commands still traverse the state machine: PLACE inside a
            // workflow triggers the Idle→Active transition normally.
            if (command is AtomicCommand || command is PostExecutionLogCommand)
            {
                command.Execute(this);
                commandHistory.Add(command);
                return;
            }

            CurrentState.ExecuteCommand(command);
            commandHistory.Add(command);
        }

        /// <summary>
        /// Produces a deep-enough copy of <paramref name="original"/> so AtomicCommand can dry-run a
        /// workflow without mutating the real robot. Preserves map, position, facing and current state.
        /// </summary>
        public static AdvancedRobot Clone(AdvancedRobot original)
        {
            var clone = new AdvancedRobot(original.CurrentMap);
            if (original.CurrentPosition != null)
            {
                clone.CurrentPosition = new Coordinate(
                    original.CurrentPosition.X,
                    original.CurrentPosition.Y);
            }
            clone.Facing = original.Facing;

            if (original.CurrentState is ActiveState)
            {
                clone.CurrentState = new ActiveState(clone);
            }
            else
            {
                clone.CurrentState = new IdleState(clone);
            }

            return clone;
        }
    }
}
