using System.Threading.Tasks;

public class PuzzleHintSystem : Singleton<PuzzleHintSystem>
{
    int hintCount;

    protected override async Task AsyncInit()
    {
        // TODO: hint count should sync from server
        hintCount = 10;
    }

    public int HintCount { get { return hintCount; } }

    public bool UseHint()
    {
        var puzzle = Puzzle.Current;
        if (null != puzzle && 
            !puzzle.Finished &&
            puzzle.InState<PuzzleNormalState>() && 
            hintCount > 0)
        {
            --hintCount;
            GameEvent.Instance.Fire(GameEvent.UseHint);
            return true;
        }
        return false;
    }
}
