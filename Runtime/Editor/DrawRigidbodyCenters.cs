#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace _UTIL_e
{
    public static class DrawRigidbodyCenters
    {
        static readonly List<Rigidbody> rigidbodies = new();

        public static readonly bool DRAW = false;

        //----------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            EditorApplication.hierarchyChanged -= RefreshList;

            if (DRAW)
            {
                SceneView.duringSceneGui += OnSceneGUI;
                EditorApplication.hierarchyChanged += RefreshList;
                RefreshList();
            }
        }

        //----------------------------------------------------------------------------------------------------------

        static void RefreshList()
        {
            rigidbodies.Clear();
            rigidbodies.AddRange(Object.FindObjectsByType<Rigidbody>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));
        }

        static void OnSceneGUI(SceneView sceneView)
        {
            Vector3 disc_dir = sceneView.camera.transform.forward;

            Color color_rb = Color.yellow;
            color_rb.a = .6f;
            Color color_cog = Color.yellow;
            color_cog.a = .85f;

            for (int i = 0; i < rigidbodies.Count; i++)
                if (rigidbodies[i] != null)
                {
                    Rigidbody rb = rigidbodies[i];

                    Vector3 com = rb.worldCenterOfMass;
                    float size = Mathf.Lerp(
                        0.25f,
                        .1f * HandleUtility.GetHandleSize(com),
                        .5f);
                    Vector3 size_x = new(size, 0, 0);
                    Vector3 size_y = new(0, size, 0);
                    Vector3 size_z = new(0, 0, size);

                    Draw(com, color_cog);
                    Draw(rb.position, color_rb);

                    void Draw(in Vector3 com, in Color color)
                    {
                        Handles.color = color;

                        Handles.DrawLine(com + size_x, com - size_x);
                        Handles.DrawLine(com + size_y, com - size_y);
                        Handles.DrawLine(com + size_z, com - size_z);

                        Handles.DrawWireDisc(com, disc_dir, size * 0.6f);
                    }
                }
        }
    }
}
#endif