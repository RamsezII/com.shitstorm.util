using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace _UTIL_.Editor
{
    [InitializeOnLoad]
    internal static class OptionalModuleDefines
    {
        static readonly (string assemblyName, string symbol)[] modules =
        {
            ("shitstorm._BOA_", "HAS_BOA"),
            ("shitstorm._SGUI_", "HAS_SGUI"),
            ("shitstorm._TERM_", "HAS_TERM"),
            ("shitstorm._TERMINAL_", "HAS_TERMINAL"),
        };

        [Serializable]
        sealed class AssemblyDefinition
        {
            public string name;
        }

        static OptionalModuleDefines() => Schedule();

        internal static void Schedule()
        {
            EditorApplication.delayCall -= Synchronize;
            EditorApplication.delayCall += Synchronize;
        }

        [MenuItem("Tools/SHITSTORM/Synchronize optional module defines")]
        internal static void Synchronize()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                Schedule();
                return;
            }

            var assemblyNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (string guid in AssetDatabase.FindAssets("t:AssemblyDefinitionAsset"))
            {
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null)
                    assemblyNames.Add(JsonUtility.FromJson<AssemblyDefinition>(asset.text).name);
            }

            var targetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            var target = NamedBuildTarget.FromBuildTargetGroup(targetGroup);
            string current = PlayerSettings.GetScriptingDefineSymbols(target);
            string updated = GetSymbols(current, assemblyNames);
            if (updated != current)
                PlayerSettings.SetScriptingDefineSymbols(target, updated);
        }

        internal static string GetSymbols(string current, HashSet<string> assemblyNames)
        {
            var symbols = current.Split(';').Where(symbol => !string.IsNullOrWhiteSpace(symbol)).Distinct().ToList();
            foreach (var module in modules)
            {
                if (!assemblyNames.Contains(module.assemblyName))
                    symbols.Remove(module.symbol);
                else if (!symbols.Contains(module.symbol))
                    symbols.Add(module.symbol);
            }
            return string.Join(";", symbols);
        }
    }

    internal sealed class OptionalModuleDefinesPostprocessor : AssetPostprocessor, IActiveBuildTargetChanged
    {
        public int callbackOrder => 0;

        public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget) => OptionalModuleDefines.Schedule();

        static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            if (imported.Concat(deleted).Concat(moved).Concat(movedFrom).Any(path => path.EndsWith(".asmdef", StringComparison.OrdinalIgnoreCase)))
                OptionalModuleDefines.Schedule();
        }
    }
}
