# XenoSteel 開発ロードマップ（完全版＋ストーリー）


# Sprint 1：SRPG基盤（移動＋ターン進行）

## 目的

TBSF標準機能を使い、プレイヤーと敵ユニットが移動し、ターンが進行するSRPG基盤を完成させる。

## ターンシステム

プレイヤーターンと敵ターンを分けず、**全ユニットの機動力によって行動順を決定する方式**とする。

各ラウンド開始時に、味方・敵を含む全ユニットの機動力を比較し、数値が高いユニットから順番に行動する。
※機体ステータスはまだ作らないため、機動力は仮値を使用


```text
Round 1

味方A：機動力85
↓
敵B：機動力72
↓
味方C：機動力60
↓
敵D：機動力45

↓

Round 2

味方A：機動力85
↓
敵B：機動力72
↓
味方C：機動力60
↓
敵D：機動力45
```

機動力が同値の場合の処理については、後から決定する。

## 完了条件

* プレイヤーユニットが移動できる
* 敵ユニットがAIで移動できる
* 味方・敵を区別せず、機動力順に行動できる
* 1ラウンド終了後、次のラウンドへ移行できる
* ステータス・UIはまだ作らない

## 実装項目

* [x] PlayerUnit プレハブ作成
* [x] EnemyUnit プレハブ作成
* [x] EnemyUnit の UnitController を AIController に変更
* [x] マップに Player / Enemy を配置
* [x] TurnResolver の動作確認
* [x] 全ユニットの機動力を取得
* [x] 機動力による行動順決定
* [x] 行動済みユニットの管理
* [x] 全ユニット行動後のラウンド更新
* [x] 機動力同値時の行動順処理

## Sprint 1で作成したクラス・データの後続Sprintでの利用

* `XenoSteelTurnResolver.cs`
  * 戦闘中の行動順・ラウンド進行を管理
  * Sprint 2以降も継続使用

* `XenoSteelInitiative.cs`
  * ユニットの機動力を保持
  * `XenoSteelTurnResolver` が行動順の決定に使用
  * Sprint 2の機体ステータス実装後、機体データと接続する

* `XenoSteelTurnResolver.CurrentRound`
  * 現在のラウンド数を取得
  * 戦闘中イベント実装時に「特定ラウンド」を条件として使用する

* `PlayerUnit / EnemyUnit`
  * Sprint 2以降の機体ステータス・戦闘・AIの基盤として継続使用

* `XenoSteelBehaviourTree`
  * EnemyUnitのAI行動を定義
  * AI拡張時に継続使用

---

# Sprint 2：機体ステータス（ScriptableObject化）

## 目的

機体ごとのステータスを外部データ化し、後から自由に変更できる構造を作る。

## 完了条件

* XenoUnitData（ScriptableObject）が作成されている
* PlayerUnit / EnemyUnit が異なるステータスを参照できる
* ステータスは「表示されるだけ」でよい

## 実装項目

* [x] XenoUnitData ScriptableObject の作成
* [x] HP / EN / 装甲 / 機動力 / 環境適応などを追加
* [x] Unit プレハブに XenoUnitData を紐付ける
* [x] 簡易ステータスUI（仮UI）を作成

## Sprint 2で作成・変更したクラス・データの後続Sprintでの利用

* `XenoUnitData.cs`
  * 機体ごとのステータスをScriptableObjectとして保持
  * HP / EN / 装甲 / 機動力 / 移動力 / 環境適応 / サイズを管理
  * Sprint 2.5でPilotDataを設定するために使用
  * Sprint 3で習得技（Skills[]）を追加するために使用
  * Sprint 3以降の戦闘・ダメージ計算で機体の基本ステータスを参照する

* `XenoSteelInitiative.cs`
  * `XenoUnitData` を参照して機体の機動力を取得
  * `XenoSteelTurnResolver` が行動順を決定する際に使用
  * Sprint 2以前の仮の機動力から、機体データの機動力へ変更
  * Sprint 2.5でパイロット補正を含む最終的な機動力を参照する際に利用

* `XenoSteelStatusUI.cs`
  * 選択されたUnitの `XenoUnitData` を取得し、機体ステータスを表示
  * 現在は確認用の仮UI
  * Sprint 4で正式なステータスUIへ発展・置き換え予定

