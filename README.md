# ActionGameCore
3Dゲームにおけるキャラクターの基本動作のスクリプトです。アクションゲームだけでなく、謎解きゲームや散歩ゲームなどキャラクターが移動するゲーム全般で使えます。
 
![chara](https://github.com/eviltwo/ActionGameCore/assets/7721151/6921cd9a-26cf-404f-8d62-856556d62d1f)

# 特徴
- キャラクターはRigidbodyで動き、カメラの向きに合わせて前後左右に歩きます。
  - 壁や物に当たったり落下します。
  - 足がバネになっているので、段差や坂をスムーズに移動します。
  - 移動床やエレベーターに乗ると追従します。(床にもKinematicなRididbodyを付ける)
- 各機能は別々のパッケージ・コンポーネントに分離しています。
  - 例えば、キャラクターの移動処理とカメラ処理は別なので、ゲームに合わせてFPSカメラとTPSカメラを選べます。
  - 他にもボタンのインタラクトやキャラクターアニメーションなども分離して実装予定です。
- 新しいInputSystemに対応していますが、古いInputManagerは未対応です。
  - InputSystemパッケージをインストールすればデフォルト設定のままで動作します。

# パッケージ一覧 (UPMでインポートできます)
### CharacterControls v0.6.1
キャラクターの歩行・ジャンプ。
```
https://github.com/eviltwo/ActionGameCore.git?path=src/ActionGameCore/Assets/CharacterControls
```

### FPSCameraControls v1.1.0
FPS視点のカメラ。
```
https://github.com/eviltwo/ActionGameCore.git?path=src/ActionGameCore/Assets/FPSCameraControls
```

### TPSCameraControls v1.1.0
TPS視点のカメラ。
```
https://github.com/eviltwo/ActionGameCore.git?path=src/ActionGameCore/Assets/TPSCameraControls
```

### Interactions v0.5.1
視線の先にある物にインタラクトする。3Dボタンなど。 [README](src/ActionGameCore/Assets/Interactions/README.md)
```
https://github.com/eviltwo/ActionGameCore.git?path=src/ActionGameCore/Assets/Interactions
```
