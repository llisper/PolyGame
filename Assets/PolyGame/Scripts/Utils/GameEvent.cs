
public class GameEvent : EvSystem<GameEvent>
{
    public const int UpdateProgress = 0; // level, name, perc
    public const int PuzzleFinished = 1;
    public const int ReloadItem = 2;
    public const int UseHint = 3;
    public const int PuzzleState = 4;
}