* `XenoUnitData` ScriptableObject
  * 実際の機体ごとのステータスデータを保存
  * PlayerUnit / EnemyUnitにそれぞれ異なるデータを設定可能
  * Sprint 2.5以降も機体データとして継続使用
---

# Sprint 2.5：パイロットシステム

## 目的

機体だけではなく、搭乗するパイロットによってユニットの性能が変化する仕組みを作る。

同じ機体でも、パイロットによって異なる性能を発揮できるようにする。

## 完了条件

* PilotData（ScriptableObject）が作成されている
* 機体にパイロットを割り当てられる
* 機体ステータスとパイロットステータスから最終ステータスを計算できる
* 同じ機体でもパイロットによって性能が変化する
* パイロットを設定していないユニットも使用できる

## 実装項目

* [x] PilotData ScriptableObject の作成
* [x] HP / EN / 装甲 / 機動力 などのパイロットステータスを追加
* [x] XenoUnitData に PilotData の設定項目を追加
* [x] 機体ステータスとパイロットステータスの合算処理
* [x] 最終ステータスをゲーム内で参照する処理
* [x] パイロット情報の簡易表示
* [x] 複数パイロットによる性能差の確認

## ステータス計算

基本的には、

```text
最終ステータス
=
機体ステータス
+
パイロット補正
```

とする。

パイロットによる影響が大きくなりすぎないよう、初期段階では補正値を直接設定する。

将来的に必要であれば、レベルや能力値による計算方式へ拡張する。

## パイロット固有アビリティ

パイロットごとに固有アビリティを設定できる構造を想定する。

ただし、パイロットシステムの基本機能を優先し、固有アビリティの実装は余裕がある場合に行う。

* [ ] PilotAbility の設計
* [ ] パイロット固有アビリティの設定
* [ ] 戦闘中のアビリティ効果適用

## Sprint 2.5で作成・変更したクラス・データの後続Sprintでの利用

* `PilotData.cs`

  * パイロットのデータをScriptableObjectとして保持
  * パイロット名を管理
  * HP / EN / 装甲 / 機動力の倍率を管理
  * `XenoUnitData` から参照され、搭乗パイロットとして設定される
  * 今後、ユニット編成・ユニット編集時にパイロットを変更する際に使用する
  * 将来的にパイロット固有アビリティなどを追加する場合の基盤として使用する

* `XenoSteelUnitStats.cs`

  * `XenoUnitData` の機体ステータスと `PilotData` の倍率から、ゲーム中で使用する最終ステータスを計算する
  * パイロットが設定されていない場合は機体ステータスをそのまま使用する
  * HP / EN / 装甲 / 機動力の最終値を提供する
  * Sprint 3以降の戦闘・ダメージ計算などで、最終ステータスを参照するために使用する

* `XenoUnitData.cs`

  * `PilotData` の参照項目を追加
  * 機体に搭乗するパイロットを設定するために使用する
  * 機体ステータスそのものは引き続き機体側で管理する
  * 環境適応は機体固有のステータスとして引き続き保持する
  * 今後のユニット編成・戦闘システムでも機体データとして継続使用する

* `XenoSteelInitiative.cs`

  * `XenoSteelUnitStats` を使用して最終的な機動力を取得するよう変更
  * パイロットの機動力倍率を反映した値を行動順の判定に使用する
  * `XenoSteelTurnResolver` が行動順を決定する際に継続使用する

* `XenoUnitStatusUI.cs`

  * `XenoSteelUnitStats` を使用して最終ステータスを表示するよう変更
  * パイロット補正後のHP / EN / 装甲 / 機動力を表示する
  * 機体名とパイロット名を確認できる
  * 現在は確認用の仮UIとして使用し、正式なステータスUI実装時に発展・置き換え予定

* `PilotData` ScriptableObject

  * `Data/Units/PilotData/` にパイロットごとのデータを作成する
  * 同じ機体でも異なるパイロットを設定することで、最終ステータスを変化させられる
  * パイロット未設定のユニットにも対応可能

### Sprint 2.5のステータス計算方式

基本的には、

