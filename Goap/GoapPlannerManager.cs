using AI.Goap.Handler;

namespace AI.Goap
{
    public class GoapPlannerManager
    {
        private static GoapPlannerManager m_instance;
        public static GoapPlannerManager s_Instance
        {
            get
            {
                if (m_instance == null) m_instance = new GoapPlannerManager();
                return m_instance;
            }
        }

        private GoapPlanner m_goapPlanner;
        public GoapPlanner GetPlanner() { return m_goapPlanner; }

        public GoapPlannerManager()
        {
            m_goapPlanner = new GoapPlanner();
        }


    }
}