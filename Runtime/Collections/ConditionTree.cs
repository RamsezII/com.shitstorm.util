namespace _UTIL_
{
    public abstract class ConditionTree : OnValue_bool
    {
        protected abstract void Propagate(bool value);
    }

    public abstract class Condition_binary : ConditionTree
    {
        public readonly OnValue_bool a, b;

        //--------------------------------------------------------------------------------------------------------------

        protected Condition_binary(in OnValue_bool a, in OnValue_bool b)
        {
            this.a = a;
            this.b = b;
            a.AddListener(Propagate);
            b.AddListener(Propagate);
        }
    }

    public sealed class Condition_or : Condition_binary
    {
        public Condition_or(in OnValue_bool a, in OnValue_bool b) : base(a, b) { }
        protected override void Propagate(bool value) => Value = a._value || b._value;
    }

    public sealed class Condition_and : Condition_binary
    {
        public Condition_and(in OnValue_bool a, in OnValue_bool b) : base(a, b) { }
        protected override void Propagate(bool value) => Value = a._value && b._value;
    }
}