using System.Threading.Tasks;
using UnityEngine;
using XenoSteel.Units;

namespace XenoSteel.Combat
{
    public class XenoSteelAttackPresentation : MonoBehaviour
    {
        [Header("共通")]
        [SerializeField]
        private Transform visualRoot;

        [Header("Animator")]
        [SerializeField]
        private Animator animator;

        public async Task PlayAttackPresentation(
            XenoSteelCombatPresentationData presentation)
        {
            if (presentation == null)
            {
                return;
            }

            switch (presentation.presentationType)
            {
                case PresentationType.None:
                    break;

                case PresentationType.SimpleMotion:
                    await PlaySimpleMotion(
                        presentation.attackDistance,
                        presentation.attackDuration
                    );
                    break;

                case PresentationType.NormalMotion:
                    await PlayAnimatorMotion(
                        presentation.animatorTriggerName,
                        presentation.attackAnimation
                    );
                    break;

                case PresentationType.Special:
                    // 特別演出は後で実装
                    break;
            }
        }

        private async Task PlaySimpleMotion(
            float attackDistance,
            float attackDuration)
        {
            if (visualRoot == null)
            {
                Debug.LogWarning(
                    $"Visual Root is not assigned on {gameObject.name}.");
                return;
            }

            var facing =
                GetComponent<XenoSteelUnitFacing>();

            if (facing == null)
            {
                Debug.LogWarning(
                    $"XenoSteelUnitFacing is not found on {gameObject.name}.");
                return;
            }

            Vector3 direction = facing.Direction switch
            {
                XenoSteelUnitFacing.FacingDirection.Up
                    => Vector3.forward,
                XenoSteelUnitFacing.FacingDirection.Down
                    => Vector3.back,
                XenoSteelUnitFacing.FacingDirection.Right
                    => Vector3.right,
                XenoSteelUnitFacing.FacingDirection.Left
                    => Vector3.left,
                _ => Vector3.forward
            };

            Vector3 startPosition =
                visualRoot.localPosition;

            Vector3 attackPosition =
                startPosition + direction * attackDistance;

            float halfDuration =
                attackDuration * 0.5f;

            float timer = 0f;

            while (timer < halfDuration)
            {
                timer += Time.deltaTime;

                float t =
                    Mathf.Clamp01(timer / halfDuration);

                visualRoot.localPosition =
                    Vector3.Lerp(
                        startPosition,
                        attackPosition,
                        t);

                await Task.Yield();
            }

            timer = 0f;

            while (timer < halfDuration)
            {
                timer += Time.deltaTime;

                float t =
                    Mathf.Clamp01(timer / halfDuration);

                visualRoot.localPosition =
                    Vector3.Lerp(
                        attackPosition,
                        startPosition,
                        t);

                await Task.Yield();
            }

            visualRoot.localPosition = startPosition;
        }

        private async Task PlayAnimatorMotion(
            string triggerName,
            AnimationClip attackAnimation)
        {
            if (animator == null)
            {
                Debug.LogWarning(
                    $"XenoSteelAttackPresentation: Animatorが設定されていません。" +
                    $" Unit={gameObject.name}"
                );
                return;
            }

            if (string.IsNullOrEmpty(triggerName))
            {
                Debug.LogWarning(
                    $"XenoSteelAttackPresentation: Animator Triggerが設定されていません。" +
                    $" Unit={gameObject.name}"
                );
                return;
            }

            if (attackAnimation == null)
            {
                Debug.LogWarning(
                    $"XenoSteelAttackPresentation: Attack Animationが設定されていません。" +
                    $" Unit={gameObject.name}"
                );
                return;
            }
            Debug.Log(
                $"Animator Trigger発火: Unit={gameObject.name}, Trigger={triggerName}"
            );
            animator.SetTrigger(triggerName);

            float timer = 0f;

            while (timer < attackAnimation.length)
            {
                timer += Time.deltaTime;
                await Task.Yield();
            }
        }
        public void OnAttackTiming()
        {
            Debug.Log(
                $"Attack Timing: Unit={gameObject.name}"
            );
        }
    }
}