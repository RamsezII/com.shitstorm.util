#if UNITY_EDITOR
using UnityEngine;

namespace _UTIL_e
{
    sealed class HierarchyRenamer : MonoBehaviour
    {
        [SerializeField] Transform target;

        //----------------------------------------------------------------------------------------------------------

        [ContextMenu(nameof(Operate))]
        void Operate()
        {
            Rename(transform, target);
            static void Rename(in Transform self, in Transform target)
            {
                self.name = target.name;
                int min = Mathf.Min(self.childCount, target.childCount);
                for (int i = 0; i < min; ++i)
                    Rename(self.GetChild(i), target.GetChild(i));
            }
            DestroyImmediate(this);
        }
    }
}
#endif