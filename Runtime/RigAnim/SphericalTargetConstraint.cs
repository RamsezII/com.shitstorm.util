#if HAS_ANIMRIG
using System;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

namespace _UTIL_
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Animation Rigging/Spherical Target Constraint")]
    public class SphericalTargetConstraint : RigConstraint<SphericalTargetConstraint.Job, SphericalTargetConstraint.Data, SphericalTargetConstraint.Binder>
    {
        [BurstCompile]
        public struct Job : IWeightedAnimationJob
        {
            public ReadOnlyTransformHandle read_root, read_mid, read_tip, read_target;
            public ReadWriteTransformHandle write_hint, write_target;
            public FloatProperty jobWeight { get; set; }

            //----------------------------------------------------------------------------------------------------------

            public readonly void ProcessRootMotion(AnimationStream stream) { }

            public void ProcessAnimation(AnimationStream stream)
            {
                float weight = jobWeight.Get(stream);

                if (weight <= 0f)
                {
                    AnimationRuntimeUtils.PassThrough(stream, write_hint);
                    AnimationRuntimeUtils.PassThrough(stream, write_target);
                    return;
                }

                Vector3 apos = read_root.GetPosition(stream);
                Vector3 hpos = read_mid.GetPosition(stream);
                Vector3 cpos = read_tip.GetPosition(stream);
                Vector3 tpos = read_target.GetPosition(stream);
                Vector3 cdelta = cpos - apos;
                Vector3 hdelta = hpos - apos;
                Vector3 tdelta = tpos - apos;
                Quaternion deltaRot = Quaternion.FromToRotation(cdelta, tdelta);

                write_hint.SetPosition(stream, apos + Vector3.Slerp(hdelta, deltaRot * hdelta, weight));

                write_target.SetPosition(stream, apos + Vector3.Slerp(cdelta, tdelta, weight));

                Quaternion crot = deltaRot * read_tip.GetRotation(stream);
                Quaternion trot = read_target.GetRotation(stream);

                write_target.SetRotation(stream, Quaternion.Slerp(crot, trot, weight));
            }
        }

        [Serializable]
        public struct Data : IAnimationJobData
        {
            public TwoBoneIKConstraint ik;
            [SyncSceneToStream] public Transform target;

            //----------------------------------------------------------------------------------------------------------

            public readonly bool IsValid() =>
                ik != null
                && target != null;

            public void SetDefaultValues()
            {
                ik = null;
                target = null;
            }
        }

        public class Binder : AnimationJobBinder<Job, Data>
        {
            public override Job Create(Animator animator, ref Data data, Component component) => new()
            {
                read_root = ReadOnlyTransformHandle.Bind(animator, data.ik.data.root),
                read_mid = ReadOnlyTransformHandle.Bind(animator, data.ik.data.mid),
                read_tip = ReadOnlyTransformHandle.Bind(animator, data.ik.data.tip),
                read_target = ReadOnlyTransformHandle.Bind(animator, data.target),
                write_hint = ReadWriteTransformHandle.Bind(animator, data.ik.data.hint),
                write_target = ReadWriteTransformHandle.Bind(animator, data.ik.data.target),
            };

            //----------------------------------------------------------------------------------------------------------

            public override void Destroy(Job job)
            {
            }
        }
    }
}
#endif