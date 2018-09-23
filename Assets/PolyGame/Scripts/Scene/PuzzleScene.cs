using Experiments;

public class PuzzleScene : GameScene.IScene
{
	public void Start()
    {
        // UI.Instance.OpenPanel<PuzzlePanel>();	
        FUI.Instance.OpenPanel<Experiments.PuzzlePanel>();
	}
	
	public void OnDestroy()
    {
        FUI.Instance.ClosePanel<Experiments.PuzzlePanel>();
		var puzzle = Puzzle.Current;		
		if (null != puzzle)
		{
			puzzle.SaveProgress();
            PuzzleSnapshotOneOff.Take(puzzle.PuzzleName, puzzle.FinishedFlags);
		}
	}
}
