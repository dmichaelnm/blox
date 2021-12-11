using UnityEngine;

namespace Blox.TerrainNS
{
    public class Model : MonoBehaviour
    {
        public void Highlight(bool enabled)
        {
            var outline = GetComponent<Outline>();
            outline.enabled = enabled;
        }
    }
}