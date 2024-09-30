using System.Collections.Generic;
using UnityEngine;

namespace com.aoyon.triangleselector.utils
{
    public class CleanUpHierarchy
    {
        public static void CheckAndDeleteRecursive(GameObject obj, HashSet<GameObject> objectsToSave, HashSet<object> componentsToSave)
        {   
            List<GameObject> children = TraceObjects.GetChildren(obj);

            // 子オブジェクトに対して再帰的に処理を適用
            foreach (GameObject child in children)
            {   
                CheckAndDeleteRecursive(child, objectsToSave, componentsToSave);
            }

            // 削除しない条件
            if (objectsToSave.Contains(obj) || obj.transform.childCount != 0)
            {
                ActivateObject(obj);
                RemoveComponents(obj, componentsToSave);
                return;
            }
            
            Object.DestroyImmediate(obj, true);
        }

        private static void ActivateObject(GameObject obj)
        {
            obj.SetActive(true);
            obj.tag = "Untagged"; 
        }

        private static void RemoveComponents(GameObject targetGameObject, HashSet<object> componentsToSave)
        {
            var components = targetGameObject.GetComponents<Component>();
            foreach (var component in components)
            {
                if (!(component is Transform) && !componentsToSave.Contains(component))
                {
                    Object.DestroyImmediate(component, true);
                }
            }
        }

    }
}