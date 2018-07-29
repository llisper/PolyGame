using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MenuPanel : Panel
{
    Panel selectedPanel;

    public override bool Persistent { get { return true; } }

    void Awake()
    {
        var artCollectionPanel = UI.Instance.OpenPanel<ArtCollectionPanel>(UILayer.Base);
        selectedPanel = artCollectionPanel;
        var myWorksPanel = UI.Instance.OpenPanel<MyWorksPanel>(UILayer.Base);
        myWorksPanel.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        selectedPanel.gameObject.SetActive(true);
    }

    void OnDisable()
    {
        selectedPanel.gameObject.SetActive(false);
    }

    void SwitchPanel<T>() where T : Panel
    {
        Panel panel = UI.Instance.GetPanel<T>();
        if (selectedPanel != panel)
        {
            selectedPanel.gameObject.SetActive(false);
            selectedPanel = panel;
        }
        selectedPanel.gameObject.SetActive(true);
    }

    public void OnGalleryClicked(Button button)
    {
        SwitchPanel<ArtCollectionPanel>();
    }

    public void OnMyWorksClicked(Button button)
    {
        SwitchPanel<MyWorksPanel>();
    }
}
