using System;
using System.Collections.Generic;
using UnityEngine;

namespace _UTIL_
{
    public sealed class HeartBeat : IDisposable
    {
        public class Operation : Disposable
        {
            public float delay;
            public readonly bool loop;
            [Range(0, 1)] public float timer;
            public readonly Action action;
            public readonly Action<Operation> action_f;

            //----------------------------------------------------------------------------------------------------------

            Operation(in float delay, in bool loop)
            {
                this.delay = delay;
                this.loop = loop;
            }

            public Operation(in float delay, in bool loop, in Action action) : this(delay, loop)
            {
                this.action = action ?? throw new ArgumentNullException(nameof(action));
            }

            public Operation(in float delay, in bool loop, in Action<Operation> action_f) : this(delay, loop)
            {
                this.action_f = action_f ?? throw new ArgumentNullException(nameof(action_f));
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
                        op.action_f?.Invoke(op);

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