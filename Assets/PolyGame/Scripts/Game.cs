using UnityEngine;

public class Game : MonoBehaviour
{
    public static Game Instance;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start ()
    {
        Puzzle.Start("Bill");
	}
}
