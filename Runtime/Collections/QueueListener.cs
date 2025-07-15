using System.Collections.Generic;

namespace _UTIL_
{
    public sealed class QueueListener : QueueListener<object>
    {
    }

    public class QueueListener<T> : CollectionListener<Queue<T>>
    {
        protected override void OnRemoveZombies()
        {
        }

        //------------------------------------------------------------------------------------------------------------------------------

        public override void _Clear()
        {
            _collection.Clear();
        }
    }
}