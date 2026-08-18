using System;
using UnityEngine;

namespace _UTIL_
{
    [Serializable]
    public class IKSolver
    {
        public readonly Transform a, b, c;
        readonly Quaternion ik2a, ik2b;

        //----------------------------------------------------------------------------------------------------------

        public IKSolver(in Transform a, in Transform b, in Transform c, in Quaternion initRot)
        {
            this.a = a;
            this.b = b;
            this.c = c;

            ik2a = Quaternion.Inverse(Quaternion.LookRotation(b.position - a.position, initRot * Vector3.up)) * a.rotation;
            ik2b = Quaternion.Inverse(Quaternion.LookRotation(c.position - b.position, initRot * Vector3.up)) * b.rotation;
        }

        //----------------------------------------------------------------------------------------------------------

        public void Solve_scaled(in Vector3 hint, in Vector3 target, in float weight)
        {
            Vector3 pos_a = a.position;

            Vector3
                bpos = a.InverseTransformPoint(b.position),
                cpos = a.InverseTransformPoint(c.position),
                tpos = a.InverseTransformPoint(target);

            float ab = bpos.magnitude;
            float bc = Vector3.Distance(bpos, cpos);
            float at = tpos.magnitude;

            Solve(pos_a, ab, bc, at, hint, target, weight);
        }

        public void Solve_unscaled(in Vector3 hint, in Vector3 target, in float weight)
        {
            Vector3
                pos_a = a.position,
                pos_b = b.position,
                pos_c = c.position;

            float
                ab = Vector3.Distance(pos_a, pos_b),
                bc = Vector3.Distance(pos_b, pos_c),
                at = Vector3.Distance(pos_a, target);

            Solve(pos_a, ab, bc, at, hint, target, weight);
        }

        void Solve(in Vector3 pos_a, in float ab, in float bc, float at, in Vector3 hint, in Vector3 target, in float weight)
        {
            at = Mathf.Clamp(at, 1.01f * (ab - bc), .99f * (ab + bc));

            if (!Util.SolveIK(at, ab, bc, out float angleA, out _, out float angleC))
            {
                Debug.LogWarning($"{GetType().FullName} (cant IK) {{ {nameof(angleA)}: {angleA}, {{ {nameof(angleC)}: {angleC} }}");
                return;
            }

            Quaternion
                rot = Quaternion.LookRotation(target - pos_a, hint - pos_a),
                rota = rot * Quaternion.Euler(-angleA, 0, 0) * ik2a,
                rotb = Quaternion.Inverse(rota) * rot * Quaternion.Euler(angleC, 0, 0) * ik2b;

            Quaternion arot = a.rotation = Quaternion.Slerp(a.rotation, rota, weight);
            b.localRotation = Quaternion.Slerp(b.localRotation, Quaternion.Inverse(b.parent.rotation) * arot * rotb, weight);
        }

        //----------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR
        public void DrawGizmos(in Color color)
        {
            if (a == null || b == null || c == null)
                return;
            Gizmos.color = color;
            Gizmos.DrawLine(a.position, b.position);
            Gizmos.DrawLine(b.position, c.position);
        }
#endif
    }
}