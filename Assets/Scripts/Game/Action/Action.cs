using UnityEngine;

namespace Blox.GameNS.ActionNS
{
    public abstract class Action : MonoBehaviour
    {
        public float time;

        [SerializeField] protected GameManager m_GameManager;

        public abstract bool IsActionStarted(bool firstCall);

        public abstract void OnFinished();

        public virtual void OnCancelled()
        {
        }
    }
}