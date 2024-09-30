using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace com.aoyon.triangleselector.utils
{

    public class TraceObjects
    {

        public static GameObject TraceCopiedObject(GameObject root, GameObject new_root, GameObject targetobj)
        {
            IEnumerable<GameObject> copiedObjects = TraceCopiedObjects(root, new_root, new GameObject[]{ targetobj });
            return copiedObjects.First();
        }

        public static IEnumerable<GameObject> TraceCopiedObjects(GameObject root, GameObject new_root, IEnumerable<GameObject> targetobjs)
        {
            var allChildrenRoot = GetAllChildren(root);
            var allChildrenNewRoot = GetAllChildren(new_root);

            return targetobjs.Select(obj => Array.IndexOf(allChildrenRoot, obj.transform))
                       .Where(index => index != -1 && index < allChildrenNewRoot.Length)
                       .Select(index => allChildrenNewRoot[index].gameObject);
        }

        public static SkinnedMeshRenderer TraceCopiedRenderer(GameObject root, GameObject new_root, SkinnedMeshRenderer targetRenderer)
        {
            IEnumerable<SkinnedMeshRenderer> copiedRenderers = TraceCopiedRenderers(root, new_root, new SkinnedMeshRenderer[]{ targetRenderer });
            return copiedRenderers.First();
        }

        public static IEnumerable<SkinnedMeshRenderer> TraceCopiedRenderers(GameObject root, GameObject new_root, IEnumerable<SkinnedMeshRenderer> targetRenderers)
        {
            var allChildrenRoot = GetAllChildren(root);
            var allChildrenNewRoot = GetAllChildren(new_root);

            return targetRenderers.Select(renderer => Array.IndexOf(allChildrenRoot, renderer.transform))
                       .Where(index => index != -1 && index < allChildrenNewRoot.Length)
                       .Select(index => allChildrenNewRoot[index].GetComponent<SkinnedMeshRenderer>());
        }

        public static List<GameObject> GetChildren(GameObject parent)
        {
            List<GameObject> children = new List<GameObject>();
            foreach (Transform child in parent.transform)
            {
                children.Add(child.gameObject);
            }
            return children;
        }

        public static Transform[] GetAllChildren(GameObject parent)
        {
            Transform[] children = parent.GetComponentsInChildren<Transform>(true);
            return children;
        }

        public static GameObject GetCommonRoot(IEnumerable<GameObject> objs)
        {
            GameObject commonRoot = null;
            foreach (var obj in objs)
            {
                var root = GetRoot(obj);
                if (commonRoot != null && root != commonRoot)
                {
                    throw new InvalidOperationException("Please select the objects that have a common parent");
                }
                else
                {
                    commonRoot = root;
                }
            }
            
            return commonRoot;
        }

        public static GameObject GetRoot(GameObject obj)
        {
            GameObject root;
            if (PrefabUtility.IsPartOfPrefabInstance(obj))
            {
                root = PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
            }
            else
            {
                Transform parent = obj.transform.parent;
                if (parent == null)
                {
                    throw new InvalidOperationException("Please select the object that has a parent");
                }
                root = parent.gameObject;
            }
            return root;
        }

        public static HashSet<GameObject> TraceSkinnedMeshRenderer(
            SkinnedMeshRenderer skinnedMeshRenderer,
            HashSet<GameObject> gameobjectsToSave,
            HashSet<object> componentsToSave)
        {
            return TraceSkinnedMeshRenderers(
                new SkinnedMeshRenderer[] { skinnedMeshRenderer },
                gameobjectsToSave,
                componentsToSave
                );
        }

        public static HashSet<GameObject> TraceSkinnedMeshRenderers(
            IEnumerable<SkinnedMeshRenderer> skinnedMeshRenderers,
            HashSet<GameObject> gameobjectsToSave,
            HashSet<object> componentsToSave)
        {
            foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
            {
                gameobjectsToSave.Add(skinnedMeshRenderer.gameObject);
                componentsToSave.Add(skinnedMeshRenderer);

                // SkinnedMeshRendererのrootBoneとanchor overrideに設定されているオブジェクトを追加
                Transform rootBone = skinnedMeshRenderer.rootBone;
                Transform anchor = skinnedMeshRenderer.probeAnchor;
                if (rootBone) gameobjectsToSave.Add(rootBone.gameObject);
                if (anchor) gameobjectsToSave.Add(anchor.gameObject);
            }

            // ウェイトをつけているオブジェクトを追加
            HashSet<GameObject> weightedBones = GetWeightedBones(skinnedMeshRenderers);
            gameobjectsToSave.UnionWith(weightedBones);
            return weightedBones;
        }

        public static HashSet<GameObject> GetWeightedBones(IEnumerable<SkinnedMeshRenderer> skinnedMeshRenderers)
        {
            HashSet<GameObject> weightedBones = new HashSet<GameObject>();
            foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
            {
                BoneWeight[] boneWeights = skinnedMeshRenderer.sharedMesh.boneWeights;
                Transform[] bones = skinnedMeshRenderer.bones;
                int missingBoneCount = 0;

                foreach (BoneWeight boneWeight in boneWeights)
                {
                    if (boneWeight.weight0 > 0)
                    {
                        Transform boneTransform = bones[boneWeight.boneIndex0];
                        if (boneTransform == null) 
                            missingBoneCount++;
                        else
                            weightedBones.Add(boneTransform.gameObject);
                    }

                    if (boneWeight.weight1 > 0)
                    {
                        Transform boneTransform = bones[boneWeight.boneIndex1];
                        if (boneTransform == null) 
                            missingBoneCount++;
                        else
                            weightedBones.Add(boneTransform.gameObject);
                    }

                    if (boneWeight.weight2 > 0)
                    {
                        Transform boneTransform = bones[boneWeight.boneIndex2];
                        if (boneTransform == null) 
                            missingBoneCount++;
                        else
                            weightedBones.Add(boneTransform.gameObject);
                    }

                    if (boneWeight.weight3 > 0)
                    {
                        Transform boneTransform = bones[boneWeight.boneIndex3];
                        if (boneTransform == null) 
                            missingBoneCount++;
                        else
                            weightedBones.Add(boneTransform.gameObject);
                    }
                }

                if (missingBoneCount > 0)
                {
                    throw new InvalidOperationException($"Some bones weighting {skinnedMeshRenderer.name} could not be found. Total missing bones: {missingBoneCount}");
                }
                else
                {
                    Debug.Log($"Bones weighting {skinnedMeshRenderer.name}: {weightedBones.Count}/{skinnedMeshRenderer.bones.Length}");
                }
            }

            return weightedBones;
        }
    }
}