```text
最終ステータス
=
機体ステータス
×
パイロット倍率
```

とする。

対象となるステータスは、

```text
HP
EN
装甲
機動力
```

とする。

例えば、

```text
機体HP       1000 × パイロットHP倍率 1.3  = 1300
機体EN        100 × パイロットEN倍率 1.2  = 120
機体装甲      200 × パイロット装甲倍率 0.9 = 180
機体機動力     80 × パイロット機動力倍率 1.15 = 92
```

となる。

移動力・環境適応・サイズはパイロット補正の対象外とし、機体側で管理する。

### Sprint 2.5追加：移動力のTBSF接続

Sprint 2.5までに作成した機体ステータスのうち、**移動力をTBSF標準の移動システムへ接続**した。

TBSFの `Unit` が持つ `MovementPoints` を利用し、`XenoUnitData` に設定した移動力によって実際の移動可能範囲が変化するようにした。

TBSFでは `MovementPoints` が移動可能距離を決定するため、既存の `MoveAbility` や `Unit.cs` の移動処理は変更せず、そのまま利用する。

#### 実装項目

* [x] `XenoUnitData` の移動力をゲーム中の移動処理へ接続
* [x] TBSF `Unit.MovementPoints` への移動力設定
* [x] `MaxMovementPoints` への初期値反映を確認
* [x] 機体ごとに異なる移動力を設定できることを確認
* [x] 移動力の変更によって実際の移動可能範囲が変化することを確認

#### Sprint 2.5追加で作成したクラス・データ

* `XenoSteelMovement.cs`

  * XenoSteelの機体データから移動力を取得
  * `XenoSteelUnitStats` の `Movement` を使用
  * TBSFの `Unit.MovementPoints` に移動力を設定
  * TBSF標準の `MoveAbility` をそのまま利用できるようにする
  * `XenoSteelInitiative.cs` とは分離し、移動に関する処理を担当する

* `XenoSteelUnitStats.cs`

  * `XenoUnitData.movement` を `Movement` として提供
  * 移動力についてはパイロット補正を行わず、機体側の値をそのまま使用する

* `Unit.cs`（TBSF標準）

  * `MovementPoints` を実際の移動可能範囲の基準として使用
  * `MaxMovementPoints` によってターン終了時の移動力回復を管理
  * TBSF標準コード自体は変更せず、XenoSteel側から値を設定する形で利用



---

# Sprint 3：技（Skill）システムの実装

## 目的

ユニットが技を使用し、ダメージ計算が行われ、戦闘が成立する。

## 完了条件

* 技（Skill）が選択できる
* 技の射程・EN消費・ダメージが適用される
* ダメージ計算が行われる

## 実装項目

* [x] `SkillData` ScriptableObject の作成
* [x] `XenoUnitData` に習得技リスト（`skills[]`）を追加
* [x] `SkillData` の射程（`range`）を攻撃可能範囲の判定に使用
* [x] XenoSteel独自のAttack Abilityを作成
* [x] XenoSteel独自のAttack AbilityをUnitへ登録
* [x] SkillDataを使用した攻撃対象判定
* [x] `XenoSteelDamageCalculator` によるダメージ計算処理
* [x] SkillのEN消費処理
* [x] AIによるSkillの射程判定・攻撃処理
* [x] AIが射程外の敵へ接近する移動処理
* [x] 敵がSkillの射程内に入った場合、それ以上必要以上に接近しないAI移動処理
* [x] XenoSteel独自Behaviour Treeの作成
* [x] XenoSteel独自AI攻撃処理の作成
* [x] XenoSteel独自AI移動処理の作成
* [x] EN消費後のStats UIへの反映
* [x] 技選択 UI（簡易）
* [x] 複数Skillからの技選択
* [x] 攻撃範囲（`area`）の実装
* [x] 攻撃方向の実装

## Sprint 3で作成・変更したクラス・データ

* `SkillData.cs`

  * 技1つ分のデータをScriptableObjectとして保持
  * 技名、威力、射程、EN消費、攻撃範囲などを管理
  * 攻撃形状として `Single` / `Line` / `Cross` を設定可能
  * `Line` の攻撃幅を1～10で設定可能
  * 範囲攻撃時の中心対象以外へのダメージ倍率を設定可能
  * `XenoUnitData.skills[]` から機体が習得している技として参照する
  * 今後、属性・特殊効果・追加の攻撃形状などを拡張する際の基盤として使用する

