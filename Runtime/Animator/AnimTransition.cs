using System;
using System.IO;
using UnityEngine;

namespace _UTIL_
{
    [Serializable]
    public sealed class AnimTransition<T> : AnimTransition where T : Enum
    {
        public T Target
        {
            set => target = Convert.ToInt32(value);
#if UNITY_EDITOR
            get => target.ToEnum<T>();
#endif
        }

        public T Current => (T)Enum.ToObject(typeof(T), current);

#if UNITY_EDITOR
        [SerializeField] T _current, _target;
#endif

        //----------------------------------------------------------------------------------------------------------

        public AnimTransition(in Animator animator, in int layerIndex) : base(animator, layerIndex)
        {
        }

        //----------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR
        public override void OnState(in int value)
        {
            base.OnState(value);
            _current = Current;
        }
#endif

        public void Apply(in T value, in float fade = 0, in float offset = 0, in bool nfade = false, in bool force = true)
        {
            Target = value;
            this.force = force;
            this.fade = fade;
            this.offset = offset;
            this.nfade = nfade;
            Apply();
        }
    }

    [Serializable]
    public class AnimTransition
    {
        public readonly Animator animator;
        public readonly int layerIndex;
        public int current, target;
        public bool nfade, force;
        public float fade, offset, last_scaled, last_unscaled;
        public int last_apply_frame;
        public bool TargetChanged => !current.Equals(target);
        public bool NoChange => current.Equals(target);

        //----------------------------------------------------------------------------------------------------------

        public AnimTransition(in Animator animator, in int layerIndex) : base()
        {
            this.animator = animator;
            this.layerIndex = layerIndex;
        }

        //----------------------------------------------------------------------------------------------------------

        public virtual void OnState(in int value)
        {
            last_scaled = Time.time;
            last_unscaled = Time.unscaledTime;
            current = value;
        }

        public void BeforeEval()
        {
            target = current;
            nfade = false;
            force = false;
            fade = .2f;
            offset = 0;
        }

        public void Apply()
        {
            if (force || TargetChanged && last_apply_frame != Time.frameCount)
            {
                if (animator.IsInTransition(layerIndex))
                    if (force)
                        fade = 0;
                    else
                        return;

                last_apply_frame = Time.frameCount;
                int state = Convert.ToInt32(target);

                if (nfade)
                    animator.CrossFade(state, fade, layerIndex, offset);
                else
                    animator.CrossFadeInFixedTime(state, fade, layerIndex, offset);
            }
        }

        public void OnWriteBytes(BinaryWriter writer)
        {
            writer.Write((byte)layerIndex);
            writer.Write(current);
            writer.Write(nfade);
            writer.Write_f16(fade);
            writer.Write_f16(offset);
        }

        public void OnReadBytes(in BinaryReader reader, in Animator animator)
        {
            target = current = reader.ReadInt32();
            nfade = reader.ReadBoolean();
            fade = reader.Read_f16();
            offset = reader.Read_f16();
            force = true;
            Apply();
        }
    }
}