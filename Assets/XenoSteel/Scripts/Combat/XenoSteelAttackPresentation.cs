using System.Threading.Tasks;

using UnityEngine;

using XenoSteel.Units;

namespace XenoSteel.Combat
{
    public class XenoSteelAttackPresentation : MonoBehaviour
    {
        public enum PresentationType
        {
            SimpleMotion,
            Animator
        }

        [Header("演出方式")]
        [SerializeField]
        private PresentationType presentationType =
            PresentationType.SimpleMotion;

        [Header("共通")]
        [SerializeField]
        private Transform visualRoot;

        [Header("簡易モーション")]
        [SerializeField]
        private float attackDistance = 0.5f;

        [SerializeField]
        private float attackDuration = 0.15f;

        [Header("Animator")]
        [SerializeField]
        private Animator animator;

        [SerializeField]
        private string attackStateName = "Aster_BeamSaber";

        public async Task PlayAttackPresentation()
        {
            switch (presentationType)
            {
                case PresentationType.SimpleMotion:
                    await PlaySimpleMotion();
                    break;

                case PresentationType.Animator:
                    await PlayAnimatorMotion();
                    break;
            }
        }

        private async Task PlaySimpleMotion()
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

            // 前進
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

            // 後退
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

            visualRoot.localPosition =
                startPosition;
        }

        private async Task PlayAnimatorMotion()
        {
            if (animator == null)
            {
                Debug.LogWarning(
                    $"Animator is not assigned on {gameObject.name}.");

                return;
            }

            animator.Play(
                attackStateName,
                0,
                0f);

            await Task.CompletedTask;
        }
    }
}