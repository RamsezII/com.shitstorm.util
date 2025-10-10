#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace _UTIL_e
{
    [InitializeOnLoad]
    public static class DrawRigidbodyCenters
    {
        static readonly List<Rigidbody> rigidbodies = new();

        //----------------------------------------------------------------------------------------------------------

        static DrawRigidbodyCenters()
        {
            RefreshList();

            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;

            EditorApplication.hierarchyChanged -= RefreshList;
            EditorApplication.hierarchyChanged += RefreshList;
        }

        //----------------------------------------------------------------------------------------------------------

        static void RefreshList()
        {
            rigidbodies.Clear();
            rigidbodies.AddRange(Object.FindObjectsByType<Rigidbody>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));
        }

        static void OnSceneGUI(SceneView sceneView)
        {
            Color color = Color.yellow;
            color.a = .85f;
            Handles.color = color;

            for (int i = 0; i < rigidbodies.Count; i++)
            {
                Rigidbody rb = rigidbodies[i];

                Vector3 com = rb.worldCenterOfMass;
                float size = Mathf.Lerp(
                    0.25f,
                    .1f * HandleUtility.GetHandleSize(com),
                    .5f);

                Handles.DrawLine(com + Vector3.up * size, com - Vector3.up * size);
                Handles.DrawLine(com + Vector3.right * size, com - Vector3.right * size);
                Handles.DrawLine(com + Vector3.forward * size, com - Vector3.forward * size);

                Handles.DrawWireDisc(com, sceneView.camera.transform.forward, size * 0.6f);
            }
        }
    }
}
#endif