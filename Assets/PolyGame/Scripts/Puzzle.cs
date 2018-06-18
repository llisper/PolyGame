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
    Dictionary<GameObject, Vector3> positionMap = new Dictionary<GameObject, Vector3>();
    bool[] finished;

    void Awake()
    {
        PuzzleTouch.onFingerDrag += OnObjMove;
        PuzzleTouch.onObjPicked += OnObjPicked;
        PuzzleTouch.onObjReleased += OnObjReleased;
    }

    void OnDestroy()
    {
        PuzzleTouch.onFingerDrag -= OnObjMove;
        PuzzleTouch.onObjPicked -= OnObjPicked;
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

    void OnObjMove(Vector2 screenCurrent, Vector2 screenDelta, Transform objPicked)
    {
        if (null == objPicked)
            return;

        Vector3 pos = objPicked.transform.position;
        Vector3 newPos = PuzzleCamera.Main.ScreenToWorldPoint(screenCurrent);
        pos.x = newPos.x;
        pos.y = newPos.y;
        objPicked.transform.position = pos;
    }

    void OnObjPicked(Transform objPicked)
    {

    }

    void OnObjReleased(Transform objPicked)
    {

    }
}
