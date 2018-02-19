using System.Collections.Generic;

namespace AI.Goap.Core
{
    public interface IGoapAgent
    {
        string GetName();

        void UpdateAgent(float delta);

        IGoapGoal GetCurrentGoal();
        IGoapMemory GetMemory();
        List<IGoapAction> GetActions();
        List<IGoapGoal> GetGoals();
        List<IGoapSensor> GetSensors();
    }
}
