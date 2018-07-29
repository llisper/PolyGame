
public class MenuScene : GameScene.IScene
{
    MenuPanel menuPanel;

	public void Start()
    {
        UI.Instance.OpenPanel<BackgroundPanel>(UILayer.Background);	
        menuPanel = UI.Instance.OpenPanel<MenuPanel>(UILayer.Menu);	
        Puzzle.RetakeExpiredSnapshot();
	}
	
	public void OnDestroy()
    {
        menuPanel.gameObject.SetActive(false);
	}
}
