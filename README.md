# Practice_build_win
Windows アプリケーション開発の練習用レポジトリです。

## hello world プログラム (Python) ✅

- `main.py` は `hello world` を出力します。
- 例外が発生した場合は標準エラーにメッセージを出力して、終了コード `1` で終了します（正常終了は `0`）。

実行方法:

- Windows (cmd) で:

  ```bash
  python main.py
  python main.py --fail  # 故意に異常終了させて終了コードを確認する
  ```

- `--fail` オプションはテスト用で、例外を発生させ異常終了をシミュレートします。

---

## hello world プログラム (C#) ✅

- `HelloWorldApp/Program.cs` は `Hello World!` を出力します。
- 例外が発生した場合はメッセージを出力して、終了コード `1` で終了します（正常終了は `0`）。

実行方法:

- `dotnet` を使用:

  ```powershell
  dotnet run --project HelloWorldApp --configuration Release --    # 正常実行
  dotnet run --project HelloWorldApp --configuration Release -- --throw  # 故意に例外を発生させる
  ```

- `csc` (古い環境) を使用する場合:

  ```powershell
  csc -nologo -out:HelloWorldApp.exe HelloWorldApp\\Program.cs
  HelloWorldApp.exe --throw
  ```

`--throw` オプションはテスト用で、例外を発生させ異常終了をシミュレートします。