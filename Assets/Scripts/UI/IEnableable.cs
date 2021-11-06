namespace Blox.UINS
{
    /// <summary>
    /// This interface can be implemented by classes that can be enabled or disabled.
    /// </summary>
    public interface IEnableable
    {
        /// <summary>
        /// Enables or disables the class.
        /// </summary>
        /// <param name="enabled">True when enabled or false when disabled</param>
        public void SetEnabled(bool enabled);
    }
}