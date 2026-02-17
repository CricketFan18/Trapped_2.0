using UnityEngine;

public abstract class PuzzleBase : MonoBehaviour
{
    [Header("Puzzle Info")]
    public string PuzzleName;
    [TextArea] public string PuzzleDescription;

    [Header("Puzzle Status")]
    public bool IsSolved = false;

    // Called when the puzzle is solved
    public virtual void OnPuzzleSolved()
    {
        IsSolved = true;
        Debug.Log($"Puzzle Solved: {PuzzleName}");
        // You can convert this to an event or call GameManager
    }

    // Optional: Reset the puzzle
    public virtual void ResetPuzzle()
    {
        IsSolved = false;
    }
}
