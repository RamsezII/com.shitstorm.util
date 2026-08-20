using System;
using System.Collections.Generic;
using System.Linq;

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
            foreach (var node in _nodes.ToArray())
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

    public sealed class ValueNotifier_group_custom<U> : ValueNotifier<U>
    {
        readonly Func<U> _onConvert;

        //--------------------------------------------------------------------------------------------------------------

        public ValueNotifier_group_custom(in Func<U> onConvert)
        {
            _onConvert = onConvert;
        }

        //--------------------------------------------------------------------------------------------------------------

        public void PropagateValue()
        {
            Value = _onConvert();
        }
    }

    public sealed class ValueNotifier_group_custom<InputType, OutputType> : ValueNotifier<OutputType>
    {
        readonly Func<OutputType> _onConvert;

        //--------------------------------------------------------------------------------------------------------------

        public ValueNotifier_group_custom(in Func<OutputType> onConvert, params Action<Action<InputType>>[] listeners)
        {
            _onConvert = onConvert;
            foreach (var listener in listeners)
                listener(PropagateValue);
        }

        //--------------------------------------------------------------------------------------------------------------

        public void PropagateValue(InputType _)
        {
            Value = _onConvert();
        }
    }

    [Serializable]
    public sealed class BoolNotifier_not : NotifierNode<bool, bool>
    {
        readonly ValueNotifier<bool> _node;

        //--------------------------------------------------------------------------------------------------------------

        public BoolNotifier_not(in ValueNotifier<bool> node)
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
    public sealed class ValueNotifier_bool<InputType> : ValueNotifier<bool>
    {
        ValueNotifier<InputType> _input;
        readonly Func<bool> _onConvert;
        public ValueNotifier<InputType> Input => _input;

        //--------------------------------------------------------------------------------------------------------------

        public ValueNotifier_bool(in ValueNotifier<InputType> input, Func<bool> onConvert)
        {
            _input = input;
            _onConvert = onConvert;
        }

        //--------------------------------------------------------------------------------------------------------------

        public void SetInput(in ValueNotifier<InputType> input, in bool doNotCallThisTime = false)
        {
            _input?.RemoveListener(PropagateValue);
            _input = input;
            _input.AddListener(PropagateValue, doNotCallThisTime: doNotCallThisTime);
        }

        //--------------------------------------------------------------------------------------------------------------

        void PropagateValue(InputType value)
        {
            Value = _onConvert();
        }

        //--------------------------------------------------------------------------------------------------------------

        protected override void OnDispose()
        {
            base.OnDispose();
            SetInput(null);
        }
    }

    [Serializable]
    public sealed class BoolNotifier_or : NotifierNode_group<bool, bool>
    {
        public BoolNotifier_or(params ValueNotifier<bool>[] nodes) : base(nodes) { }
        protected override void PropagateValue()
        {
            bool value = false;
            foreach (var node in _nodes)
                if (node._value)
                {
                    value = true;
                    break;
                }
            Value = value;
        }
    }

    [Serializable]
    public sealed class BoolNotifier_and : NotifierNode_group<bool, bool>
    {
        public BoolNotifier_and(params ValueNotifier<bool>[] nodes) : base(nodes) { }
        protected override void PropagateValue()
        {
            bool value = true;
            foreach (var node in _nodes)
                if (!node._value)
                {
                    value = false;
                    break;
                }
            Value = value;
        }
    }
}