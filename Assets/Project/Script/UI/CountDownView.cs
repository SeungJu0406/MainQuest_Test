using NSJ_MVVM;
using TMPro;
using UnityEngine;

public class CountDownView : BaseView
{
   private GameObject _countdownPanel; // 5초 전 팝업 패널
   private TMP_Text _countdownText;    // 팝업 내 카운트다운 숫자

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


    private void StartCountdown(int count)
    {
        _countdownText.text = count.ToString();
        if (_countdownPanel != null) _countdownPanel.SetActive(true);
    }

    private void HideCountdown()
    {
        if (_countdownPanel != null) _countdownPanel.SetActive(false);
    }
}
