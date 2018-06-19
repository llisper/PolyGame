using UnityEngine;

public class Game : MonoBehaviour
{
    public static Game Instance;

    public GameObject testPuzzle;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start ()
    {
        if (null != testPuzzle)
            Puzzle.Start(testPuzzle.name);
	}
}
