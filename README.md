# ActionGameCore
3Dゲームにおけるキャラクターの基本動作のスクリプトです。アクションゲームだけでなく、謎解きゲームや散歩ゲームなどキャラクターが移動するゲーム全般で使えます。
 
![action](https://github.com/user-attachments/assets/c8a67533-cd89-4db9-bcca-5d2db4c2f2d3)

# 特徴
- キャラクターはRigidbodyで動き、カメラの向きに合わせて前後左右に歩きます。
  - 壁や物に当たったり落下します。
  - 足がバネになっているので、段差や坂をスムーズに移動します。
  - 移動床やエレベーターに乗ると追従します。(床にもKinematicなRididbodyを付ける)
- 各機能は別々のパッケージ・コンポーネントに分離しています。
  - 例えば、キャラクターの移動処理とカメラ処理は別です。ゲームに合わせてFPSカメラとTPSカメラを選べます。
  - 他にもボタンのインタラクトやキャラクターアニメーションなども分離して実装予定です。
- 入力処理はInputSystemに対応しています。

# パッケージ一覧 (UPMでインポートできます)
### CharacterControls
キャラクターの歩行・ジャンプ。
```
https://github.com/eviltwo/ActionGameCore.git?path=src/ActionGameCore/Assets/CharacterControls
```

### CameraControls
FPSとTPS視点のカメラ。
```
https://github.com/eviltwo/ActionGameCore.git?path=src/ActionGameCore/Assets/CameraControls
```

# 活動支援
私は個人開発者なので、金銭的に支援して頂けるととても助かります！
- [Asset Store](https://assetstore.unity.com/publishers/12117)
- [Steam](https://store.steampowered.com/curator/45066588)
- [GitHub Sponsors](https://github.com/sponsors/eviltwo)
