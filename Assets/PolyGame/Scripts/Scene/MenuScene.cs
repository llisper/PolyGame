﻿using Experiments;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

public class MenuScene : GameScene.IScene
{
    static bool firstTime = true;
    static Background background;
    static Experiments.MenuPanel menuPanel;
    static List<FPanel> pages = new List<FPanel>();
    static int pageIndex;

    bool switchingPage;

	public void Start()
    {
        if (firstTime)
        {
            background = FUI.Instance.OpenPanel<Background>();
            menuPanel = FUI.Instance.OpenPanel<Experiments.MenuPanel>();
            AddPage<Experiments.ArtCollectionPanel>();
            AddPage<ShowAllPanel>();
            firstTime = false;
        }
        else
        {
            background.View.visible = true;
            menuPanel.View.visible = true;
            pages[pageIndex].View.visible = true;
        }
        Puzzle.RetakeExpiredSnapshot();
	}
	
	public void OnDestroy()
    {
        background.Close();
        menuPanel.Close();
        pages[pageIndex].Close();
	}

    public void ShowPage<T>(Action<T> onPageShow = null) where T : FPanel
    {
        if (!switchingPage)
        {
            int i = pages.FindIndex(v => v.GetType() == typeof(T));
            if (i >= 0 && i != pageIndex)
                SwitchingPage(i, onPageShow).WrapErrors();
        }
    }

    void AddPage<T>() where T : FPanel
    {
        pages.Add(FUI.Instance.OpenPanel<T>());
        if (pages.Count > 1)
            pages[pages.Count - 1].Close();
    }

    async Task SwitchingPage(int to, Delegate onPageShow)
    {
        switchingPage = true;
        await ScreenFader.AsyncFade(true);

        pages[pageIndex].Close();
        pages[to].View.visible = true;
        pageIndex = to;

        if (null != onPageShow)
            onPageShow.DynamicInvoke(pages[to]);

        await ScreenFader.AsyncFade(false);
        switchingPage = false;
    }
}
