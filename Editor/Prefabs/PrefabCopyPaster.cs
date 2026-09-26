using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Unity.Scripting.LifecycleManagement;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _UTIL_.Editor
{
    static partial class PrefabCopyPaster
    {
        const string ClipboardKey = "_UTIL_.PrefabCopyPaster.Hierarchy";
        const string UndoName = "Paste Hierarchy Values";
        [AutoStaticsCleanup] static readonly HashSet<string> IdentityFields = new() { "m_ObjectHideFlags", "m_CorrespondingSourceObject", "m_PrefabInstance", "m_PrefabAsset", "m_GameObject", "m_Script", "m_Father", "m_Children", "m_RootOrder" };

        [Serializable]
        class Node
        {
            public int id;
            public string name;
            public bool active;
            public int layer;
            public string tag;
            public List<ComponentData> components = new List<ComponentData>();
            public List<Node> children = new List<Node>();
        }

        [Serializable]
        class ComponentData
        {
            public int id;
            public string type;
            public string json;
            public List<ReferenceData> references = new List<ReferenceData>();
        }

        [Serializable]
        class ReferenceData
        {
            public string path;
            public int target;
        }

        [MenuItem("CONTEXT/Transform/Copy Hierarchy Values")]
        static void CopyTransform(MenuCommand command) => CopyHierarchyValues(((Transform)command.context).gameObject);

        [MenuItem("CONTEXT/Transform/Paste Hierarchy Values")]
        static void PasteTransform(MenuCommand command) => PasteHierarchyValues(((Transform)command.context).gameObject);

        [MenuItem("GameObject/Copy Hierarchy Values", false, 49)]
        static void CopySelected() => CopyHierarchyValues(Selection.activeGameObject);

        [MenuItem("GameObject/Paste Hierarchy Values", false, 50)]
        static void PasteSelected() => PasteHierarchyValues(Selection.activeGameObject);

        public static void CopyHierarchyValues(this GameObject source)
        {
            if (source == null) return;
            try
            {
                // SessionState survives assembly reloads and Play/Edit transitions, until the Editor closes.
                var internalIds = new Dictionary<Object, int>();
                foreach (var transform in source.GetComponentsInChildren<Transform>(true))
                {
                    internalIds.Add(transform.gameObject, internalIds.Count + 1);
                    foreach (var component in transform.GetComponents<Component>())
                        if (component != null) internalIds.Add(component, internalIds.Count + 1);
                }
                SessionState.SetString(ClipboardKey, JsonUtility.ToJson(Capture(source, internalIds)));
                Debug.Log($"Hierarchy values copied: {source.name}", source);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, source);
            }
        }

        static Node Capture(GameObject source, Dictionary<Object, int> internalIds)
        {
            var node = new Node { id = internalIds[source], name = source.name, active = source.activeSelf, layer = source.layer, tag = source.tag };
            foreach (var component in source.GetComponents<Component>())
            {
                if (component == null) throw new InvalidOperationException($"Missing script on {source.name}; copy cancelled.");
                var json = JObject.Parse(EditorJsonUtility.ToJson(component));
                // Never transfer Unity identity, prefab links, or hierarchy ownership from the runtime object.
                var body = json.Count == 1 && json.Properties().First().Value is JObject wrapped ? wrapped : json;
                foreach (var key in IdentityFields) body.Remove(key);
                RemoveReferences(json);
                var data = new ComponentData { id = internalIds[component], type = component.GetType().AssemblyQualifiedName, json = json.ToString(Newtonsoft.Json.Formatting.None) };
                using (var serialized = new SerializedObject(component))
                {
                    var property = serialized.GetIterator();
                    while (property.Next(true))
                    {
                        if (property.propertyType != SerializedPropertyType.ObjectReference || IdentityFields.Contains(property.propertyPath.Split('.')[0])) continue;
                        var reference = property.objectReferenceValue;
                        if (reference == null)
                            data.references.Add(new ReferenceData { path = property.propertyPath, target = 0 });
                        else if (internalIds.TryGetValue(reference, out int id))
                            data.references.Add(new ReferenceData { path = property.propertyPath, target = id });
                    }
                }
                node.components.Add(data);
            }
            foreach (Transform child in source.transform)
                node.children.Add(Capture(child.gameObject, internalIds));
            return node;
        }

        static void RemoveReferences(JToken token)
        {
            if (token is JObject obj)
            {
                foreach (var property in obj.Properties().ToArray())
                {
                    if (property.Value is JObject reference && reference["instanceID"] != null)
                        property.Remove();
                    else
                        RemoveReferences(property.Value);
                }
            }
            else if (token is JArray array)
            {
                foreach (var item in array.ToArray())
                {
                    if (item is JObject reference && reference["instanceID"] != null)
                        reference["instanceID"] = 0;
                    else
                        RemoveReferences(item);
                }
            }
        }

        public static void PasteHierarchyValues(this GameObject destination)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || destination == null) return;
            if (EditorUtility.IsPersistent(destination))
            {
                Debug.LogWarning("Select a scene instance or open the prefab in Prefab Mode before pasting.", destination);
                return;
            }
            var clipboard = SessionState.GetString(ClipboardKey, "");
            if (string.IsNullOrEmpty(clipboard))
            {
                Debug.LogWarning("No hierarchy values copied.", destination);
                return;
            }
            var node = JsonUtility.FromJson<Node>(clipboard);
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(UndoName);
            try
            {
                ValidateTypes(node);
                var objects = new Dictionary<int, Object>();
                var values = new List<KeyValuePair<Component, ComponentData>>();
                Restore(node, destination, objects, values);
                // All targets now exist, including children and components created during this paste.
                foreach (var pair in values)
                {
                    Undo.RegisterCompleteObjectUndo(pair.Key, UndoName);
                    EditorJsonUtility.FromJsonOverwrite(pair.Value.json, pair.Key);
                    RecordOverrides(pair.Key);
                }
                foreach (var pair in values) RestoreReferences(pair.Key, pair.Value, objects);
                Undo.FlushUndoRecordObjects();
                Undo.CollapseUndoOperations(group);
                Debug.Log($"Hierarchy values pasted: {destination.name}. Undo and prefab Apply/Revert are available.", destination);
            }
            catch (Exception exception)
            {
                Undo.FlushUndoRecordObjects();
                Undo.RevertAllDownToGroup(group);
                Debug.LogException(exception, destination);
            }
        }

        static void ValidateTypes(Node node)
        {
            foreach (var data in node.components)
            {
                var type = Type.GetType(data.type);
                if (type == null || !typeof(Component).IsAssignableFrom(type))
                    throw new InvalidOperationException($"Component type unavailable: {data.type}");
            }
            foreach (var child in node.children) ValidateTypes(child);
        }

        static void RestoreReferences(Component component, ComponentData data, Dictionary<int, Object> objects)
        {
            if (data.references == null || data.references.Count == 0) return;
            using (var serialized = new SerializedObject(component))
            {
                foreach (var reference in data.references)
                {
                    var property = serialized.FindProperty(reference.path);
                    if (property == null || property.propertyType != SerializedPropertyType.ObjectReference)
                        throw new InvalidOperationException($"Reference property unavailable: {reference.path}");
                    Object target = null;
                    if (reference.target != 0 && (!objects.TryGetValue(reference.target, out target) || target == null))
                        throw new InvalidOperationException($"Copied reference has no destination: {reference.path}");
                    property.objectReferenceValue = target;
                }
                serialized.ApplyModifiedProperties();
            }
            RecordOverrides(component);
        }

        static void Restore(Node node, GameObject destination, Dictionary<int, Object> objects, List<KeyValuePair<Component, ComponentData>> values)
        {
            if (node.id != 0) objects.Add(node.id, destination);
            Undo.RegisterCompleteObjectUndo(destination, UndoName);
            destination.name = node.name;
            destination.layer = node.layer;
            destination.tag = node.tag;
            destination.SetActive(node.active);
            RecordOverrides(destination);

            var remaining = destination.GetComponents<Component>().ToList();
            var pairs = new List<KeyValuePair<Component, ComponentData>>();
            foreach (var data in node.components)
            {
                var type = Type.GetType(data.type);
                var component = remaining.FirstOrDefault(item => item != null && item.GetType() == type);
                if (component != null)
                    remaining.Remove(component);
                else
                {
                    // A RequireComponent dependency may have been added by an earlier AddComponent.
                    component = destination.GetComponents(type).Cast<Component>().FirstOrDefault(item => pairs.All(pair => pair.Key != item));
                    if (component == null) component = Undo.AddComponent(destination, type);
                }
                if (component == null) throw new InvalidOperationException($"Cannot add {type.Name} to {destination.name}.");
                pairs.Add(new KeyValuePair<Component, ComponentData>(component, data));
                if (data.id != 0) objects.Add(data.id, component);
            }
            // Remove dependents before their requirements where possible; fail atomically if Unity refuses.
            for (int index = remaining.Count - 1; index >= 0; index--)
            {
                var component = remaining[index];
                if (component == null) throw new InvalidOperationException($"Missing script on {destination.name}.");
                if (component is Transform) throw new InvalidOperationException("Transform and RectTransform must match on the destination.");
                Undo.DestroyObjectImmediate(component);
                if (component != null) throw new InvalidOperationException($"Cannot remove component on {destination.name}.");
            }
            values.AddRange(pairs);

            // Match direct children by name and occurrence, so duplicate names are supported.
            var children = destination.transform.Cast<Transform>().ToList();
            for (int index = 0; index < node.children.Count; index++)
            {
                var childData = node.children[index];
                var child = children.FirstOrDefault(item => item.name == childData.name);
                if (child != null)
                    children.Remove(child);
                else
                {
                    var transformType = Type.GetType(childData.components[0].type);
                    var created = new GameObject(childData.name, transformType);
                    Undo.RegisterCreatedObjectUndo(created, UndoName);
                    Undo.SetTransformParent(created.transform, destination.transform, UndoName);
                    child = created.transform;
                }
                Undo.RegisterCompleteObjectUndo(child, UndoName);
                child.SetSiblingIndex(index);
                Restore(childData, child.gameObject, objects, values);
            }
            foreach (var child in children)
            {
                Undo.DestroyObjectImmediate(child.gameObject);
                if (child != null) throw new InvalidOperationException("Unity refused to remove a child (possibly a nested prefab constraint).");
            }
        }

        static void RecordOverrides(Object target)
        {
            EditorUtility.SetDirty(target);
            if (PrefabUtility.IsPartOfPrefabInstance(target))
                PrefabUtility.RecordPrefabInstancePropertyModifications(target);
        }
    }
}