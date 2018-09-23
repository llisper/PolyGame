using Experiments;

public class MenuScene : GameScene.IScene
{
    Background background;
    Experiments.MenuPanel menuPanel;

	public void Start()
    {
        background = FUI.Instance.OpenPanel<Background>();
        menuPanel = FUI.Instance.OpenPanel<Experiments.MenuPanel>();
        Puzzle.RetakeExpiredSnapshot();
	}
	
	public void OnDestroy()
    {
        background.View.visible = false;
        menuPanel.View.visible = false;
	}
}
