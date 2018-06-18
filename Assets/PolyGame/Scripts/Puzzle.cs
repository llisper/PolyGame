using UnityEngine;
using System.Collections.Generic;

public class Puzzle : MonoBehaviour
{
    public static void Start(string puzzleName)
    {
        GameScene.LoadScene("Puzzle", (c, n) => 
        {
            var go = new GameObject("Puzzle_" + puzzleName);
            var puzzle = go.AddComponent<Puzzle>();
            puzzle.Run(puzzleName);
        });
    }

    const float ScrambleRadius = 50f;
    
    string puzzleName;
    PolyGraphBehaviour puzzleObject;
    DebrisMoveContainer debrisMoveContainer;
    Dictionary<GameObject, Vector3> positionMap = new Dictionary<GameObject, Vector3>();
    bool[] finished;

    void Awake()
    {
        var go = new GameObject("DebrisMoveContainer");
        go.transform.SetParent(transform);
        debrisMoveContainer = go.AddComponent<DebrisMoveContainer>();

        PuzzleTouch.onObjPicked += OnObjPicked;
        PuzzleTouch.onObjMove += OnObjMove;
        PuzzleTouch.onObjReleased += OnObjReleased;
    }

    void OnDestroy()
    {
        PuzzleTouch.onObjPicked -= OnObjPicked;
        PuzzleTouch.onObjMove -= OnObjMove;
        PuzzleTouch.onObjReleased -= OnObjReleased;
    }

    void Run(string puzzleName)
    {
        this.puzzleName = puzzleName;
        puzzleObject = Load();

        // TODO: if there is a save, load the save, or else start anew
        StartNew();

        PuzzleCamera.Instance.Init(puzzleObject.size);
    }

    void StartNew()
    {
        Scramble();
    }

    PolyGraphBehaviour Load()
    {
        var prefab = Resources.Load(string.Format("{0}/{1}/{1}", Paths.Artworks, puzzleName));
        var go = (GameObject)Instantiate(prefab);
        go.transform.SetParent(transform);

        for (int i = 0; i < go.transform.childCount; ++i)
        {
            var child = go.transform.GetChild(i);
            Vector3 pos = child.localPosition;
            positionMap.Add(child.gameObject, pos);
            child.localPosition = ArrangeDepth(i, pos);
        }
        finished = new bool[go.transform.childCount];
        return go.GetComponent<PolyGraphBehaviour>();
    }

    Vector3 ArrangeDepth(int i, Vector3 pos)
    {
        pos.z = -i * 0.1f;
        return pos;
    }

    void Scramble()
    {
        for (int i = 0; i < puzzleObject.transform.childCount; ++i)
        {
            var child = puzzleObject.transform.GetChild(i);
            child.localPosition += (Vector3)(Random.insideUnitCircle * ScrambleRadius);
        }
    }

    public float moveSpeed = 15f;

    void OnObjMove(Vector2 screenCurrent)
    {
        if (null == debrisMoveContainer.Target)
            return;

        Vector3 pos = debrisMoveContainer.transform.position;
        Vector3 newPos = PuzzleCamera.Main.ScreenToWorldPoint(screenCurrent);
        newPos.z = pos.z;
        pos = Vector3.Lerp(pos, newPos, Time.deltaTime * moveSpeed);
        debrisMoveContainer.transform.position = pos;
    }

    void OnObjPicked(Transform objPicked)
    {
        Vector3 screenPos = PuzzleTouch.Instance.MainFinger.ScreenPosition;
        debrisMoveContainer.transform.position = PuzzleCamera.Main.ScreenToWorldPoint(screenPos);
        debrisMoveContainer.Target = objPicked;
    }

    void OnObjReleased(Transform objPicked)
    {
        debrisMoveContainer.Target = null;
    }
}
