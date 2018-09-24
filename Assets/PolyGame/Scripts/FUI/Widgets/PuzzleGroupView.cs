using FairyGUI;
using FairyGUI.Utils;

namespace Experiments
{
    public class PuzzleGroupView : GComponent
    {
        const int itemLimitHalf = 5;

        GTextField title;
        GList scrollview;

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            title = GetChild("title") as GTextField;
            scrollview = GetChild("scrollview") as GList;
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public void Init(ArtCollection.Group group)
        {
            var items = group.items;
            int showCount = items.Count;
            int itemLimit = itemLimitHalf * 2;
            if (items.Count > itemLimit)
                showCount = itemLimit - 1;

            for (int i = 0; i < showCount; ++i)
            {
                var item = scrollview.AddItemFromPool() as PuzzleItem;
                item.Init(items[i].name);
            }

            if (showCount < items.Count)
            {
                var item = scrollview.AddItemFromPool() as PuzzleItem;
                item.InitAsShowAll();
            }
            title.text = I18n.Get(group.name);
        }
    }
}
