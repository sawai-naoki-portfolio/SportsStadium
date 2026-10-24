using UnityEngine;

namespace SportsGame
{
    /// <summary>タイトルシーンの画面遷移(スタート → 種目選択 → 試合設定 → 試合開始)。</summary>
    public class TitleScreen : MonoBehaviour
    {
        TitleBackdrop backdrop;
        RectTransform root;
        GameObject startPanel, selectPanel, setupPanel;
        Sport chosen;

        void Start()
        {
            backdrop = TitleBackdrop.Create();
            root = UIKit.SafeRoot(UIKit.CreateCanvas("TitleUI", 5));
            startPanel = StartPanel.Build(root, ShowSelect);
            selectPanel = BuildSelect();
            Show(startPanel);
            AudioManager.SetAmbience(false);
            AudioManager.SetTitleMusic(true);
            AudioManager.Warmup(Sfx.Click, Sfx.Confirm, Sfx.Back);
        }

        GameObject BuildSelect() => SelectPanel.Build(root, ShowSetup, () => Show(startPanel), OpenOptions);

        void Show(GameObject panel)
        {
            foreach (var p in new[] { startPanel, selectPanel, setupPanel })
                if (p != null) p.SetActive(p == panel);
            if (panel != setupPanel) backdrop.Focus(-1);
            if (panel != null) StartCoroutine(UITween.Pop(panel.transform, 0.94f, 1f, 0.25f));
        }

        void ShowSelect() => Show(selectPanel);

        void ShowSetup(Sport s)
        {
            chosen = s;
            backdrop.Focus(System.Array.IndexOf(SportCatalog.All, s));
            if (setupPanel != null) Destroy(setupPanel);
            setupPanel = SetupPanel.Build(root, s, StartMatch, OpenHowTo, ShowSelect);
            Show(setupPanel);
        }

        void OpenHowTo()
        {
            setupPanel.SetActive(false);
            HowToPanel.Open(root, chosen, () => Show(setupPanel));
        }

        void OpenOptions()
        {
            selectPanel.SetActive(false);
            OptionsPanel.Open(root, ShowSelect, RebuildSelect);
        }

        /// <summary>戦績リセット後にカードの表示を更新する。</summary>
        void RebuildSelect()
        {
            Destroy(selectPanel);
            selectPanel = BuildSelect();
            selectPanel.SetActive(false);
        }

        void StartMatch()
        {
            GameSettings.Save();
            AudioManager.SetTitleMusic(false);
            SceneFlow.GoSport(chosen);
        }
    }
}
