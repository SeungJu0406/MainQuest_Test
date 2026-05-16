using NSJ_MVVM;
using TMPro;
using UnityEngine;

public class CountDownView : BaseView
{
   private GameObject _countdownPanel; // 5초 전 팝업 패널
   private TMP_Text _countdownText;    // 팝업 내 카운트다운 숫자


    private bool _countingDown;
    private float _countdownValue;
    protected override void InitGetUI()
    {
        _countdownPanel = GetUI("CountdownPanel");
        _countdownText = GetUI<TMP_Text>("CountdownText");
    }
    protected override void InitAwake()
    {
      
    }


    protected override void InitStart()
    {
        HideCountdown();
    }

    protected override void SubscribeEvents()
    {
        GameManager.OnCountdownStarted += StartCountdown;
        GameManager.OnRoundEnded += HideCountdown;
    }

    private void OnDestroy()
    {
        GameManager.OnCountdownStarted -= StartCountdown;
        GameManager.OnRoundEnded -= HideCountdown;
    }
    private void Update()
    {

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
}
