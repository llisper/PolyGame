using UnityEngine.UI;

public class MenuPanel : Panel
{
    public Dropdown dropdown;

    public void OnStartClicked()
    {
        string text= dropdown.captionText.text;
        Puzzle.Start(text);
    }
}
