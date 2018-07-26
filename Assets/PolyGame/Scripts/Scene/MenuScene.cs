
public class MenuScene : GameScene.IScene
{
    BackgroundPanel background;
    ArtCollectionPanel artCollectionPanel;

	public void Start()
    {
        background = UI.Instance.OpenPanel<BackgroundPanel>();	
        artCollectionPanel = UI.Instance.OpenPanel<ArtCollectionPanel>();
        UI.Instance.OpenPanel<MenuPanel>();	
        Puzzle.RetakeExpiredSnapshot();
	}
	
	public void OnDestroy()
    {
        background.gameObject.SetActive(false);
        artCollectionPanel.gameObject.SetActive(false);
	}
}
