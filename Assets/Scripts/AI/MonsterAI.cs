using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

// NavMeshAgent SADECE sunucuda çalışır.
// NetworkTransformReliable pozisyonu tüm clientlara senkronize eder.
// Canavar prefab'ında: NetworkIdentity, NetworkTransformReliable, NavMeshAgent, Collider(Trigger) olmalı.
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NetworkIdentity))]
public class MonsterAI : NetworkBehaviour
{
    public enum MonsterState { Patrol, Chase }

    [Header("Algılama")]
    public float detectionRadius = 12f;
    public float attackRadius    = 1.5f;
    public LayerMask playerMask;           // Inspector'da "Player" layer seç

    [Header("Hareket Hızları")]
    public float patrolSpeed  = 2f;
    public float chaseSpeed   = 6f;

    [Header("Devriye - Waypoints (boş bırakılırsa rastgele)")]
    public Transform[] waypoints;
    public float patrolRadius = 20f;       // Waypoint yoksa rastgele yarıçap
    public float waypointWaitTime = 1.5f;  // Noktaya varınca bekleme süresi

    [Header("Ceza Ayarları")]
    public Transform spawnPoint;           // Cezalandırılan oyuncu buraya ışınlanır
    public float punishCooldown = 2f;      // Aynı oyuncuya arka arkaya ceza önleme

    // ─── SyncVar: tüm clientlar durumu bilir (animasyon için kullanılabilir) ──
    [SyncVar] public MonsterState CurrentState = MonsterState.Patrol;
    [SyncVar] public float        CurrentSpeed = 0f;   // Animator'a bağlanabilir

    private NavMeshAgent _agent;
    private Transform    _target;
    private int          _waypointIndex;
    private bool         _isWaiting;
    private float        _waitTimer;
    private bool         _punishOnCooldown;

    // ═══════════════════════════════════════════════════════════════════════
    // Mirror Başlangıç Callbackleri
    // ═══════════════════════════════════════════════════════════════════════

    public override void OnStartServer()
    {
        _agent       = GetComponent<NavMeshAgent>();
        _agent.speed = patrolSpeed;
        GoNextWaypoint();
    }

    public override void OnStartClient()
    {
        // Host (server+client aynı makine) ise NavMeshAgent sunucu tarafında çalışsın
        if (isServer) return;

        // Pure client: NavMeshAgent'ı devre dışı bırak.
        // NetworkTransformReliable sunucunun pozisyonunu bu clienta otomatik uygular.
        NavMeshAgent na = GetComponent<NavMeshAgent>();
        if (na != null) na.enabled = false;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Ana Döngü — Sadece Sunucu
    // ═══════════════════════════════════════════════════════════════════════

    void Update()
    {
        if (!isServer) return;

        ScanForPlayers();

        switch (CurrentState)
        {
            case MonsterState.Patrol: UpdatePatrol(); break;
            case MonsterState.Chase:  UpdateChase();  break;
        }

        // Clientlardaki animasyon scriptleri CurrentSpeed SyncVar'ını okuyabilir
        CurrentSpeed = _agent.velocity.magnitude;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Oyuncu Algılama
    // ═══════════════════════════════════════════════════════════════════════

    [Server]
    void ScanForPlayers()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, playerMask);

        if (hits.Length == 0)
        {
            if (CurrentState == MonsterState.Chase) EnterPatrol();
            return;
        }

        // En yakın oyuncuyu seç
        Transform closest = null;
        float     minDist = float.MaxValue;
        foreach (var h in hits)
        {
            float d = Vector3.Distance(transform.position, h.transform.position);
            if (d < minDist) { minDist = d; closest = h.transform; }
        }

        _target = closest;
        if (CurrentState != MonsterState.Chase) EnterChase();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Devriye Durumu
    // ═══════════════════════════════════════════════════════════════════════

    [Server]
    void UpdatePatrol()
    {
        if (_agent.pathPending) return;

        if (_agent.remainingDistance <= _agent.stoppingDistance)
        {
            if (!_isWaiting)
            {
                _isWaiting  = true;
                _waitTimer  = waypointWaitTime;
            }

            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0f)
            {
                _isWaiting = false;
                GoNextWaypoint();
            }
        }
    }

    [Server]
    void GoNextWaypoint()
    {
        if (waypoints != null && waypoints.Length > 0)
        {
            // Sahnede belirlenmiş waypoint varsa sırayla dön
            _agent.SetDestination(waypoints[_waypointIndex].position);
            _waypointIndex = (_waypointIndex + 1) % waypoints.Length;
        }
        else
        {
            // Waypoint yoksa spawn pozisyonu etrafında rastgele NavMesh noktası bul
            Vector3 origin    = spawnPoint != null ? spawnPoint.position : transform.position;
            Vector3 randomDir = origin + Random.insideUnitSphere * patrolRadius;
            if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
                _agent.SetDestination(hit.position);
        }
    }

    [Server]
    void EnterPatrol()
    {
        CurrentState  = MonsterState.Patrol;
        _target       = null;
        _agent.speed  = patrolSpeed;
        GoNextWaypoint();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Kovalama Durumu
    // ═══════════════════════════════════════════════════════════════════════

    [Server]
    void UpdateChase()
    {
        if (_target == null) { EnterPatrol(); return; }

        _agent.SetDestination(_target.position);

        // Mesafe tabanlı temas kontrolü (trigger olmasa bile çalışır)
        if (Vector3.Distance(transform.position, _target.position) <= attackRadius)
            TryPunish(_target);
    }

    [Server]
    void EnterChase()
    {
        CurrentState = MonsterState.Chase;
        _agent.speed = chaseSpeed;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Trigger Temas (Collider → IsTrigger = true olmalı prefabda)
    // ═══════════════════════════════════════════════════════════════════════

    void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;
        if (!other.CompareTag("Player")) return;
        TryPunish(other.transform);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Ceza Sistemi
    // ═══════════════════════════════════════════════════════════════════════

    [Server]
    void TryPunish(Transform playerTransform)
    {
        if (_punishOnCooldown) return;

        NetworkIdentity netId = playerTransform.GetComponent<NetworkIdentity>();
        if (netId?.connectionToClient == null) return;

        // Sunucu tarafında envanteri sıfırla
        PlayerInventory inv = playerTransform.GetComponent<PlayerInventory>();
        inv?.ResetInventory();

        // Sadece cezalandırılan oyuncunun clientına TargetRpc gönder
        Vector3 safePos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        RpcPunishPlayer(netId.connectionToClient, safePos);

        StartCoroutine(PunishCooldownRoutine());
    }

    IEnumerator PunishCooldownRoutine()
    {
        _punishOnCooldown = true;
        yield return new WaitForSeconds(punishCooldown);
        _punishOnCooldown = false;
    }

    // Yalnızca hedef oyuncunun client makinesinde çalışır
    [TargetRpc]
    void RpcPunishPlayer(NetworkConnection target, Vector3 safePos)
    {
        Transform player = NetworkClient.localPlayer?.transform;
        if (player == null) return;

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = safePos;
        }
        // Rigidbody'nin bir sonraki frame'de Transform'ı güncellemesini bekle
        player.position = safePos;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Editor Görselleştirme
    // ═══════════════════════════════════════════════════════════════════════

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
