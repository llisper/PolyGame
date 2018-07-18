
public class PuzzleScene : GameScene.IScene
{
	public void Start()
    {
        // UI.Instance.OpenPanel<PuzzlePanel>();	
	}
	
	public void OnDestroy()
    {
		var puzzle = Puzzle.Current;		
		if (null != puzzle)
		{
			puzzle.SaveProgress();
            PuzzleSnapshotOneOff.Take(puzzle.PuzzleName, puzzle.FinishedFlags);
		}
	}
}
