using UnityEngine;

public interface IUsable
{
    // Called when the item is held and the user presses the 'Use' key (e.g. Mouse Click)
    public void Use(Interactor interactor);
}
