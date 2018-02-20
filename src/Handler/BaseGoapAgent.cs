using System;
using System.Collections.Generic;
using Libraries.btcp.Goap.src.Core;

namespace Libraries.btcp.Goap.src.Handler
{
    public abstract class BaseGoapAgent : IGoapAgent
    {
        private string m_agentName;
        protected BaseGoapMemory m_agentMemory;
        protected List<IGoapSensor> m_agentSensors;
        protected List<IGoapGoal> m_agentGoals;
        protected List<IGoapAction> m_agentActions;

        private IGoapGoal m_currentGoal;
        private IGoapAction m_currentAction;

        private float m_goalReAttemptTime = 1;
        private float m_currentGoalReAttemptTime = 0;
        private bool m_isDisabled = false;

        public BaseGoapAgent()
        {
            m_agentGoals = new List<IGoapGoal>();
            m_agentActions = new List<IGoapAction>();
            m_agentMemory = new BaseGoapMemory();
            m_agentSensors = new List<IGoapSensor>();
        }

        public void Init(string name)
        {
            m_agentName = name;
        }

        private void CancelGoal()
        {
            InterruptCurrentGoal();
        }

        private void RecalculateGoal()
        {
            InterruptCurrentGoal();
            CalculateGoal();
        }

        private void CalculateGoal()
        {
            m_currentGoalReAttemptTime = 0;
            m_currentGoal = GoapPlannerManager.s_Instance.GetPlanner().Plan(this);
            if (m_currentGoal == null)
            {
                OnCalculateGoalFailed();
                return;
            }

            m_currentGoal.OnBegan();
            m_currentGoal.Run();
        }


        private void OnCalculateGoalFailed()
        {
            GoapLogger.Log("BaseGoapAgent :: No Goal found for " + GetName() + " > 'Give me something to do!'");
        }

        private void CompleteCurrentGoal()
        {
            FinishCurrentGoal();
        }

        private void InterruptCurrentGoal()
        {
            if (m_currentGoal != null)
            {
                m_currentGoal.OnInterrupted();
                m_currentGoal = null;
            }

            InterruptCurrentAction();
        }
        
        
        private void FailCurrentGoal()
        {
            GoapLogger.LogWarning("BaseGoapAgent :: Goal Failed " + m_currentGoal.GetName());
            if (m_currentGoal != null)
            {
                m_currentGoal.OnFailed();
                m_currentGoal = null;
            }

            InterruptCurrentAction();
        }

        private void FinishCurrentGoal()
        {
            if (m_currentGoal != null)
            {
                m_currentGoal.OnFinish();
                m_currentGoal = null;
            }
        }

        private void InterruptCurrentAction()
        {
            if (m_currentAction != null)
            {
                m_currentAction.OnInterrupted();
                m_currentAction = null;
            }
        }

        private void FinishCurrentAction()
        {
            if (m_currentAction != null)
            {
                m_currentAction.OnFinish();
                m_currentAction = null;
            }
        }

        protected void RunNextAction()
        {
            if (m_currentGoal == null || m_currentAction != null) return;
            var goalActions = m_currentGoal.GetPlan();
            if (goalActions.Count > 0)
            {
                IGoapAction nextAction = goalActions.Dequeue();
                m_currentAction = nextAction;
                m_currentAction.OnBegin(m_currentGoal.GetGoalState(this));
            }
            else
            {
                CompleteCurrentGoal();
                CalculateGoal();
            }
        }

        public virtual void UpdateAgent(float delta)
        {
            if (m_isDisabled) return;
            UpdateInterruption();
            UpdateSensors();
            UpdateGoals(delta);
        }

        private void UpdateSensors()
        {
            if (m_isDisabled) return;

            foreach (IGoapSensor sensor in m_agentSensors)
            {
                sensor.UpdateSensor(m_agentMemory);
            }
        }

