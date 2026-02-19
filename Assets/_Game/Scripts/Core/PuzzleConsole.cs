using UnityEngine;

public class PuzzleConsole : MonoBehaviour, IInteractable
{
    [Header("Configuration")]
    public string PuzzleID; 
    public GameObject UIPrefab;
    public int ScoreAward = 100;

    private GameObject _spawnedUI;
    private BasePuzzleUI _puzzleLogic;
    private bool _isSolved => GameManager.Instance.IsPuzzleSolved(PuzzleID);

    public string InteractionPrompt => _isSolved ? "System Offline (Solved)" : "Press E to Interact";

    public bool Interact(Interactor interactor)
    {
        if (_spawnedUI == null)
        {
            _spawnedUI = Instantiate(UIPrefab, UIManager.Instance.PuzzleParent);
            _puzzleLogic = _spawnedUI.GetComponentInChildren<BasePuzzleUI>(true);
            _puzzleLogic.Initialize(this);
        }

        UIManager.Instance.OpenPuzzle(_spawnedUI);

        if (_isSolved) _puzzleLogic.TriggerSolvedState();

        return true;
    }

    // Called by the UI when the player wins
    public void MarkAsSolved()
    {
        if (_isSolved) return;
        GameManager.Instance.AddScore(ScoreAward, PuzzleID);
        // Add Green Light / Door Open logic here
    }
}