using UnityEngine;

namespace Blox.UINS
{
    /// <summary>
    /// This interface can be implemented by classes that provides a color.
    /// </summary>
    public interface IColorProvider
    {
        /// <summary>
        /// Returns the color.
        /// </summary>
        /// <returns></returns>
        public Color GetColor();

        /// <summary>
        /// Sets the color.
        /// </summary>
        /// <param name="color">A color</param>
        public void SetColor(Color color);
    }
}