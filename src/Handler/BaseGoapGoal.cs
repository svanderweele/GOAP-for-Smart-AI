using System.Collections.Generic;
using Libraries.btcp.Goap.src.Core;

namespace Libraries.btcp.Goap.src.Handler
{
    public abstract class BaseGoapGoal : IGoapGoal
    {
        public virtual bool CanRun(IGoapAgent agent) { return true; }
        // public virtual bool ValidateGoal(IGoapAgent agent) { return true; }

        public virtual void OnBegan() { }
        public virtual void OnFinish() { }
        public virtual void OnInterrupted() { }
        public virtual void Run() { }
        public virtual void OnFailed() { }

        public string GetValidationReason() { return m_debug_validationReason; }

        protected string m_debug_validationReason;
        private string m_goalName;
        private float m_goalPriority;
        protected GoapState m_goalState;
        private Queue<IGoapAction> m_goalPlan;

        public BaseGoapGoal()
        {
            m_goalState = GoapState.Instantiate();
        }

        public void Init(string name, int priority)
        {
            SetName(name);
            SetPriority(priority);
        }

        protected void SetName(string name)
        {
            m_goalName = name;
        }

        protected void SetPriority(int priority)
        {
            m_goalPriority = priority;
        }

        public virtual GoapState GetGoalState(IGoapAgent agent)
        {
            return m_goalState;
        }

        public virtual string GetName()
        {
            return m_goalName;
        }

        public Queue<IGoapAction> GetPlan()
        {
            return m_goalPlan;
        }

        public void SetPlan(Queue<IGoapAction> plan)
        {
            m_goalPlan = plan;
            Run();
        }

        public float GetPriority()
        {
            return m_goalPriority;
        }



    }
}
