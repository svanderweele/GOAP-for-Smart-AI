using Simon.Goap.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Simon.Goap.Handler
{
    public class GoapPlanner : IGoapPlanner
    {
        private IGoapAgent m_agent;
        private IGoapGoal m_currentGoal;

        private bool m_Calculated = false;
        private AStar<GoapState> m_aStar;

        public GoapPlanner()
        {
            GoapNode<GoapState>.Warmup(100);
            GoapState.Warmup(100);

            m_aStar = new AStar<GoapState>(100);
        }

        public IGoapGoal Plan(IGoapAgent agent)
        {
            m_agent = agent;
            m_currentGoal = null;
            m_Calculated = false;

            List<IGoapGoal> possibleGoals = GetPossibleGoals(agent);

            if (possibleGoals.Count == 0)
            {
                GoapLogger.LogWarning("[ReGoapPlanner] Agent does not have any Goals to perform. " + m_agent.GetName());
            }

            while (possibleGoals.Count > 0)
            {
                m_currentGoal = possibleGoals[possibleGoals.Count - 1];
                possibleGoals.RemoveAt(possibleGoals.Count - 1);

                if (CanFullfillWithActions(m_agent, m_currentGoal) == false)
                {
                    //No actions can't handle this goal
                    GoapLogger.LogWarning("GoalPlanner :: No Actions to handle Goal (" + m_currentGoal.GetName() + ")");
                    m_currentGoal = null;
                    continue;
                }


                GoapState targetState = m_currentGoal.GetGoalState(agent);

                GoapNode<GoapState> leaf = (GoapNode<GoapState>)m_aStar.Run(GoapNode<GoapState>.Instantiate(this, targetState, null, null), targetState);

                if (leaf == null)
                {
                    GoapLogger.LogWarning("GoapPlanner :: Pathfinding failed!");
                    m_currentGoal = null;
                    continue;
                }

                Queue<IGoapAction> actions = leaf.CalculatePath();
                if (actions.Count == 0)
                {
                    GoapLogger.LogWarning("GoapPlanner :: Calculating Path failed!");
                    m_currentGoal = null;
                    continue;
                }

                m_currentGoal.SetPlan(actions);
                break;
            }

            if (m_currentGoal != null)
                GoapLogger.Log(string.Format("[ReGoapPlanner] Calculated plan for goal '{0}', plan length: {1}", m_currentGoal, m_currentGoal.GetPlan().Count));
            else
                GoapLogger.LogWarning("[ReGoapPlanner] Error while calculating plan.");

            return m_currentGoal;
        }


        private bool CanFullfillWithActions(IGoapAgent agent, IGoapGoal goal)
        {
            GoapState goalState = goal.GetGoalState(agent).Clone();

            foreach (IGoapAction action in agent.GetActions())
            {
                if (CanRunAction(agent, action) == false)
                {
                    continue;
                }

                goalState.RemoveCompletedConditions(action.GetContextPostEffects(goalState)); ;
            }

            goalState.RemoveCompletedConditions(m_agent.GetMemory().GetWorldState());

            if (goalState.Count > 0)
            {
                return false;
            }

            return true;
        }


        private List<IGoapGoal> GetPossibleGoals(IGoapAgent agent)
        {
            List<IGoapGoal> possibleGoals = new List<IGoapGoal>();
            foreach (IGoapGoal goal in agent.GetGoals())
            {
                if (CanRunGoal(agent, goal))
                {
                    possibleGoals.Add(goal);
                }
            }

            possibleGoals.Sort((x, y) => x.GetPriority().CompareTo(y.GetPriority()));

            return possibleGoals;
        }


        public bool CanRunGoal(IGoapAgent agent, IGoapGoal goal)
        {
            if (agent.GetCurrentGoal() == goal)
            {
                return false;
            }

            if (goal.CanRun(agent) == false)
            {
                return false;
            }

            GoapState differenceState = goal.GetGoalState(agent).Clone();
            differenceState.RemoveCompletedConditions(m_agent.GetMemory().GetWorldState());

            if (differenceState.Count == 0)
            {
                return false;
            }


            return true;
        }

        public bool CanRunAction(IGoapAgent agent, IGoapAction action)
        {
            action.OnPrepare(agent);

            if (action.ValidateContextPreconditions() == false)
            {
                return false;
            }

            return true;
        }


        public IGoapAgent GetAgent()
        {
            return m_agent;
        }

    }
}
