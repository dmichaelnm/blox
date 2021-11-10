using System;
using System.Collections.Generic;
using System.Linq;
using Blox.UtilitiesNS;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Blox.UINS
{
    public abstract class FadeBehaviour : MonoBehaviour
    {
        public enum FadeState
        {
            None,
            FadeIn,
            FadeOut
        }

        public float FadeTime = 1f;

        private FadeState m_FadeState;
        private float m_FadeTimer;
        private List<Graphic> m_GraphicElements;
        [CanBeNull] private Action<FadeState> m_Callback;

        public void FadeIn(Action<FadeState> callback = null)
        {
            InitializeFading(FadeState.FadeIn, callback);
        }

        public void FadeOut(Action<FadeState> callback = null)
        {
            InitializeFading(FadeState.FadeOut, callback);
        }

        private void InitializeFading(FadeState fadeState, Action<FadeState> callback)
        {
            m_Callback = callback;
            m_GraphicElements = new List<Graphic>();
            CollectGraphicElements(gameObject, m_GraphicElements);
            Iterate(graphic =>
            {
                graphic.enabled = true;
                var c = graphic.color;
                graphic.color = fadeState == FadeState.FadeOut
                    ? new Color(c.r, c.g, c.b, 1f)
                    : new Color(c.r, c.g, c.b, 0f);
            });
            m_FadeState = fadeState;
        }

        private void CollectGraphicElements([NotNull] GameObject obj, [NotNull] List<Graphic> elements)
        {
            elements.AddRange(obj.GetComponents<Graphic>());
            obj.Iterate(child => CollectGraphicElements(child, elements));
        }

        private void Awake()
        {
            OnAwake();
        }

        private void Update()
        {
            if (m_FadeState != FadeState.None)
            {
                m_FadeTimer += Time.deltaTime;
                if (m_FadeTimer < FadeTime)
                {
                    var min = m_FadeState == FadeState.FadeIn ? 0f : 1f;
                    var max = m_FadeState == FadeState.FadeIn ? 1f : 0f;
                    var t = m_FadeTimer / FadeTime;
                    var a = Mathf.Lerp(min, max, t);

                    Iterate(element =>
                    {
                        var c = element.color;
                        element.color = new Color(c.r, c.g, c.b, a);
                    });
                }
                else
                {
                    Iterate(graphic =>
                    {
                        var c = graphic.color;
                        graphic.color = m_FadeState == FadeState.FadeIn
                            ? new Color(c.r, c.g, c.b, 1f)
                            : new Color(c.r, c.g, c.b, 0f);
                    });
                    m_Callback?.Invoke(m_FadeState);
                    m_FadeState = FadeState.None;
                    m_FadeTimer = 0f;
                }
            }
        }

        protected virtual void OnAwake()
        {
        }

        private void Iterate(Action<Graphic> iterator)
        {
            foreach (var element in m_GraphicElements.Where(element => !element.tag.Equals("NoFade")))
            {
                iterator(element);
            }
        }
    }
}