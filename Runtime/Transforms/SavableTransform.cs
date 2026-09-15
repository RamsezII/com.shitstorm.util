using System;
using UnityEngine;

namespace _UTIL_
{
    [Serializable]
    public class SavableTransform
    {
        public string path;
        public Transform tfm;

        //----------------------------------------------------------------------------------------------------------

        public virtual void OnSave()
        {
            if (tfm != null)
                path = tfm.GetPath(false);
        }

        public virtual void OnRead(in Transform root)
        {
            if (string.IsNullOrWhiteSpace(path))
                tfm = null;
            else
                tfm = root.ForceFind(path);
        }
    }
}