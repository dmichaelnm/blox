using Blox.CommonNS;
using Blox.ConfigurationNS;
using UnityEngine;

namespace Blox.GameNS.ActionNS
{
    public class ActionRotateCreatableModel : Action
    {
        private bool m_KeyPressed;
        
        public override bool IsActionStarted(bool firstCall)
        {
            if (firstCall)
            {
                if (m_GameManager.selectedEntityType is CreatableModelType && m_KeyPressed)
                {
                    return true;
                }
            }

            if (!m_KeyPressed || !(m_GameManager.selectedEntityType is CreatableModelType))
                return false;
                
            return true;
        }

        public override void OnFinished()
        {
            if (m_GameManager.selectedPosition != null)
            {
                var chunkManager = m_GameManager.chunkManager;
                var position = ((Vector3)m_GameManager.selectedPosition).ToVector3Int();
                var model = chunkManager.GetModel(position);
                if (model != null)
                {
                    var rotation = chunkManager.GetRotation(position);
                    var rid =  ((int)rotation + 1) % 4;
                    chunkManager.SetRotation(position, (CreatableModelType.Rotation)rid);
                    model.transform.Rotate(Vector3.up, 90f);
                }
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(m_GameManager.rotation) && !m_KeyPressed)
                m_KeyPressed = true;
            else if (Input.GetKeyUp(m_GameManager.rotation) && m_KeyPressed)
                m_KeyPressed = false;
        }
    }
}