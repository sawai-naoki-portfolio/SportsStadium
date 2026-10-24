using System.Runtime.CompilerServices;

// テストから試合の内部状態(得点・統計)を読めるようにする
[assembly: InternalsVisibleTo("SportsGame.Tests.EditMode")]
[assembly: InternalsVisibleTo("SportsGame.Tests.PlayMode")]
