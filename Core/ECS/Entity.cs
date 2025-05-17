namespace Simple3DGame.Core.ECS
{
    // Represents an entity in the world, identified by a unique ID.
    public readonly struct Entity // Made public
    {
        public int Id { get; } // Added public getter

        // Made constructor public
        public Entity(int id)
        {
            Id = id;
        }

        // Optional: Override Equals and GetHashCode for dictionary keys, etc.
        public override bool Equals(object? obj) => obj is Entity other && Id == other.Id;
        public override int GetHashCode() => Id;
        public static bool operator ==(Entity left, Entity right) => left.Equals(right);
        public static bool operator !=(Entity left, Entity right) => !(left == right);
    }
}
