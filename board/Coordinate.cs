public struct Coordinate : IEquatable<Coordinate>
{
    public int X { get; }
    public int Y { get; }

    public Coordinate(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override bool Equals(object obj) => obj is Coordinate other && Equals(other);

    public bool Equals(Coordinate other) => X == other.X && Y == other.Y;

    public override int GetHashCode() => HashCode.Combine(X, Y);

    public static bool operator ==(Coordinate left, Coordinate right) => left.Equals(right);
    public static bool operator !=(Coordinate left, Coordinate right) => !left.Equals(right);

    public override string ToString() => $"({X}, {Y})";
}