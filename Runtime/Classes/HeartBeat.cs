using System;
using System.Collections.Generic;
using UnityEngine;

namespace _UTIL_
{
    public sealed class HeartBeat : IDisposable
    {
        public class Operation : Disposable
        {
            public float timeStep;
            public readonly bool play_once;
            [SerializeField, Range(0, 1)] internal float timer;
            public readonly Action action;
            public readonly Action<float> action_f;

            //----------------------------------------------------------------------------------------------------------

            Operation(in float timeStep, in bool play_once)
            {
                this.timeStep = timeStep;
                this.play_once = play_once;
            }

            public Operation(in float timeStep, in bool play_once, in Action action) : this(timeStep, play_once)
            {
                this.action = action ?? throw new ArgumentNullException(nameof(action));
            }

            public Operation(in float timeStep, in bool play_once, in Action<float> action_f) : this(timeStep, play_once)
            {
                this.action_f = action_f ?? throw new ArgumentNullException(nameof(action_f));
            }
        }

        public readonly List<Operation> operations = new();

        //----------------------------------------------------------------------------------------------------------

        public void Tick(in float deltaTime)
        {
            for (int i = 0; i < operations.Count; i++)
                if (operations[i]._disposed)
                    operations.RemoveAt(i--);
                else
                {
                    Operation op = operations[i];
                    op.timer += deltaTime;

                    if (op.timer >= op.timeStep)
                    {
                        if (op.timeStep > 0)
                            op.timer %= op.timeStep;
                        else
                            op.timer = deltaTime;

                        op.action?.Invoke();
                        op.action_f?.Invoke(op.timer);

                        if (op.play_once)
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