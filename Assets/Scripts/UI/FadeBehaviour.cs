using UnityEngine;

namespace Blox.UINS
{
    /// <summary>
    /// A component that can be faded in and faded out.
    /// </summary>
    public abstract class FadeBehaviour : MonoBehaviour
    {
        /// <summary>
        /// This enumeration contains all fading states.
        /// </summary>
        protected enum State
        {
            /// <summary>
            /// No fade.
            /// </summary>
            None,
            
            /// <summary>
            /// Fading in.
            /// </summary>
            FadeIn,
            
            /// <summary>
            /// Fading out.
            /// </summary>
            FadeOut
        } 
        
        /// <summary>
        /// The time the loading screen takes to fade out.
        /// </summary>
        public float FadeTime = 1f;

        /// <summary>
        /// The internal fade state. 
        /// </summary>
        private State m_State;

        /// <summary>
        /// Internal fade timer.
        /// </summary>
        private float m_FadeTimer;

        /// <summary>
        /// Starts to fade in.
        /// </summary>
        public void FadeIn()
        {
            m_State = State.FadeIn;
            StartFade(m_State);
        }

        /// <summary>
        /// Starts to fade out.
        /// </summary>
        public void FadeOut()
        {
            m_State = State.FadeOut;
            StartFade(m_State);
        }
        
        /// <summary>
        /// This mehtod is called every frame.
        /// </summary>
        private void Update()
        {
            if (m_State != State.None)
            {
                m_FadeTimer += Time.deltaTime;
                if (m_FadeTimer < FadeTime)
                {
                    var min = m_State == State.FadeIn ? 0f : 1f;
                    var max = m_State == State.FadeIn ? 1f : 0f;
                    var t = m_FadeTimer / FadeTime;
                    var a = Mathf.Lerp(min, max, t);
                    Fading(m_State, a);
                }
                else
                {
                    EndFade(m_State);
                    m_State = State.None;
                    m_FadeTimer = 0f;
                }
            }
        }

        /// <summary>
        /// This method is called when a fading has started.
        /// </summary>
        /// <param name="state">The fading state.</param>
        protected abstract void StartFade(State state);

        /// <summary>
        /// This method is called during a fading.
        /// </summary>
        /// <param name="state">The fading state.</param>
        /// <param name="value">The current fading value between 0 and 1</param>
        protected abstract void Fading(State state, float value);

        /// <summary>
        /// This method is called when a fading has finished.
        /// </summary>
        /// <param name="state">The fading state.</param>
        protected abstract void EndFade(State state);
    }
}