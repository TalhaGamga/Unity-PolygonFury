using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/InputReaderSO/SquareInputReaderSO")]
public class PlayerInputReaderSO : InputReaderBaseSO<PlayerInputReader>
{
    public override PlayerInputReader GetInputReader()
    {
        if (InputReader == null)
        {
            InputReader = new PlayerInputReader();
        }

        return InputReader;
    }
}