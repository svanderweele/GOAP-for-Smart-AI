using System.Collections.Generic;
using System.Diagnostics;

namespace AI.Goap.Core
{
    public class GoapState
    {

        private Dictionary<string, object> m_values;

        private Dictionary<string, object> m_bufferA;

        private Dictionary<string, object> m_bufferB;

        public GoapState()
        {
            m_values = new Dictionary<string, object>();
            m_bufferB = new Dictionary<string, object>();

            m_bufferA = m_values;
        }

        public void Init(GoapState old = null)
        {
            m_values.Clear();

            if (old != null)
            {
                lock (old.m_values)
                {
                    foreach (var pair in old.m_values)
                    {
                        m_values[pair.Key] = pair.Value;
                    }
                }
            }
        }


        public int Count { get { return m_values.Count; } }

        public GoapState Clone()
        {
            return Instantiate(this);
        }

        public static GoapState operator +(GoapState a, GoapState b)
        {
            GoapState state;

            lock (a.m_values)
            {
                state = a.Clone();
            }

            lock (b.m_values)
            {
                foreach (var pair in b.m_values)
                {
                    state.m_values[pair.Key] = pair.Value;
                }

                return state;
            }
        }

        public void CreateStateWithMissingDifferences(GoapState other, ref GoapState differenceOutput)
        {
            if (differenceOutput == null)
            {
                Debug.Assert(differenceOutput != null, "GoapState :: CreateStaeWithMissingDifferences :: Input Reference was null!");
            }

            lock (m_values)
            {
                foreach (var pair in m_values)
                {
                    object otherValue;
                    other.m_values.TryGetValue(pair.Key, out otherValue);

                    if (!Equals(pair.Value, otherValue))
                    {
                        differenceOutput.m_values[pair.Key] = pair.Value;
                    }
                }
            }
        }

        public int RemoveCompletedConditions(GoapState other, int stopAt = int.MaxValue)
        {
            lock (m_values)
            {
                int count = 0;
                var buffer = m_values;
                m_values = (m_values == m_bufferA) ? m_bufferB : m_bufferA;
                m_values.Clear();

                foreach (var pair in buffer)
                {
                    object otherValue;
                    other.m_values.TryGetValue(pair.Key, out otherValue);

                    if (!Equals(pair.Value, otherValue))
                    {
                        m_values[pair.Key] = pair.Value;
                        count++;

                        if (count >= stopAt)
                        {
                            break;
                        }
                    }
                }

                return count;
            }
        }


        public bool HasAny(GoapState other)
        {
            lock (m_values)
            {
                lock (other.m_values)
                {
                    foreach (var pair in other.m_values)
                    {
                        object thisValue;
                        m_values.TryGetValue(pair.Key, out thisValue);
                        if (Equals(pair.Value, thisValue))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }


        public bool HasAnyConflict(GoapState other)
        {
            lock (m_values) lock (other.m_values)
                {
                    foreach (var pair in other.m_values)
                    {
                        object thisValue;
                        m_values.TryGetValue(pair.Key, out thisValue);
                        object otherValue = pair.Value;

                        if (otherValue == null || Equals(otherValue, false))
                        {
                            continue;
                        }

                        if (thisValue != null && !Equals(thisValue, otherValue))
                        {
                            return true;
                        }
                    }

                    return false;

                }
        }

        public override string ToString()
        {
            lock (m_values)
            {
                var result = "GoapState: ";
                foreach (var pair in m_values)
                    result += string.Format("'{0}': {1}, ", pair.Key, pair.Value);
                return result;
            }
        }

        public object Get(string key)
        {
            lock (m_values)
            {
                if (!m_values.ContainsKey(key))
                    return default(object);

                return m_values[key];
            }
        }

        public void Set(string key, object value)
        {
            lock (m_values)
            {
                m_values[key] = value;
            }
        }

        public void Remove(string key)
        {
            lock (m_values)
            {
                m_values.Remove(key);
            }
        }

        public Dictionary<string, object> GetValues()
        {
            lock (m_values)
                return m_values;
        }

        public bool HasKey(string key)
        {
            lock (m_values)
                return m_values.ContainsKey(key);
        }

        public void Clear()
        {
            lock (m_values)
                m_values.Clear();
        }

        public void AddFromState(GoapState other)
        {
            lock (m_values) lock (other.m_values)
                {
                    foreach (var pair in other.m_values)
                    {
                        m_values[pair.Key] = pair.Value;
                    }
                }
        }

        #region State Factory
        private static Stack<GoapState> m_cachedStates;

        public static void Warmup(int count)
        {
            m_cachedStates = new Stack<GoapState>();

            for (int i = 0; i < m_cachedStates.Count; i++)
            {
                m_cachedStates.Push(new GoapState());
            }
        }

        public static GoapState Instantiate(GoapState old = null)
        {
            GoapState state;

            if (m_cachedStates == null)
            {
                m_cachedStates = new Stack<GoapState>();
            }

            state = (m_cachedStates.Count > 0) ? m_cachedStates.Pop() : new GoapState();
            state.Init(old);
            return state;
        }


        public void Recycle()
        {
            m_cachedStates.Push(this);
        }

        #endregion

    }
}
