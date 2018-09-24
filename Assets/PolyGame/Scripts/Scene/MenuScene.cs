using Experiments;

public class MenuScene : GameScene.IScene
{
    Background background;
    Experiments.MenuPanel menuPanel;
    PuzzleGroupViewPanel puzzleGroupViewPanel;

	public void Start()
    {
        background = FUI.Instance.OpenPanel<Background>();
        menuPanel = FUI.Instance.OpenPanel<Experiments.MenuPanel>();
        puzzleGroupViewPanel = FUI.Instance.OpenPanel<PuzzleGroupViewPanel>();
        Puzzle.RetakeExpiredSnapshot();
	}
	
	public void OnDestroy()
    {
        background.Close();
        menuPanel.Close();
        puzzleGroupViewPanel.Close();
	}
}
