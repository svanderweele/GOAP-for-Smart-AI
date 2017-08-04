using System;
using AI.Goap.Core;

namespace AI.Goap.Handler
{
    public abstract class BaseGoapAction : IGoapAction
    {

        public virtual void OnRun(GoapState goalState) { }
        public virtual void OnFinish() { }

        public virtual bool ValidateContextPreconditions() { return true; }
        public virtual bool ValidateAction() { return true; }

        public virtual string GetValidationReason() { return m_debug_validationReason;}

        protected string m_debug_validationReason = "N/A";

        protected IGoapAgent m_agent;
        private GoapState m_postEffects;
        private GoapState m_preConditions;

        protected string m_actionName = "";
        protected float m_actionCost;
        protected bool m_canInterrupt = false;


        public event EventHandler m_OnComplete;
        public event EventHandler m_OnFailed;

        public BaseGoapAction()
        {
            m_postEffects = GoapState.Instantiate();
            m_preConditions = GoapState.Instantiate();
            m_canInterrupt = true;
        }

        public virtual void OnPrepare(IGoapAgent agent)
        {
            m_agent = agent;
        }

        public virtual void OnBegin(GoapState goalState)
        {
            GoapLogger.Log("BaseGoapAction :: " + GetName() + " has begun.");
        }

        public virtual string GetName()
        {
            if (m_actionName == "")
            {
                return GetType().Name;
            }

            return m_actionName;
        }

        public virtual float GetCost()
        {
            return m_actionCost;
        }


        protected void ClearPostEffects()
        {
            m_postEffects.Clear();
        }

        public virtual GoapState GetContextPostEffects(GoapState goalState)
        {
            return GetPostEffects(goalState);
        }

        public GoapState GetPostEffects(GoapState goalState)
        {
            return m_postEffects;
        }

        protected void SetPostEffect(string key, object postEffect)
        {
            m_postEffects.Set(key, postEffect);
        }


        protected void ClearPreConditions()
        {
            m_preConditions.Clear();
        }

        public virtual GoapState GetContextPreConditions(GoapState goalState)
        {
            return GetPreConditions(goalState);
        }

        protected void SetPreCondition(string key, object preCondition)
        {
            m_preConditions.Set(key, preCondition);
        }

        public GoapState GetPreConditions(GoapState goalState)
        {
            return m_preConditions;
        }

        public virtual bool CanInterrupt()
        {
            return m_canInterrupt;
        }

        public virtual void OnInterrupted()
        {
            GoapLogger.Log("BaseGoapAction :: " + GetName() + " was interrupted");
        }


        protected virtual void OnComplete()
        {
            if (m_OnComplete != null)
            {
                m_OnComplete(this, null);
            }
        }

        protected virtual void OnFailed()
        {
            if (m_OnFailed != null)
            {
                m_OnFailed(this, null);
            }
        }

    }
}
