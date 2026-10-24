using UnityEngine;

namespace SportsGame
{
    /// <summary>あそびかた(操作説明とルール)。</summary>
    public static class HowToPanel
    {
        public static ModalPanel Open(Transform parent, Sport s, System.Action onClose)
        {
            var modal = ModalPanel.Open(parent, null, new Vector2(1500f, 960f), 0.5f);
            var body = modal.Body;
            var title = UIKit.Label(body, "Title", "あそびかた - " + SportCatalog.Name(s), 56, UITheme.Gold);
            UIKit.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(1400f, 90f));
            var text = UIKit.Label(body, "Body", HowToTexts.For(s), 34, Color.white, TextAnchor.UpperLeft, false);
            text.lineSpacing = 1.15f;
            UIKit.Place(text.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -130f), new Vector2(1380f, 660f));
            var close = UIKit.Button(body, "Close", "とじる", UITheme.Blue, () =>
            {
                modal.Close();
                onClose();
            }, 44, Sfx.Back);
            UIKit.Place((RectTransform)close.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(460f, 110f));
            return modal;
        }
    }

    /// <summary>操作説明の本文。</summary>
    public static class HowToTexts
    {
        const string Keyboard = "・キーボード: WASD 移動 / J・K・L ボタン / Esc ポーズ";

        public static string For(Sport s)
        {
            switch (s)
            {
                case Sport.Basketball:
                    return "<color=#FFD24A>■ 1on1 ハーフコート</color>  CPUと1対1で点を取り合おう！\n" +
                           "・左側をドラッグ: 移動(ドリブル)\n" +
                           "・<b>シュート</b>: 長押しでジャンプ → ゲージが<color=#5CFF7A>緑</color>のときに離すと成功率アップ\n" +
                           "・ゴール付近でシュート → レイアップ / ダッシュ中ならダンク！\n" +
                           "・<b>クロス</b>: 切り返しで相手を抜く   ・<b>ダッシュ</b>: 押している間スピードアップ\n" +
                           "・守備時: <b>ブロック</b>(ジャンプ) / <b>スティール</b>(ボールを奪う)\n" +
                           "<color=#FFD24A>■ ルール</color>\n" +
                           "・3Pラインの外からは3点、内側は2点。ショットクロックは12秒\n" +
                           "・相手からボールを奪ったら、3Pラインの外に戻してから攻撃\n" + Keyboard;
                case Sport.Tennis:
                    return "<color=#FFD24A>■ シングルス</color>  手前のコートがあなたです\n" +
                           "・左側をドラッグ: 移動(黄色いリング = ボールの落下地点)\n" +
                           "・<b>トップスピン / スライス / ロブ</b>: ボールが届く前に押しておくと自動でスイング\n" +
                           "   早めに押すほど強い打球に。打つ瞬間のスティックの向きでコースを打ち分け\n" +
                           "   (左右 = コース, 上 = 深く, 下 = 短く)\n" +
                           "・<b>サーブ</b>: 1回目でトス → ボールが一番高いときにもう一度押す\n" +
                           "<color=#FFD24A>■ ルール</color>\n" +
                           "・0-15-30-40、デュース・アドバンテージ、タイブレークあり\n" +
                           "・サーブは対角のサービスボックスへ。2回失敗でダブルフォルト\n" + Keyboard;
                default:
                    return "<color=#FFD24A>■ ビーチバレー 2on2</color>  味方はCPUが操作します\n" +
                           "・左側をドラッグ: 移動(<color=#5CFF7A>緑のリング</color> = あなたが行くべき場所)\n" +
                           "・<b>レシーブ / トス</b>: ボールが来る前に押しておくと自動で触ります\n" +
                           "・<b>スパイク</b>: トスが上がったら押してジャンプ → ボールに合わせて自動で打つ\n" +
                           "   スティックの向きでコース指定。<b>レシーブ</b>ボタンならフェイント\n" +
                           "・<b>ブロック</b>: 相手のスパイクに合わせてネット際でジャンプ\n" +
                           "・<b>サーブ</b>: 1回目でトス → もう一度押して打つ\n" +
                           "<color=#FFD24A>■ ルール</color>\n" +
                           "・ラリーポイント制。3回以内に相手コートへ返そう。2点差がつくまで続く\n" + Keyboard;
            }
        }
    }
}
