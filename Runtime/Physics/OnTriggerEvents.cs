using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _UTIL_
{
    public class OnTriggerEvents : MonoBehaviour
    {
        public enum Types
        {
            Enter,
            Stay,
            Exit,
        }

        [Header("~@ Editor @~")]
        public Collider[] triggers;

        public Action<Collider, Types> onTriggerEvent;
        public Action<Collider2D, Types> onTriggerEvent2D;

#if UNITY_EDITOR
        [SerializeField] List<Collider> _colliders;
        [SerializeField] List<Collider2D> _colliders2D;
#endif

        //--------------------------------------------------------------------------------------------------------------

        private void Awake()
        {
            triggers = GetComponentsInChildren<Collider>().Where(cld => cld.isTrigger).ToArray();
#if UNITY_EDITOR
            _colliders = new();
            _colliders2D = new();
#endif
        }

        //--------------------------------------------------------------------------------------------------------------

        private void OnTriggerEnter(Collider other)
        {
#if UNITY_EDITOR
            _colliders.Add(other);
#endif
            onTriggerEvent?.Invoke(other, Types.Enter);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
#if UNITY_EDITOR
            _colliders2D.Add(other);
#endif
            onTriggerEvent2D?.Invoke(other, Types.Enter);
        }

        //--------------------------------------------------------------------------------------------------------------

        private void OnTriggerExit(Collider other)
        {
#if UNITY_EDITOR
            _colliders.Remove(other);
#endif
            onTriggerEvent?.Invoke(other, Types.Exit);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
#if UNITY_EDITOR
            _colliders2D.Remove(other);
#endif
            onTriggerEvent2D?.Invoke(other, Types.Exit);
        }
    }
}