using System;
using Fusion;
using TMPro;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public static Player LocalInstance { get; private set; }

    [SerializeField]
    private Ball _prefabBall;

    [Networked]
    private TickTimer delay { get; set; }

    [Networked]
    public bool spawnedProjectile { get; set; }

    private ChangeDetector _changeDetector;
    public Material _material;
    private NetworkCharacterController _cc;
    private Vector3 _forward;

    private ChatPanel _chatPanel;

    [Rpc(
        RpcSources.InputAuthority,
        RpcTargets.StateAuthority,
        HostMode = RpcHostMode.SourceIsHostPlayer
    )]
    public void RPC_SendMessage(string message, RpcInfo info = default)
    {
        RPC_RelayMessage(message, info.Source);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_RelayMessage(string message, PlayerRef messageSource)
    {
        if (_chatPanel == null)
            _chatPanel = FindFirstObjectByType<ChatPanel>();

        if (_chatPanel != null)
        {
            _chatPanel.AddMessage(message, messageSource, Runner.LocalPlayer);
        }
    }

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

        // LocalPlayerの場合のみカメラをセットアップ
        if (Object.HasInputAuthority)
        {
            LocalInstance = this;
            CameraFollower cameraFollower = FindFirstObjectByType<CameraFollower>();
            if (cameraFollower != null)
            {
                cameraFollower.SetTarget(transform);
            }
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (Object.HasInputAuthority && LocalInstance == this)
        {
            LocalInstance = null;
        }
    }

    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterController>();
        _forward = transform.forward;
        _material = GetComponentInChildren<MeshRenderer>().material;
    }

    private void Update()
    {
        if (Object.HasInputAuthority && Input.GetKeyDown(KeyCode.R))
        {
            RPC_SendMessage("Hey Mate!");
        }
    }

    public override void Render()
    {
        foreach (var change in _changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(spawnedProjectile):
                    _material.color = Color.white;
                    break;
            }
        }
        _material.color = Color.Lerp(_material.color, Color.blue, Time.deltaTime);
    }

    /*
    Player.FixedUpdateNetwork() は **Unity の FixedUpdate() ではなく、Fusion の「シミュレーショ
      ン・ティックごと」**に呼ばれるメソッドです。
      ざっくり言うと：
                                                                                                  
      - タイミング: ネットワークの ティック（TickRate）ごと
      - 呼ばれる側:
          - 入力権限のあるクライアント（予測）
          - サーバー/ホスト（権威計算）
      - 順序感: OnInput で集めた入力 → FixedUpdateNetwork でシミュレーション
                                                                                                  
      なのでフレームに1回とは限らず、1フレームで複数回/0回になることもあります（追いつき/間引
      き）。
      このため 入力の向き変換は OnInput 側でやるのが安定します。
    */
    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            data.direction.Normalize();

            _cc.Move(5 * data.direction * Runner.DeltaTime);

            if (data.direction.sqrMagnitude > 0)
                _forward = data.direction;

            if (HasStateAuthority && delay.ExpiredOrNotRunning(Runner))
            {
                if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON0))
                {
                    delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                    Runner.Spawn(
                        _prefabBall,
                        transform.position + _forward,
                        Quaternion.LookRotation(_forward),
                        Object.InputAuthority,
                        (runner, o) =>
                        {
                            // Initialize the Ball before synchronizing it
                            o.GetComponent<Ball>().Init();
                        }
                    );
                    spawnedProjectile = !spawnedProjectile;
                }
            }
        }
    }
}
