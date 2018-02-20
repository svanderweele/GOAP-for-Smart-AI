using Libraries.btcp.Goap.src.Core;

namespace Libraries.btcp.Goap.src.Handler
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
