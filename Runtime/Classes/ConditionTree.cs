using System;
using System.Collections.Generic;

namespace _UTIL_
{
    public abstract class Condition_tree : OnValue_bool
    {
        public Condition_not As_NOT => this as Condition_not;
        public Condition_and As_AND => this as Condition_and;
        public Condition_or As_OR => this as Condition_or;
        protected abstract void PropagateValue(bool value);
    }

    public abstract class Condition_nodes : Condition_tree
    {
        protected readonly HashSet<OnValue_bool> nodes;

        //--------------------------------------------------------------------------------------------------------------

        protected Condition_nodes(params OnValue_bool[] nodes)
        {
            this.nodes = new(nodes);
            for (int i = 0; i < nodes.Length; i++)
                nodes[i].onChange += PropagateValue;
            PropagateValue(default);
        }

        //--------------------------------------------------------------------------------------------------------------

        public void AddNode(in OnValue_bool node)
        {
            if (!nodes.Contains(node))
            {
                nodes.Add(node);
                node.AddListener(PropagateValue);
            }
        }
    }

    [Serializable]
    public sealed class Condition_not : Condition_tree
    {
        public Condition_not(in OnValue_bool node) => node.AddListener(PropagateValue);
        protected override void PropagateValue(bool value) => Value = !value;
    }

    [Serializable]
    public sealed class Condition_or : Condition_nodes
    {
        public Condition_or(params OnValue_bool[] nodes) : base(nodes) { }
        protected override void PropagateValue(bool value)
        {
            value = false;
            foreach (OnValue_bool node in nodes)
                if (node._value)
                {
                    value = true;
                    break;
                }
            Value = value;
        }
    }

    [Serializable]
    public sealed class Condition_and : Condition_nodes
    {
        public Condition_and(params OnValue_bool[] nodes) : base(nodes) { }
        protected override void PropagateValue(bool value)
        {
            value = true;
            foreach (OnValue_bool node in nodes)
                if (!node._value)
                {
                    value = false;
                    break;
                }
            Value = value;
        }
    }
}