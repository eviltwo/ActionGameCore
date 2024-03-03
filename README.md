# ActionGameCore
3Dゲームにおけるキャラクターの基本動作のスクリプト。アクションゲームだけでなく、謎解きゲームや散歩ゲームなどキャラクターが移動するゲーム全般で使える。
 
![chara](https://github.com/eviltwo/ActionGameCore/assets/7721151/6921cd9a-26cf-404f-8d62-856556d62d1f)

# 特徴
- キャラクターはRigidbodyで動く。
  - 壁や物に当たる・落下する。
  - 足がバネになっているので、段差や坂をスムーズに移動する。
  - Rigidbodyで移動する床やエレベーターに乗ると追従する。
- 各機能は別々のパッケージ・コンポーネントに分離している。
  - 例えば、キャラクターの移動処理とカメラ処理は別なので、ゲームに合わせてFPSカメラとTPSカメラを変更できる。
  - 他にもアイテムやボタンのインタラクトやキャラクターアニメーションなども分離して実装（予定）
- 新しいInputSystemに対応。古いInputManagerは未対応。
  - InputSystemパッケージをインストールすればデフォルトのInputActionAsset設定で動作する。

# パッケージインストール (UPM)

[CharacterControls](https://github.com/eviltwo/ActionGameCore/blob/main/src/ActionGameCore/Assets/CharacterControls/CHANGELOG.md)（キャラクターの移動）
```
https://github.com/eviltwo/ActionGameCore.git?path=src/ActionGameCore/Assets/CharacterControls
```
[FPSCameraControls](https://github.com/eviltwo/ActionGameCore/blob/main/src/ActionGameCore/Assets/FPSCameraControls/CHANGELOG.md)（FPS視点のカメラ）
```
https://github.com/eviltwo/ActionGameCore.git?path=src/ActionGameCore/Assets/FPSCameraControls
```
[TPSCameraControls](https://github.com/eviltwo/ActionGameCore/blob/main/src/ActionGameCore/Assets/TPSCameraControls/CHANGELOG.md)（TPS視点のカメラ）
```
https://github.com/eviltwo/ActionGameCore.git?path=src/ActionGameCore/Assets/TPSCameraControls
```
