using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private QuestionData _questionData;
    [SerializeField] private OXZone _oZone;
    [SerializeField] private OXZone _xZone;
    [SerializeField] private StartView _startView;
    [SerializeField] private int _roundTime = 15;  // 라운드당 제한 시간(초)

    // 모든 클라이언트에 동기화되는 타이머 (마스터가 감산)
    [Networked] public int Timer { get; private set; }
    // 현재 문제 인덱스 — 마스터 교체 시 새 마스터가 이어받을 수 있도록 동기화
    [Networked] private int CurrentQuestionIndex { get; set; }

    // UI(MonoBehaviour)가 구독하는 정적 이벤트
    public static event Action<string> OnQuestionPresented;  // 문제 텍스트
    public static event Action<string> OnQuestionPresentedPersonal;  // 문제 텍스트 (개인용, 필요 시 추가)
    public static event Action<int> OnCountdownStarted;   // 5초 전 카운트다운 팝업
    public static event Action OnRoundEnded;         // 라운드 종료
    public static event Action<bool> OnResultReceived;     // 내 정답 여부

    private bool _countdownSent;   // 5초 RPC 중복 방지
    private bool _roundEndSent;    // 0초 RPC 중복 방지

    // 마스터가 각 클라이언트의 제출을 수집
    private readonly Dictionary<PlayerRef, bool> _submittedAnswers = new();
    private bool _collectingAnswers;
    private float _collectTimer;
    private const float CollectTimeout = 2f;  // 전원 미제출 시 강제 마감 시간

    private bool _correctIsO;    // 현재 문제 정답 (CurrentQuestionIndex로 언제든 재구성 가능)
    private bool _wasMaster;     // 이전 틱에 마스터였는지 — 마스터 교체 감지용

    QuestionData.Question _curQuation => _questionData.Questions[CurrentQuestionIndex % _questionData.Questions.Length];

    public override void Spawned()
    {
        Manager.SetGameManager(this);

        // 게임 진행중에 들어왔을 떄 현재 문제 전광판에 띄우기
        if (Timer > 0)
        {
            OnQuestionPresentedPersonal?.Invoke(_curQuation.Text);
        }

        if (!Runner.IsSharedModeMasterClient) return;

        _wasMaster = true;
        _startView.ActivateButton(true);
    }

    // 새 마스터가 됐을 때 동기화된 [Networked] 값으로 로컬 상태 재구성
    private void OnBecameMaster()
    {
        // CurrentQuestionIndex, Timer는 [Networked]로 이미 동기화되어 있음
        // _correctIsO만 인덱스로 재파생
        if (_questionData != null && _questionData.Questions.Length > 0)
            _correctIsO = _questionData.Questions[CurrentQuestionIndex % _questionData.Questions.Length].CorrectIsO;

        // Timer 상태에 맞게 플래그 복원 (_tickAccum은 최대 1초 오차 허용)
        _countdownSent = Timer <= 5;
        _roundEndSent = Timer <= 0;
        _tickAccum = 0f;

        // 라운드가 이미 종료된 상태면 시작 버튼 표시
        if (Timer <= 0)
            _startView.ActivateButton(true);
    }

    public void StartRound()
    {
        if (_questionData == null || _questionData.Questions.Length == 0) return;

        var q = _questionData.Questions[CurrentQuestionIndex % _questionData.Questions.Length];
        _correctIsO = q.CorrectIsO;
        Timer = _roundTime;
        _countdownSent = false;
        _roundEndSent = false;
        _submittedAnswers.Clear();
        _collectingAnswers = false;

        RPC_PresentQuestion(q.Text);
    }

    public override void FixedUpdateNetwork()
    {

        bool isMaster = Runner.IsSharedModeMasterClient;

        // 마스터가 된 첫 틱 감지 → 로컬 상태 재구성
        if (isMaster && !_wasMaster)
            OnBecameMaster();
        _wasMaster = isMaster;

        if (!isMaster) return;

        // 답변 수집 중이면 타임아웃 처리
        if (_collectingAnswers)
        {
            _collectTimer += Runner.DeltaTime;
            if (_collectTimer >= CollectTimeout)
                ProcessResults();
            return;
        }

        if (Timer <= 0) return;

        // 1초마다 감산 (FixedUpdateNetwork는 틱 단위라 누적 후 처리)
        _tickAccum += Runner.DeltaTime;
        if (_tickAccum >= 1f)
        {
            _tickAccum -= 1f;
            Timer = Mathf.Max(0, Timer - 1);
        }

        // 5초 전: 카운트다운 UI 신호
        if (Timer <= 5 && !_roundEndSent)
        {
            // _countdownSent = true;
            RPC_ShowCountdown(Timer);
        }

        // 종료: 라운드 종료 신호
        if (Timer <= 0 && !_roundEndSent)
        {
            _roundEndSent = true;
            RPC_EndRound();
        }
    }

    private float _tickAccum;

    // ──────────────────────────────────────────
    // RPC: 마스터 → 전체
    // ──────────────────────────────────────────

    // 문제 제시 (마스터가 발사, 전체 수신)
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PresentQuestion(string question)
    {
        OnQuestionPresented?.Invoke(question);
    }

    // 5초 전 카운트다운 팝업 신호
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ShowCountdown(int count)
    {
        OnCountdownStarted?.Invoke(count);
    }

    // 라운드 종료: 각 클라이언트가 본인 위치로 정답 판정 후 제출
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_EndRound()
    {
        OnRoundEnded?.Invoke();

        // 본인 PlayerController 위치 조회 → O/X존 판정 → 마스터에 제출
        foreach (var netObj in Runner.GetAllNetworkObjects())
        {
            var pc = netObj.GetComponent<PlayerController>();
            if (pc == null || !pc.HasStateAuthority) continue;

            Vector2 pos = pc.transform.position;
            bool isO = _oZone != null && _oZone.Contains(pos);
            // X존이거나 어느 존도 아니면 false(X 선택)로 처리
            RPC_SubmitAnswer(isO);
            break;
        }
    }

    // 결과 발표 (마스터가 발사, 전체 수신)
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_AnnounceResult(PlayerRef player, NetworkBool correct)
    {
        // 본인 결과만 처리
        if (Runner.LocalPlayer != player) return;

        OnResultReceived?.Invoke(correct);
    }

    // ──────────────────────────────────────────
    // RPC: 클라이언트 → 마스터
    // ──────────────────────────────────────────

    // 클라이언트가 본인의 정답(O존 여부)을 마스터에 제출
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SubmitAnswer(NetworkBool isO, RpcInfo info = default)
    {
        if (_submittedAnswers.ContainsKey(info.Source)) return;

        _submittedAnswers[info.Source] = isO;

        // 첫 제출 시 수집 타임아웃 시작
        if (!_collectingAnswers)
        {
            _collectingAnswers = true;
            _collectTimer = 0f;
        }

        // 활성 플레이어 전원 제출 완료 시 즉시 처리
        int activeCount = 0;
        foreach (var _ in Runner.ActivePlayers) activeCount++;
        if (_submittedAnswers.Count >= activeCount)
            ProcessResults();
    }

    // 수집된 답변으로 정답/오답 판정 후 각 클라이언트에 전달
    private void ProcessResults()
    {
        _collectingAnswers = false;

        foreach (var kvp in _submittedAnswers)
        {
            bool correct = kvp.Value == _correctIsO;
            RPC_AnnounceResult(kvp.Key, correct);
        }

        // 다음 문제 인덱스 증가 — [Networked]라 새 마스터도 이어받음
        CurrentQuestionIndex++;

        // 마스터 클라이언트의 재시작 버튼 클릭 후 시작
        _startView.ActivateButton(true);
    }
}
