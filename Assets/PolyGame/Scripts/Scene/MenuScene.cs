
public class MenuScene : GameScene.IScene
{
	public void Start()
    {
        UI.Instance.OpenPanel<MenuPanel>();	
	}
	
	public void OnDestroy()
    {
		
	}
}
