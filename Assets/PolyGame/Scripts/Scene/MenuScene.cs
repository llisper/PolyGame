using Experiments;

public class MenuScene : GameScene.IScene
{
    Background background;
    Experiments.MenuPanel menuPanel;
    Experiments.ArtCollectionPanel artCollectionPanel;

	public void Start()
    {
        background = FUI.Instance.OpenPanel<Background>();
        menuPanel = FUI.Instance.OpenPanel<Experiments.MenuPanel>();
        artCollectionPanel = FUI.Instance.OpenPanel<Experiments.ArtCollectionPanel>();
        Puzzle.RetakeExpiredSnapshot();
	}
	
	public void OnDestroy()
    {
        background.Close();
        menuPanel.Close();
        artCollectionPanel.Close();
	}
}
