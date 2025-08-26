using System;
using System.Collections.Generic;

namespace _UTIL_
{
    public sealed class HeartBeat : IDisposable
    {
        public class Operation : Disposable
        {
            public readonly float time;
            public readonly bool play_once;
            public float timer;
            public readonly Action<float> action;

            //----------------------------------------------------------------------------------------------------------

            public Operation(in float time, in bool play_once, in Action<float> action)
            {
                this.time = time;
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

                if (op.timer >= op.time)
                {
                    op.timer %= op.time;
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