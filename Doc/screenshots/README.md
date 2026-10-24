# スクリーンショット

実機で撮影した主要画面です。画面ごとの表示項目・操作は[画面定義書](../screen-definitions.md)を参照してください。

| ファイル | 画面 | 見どころ |
|---|---|---|
| [`01-start.jpg`](01-start.jpg) | S01 スタート | ロゴと「画面をタップしてスタート」。背景は 3D のタイトルステージ |
| [`00-title-stage.jpg`](00-title-stage.jpg) | タイトルの 3D ステージ（UI なし） | 台座・床・リング、ボールの模様まで、すべて実行時にコードで生成（`TitleBackdrop`・`ProcTextures`） |
| [`02-select.jpg`](02-select.jpg) | S02 種目選択 | 種目のテーマカラーのカードと通算戦績 |
| [`10-basketball.jpg`](10-basketball.jpg) | S10 試合（バスケットボール） | 守備時の HUD。ショットクロック、状況で変わるボタン（ブロック／スティール／ダッシュ）とヒント。チームカラーはグリーン |
| [`11-tennis.jpg`](11-tennis.jpg) | S10 試合（テニス） | ゲーム数とポイントのスコアボード、サーブ権の点、トップスピン／スライス／ロブ |
| [`12-volleyball.jpg`](12-volleyball.jpg) | S10 試合（ビーチバレー） | 2on2、レシーブ／ブロックのボタン |

<table>
<tr>
<td width="50%"><img src="01-start.jpg" alt="スタート"></td>
<td width="50%"><img src="02-select.jpg" alt="種目選択"></td>
</tr>
<tr>
<td width="50%"><img src="10-basketball.jpg" alt="バスケットボール"></td>
<td width="50%"><img src="11-tennis.jpg" alt="テニス"></td>
</tr>
<tr>
<td width="50%"><img src="12-volleyball.jpg" alt="ビーチバレー"></td>
<td width="50%"><img src="00-title-stage.jpg" alt="タイトルの 3D ステージ"></td>
</tr>
</table>

## まだ撮影していない画面

S03 試合設定・S04 設定・S05 あそびかた・S11 ポーズ・S12 リザルトは、画面定義書ではコードのレイアウトと文言から起こした**ワイヤーフレーム**（[`../diagrams/svg/wf-*.svg`](../diagrams/svg/)）で示しています。

| ファイル名 | 画面 | 撮り方 |
|---|---|---|
| `03-setup.jpg` | S03 試合設定 | 種目のカードを選ぶ |
| `04-options.jpg` | S04 設定 | 種目選択の「設定」 |
| `20-result.jpg` | S12 リザルト（YOU WIN!） | 試合を最後まで |

- **Unity エディタ**：Game ビューの解像度を 1920×1080 にし、`DebugOptions.AutoPlay = true` で自動対戦させると決定的な場面を狙いやすい
- **Windows ビルド**：メニュー **SportsGame > Build > Windows** で作った `SportsStadium.exe` を 1920×1080 で起動し、`Win + Shift + S`
- **Android 実機**：電源 + 音量下
