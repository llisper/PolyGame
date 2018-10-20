
public class GameEvent : EvSystem<GameEvent>
{
    public const int UpdateProgress = 0; // level, name, perc
    public const int PuzzleFinished = 1;
    public const int ReloadItem = 2;
}