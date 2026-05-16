using UnityEngine;

[CreateAssetMenu(fileName = "QuestionData", menuName = "OX Quiz/Question Data")]
public class QuestionData : ScriptableObject
{
    [System.Serializable]
    public struct Question
    {
        public string Text;      // 문제 텍스트
        public bool CorrectIsO;  // true = O가 정답, false = X가 정답
    }

    public Question[] Questions;
}
