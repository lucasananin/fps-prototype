using UnityEngine;

namespace FPS
{
    public class PingPongMover : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Rigidbody _rigidbody;
        [Tooltip("Ponto de partida. Use um objeto vazio que NÃO seja filho deste objeto.")]
        [SerializeField] private Transform _pointA;
        [Tooltip("Ponto de destino. Use um objeto vazio que NÃO seja filho deste objeto.")]
        [SerializeField] private Transform _pointB;

        [Header("Movement")]
        [SerializeField] private float _speed = 3f;
        [Tooltip("Tempo parado (em segundos) ao chegar em cada ponta.")]
        [SerializeField] private float _waitTime = 0.5f;

        private Vector3 _positionA;
        private Vector3 _positionB;
        private Vector3 _currentTarget;
        private float _waitTimer;

        private void Awake()
        {
            if (_rigidbody == null)
                _rigidbody = GetComponent<Rigidbody>();

            // Guarda as posições no início, assim mover os marcadores depois não afeta o trajeto.
            _pointA.parent = null;
            _pointB.parent = null;
            _positionA = _pointA.position;
            _positionB = _pointB.position;
        }

        private void Start()
        {
            _rigidbody.position = _positionA;
            _currentTarget = _positionB;
        }

        private void FixedUpdate()
        {
            if (_waitTimer > 0f)
            {
                _waitTimer -= Time.fixedDeltaTime;
                return;
            }

            Vector3 nextPosition = Vector3.MoveTowards(
                _rigidbody.position,
                _currentTarget,
                _speed * Time.fixedDeltaTime);

            _rigidbody.MovePosition(nextPosition);

            if ((nextPosition - _currentTarget).sqrMagnitude < 0.0001f)
            {
                // Chegou: troca o destino e espera um pouco.
                _currentTarget = _currentTarget == _positionB ? _positionA : _positionB;
                _waitTimer = _waitTime;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_pointA == null || _pointB == null)
                return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(_pointA.position, _pointB.position);
            Gizmos.DrawWireSphere(_pointA.position, 0.2f);
            Gizmos.DrawWireSphere(_pointB.position, 0.2f);
        }
    }
}
