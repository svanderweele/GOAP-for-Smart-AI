using System.Collections.Generic;

namespace Libraries.btcp.Goap.src.Core
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
