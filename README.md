# ActionGameCore
3Dゲームにおけるキャラクターの基本動作のスクリプト。アクションゲームだけでなく、謎解きゲームや散歩ゲームなどキャラクターが移動するゲーム全般で使える。
 
![chara](https://github.com/eviltwo/ActionGameCore/assets/7721151/6921cd9a-26cf-404f-8d62-856556d62d1f)

# 特徴
- キャラクターはRigidbodyで動く。
  - 壁や物に当たる・落下する。
  - 足がバネになっているので、段差や坂をスムーズに移動する。
- 各機能は別々のパッケージ・コンポーネントに分離している。
  - 例えば、キャラクターの移動処理とカメラ処理は別なので、ゲームに合わせてFPSカメラとTPSカメラを変更できる。
  - 他にもアイテムやボタンのインタラクトやキャラクターアニメーションなども分離して実装（予定）

# パッケージインストール (UPM)

CharacterControls（キャラクターの移動）
```
https://github.com/eviltwo/PasteNext.git?path=src/ActionGameCore/Assets/CharacterControls
```
FPSCameraControls（FPS視点のカメラ）
```
https://github.com/eviltwo/PasteNext.git?path=src/ActionGameCore/Assets/FPSCameraControls
```
