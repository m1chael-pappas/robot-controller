namespace RobotController
{
    /// <summary>
    /// Enumeration presenting sides of the world: <see cref="North"/>, <see cref="East"/>, <see cref="South"/> and <see cref="West"/>.
    /// </summary>
    public enum Direction
    {
        North,
        East,
        South,
        West
    }

    /// <summary>
    /// Playing map of a certain size 
    /// </summary>
    public class Map
    {
        /// <summary>
        /// Number of <see cref="Columns"/> in the <see cref="Map"/>. 
        /// </summary>
        public int Columns { get; }

        /// <summary>
        /// Number of <see cref="Rows"/> in the <see cref="Map"/>. 
        /// </summary>
        public int Rows { get; }

        /// <summary>
        /// Constructor that initializes the map of a certain size
        /// </summary>
        /// <param name="rows">Sets the number of <see cref="Rows"/>.</param>
        /// <param name="cols">Sets the number of <see cref="Columns"/>.</param>
        public Map(int rows, int cols)
        {
            Rows = rows;
            Columns = cols;
        }

        /// <summary>
        /// Checks whether a certain coordinate is on <see cref="Map"/>
        /// </summary>
        /// <param name="X">X coordinate.</param>
        /// <param name="Y">Y coordinate.</param>
        /// <returns>True if the coodinate is on <see cref="Map"/>.</returns>
        public bool IsOnMap(int X, int Y)
        {
            return X >= 0 && Y >= 0 && X < Rows && Y < Columns;
        }
    }

    /// <summary>
    /// Position on the map. 
    /// </summary>
    public class Coordinate
    {
        /// <summary>
        /// X coordinate.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Y coordinate.
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Initializes a certain coodinate.
        /// </summary>
        /// <param name="x">Sets the <see cref="X"/>.</param>
        /// <param name="y">Sets the <see cref="Y"/>.</param>
        public Coordinate(int x, int y)
        {
            X = x;
            Y = y;
        }
    }


}
