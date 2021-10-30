using System;
using System.Collections.Generic;
using Blox.Environment.Config;
using UnityEngine;

namespace Blox.Environment
{
    public class ChunkMesh
    {
        public static readonly Vector3[,] Vertices =
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

        public static readonly Vector2[] UV =
            { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };

        public static readonly int[] Triangles = { 0, 1, 2, 0, 2, 3 };

        [Flags]
        public enum Flags
        {
            None = 0,
            CastShadow = 1,
            CreateCollider = 2
        }

        public readonly List<Vector3> vertices;
        public readonly List<int> triangles;
        public readonly List<Vector2> uv;
        public readonly Flags flags;
        public readonly LayerMask layer;

        public string name => m_TextureType.name;
        public Material material => m_TextureType.material;

        private readonly TextureType m_TextureType;

        public ChunkMesh(TextureType textureType, Flags flags, LayerMask layer)
        {
            m_TextureType = textureType;
            this.flags = flags;
            this.layer = layer;
            vertices = new List<Vector3>();
            triangles = new List<int>();
            uv = new List<Vector2>();
        }
    }
}