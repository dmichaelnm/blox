using System;
using System.Collections.Generic;
using Blox.CommonNS;
using Blox.ConfigurationNS;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = System.Diagnostics.Debug;

namespace Blox.TerrainNS
{
    public class ChunkMesh
    {
        [Flags]
        public enum Flags
        {
            None = 0,
            CastShadow = 1,
            CreateCollider = 2,
            UseGlobalUV = 4
        }

        public readonly List<Vector3> vertices;
        public readonly List<int> triangles;
        public readonly List<Vector2> uv;

        private readonly TextureType m_TextureType;
        private readonly Flags m_Flags;

        public ChunkMesh([NotNull] TextureType textureType, Flags flags)
        {
            m_TextureType = textureType;
            m_Flags = flags;

            vertices = new List<Vector3>();
            triangles = new List<int>();
            uv = new List<Vector2>();
        }

        public void Apply([NotNull] GameObject parent)
        {
            var meshObj = parent.GetChild(m_TextureType.name, true);
            Debug.Assert(meshObj != null, nameof(meshObj) + " != null");
            meshObj.layer = LayerMask.NameToLayer("Terrain");

            var mesh = new Mesh();
            mesh.name = $"{m_TextureType.name} Mesh";
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uv.ToArray();
            mesh.RecalculateNormals();

            var meshFilter = meshObj.GetOrAddComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            var meshRenderer = meshObj.GetOrAddComponent<MeshRenderer>();
            meshRenderer.material = m_TextureType.material;
            meshRenderer.shadowCastingMode =
                (m_Flags & Flags.CastShadow) != 0 ? ShadowCastingMode.On : ShadowCastingMode.Off;

            if ((m_Flags & Flags.CreateCollider) != 0)
            {
                var meshCollider = meshObj.GetOrAddComponent<MeshCollider>();
                meshCollider.sharedMesh = mesh;
            }
        }
    }
}