* `XenoUnitData.cs`

  * 機体データに `SkillData[] skills` を追加
  * 機体ごとに異なる複数のSkillを設定できる構造を追加
  * PlayerのSkill選択およびEnemy AIのSkill選択から参照する
  * Sprint 3以降の攻撃・戦闘処理で継続使用する

* `XenoSteelAttackAbility.cs`

  * TBSFの `Ability` を継承したXenoSteel独自の攻撃Ability
  * TBSF本体の `AttackAbility` / `AttackAbilityImpl` を変更せずに使用する
  * `XenoSteelInitiative` から `XenoUnitData` と `XenoSteelUnitStats` を取得
  * 現在使用するSkillを保持し、Player / Enemy双方の攻撃処理から使用する
  * Skillの `range` を使用して攻撃可能な対象を判定
  * 攻撃可能な敵をTBSFのHighlighter機能で表示
  * `XenoSteelAttackTargeting` と連携して攻撃対象を決定
  * `XenoSteelDamageCalculator` を使用してダメージを計算
  * SkillのEN消費を実行
  * `area` による範囲攻撃に対応
  * 範囲攻撃時の中心対象と周囲の対象でダメージ倍率を変更可能
  * `Single` / `Line` / `Cross` の攻撃形状に対応
  * `Line` の攻撃幅を使用した複数対象への攻撃に対応
  * TBSFの `AttackCommand` を使用して実際の攻撃を実行する
  * 攻撃方向についてはSprint 4の戦闘演出・アニメーション実装時に正式調整する

* `XenoSteelAttackTargeting.cs`

  * XenoSteel独自の攻撃対象判定を担当
  * `SkillData` の攻撃形状を使用して対象セル・対象ユニットを判定
  * `Single` / `Line` / `Cross` の攻撃形状に対応
  * `Line` の攻撃幅を使用した複数セルへの攻撃に対応
  * 攻撃対象判定処理を `XenoSteelAttackAbility` から分離
  * 今後、新しい攻撃形状や攻撃範囲ルールを追加しやすい構造とする

* `XenoSteelDamageCalculator.cs`

  * Skillの威力、機体のAttack、敵機のArmorを使用してダメージを計算
  * 戦闘計算をAttack Abilityから分離して管理
  * 計算結果が0以下の場合は最低1ダメージとする
  * Player / Enemyの双方から同じダメージ計算処理を使用する
  * 将来的なパイロットアビリティ・機体アビリティによる補正追加に対応する

* `XenoSteelUnitStats.cs`

  * `XenoUnitData.attack` を機体のAttack性能として提供
  * HP / EN / Armor / Mobility / Movementなどの最終ステータスを提供
  * ENの最大値を保持
  * 戦闘中の現在ENを管理
  * Skill使用時のEN消費を処理
  * ダメージ計算・攻撃処理から現在のユニット性能を参照するために使用する

* `XenoSteelInitiative.cs`

  * `XenoUnitData` と `XenoSteelUnitStats` を保持
  * XenoSteel独自AbilityやAIから機体データ・最終ステータスを参照するために使用
  * Sprint 2.5までのパイロット補正を反映した最終ステータスをSprint 3の戦闘処理へ接続する

* `XenoSteelSkillSelectionUI.cs`

  * Playerが使用するSkillを選択するための簡易UI
  * `XenoUnitData.skills[]` に設定された複数のSkillを表示・選択
  * 選択したSkillを `XenoSteelAttackAbility` へ渡して攻撃に使用する
  * Sprint 4以降で正式な戦闘UIへ発展・整備する

* `XenoSteelAIAttackActionNode.cs`

  * XenoSteel用のAI攻撃処理
  * `XenoUnitData.skills[]` から使用可能なSkillを取得
  * SkillごとのEN消費と射程を確認
  * 射程内に攻撃対象が存在するSkillを候補とする
  * `XenoSteelDamageCalculator` を使用して各Skillの予測ダメージを計算
  * 使用可能なSkillの中から予測ダメージが最も高いSkillを選択
  * `XenoSteelAttackAbility` に選択したSkillを設定
  * TBSFの `AttackCommand` と `AIExecuteAbility()` を使用して攻撃を実行する
  * Playerと同じXenoSteel独自のダメージ計算を使用する

