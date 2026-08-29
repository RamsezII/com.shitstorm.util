using System;
using System.IO;
using UnityEngine;

namespace _UTIL_
{
    [Serializable]
    public struct PosRot
    {
        public Vector3 pos;
        public Quaternion rot;

        //----------------------------------------------------------------------------------------------------------

        public PosRot(in Vector3 pos, in Quaternion rot)
        {
            this.pos = pos;
            this.rot = rot;
        }
    }

    [Serializable]
    public struct PosEul : IBytes
    {
        public Vector3 pos;
        public Vector3 eul;
        public readonly Quaternion Rot => Quaternion.Euler(eul);
        public readonly PosRot PosRot => new(pos, Rot);

        //----------------------------------------------------------------------------------------------------------

        public PosEul(in Transform T) : this(T.localPosition, T.localEulerAngles.SignedEuler_OLD()) { }
        public PosEul(in Vector3 pos, in Vector3 eul)
        {
            this.pos = pos;
            this.eul = eul;
        }

        //----------------------------------------------------------------------------------------------------------

        public void Slerp(in PosEul b, in float lerp)
        {
            pos = Vector3.Lerp(pos, b.pos, lerp);
            eul = Quaternion.Slerp(Quaternion.Euler(eul), Quaternion.Euler(b.eul), lerp).eulerAngles.SignedEuler_OLD();
        }

        public static PosEul Lerp(in PosEul a, in PosEul b, in float lerp) => new(
            Vector3.Lerp(a.pos, b.pos, lerp),
            Vector3.Lerp(a.eul, b.eul, lerp)
            );

        public readonly void WriteBytes(in BinaryWriter writer)
        {
            writer.WriteV3_3f32(pos);
            writer.WriteV3_3f16(eul);
        }

        public void ReadBytes(in BinaryReader reader)
        {
            pos = reader.ReadV3_3f32();
            eul = reader.ReadV3_3f16();
        }
    }
}