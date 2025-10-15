using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/InputReaderSO/TriangleBossInputReaderSO")]
public class TriangleBossInputReaderSO : InputReaderBaseSO<TriangleBossInputReader>
{
    public override TriangleBossInputReader GetInputReader()
    {
        if (InputReader == null)
        {
            InputReader = new TriangleBossInputReader();
        }

        return InputReader;
    }
}