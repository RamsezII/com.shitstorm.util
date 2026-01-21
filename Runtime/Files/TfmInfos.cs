using System;
using System.IO;
using UnityEngine;

namespace _UTIL_
{
    [Serializable]
    public struct TfmInfos : IBytes
    {
        public string path;
        public Vector3 position, eulers, scale;

        //----------------------------------------------------------------------------------------------------------

        public TfmInfos(in Transform transform) : this(transform, transform.root)
        {
        }

        public TfmInfos(in Transform transform, in Transform root) : this(transform.GetRelativePath(root), transform.localPosition, transform.localEulerAngles, transform.localScale)
        {
        }

        public TfmInfos(string path, Vector3 position, Vector3 eulers, Vector3 scale)
        {
            this.path = path;
            this.position = position;
            this.eulers = eulers;
            this.scale = scale;
        }

        //----------------------------------------------------------------------------------------------------------

        public readonly Transform TryForceFindTransform(in Transform root, in bool logError = false)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                if (logError)
                    Debug.LogError($"{GetType().FullName}.{nameof(TryForceFindTransform)}() {nameof(path)} is null or empty.");
                return null;
            }

            Transform transform = root == null ? path.ForceFindTransform() : root.ForceFind(path, false);
            transform.SetLocalPositionAndRotation(position, Quaternion.Euler(eulers));
            transform.localScale = scale;

            return transform;
        }

        public readonly void WriteBytes(in BinaryWriter writer)
        {
            writer.Write(path);
            writer.WriteV3_3f32(position);
            writer.WriteV3_3f16(eulers);
            writer.WriteV3_3f16(scale);
        }

        public void ReadBytes(in BinaryReader reader)
        {
            path = reader.ReadString();
            position = reader.ReadV3_3f32();
            eulers = reader.ReadV3_3f16();
            scale = reader.ReadV3_3f16();
        }
    }
}