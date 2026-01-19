using System.Collections.Generic;
using UnityEngine;

namespace _EDITOR_
{
    public class TPoser : MonoBehaviour
    {
#if UNITY_EDITOR
        public Transform root;

        Dictionary<Transform, Quaternion> rots;
        Dictionary<Transform, Vector3> pos;

        [ContextMenu(nameof(Memorize))]
        void Memorize()
        {
            rots = new Dictionary<Transform, Quaternion>();
            pos = new Dictionary<Transform, Vector3>();

            if (!root)
                root = transform;

            foreach (Transform bone in root.GetComponentsInChildren<Transform>())
            {
                rots[bone] = bone.rotation;
                pos[bone] = bone.position;
            }
        }

        [ContextMenu(nameof(Apply))]
        void Apply()
        {
            foreach (KeyValuePair<Transform, Quaternion> bone in rots)
                bone.Key.rotation = bone.Value;

            foreach (KeyValuePair<Transform, Vector3> bone in pos)
                bone.Key.position = bone.Value;
        }
#endif

        private void Awake() => Destroy(this);
    }
}