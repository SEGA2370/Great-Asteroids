using System;
using UnityEngine;
public class PlayerShip : MonoBehaviour, ICollisionParent
{
    [SerializeField] private float _turnSpeed = 200f;
    [SerializeField] private float _thrustSpeed = 120f;
    [SerializeField] private GameObject _exhaust;
    [SerializeField] private PlayerWeapons _playerWeapons;
    private bool _isAlive = true;
    public bool IsAlive => _isAlive;
    private bool _isInvulnerable;
    private bool _thrusting;
    private bool _canResetPosition = true;
    private Rigidbody2D _rigidBody;
    private Renderer _renderer;
    private Collider2D _collider;
    private Scorer _scorer;
    private float _reviveCooldown = 1f;
    private float _lastReviveTime;
    private void Awake()
    {
        CacheComponents();
        InitializeState();
    }
    private void CacheComponents()
    {
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<Collider2D>();
        _rigidBody = GetComponent<Rigidbody2D>();
        _scorer = GetComponent<Scorer>();
    }
    private void InitializeState()
    {
        _thrusting = false;
        _isInvulnerable = false;
        _exhaust.SetActive(false);
    }
    private void FixedUpdate()
    {
        if (_isAlive)
        {
            HandleThrust();
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_isAlive)
        {
            Collided(collision);
        }
    }
    public void Collided(Collision2D collision)
    {
        ExplosionSpawner.Instance?.SpawnExplosion(transform.position);
        _scorer?.ScorePoints(collision);
        DisableShip();
        GameManager.Instance?.PlayerDied();
    }
    public void FireBullet()
    {
        if (_isAlive)
        {
            _playerWeapons?.FireBullet();
        }
    }
    public void DisableShip()
    {
        if (!_isAlive) return;
        _isAlive = false;
        SetShipActiveState(false);
        CancelInvulnerability();
    }
    public void ReviveShip()
    {
        if (Time.time - _lastReviveTime < _reviveCooldown) return;
        _lastReviveTime = Time.time;
        _isAlive = true;
        SetShipActiveState(true);
        ResetShipToStartPosition();
        CancelInvulnerability();
    }
    public void SetShipActiveState(bool isActive)
    {
        _renderer.enabled = isActive;
        _collider.enabled = isActive;
        _exhaust.SetActive(false);
    }
    public void ResetShipToStartPosition()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        _rigidBody.velocity = Vector2.zero;
        _rigidBody.angularVelocity = 0f;

        _isAlive = true;
        _isInvulnerable = false;
        SetShipActiveState(true);
        _exhaust.SetActive(false);
    }
    public void AllowPositionReset(bool allow)
    {
        _canResetPosition = allow;
    }
    public void EnableInvulnerability()
    {
        if (_isInvulnerable) return;
        _isInvulnerable = true;
        _collider.enabled = false;
        SetShipAlpha(0.25f);
    }
    public void CancelInvulnerability()
    {
        if (!_isInvulnerable) return;
        _isInvulnerable = false;
        _collider.enabled = true;
        SetShipAlpha(1f);
    }
    public void Rotate(float rotationInput)
    {
        if (!_isAlive) return;
        var rotateAmount = rotationInput * _turnSpeed * Time.deltaTime;
        transform.Rotate(0, 0, rotateAmount);
    }
    public void SetThrust(bool thrusting)
    {
        _thrusting = thrusting;
    }
    //public void EnterHyperspace()
    //{
    //    if (_isAlive)
    //    {
    //        transform.position = ViewportHelper.Instance?.GetRandomVisiblePosition() ?? Vector3.zero;
    //    }
    //}
    private void HandleThrust()
    {
        if (_thrusting != _exhaust.activeSelf)
        {
            _exhaust.SetActive(_thrusting);
        }
        if (_thrusting)
        {
            var thrustAmount = _thrustSpeed * Time.fixedDeltaTime;
            _rigidBody.AddForce(transform.up * thrustAmount, ForceMode2D.Force);
        }
    }
    private void SetShipAlpha(float alpha)
    {
        if (_renderer.material.HasProperty("_Color"))
        {
            var color = _renderer.material.color;
            color.a = alpha;
            _renderer.material.color = color;
        }
    }
}