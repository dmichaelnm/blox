using System.IO;
using UnityEngine;

namespace Blox.GameNS
{
    public static class Game
    {
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

        public static string CurrentName;
    }
}