        private void UpdateInterruption()
        {
            //Check for higher priority goals becoming available
            if (m_currentAction != null)
            {
                if (m_currentAction.CanInterrupt())
                {
                    foreach (IGoapGoal goal in m_agentGoals)
                    {
                        if (goal.GetName() != m_currentGoal.GetName())
                        {
                            bool canRun = GoapPlannerManager.s_Instance.GetPlanner().CanRunGoal(this, goal);

                            if (canRun)
                            {
                                if (m_currentGoal == null || m_currentGoal.GetPriority() > goal.GetPriority())
                                {
                                    continue;
                                }


                                GoapLogger.LogWarning("BaseGoapAgent :: " + goal.GetName() + " interrupted goal " +
                                                      m_currentGoal.GetName());

                                RecalculateGoal();

                                break;
                            }
                        }
                    }
                }
            }
        }

        private void UpdateGoals(float delta)
        {
            if (m_currentGoal == null)
            {
                if (m_currentGoalReAttemptTime < m_goalReAttemptTime)
                {
                    m_currentGoalReAttemptTime += delta;
                }
                else
                {
                    CalculateGoal();
                    return;
                }
            }
            else
            {
                //TODO : GetGoalRelevance? 
                // if (m_currentGoal.ValidateGoal(this) == false)
                // {
                //     GoapLogger.LogWarning("BaseGoapAgent :: Current goal validation failed! > " + m_currentGoal.GetName() + " info: " + m_currentGoal.GetValidationReason());
                //     RecalculateGoal();
                //     return;
                // }

                IGoapAction[] currentActions = m_currentGoal.GetPlan().ToArray();
                //Check if actions are still valid
                for (int i = 0; i < currentActions.Length; i++)
                {
                    if (currentActions[i].ValidateAction() == false)
                    {
                        GoapLogger.LogWarning("BaseGoapAgent :: Current action validation failed! > " +
                                              currentActions[i].GetName() + " info: " +
                                              currentActions[i].GetValidationReason());
                        FailCurrentGoal();
                        return;
                    }
                }
            }
        }

        public void UpdateCurrentAction()
        {
            if (m_isDisabled) return;
            if (m_currentAction != null)
            {
                m_currentAction.OnRun(m_currentGoal.GetGoalState(this));
            }
        }

        private void OnActionCompleted(object sender, EventArgs args)
        {
            GoapLogger.Log("BaseGoapAgent :: Action Complete (" + m_currentAction.GetName() + " )");
            FinishCurrentAction();
        }

        private void OnActionFailed(object sender, EventArgs args)
        {
            GoapLogger.Log("BaseGoapAgent :: Action Failed (" + m_currentAction.GetName() + " )");
            InterruptCurrentAction();
            RecalculateGoal();
        }


        private void OnGoalCompleted()
        {
            GoapLogger.Log("BaseGoapAgent :: Goal Completed " + m_currentAction.GetName());
        }

        private void OnGoalUpdated()
        {
            GoapLogger.Log("BaseGoapAgent :: Goal Updated " + m_currentAction.GetName());
        }

        private void OnGoalFailed()
        {
            GoapLogger.Log("BaseGoapAgent :: Goal Failed " + m_currentAction.GetName());
        }

        public virtual IGoapGoal GetCurrentGoal()
        {
            return m_currentGoal;
        }

        public virtual string GetName()
        {
            return m_agentName;
        }

        public virtual IGoapMemory GetMemory()
        {
            return m_agentMemory;
        }

        public List<IGoapAction> GetActions()
        {
            return m_agentActions;
        }

        public List<IGoapGoal> GetGoals()
        {
            return m_agentGoals;
        }

        public List<IGoapSensor> GetSensors()
        {
            return m_agentSensors;
        }

        public void EnableAgent()
        {
            m_isDisabled = false;
        }

        public void DisableAgent()
        {
            m_isDisabled = true;
            InterruptCurrentGoal();
        }


        public void AddAction(IGoapAction newAction)
        {
            m_agentActions.Add(newAction);
            newAction.m_OnComplete += OnActionCompleted;
            newAction.m_OnFailed += OnActionFailed;
        }

        public void AddGoal(IGoapGoal goal)
        {
            m_agentGoals.Add(goal);
        }


        public void KillAgent()
        {
            RemoveEvents();
            CancelGoal();
        }

        private void RemoveEvents()
        {
            foreach (IGoapAction action in m_agentActions)
            {
                action.m_OnComplete -= OnActionCompleted;
                action.m_OnFailed -= OnActionFailed;
            }
        }
    }
}