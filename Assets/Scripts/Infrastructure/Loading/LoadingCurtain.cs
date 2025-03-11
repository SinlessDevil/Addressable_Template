using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Infrastructure.Loading
{
    public class LoadingCurtain : MonoBehaviour, ILoadingCurtain
    {
        private const float Delay = 1.75f;
        private const float AnimationDuration = 0.65f;
        
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
        
        private RectTransform _currentContainer;
        private StatusLoading _currentStatusLoading;
        private Status Current = Status.Hided;
        
        private Coroutine _loadingTextCoroutine;

        private Action _onContinueClick;
        
        private void Awake()
        {
            DontDestroyOnLoad(this);
            
            _retryLoadingButton.onClick.AddListener(OnContinueClick);

            SetCurrentStatusLoading(StatusLoading.Start);
            
            _logoCuratinContainer.gameObject.SetActive(false);
            _levelCurtainContainer.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _retryLoadingButton.onClick.RemoveListener(OnContinueClick);
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

        public void Hide()
        {
            StartCoroutine(AnimationOpen());
            StartLoadingTextAnimation();
        }

        private void SetCurrentStatusLoading(StatusLoading loading)
        {
            _currentStatusLoading = loading;
            switch (_currentStatusLoading)
            {
                case StatusLoading.Start:
                    _currentContainer = _logoCuratinContainer;
                    break;
                case StatusLoading.BetweenLevels:
                    _currentContainer = _levelCurtainContainer;
                    break;
            }
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
        
        private IEnumerator AnimationOpen()
        {
            switch (_currentStatusLoading)
            {
                case StatusLoading.Start:
                    yield return AnimationOpenLevelCurtainRoutine();
                    break;
                case StatusLoading.BetweenLevels:
                    yield return AnimationOpenLogoCurtainRoutine(); 
                    break;
            }
            
            StopLoadingTextAnimation();

            SetCurrentStatusLoading(StatusLoading.BetweenLevels);
            
            gameObject.SetActive(false);
            
            IsActive = false;
        }

        private IEnumerator AnimationOpenLevelCurtainRoutine()
        {
            yield return new WaitForSeconds(Delay);
            
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
                yield return null;
            }

            _left.anchoredPosition = leftTarget;
            _right.anchoredPosition = rightTarget;
        }

        private IEnumerator AnimationOpenLogoCurtainRoutine()
        {
            yield return new WaitForSeconds(Delay);

            float screenHeight = Screen.height;
            float elapsedTime = 0f;

            Vector2 startPos = _logoCuratinContainer.anchoredPosition;
            Vector2 targetPos = new Vector2(startPos.x, screenHeight);

            while (elapsedTime < AnimationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / AnimationDuration;
                _logoCuratinContainer.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                yield return null;
            }

            _logoCuratinContainer.anchoredPosition = targetPos;
        }
        
        private void StartLoadingTextAnimation()
        {
            _loadingTextCoroutine = StartCoroutine(LoadingTextEffectRoutine());
        }

        private void StopLoadingTextAnimation()
        {
            if (_loadingTextCoroutine != null)
                StopCoroutine(_loadingTextCoroutine);
        }

        private IEnumerator LoadingTextEffectRoutine()
        {
            string baseText = "Loading";
            while (true)
            {
                _loadingText.text = baseText + "";
                yield return new WaitForSeconds(0.15f);
                _loadingText.text = baseText + ".";
                yield return new WaitForSeconds(0.15f);
                _loadingText.text = baseText + "..";
                yield return new WaitForSeconds(0.15f);
                _loadingText.text = baseText + "...";
                yield return new WaitForSeconds(0.15f);
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