* `XenoSteelAIMoveActionNode.cs`

  * XenoSteel用のAI移動処理
  * TBSF標準の移動処理を参考にXenoSteel側で実装
  * 移動可能なセルを評価して移動先を決定
  * Unitの移動可能範囲を考慮して移動先を決定
  * 攻撃対象との距離を考慮して移動先を決定
  * 攻撃対象が使用可能なSkillの射程外にいる場合は接近する
  * 攻撃対象がSkillの射程内にいる場合は必要以上に接近しない
  * TBSFの `MoveCommand` を使用して実際の移動を実行する

* `XenoSteelRegularBehaviourTreeResource.cs`

  * TBSF標準のBehaviour Tree Resourceを直接変更せず、XenoSteel側に独自Behaviour Treeを作成
  * XenoSteel独自の攻撃・移動Action NodeをBehaviour Treeから呼び出す
  * TBSFのPosition Evaluatorを利用して移動先を評価
  * 攻撃処理には `XenoSteelAIAttackActionNode` を使用
  * 移動処理には `XenoSteelAIMoveActionNode` を使用
  * TBSF標準の `AttackActionNode` に依存せず、XenoSteelのSkillシステムへ接続する

* `XenoUnitStatusUI.cs`

  * XenoSteel独自のUnitステータスUI
  * `XenoSteelInitiative.Stats` を参照してステータスを表示
  * 以下を表示
    - 機体名
    - パイロット名
    - HP
    - EN
    - Attack
    - Armor
    - Mobility
    - Movement
    - Terrain
    - Size
  * ENを現在値 / 最大値で表示
  * `XenoSteelUnitStats` を参照することで、Skill使用後のEN消費をUIへ反映する
  * Sprint 4で正式なステータスUIへ発展・整備する

* `XenoSteelUnitFacing.cs`

  * Unitの向き（Facing）を管理するXenoSteel独自コンポーネント
  * `Up` / `Down` / `Left` / `Right` の4方向を管理
  * Unitの移動方向に応じて向きを変更する処理を実装
  * Visual Rootの回転を変更して、Unitの見た目の向きを制御する
  * 攻撃中は移動による自動的な向き変更を抑制するための制御を追加
  * 攻撃対象の方向へUnitを向ける処理と接続
  * Sprint 3では基本的なFacing処理まで実装
  * 攻撃アニメーションによるTransform移動との干渉など、細かな向きの制御はSprint 4で調整する

### Sprint 3の実装結果

* [x] `SkillData` ScriptableObjectを作成
* [x] `XenoUnitData` に複数Skillを設定できる構造を追加
* [x] Playerが複数Skillから使用するSkillを選択できる
* [x] Skillごとの射程を攻撃対象判定に使用
* [x] SkillごとのEN消費を実装
* [x] Skillによるダメージ計算を実装
* [x] Player / Enemyの双方でXenoSteel独自のダメージ計算を使用
* [x] 範囲攻撃（`area`）を実装
* [x] `Single` / `Line` / `Cross` の攻撃形状を設定可能
* [x] `Line` の攻撃幅を設定可能
* [x] 範囲攻撃時のダメージ倍率を設定可能
* [x] Enemy AIが複数Skillから使用するSkillを選択可能
* [x] Enemy AIが予測ダメージを比較してSkillを選択
* [x] XenoSteel独自の攻撃対象判定を実装
* [x] TBSF標準の `AttackCommand` を利用して攻撃を実行
* [x] TBSF標準コードを直接変更せずXenoSteel側から攻撃システムを拡張

### Sprint 3時点の攻撃処理

現在は以下の構造で攻撃を行う。

```text
XenoUnitData
    ↓
skills[0]
    ↓
SkillData
    ├─ Power
    ├─ Range
    ├─ EN Cost
    └─ Area
    ↓
XenoSteelAttackAbility
    ↓
攻撃可能な敵をRangeで判定
    ↓
XenoSteelDamageCalculator
    ↓
ダメージ計算
    ↓
TBSF AttackCommand
    ↓
攻撃実行
```

