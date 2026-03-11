#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

public static partial class Util_e_OLD
{
    [MenuItem("CONTEXT/" + nameof(AudioSource) + "/" + nameof(_EDITOR_) + "/" + nameof(PlayAudio))]
    static void PlayAudio(MenuCommand menuCommand) => ((AudioSource)menuCommand.context).Play();

    [MenuItem("CONTEXT/" + nameof(AudioSource) + "/" + nameof(_EDITOR_) + "/" + nameof(StopAudio))]
    static void StopAudio(MenuCommand menuCommand) => ((AudioSource)menuCommand.context).Stop();

    [MenuItem("CONTEXT/" + nameof(AudioSource) + "/" + nameof(_EDITOR_) + "/" + nameof(IsPlaying))]
    static void IsPlaying(MenuCommand menuCommand) => Debug.Log(((AudioSource)menuCommand.context).isPlaying);

    [MenuItem("Assets/" + nameof(_EDITOR_) + "/" + nameof(ExportExposedParametersAsEnum))]
    static void ExportExposedParametersAsEnum() => ExportExposedParametersAsEnum((AudioMixer)Selection.activeObject);

    static void ExportExposedParametersAsEnum(in AudioMixer mixer)
    {
        SerializedObject dynMixer = new(mixer);
        SerializedProperty parameters = dynMixer.FindProperty("m_ExposedParameters");
        StringBuilder log = new("public enum Parameters\n{\n");

        if (parameters != null && parameters.isArray)
            for (int i = 0; i < parameters.arraySize; i++)
            {
                SerializedProperty param = parameters.GetArrayElementAtIndex(i);
                SerializedProperty nameProp = param.FindPropertyRelative("name");

                if (nameProp != null)
                    log.AppendLine($"{nameProp.stringValue},");
            }

        log.Append("}");
        string message = log.ToString();
        Debug.Log(message);
        GUIUtility.systemCopyBuffer = message;
    }

    [MenuItem("Assets/" + nameof(_EDITOR_) + "/" + nameof(LogAllAudioListeners))]
    static void LogAllAudioListeners(MenuCommand command)
    {
        foreach (AudioListener listener in Object.FindObjectsOfType<AudioListener>(true))
            Debug.Log($"{listener.name}.{nameof(listener.enabled)}: {listener.enabled} ({listener.transform.GetPath(true)})");
    }
}
#endif