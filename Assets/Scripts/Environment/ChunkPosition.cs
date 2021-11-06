using System;
using Blox.ConfigurationNS;
using UnityEngine;

namespace Blox.EnvironmentNS
{
    /// <summary>
    /// This struct defines the position of a chunk in the world.
    /// </summary>
    public readonly struct ChunkPosition
    {
        /// <summary>
        /// Default chunk position with X = 0 and Z = 0.
        /// </summary>
        public static readonly ChunkPosition Zero = new ChunkPosition(0, 0);

        /// <summary>
        /// Calculates the position of a chunk containing the block given by its global coordinates.
        /// </summary>
        /// <param name="size">The size of a chunk</param>
        /// <param name="globalPosition">The global position of a block</param>
        /// <returns>The chunks position</returns>
        public static ChunkPosition FromGlobalPosition(ChunkSize size, Vector3Int globalPosition)
        {
            // Calculation of the X coordinate
            var gx = globalPosition.x;
            var cx = gx >= 0 ? gx / size.Width : (gx - size.Width + 1) / size.Width;

            // Calculation of the Z coordinate
            var gz = globalPosition.z;
            var cz = gz >= 0 ? gz / size.Width : (gz - size.Width + 1) / size.Width;

            return new ChunkPosition(cx, cz);
        }
        
        /// <summary>
        /// Iterates over Z and X coordinates in an interval from Z - offset to Z + offset and X - offset to X + offset.
        /// </summary>
        /// <param name="chunkPosition">A chunks position</param>
        /// <param name="offset">An offset value</param>
        /// <param name="action">An action method to be invoked for every iteration.</param>
        public static void Iterate(ChunkPosition chunkPosition, int offset, Action<ChunkPosition> action)
        {
            var cx = chunkPosition.X;
            var cz = chunkPosition.Z;

            for (var z = cz - offset; z <= cz + offset; z++)
            {
                for (var x = cx - offset; x <= cx + offset; x++)
                {
                    action.Invoke(new ChunkPosition(x, z));
                }
            }
        }

        /// <summary>
        /// Returns a key for the given chunks position coordinates. This key is used by chunk managers cache to
        /// identify a chunk data container.
        /// </summary>
        /// <param name="x">The X coordinate of a chunk position</param>
        /// <param name="z">The Z coordinate of a chunk position</param>
        /// <returns>A string containing the chunh position</returns>
        public static string ToCacheKey(int x, int z)
        {
            return "[" + x + ":" + z + "]";
        }

        /// <summary>
        /// The X coordinate of the chunks position.
        /// </summary>
        public readonly int X;

        /// <summary>
        /// The Z coordinate of the chunks position.
        /// </summary>
        public readonly int Z;

        /// <summary>
        /// The left chunk position.
        /// </summary>
        public ChunkPosition Left => new ChunkPosition(X - 1, Z);
        
        /// <summary>
        /// The right chunk position.
        /// </summary>
        public ChunkPosition Right => new ChunkPosition(X + 1, Z);

        /// <summary>
        /// The front chunk position.
        /// </summary>
        public ChunkPosition Front => new ChunkPosition(X, Z - 1);

        /// <summary>
        /// The back chunk position.
        /// </summary>
        public ChunkPosition Back => new ChunkPosition(X, Z + 1);

        /// <summary>
        /// Creates a new chunk position with the given coordinates.
        /// </summary>
        /// <param name="x">The X coordinate of the chunks position.</param>
        /// <param name="z">The Z coordinate of the chunks position.</param>
        public ChunkPosition(int x, int z)
        {
            X = x;
            Z = z;
        }

        /// <summary>
        /// Returns a global position vector defined by this chunk position and the local coordinates of a block within
        /// a chunk.
        /// </summary>
        /// <param name="size">The size of a chunk</param>
        /// <param name="x">The local X coordinate</param>
        /// <param name="y">The local Y coordinate</param>
        /// <param name="z">The local Z coordinate</param>
        /// <returns>The global position vector.</returns>
        public Vector3Int ToGlobalPosition(ChunkSize size, int x, int y, int z)
        {
            var gx = X * size.Width + x;
            var gz = Z * size.Width + z;
            return new Vector3Int(gx, y, gz);
        }

        /// <summary>
        /// Returns a global position vector defined by this chunk position and a local position vector of a block
        /// within a chunk.
        /// </summary>
        /// <param name="size">The size of a chunk</param>
        /// <param name="localPosition">A local position vector</param>
        /// <returns>The global position vector.</returns>
        public Vector3Int ToGlobalPosition(ChunkSize size, Vector3Int localPosition)
        {
            return ToGlobalPosition(size, localPosition.x, localPosition.y, localPosition.z);
        }

        /// <summary>
        /// Returns a local position vector defined by this chunk position and the global coordinates.
        /// </summary>
        /// <param name="size">The size of a chunk</param>
        /// <param name="x">The global X coordinate</param>
        /// <param name="y">The global Y coordinate</param>
        /// <param name="z">The global Z coordinate</param>
        /// <returns>A local position vector</returns>
        public Vector3Int ToLocalPosition(ChunkSize size, int x, int y, int z)
        {
            var lx = x - size.Width * X;
            var lz = z - size.Width * Z;
            return new Vector3Int(lx, y, lz);
        }

        /// <summary>
        /// Returns a local position vector defined by this chunk position and the global position vector.
        /// </summary>
        /// <param name="size">The size of a chunk</param>
        /// <param name="globalPosition">The global position vector</param>
        /// <returns>A local position vector</returns>
        public Vector3Int ToLocalPosition(ChunkSize size, Vector3Int globalPosition)
        {
            return ToLocalPosition(size, globalPosition.x, globalPosition.y, globalPosition.z);
        }
        
        /// <summary>
        /// Returns this chunk data containers cache key. Such a key is used by chunk managers cache to
        /// identify a chunk data container.
        /// </summary>
        /// <returns>Cache key</returns>
        public string ToCacheKey()
        {
            return ToCacheKey(X, Z);
        }

        /// <summary>
        /// Returns the absolute file path of the persisted chunk data container for this position.
        /// </summary>
        /// <returns>File path</returns>
        public string ToCacheFilename()
        {
            return "chunk_" + X + "_" + Z + ".dat";
        }

        /// <summary>
        /// Compares this chunk position with another object.
        /// </summary>
        /// <param name="obj">An object</param>
        /// <returns>True, if the other object is also a chunk position and has the same coordinates, otherwise false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is ChunkPosition cp)
                return X == cp.X && Z == cp.Z;

            return false;
        }

        /// <summary>
        /// Returns a hash code for this chunk position.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return X + Z * 65536;
        }

        /// <summary>
        /// Returns a string representation of the chunk position.
        /// </summary>
        /// <returns>A string containing chunk position information.</returns>
        public override string ToString()
        {
            return "ChunkPosition[X=" + X + "; Z=" + Z + "]";
        }
    }
}