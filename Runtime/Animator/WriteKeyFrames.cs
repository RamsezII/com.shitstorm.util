#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

partial class Util_e
{
    /// Écrit des keyframes pour toutes les feuilles float sous `rootProp` (SerializedProperty),
    /// à l’instant `time`, sur les courbes de `clip` ciblant `component`.
    /// - Parcours borné par PROFONDEUR (pas d’évasion hors sous-arbre)
    /// - Batch : un seul SetEditorCurves à la fin
    /// - Merge : si une clé existe ~au même time, on la REMPLACE proprement
    public static void WriteKeyframes(this AnimationClip clip, in Component component, in SerializedProperty property, float time, bool write_all_curves = false, float epsilon = 1e-5f)
    {
        if (!clip || !component || property == null)
            return;

        // si c'est une ref non initialisée → rien à écrire
        if (property.propertyType == SerializedPropertyType.ManagedReference && property.managedReferenceValue == null)
            return;

        Transform root = component.GetComponentInParent<Animator>().transform;
        if (!root)
            return;

        string path = AnimationUtility.CalculateTransformPath(component.transform, root);
        Type type = component.GetType();

        Dictionary<EditorCurveBinding, AnimationCurve> curves = new();

        // --- récursif ---
        void AddKey(string propPath, float v)
        {
            EditorCurveBinding b = new() { path = path, type = type, propertyName = propPath };

            // si la courbe n'existe pas et force==false -> on skip
            AnimationCurve existingCurve = AnimationUtility.GetEditorCurve(clip, b);
            if (existingCurve == null && !write_all_curves)
                return;

            if (!curves.TryGetValue(b, out var c))
            {
                c = existingCurve ?? new AnimationCurve();
                curves.Add(b, c);
            }

            UpsertKey(c, time, v, epsilon);
        }

        void Process(SerializedProperty p)
        {
            switch (p.propertyType)
            {
                case SerializedPropertyType.Float:
                    AddKey(p.propertyPath, p.floatValue);
                    return;

                // ---- types Unity "packés" : on éclate ici ----
                case SerializedPropertyType.Vector2:
                    Vector2 v2 = p.vector2Value;
                    AddKey(p.propertyPath + ".x", v2.x);
                    AddKey(p.propertyPath + ".y", v2.y);
                    return;

                case SerializedPropertyType.Vector3:
                    Vector3 v3 = p.vector3Value;
                    AddKey(p.propertyPath + ".x", v3.x);
                    AddKey(p.propertyPath + ".y", v3.y);
                    AddKey(p.propertyPath + ".z", v3.z);
                    return;

                case SerializedPropertyType.Vector4:
                    Vector4 v4 = p.vector4Value;
                    AddKey(p.propertyPath + ".x", v4.x);
                    AddKey(p.propertyPath + ".y", v4.y);
                    AddKey(p.propertyPath + ".z", v4.z);
                    AddKey(p.propertyPath + ".w", v4.w);
                    return;

                case SerializedPropertyType.Quaternion:
                    Quaternion q = p.quaternionValue;
                    AddKey(p.propertyPath + ".x", q.x);
                    AddKey(p.propertyPath + ".y", q.y);
                    AddKey(p.propertyPath + ".z", q.z);
                    AddKey(p.propertyPath + ".w", q.w);
                    return;

                case SerializedPropertyType.Color:
                    Color c = p.colorValue;
                    AddKey(p.propertyPath + ".r", c.r);
                    AddKey(p.propertyPath + ".g", c.g);
                    AddKey(p.propertyPath + ".b", c.b);
                    AddKey(p.propertyPath + ".a", c.a);
                    return;

                // ---- conteneurs / structs custom : on descend ----
                case SerializedPropertyType.Generic:
                case SerializedPropertyType.ManagedReference:
                    if (p.hasVisibleChildren)
                    {
                        var it = p.Copy();
                        int depth0 = it.depth;

                        if (it.NextVisible(true))
                            do
                            {
                                if (it.depth <= depth0)
                                    break;
                                Process(it);
                            }
                            while (it.NextVisible(false));
                    }
                    return;

                // ---- arrays / lists ----
                default:
                    if (p.isArray && p.propertyType != SerializedPropertyType.String)
                    {
                        for (int i = 0; i < p.arraySize; i++)
                            Process(p.GetArrayElementAtIndex(i));
                    }
                    return;
            }
        }

        Process(property);

        // Commit en une passe
        if (curves.Count > 0)
        {
            EditorCurveBinding[] bindings = new EditorCurveBinding[curves.Count];
            AnimationCurve[] outCurves = new AnimationCurve[curves.Count];
            int i = 0;

            foreach (var kv in curves)
            {
                bindings[i] = kv.Key;
                outCurves[i] = kv.Value;
                i++;
            }

            AnimationUtility.SetEditorCurves(clip, bindings, outCurves);
            // Optionnel : si tu as potentiellement écrit des quaternions :
            clip.EnsureQuaternionContinuity();
        }
    }

