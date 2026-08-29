#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEngine;

partial class Util
{
    [MenuItem("Assets/" + nameof(_EDITOR_) + "/" + nameof(LogShaderPropertyIDs))]
    static void LogShaderPropertyIDs()
    {
        Shader shader = Selection.activeObject as Shader;

        StringBuilder sb = new("enum ShaderIDs\n{\n");

        int propertyCount = ShaderUtil.GetPropertyCount(shader);
        for (int i = 0; i < propertyCount; i++)
        {
            string propName = ShaderUtil.GetPropertyName(shader, i);
            int propID = Shader.PropertyToID(propName);
            sb.AppendLine($"{propName} = {propID},");
        }

        sb.Append("}");
        string text = sb.ToString();

        GUIUtility.systemCopyBuffer = text;
        Debug.Log(text, shader);
    }
}
#endif