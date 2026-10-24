# スポーツスタジアム (SPORTS STADIUM)

> 澤井直輝のポートフォリオ作品のひとつです（[作品一覧へ](https://github.com/sawai-naoki-portfolio/Portfolio)）。

バスケットボール・テニス・バレーボールの3種目を遊べる、スマホ向け(横画面)の3Dスポーツゲームです。
コート・キャラクター・テクスチャ・効果音/BGM はすべて実行時に生成しており、外部アセットは日本語フォントのみです。

<table>
<tr>
<td width="33%"><img src="Doc/screenshots/10-basketball.jpg" alt="バスケットボール"></td>
<td width="33%"><img src="Doc/screenshots/11-tennis.jpg" alt="テニス"></td>
<td width="33%"><img src="Doc/screenshots/12-volleyball.jpg" alt="ビーチバレー"></td>
</tr>
<tr>
<td width="33%"><img src="Doc/screenshots/01-start.jpg" alt="スタート"></td>
<td width="33%"><img src="Doc/screenshots/02-select.jpg" alt="種目選択"></td>
<td width="33%"><img src="Doc/screenshots/00-title-stage.jpg" alt="タイトルの 3D ステージ"></td>
</tr>
</table>

| 種目 | 形式 | 主な要素 |
| --- | --- | --- |
| バスケットボール | 1on1 ハーフコート | ドリブル、ジャンプシュート(タイミングゲージ)、レイアップ/ダンク、クロスオーバー、スティール、ブロック、リバウンド、ショットクロック12秒、2点/3点 |
| テニス | シングルス | トップスピン/スライス/ロブ/ボレー/スマッシュ、サーブ(トス→タイミング)、フォルト/レット、デュース、タイブレーク、セット |
| バレーボール | ビーチ 2on2 | レシーブ→トス→スパイクの連携、ブロック、フェイント、ラリーポイント制、サーブ順の交代 |

難易度(かんたん/ふつう/むずかしい)・試合の長さ・チームカラー(ブルー/グリーン/パープル/イエロー。ユニフォーム・HUD・バスケのコートに反映)を選べ、種目ごとの通算戦績を保存します。

> **設計ドキュメント** — 企画・要件定義書・画面定義書(スクリーンショット/ワイヤーフレーム)・設計書(クラス図/シーケンス図/状態遷移図/画面遷移図)・テスト仕様は
> [`Doc/`](Doc/README.md) にまとめています。

## 動作環境

- Unity 6000.6 / URP 17.6 / Input System 1.20 / uGUI 2.6
- 対象: Android / iOS(横画面)。エディタではキーボード・ゲームパッドでも操作可能

## セットアップ

1. プロジェクトを Unity で開く
2. メニュー **SportsGame > Setup Project (Materials + Scenes + Settings)** を実行
   - `Resources/SportsGame/Materials` のベースマテリアル、ポストエフェクトのプロファイル(`Resources/SportsGame/PostFX.asset`)、4つのシーン、ビルド設定、プレイヤー設定(アプリ名・横画面固定)を作成します
3. `Assets/SportsGame/Scenes/Title.unity` を開いて再生

### ビルド

| 対象 | 手順 |
| --- | --- |
| Android (APK) | 1. Unity Hub → インストール → 6000.6.6f1 の歯車 → モジュールを加える → **Android Build Support**(OpenJDK・Android SDK & NDK Tools にもチェック)を追加<br>2. Unity エディタを再起動<br>3. メニュー **SportsGame > Build > Android (APK)** → `Builds/Android/SportsStadium.apk`<br>USB でつないだ実機(USB デバッグを有効化)にそのまま入れて起動するなら **SportsGame > Build > Android (APK) + Run on Device** |
| Windows | メニュー **SportsGame > Build > Windows** → `Builds/Windows/SportsStadium.exe`(動作確認済み) |
| iOS | iOS Build Support を追加し、Build Profiles で iOS を選んでビルド(Xcode プロジェクトを出力。実機向けのビルドには Mac が必要) |

ビルド時は IL2CPP / ARM64(Android)・横画面固定・アプリ名「スポーツスタジアム」が自動で設定されます。`Builds/` は Git 管理外です。

> **注意:** Android のビルドツールは、プロジェクトのパスに日本語などの全角文字が含まれていると動きません(`Project path ... contains non-ASCII characters`)。`C:\UnityProjects\SportsGame` のような英数字だけのパスに置いてください。OneDrive の同期フォルダの外に置くのがおすすめです。

## 操作

| 入力 | タッチ | キーボード | ゲームパッド |
| --- | --- | --- | --- |
| 移動 | 画面左側をドラッグ(フローティングスティック) | WASD / 矢印 | 左スティック |
| A / B / C | 画面右のボタン | J(Space) / K / L | South / East / West |
| ポーズ | 右上の II | Esc | Start |

各種目の詳しい操作はゲーム内の「あそびかた」に記載しています。

## フォルダ構成

```
Assets/SportsGame/
├── Resources/                 フォント(OFL)・ベースマテリアル・ポストエフェクト
├── Scenes/                    Title / Basketball / Tennis / Volleyball
├── Scripts/
│   ├── Runtime/               SportsGame.Runtime.asmdef
│   │   ├── App/               起動・シーン遷移・ログ・開発用オプション
│   │   ├── Settings/          種目情報・ユーザー設定・戦績(PlayerPrefs)
│   │   ├── Input/             タッチ/キーボード/ゲームパッドの統合入力
│   │   ├── Audio/             再生管理と波形合成(効果音・BGM)
│   │   ├── Physics/           決定論的なボール物理・軌道計算・先読み・コライダー
│   │   ├── Rendering/         マテリアル・メッシュ・手続き的テクスチャ
│   │   ├── Characters/        人型キャラクターの組み立てと手続き的アニメーション
│   │   ├── Presentation/      カメラ・ボール表示・観客席・演出
│   │   ├── UI/                UI キット・タッチ操作・試合 HUD
│   │   ├── Match/             3種目共通の試合の土台(MatchBase)
│   │   ├── Title/             タイトル・種目選択・設定画面
│   │   └── Sports/            Basketball / Tennis / Volleyball
│   └── Editor/                SportsGame.Editor.asmdef(セットアップメニュー)
└── Tests/
    ├── EditMode/              SportsGame.Tests.EditMode.asmdef
    └── PlayMode/              SportsGame.Tests.PlayMode.asmdef
```

## 設計

各種目は同じ構成で、役割ごとにクラスを分けています。

| 役割 | 例(テニス) | 内容 |
| --- | --- | --- |
| 寸法・判定 | `TennisDims` | コート寸法、イン/アウト |
| 得点 | `TennisScore` | 得点・ゲーム・セット・サーブ権(Unity 非依存、テスト対象) |
| 審判 | `TennisReferee` | ボールの接触からフォルト・アウト・得点を判定(テスト対象) |
| 打球 | `TennisShotSolver` | 狙いと初速度の計算 |
| 操作 | `ITennisController` / `TennisHumanController` / `TennisAIController` | 人の入力と CPU の判断を同じインターフェースで扱う |
| 表示 | `TennisHudPresenter` | 状態を HUD の文言・ボタンへ変換 |
| 調整値 | `TennisConfig` | Inspector で変更できるパラメータ |
| 進行 | `TennisGame`(`MatchBase` を継承) | 状態遷移と各部品の連携 |

- ボール物理は重力を解析解で積分するため、軌道計算(`BallMath`)と実際の動きが一致し、CPU は `BallPredictor` で先読みできます。
- 依存の向きは `Sports → Match/UI/Presentation → Physics/Rendering/Audio/Input → App/Settings` です。
- マテリアル・テクスチャはキャッシュし、シーン遷移時に未使用アセットを解放します。
- オプションの「高画質」では影・観客の密度・パーティクル量を上げ、ポストエフェクト(トーンマッピング・ブルーム・色調補正・ビネット)を有効にします。「省電力」ではいずれも抑えます。
- アプリがバックグラウンドへ回ると自動でポーズします。PlayerPrefs の値は読み込み時に検証します。

## 調整

各シーンの `BasketballGame` / `TennisGame` / `VolleyballGame` コンポーネントの Inspector に、CPU の強さ・移動速度・打球速度などの調整値があります。`Vector2` の項目は (かんたん, むずかしい) の値で、難易度に応じて補間されます。

調整値はシーンに保存されるため、コード側(`XxxConfig`)の初期値を変更したときは **SportsGame > Create Scenes + Build Settings** でシーンを作り直すと反映されます(Inspector で変更した値は初期値に戻ります)。

## テスト

Window > General > Test Runner で実行します。

- **EditMode**(`SportsGame.Tests.EditMode`): 得点ルール・審判・シュート成功率・放物線計算・「成功判定のシュートが物理的にも入るか」・入力状態などを検証します。
- **PlayMode**(`SportsGame.Tests.PlayMode`): 各シーンを実際に起動し、タイトルの画面遷移、3種目の自動対戦で得点が進むこと、タッチ操作(シュート・トス→サーブ)が効くことを確認します。`FullMatchTests` は3種目を(難易度を変えて)最後まで戦わせ、リザルト画面まで到達することを確認します(数分かかります)。エラーログが出ると失敗します。
- 自動対戦(テスト含む)の試合は通算戦績に記録されません。

## 開発用

- `DebugOptions.AutoPlay = true` にしてから試合シーンを読み込むと、プレイヤー側も CPU が操作します(エディタ・開発ビルドのみ)。バランス調整や動作確認に使います。
- `GameLog.Info` はエディタとデバッグビルドでのみ出力され、リリースビルドでは除去されます。

## ライセンス

- 日本語フォント: M PLUS Rounded 1c(SIL Open Font License 1.1)。`Assets/SportsGame/Resources/Fonts/LICENSE_GameFont.txt` を参照。
