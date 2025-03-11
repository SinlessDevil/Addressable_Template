using DG.Tweening;
using UI.Menu.Windows.Map;
using UnityEngine;

namespace UI.Menu.Windows.Meta
{
    public class MetaWindowState : IWindowState
    {
        private MetaWindow _window;

        private RectTransform _rectTransform;
        private Vector2 _hiddenPosition;
        private Vector2 _visiblePosition;

        public void Constructor(BaseWindow window)
        {
            _window = (MetaWindow)window;
            _rectTransform = _window.GetComponent<RectTransform>();

            _hiddenPosition = new Vector2(Screen.width, _rectTransform.anchoredPosition.y);
            _visiblePosition = _rectTransform.anchoredPosition;
            _rectTransform.anchoredPosition = _hiddenPosition;
        }

        public void Enter()
        {
            _window.gameObject.SetActive(true);
            _rectTransform.DOAnchorPosX(_visiblePosition.x, 0.5f)
                .OnStart(() => { _rectTransform.gameObject.SetActive(true); })
                .SetEase(Ease.OutExpo)
                .OnComplete(() =>
                {
                    //TODO: update Window
                });
        }

        public void Exit()
        {
            _rectTransform.DOAnchorPosX(_hiddenPosition.x, 0.5f)
                .SetEase(Ease.OutQuint)
                .OnComplete(() => { _rectTransform.gameObject.SetActive(false); });
        }

        public void Update()
        {
        }
    }
}