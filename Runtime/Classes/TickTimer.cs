namespace _UTIL_
{
    public sealed partial class TickTimer
    {
        public float lastTick;

        //----------------------------------------------------------------------------------------------------------

        public TickTimer(in float startTime = 0)
        {
            lastTick = startTime;
        }

        //----------------------------------------------------------------------------------------------------------

        public bool Tick(in float time, in float delay, in bool compensate = false)
        {
            if (lastTick + delay <= time)
            {
                if (compensate)
                    lastTick += delay;
                else
                    lastTick = time;
                return true;
            }
            return false;
        }
    }
}