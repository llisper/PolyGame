using System.IO;
using System.Linq;
using System.Collections.Generic;
using FairyGUI;

namespace UI
{
    public class MyWorksPanel : FPanel
    {
        GList scrollview;
        HashSet<string> items = new HashSet<string>();

        protected override void OnInit()
        {
            scrollview = component.GetChild("scrollview") as GList;
            scrollview.onClickItem.Add(OnClickItem);

            UpdateItems();
            GameEvent.Instance.Subscribe(GameEvent.ReloadItem, OnReloadItem);
        }

        protected override void OnDispose()
        {
            GameEvent.Instance.Unsubscribe(GameEvent.ReloadItem, OnReloadItem);
            base.OnDispose();
        }

        bool UpdateItems()
        {
            string[] savePaths = Directory
                .EnumerateDirectories(Paths.Saves)
                .Where(p => !items.Contains(Path.GetFileName(p)))
                .OrderBy(p => File.GetLastAccessTimeUtc(p))
                .ToArray();

            for (int i = 0; i < savePaths.Length; ++i)
            {
                string name = Path.GetFileName(savePaths[i]);
                items.Add(name);

                var item = scrollview.AddItemFromPool() as PuzzleItem;
                scrollview.SetChildIndex(item, 0);
                item.Init(name);
            }

            return savePaths.Length > 0;
        }

        void OnClickItem(EventContext eventContext)
        {
            var item = (PuzzleItem)eventContext.data;
            item.Start();
        }

        void OnReloadItem(int e, object[] p)
        {
            UpdateItems();
            string graphName = (string)p[0];
            var item = (PuzzleItem)scrollview.GetChild(graphName);
            if (null != item)
            {
                item.Load();
                scrollview.SetChildIndex(item, 0);
            }
        }
    }
}
