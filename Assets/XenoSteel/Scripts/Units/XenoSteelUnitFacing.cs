using UnityEngine;

namespace XenoSteel.Units
{
    public class XenoSteelUnitFacing : MonoBehaviour
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

        private Vector3 _lastPosition;

        public FacingDirection Direction => _direction;

        private bool _isAttacking;

        private void Awake()
        {
            if (_visualRoot == null)
            {
                _visualRoot = transform;
            }

            _lastPosition = transform.position;

            ApplyRotation();
        }

        private void Update()
        {
            if (_isAttacking)
            {
                return;
            }

            Vector3 currentPosition = transform.position;
            Vector3 movement = currentPosition - _lastPosition;

            // 実際に移動している時だけ向きを更新
            if (movement.sqrMagnitude > 0.0001f)
            {
                if (Mathf.Abs(movement.x) >= Mathf.Abs(movement.z))
                {
                    SetDirection(
                        movement.x >= 0f
                            ? FacingDirection.Right
                            : FacingDirection.Left);
                }
                else
                {
                    SetDirection(
                        movement.z >= 0f
                            ? FacingDirection.Up
                            : FacingDirection.Down);
                }
            }

            _lastPosition = currentPosition;
        }

        public void SetAttacking(bool attacking)
        {
            _isAttacking = attacking;

            if (!attacking)
            {
                // 攻撃中に発生した位置変化を
                // 通常移動として判定しない
                _lastPosition = transform.position;
            }
        }

        public void SetDirection(FacingDirection direction)
        {
            _direction = direction;

            Debug.Log(
                $"SetDirection: {direction}\n" +
                System.Environment.StackTrace
            );

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
    }
}