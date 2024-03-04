
![selection_2](https://github.com/eviltwo/ActionGameCore/assets/7721151/93be2999-f0ae-468a-9fc6-e7d83c31d50c)

# Interactions
このパッケージは、カメラの中心にあるオブジェクトを検知し、プレイヤーが操作できるようにします。（例：電気のスイッチや棚の引き出し）

また、オブジェクトを見た時に"Eキーで開ける"などの文字を出せます。


# 注意
新InputSystemに対応しており、旧InputManagerは未対応です。

# インポート
UPMを使用して必要なパッケージをインポートできます。
1. InputSystemパッケージをインポートします。
1. このInteractionsパッケージをインポートします。
```
https://github.com/eviltwo/ActionGameCore.git?path=src/ActionGameCore/Assets/Interactions
```

# 初期設定
1. InputActionAssetを作成し、その中に"Interact"というActionを作ります。
    - キーボードの"E"キーを割り当てます。
1. メニューの"GameObject > Create Empty"で空オブジェクトを作成し、 `InteractRaycaster` と `InteractSystem` コンポーネントをアタッチします。
1. `InteractSystem` のInteractActionReferencesに"Interact"を設定します。

# 3Dボタン
1. CubeなどのColliderが付いているオブジェクトを作成し、 `Button3D` コンポーネントをアタッチします。
    - InteractEventに任意の関数を設定します。
    - 例えば、CubeのGameObjectのSetActiveを設定します。
1. ゲームを再生し、Cubeを見ながら"E"を押します。Cubeが消えるなど意図通りの関数が実行されたら完了です。

# Select/Deselect Event
`InteractSystem` コンポーネントの `SelectEvent` や、ボタン等に `SelectEventTrigger` コンポーネントを付けることで、直視した/やめた時のイベントを利用できます。これを使って直視している間に"ボタンを押せ"などのテキストを表示することができます。

# Change log
[CHANGELOG.md](CHANGELOG.md)
