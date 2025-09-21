#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static partial class Util_e_OLD
{
    [MenuItem("CONTEXT/" + nameof(Animator) + "/" + nameof(_EDITOR_) + "/" + nameof(LogHumanScale))]
    static void LogHumanScale(MenuCommand command) => Debug.Log(((Animator)command.context).humanScale);

    [MenuItem("CONTEXT/" + nameof(Animator) + "/" + nameof(_EDITOR_) + "/" + nameof(LogAnimatorClips))]
    static void LogClips(MenuCommand command) => LogAnimatorClips((Animator)command.context);

    [MenuItem("CONTEXT/" + nameof(Animator) + "/" + nameof(_EDITOR_) + "/" + nameof(LogSpeed))]
    static void LogSpeed(MenuCommand command) => Debug.Log(((Animator)command.context).speed);

    [MenuItem("CONTEXT/" + nameof(Animator) + "/" + nameof(_EDITOR_) + "/" + nameof(RebindAnimator))]
    static void RebindAnimator(MenuCommand command) => ((Animator)command.context).Rebind();

    public static void LogAnimatorClips(this Animator animator)
    {
        string log = "";
        int i = 0;

        foreach (var clip in animator.runtimeAnimatorController.animationClips)
            log += (i++) + " : " + clip.name + "\n";

        Debug.Log(log);
    }

    [MenuItem("Assets/" + nameof(_EDITOR_) + "/" + nameof(LogAnimatorHashes))]
    static void LogAnimatorHashes() => LogAnimatorHashes((AnimatorController)Selection.activeObject);

    [MenuItem("CONTEXT/" + nameof(Animator) + "/" + nameof(_EDITOR_) + "." + nameof(LogAnimatorHashes))]
    static void LogAnimatorHashes(MenuCommand command) => LogAnimatorHashes(((Animator)command.context).runtimeAnimatorController as AnimatorController);

    public static void LogAnimatorHashes(this AnimatorController animator)
    {
        StringBuilder log = new();

        if (animator.layers.Length > 0)
        {
            log.Append("public enum AnimLayers : byte { ");
            foreach (AnimatorControllerLayer layer in animator.layers)
                log.Append(layer.name + ", ");
            log.AppendLine("_last_ }");

            foreach (AnimatorControllerLayer layer in animator.layers)
                if (layer.syncedLayerIndex < 0)
                {
                    log.AppendLine();
                    log.AppendLine($"public enum {layer.name}States");
                    log.AppendLine("{");

                    AllStates(layer.stateMachine, string.Empty, layer.name);

                    log.AppendLine("}");

                    void AllStates(in AnimatorStateMachine stateMachine, in string branch_name, in string branch_hash)
                    {
                        if (stateMachine.stateMachines != null)
                            foreach (ChildAnimatorStateMachine subStateMachine in stateMachine.stateMachines)
                                if (string.IsNullOrEmpty(branch_name))
                                    AllStates(subStateMachine.stateMachine, $"{subStateMachine.stateMachine.name}_", $"{branch_hash}.{subStateMachine.stateMachine.name}");
                                else
                                    AllStates(subStateMachine.stateMachine, $"{branch_name}_{subStateMachine.stateMachine.name}_", $"{branch_hash}.{subStateMachine.stateMachine.name}");

                        if (stateMachine.states != null)
                            foreach (var subState in stateMachine.states)
                            {
                                log.Append($"    {branch_name}{subState.state.name} = ");
                                if (string.IsNullOrEmpty(branch_hash))
                                    log.AppendLine(subState.state.nameHash + ",");
                                else
                                {
                                    string fullhashname = branch_hash + "." + subState.state.name;
                                    log.AppendLine(Animator.StringToHash(fullhashname) + ",");
                                }
                            }
                    }
                }
        }

        if (animator.parameters.Length > 0)
        {
            log.AppendLine();
            log.AppendLine("public enum AnimParameters");
            log.AppendLine("{");

            foreach (var p in animator.parameters)
                log.AppendLine($"    {p.name} = {p.nameHash},");

            log.AppendLine("}");
        }

        string _log = log.ToString();
        Debug.Log(_log);
        _log.WriteToClipboard();
    }
}
#endif