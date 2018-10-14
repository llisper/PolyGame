using FairyGUI;

namespace Experiments
{
    public class MenuPanel : FPanel
    {
        protected override void OnInit()
        {
            var btn = component.GetChild("gallery") as GButton;
            btn.onClick.Add(OnGalleryClick);

            btn = component.GetChild("collection") as GButton;
            btn.onClick.Add(OnMyWorksClick);
        }

        void OnGalleryClick(EventContext eventContext)
        {
            GameScene.Current<MenuScene>().ShowPage<ArtCollectionPanel>();
        }

        void OnMyWorksClick(EventContext eventContext)
        {
            GameScene.Current<MenuScene>().ShowPage<MyWorksPanel>();
        }
    }
}
