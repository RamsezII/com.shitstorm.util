#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEngine;

namespace _UTIL_.Editor
{
    static class Rigidbodies
    {
        [MenuItem("CONTEXT/" + nameof(Rigidbody) + "/" + nameof(LogStatus))]
        static void LogStatus(MenuCommand command)
        {
            var rgb = (Rigidbody)command.context;
            var sb = new StringBuilder();
            sb.AppendLine($"{nameof(rgb.detectCollisions)}: {rgb.detectCollisions}");
            sb.AppendLine($"{nameof(rgb.collisionDetectionMode)}: {rgb.collisionDetectionMode}");
            Debug.Log(sb.ToString()[..^1], rgb);
        }
    }
}
#endif