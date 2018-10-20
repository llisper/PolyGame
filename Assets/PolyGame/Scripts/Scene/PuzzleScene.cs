using UI;

public class PuzzleScene : GameScene.IScene
{
	public void Start()
    {
        // UI.Instance.OpenPanel<PuzzlePanel>();	
        FUI.Instance.OpenPanel<PuzzlePanel>();
	}
	
	public void OnDestroy()
    {
        FUI.Instance.ClosePanel<PuzzlePanel>();
		var puzzle = Puzzle.Current;		
		if (null != puzzle)
		{
			puzzle.SaveProgress();
            PuzzleSnapshotOneOff.Take(puzzle.PuzzleName, puzzle.FinishedFlags);
            GameEvent.Instance.Fire(GameEvent.ReloadItem, puzzle.PuzzleName);
		}
	}
}
