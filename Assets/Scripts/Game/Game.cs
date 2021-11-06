using System.IO;
using UnityEngine;

namespace Blox.GameNS
{
    /// <summary>
    /// Central game class that holds the current game state information.
    /// </summary>
    public static class Game
    {
        /// <summary>
        /// The path to the temporary directory of the game.
        /// </summary>
        public static string TemporaryDirectory
        {
            get
            {
                var path = Application.persistentDataPath + "/temp";
                // Create the directory if it not exists yet
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                
                return path;
            }
        }
    }
}