using System.Collections.Generic;
using Libraries.btcp.Goap.src.Core;

namespace Libraries.btcp.Goap.src.Handler
{
    public class GoapNode<T> : INode<GoapState>
    {
        private IGoapPlanner m_planner;
        private GoapNode<T> m_parent;
        private IGoapAction m_action;

        private GoapState m_currentState;
        private GoapState m_targetState;

        private float m_gCost;
        private float m_hCost;

        private List<INode<GoapState>> m_expandList;

        public GoapNode()
        {
            m_expandList = new List<INode<GoapState>>();
        }

        public void Init(IGoapPlanner planner, GoapState goalState, GoapNode<T> parent, IGoapAction action)
        {
            m_expandList.Clear();

            m_planner = planner;
            m_parent = parent;
            m_action = action;


            if (m_parent != null)
            {
                m_currentState = parent.GetState().Clone();
                m_gCost = parent.GetCost();
            }
            else
            {
                m_currentState = m_planner.GetAgent().GetMemory().GetWorldState().Clone();
            }


            if (action != null)
            {
                m_gCost += action.GetCost();

                GoapState preconditions = action.GetPreConditions(goalState);
                m_targetState = goalState + preconditions;

                GoapState effects = action.GetPostEffects(goalState);
                m_currentState.AddFromState(effects);


                //Did this action's effect fulfill any of the goals?
                m_targetState.RemoveCompletedConditions(effects);

                //Did the world fulfill any of the goals?
                m_targetState.RemoveCompletedConditions(m_planner.GetAgent().GetMemory().GetWorldState());
            }
            else
            {
                var diff = GoapState.Instantiate();
                goalState.CreateStateWithMissingDifferences(m_currentState, ref diff);
                m_targetState = diff;
            }


            //Cost is equal to the amount of extra actions
            m_hCost = m_targetState.Count;

        }

        public float GetCost()
        {
            return m_gCost;
        }

        public float GetHeuristic()
        {
            return m_hCost;
        }


        public GoapState GetState()
        {
            return m_currentState;
        }

        public List<INode<GoapState>> GetNeighbours()
        {
            m_expandList.Clear();

            IGoapAgent agent = m_planner.GetAgent();
            List<IGoapAction> actions = agent.GetActions();

            for (int i = actions.Count - 1; i >= 0; i--)
            {
                IGoapAction possibleAction = actions[i];

                if (possibleAction == m_action)
                {
                    continue;
                }

                if (GoapPlannerManager.s_Instance.GetPlanner().CanRunAction(agent, possibleAction) == false)
                {
                    continue;
                }

                GoapState preConditions = possibleAction.GetContextPreConditions(m_targetState);
                GoapState postEffects = possibleAction.GetContextPostEffects(m_targetState);

                bool isValid = (postEffects.HasAny(m_targetState)) &&
                         (!m_targetState.HasAnyConflict(preConditions)) && (!m_targetState.HasAnyConflict(postEffects));

                if (isValid)
                {
                    GoapState targetState = m_targetState;
                    m_expandList.Add(Instantiate(m_planner, targetState, this, possibleAction));
                }

            }

            return m_expandList;
        }

        public Queue<IGoapAction> CalculatePath()
        {
            Queue<IGoapAction> path = new Queue<IGoapAction>();
            CalculatePath(ref path);
            return path;
        }

        public void CalculatePath(ref Queue<IGoapAction> container)
        {
            GoapNode<T> node = this;

            while (node.GetParent() != null)
            {
                container.Enqueue(node.m_action);
                node = (GoapNode<T>)node.GetParent();
            }
        }



        public INode<GoapState> GetParent()
        {
            return m_parent;
        }

        public bool IsGoal(GoapState goal)
        {
            return (m_hCost == 0);
        }

        public void Recycle()
        {
            m_currentState.Recycle();
            m_currentState = null;
            m_targetState.Recycle();
            m_targetState = null;

            lock (m_cachedNodes)
            {
                m_cachedNodes.Push(this);
            }
        }


        #region Node Factory

        private static Stack<GoapNode<T>> m_cachedNodes;

        public static void Warmup(int count)
        {
            m_cachedNodes = new Stack<GoapNode<T>>(count);

            for (int i = 0; i < count; i++)
            {
                m_cachedNodes.Push(new GoapNode<T>());
            }
        }

        public static GoapNode<T> Instantiate(IGoapPlanner planner, GoapState goalState, GoapNode<T> parent, IGoapAction action)
        {
            GoapNode<T> node;

            if (m_cachedNodes == null)
            {
                m_cachedNodes = new Stack<GoapNode<T>>();
            }

            node = (m_cachedNodes.Count > 0) ? m_cachedNodes.Pop() : new GoapNode<T>();
            node.Init(planner, goalState, parent, action);
            return node;
        }


        #endregion

        public int QueueIndex { get; set; }
        public float Priority { get; set; }

    }
}
