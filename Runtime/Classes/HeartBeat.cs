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
            public readonly Action<float> action;

            //----------------------------------------------------------------------------------------------------------

            public Operation(in float timeStep, in bool play_once, in Action<float> action)
            {
                this.timeStep = timeStep;
                this.play_once = play_once;
                this.action = action;
            }
        }

        public readonly List<Operation> operations = new();

        //----------------------------------------------------------------------------------------------------------

        public void Tick(in float deltaTime)
        {
            for (int i = 0; i < operations.Count; i++)
            {
                Operation op = operations[i];
                op.timer += deltaTime;

                if (op.timer >= op.timeStep)
                {
                    op.timer %= op.timeStep;
                    op.action(op.timer);

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