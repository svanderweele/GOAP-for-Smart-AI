using System.Collections.Generic;
using Simon.Goap.Core;
using IdleSiege.Systems.Goap;
using System;
using IdleSiege.Utilities;

namespace Simon.Goap.Handler
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
        private bool m_CalculateGoalOnStart = true;

        public BaseGoapAgent(string agentName)
        {
            m_agentName = agentName;

            m_agentGoals = new List<IGoapGoal>();
            m_agentActions = new List<IGoapAction>();
            m_agentMemory = new BaseGoapMemory();
            m_agentSensors = new List<IGoapSensor>();
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

        public void CalculateGoal()
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
            RunNextAction();
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
            }

            InterruptCurrentAction();
            FinishCurrentGoal();
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
            }

            FinishCurrentAction();
        }

        private void FinishCurrentAction()
        {
            if (m_currentAction != null)
            {
                m_currentAction.OnFinish();
                m_currentAction = null;
            }
        }

        private void CompleteCurrentAction()
        {
            FinishCurrentAction();
        }

        private void RunNextAction()
        {
            Queue<IGoapAction> goalActions = m_currentGoal.GetPlan();

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

        public virtual void UpdateAgent()
        {
            if (m_CalculateGoalOnStart)
            {
                CalculateGoal();
                m_CalculateGoalOnStart = false;
            }

            UpdateInterruption();
            UpdateSensors();
            UpdateGoals();
            UpdateCurrentAction();
        }

        private void UpdateSensors()
        {
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


                                GoapLogger.LogWarning("BaseGoapAgent :: " + goal.GetName() + " interrupted goal " + m_currentGoal.GetName());

                                RecalculateGoal();

                                break;
                            }
                        }
                    }
                }
            }
        }

        private void UpdateGoals()
        {
            if (m_currentGoal == null)
            {
                if (m_currentGoalReAttemptTime < m_goalReAttemptTime)
                {
                    m_currentGoalReAttemptTime += TimeManager.GameplayTime.GetDeltaTime();
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
                        GoapLogger.LogWarning("BaseGoapAgent :: Current action validation failed! > " + currentActions[i].GetName() + " info: " + currentActions[i].GetValidationReason());
                        RecalculateGoal();
                        return;
                    }
                }

            }
        }

        private void UpdateCurrentAction()
        {
            if (m_currentAction != null)
            {
                m_currentAction.OnRun(m_currentGoal.GetGoalState(this));
            }
        }

        private void OnActionCompleted(object sender, EventArgs args)
        {
            GoapLogger.Log("BaseGoapAgent :: Action Complete (" + m_currentAction.GetName() + " )");
            CompleteCurrentAction();
            RunNextAction();
        }

        private void OnActionFailed(object sender, EventArgs args)
        {
            GoapLogger.Log("BaseGoapAgent :: Action Failed (" + m_currentAction.GetName() + " )");
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


        public void AddAction(IGoapAction newAction)
        {
            m_agentActions.Add(newAction);
            newAction.m_OnComplete += OnActionCompleted;
            newAction.m_OnFailed += OnActionFailed;
        }


        public void KillAgent()
        {
            RemoveEvents();
            CancelGoal();
        }

        private void RemoveEvents()
        {
            foreach(IGoapAction action in m_agentActions)
            {
                action.m_OnComplete -= OnActionCompleted;
                action.m_OnFailed -= OnActionFailed;
            }
        }
    }
}
