
public class PuzzleScene : GameScene.IScene
{
	public void Start()
    {
        UI.Instance.OpenPanel<PuzzlePanel>();	
	}
	
	public void OnDestroy()
    {
		
	}
}
