using System;

namespace Infrastructure.Loading
{
    public interface ILoadingCurtain
    {
        event Action StartedShowEvent;
        event Action StartedHidedEvent;
        event Action FinishedHidedEvent;
        void ShowProgress(float progress);
        void ShowNoInternetWarning(Action onContinueClick);
        
        void Show();
        void Hide();
    }
}