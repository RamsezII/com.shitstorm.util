using UnityEngine;

partial class Util
{
    /// <summary>
    /// Calcule la bounds en MONDE en agrégeant tous les Colliders et Renderers.
    /// </summary>
    public static Bounds GetWorldBounds(
        this Transform root,
        bool includeColliders = true,
        bool includeRenderers = true,
        bool includeInactive = true,
        bool includeTriggerColliders = true)
    {
        var hasAny = false;
        Bounds world = new(root.position, Vector3.zero);

        if (includeColliders)
        {
            var cols = root.GetComponentsInChildren<Collider>(includeInactive);
            foreach (var c in cols)
            {
                if (!includeTriggerColliders && c.isTrigger) continue;
                if (!hasAny) { world = c.bounds; hasAny = true; }
                else world.Encapsulate(c.bounds);
            }
        }

        if (includeRenderers)
        {
            var rends = root.GetComponentsInChildren<Renderer>(includeInactive);
            foreach (var r in rends)
            {
                // Si tu veux ignorer les renderers désactivés visuellement, décommente :
                // if (!r.enabled) continue;
                if (!hasAny) { world = r.bounds; hasAny = true; }
                else world.Encapsulate(r.bounds);
            }
        }

        if (!hasAny)
            return new Bounds(root.position, Vector3.zero);

        return world;
    }

    /// <summary>
    /// Convertit une bounds MONDE en bounds LOCALE par rapport à "space" (souvent root).
    /// </summary>
    public static Bounds WorldToLocalBounds(Transform space, Bounds worldBounds)
    {
        // Transforme les 8 coins dans l’espace local puis ré-encapsule
        Vector3 c = worldBounds.center;
        Vector3 e = worldBounds.extents;

        var corners = new Vector3[8]
        {
            new Vector3( c.x - e.x, c.y - e.y, c.z - e.z ),
            new Vector3( c.x + e.x, c.y - e.y, c.z - e.z ),
            new Vector3( c.x - e.x, c.y + e.y, c.z - e.z ),
            new Vector3( c.x + e.x, c.y + e.y, c.z - e.z ),
            new Vector3( c.x - e.x, c.y - e.y, c.z + e.z ),
            new Vector3( c.x + e.x, c.y - e.y, c.z + e.z ),
            new Vector3( c.x - e.x, c.y + e.y, c.z + e.z ),
            new Vector3( c.x + e.x, c.y + e.y, c.z + e.z ),
        };

        var local = new Bounds(space.InverseTransformPoint(corners[0]), Vector3.zero);
        for (int i = 1; i < corners.Length; i++)
            local.Encapsulate(space.InverseTransformPoint(corners[i]));

        return local;
    }

    // Raccourcis pratiques
    public static Vector3 GetWorldSize(this Transform root,
        bool includeColliders = true, bool includeRenderers = true,
        bool includeInactive = true, bool includeTriggerColliders = true)
        => GetWorldBounds(root, includeColliders, includeRenderers, includeInactive, includeTriggerColliders).size;

    public static Vector3 GetLocalSize(this Transform root,
        bool includeColliders = true, bool includeRenderers = true,
        bool includeInactive = true, bool includeTriggerColliders = true)
        => WorldToLocalBounds(root.transform,
            GetWorldBounds(root, includeColliders, includeRenderers, includeInactive, includeTriggerColliders)).size;
}