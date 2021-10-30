using System.Collections.Generic;
using Blox.Environment.Jobs;
using Blox.Utility;
using UnityEngine;
using UnityEngine.Rendering;

namespace Blox.Environment
{
    public class Chunk : MonoBehaviour
    {
        public ChunkPosition chunkPosition;
        
        public void CreateMeshes(IList<ChunkMesh> meshes)
        {
            foreach (var meshEnty in meshes)
            {
                var obj = gameObject.GetChildObject(meshEnty.name, true);
                obj.transform.localPosition = Vector3.zero;
                obj.layer = meshEnty.layer;

                var mesh = new Mesh();
                mesh.name = meshEnty.name + " Mesh";
                mesh.indexFormat = IndexFormat.UInt32;
                mesh.vertices = meshEnty.vertices.ToArray();
                mesh.triangles = meshEnty.triangles.ToArray();
                mesh.uv = meshEnty.uv.ToArray();
                mesh.RecalculateNormals();

                var meshFilter = obj.GetOrAddComponent<MeshFilter>();
                meshFilter.sharedMesh = mesh;

                var meshRenderer = obj.GetOrAddComponent<MeshRenderer>();
                meshRenderer.material = meshEnty.material;
                meshRenderer.shadowCastingMode = (meshEnty.flags & ChunkMesh.Flags.CastShadow) != 0
                    ? ShadowCastingMode.On
                    : ShadowCastingMode.Off;

                if ((meshEnty.flags & ChunkMesh.Flags.CreateCollider) != 0)
                {
                    var meshCollider = obj.GetOrAddComponent<MeshCollider>();
                    meshCollider.sharedMesh = mesh;
                }
            }
        }

        public void RemoveChilren()
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                var obj = transform.GetChild(i).gameObject;
                Destroy(obj);       
            }
        }
    }
}