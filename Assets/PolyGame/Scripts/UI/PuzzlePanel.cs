
public class PuzzlePanel : Panel
{
    public void OnBackClicked()
    {
        GameScene.LoadScene<MenuScene>().WrapErrors();
    }
}