    public static void WriteTransformKeyframes(
        this AnimationClip clip,
        in Transform transform,
        float time,
        in bool write_pos = false,
        in bool write_rot = false,
        in bool write_scale = false,
        bool create_if_abscent = false,
        float epsilon = 1e-5f)
    {
        if (!clip || !transform) return;

        Transform animRoot = transform.GetComponentInParent<Animator>().transform;
        if (!animRoot) return;

        string path = AnimationUtility.CalculateTransformPath(transform, animRoot);
        Type type = typeof(Transform);

        List<EditorCurveBinding> bindings = new(10);
        List<AnimationCurve> curves = new(10);

        void AddKey(string propName, float value)
        {
            EditorCurveBinding b = new() { path = path, type = type, propertyName = propName };

            // Vérifier si la courbe existe déjà
            var existingCurve = AnimationUtility.GetEditorCurve(clip, b);
            if (existingCurve == null && !create_if_abscent)
                return;

            int idx = bindings.IndexOf(b);
            AnimationCurve c;

            if (idx >= 0)
                c = curves[idx];
            else
            {
                c = existingCurve ?? new AnimationCurve();
                bindings.Add(b);
                curves.Add(c);
            }

            UpsertKey(c, time, value, epsilon);
        }

        if (write_pos)
        {
            Vector3 p = transform.localPosition;
            AddKey("m_LocalPosition.x", p.x);
            AddKey("m_LocalPosition.y", p.y);
            AddKey("m_LocalPosition.z", p.z);
        }

        if (write_rot)
        {
            Vector3 e = transform.localEulerAngles;
            AddKey("localEulerAnglesRaw.x", e.x);
            AddKey("localEulerAnglesRaw.y", e.y);
            AddKey("localEulerAnglesRaw.z", e.z);
            //Quaternion q = transform.localRotation;
            //AddKey("m_LocalRotation.x", q.x);
            //AddKey("m_LocalRotation.y", q.y);
            //AddKey("m_LocalRotation.z", q.z);
            //AddKey("m_LocalRotation.w", q.w);
        }

        if (write_scale)
        {
            Vector3 s = transform.localScale;
            AddKey("m_LocalScale.x", s.x);
            AddKey("m_LocalScale.y", s.y);
            AddKey("m_LocalScale.z", s.z);
        }

        if (bindings.Count == 0)
            return;

        AnimationUtility.SetEditorCurves(clip, bindings.ToArray(), curves.ToArray());
        clip.EnsureQuaternionContinuity();
    }

    // remplace/ajoute une clé au même instant (tolérance eps)
    static void UpsertKey(AnimationCurve curve, float time, float value, float eps)
    {
        var keys = curve.keys;
        for (int i = 0; i < keys.Length; i++)
        {
            if (Mathf.Abs(keys[i].time - time) <= eps)
            {
                var k = keys[i];
                k.value = value;
                curve.MoveKey(i, k);
                return;
            }
        }
        curve.AddKey(new Keyframe(time, value));
    }
}
#endif