using System.Collections.Generic;
using System.IO;
using Blox.Utility;
using Newtonsoft.Json;
using UnityEngine;

namespace Blox.MainMenu
{
    public class PresetManager
    {
        private static PresetManager _instance;

        public readonly List<Preset> presets;
        public  Preset selectedPreset { get; private set; }
        
        public static PresetManager GetInstance()
        {
            return _instance ??= new PresetManager();
        }

        private PresetManager()
        {
            var startTime = Time.realtimeSinceStartup;
            
            presets = new List<Preset>();
            
            var asset = Resources.Load<TextAsset>("generator-presets");
            using (var reader = new JsonTextReader(new StringReader(asset.text)))
            {
                reader.NextTokenIsStartObject();
                reader.NextPropertyNameIs("presets");
                reader.NextTokenIsStartArray();
                while (!reader.NextTokenIsEndArray())
                {
                    reader.CurrentTokenIsStartObject();
                    var preset = new Preset(reader);                   
                    presets.Add(preset);
                    reader.NextTokenIsEndObject();
                }
            }
            
            var time = (Time.realtimeSinceStartup - startTime) * 1000f;
            Debug.Log("Loading presets (" + time + "ms)");            
        }

        public void SelectPreset(string name, int randomSeed)
        {
            selectedPreset = presets.Find(item => item.name.Equals(name));
            if (selectedPreset != null)
                selectedPreset.randomSeed = randomSeed;
        }
    }
}