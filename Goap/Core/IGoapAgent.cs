using System.Collections.Generic;

namespace Simon.Goap.Core
{
    public interface IGoapAgent
    {
        string GetName();

        void UpdateAgent();

        IGoapGoal GetCurrentGoal();
        IGoapMemory GetMemory();
        List<IGoapAction> GetActions();
        List<IGoapGoal> GetGoals();
        List<IGoapSensor> GetSensors();
    }
}
