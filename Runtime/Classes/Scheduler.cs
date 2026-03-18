using System;
using System.Collections.Generic;
using UnityEngine;

namespace _UTIL_
{
    public sealed class Scheduler : IDisposable
    {
        public class Operation : Disposable
        {
            public float delay;
            public readonly bool loop;
            [Range(0, 1)] public float timer;
            public readonly Action action;
            public readonly Action<Operation> action_op;

            //----------------------------------------------------------------------------------------------------------

            Operation(in string name, in float delay, in bool loop) : base(name)
            {
                this.delay = delay;
                this.loop = loop;
            }

            public Operation(in string name, in float delay, in bool loop, in Action action) : this(name, delay, loop)
            {
                this.action = action ?? throw new ArgumentNullException(nameof(action));
            }

            public Operation(in string name, in float delay, in bool loop, in Action<Operation> action_op) : this(name, delay, loop)
            {
                this.action_op = action_op ?? throw new ArgumentNullException(nameof(action_op));
            }
        }

        readonly List<Operation> operations = new();

        //----------------------------------------------------------------------------------------------------------

        public Operation AddOperation(in Operation operation)
        {
            operations.Remove(operation);
            if (operation == null)
                Debug.LogWarning($"trying to add empty operation ({GetType()}");
            else
                operations.Add(operation);
            return operation;
        }

        public void RemoveOperation(in Operation operation)
        {
            operations.Remove(operation);
        }

        public void Tick(in float deltaTime)
        {
            for (int i = 0; i < operations.Count; i++)
                if (operations[i]._disposed)
                    operations.RemoveAt(i--);
                else
                {
                    Operation op = operations[i];
                    op.timer += deltaTime;

                    if (op.timer >= op.delay)
                    {
                        if (op.delay > 0)
                            op.timer %= op.delay;
                        else
                            op.timer = deltaTime;

                        op.action?.Invoke();
                        op.action_op?.Invoke(op);

                        if (!op.loop)
                        {
                            op.Dispose();
                            operations.RemoveAt(i--);
                        }
                    }
                }
        }

        //----------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            for (int i = 0; i < operations.Count; i++)
                operations[i].Dispose();
            operations.Clear();
        }
    }
}