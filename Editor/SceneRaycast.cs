using System;
using UnityEditor;
using UnityEngine;

namespace com.aoyon.triangleselector
{

    public class SceneRaycastUtility
    {
        private MeshCollider selectedMeshCollider;
        private MeshCollider ubselectedMeshCollider;

        private RaycastHit[] hits = new RaycastHit[20];

        public bool TryRaycast(out RaycastHit hitInfo)
        {
            hitInfo = new RaycastHit();

            if (Event.current == null)
            {
                return false;
            }

            HandleUtility.PickGameObject(Event.current.mousePosition, false);

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            
            int hitCount = Physics.RaycastNonAlloc(ray, hits, Mathf.Infinity);
            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = hits[i];
                if (hit.collider == selectedMeshCollider || hit.collider == ubselectedMeshCollider)
                {
                    hitInfo = hit;
                    return true;
                }
            }

            return false;
        }

        public bool IsSelected(RaycastHit hitInfo)
        {
            MeshCollider hitcolider = hitInfo.collider as MeshCollider;
            if (hitcolider == selectedMeshCollider)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public void AddCollider(GameObject selectedObject, GameObject unselectedObject)
        {   
            selectedMeshCollider = AddMeshCollider(selectedObject);
            ubselectedMeshCollider = AddMeshCollider(unselectedObject);

            return;
            
            MeshCollider AddMeshCollider(GameObject obj)
            {
                MeshCollider meshCollider = obj.GetComponent<MeshCollider>();
                if (meshCollider == null)
                {
                    meshCollider = obj.AddComponent<MeshCollider>();
                    meshCollider.convex = false;
                }
                return meshCollider;
            }
            
        }

        public void UpdateColider(Mesh mesh, bool IsSelected)
        {
            if (IsSelected)
            {
                selectedMeshCollider.sharedMesh = mesh;
            }
            else
            {
                ubselectedMeshCollider.sharedMesh = mesh;
            }
        }

    }
}