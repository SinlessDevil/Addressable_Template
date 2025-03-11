using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Infrastructure.Loading
{
    public class LoadingCurtain : MonoBehaviour, ILoadingCurtain
    {
        private const float Delay = 1.75f;
        private const float AnimationDuration = 0.7f;
        
        [SerializeField] private RectTransform _logoCuratinContainer;
        [Space(10)] [Header("Between Levels Panel Components")]
        [SerializeField] private RectTransform _levelCurtainContainer;
        [SerializeField] private RectTransform _right;
        [SerializeField] private RectTransform _left;
        [SerializeField] private Text _loadingText;
        [Space(10)] [Header("Internet Warning Panel Components")]
        [SerializeField] private GameObject _noInternetWarning;
        [SerializeField] private Button _retryLoadingButton;
        [Space(10)] [Header("Progress Bars")]
        [SerializeField] private ProgressBar _progressBarStart;
        [SerializeField] private ProgressBar _progressBarBetweenLevels;
        [SerializeField] private List<CanvasGroup> _progressBar;
        
        private RectTransform _currentContainer;
        private StatusLoading _currentStatusLoading = StatusLoading.Start;
        private Status Current = Status.Hided;
        
        private CancellationTokenSource _loadingTextCts;

        private Action _onContinueClick;
        
        private void Awake()
        {
            DontDestroyOnLoad(this);
            
            _retryLoadingButton.onClick.AddListener(OnContinueClick);
            
            ResetPanels();
            ResetProgressBars();
            
            SetCurrentStatusLoading(StatusLoading.Start);
        }

        private void OnDestroy()
        {
            _retryLoadingButton.onClick.RemoveListener(OnContinueClick);
            _loadingTextCts?.Cancel();
        }

        public bool IsActive { get; private set; } = false;
        
        public event Action StartedShowEvent;
        public event Action StartedHidedEvent;
        public event Action FinishedHidedEvent;

        public void ShowProgress(float progress)
        {
            switch (_currentStatusLoading)
            {
                case StatusLoading.Start:
                    _progressBarStart.DrawBar(progress);
                    break;
                case StatusLoading.BetweenLevels:
                    _progressBarBetweenLevels.DrawBar(progress);
                    break;
            }
            
            Current = Status.Showed;
        }

        public void ShowNoInternetWarning(Action onContinueClick)
        {
            _onContinueClick = onContinueClick;
            _currentContainer.gameObject.SetActive(false);
            _noInternetWarning.SetActive(true);
        }

        public void Show()
        {
            IsActive = true;
            gameObject.SetActive(true);

            _progressBar.ForEach(x => x.alpha = 1f);
            
            switch (_currentStatusLoading)
            {
                case StatusLoading.Start:
                    SetUpLogoCurtainComponents();
                    break;
                case StatusLoading.BetweenLevels:
                    SetUpLevelCurtainComponents();
                    break;
            }
        }

        public async void Hide()
        {
            await AnimationOpenRoutine();
            StartLoadingTextAnimation();
        }
        
        private void SetCurrentStatusLoading(StatusLoading loading)
        {
            _currentStatusLoading = loading;
            _currentContainer = loading switch
            {
                StatusLoading.Start => _logoCuratinContainer,
                StatusLoading.BetweenLevels => _levelCurtainContainer,
                _ => _currentContainer
            };
        }
        
        private void SetUpLevelCurtainComponents()
        {
            _levelCurtainContainer.gameObject.SetActive(true);
            
            _left.anchoredPosition = Vector2.zero;
            _right.anchoredPosition = Vector2.zero;
        }
        
        private void SetUpLogoCurtainComponents()
        {
            _logoCuratinContainer.gameObject.SetActive(true);
        }
        
        private void OnContinueClick()
        {
            _onContinueClick?.Invoke();
            _currentContainer.gameObject.SetActive(true);
            _noInternetWarning.SetActive(false);
        }
        
        private async UniTask AnimationOpenRoutine()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(Delay));
            
            StopLoadingTextAnimation();
            ResetProgressBars();
            
            switch (_currentStatusLoading)
            {
                case StatusLoading.Start:
                    await AnimationOpenLogoCurtainRoutine();
                    break;
                case StatusLoading.BetweenLevels:
                    await AnimationOpenLevelCurtainRoutine();
                    break;
            }
            
            SetCurrentStatusLoading(StatusLoading.BetweenLevels);
            
            _currentContainer.gameObject.SetActive(false);
            
            ResetPanels();
            
            IsActive = false;
        }

        private void ResetProgressBars()
        {
            _progressBarStart.DrawBar(0f);
            _progressBarBetweenLevels.DrawBar(0f);
            
            _progressBar.ForEach(x => x.alpha = 0f);
        }

        private void ResetPanels()
        {
            _logoCuratinContainer.gameObject.SetActive(false);
            _levelCurtainContainer.gameObject.SetActive(false);
        }
        
        private async UniTask AnimationOpenLevelCurtainRoutine()
        {
            float screenWidth = Screen.width;
            float elapsedTime = 0f;

            Vector2 leftStart = _left.anchoredPosition;
            Vector2 leftTarget = new Vector2(-screenWidth / 2, leftStart.y);
            Vector2 rightStart = _right.anchoredPosition;
            Vector2 rightTarget = new Vector2(screenWidth / 2, rightStart.y);

            while (elapsedTime < AnimationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / AnimationDuration;
                _left.anchoredPosition = Vector2.Lerp(leftStart, leftTarget, t);
                _right.anchoredPosition = Vector2.Lerp(rightStart, rightTarget, t);
                await UniTask.NextFrame();
            }
        }

        private async UniTask AnimationOpenLogoCurtainRoutine()
        {
            float screenHeight = Screen.height / 2;
            float elapsedTime = 0f;
            
            Vector2 startPos = _logoCuratinContainer.anchoredPosition;
            Vector2 targetPos = new Vector2(startPos.x, screenHeight);

            while (elapsedTime < AnimationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / AnimationDuration;
                _logoCuratinContainer.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                await UniTask.Yield();
            }
        }
        
        private void StartLoadingTextAnimation()
        {
            _loadingTextCts = new CancellationTokenSource();
            LoadingTextEffectRoutine(_loadingTextCts.Token).Forget();
        }

        private void StopLoadingTextAnimation()
        {
            _loadingTextCts?.Cancel();
            _loadingText.text = "";
        }

        private async UniTaskVoid LoadingTextEffectRoutine(CancellationToken token)
        {
            string baseText = "Loading";
            while (!token.IsCancellationRequested)
            {
                _loadingText.text = baseText;
                await UniTask.Delay(TimeSpan.FromSeconds(0.15f), cancellationToken: token);
                _loadingText.text = baseText + ".";
                await UniTask.Delay(TimeSpan.FromSeconds(0.15f), cancellationToken: token);
                _loadingText.text = baseText + "..";
                await UniTask.Delay(TimeSpan.FromSeconds(0.15f), cancellationToken: token);
                _loadingText.text = baseText + "...";
                await UniTask.Delay(TimeSpan.FromSeconds(0.15f), cancellationToken: token);
            }
        }
    }
    
    public enum Status 
    {
        Unknown = 0,
        Hided = 1,
        Showed = 2,
    }
    
    public enum StatusLoading 
    {
        Unknown = 0,
        Start = 1,
        BetweenLevels = 2,
    }
}
