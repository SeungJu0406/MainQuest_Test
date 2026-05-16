using TMPro;
using UnityEngine;

// GameManager의 정적 이벤트를 구독해서 UI를 구동하는 MonoBehaviour.
// NetworkBehaviour가 아닌 일반 컴포넌트로 유지한다.
public class GameUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _questionText;     // 문제 표시
    [SerializeField] private TMP_Text _timerText;        // 남은 시간
    [SerializeField] private GameObject _countdownPanel; // 5초 전 팝업 패널
    [SerializeField] private TMP_Text _countdownText;    // 팝업 내 카운트다운 숫자
    [SerializeField] private TMP_Text _resultText;       // 정답/오답 표시

    private bool _countingDown;
    private float _countdownValue;

    private void OnEnable()
    {
        GameManager.OnQuestionPresented += ShowQuestion;
        GameManager.OnCountdownStarted  += StartCountdown;
        GameManager.OnRoundEnded        += HideCountdown;
        GameManager.OnResultReceived    += ShowResult;
    }

    private void OnDisable()
    {
        GameManager.OnQuestionPresented -= ShowQuestion;
        GameManager.OnCountdownStarted  -= StartCountdown;
        GameManager.OnRoundEnded        -= HideCountdown;
        GameManager.OnResultReceived    -= ShowResult;
    }

    private void Update()
    {
        // 타이머 텍스트 갱신 (Manager.GameManager 폴링)
        var gm = Manager.GameManager;
        if (_timerText != null && gm != null)
            _timerText.text = gm.Timer.ToString();

        // 카운트다운 팝업 숫자 갱신
        if (_countingDown)
        {
            _countdownValue -= Time.deltaTime;
            if (_countdownValue <= 0f)
            {
                _countingDown = false;
                if (_countdownPanel != null) _countdownPanel.SetActive(false);
            }
            else if (_countdownText != null)
            {
                _countdownText.text = Mathf.CeilToInt(_countdownValue).ToString();
            }
        }
    }

    private void ShowQuestion(string question)
    {
        if (_questionText != null) _questionText.text = question;
        if (_resultText != null)   _resultText.text   = string.Empty;
    }

    private void StartCountdown()
    {
        _countingDown = true;
        _countdownValue = 5f;
        if (_countdownPanel != null) _countdownPanel.SetActive(true);
    }

    private void HideCountdown()
    {
        _countingDown = false;
        if (_countdownPanel != null) _countdownPanel.SetActive(false);
    }

    private void ShowResult(bool correct)
    {
        if (_resultText == null) return;
        _resultText.text = correct ? "정답!" : "오답!";
    }
}
