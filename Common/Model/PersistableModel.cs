namespace InertGas.Common.Model
{
    public abstract class PersistableModel : IEquatable<PersistableModel>
    {
        public abstract string Id { get; }

        public override int GetHashCode()
        {
            unchecked
            {
                return Id?.GetHashCode() ?? 0;
            }
        }

        public bool Equals(PersistableModel other) =>
            string.Equals(Id, other?.Id);

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((PersistableModel)obj);
        }
    }
}
