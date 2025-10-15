using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/InputReaderSO/SquareInputReaderSO")]
public class SquareInputReaderSO : InputReaderBaseSO<SquareInputReader>
{
    public override SquareInputReader GetInputReader()
    {
        if (InputReader == null)
        {
            InputReader = new SquareInputReader();
        }

        return InputReader;
    }
}