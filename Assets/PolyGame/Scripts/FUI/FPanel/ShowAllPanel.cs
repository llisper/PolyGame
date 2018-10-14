using FairyGUI;

namespace UI
{
    public class ShowAllPanel : FPanel
    {
        GList scrollview;
        GTextField title;

        protected override void OnInit()
        {
            scrollview = component.GetChild("scrollview") as GList;
            scrollview.onClickItem.Add(OnClickItem);

            var backBtn = component.GetChild("backBtn") as GButton;
            backBtn.onClick.Add(OnBackClick);

            title = component.GetChild("title") as GTextField;
        }

        public void Init(ArtCollection.Group group)
        {
            title.text = I18n.Get(group.name);
            for (int i = 0; i < group.items.Count; ++i)
            {
                var item = scrollview.AddItemFromPool() as PuzzleItem;
                item.Init(group.items[i].name);
            }
        }

        void OnClickItem(EventContext eventContext)
        {
            var item = (PuzzleItem)eventContext.data;
            item.Start();
        }

        void OnBackClick(EventContext eventContext)
        {
            GameScene.Current<MenuScene>().ShowPage<ArtCollectionPanel>();
        }
    }
}
