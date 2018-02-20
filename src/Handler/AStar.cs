using System.Collections.Generic;

namespace Libraries.btcp.Goap.src.Handler
{
    public class AStar<T>
    {

        private FastPriorityQueue<INode<T>, T> m_frontier;
        private Dictionary<T, INode<T>> m_stateToNode;
        private Dictionary<T, INode<T>> m_exploredNodes;
        private List<INode<T>> m_createdNodes;

        public AStar(int maxNodesCreated)
        {
            m_frontier = new FastPriorityQueue<INode<T>, T>(maxNodesCreated);
            m_stateToNode = new Dictionary<T, INode<T>>();
            m_exploredNodes = new Dictionary<T, INode<T>>();
            m_createdNodes = new List<INode<T>>(maxNodesCreated);
        }

        private void ClearNodes()
        {
            foreach (var node in m_createdNodes)
            {
                node.Recycle();
            }

            m_createdNodes.Clear();
        }

        public INode<T> Run(INode<T> start, T goal, int maxIterations = 100, bool earlyExit = true, bool clearNodes = true)
        {
            m_frontier.Clear();
            m_exploredNodes.Clear();
            m_stateToNode.Clear();

            if (clearNodes)
            {
                ClearNodes();
                m_createdNodes.Add(start);
            }

            m_frontier.Enqueue(start, start.GetCost());

            var iterations = 0;
            while (m_frontier.Count > 0 && (iterations < maxIterations) && ((m_frontier.Count + 1) < m_frontier.MaxSize))
            {
                iterations++;

                if (iterations == maxIterations)
                {
                    GoapLogger.LogWarning("AStar :: Hit Max Iterations!");
                }

                var node = m_frontier.Dequeue();

                if (node.IsGoal(goal))
                {
                    if (node == start)
                    {
                        GoapLogger.Log("AStar :: Start Node was End Node!");
                    }

                    return node;
                }

                m_exploredNodes[node.GetState()] = node;

                foreach (var child in node.GetNeighbours())
                {
                    if (clearNodes)
                    {
                        m_createdNodes.Add(child);
                    }

                    if (earlyExit && child.IsGoal(goal))
                    {
                        return child;
                    }

                    float childCost = child.GetCost();
                    T childState = child.GetState();

                    if (m_exploredNodes.ContainsKey(childState))
                    {
                        continue;
                    }

                    INode<T> similarNode;
                    m_stateToNode.TryGetValue(childState, out similarNode);

                    if (similarNode != null)
                    {
                        if (similarNode.GetCost() > childCost)
                        {
                            m_frontier.Remove(similarNode);
                        }
                        else
                        {
                            break;
                        }
                    }

                    m_frontier.Enqueue(child, childCost);
                    m_stateToNode[childState] = child;

                }
            }

            return null;
        }

    }


    public interface INode<T>
    {
        float GetCost();
        float GetHeuristic();

        T GetState();

        INode<T> GetParent();

        List<INode<T>> GetNeighbours();
        void Recycle();
        bool IsGoal(T goal);

        int QueueIndex { get; set; }
        float Priority { get; set; }
    }
}
