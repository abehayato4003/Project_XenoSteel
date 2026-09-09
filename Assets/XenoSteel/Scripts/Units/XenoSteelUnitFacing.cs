using UnityEngine;
using TurnBasedStrategyFramework.Unity.Units;

namespace XenoSteel.Units
{
    public class XenoSteelUnitFacing : MonoBehaviour, IUnitFacing
    {
        public enum FacingDirection
        {
            Up,
            Down,
            Left,
            Right
        }

        [SerializeField]
        private FacingDirection _direction = FacingDirection.Up;

        [SerializeField]
        private Transform _visualRoot;

        public FacingDirection Direction => _direction;

        private void Awake()
        {
            if (_visualRoot == null)
            {
                _visualRoot = transform;
            }

            ApplyRotation();
        }

        public void SetDirection(FacingDirection direction)
        {
            _direction = direction;
            ApplyRotation();
        }

        private void ApplyRotation()
        {
            if (_visualRoot == null)
            {
                return;
            }

            float yRotation = _direction switch
            {
                FacingDirection.Up => 0f,
                FacingDirection.Right => 90f,
                FacingDirection.Down => 180f,
                FacingDirection.Left => 270f,
                _ => 0f
            };

            _visualRoot.localRotation =
                Quaternion.Euler(0f, yRotation, 0f);
        }

        public void SetFacing(float x, float z)
        {
            if (Mathf.Abs(x) >= Mathf.Abs(z))
            {
                SetDirection(
                    x >= 0f
                        ? FacingDirection.Right
                        : FacingDirection.Left
                );
            }
            else
            {
                SetDirection(
                    z >= 0f
                        ? FacingDirection.Up
                        : FacingDirection.Down
                );
            }
        }
    }
}