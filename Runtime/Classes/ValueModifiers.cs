using System;
using System.Collections.Generic;

namespace _UTIL_
{
    public abstract class NotifierNode<InputType, OutputType> : ValueNotifier<OutputType>
    {
        protected NotifierNode() { }
        protected abstract void PropagateValue();
    }

    public abstract class NotifierNode_group<InputType, OutputType> : NotifierNode<InputType, OutputType>
    {
        protected readonly HashSet<ValueNotifier<InputType>> _nodes = new();

        //--------------------------------------------------------------------------------------------------------------

        protected NotifierNode_group(params ValueNotifier<InputType>[] nodes)
        {
            for (int i = 0; i < nodes.Length; i++)
                AddNode(nodes[i], doNotCallThisTime: i < nodes.Length - 1);
        }

        //--------------------------------------------------------------------------------------------------------------

        public void AddNode(in ValueNotifier<InputType> node, bool doNotCallThisTime = false)
        {
            if (_nodes.Add(node))
                node.AddListener(PropagateValue, doNotCallThisTime: doNotCallThisTime);
        }

        public void RemoveNode(in ValueNotifier<InputType> node)
        {
            if (_nodes.Remove(node))
                node.RemoveListener(PropagateValue);
        }

        //--------------------------------------------------------------------------------------------------------------

        protected override void OnDispose()
        {
            base.OnDispose();
            foreach (var node in _nodes)
                RemoveNode(node);
            _nodes.Clear();
        }
    }

    public sealed class NotifierNode_group_custom<InputType, OutputType> : NotifierNode_group<InputType, OutputType>
    {
        readonly Func<NotifierNode_group_custom<InputType, OutputType>, OutputType> _onConvert;

        //--------------------------------------------------------------------------------------------------------------

        public NotifierNode_group_custom(in Func<NotifierNode_group_custom<InputType, OutputType>, OutputType> onConvert, params ValueNotifier<InputType>[] nodes) : base(nodes)
        {
            _onConvert = onConvert;
        }

        //--------------------------------------------------------------------------------------------------------------

        protected override void PropagateValue()
        {
            Value = _onConvert(this);
        }
    }

    [Serializable]
    public sealed class BoolModifier_not : NotifierNode<bool, bool>
    {
        readonly ValueNotifier<bool> _node;

        //--------------------------------------------------------------------------------------------------------------

        public BoolModifier_not(in ValueNotifier<bool> node)
        {
            _node = node;
            _node.AddListener(PropagateValue, doNotCallThisTime: true);
        }

        //--------------------------------------------------------------------------------------------------------------

        protected override void PropagateValue() => Value = !_node._value;

        //--------------------------------------------------------------------------------------------------------------

        protected override void OnDispose()
        {
            base.OnDispose();
            _node.RemoveListener(PropagateValue);
        }
    }

    [Serializable]
    public sealed class BoolModifier_or : NotifierNode_group<bool, bool>
    {
        public BoolModifier_or(params ValueNotifier<bool>[] nodes) : base(nodes) { }
        protected override void PropagateValue()
        {
            bool value = false;
            foreach (ValueNotifier<bool> node in _nodes)
                if (node._value)
                {
                    value = true;
                    break;
                }
            Value = value;
        }
    }

    [Serializable]
    public sealed class BoolModifier_and : NotifierNode_group<bool, bool>
    {
        public BoolModifier_and(params ValueNotifier<bool>[] nodes) : base(nodes) { }
        protected override void PropagateValue()
        {
            bool value = true;
            foreach (ValueNotifier<bool> node in _nodes)
                if (!node._value)
                {
                    value = false;
                    break;
                }
            Value = value;
        }
    }
}