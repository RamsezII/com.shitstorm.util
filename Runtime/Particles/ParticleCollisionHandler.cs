using System;
using System.Collections.Generic;
using UnityEngine;

namespace _UTIL_
{
    public class ParticleCollisionHandler : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] int _count;
#endif
        public ParticleSystem particleSystem;
        public Action<GameObject, ParticleCollisionEvent> onParticleCollision;
        readonly List<ParticleCollisionEvent> collisionEvents = new();

        //--------------------------------------------------------------------------------------------------------------

        protected virtual void Awake()
        {
            particleSystem = GetComponentInChildren<ParticleSystem>(true);
            if (particleSystem == null)
                particleSystem = GetComponentInParent<ParticleSystem>(true);

            ParticleSystem.CollisionModule collision = particleSystem.collision;
            collision.sendCollisionMessages = true;
        }

        //--------------------------------------------------------------------------------------------------------------

        private void OnParticleCollision(GameObject other)
        {
#if UNITY_EDITOR
            ++_count;
#endif
            if (onParticleCollision == null)
                Debug.LogWarning($"{this}({transform.GetPath(true)}).{nameof(onParticleCollision)}({onParticleCollision.GetType().FullName}) not set", this);
            else
            {
                int numCollisionEvents = ParticlePhysicsExtensions.GetCollisionEvents(particleSystem, other, collisionEvents);
                for (int i = 0; i < numCollisionEvents; i++)
                    onParticleCollision(other, collisionEvents[i]);
            }
        }
    }
}