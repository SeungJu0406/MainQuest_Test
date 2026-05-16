using NSJ_MVVM;
using UnityEngine;
using UnityEngine.UI;

public class StartView : BaseView
{
    private Button _startButton;

    protected override void InitAwake()
    {
       
    }

    protected override void InitGetUI()
    {
       _startButton = GetUI<Button>("StartButton");
    }

    protected override void InitStart()
    {
        _startButton.gameObject.SetActive(false);
    }

    protected override void SubscribeEvents()
    {
        _startButton.onClick.AddListener(StartGame);
    }

    private void StartGame()
    {
        Manager.GameManager.StartRound();
        _startButton.gameObject.SetActive(false);
    }

    public void ActivateButton(bool active)
    {
        _startButton.gameObject.SetActive(active);
        
    }
}
