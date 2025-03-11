using UnityEngine;
using UnityEngine.UI;

namespace Infrastructure.Loading
{
    public class ProgressBar : MonoBehaviour
    {
        [SerializeField] private Image _filler;
        
        public void DrawBar(float value)
        {
            _filler.fillAmount = Mathf.Clamp01(value);
        }

        public void ResetFill()
        {
            _filler.fillAmount = 0;
        }
    }
}