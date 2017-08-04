using System;

namespace Simon.Goap.Core
{
    public interface IGoapAction
    {
        event EventHandler m_OnComplete;
        event EventHandler m_OnFailed;


        void OnPrepare(IGoapAgent agent);
        void OnBegin(GoapState goalState);
        void OnRun(GoapState goalState);
        void OnFinish();

        void OnInterrupted();
        bool CanInterrupt();
        bool ValidateContextPreconditions();
        bool ValidateAction();

        string GetName();
        float GetCost();

        string GetValidationReason();
        
        GoapState GetContextPreConditions(GoapState goalState);
        GoapState GetContextPostEffects(GoapState goalState);
        
        GoapState GetPreConditions(GoapState goalState);
        GoapState GetPostEffects(GoapState goalState);
    }

}