using System;
using System.IO;
using UnityEngine;

namespace _UTIL_
{
    [Serializable]
    public class AnimTransition<T> where T : Enum
    {
        public readonly int layerIndex;
        public T current, target;
        public bool nfade;
        public float fade, offset;

        //----------------------------------------------------------------------------------------------------------

        public AnimTransition(in int layerIndex) : base()
        {
            this.layerIndex = layerIndex;
        }

        //----------------------------------------------------------------------------------------------------------

        public void Apply(in Animator animator, in bool force)
        {
            if (force || !target.Equals(current))
            {
                int state = Convert.ToInt32(current);
                if (nfade)
                    animator.CrossFade(state, fade, layerIndex, offset);
                else
                    animator.CrossFadeInFixedTime(state, fade, layerIndex, offset);
            }
        }

        public void OnWriteBytes(in BinaryWriter writer)
        {
            writer.Write(Convert.ToInt32(current));
            writer.Write(nfade);
            writer.Write_f16(fade);
            writer.Write_f16(offset);
        }

        public void OnReadBytes(in BinaryReader reader)
        {
            current = (T)Enum.ToObject(typeof(T), reader.ReadInt32());
            nfade = reader.ReadBoolean();
            fade = reader.Read_f16();
            offset = reader.Read_f16();
        }
    }
}