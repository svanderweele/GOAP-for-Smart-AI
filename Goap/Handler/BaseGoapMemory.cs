using Simon.Goap.Core;

namespace Simon.Goap.Handler
{
    public class BaseGoapMemory : IGoapMemory
    {

        private GoapState m_state;
        public BaseGoapMemory()
        {
            m_state = GoapState.Instantiate();
        }

        public GoapState GetWorldState()
        {
            return m_state;
        }

    }
}
