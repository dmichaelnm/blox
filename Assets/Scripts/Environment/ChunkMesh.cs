using System;
using System.Collections.Generic;
using Blox.ConfigurationNS;
using Blox.UtilitiesNS;
using UnityEngine;
using UnityEngine.Rendering;

namespace Blox.EnvironmentNS
{
    /// <summary>
    /// Contains the data for the chunk mesh.
    /// </summary>
    public class ChunkMesh
    {
        /// <summary>
        /// This enum defines flags used when creating the mesh components of the chunk object.
        /// </summary>
        [Flags]
        public enum Flags
        {
            /// <summary>
            /// No flags are set.
            /// </summary>
            None,

            /// <summary>
            /// If this flag is set, the mesh renderer will cast shadows.
            /// </summary>
            CastShadows,

            /// <summary>
            /// IF this flag ist set, a mesh collider will be created.
            /// </summary>
            CreateCollider
        }

        /// <summary>
        /// Definition of the vertices for each face.
        /// </summary>
        public static readonly Vector3[,] FaceVertices =
        {
            // top face
            { new Vector3(0, 1, 0), new Vector3(0, 1, 1), new Vector3(1, 1, 1), new Vector3(1, 1, 0) },
            // bottom face
            { new Vector3(0, 0, 1), new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 1) },
            // front face
            { new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 0, 0) },
            // back face
            { new Vector3(1, 0, 1), new Vector3(1, 1, 1), new Vector3(0, 1, 1), new Vector3(0, 0, 1) },
            // left face
            { new Vector3(0, 0, 1), new Vector3(0, 1, 1), new Vector3(0, 1, 0), new Vector3(0, 0, 0) },
            // right face
            { new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 1), new Vector3(1, 0, 1) }
        };

        /// <summary>
        /// Default UV coordinates.
        /// </summary>
        public static readonly Vector2[] DefaultUV =
            { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };

        /// <summary>
        /// The offsets for the triangles.
        /// </summary>
        public static readonly int[] TriangleOffsets = { 0, 1, 2, 0, 2, 3 };

        /// <summary>
        /// The list containing all vertices of the mesh.
        /// </summary>
        public readonly List<Vector3> Vertices;

        /// <summary>
        /// The list containing all triangle indicies of the mesh.
        /// </summary>
        public readonly List<int> Triangles;

        /// <summary>
        /// The list containing all UV coordinates of the mesh.
        /// </summary>
        public readonly List<Vector2> UV;

        /// <summary>
        /// The flags set for this chunk mesh.
        /// </summary>
        private readonly Flags m_Flags;

        /// <summary>
        /// The texture type this mesh belongs to.
        /// </summary>
        private readonly TextureType m_TextureType;

        /// <summary>
        /// The layer mask of the chunk object.
        /// </summary>
        private readonly LayerMask m_LayerMask;

        /// <summary>
        /// Creates a new chunk mesh for a specific texture type and a set of flags.
        /// </summary>
        /// <param name="textureType">The texture type</param>
        /// <param name="flags">The flags</param>
        /// <param name="layerMask">The layer mask of the chunk object</param>
        public ChunkMesh(TextureType textureType, Flags flags, LayerMask layerMask)
        {
            m_TextureType = textureType;
            m_Flags = flags;
            m_LayerMask = layerMask;
            Vertices = new List<Vector3>();
            Triangles = new List<int>();
            UV = new List<Vector2>();
        }

        /// <summary>
        /// Creates or updates the chunk object for this mesh data container. If a new object must be created, it will
        /// be a child of the given parent.
        /// </summary>
        /// <param name="parent">Parent game object</param>
        public void Create(GameObject parent)
        {
            var child = parent.GetChild(m_TextureType.Name, true);
            child.transform.localPosition = Vector3.zero;
            child.layer = m_LayerMask;

            var mesh = new Mesh();
            mesh.name = m_TextureType.Name + " Mesh";
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.vertices = Vertices.ToArray();
            mesh.triangles = Triangles.ToArray();
            mesh.uv = UV.ToArray();
            mesh.RecalculateNormals();

            var meshFilter = child.GetOrAddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            var meshRenderer = child.GetOrAddComponent<MeshRenderer>();
            meshRenderer.material = m_TextureType.Material;
            meshRenderer.shadowCastingMode =
                (m_Flags & Flags.CastShadows) != 0 ? ShadowCastingMode.On : ShadowCastingMode.Off;

            if ((m_Flags & Flags.CreateCollider) != 0)
            {
                var meshCollider = child.GetOrAddComponent<MeshCollider>();
                meshCollider.sharedMesh = mesh;
            }
        }
    }
}