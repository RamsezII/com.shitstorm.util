using System;
using System.Collections.Generic;

namespace _UTIL_
{
    public abstract class ValueModifier<T> : ValueHandler<T>
    {
        protected abstract void PropagateValue(T value);
    }

    public abstract class ValueModifier_unary<T> : ValueModifier<T>
    {
        public ValueHandler<T> _node;

        //--------------------------------------------------------------------------------------------------------------

        public ValueModifier_unary(in ValueHandler<T> node) => AssignNode(node);

        //--------------------------------------------------------------------------------------------------------------

        public void AssignNode(in ValueHandler<T> node)
        {
            _node?.RemoveListener(PropagateValue);
            _node = node;
            _node?.AddListener(PropagateValue);
        }

        public void UnassignNode()
        {
            _node?.RemoveListener(PropagateValue);
            _node = null;
        }
    }

    public class ValueModifier_unary_custom<T> : ValueModifier_unary<T>
    {
        public readonly Func<T, T> onPropagate;
        public ValueModifier_unary_custom(in ValueHandler<T> node, in Func<T, T> onPropagate) : base(node) => this.onPropagate = onPropagate;
        protected sealed override void PropagateValue(T value) => Value = onPropagate(value);
    }

    public abstract class ValueModifier_group<T> : ValueModifier<T>
    {
        public readonly HashSet<ValueHandler<T>> _nodes;

        //--------------------------------------------------------------------------------------------------------------

        protected ValueModifier_group(params ValueHandler<T>[] nodes)
        {
            _nodes = new(nodes);
            for (int i = 0; i < nodes.Length; i++)
                nodes[i].AddListener(PropagateValue, doNotCallThisTime: true);
            PropagateValue(default);
        }

        //--------------------------------------------------------------------------------------------------------------

        public void AddNode(in ValueHandler<T> node)
        {
            if (_nodes.Add(node))
                node.AddListener(PropagateValue);
        }

        public void RemoveNode(in ValueHandler<T> node)
        {
            if (_nodes.Remove(node))
                node.RemoveListener(PropagateValue);
        }
    }

    public class ValueModifier_group_custom<T> : ValueModifier_group<T>
    {
        public readonly Func<ValueModifier_group_custom<T>, T> onPropagate;
        public ValueModifier_group_custom(in Func<ValueModifier_group_custom<T>, T> onPropagate, params ValueHandler<T>[] nodes) : base(nodes) => this.onPropagate = onPropagate;
        protected sealed override void PropagateValue(T value) => Value = onPropagate(this);
    }

    [Serializable]
    public sealed class BoolModifier_not : ValueModifier_unary<bool>
    {
        public BoolModifier_not(in ValueHandler<bool> node) : base(node) { }
        protected override void PropagateValue(bool value) => Value = !value;
    }

    [Serializable]
    public sealed class BoolModifier_or : ValueModifier_group<bool>
    {
        public BoolModifier_or(params ValueHandler<bool>[] nodes) : base(nodes) { }
        protected override void PropagateValue(bool value)
        {
            value = false;
            foreach (ValueHandler<bool> node in _nodes)
                if (node._value)
                {
                    value = true;
                    break;
                }
            Value = value;
        }
    }

    [Serializable]
    public sealed class BoolModifier_and : ValueModifier_group<bool>
    {
        public BoolModifier_and(params ValueHandler<bool>[] nodes) : base(nodes) { }
        protected override void PropagateValue(bool value)
        {
            value = true;
            foreach (ValueHandler<bool> node in _nodes)
                if (!node._value)
                {
                    value = false;
                    break;
                }
            Value = value;
        }
    }
}