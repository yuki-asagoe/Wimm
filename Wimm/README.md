# Wimm - Wish It Move Magically

## 概要
神戸大学ロボット研究会「六甲おろし」用、複数のロボットを統一的にモダンなインタフェースで制御することを目指したソフトウェア。

## ロボットデータの表現
実行ファイルと同一階層に`Machines` フォルダを作成しそこに各ロボットのデータを配置します

#### 例
後でdocsにまとめます

拡張子がないのはフォルダ
- {wimm.exe} : 実行ファイル
- Machines
  - Machine1 : dllと同じ名前
    - icon.png 正方形をおすすめ、なくてもいいけどその場合Wimmにアイコンは表示されない
    - Machine1.dll : Machineクラスを定義したdll
    - description.txt : ロボットの説明を記述できます。Wimmからは使用しない(はず)
    - meta_info.json : ロボットの名前と制御ボードの名前などを定義
    - config.json : ロボットに与える情報のうちユーザーが書き換えられ、単純な文字列として与えられるデータ。詳しくはMachineConfigクラス参照
    - script
      - initialize.neo.lua : 初期化時に一回呼び出されます(definitionの後)。初期化処理が必要ならここで
      - definition.neo.lua : 初期化時に一回呼び出されます。関数やグローバル変数定義を行う
      - control_map.neo.lua : 初期化時に一回呼び出されます(最後)。コントローラーの入力と呼び出す関数を紐づけます。
      - on_control.neo.lua : 毎制御処理毎に呼び出されます。単純なコントローラーとのマッピングでは制御できない複雑なコントロールを処理
      - macro : マクロ処理を定義します。
        - macros.json
        - 1.neo.lua
        - 2.neo.lua
        - ...
    - docs : スクリプトを書くにあたって必要な情報がWimmから提供されるフォルダ、中身は未定義