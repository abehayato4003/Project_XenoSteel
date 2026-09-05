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

        private int _attackEndIgnoreFrames;



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
            Vector3 currentPosition = transform.position;
            Vector3 movement = currentPosition - _lastPosition;

            // ★ 攻撃中は movement を完全に無視する
            if (_isAttacking)
            {
                _lastPosition = currentPosition;
                return;
            }

            // ★ 攻撃終了後の揺れを無視する（アニメーションの戻り対策）
            if (_attackEndIgnoreFrames > 0)
            {
                _attackEndIgnoreFrames--;
                _lastPosition = currentPosition;
                return;
            }

            // ★ 移動時のみ方向を変える（movement が十分大きい時）
            if (movement.sqrMagnitude > 0.01f)
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
                _lastPosition = transform.position;
                _attackEndIgnoreFrames = 5;
            }
        }

        public void SetDirection(FacingDirection direction)
        {
            _direction = direction;

            Debug.Log(
                $"SetDirection: {direction}, " +
                $"Unit={gameObject.name}, " +
                $"Attacking={_isAttacking}\n" +
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