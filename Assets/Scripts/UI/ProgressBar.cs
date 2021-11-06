using UnityEngine;
using UnityEngine.UI;

namespace Blox.UINS
{
    /// <summary>
    /// This component controls the progress bar UI.
    /// </summary>
    public class ProgressBar : MonoBehaviour
    {
        /// <summary>
        /// The image component of the progress bar.
        /// </summary>
        private Image m_ProgressImage;

        /// <summary>
        /// Shows or hide the progress bar.
        /// </summary>
        /// <param name="enabled">True to show the progress or false to hide the progress bar</param>
        public void SetProgressBarEnabled(bool enabled)
        {
            m_ProgressImage.enabled = enabled;
            SetProgressValue(0f);
        }

        /// <summary>
        /// Set the current and max value to calculate the progress.
        /// </summary>
        /// <param name="current">The current value</param>
        /// <param name="max">The maximum value</param>
        public void SetProgressValue(float current, float max)
        {
            SetProgressValue(current / max);    
        }
        
        /// <summary>
        /// The the percentage progress.
        /// </summary>
        /// <param name="percent">A value between 0 and 1.</param>
        public void SetProgressValue(float percent)
        {
            var material = m_ProgressImage.material;
            material.SetFloat("_Progress", percent);
        }
        
        /// <summary>
        /// This method is called when this component is created.
        /// </summary>
        private void Awake()
        {
            m_ProgressImage = GetComponent<Image>();
            SetProgressBarEnabled(false);
        }
    }
}