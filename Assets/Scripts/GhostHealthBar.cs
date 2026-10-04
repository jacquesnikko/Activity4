using AppliedMath.Week3;
using UnityEngine;
using UnityEngine.UI;

namespace AppliedMath.Week4
{
    public sealed class GhostHealthBar : MonoBehaviour
    {
        private Image realBar;
        private Image ghostBar;
        private Text label;
        private float ghostStart;
        private float ghostTarget = 1f;
        private float holdTimer;
        private float easeTimer;
        private const float HoldDuration = 0.45f;
        private const float EaseDuration = 0.8f;

        public void Configure(Image immediateBar, Image delayedBar, Text hpLabel)
        {
            realBar = immediateBar;
            ghostBar = delayedBar;
            label = hpLabel;
        }

        public void SetHealth(int current, int maximum, bool immediate)
        {
            float normalized = maximum <= 0 ? 0f : ManualMath.Clamp01(current / (float)maximum);
            realBar.fillAmount = normalized;
            label.text = "BASE HP  " + current + " / " + maximum;

            if (immediate)
            {
                ghostBar.fillAmount = normalized;
                ghostStart = normalized;
                ghostTarget = normalized;
                holdTimer = 0f;
                easeTimer = EaseDuration;
                return;
            }

            ghostStart = ghostBar.fillAmount;
            ghostTarget = normalized;
            holdTimer = HoldDuration;
            easeTimer = 0f;
        }

        private void Update()
        {
            if (holdTimer > 0f)
            {
                holdTimer -= Time.deltaTime;
                return;
            }

            if (easeTimer >= EaseDuration)
            {
                ghostBar.fillAmount = ghostTarget;
                return;
            }

            easeTimer += Time.deltaTime;
            float t = ManualMath.Clamp01(easeTimer / EaseDuration);
            float eased = ManualMath.EaseOutCubic(t);
            ghostBar.fillAmount = ManualMath.Lerp(ghostStart, ghostTarget, eased);
        }
    }
}
