using System.IO;
using System.Text;

public class MenuScene : GameScene.IScene
{
    ArtCollectionPanel artCollectionPanel;

	public void Start()
    {
        artCollectionPanel = UI.Instance.OpenPanel<ArtCollectionPanel>();
        Puzzle.RetakeExpiredSnapshot();
	}
	
	public void OnDestroy()
    {
        artCollectionPanel.gameObject.SetActive(false);
	}
}
