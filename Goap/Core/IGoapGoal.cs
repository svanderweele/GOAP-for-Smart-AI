using System.Collections.Generic;

namespace AI.Goap.Core
{
    public interface IGoapGoal
    {

        void OnBegan();
        void OnFinish();
        void OnInterrupted();
        void OnFailed();
        void Run();
        bool CanRun(IGoapAgent agent);
        // bool ValidateGoal(IGoapAgent agent);

        string GetName();
        float GetPriority();
        string GetValidationReason();
        Queue<IGoapAction> GetPlan();
        void SetPlan(Queue<IGoapAction> plan);

        GoapState GetGoalState(IGoapAgent agent);
    }
}
