#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace _UTIL_e
{
    /// <summary>
    /// Matches two skinned skeletons by topology, renames the target bones, then optionally
    /// rebuilds the target bind poses at its current pose. Imported meshes are never edited.
    /// </summary>
    [DisallowMultipleComponent]
    sealed class HierarchyRenamer : MonoBehaviour
    {
        [Header("Characters")]
        [SerializeField, Tooltip("Canonical character. If empty, this component's Transform is used.")]
        Transform source;

        [SerializeField, Tooltip("Character whose bones will be renamed and rebound.")]
        Transform target;

        [Header("Matching")]
        [SerializeField, Range(0.5f, 1f), Tooltip("Higher values leave more ambiguous branches untouched.")]
        float minimumTopologySimilarity = 0.72f;

        [SerializeField, Tooltip("Normally disabled: character root names do not affect skinning.")]
        bool renameCharacterRoots;

        [SerializeField, Tooltip("Copies the source import-wrapper rotation and scale (for example ModelObject x100/-90 degrees) while preserving the target's world-space size and joint positions.")]
        bool normalizeImportSpace = true;

        [SerializeField, Tooltip("Copies matched source local rotations while preserving the target joint positions and unmatched branch world rotations.")]
        bool alignLocalRotations = true;

        [Header("Rebind")]
        [SerializeField, Tooltip("Uses the target's current pose as its new bind pose. Put it in the intended rest pose first.")]
        bool rebindSkinnedMeshes = true;

        [Header("Report")]
        [SerializeField] bool logDetailedReport;

        //----------------------------------------------------------------------------------------------------------

        [ContextMenu("Preview Bone Matching")]
        internal void Preview()
        {
            MatchPlan plan;
            if (TryBuildPlan(out plan))
                LogPlan(plan, false, 0);
        }

        [ContextMenu("Match Bones, Align Target And Rebind")]
        internal void Operate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("Run the bone matcher outside Play Mode.", this);
                return;
            }
            if ((normalizeImportSpace || alignLocalRotations) && !rebindSkinnedMeshes)
            {
                Debug.LogError("Import-space normalization and rotation alignment require Rebind Skinned Meshes.", this);
                return;
            }
            if (rebindSkinnedMeshes && AnimationMode.InAnimationMode())
            {
                Debug.LogError("Exit Animation Preview before rebinding; its displayed pose must not become the bind pose.", this);
                return;
            }

            MatchPlan plan;
            if (!TryBuildPlan(out plan))
                return;
            if (plan.Pairs.Count <= 1)
            {
                Debug.LogError("No reliable bone correspondence was found. Nothing was changed.", this);
                return;
            }

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Match skeleton topology and rebind");
            int reboundCount = 0;
            int alignedRotationCount = 0;
            int normalizedWrapperCount = 0;
            bool operationSucceeded = false;

            try
            {
                RenameMatchedBones(plan);
                if (normalizeImportSpace)
                    normalizedWrapperCount = NormalizeTargetImportSpace(plan);
                if (alignLocalRotations)
                    alignedRotationCount = AlignMatchedBoneRotations(plan);
                if (rebindSkinnedMeshes)
                    reboundCount = RebindTargetRenderers(plan.TargetRoot);

                if (plan.TargetRoot.gameObject.scene.IsValid())
                    EditorSceneManager.MarkSceneDirty(plan.TargetRoot.gameObject.scene);
                AssetDatabase.SaveAssets();
                LogPlan(plan, true, reboundCount);
                if (alignLocalRotations)
                    Debug.Log("Canonical local rotations applied to " + alignedRotationCount + " target bones; target joint positions were preserved.", this);
                if (normalizeImportSpace)
                    Debug.Log("Canonical import space applied to " + normalizedWrapperCount + " target wrappers; renderer world transforms were preserved.", this);
                operationSucceeded = true;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                Undo.RevertAllDownToGroup(undoGroup);
                AssetDatabase.SaveAssets();
                Debug.LogError("The operation failed and was rolled back completely; no partial bone rotation was kept.", this);
            }
            finally
            {
                if (operationSucceeded)
                    Undo.CollapseUndoOperations(undoGroup);
            }
        }

        bool TryBuildPlan(out MatchPlan plan)
        {
            plan = null;
            Transform sourceRoot = source != null ? source : transform;

            if (sourceRoot == null || target == null)
            {
                Debug.LogError("Assign a target character. The source can be explicit or this component's Transform.", this);
                return false;
            }
            if (sourceRoot == target || sourceRoot.IsChildOf(target) || target.IsChildOf(sourceRoot))
            {
                Debug.LogError("Source and target must be two separate character hierarchies.", this);
                return false;
            }

            int sourceRendererCount;
            int sourceExternalBoneCount;
            HashSet<Transform> sourceSkinBones;
            HashSet<Transform> sourceBones = BuildBoneHierarchy(sourceRoot, out sourceRendererCount,
                out sourceExternalBoneCount, out sourceSkinBones);
            int targetRendererCount;
            int targetExternalBoneCount;
            HashSet<Transform> targetSkinBones;
            HashSet<Transform> targetBones = BuildBoneHierarchy(target, out targetRendererCount,
                out targetExternalBoneCount, out targetSkinBones);

            if (sourceRendererCount == 0 || sourceBones.Count <= 1)
            {
                Debug.LogError("The source has no usable SkinnedMeshRenderer bone hierarchy. Select the whole character.", this);
                return false;
            }
            if (targetRendererCount == 0 || targetBones.Count <= 1)
            {
                Debug.LogError("The target has no usable SkinnedMeshRenderer bone hierarchy. Select the whole character.", this);
                return false;
            }

            TopologyNode sourceTree = BuildTopology(sourceRoot, sourceBones);
            TopologyNode targetTree = BuildTopology(target, targetBones);
            plan = new MatchPlan(sourceRoot, target, sourceBones.Count, targetBones.Count, targetBones,
                sourceSkinBones, targetSkinBones, sourceExternalBoneCount, targetExternalBoneCount);
            AddPair(plan, sourceTree, targetTree, 1f, "explicit roots");
            MatchChildren(plan, sourceTree, targetTree);
            return true;
        }

        static HashSet<Transform> BuildBoneHierarchy(Transform characterRoot, out int rendererCount,
            out int externalBoneCount, out HashSet<Transform> skinBones)
        {
            HashSet<Transform> result = new HashSet<Transform>();
            skinBones = new HashSet<Transform>();
            result.Add(characterRoot);
            externalBoneCount = 0;

            SkinnedMeshRenderer[] renderers = characterRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            rendererCount = renderers.Length;
            for (int rendererIndex = 0; rendererIndex < renderers.Length; ++rendererIndex)
            {
                SkinnedMeshRenderer renderer = renderers[rendererIndex];
                AddBoneAndAncestors(renderer.rootBone, characterRoot, result, skinBones, ref externalBoneCount);
                Transform[] bones = renderer.bones;
                for (int boneIndex = 0; boneIndex < bones.Length; ++boneIndex)
                    AddBoneAndAncestors(bones[boneIndex], characterRoot, result, skinBones, ref externalBoneCount);
            }
            return result;
        }

        static void AddBoneAndAncestors(Transform bone, Transform root, HashSet<Transform> result,
            HashSet<Transform> skinBones, ref int externalBoneCount)
        {
            if (bone == null)
                return;
            if (bone != root && !bone.IsChildOf(root))
            {
                ++externalBoneCount;
                return;
            }

            skinBones.Add(bone);

            Transform current = bone;
            while (current != null)
            {
                result.Add(current);
                if (current == root)
                    return;
                current = current.parent;
            }
        }

        static TopologyNode BuildTopology(Transform transform, HashSet<Transform> boneHierarchy)
        {
            TopologyNode node = new TopologyNode(transform);
            for (int index = 0; index < transform.childCount; ++index)
            {
                Transform child = transform.GetChild(index);
                if (boneHierarchy.Contains(child))
                    node.Children.Add(BuildTopology(child, boneHierarchy));
            }
            node.Finish();
            return node;
        }

        void MatchChildren(MatchPlan plan, TopologyNode sourceParent, TopologyNode targetParent)
        {
            List<TopologyNode> sourceChildren = sourceParent.Children;
            List<TopologyNode> targetChildren = targetParent.Children;
            bool[] sourceMatched = new bool[sourceChildren.Count];
            bool[] targetMatched = new bool[targetChildren.Count];
            List<PendingPair> matches = new List<PendingPair>();

            // A unique complete subtree can be recognized even when another sibling branch was inserted.
            for (int sourceIndex = 0; sourceIndex < sourceChildren.Count; ++sourceIndex)
            {
                TopologyNode sourceChild = sourceChildren[sourceIndex];
                if (sourceChild.IsLeaf || CountShape(sourceChildren, sourceChild.ShapeHash) != 1)
                    continue;

                int targetIndex = FindUniqueShape(targetChildren, sourceChild.ShapeHash);
                if (targetIndex < 0)
                    continue;

                sourceMatched[sourceIndex] = true;
                targetMatched[targetIndex] = true;
                matches.Add(new PendingPair(sourceChild, targetChildren[targetIndex], 1f, "unique topology"));
            }

            // Equal child counts make the sibling index meaningful. Incompatible shapes are still rejected.
            if (sourceChildren.Count == targetChildren.Count)
            {
                for (int index = 0; index < sourceChildren.Count; ++index)
                {
                    if (sourceMatched[index] || targetMatched[index])
                        continue;
                    float similarity = StructuralSimilarity(sourceChildren[index], targetChildren[index]);
                    if (similarity < minimumTopologySimilarity)
                        continue;

                    sourceMatched[index] = true;
                    targetMatched[index] = true;
                    matches.Add(new PendingPair(sourceChildren[index], targetChildren[index], similarity, "sibling index"));
                }
            }

            // Unequal child counts are dangerous. Require a mutual best match with a clear score margin.
            for (int sourceIndex = 0; sourceIndex < sourceChildren.Count; ++sourceIndex)
            {
                if (sourceMatched[sourceIndex] || sourceChildren[sourceIndex].IsLeaf)
                    continue;

                BestCandidate sourceBest = FindBest(sourceChildren[sourceIndex], sourceIndex, sourceChildren.Count,
                    targetChildren, targetMatched);
                if (sourceBest.Index < 0 || sourceBest.Score < minimumTopologySimilarity || sourceBest.Margin < 0.1f)
                    continue;

                BestCandidate targetBest = FindBest(targetChildren[sourceBest.Index], sourceBest.Index, targetChildren.Count,
                    sourceChildren, sourceMatched);
                if (targetBest.Index != sourceIndex || targetBest.Margin < 0.1f)
                    continue;

                sourceMatched[sourceIndex] = true;
                targetMatched[sourceBest.Index] = true;
                matches.Add(new PendingPair(sourceChildren[sourceIndex], targetChildren[sourceBest.Index],
                    sourceBest.Score, "mutual topology"));
            }

            matches.Sort(delegate(PendingPair left, PendingPair right)
            {
                return left.Source.Transform.GetSiblingIndex().CompareTo(right.Source.Transform.GetSiblingIndex());
            });
            for (int index = 0; index < matches.Count; ++index)
            {
                PendingPair pair = matches[index];
                AddPair(plan, pair.Source, pair.Target, pair.Confidence, pair.Reason);
                MatchChildren(plan, pair.Source, pair.Target);
            }
        }

        static int CountShape(List<TopologyNode> nodes, ulong shapeHash)
        {
            int count = 0;
            for (int index = 0; index < nodes.Count; ++index)
                if (nodes[index].ShapeHash == shapeHash)
                    ++count;
            return count;
        }

        static int FindUniqueShape(List<TopologyNode> nodes, ulong shapeHash)
        {
            int found = -1;
            for (int index = 0; index < nodes.Count; ++index)
            {
                if (nodes[index].ShapeHash != shapeHash)
                    continue;
                if (found >= 0)
                    return -1;
                found = index;
            }
            return found;
        }

        static BestCandidate FindBest(TopologyNode node, int nodeIndex, int nodeCount,
            List<TopologyNode> candidates, bool[] candidateMatched)
        {
            int bestIndex = -1;
            float bestScore = -1f;
            float secondBestScore = -1f;

            for (int index = 0; index < candidates.Count; ++index)
            {
                if (candidateMatched[index] || candidates[index].IsLeaf)
                    continue;
                float structure = StructuralSimilarity(node, candidates[index]);
                float order = SiblingOrderSimilarity(nodeIndex, nodeCount, index, candidates.Count);
                float score = structure * 0.9f + order * 0.1f;
                if (score > bestScore)
                {
                    secondBestScore = bestScore;
                    bestScore = score;
                    bestIndex = index;
                }
                else if (score > secondBestScore)
                {
                    secondBestScore = score;
                }
            }

            float margin = secondBestScore < 0f ? 1f : bestScore - secondBestScore;
            return new BestCandidate(bestIndex, bestScore, margin);
        }

        static float StructuralSimilarity(TopologyNode left, TopologyNode right)
        {
            if (left.ShapeHash == right.ShapeHash)
                return 1f;
            float children = Ratio(left.Children.Count + 1, right.Children.Count + 1);
            float height = Ratio(left.Height + 1, right.Height + 1);
            float nodes = Ratio(left.NodeCount, right.NodeCount);
            float leaves = Ratio(left.LeafCount, right.LeafCount);
            return children * 0.3f + height * 0.25f + nodes * 0.3f + leaves * 0.15f;
        }

        static float Ratio(int left, int right)
        {
            int maximum = Mathf.Max(left, right);
            return maximum == 0 ? 1f : (float)Mathf.Min(left, right) / maximum;
        }

        static float SiblingOrderSimilarity(int leftIndex, int leftCount, int rightIndex, int rightCount)
        {
            float left = leftCount <= 1 ? 0.5f : (float)leftIndex / (leftCount - 1);
            float right = rightCount <= 1 ? 0.5f : (float)rightIndex / (rightCount - 1);
            return 1f - Mathf.Abs(left - right);
        }

        static void AddPair(MatchPlan plan, TopologyNode sourceNode, TopologyNode targetNode,
            float confidence, string reason)
        {
            plan.Pairs.Add(new BonePair(sourceNode.Transform, targetNode.Transform, confidence, reason,
                GetRelativePath(plan.SourceRoot, sourceNode.Transform),
                GetRelativePath(plan.TargetRoot, targetNode.Transform)));
            plan.MatchedSources.Add(sourceNode.Transform);
            plan.MatchedTargets.Add(targetNode.Transform);
        }

        void RenameMatchedBones(MatchPlan plan)
        {
            for (int index = 0; index < plan.Pairs.Count; ++index)
            {
                BonePair pair = plan.Pairs[index];
                if (index == 0 && !renameCharacterRoots)
                    continue;
                if (pair.Target.name == pair.Source.name)
                    continue;

                Undo.RecordObject(pair.Target, "Rename matched target bone");
                pair.Target.name = pair.Source.name;
                EditorUtility.SetDirty(pair.Target);
                PrefabUtility.RecordPrefabInstancePropertyModifications(pair.Target);
            }
        }

        int NormalizeTargetImportSpace(MatchPlan plan)
        {
            Dictionary<Transform, Transform> canonicalWrappers = new Dictionary<Transform, Transform>();
            for (int index = 1; index < plan.Pairs.Count; ++index)
            {
                BonePair pair = plan.Pairs[index];
                if (!plan.SourceSkinBones.Contains(pair.Source) && !plan.TargetSkinBones.Contains(pair.Target))
                    canonicalWrappers[pair.Target] = pair.Source;
            }

            Dictionary<Transform, TransformSnapshot> hierarchySnapshots = new Dictionary<Transform, TransformSnapshot>();
            HashSet<UnityEngine.Object> undoTargets = new HashSet<UnityEngine.Object>();
            for (int index = 0; index < plan.TargetBoneHierarchy.Count; ++index)
            {
                Transform item = plan.TargetBoneHierarchy[index];
                hierarchySnapshots.Add(item, new TransformSnapshot(item.position, item.rotation, item.lossyScale));
                if (item != plan.TargetRoot)
                    undoTargets.Add(item);
            }

            Dictionary<Transform, TransformSnapshot> rendererSnapshots = new Dictionary<Transform, TransformSnapshot>();
            SkinnedMeshRenderer[] renderers = plan.TargetRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int index = 0; index < renderers.Length; ++index)
            {
                Transform rendererTransform = renderers[index].transform;
                if (rendererSnapshots.ContainsKey(rendererTransform))
                    continue;
                rendererSnapshots.Add(rendererTransform, new TransformSnapshot(rendererTransform.position,
                    rendererTransform.rotation, rendererTransform.lossyScale));
                undoTargets.Add(rendererTransform);
            }

            if (undoTargets.Count > 0)
            {
                UnityEngine.Object[] undoObjects = new UnityEngine.Object[undoTargets.Count];
                undoTargets.CopyTo(undoObjects);
                Undo.RecordObjects(undoObjects, "Normalize target import space");
            }

            int normalizedCount = 0;
            for (int index = 0; index < plan.TargetBoneHierarchy.Count; ++index)
            {
                Transform item = plan.TargetBoneHierarchy[index];
                if (item == plan.TargetRoot)
                    continue;

                TransformSnapshot snapshot = hierarchySnapshots[item];
                Transform canonicalWrapper;
                if (canonicalWrappers.TryGetValue(item, out canonicalWrapper))
                {
                    if (Quaternion.Angle(item.localRotation, canonicalWrapper.localRotation) > 0.001f ||
                        !Approximately(item.localScale, canonicalWrapper.localScale))
                    {
                        ++normalizedCount;
                    }
                    item.localRotation = canonicalWrapper.localRotation;
                    item.localScale = canonicalWrapper.localScale;
                }
                else
                {
                    // Bones and unmatched branches keep their visible orientation. Their local position
                    // and parent-space units are intentionally allowed to change below the new wrapper.
                    item.rotation = snapshot.WorldRotation;
                }

                item.position = snapshot.WorldPosition;
                EditorUtility.SetDirty(item);
                PrefabUtility.RecordPrefabInstancePropertyModifications(item);
            }

            foreach (KeyValuePair<Transform, TransformSnapshot> entry in rendererSnapshots)
            {
                Transform rendererTransform = entry.Key;
                TransformSnapshot snapshot = entry.Value;
                rendererTransform.position = snapshot.WorldPosition;
                rendererTransform.rotation = snapshot.WorldRotation;
                SetWorldScale(rendererTransform, snapshot.WorldScale);
                EditorUtility.SetDirty(rendererTransform);
                PrefabUtility.RecordPrefabInstancePropertyModifications(rendererTransform);
            }

            return normalizedCount;
        }

        int AlignMatchedBoneRotations(MatchPlan plan)
        {
            Dictionary<Transform, Transform> canonicalSources = new Dictionary<Transform, Transform>();
            for (int index = 1; index < plan.Pairs.Count; ++index)
            {
                BonePair pair = plan.Pairs[index];
                if (plan.SourceSkinBones.Contains(pair.Source) && plan.TargetSkinBones.Contains(pair.Target))
                    canonicalSources[pair.Target] = pair.Source;
            }

            Dictionary<Transform, TransformSnapshot> snapshots = new Dictionary<Transform, TransformSnapshot>();
            HashSet<UnityEngine.Object> undoTargets = new HashSet<UnityEngine.Object>();
            for (int index = 0; index < plan.TargetBoneHierarchy.Count; ++index)
            {
                Transform bone = plan.TargetBoneHierarchy[index];
                snapshots.Add(bone, new TransformSnapshot(bone.position, bone.rotation));
                if (bone != plan.TargetRoot)
                    undoTargets.Add(bone);
            }

            // Import wrappers can also parent the renderer. They must retain their world pose even when
            // a bone above them changes, otherwise the complete mesh object receives the axis correction.
            Dictionary<Transform, TransformSnapshot> rendererSnapshots = new Dictionary<Transform, TransformSnapshot>();
            SkinnedMeshRenderer[] renderers = plan.TargetRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int index = 0; index < renderers.Length; ++index)
            {
                Transform rendererTransform = renderers[index].transform;
                if (plan.TargetSkinBones.Contains(rendererTransform) || rendererSnapshots.ContainsKey(rendererTransform))
                    continue;
                rendererSnapshots.Add(rendererTransform,
                    new TransformSnapshot(rendererTransform.position, rendererTransform.rotation));
                undoTargets.Add(rendererTransform);
            }

            if (undoTargets.Count > 0)
            {
                UnityEngine.Object[] undoObjects = new UnityEngine.Object[undoTargets.Count];
                undoTargets.CopyTo(undoObjects);
                Undo.RecordObjects(undoObjects, "Align target bone rotations");
            }

            int alignedCount = 0;
            for (int index = 0; index < plan.TargetBoneHierarchy.Count; ++index)
            {
                Transform bone = plan.TargetBoneHierarchy[index];
                if (bone == plan.TargetRoot)
                    continue;

                // Parents are processed first. Restoring world positions compensates their new rotations,
                // so the target keeps its own proportions and anatomical pivot locations.
                TransformSnapshot snapshot = snapshots[bone];
                bone.position = snapshot.WorldPosition;

                Transform canonicalSource;
                if (canonicalSources.TryGetValue(bone, out canonicalSource))
                {
                    // Animation curves are local, so only genuine bones receive the canonical local
                    // rotation. ModelObject/Armature-style conversion wrappers are never copied.
                    if (Quaternion.Angle(bone.localRotation, canonicalSource.localRotation) > 0.001f)
                        ++alignedCount;
                    bone.localRotation = canonicalSource.localRotation;
                }
                else
                {
                    // Extra facial/accessory branches are not semantically matched. Compensate the
                    // parent's change so their visible world orientation remains untouched.
                    bone.rotation = snapshot.WorldRotation;
                }

                EditorUtility.SetDirty(bone);
                PrefabUtility.RecordPrefabInstancePropertyModifications(bone);
            }

            foreach (KeyValuePair<Transform, TransformSnapshot> entry in rendererSnapshots)
            {
                entry.Key.position = entry.Value.WorldPosition;
                entry.Key.rotation = entry.Value.WorldRotation;
                EditorUtility.SetDirty(entry.Key);
                PrefabUtility.RecordPrefabInstancePropertyModifications(entry.Key);
            }

            return alignedCount;
        }

        int RebindTargetRenderers(Transform targetRoot)
        {
            SkinnedMeshRenderer[] renderers = targetRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            int reboundCount = 0;
            for (int rendererIndex = 0; rendererIndex < renderers.Length; ++rendererIndex)
            {
                SkinnedMeshRenderer renderer = renderers[rendererIndex];
                Mesh originalMesh = renderer.sharedMesh;
                Transform[] bones = renderer.bones;
                if (originalMesh == null || bones == null || bones.Length == 0)
                {
                    Debug.LogWarning("Skipped " + renderer.name + ": it has no mesh or no bones.", renderer);
                    continue;
                }
                if (ContainsExternalBone(bones, targetRoot))
                {
                    Debug.LogWarning("Skipped " + renderer.name + ": a bone is outside the selected target.", renderer);
                    continue;
                }

                Matrix4x4[] oldBindPoses = originalMesh.bindposes;
                Matrix4x4[] bindPoses = new Matrix4x4[bones.Length];
                Matrix4x4 rendererLocalToWorld = renderer.transform.localToWorldMatrix;
                for (int boneIndex = 0; boneIndex < bones.Length; ++boneIndex)
                {
                    Transform bone = bones[boneIndex];
                    bindPoses[boneIndex] = bone != null
                        ? bone.worldToLocalMatrix * rendererLocalToWorld
                        : boneIndex < oldBindPoses.Length ? oldBindPoses[boneIndex] : Matrix4x4.identity;
                }

                Mesh reboundMesh = UnityEngine.Object.Instantiate(originalMesh);
                reboundMesh.name = originalMesh.name + "_Rebound";
                reboundMesh.bindposes = bindPoses;
                string folder = GetModifiedAssetFolder(originalMesh, targetRoot);
                string fileName = SanitizeFileName(targetRoot.name + "_" + renderer.name + "_" + reboundMesh.name) + ".asset";
                string assetPath = AssetDatabase.GenerateUniqueAssetPath(folder + "/" + fileName);
                AssetDatabase.CreateAsset(reboundMesh, assetPath);
                Undo.RegisterCreatedObjectUndo(reboundMesh, "Create rebound mesh");

                Undo.RecordObject(renderer, "Rebind skinned mesh renderer");
                renderer.bones = (Transform[])bones.Clone(); // preserve bone index -> weight index exactly
                if (renderer.rootBone == null || !IsAtOrBelow(renderer.rootBone, targetRoot))
                    renderer.rootBone = FindCommonAncestor(bones, targetRoot);
                renderer.sharedMesh = reboundMesh;
                EditorUtility.SetDirty(renderer);
                PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
                ++reboundCount;
            }
            return reboundCount;
        }

        static bool ContainsExternalBone(Transform[] bones, Transform targetRoot)
        {
            for (int index = 0; index < bones.Length; ++index)
                if (bones[index] != null && !IsAtOrBelow(bones[index], targetRoot))
                    return true;
            return false;
        }

        static bool IsAtOrBelow(Transform item, Transform root)
        {
            return item == root || (item != null && item.IsChildOf(root));
        }

        static bool Approximately(Vector3 left, Vector3 right)
        {
            return Mathf.Approximately(left.x, right.x) &&
                   Mathf.Approximately(left.y, right.y) &&
                   Mathf.Approximately(left.z, right.z);
        }

        static void SetWorldScale(Transform item, Vector3 desiredWorldScale)
        {
            if (item.parent == null)
            {
                item.localScale = desiredWorldScale;
                return;
            }

            Vector3 parentScale = item.parent.lossyScale;
            item.localScale = new Vector3(
                SafeDivide(desiredWorldScale.x, parentScale.x),
                SafeDivide(desiredWorldScale.y, parentScale.y),
                SafeDivide(desiredWorldScale.z, parentScale.z));
        }

        static float SafeDivide(float value, float divisor)
        {
            return Mathf.Abs(divisor) < 0.000001f ? value : value / divisor;
        }

        static Transform FindCommonAncestor(Transform[] bones, Transform limit)
        {
            Transform candidate = null;
            for (int index = 0; index < bones.Length && candidate == null; ++index)
                candidate = bones[index];

            while (candidate != null && IsAtOrBelow(candidate, limit))
            {
                bool containsAll = true;
                for (int index = 0; index < bones.Length; ++index)
                {
                    if (bones[index] != null && !IsAtOrBelow(bones[index], candidate))
                    {
                        containsAll = false;
                        break;
                    }
                }
                if (containsAll)
                    return candidate;
                if (candidate == limit)
                    break;
                candidate = candidate.parent;
            }
            return limit;
        }

        static string GetModifiedAssetFolder(Mesh originalMesh, Transform targetRoot)
        {
            string assetPath = AssetDatabase.GetAssetPath(originalMesh);
            if (!IsWritableProjectAsset(assetPath))
                assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(targetRoot.gameObject);
            if (!IsWritableProjectAsset(assetPath) && targetRoot.gameObject.scene.IsValid())
                assetPath = targetRoot.gameObject.scene.path;

            if (!IsWritableProjectAsset(assetPath))
            {
                Debug.LogWarning("The original mesh has no writable asset folder; the rebound copy will be saved directly in Assets.",
                    targetRoot);
                return "Assets";
            }

            string folder = Path.GetDirectoryName(assetPath);
            return string.IsNullOrEmpty(folder) ? "Assets" : folder.Replace('\\', '/');
        }

        static bool IsWritableProjectAsset(string assetPath)
        {
            return !string.IsNullOrEmpty(assetPath) &&
                   assetPath.StartsWith("Assets/", StringComparison.Ordinal);
        }

        static string SanitizeFileName(string fileName)
        {
            char[] invalid = Path.GetInvalidFileNameChars();
            for (int index = 0; index < invalid.Length; ++index)
                fileName = fileName.Replace(invalid[index], '_');
            return fileName;
        }

        void LogPlan(MatchPlan plan, bool applied, int reboundRendererCount)
        {
            int matchedCount = plan.Pairs.Count - (renameCharacterRoots ? 0 : 1);
            StringBuilder message = new StringBuilder();
            message.Append(applied ? "Skeleton matching applied. " : "Skeleton matching preview. ");
            message.Append(matchedCount).Append(" target bones matched; ")
                .Append(plan.SourceBoneCount - plan.MatchedSources.Count).Append(" source-only and ")
                .Append(plan.TargetBoneCount - plan.MatchedTargets.Count).Append(" target-only bones left untouched.");
            if (applied)
                message.Append(" Rebound renderers: ").Append(reboundRendererCount).Append('.');
            if (plan.SourceExternalBoneCount > 0 || plan.TargetExternalBoneCount > 0)
                message.Append(" External bone references ignored: source ").Append(plan.SourceExternalBoneCount)
                    .Append(", target ").Append(plan.TargetExternalBoneCount).Append('.');

            if (logDetailedReport)
            {
                message.AppendLine().AppendLine("Matches:");
                for (int index = 0; index < plan.Pairs.Count; ++index)
                {
                    if (index == 0 && !renameCharacterRoots)
                        continue;
                    BonePair pair = plan.Pairs[index];
                    message.Append("  ").Append(pair.TargetPath).Append("  <-  ").Append(pair.SourcePath)
                        .Append("  [").Append(pair.Reason).Append(", ")
                        .Append(Mathf.RoundToInt(pair.Confidence * 100f)).AppendLine("%]");
                }
            }
            Debug.Log(message.ToString(), this);
        }

        static string GetRelativePath(Transform root, Transform item)
        {
            if (item == root)
                return root.name;
            Stack<string> names = new Stack<string>();
            Transform current = item;
            while (current != null && current != root)
            {
                names.Push(current.name);
                current = current.parent;
            }
            StringBuilder path = new StringBuilder(root.name);
            while (names.Count > 0)
                path.Append('/').Append(names.Pop());
            return path.ToString();
        }

        static int GetDepthFromRoot(Transform item, Transform root)
        {
            int depth = 0;
            while (item != null && item != root)
            {
                ++depth;
                item = item.parent;
            }
            return depth;
        }

        //----------------------------------------------------------------------------------------------------------

        sealed class TopologyNode
        {
            internal readonly Transform Transform;
            internal readonly List<TopologyNode> Children = new List<TopologyNode>();
            internal int Height;
            internal int NodeCount;
            internal int LeafCount;
            internal ulong ShapeHash;
            internal bool IsLeaf { get { return Children.Count == 0; } }

            internal TopologyNode(Transform transform) { Transform = transform; }

            internal void Finish()
            {
                Height = 0;
                NodeCount = 1;
                LeafCount = Children.Count == 0 ? 1 : 0;
                const ulong offset = 1469598103934665603UL;
                const ulong prime = 1099511628211UL;
                ulong hash = (offset ^ (ulong)Children.Count) * prime;
                for (int index = 0; index < Children.Count; ++index)
                {
                    TopologyNode child = Children[index];
                    Height = Mathf.Max(Height, child.Height + 1);
                    NodeCount += child.NodeCount;
                    LeafCount += child.LeafCount;
                    hash = (hash ^ child.ShapeHash) * prime;
                    hash = (hash ^ 255UL) * prime;
                }
                ShapeHash = hash;
            }
        }

        sealed class MatchPlan
        {
            internal readonly Transform SourceRoot;
            internal readonly Transform TargetRoot;
            internal readonly int SourceBoneCount;
            internal readonly int TargetBoneCount;
            internal readonly int SourceExternalBoneCount;
            internal readonly int TargetExternalBoneCount;
            internal readonly List<Transform> TargetBoneHierarchy;
            internal readonly HashSet<Transform> SourceSkinBones;
            internal readonly HashSet<Transform> TargetSkinBones;
            internal readonly List<BonePair> Pairs = new List<BonePair>();
            internal readonly HashSet<Transform> MatchedSources = new HashSet<Transform>();
            internal readonly HashSet<Transform> MatchedTargets = new HashSet<Transform>();

            internal MatchPlan(Transform sourceRoot, Transform targetRoot, int sourceBoneCount, int targetBoneCount,
                HashSet<Transform> targetBoneHierarchy, HashSet<Transform> sourceSkinBones,
                HashSet<Transform> targetSkinBones, int sourceExternalBoneCount, int targetExternalBoneCount)
            {
                SourceRoot = sourceRoot;
                TargetRoot = targetRoot;
                SourceBoneCount = sourceBoneCount;
                TargetBoneCount = targetBoneCount;
                SourceExternalBoneCount = sourceExternalBoneCount;
                TargetExternalBoneCount = targetExternalBoneCount;
                SourceSkinBones = new HashSet<Transform>(sourceSkinBones);
                TargetSkinBones = new HashSet<Transform>(targetSkinBones);
                TargetBoneHierarchy = new List<Transform>(targetBoneHierarchy);
                TargetBoneHierarchy.Sort(delegate(Transform left, Transform right)
                {
                    return GetDepthFromRoot(left, targetRoot).CompareTo(GetDepthFromRoot(right, targetRoot));
                });
            }
        }

        struct TransformSnapshot
        {
            internal readonly Vector3 WorldPosition;
            internal readonly Quaternion WorldRotation;
            internal readonly Vector3 WorldScale;

            internal TransformSnapshot(Vector3 worldPosition, Quaternion worldRotation)
                : this(worldPosition, worldRotation, Vector3.one)
            {
            }

            internal TransformSnapshot(Vector3 worldPosition, Quaternion worldRotation, Vector3 worldScale)
            {
                WorldPosition = worldPosition;
                WorldRotation = worldRotation;
                WorldScale = worldScale;
            }
        }

        struct BonePair
        {
            internal readonly Transform Source;
            internal readonly Transform Target;
            internal readonly float Confidence;
            internal readonly string Reason;
            internal readonly string SourcePath;
            internal readonly string TargetPath;

            internal BonePair(Transform source, Transform target, float confidence, string reason,
                string sourcePath, string targetPath)
            {
                Source = source;
                Target = target;
                Confidence = confidence;
                Reason = reason;
                SourcePath = sourcePath;
                TargetPath = targetPath;
            }
        }

        struct PendingPair
        {
            internal readonly TopologyNode Source;
            internal readonly TopologyNode Target;
            internal readonly float Confidence;
            internal readonly string Reason;

            internal PendingPair(TopologyNode source, TopologyNode target, float confidence, string reason)
            {
                Source = source;
                Target = target;
                Confidence = confidence;
                Reason = reason;
            }
        }

        struct BestCandidate
        {
            internal readonly int Index;
            internal readonly float Score;
            internal readonly float Margin;

            internal BestCandidate(int index, float score, float margin)
            {
                Index = index;
                Score = score;
                Margin = margin;
            }
        }
    }

    [CustomEditor(typeof(HierarchyRenamer))]
    sealed class HierarchyRenamerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Preview first. Only bones used by SkinnedMeshRenderers (and their ancestors) are considered. " +
                "Put both characters in their intended reference pose: matched source local rotations are copied, " +
                "target joint positions are preserved, then the target meshes are rebound.",
                MessageType.Info);

            HierarchyRenamer renamer = (HierarchyRenamer)target;
            if (GUILayout.Button("Preview Bone Matching"))
                renamer.Preview();
            if (GUILayout.Button("Rename, Align Rotations And Rebind"))
                renamer.Operate();
        }
    }
}
#endif
