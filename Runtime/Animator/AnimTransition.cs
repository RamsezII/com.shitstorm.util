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
            get => target.ToEnum<T>();
        }

        public T Current => (T)Enum.ToObject(typeof(T), current);
        public T Old => (T)Enum.ToObject(typeof(T), old);

#if UNITY_EDITOR
        [SerializeField] T _old, _current, _target;
#endif

        public Action<T> onState;

        //----------------------------------------------------------------------------------------------------------

        public AnimTransition(in Animator animator, in int layerIndex) : base(animator, layerIndex)
        {
        }

        //----------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR
        public override void OnState(in int value)
        {
            base.OnState(value);
            _target = Target;
            _current = Current;
            _old = Old;
            onState?.Invoke(Current);
        }
#endif

        public void Apply(in T value) => Apply(value, Options.Default);
        public void Apply(in T value, in Options options)
        {
            Target = value;
            Apply(options);
        }
    }

    [Serializable]
    public class AnimTransition
    {
        [Serializable]
        public struct Options
        {
            public bool nfade, force;
            public float fade, offset;
            public static readonly Options Default = new()
            {
                fade = .2f,
            };
        }

        public readonly Animator animator;
        public readonly int layerIndex;
        public int old, current, target;
        [SerializeField] Options last_options;
        public float last_scaled, last_unscaled;
        public int last_apply_frame;
        public bool TargetChanged => !current.Equals(target);
        public bool NoChange => current.Equals(target);
        public Options GetDefaultOptions => Options.Default;

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
            old = current;
            current = value;
        }

        public void BeforeEval()
        {
            target = current;
            last_options = Options.Default;
        }

        public void Apply(Options options, in bool no_fade_when_forced = true)
        {
            last_options = options;
            if (options.force || TargetChanged && last_apply_frame != Time.frameCount)
            {
                if (animator.IsInTransition(layerIndex))
                    if (!options.force)
                        return;
                    else if (no_fade_when_forced)
                        options.fade = 0;

                last_apply_frame = Time.frameCount;

                if (options.nfade)
                    animator.CrossFade(target, options.fade, layerIndex, options.offset);
                else
                    animator.CrossFadeInFixedTime(target, options.fade, layerIndex, options.offset);
            }
        }

        public void OnWriteBytes(BinaryWriter writer)
        {
            writer.Write((byte)layerIndex);
            writer.Write(current);
            writer.Write(last_options.nfade);
            writer.Write_f16(last_options.fade);
            writer.Write_f16(last_options.offset);
        }

        public void OnReadBytes(in BinaryReader reader)
        {
            target = current = reader.ReadInt32();
            Options options = new()
            {
                nfade = reader.ReadBoolean(),
                fade = reader.Read_f16(),
                offset = reader.Read_f16(),
                force = true,
            };
            Apply(options);
        }
    }
}