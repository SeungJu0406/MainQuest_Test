using NSJ_MVVM;
using TMPro;
using UnityEngine;

public class QuestionView : BaseView
{
    [SerializeField] string _corretAnswer;              // 정답 (디자이너가 입력)
    [SerializeField] string _wrongAnswer;              // 오답 (디자이너가 입력)

    private TMP_Text _questionText;     // 문제 표시
    private TMP_Text _resultText;       // 정답/오답 표시
    protected override void InitAwake()
    {
        _resultText.text = string.Empty;
    }

    protected override void InitGetUI()
    {
        _questionText = GetUI<TMP_Text>("QuestionText");
        _resultText = GetUI<TMP_Text>("ResultText");
    }

    protected override void InitStart()
    {

    }

    protected override void SubscribeEvents()
    {
        GameManager.OnQuestionPresented += ShowQuestion;
        GameManager.OnResultReceived += ShowResult;
    }

    private void OnDestroy()
    {
        GameManager.OnQuestionPresented -= ShowQuestion;
        GameManager.OnResultReceived -= ShowResult;
    }

    private void ShowQuestion(string question)
    {
        if (_questionText != null)
            _questionText.text = question;
        if (_resultText != null)
            _resultText.text = string.Empty;
    }

    private void ShowResult(bool correct)
    {
        if (_resultText == null) return;
        _resultText.text = correct ? _corretAnswer : _wrongAnswer;
    }
}