### TBSFとの関係

TBSF標準の `Ability` を継承してXenoSteel独自Abilityを作成し、既存のTBSF攻撃システムを直接変更せずにXenoSteel側から拡張する。

TBSFのUnitはGameObjectに付いている `Ability` コンポーネントを取得して登録するため、`XenoSteelAttackAbility` をUnitへ追加することでAbilityとして登録される。

TBSF標準の `AttackCommand` は攻撃実行部分としてそのまま利用する。


---

# Sprint 4：戦闘UI・演出（最低限）

## 目的

戦闘を視覚的に理解できる最低限のUIと演出を追加する。

## 完了条件

* 攻撃時に簡易演出が入る
* ダメージ表示がある
* ステータスUIが整う

## 実装項目

* [x] ダメージポップアップ
* [ ] 攻撃アニメーション（簡易）
* [ ] ステータスUIの整備
* [ ] 技使用時に最低限の演出を再生（Timeline の簡易版）

---

# Sprint 5：AI拡張（行動選択）

## 目的

敵AIに「攻撃」「撤退」「追跡」などの行動選択を追加する。

## 完了条件

* HPが減ったら離脱するなどの簡易ロジックが動く
* 射程内なら攻撃、射程外なら接近する

## 実装項目

* [ ] BehaviourTreeの拡張
* [ ] 条件ノード（HP判定・距離判定）
* [ ] 行動ノード（攻撃・移動・撤退）

---

# Sprint 6：ゲームとして遊べる形へ

## 完了条件

* ステージ選択
* セーブ／ロード
* ユニット編成
* 経験値・レベルアップ
* レベルアップによる技習得

## 実装項目

* [ ] ステージ選択画面
* [ ] セーブデータ構造
* [ ] 経験値・レベルアップ処理
* [ ] レベルに応じた使用可能技の解放
* [ ] UnitData の Skills[] に技を追加
* [ ] 技選択 UI に反映

---

# Sprint 7：仕上げ（演出・バランス）

## 完了条件

* 攻撃演出の強化
* カットイン（任意）
* バランス調整
* UIの統一

## 実装項目

* [ ] Timelineによる演出強化
* [ ] カットイン画像の表示
* [ ] ステータス・Skillバランス調整
* [ ] UIデザイン統一

---

# Sprint 8：ストーリー・会話パート（スパロボ式）

## 目的

スパロボのようなストーリー進行・会話パートを実装し、戦闘と物語が連動するゲーム体験を作る。

## 完了条件

* 会話パートが再生できる
* ステージ開始前／終了後にイベントが挿入できる
* キャラ立ち絵・背景・テキストが表示される
* イベントから戦闘へ遷移できる
* 戦闘後にストーリーが進む

## 実装項目

* [ ] 会話シーン用の専用Scene（DialogueScene）の作成
* [ ] キャラ立ち絵表示システム
* [ ] テキストウィンドウ・名前表示
* [ ] スクリプト形式のイベントデータ（JSON or ScriptableObject）
* [ ] 「会話 → 戦闘」「戦闘 → 会話」の遷移処理
* [ ] ステージごとのイベント管理（StageFlowController）

## 注意点

* 会話パートは戦闘システムと独立させる
* ストーリーは後からいくらでも追加できる構造にする
* 「イベント → 戦闘 → イベント」を再現可能にする

---

# 追加：戦闘中イベント（セル・トリガー）

## 目的

戦闘中に「特定のマスに到達したらイベント発生」などのスパロボ式イベントを実装する。

## 完了条件

* 特定のセルに入った瞬間にイベントが発生する
* イベント内容は「増援」「会話」「演出」などに拡張可能
* 戦闘中でもストーリーが進行する

## 実装項目

* [ ] XenoCell にイベントID（TriggerID）を追加
* [ ] ユニットがセルに入った瞬間のコールバックを取得
* [ ] EventManager を作成し、TriggerID に応じたイベントを再生
* [ ] 戦闘中の会話・増援・演出を実装
