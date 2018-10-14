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
            scrollview.onClickItem.Add(OnClickItem);
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
                item.SetMask(SelectMask(i, items.Count));
            }

            if (showCount < items.Count)
            {
                var item = scrollview.AddItemFromPool() as PuzzleItem;
                item.InitAsShowAll(group);
                item.SetMask(SelectMask(showCount, items.Count));
            }
            title.text = I18n.Get(group.name);
        }

        int SelectMask(int i, int itemCount)
        {
            if (i % 2 == 0)
            {
                if (i == 0)
                    return 0;
                else if (i == itemCount - 1)
                    return 2;
                else 
                    return 1;
            }
            else 
            {
                if (i == 1)
                    return 3;
                else if (i == itemCount - 1)
                    return 5;
                else
                    return 4;
            }

            // int half = itemLimitHalf;
            // int step = Mathf.Min(1, i / half) * 3;
            // i = i % half;

            // var mats = SnapshotMasks.Instance.materials;
            // if (i == 0)
            //     return 0 + step;
            // else if (i < half - 1)
            //     return 1 + step;
            // else
            //     return 2 + step;
        }

        void OnClickItem(EventContext eventContext)
        {
            var item = (PuzzleItem)eventContext.data;
            item.Start();
        }
    }
}
