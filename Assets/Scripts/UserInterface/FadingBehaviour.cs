using System;
using UnityEngine;
using UnityEngine.UI;

namespace Blox.UserInterfaceNS
{
    public class FadingBehaviour : MonoBehaviour
    {
        public enum FadingType
        {
            Color,
            Transparency
        }

        public enum FadingState
        {
            None,
            In,
            Out
        }

        public FadingType fadingType;
        public FadingState fadingState;
        public float fadingTime;

        private Graphic[] m_GraphicElements;
        private Color[] m_GraphicColors;
        private float m_FadeTimer;
        private Action<FadingState> m_Callback;
        private FadingState m_State;

        public void FadeIn() => FadeIn(null);

        public void FadeIn(Action<FadingState> callback) => FadeIn(FadingType.Transparency, callback);

        public void FadeIn(FadingType type, Action<FadingState> callback = null)
        {
            m_Callback = callback;
            m_State = FadingState.In;
            fadingType = type;
        }

        public void FadeOut() => FadeOut(null);

        public void FadeOut(Action<FadingState> callback) => FadeOut(FadingType.Transparency, callback);

        public void FadeOut(FadingType type, Action<FadingState> callback)
        {
            m_Callback = callback;
            m_State = FadingState.Out;
            fadingType = type;
        }

        protected virtual void _Awake()
        {
        }

        private void Awake()
        {
            m_GraphicElements = GetComponentsInChildren<Graphic>();
            m_GraphicColors = new Color[m_GraphicElements.Length];
            for (var i = 0; i < m_GraphicElements.Length; i++)
                m_GraphicColors[i] = m_GraphicElements[i].color;

            m_State = FadingState.None;
            m_FadeTimer = 0f;
            SetFadeState(fadingState);
            _Awake();
        }

        private void Update()
        {
            if (m_State == FadingState.None)
                return;

            m_FadeTimer += Time.deltaTime;
            var t = Mathf.Clamp(m_FadeTimer / fadingTime, 0f, 1f);
            t = m_State == FadingState.In ? 1f - t : t;
            SetChildrenColor(t);

            if (m_FadeTimer >= fadingTime)
            {
                var state = m_State;
                m_FadeTimer = 0f;
                m_State = FadingState.None;
                SetFadeState(state);
                m_Callback?.Invoke(state);
            }
        }

        private void SetFadeState(FadingState state)
        {
            SetChildrenColor(state == FadingState.In ? 0f : 1f);
        }

        private void SetChildrenColor(float value)
        {
            for (var i = 0; i < m_GraphicElements.Length; i++)
            {
                var startColor = m_GraphicColors[i];
                var endColor = fadingType == FadingType.Color ? Color.black : Color.clear;
                m_GraphicElements[i].color = Color.Lerp(startColor, endColor, value);
            }
        }
    }
}