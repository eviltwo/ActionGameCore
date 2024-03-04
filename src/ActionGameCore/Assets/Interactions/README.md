# Interactions
カメラの中心にあるオブジェクトをプレイヤーが操作できるようにします。（例：電気のスイッチや棚の引き出し）

新InputSystemのみ対応。

# 初期設定
1. InputActionAssetを作成し、その中に"Interact"というActionを作ります。
1. Main Cameraオブジェクトに `EyePhysicsRaycaster` コンポーネントをアタッチします。
1. メニューの"GameObject > UI > EventSystem"を選びます。(シーンにEventSystemオブジェクトが生成されます。)
1. EventSystemオブジェクトに `InteractSystem` コンポーネントをアタッチします。
  - Raycasterの値に先ほどの `EyePhysicsRaycaster` をアサインしてください。
  
