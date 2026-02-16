using UnityEngine;

public class XORPuzzle : PuzzleBase
{
    [Header("Puzzle Components")]
    public GameObject LayerA;
    public GameObject LayerB;
    
    // In this puzzle, the solution is obtained by keying in a code or finding an object revealed by the layers.
    // The layers themselves are just visual tools.
    
    [Header("Solution Data")]
    public string SolutionCode = "1234";
    [TextArea] public string[] Clues; // The specific "Puzzle questions" or hints for this puzzle.

    public void AttemptSolve(string code)
    {
        if (code == SolutionCode)
        {
            OnPuzzleSolved();
        }
        else
        {
            Debug.Log("Incorrect Code");
        } 
    }
}
