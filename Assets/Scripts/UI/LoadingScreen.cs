using System;
using Blox.EnvironmentNS;
using Blox.PlayerNS;
using UnityEngine;
using UnityEngine.UI;

namespace Blox.UINS
{
    public class LoadingScreen : FadeBehaviour
    {
        internal enum LoadingState
        {
            ShowMainMenu,

            ShowGame
        }

        [SerializeField] private ChunkManager m_ChunkManager;
        [SerializeField] private MainMenu m_MainMenu;
        [SerializeField] private PlayerController m_PlayerController;
        [SerializeField] private GameObject m_RotationCamera;

        private LoadingState m_LoadingState;
        private Action m_Callback;

        public void Show(Action callback)
        {
            GetComponent<Image>().enabled = true;
            m_LoadingState = LoadingState.ShowGame;
            m_Callback = callback;
            FadeIn(OnEndFading);
        }

        private void Awake()
        {
            m_ChunkManager.OnChunkManagerInitialized += OnChunkManagerInitialized;
            m_ChunkManager.OnChunkManagerDestroyed += OnChunkManagerDestroyed;

            var image = GetComponent<Image>();
            image.enabled = true;

            m_LoadingState = LoadingState.ShowMainMenu;
            m_MainMenu.gameObject.SetActive(false);
        }

        private void OnChunkManagerDestroyed(ChunkManager component)
        {
            m_ChunkManager.OnChunkManagerInitialized -= OnChunkManagerInitialized;
            m_ChunkManager.OnChunkManagerDestroyed -= OnChunkManagerDestroyed;
        }

        private void OnChunkManagerInitialized(ChunkManager component)
        {
            if (m_LoadingState == LoadingState.ShowMainMenu)
            {
                m_PlayerController.gameObject.SetActive(false);

                m_MainMenu.gameObject.SetActive(true);
                m_MainMenu.FadeIn();
                FadeOut();
            }
            else
            {
                FadeOut(state => gameObject.SetActive(false));
            }
        }

        private void OnEndFading(FadeState fadeState)
        {
            if (m_LoadingState == LoadingState.ShowGame)
            {
                m_RotationCamera.SetActive(false);
                m_PlayerController.gameObject.SetActive(true);
                m_Callback.Invoke();
            }
        }
    }
}