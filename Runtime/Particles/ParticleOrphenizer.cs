using UnityEngine;

namespace _UTIL_
{
    internal class ParticleOrphenizer : MonoBehaviour
    {
        [SerializeField] ParticleSystem particleSystem;
        [SerializeField] float delay = 1;

        //--------------------------------------------------------------------------------------------------------------

        private void OnDestroy()
        {
            if (particleSystem != null)
            {
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                particleSystem.transform.SetParent(null, true);
                Destroy(particleSystem.gameObject, delay);
            }
        }
    }
}
