using Blox.GameNS.ActionNS;
using UnityEngine;
using UnityEngine.UI;

namespace Blox.GameNS
{
    public class ActionManager : MonoBehaviour
    {
        [SerializeField] private Image m_ProgressBar;
        
        private Action[] m_Actions;
        private Action m_CurrentAction;
        private float m_ProgressTimer;

        private void Awake()
        {
            m_Actions = GetComponentsInChildren<Action>();
            m_ProgressBar.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (m_CurrentAction != null)
            {
                if (m_CurrentAction.IsActionStarted(false))
                {
                    // action is still running
                    m_ProgressTimer += Time.deltaTime;
                    var progress = m_ProgressTimer / m_CurrentAction.time;
                    m_ProgressBar.material.SetFloat("_Progress", progress);
                    if (m_ProgressTimer >= m_CurrentAction.time)
                    {
                        m_ProgressTimer = 0f;
                        m_ProgressBar.gameObject.SetActive(false);
                        m_CurrentAction.OnFinished();
                        m_CurrentAction = null;
                    }
                }
                else
                {
                    // action has cancelled
                    m_ProgressTimer = 0f;
                    m_ProgressBar.gameObject.SetActive(false);
                    m_CurrentAction.OnCancelled();                    
                    m_CurrentAction = null;
                }
            }
            else
            {
                // no current active action, look for a new started action
                foreach (var action in m_Actions)
                {
                    if (action.IsActionStarted(true))
                    {
                        m_ProgressTimer = 0f;
                        m_CurrentAction = action;
                        m_ProgressBar.gameObject.SetActive(true);
                        m_ProgressBar.material.SetFloat("_Progress", 0f);
                        break;
                    }
                }
            }
        }
    }
}