using System;
using System.Collections.Generic;

namespace _UTIL_
{
    public abstract class Condition_tree : OnValue_bool
    {
        public Condition_not As_NOT => this as Condition_not;
        public Condition_and As_AND => this as Condition_and;
        public Condition_or As_OR => this as Condition_or;

        //--------------------------------------------------------------------------------------------------------------

        protected abstract void PropagateValue(bool value);
    }

    [Serializable]
    public sealed class Condition_not : Condition_tree
    {
        public OnValue_bool node;

        //--------------------------------------------------------------------------------------------------------------

        public Condition_not(in OnValue_bool node)
        {
            AssignNode(node);
        }

        //--------------------------------------------------------------------------------------------------------------

        public void AssignNode(in OnValue_bool node)
        {
            this.node?.RemoveListener(PropagateValue);
            this.node = node;
            this.node?.AddListener(PropagateValue);
        }

        public void UnassignNode()
        {
            node?.RemoveListener(PropagateValue);
            node = null;
        }

        protected override void PropagateValue(bool value) => Value = !value;
    }

    public abstract class Condition_nodes : Condition_tree
    {
        protected readonly HashSet<OnValue_bool> nodes;

        //--------------------------------------------------------------------------------------------------------------

        protected Condition_nodes(params OnValue_bool[] nodes)
        {
            this.nodes = new(nodes);
            for (int i = 0; i < nodes.Length; i++)
                nodes[i].AddListener(PropagateValue, stopCallback: true);
            PropagateValue(default);
        }

        //--------------------------------------------------------------------------------------------------------------

        public void AddNode(in OnValue_bool node)
        {
            if (nodes.Add(node))
                node.AddListener(PropagateValue);
        }

        public void RemoveNode(in OnValue_bool node)
        {
            if (nodes.Remove(node))
                node.RemoveListener(PropagateValue);
        }
    }

    [Serializable]
    public sealed class Condition_or : Condition_nodes
    {
        public Condition_or(params OnValue_bool[] nodes) : base(nodes) { }

        //--------------------------------------------------------------------------------------------------------------

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

        //--------------------------------------------------------------------------------------------------------------

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