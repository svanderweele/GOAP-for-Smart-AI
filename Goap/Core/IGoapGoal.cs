using System.Collections.Generic;

namespace Simon.Goap.Core
{
    public interface IGoapGoal
    {

        void OnBegan();
        void OnFinish();
        void OnInterrupted();
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
