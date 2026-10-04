using AppliedMath.Week3;
using UnityEngine;
using UnityEngine.UI;

namespace AppliedMath.Week4
{
    public sealed class CoinBankUi : MonoBehaviour
    {
        private RectTransform canvasTransform;
        private RectTransform bankTarget;
        private Text bankLabel;
        private Camera worldCamera;
        private int targetBalance;
        private int displayedBalance;
        private int animationStartBalance;
        private float countTimer = 1f;
        private float punchTimer = 1f;
        private const float CountDuration = 0.7f;
        private const float PunchDuration = 0.32f;

        public void Configure(
            RectTransform canvas,
            RectTransform target,
            Text label,
            Camera camera,
            int startingBalance)
        {
            canvasTransform = canvas;
            bankTarget = target;
            bankLabel = label;
            worldCamera = camera;
            targetBalance = startingBalance;
            displayedBalance = startingBalance;
            animationStartBalance = startingBalance;
            UpdateLabel();
        }

        public void SpawnCoin(GameVector2 worldPosition, int value)
        {
            Vector3 screenPosition = worldCamera.WorldToScreenPoint(worldPosition.ToVector3());
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasTransform,
                screenPosition,
                null,
                out Vector2 localStart);

            GameObject coinObject = new GameObject("Flying Coin");
            coinObject.transform.SetParent(canvasTransform, false);
            RectTransform coinRectangle = coinObject.AddComponent<RectTransform>();
            coinRectangle.sizeDelta = new Vector2(30f, 30f);
            coinRectangle.anchoredPosition = localStart;
            Image coinImage = coinObject.AddComponent<Image>();
            coinImage.sprite = Week3RuntimeFactory.SquareSprite;
            coinImage.color = new Color(1f, 0.78f, 0.08f);

            FlyingCoin flyingCoin = coinObject.AddComponent<FlyingCoin>();
            flyingCoin.Configure(this, canvasTransform, bankTarget, localStart, value);
        }

        public void Deposit(int value)
        {
            animationStartBalance = displayedBalance;
            targetBalance += value;
            countTimer = 0f;
            punchTimer = 0f;
        }

        private void Update()
        {
            if (countTimer < CountDuration)
            {
                countTimer += Time.deltaTime;
                float t = ManualMath.Clamp01(countTimer / CountDuration);
                float eased = ManualMath.EaseOutCubic(t);
                displayedBalance = (int)System.Math.Round(
                    ManualMath.Lerp(animationStartBalance, targetBalance, eased));
                UpdateLabel();
            }

            if (punchTimer < PunchDuration)
            {
                punchTimer += Time.deltaTime;
                float t = ManualMath.Clamp01(punchTimer / PunchDuration);
                float scale;
                if (t < 0.35f)
                {
                    scale = ManualMath.Lerp(1f, 1.28f, ManualMath.EaseOutCubic(t / 0.35f));
                }
                else
                {
                    scale = ManualMath.Lerp(1.28f, 1f, ManualMath.EaseOutCubic((t - 0.35f) / 0.65f));
                }

                bankTarget.localScale = new Vector3(scale, scale, 1f);
            }
            else
            {
                bankTarget.localScale = Vector3.one;
            }
        }

        private void UpdateLabel()
        {
            bankLabel.text = "BANK  " + displayedBalance;
        }
    }

    public sealed class FlyingCoin : MonoBehaviour
    {
        private CoinBankUi bank;
        private RectTransform canvasTransform;
        private RectTransform target;
        private RectTransform rectangle;
        private Vector2 start;
        private int value;
        private float elapsed;
        private const float Duration = 0.78f;

        public void Configure(
            CoinBankUi coinBank,
            RectTransform canvas,
            RectTransform bankTarget,
            Vector2 startPosition,
            int coinValue)
        {
            bank = coinBank;
            canvasTransform = canvas;
            target = bankTarget;
            rectangle = GetComponent<RectTransform>();
            start = startPosition;
            value = coinValue;
        }

        private void Update()
        {
            Vector2 targetScreen = RectTransformUtility.WorldToScreenPoint(null, target.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasTransform,
                targetScreen,
                null,
                out Vector2 localTarget);

            elapsed += Time.deltaTime;
            float t = ManualMath.Clamp01(elapsed / Duration);
            GameVector2 p0 = new GameVector2(start.x, start.y);
            GameVector2 p2 = new GameVector2(localTarget.x, localTarget.y);
            GameVector2 midpoint = ManualMath.Lerp(p0, p2, 0.5f);
            GameVector2 p1 = midpoint + new GameVector2(0f, 150f);
            GameVector2 position = ManualMath.QuadraticBezier(p0, p1, p2, ManualMath.EaseInOutCubic(t));
            rectangle.anchoredPosition = new Vector2(position.x, position.y);

            float scale = ManualMath.Lerp(1f, 0.45f, ManualMath.EaseOutCubic(t));
            rectangle.localScale = new Vector3(scale, scale, 1f);

            if (t >= 1f)
            {
                bank.Deposit(value);
                Destroy(gameObject);
            }
        }
    }
}
