using UnityEngine;

public static partial class SphereDepenetrationProxy
{
    static SphereCollider proxy_sphere;
    static readonly Collider[] collision_buffer = new Collider[8];

    //----------------------------------------------------------------------------------------------------------

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoad()
    {
        GameObject go = new(typeof(SphereDepenetrationProxy).FullName)
        {
            hideFlags = HideFlags.HideAndDontSave
        };

        Object.DontDestroyOnLoad(go);

        proxy_sphere = go.AddComponent<SphereCollider>();
        proxy_sphere.isTrigger = true;
        go.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    //----------------------------------------------------------------------------------------------------------

    public static bool SolveCameraPenetration(
        in Vector3 position,
        in float radius,
        in LayerMask mask,
        out Vector3 corrected,
        in float minObstacleSize = 0.2f,
        in int iterations = 3)
    {
        proxy_sphere.radius = radius;

        Quaternion proxy_rotation = proxy_sphere.transform.rotation;
        corrected = position;

        bool changed = false;

        for (int it = 0; it < iterations; it++)
        {
            proxy_sphere.transform.position = corrected;

            int overlaps_count = Physics.OverlapSphereNonAlloc(
                corrected,
                radius,
                collision_buffer,
                mask,
                QueryTriggerInteraction.Ignore);

            if (overlaps_count > 0)
            {
                Vector3 totalPush = Vector3.zero;
                bool pushed = false;

                for (int i = 0; i < overlaps_count; i++)
                {
                    Collider collider = collision_buffer[i];

                    // calcule la taille max du collider
                    Vector3 size = collider.bounds.size;
                    float maxAxis = Mathf.Max(size.x, size.y, size.z);

                    // ignore les tout petits trucs (poteaux fins, poignées, etc.)
                    if (maxAxis > minObstacleSize)
                    {
                        // calcule la direction de sortie
                        if (Physics.ComputePenetration(
                            colliderA: proxy_sphere,
                            positionA: corrected,
                            rotationA: proxy_rotation,
                            colliderB: collider,
                            positionB: collider.transform.position,
                            rotationB: collider.transform.rotation,
                            direction: out Vector3 dir,
                            distance: out float dist))
                        {
                            totalPush += dir * (dist + 0.001f);
                            pushed = true;
                            changed = true;
                        }
                    }
                }

                if (!pushed)
                    break;

                corrected += totalPush;
            }
        }

        return changed;
    }
}
