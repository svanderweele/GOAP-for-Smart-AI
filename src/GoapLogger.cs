namespace Libraries.btcp.Goap.src
{
    public static class GoapLogger
    {

        private enum DebugMode
        {
            OFF,
            ERRORS,
            WARNINGS,
            ALL
        }



        private static DebugMode m_debugMode = DebugMode.WARNINGS;

        public static void Log(string log)
        {
            if (m_debugMode != DebugMode.ALL)
            {
                return;
            }

            UnityEngine.Debug.Log(log);
        }

        public static void LogError(string log)
        {
            if (m_debugMode == DebugMode.OFF)
            {
                return;
            }

            UnityEngine.Debug.LogError(log);
        }


        public static void LogWarning(string log)
        {
            if (m_debugMode != DebugMode.WARNINGS && m_debugMode != DebugMode.ALL)
            {
                return;
            }

            UnityEngine.Debug.LogWarning(log);
        }
    }
}
