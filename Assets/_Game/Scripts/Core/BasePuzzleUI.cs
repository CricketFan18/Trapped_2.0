using UnityEngine;

public abstract class BasePuzzleUI : MonoBehaviour
{
    private PuzzleConsole _console;

    public void Initialize(PuzzleConsole console)
    {
        _console = console;
        OnSetup();
    }

    protected virtual void OnSetup() { }

    protected void CompletePuzzle()
    {
        _console.MarkAsSolved();
        TriggerSolvedState();
    }

    public void TriggerSolvedState()
    {
        OnShowSolvedState();
    }

    protected abstract void OnShowSolvedState();
}