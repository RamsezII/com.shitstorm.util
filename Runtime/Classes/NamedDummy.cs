namespace _UTIL_
{
    public sealed class NamedDummy
    {
        public readonly string name;
        public NamedDummy(in string name) => this.name = name;
        public override string ToString() => $"dummy[\"{name}\"]";
    }
}