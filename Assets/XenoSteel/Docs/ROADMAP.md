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
* [x] 撃アニメーション（簡易）
* [x] ステータスUIの整備
* [ ] 技使用時に最低限の演出を再生（Timeline の簡易版）

## 戦闘演出の方針

Sprint 4では、攻撃演出を後から拡張できる基本的な仕組みを構築し、
演出自体は最低限に留める。

* 通常技・モブ機体
  * マップ上で簡易攻撃モーション・エフェクトを再生
  * 戦闘テンポを優先し、専用戦闘画面へは切り替えない

* 主人公機・ボス機体などの主要ユニット
  * 将来的に専用の戦闘演出へ拡張可能な構造とする

* 必殺技・主要な専用技
  * 将来的に画面切り替えを伴う専用3Dアニメーション・Timeline演出へ拡張する
  * 必要に応じてカットインを挿入する

* Sprint 7
  * Sprint 4で構築した演出基盤を利用して、ユニット・技ごとの演出を強化する
  * カットイン、専用Timeline、専用アニメーションなどを追加する

---
## Sprint 4で作成・変更したクラス・データ

### `XenoSteelDamagePopup.cs`

* 戦闘時に与えたダメージを画面上に数値として表示するUIコンポーネント
* TextMeshProを使用してダメージ値を表示
* 表示中は上方向へ移動
* 一定時間経過後に自動で破棄
* `Initialize(int damage)` によって表示するダメージ値を設定する

### `XenoSteelDamagePopupReceiver.cs`

* `IUnit.UnitAttacked` イベントを利用してダメージ表示を発生させる
* `UnitAttackedEventArgs.DamageDealt` から実際のダメージ値を取得
* 攻撃を受けたUnitのワールド座標をCanvas上の座標へ変換
* `XenoSteelDamagePopup` のPrefabを生成してダメージ値を表示する
* TBSF標準の攻撃処理を変更せず、イベントを利用してUIを追加する

### `UnitAttackedEventArgs`

* 攻撃を受けたUnit、攻撃したUnit、与えたダメージを保持するイベントデータ
* `AffectedUnit`
  * 攻撃を受けたUnit
* `AttackingUnit`
  * 攻撃したUnit
* `DamageDealt`
  * 実際に与えたダメージ
* `XenoSteelDamagePopupReceiver` がダメージポップアップを表示する際に使用する

### `XenoSteelAttackPresentation.cs`

* Unitごとの攻撃演出を管理するXenoSteel独自コンポーネント
* UnitのGameObjectに追加して使用する
* 攻撃演出方式を切り替えられる構造を追加
  * `SimpleMotion`
    * Visual Rootを攻撃方向へ短時間移動させて戻す簡易攻撃演出
  * `Animator`
    * Unity Animatorを使用した攻撃アニメーション
* `XenoSteelUnitFacing` からUnitの現在の向きを取得し、SimpleMotionの移動方向を決定する
* 攻撃時にTargetへ向きを変更せず、現在のFacing方向を維持する
* 通常のUnitはSimpleMotion、専用モーションを持つUnitはAnimatorを使用できる構造とする
* PlayerUnitでSimpleMotion、アスターでAnimatorによる攻撃モーションの動作を確認
* 将来的にTimelineなどのより大規模な戦闘演出へ拡張するための入口として使用する

### `XenoSteelCombatPresentationData.cs`

** Skillごとの戦闘演出設定をScriptableObjectとして保持する
* `PresentationType` によって演出方式を指定する
  * `None`
  * `SimpleMotion`
  * `NormalMotion`
  * `Special`
* SimpleMotion用の攻撃距離・攻撃時間を設定可能
* Animator用のTrigger名を設定可能
* 攻撃アニメーション用の`AnimationClip`を設定可能
* 攻撃エフェクト用のPrefabを設定可能
* `SkillData.presentation` からSkillごとの演出データを参照する

### `XenoSteelUnitFacing.cs`（Sprint 4調整）

* Unitの移動方向に応じてFacingを変更する処理を継続使用
* 攻撃時に攻撃対象の方向へ自動的に向かないよう調整
* `SimpleMotion` による攻撃演出では、現在のFacing方向を攻撃方向として使用
* TBSFの移動処理からFacingを変更できるよう`IUnitFacing`を実装
* 攻撃演出による一時的なTransform移動ではFacingを変更しない

### `IUnitFacing.cs`

* TBSFの移動処理とXenoSteel独自のFacing処理を接続するインターフェース
* `SetFacing(float x, float z)` によって移動方向をXenoSteel側へ通知する
* TBSF側からXenoSteel固有クラスへ直接依存しない構造にするために使用する

### `XenoUnitStatusUI.cs`

* 現在行動中のUnitのステータスを表示する戦闘HUD
* 機体名を表示
* パイロット名を表示
* HPを現在値 / 最大値で表示
* HPゲージをSliderで表示
* ENを現在値 / 最大値で表示
* ENゲージをSliderで表示
* ATKを表示
* DEFを表示
* `XenoSteelInitiative` から`XenoUnitData`と`XenoSteelUnitStats`を取得して表示する
* `XenoSteelTurnResolver`から現在行動中のUnitを受け取って表示を更新する

### `XenoSteelTurnOrderUI.cs`

* 現在行動中のUnitを除いた次の行動順を表示する戦闘HUD
* 最大4機の次に行動するUnitを表示
* `XenoSteelTurnResolver` の現在の行動順とインデックスを使用して表示する
* 現在のラウンド内で残っているUnitのみを表示する
* ラウンドをまたいで行動順をループさせず、現在ラウンド終了時には表示数が減少する
* 新しいラウンド開始時に新しい行動順を取得して再表示する
* `SlotPrefab`を必要な数だけ生成して表示する

### `XenoSteelTurnResolver.cs`

* `XenoSteelTurnOrderUI`へ現在の行動順を渡す処理を追加
* `TurnOrder` と `CurrentIndex` を公開し、戦闘UIから現在の行動順を参照できるようにする
* ラウンド開始時にTurnOrderUIを更新する
* ターン進行時にTurnOrderUIを更新する
* 全ユニットの行動終了時に新しいラウンドの行動順を作成し、TurnOrderUIを再更新する
* `XenoUnitStatusUI`へ現在行動中のUnitを渡す処理を継続使用する

### `SkillData.cs`

* Skillごとの戦闘演出データを参照する`presentation`を追加
* `XenoSteelCombatPresentationData`を設定することで、Skillごとに攻撃演出方式を指定できる

### `XenoSteelUnitInfoUI.cs`

* マップ上でカーソルをUnitに合わせた際、そのUnitの情報を表示するUI
* 現在行動中のUnitは表示対象から除外する
* Player / Enemyを問わず、現在行動中ではないUnitを表示対象とする
* 以下の情報を表示

  * 機体名
  * HP
  * EN
  * Armor
* Unitのワールド座標を画面座標へ変換し、対象Unitの近くに情報パネルを表示
* `CanvasGroup` を使用して表示・非表示を切り替える
* UI自身の入力を妨げないよう、情報パネルは `interactable = false` / `blocksRaycasts = false` とする
* TBSFのUnit Highlightイベントを利用して表示対象を切り替える
* `XenoSteelTurnResolver` から現在行動中のUnitを受け取り、表示対象から除外する

#### `XenoSteelEndTurnUI.cs`

* プレイヤーが現在操作しているUnitのターンを任意に終了するためのUI
* TBSFの `UnityGridController.EndTurn()` を使用してターン終了を実行する
* 現在のUnitがPlayerUnitの場合のみEnd Turnボタンを有効化する
* EnemyUnitのターンではボタンを操作できない
* `XenoSteelTurnResolver` から現在のUnitを受け取り、ボタンの有効・無効を更新する

### `XenoSteelSkillSelectionUI.cs`

* Playerの現在行動中Unitが使用できるSkill一覧を表示するUI
* `XenoUnitData.skills[]` から現在のUnitが習得しているSkillを取得して表示
* SkillごとにSkillSlotを生成する構造とする
* SkillSlotには以下を表示

  * Skill名
  * DetailButton
* SkillSlot本体を押すことで、そのSkillを `XenoSteelAttackAbility` の使用Skillとして設定する
* Skill一覧をScroll ViewのContentへ生成し、複数Skillを縦方向にスクロールして表示できる構造とする
* Playerのターン中はSkill一覧を継続して表示する
* EnemyのターンではSkill一覧を非表示にする
* `SetCurrentUnit()` によって現在行動中のUnitが切り替わった際、そのUnitのSkill一覧へ更新する
* Skillの詳細表示は `XenoSteelSkillDetailUI` に委譲する
* `XenoSteelAttackAbility.OnAbilitySelected()` からSkill一覧を再生成せず、現在表示しているSkill一覧をそのまま使用する
* `CanvasGroup` を使用してSkill一覧の表示・非表示とUI入力状態を管理する

### `XenoSteelSkillDetailUI.cs`

* 選択したSkillの詳細情報を表示するUI
* SkillListの各SkillSlotにあるDetailButtonから表示する
* 以下のSkill情報を表示

  * Skill名
  * 威力
  * 射程
  * EN消費
  * 属性
  * 範囲
* `SkillData` の値を直接参照して表示内容を更新する
* SkillListとは独立した詳細パネルとして配置する
* `CanvasGroup` を使用して表示・非表示を切り替える
* 表示中はUI操作を受け付け、非表示時は `interactable = false` / `blocksRaycasts = false` とする
* CloseButtonから詳細パネルを閉じる

### `XenoSteelAttackAbility.cs`（Sprint 4調整）

* 攻撃実行時に`XenoSteelAttackPresentation`を呼び出す処理を追加
* Playerによる攻撃時に攻撃演出を再生してから`AttackCommand`を実行する構造に変更
* 攻撃演出追加後、`await`を挟んでも使用SkillのEN消費量を保持できるよう、SkillのEN消費量をローカル変数へ保存
* `XenoSteelUnitStats.ConsumeEN()` によって攻撃後のEN消費を継続して実行
* TBSF標準の`AttackCommand`は変更せず、XenoSteel側から演出を追加する

### `XenoSteelInitiative.cs`

* `XenoUnitData`から生成した`XenoSteelUnitStats`を保持する構造を継続使用
* `Stats`を通して現在のHP / EN / Attack / Armor / Mobilityなどの最終ステータスを取得する
* Sprint 4の攻撃処理でも`Stats`を参照してEN消費を行う
* `XenoUnitData`が設定されている場合、`Awake()`で`XenoSteelUnitStats`を生成する

### `XenoSteelAIAttackActionNode.cs`

* EnemyのSkill攻撃時にも`XenoSteelAttackPresentation`を使用する
* PlayerとEnemyで共通の攻撃演出システムを利用する
---

# Sprint 5：ステージシステム

## 目的

ステージを個別のBattle Sceneとして管理し、ステージ選択からプレイできる構造を作る。

ステージはクリア後に自動で次へ進むのではなく、クリアによって次のステージを解放し、ステージ選択画面からプレイヤーが選択する。

## 完了条件

* ステージを個別のBattle Sceneとして作成できる
* ステージ選択画面からプレイするステージを選択できる
* ステージごとに異なるマップ・初期ユニット配置で戦闘できる
* ステージの勝利・敗北を判定できる
* ステージクリアによって次のステージを解放できる
* ステージクリア後にステージ選択へ戻れる

## 実装項目

* [x] ステージ用Battle Sceneの作成
* [x] ステージ情報を管理する`StageData`の作成
* [x] ステージ選択画面の作成
* [x] ステージの解放状態の管理
* [x] ステージ開始処理
* [x] ステージの勝利・敗北条件の設定
* [x] ステージクリア処理
* [x] ステージ選択画面への復帰
* [x] クリアしたステージに応じた次ステージの解放

## TBSFとの関係

* Battle SceneはTBSFのScene構成をそのまま利用する
* `GridController`、`CellManager`、`UnitManager`、`PlayerManager`、`TurnResolver`などの既存の戦闘システムを各Battle Sceneで使用する
* `GameEndConditions`をステージの勝敗条件を配置する場所として使用する
* 勝敗条件からTBSFの`GridController.InvokeGameEnded()`を呼び出し、`GameEnded`イベントによってステージ終了処理へ接続する
* TBSFの標準的なScene構成を崩さず、XenoSteel側のステージシステムを追加する

## Sprint 5で作成・変更したクラス・データ

### `StageData.cs`

* ステージ1つ分の情報をScriptableObjectとして保持する
* ステージIDを管理する
* ステージ名を管理する
* ステージ番号を管理する
* 使用するBattle Scene名を管理する
* クリア時に解放する次ステージのIDを管理する
* `Data/Stages/`にステージごとのデータを作成する
* Stage SelectからBattle Sceneを開始する際に使用する

### `XenoSteelStageProgress.cs`

* ステージのクリア状態を管理する
* ステージの解放状態を管理する
* `PlayerPrefs`を使用してクリア・解放状態を保存する
* `StageData.nextStageId`を使用して、クリアしたステージから次のステージを解放する
* Stage Select開始時に、すでにクリア済みのステージから次ステージの解放状態を更新する
* 今後のセーブシステム実装時にも、ステージ進行状態を管理する基盤として使用する

### `XenoSteelStageSelect.cs`

* Stage Select Sceneでステージ一覧を管理する
* `StageData`の一覧を参照する
* ステージごとの選択項目を実行時に生成する
* ステージの解放状態を更新してから一覧を生成する
* `StageList`へステージ選択項目を配置する

### `XenoSteelStageSelectItem.cs`

* Stage Selectに表示する1ステージ分の選択項目を管理する
* ステージ名を表示する
* `CLEAR` / `LOCKED`などのステータスを表示する
* 未解放ステージを選択できないようにする
* 選択された`StageData`のBattle Sceneを読み込む

### `XenoSteelGameResultHandler.cs`

* Battle SceneでTBSFの`GameEnded`イベントを受け取る
* Player 0が勝利した場合、現在の`StageData`をクリア状態にする
* ステージクリア時に`XenoSteelStageProgress.ClearStage()`を呼び出す
* 敗北時にはステージをクリア状態にせず終了する
* 勝敗に関係なくBattle Scene終了後にStage Selectへ戻る

### `XenoSteelEnemyDefeatCondition.cs`

* 敵ユニットの残存数を監視する
* Player 1のユニットがすべて撃破された場合に敵全滅を判定する
* TBSFの`GridController.InvokeGameEnded()`を呼び出してPlayer 0の勝利を発生させる
* `GameEndConditions`に配置してステージごとの勝利条件として使用する

### `XenoSteelPlayerDefeatCondition.cs`

* Player 0の味方ユニットの残存数を監視する
* Player 0のユニットがすべて撃破された場合に敗北を判定する
* TBSFの`GridController.InvokeGameEnded()`を呼び出してPlayer 1の勝利を発生させる
* `GameEndConditions`に配置してステージごとの敗北条件として使用する

## Sprint 5で作成したScene・Prefab

### `StageSelect.unity`

* ステージ選択専用Scene
* `StageList`にステージ選択項目を実行時生成する
* ステージの解放状態・クリア状態を表示する

### `StageSelectItem.prefab`

* Stage Selectで使用するステージ選択項目のPrefab
* `StageButton`
* `StageText`
* `StatusText`
を持つ

### `Battle_001`

* Sprint 1～4で使用していたBattle Sceneをステージ用Battle Sceneとして使用
* ステージ固有のマップ・初期ユニット配置を設定する
* `StageData`から`Battle_001`を起動できる
* `GameEndConditions`に勝敗条件を配置する

### `Stage_001 / Stage_002`

* `StageData`として作成
* `Stage_001`を最初のステージとして使用
* `Stage_001`のクリアによって`Stage_002`を解放する
* ステージ間の解放関係は`nextStageId`によって管理する

## Sprint 5の実装結果

* [x] Stage SelectからStage_001を開始できる
* [x] Stage_001をクリアするとStage_002が解放される
* [x] 解放状態はPlay終了後も保持される
* [x] 敵全滅によるステージクリアを判定できる
* [x] 味方全滅によるステージ敗北を判定できる
* [x] ステージ終了後にStage Selectへ戻れる
* [x] TBSFの`GameEndConditions`と`GameEnded`を利用してステージ終了処理を構築できる
---
# Sprint 5.5：情報戦基盤・視界・探知

## 目的

戦闘中に敵の位置を完全には把握できない仕組みを導入し、視認・探知によって得られた情報をゲーム内で扱えるようにする。

既存のSkillシステムとは分離し、**非戦闘コマンド**や機体・パイロット・マップ上の設備から共通して利用できる情報戦システムの基盤を構築する。

## 完了条件

* 味方ユニットに視界を設定できる
* 建物などの遮蔽物によって視界が遮られる
* 視界内に入った敵を即座には認識しない
* 敵が視界内に入り、1ターン経過した後に視認情報を取得できる
* 視界による情報取得と探知による情報取得を区別できる
* 機体ごとにレーダー性能を設定できる
* レーダーによって視界外の敵を探知できる
* レーダーの索敵精度と敵のステルスを比較して探知成否を判定できる
* 探知できなかった場合は、敵の存在を示す情報を表示しない
* 敵から取得した情報をプレイヤー側で保持できる
* 現在取得できている敵情報と、実際の敵位置を分離して管理できる
* 戦術マップの基盤を作成できる
* 戦術マップにステージ全体のマップ構造を表示できる
* メイン画面では、その時行動しているユニットの視界のみを表示する
* 戦術マップには、視認・探知によって取得した情報を表示できる

## Sprint 5.5-1

情報戦のデータ構造

## Sprint 5.5-2

ユニットごとの視界範囲

## Sprint 5.5-3

遮蔽物を含む視界判定

## Sprint 5.5-4

「認識待ち → 1ターン経過 → 視認」の処理

## Sprint 5.5-5

レーダー・ステルス・探知判定
### レーダー処理の実装方針

レーダーはSkillとは別の情報戦システムとして実装する。

#### レーダー性能

`XenoUnitData` に以下の情報を追加。

* `radarRange`

  * レーダーが探知できる範囲
* `radarAccuracy`

  * レーダーの索敵精度
* `stealth`

  * 敵側のステルス性能

探知判定では、

```text
detectionDifference = radarAccuracy - stealth
```

を使用する。

* `detectionDifference > 0`

  * 探知成功
* `detectionDifference <= 0`

  * 探知失敗
  * 敵の存在情報を取得しない

探知成功時の情報精度は`detectionDifference`を基準とし、最大5までとする。

例：

* レーダー5 / ステルス1 → 差4 → 情報精度4
* レーダー5 / ステルス3 → 差2 → 情報精度2
* レーダー5 / ステルス5 → 差0 → 探知失敗
* レーダー5 / ステルス7 → 差-2 → 探知失敗
* レーダー8 / ステルス1 → 差7 → 情報精度5（上限）

#### レーダーシステム

`XenoSteelRadarSystem` を作成し、特定の機体やAbilityに依存しない共通のレーダー処理として管理する。

主な処理：

* レーダー範囲内の敵を取得
* レーダー性能とステルスを比較
* 探知成否を判定
* 探知成功時の情報精度を計算
* `XenoSteelInformationManager`へ取得情報を渡す

探知結果は`XenoSteelRadarDetectionResult`として扱い、対象敵と情報精度を保持する。

#### ターン開始時の自動レーダー

各ターンの開始時には、現在行動中のユニットだけではなく、`_turnOrder`に登録されている全ユニットのレーダーを実行する。

```text
ターン開始
↓
全ユニットのレーダーを実行
↓
探知成功した敵情報をInformationManagerへ保存
↓
現在行動中のユニットのターン開始
```

ラウンド開始時に行動順が再構築された場合も、その新しい`_turnOrder`を使用して全ユニットのレーダーを実行する。

これにより、レーダーは「現在行動しているユニットの能力」ではなく、戦場に存在する各ユニットの情報取得手段として機能する。

#### 手動レーダー

自動レーダーとは別に、現在行動中のユニットが任意のタイミングでレーダーを使用できるようにする。

手動レーダーはTBSFの`Ability`として実装する。

仕様：

* 現在行動中のユニットのみ使用可能
* 1ターンにつき1回のみ使用可能
* 使用してもターンは終了しない
* レーダー使用後も移動・攻撃・その他のAbilityを使用できる
* 次のターン開始時に再び使用可能になる
* 手動レーダーでは、使用した現在行動中のユニット自身のレーダーのみを使用する

```text
ユニットのターン開始
↓
全ユニットの自動レーダー
↓
現在行動中のユニットが行動
├─ 移動
├─ 攻撃
├─ その他Ability
└─ 手動レーダー（1ターン1回）
      ↓
    レーダー探知
      ↓
    行動を継続可能
↓
ターン終了
↓
次のユニットへ
```

手動レーダーは通常の攻撃Skillとは異なり、敵を攻撃せず、情報取得のみを行う。

#### 情報管理との接続

レーダーによって取得した情報は、実際の敵ユニットの位置そのものを直接表示するのではなく、`XenoSteelInformationManager`を通してプレイヤー側の敵情報として保存する。

レーダーによる情報は、

* `Source = Radar`
* `State = Confirmed`
* `LastKnownPosition`
* `LastUpdatedRound`
* `InformationPrecision`

を保持する。

視認による情報取得とは`XenoSteelInformationSource`によって区別する。

#### 作成・変更したクラス

* `XenoSteelRadarSystem.cs`

  * レーダー範囲・探知判定・探知結果取得を担当
* `XenoSteelRadarDetectionResult.cs`

  * レーダー探知結果と情報精度を保持
* `XenoSteelInformationManager.cs`

  * レーダーによって取得した敵情報を保存
* `XenoSteelEnemyInformation.cs`

  * レーダー由来の情報と情報精度を保持
* `XenoUnitData.cs`

  * `radarRange`
  * `radarAccuracy`
  * `stealth`
    を保持
* `XenoSteelTurnResolver.cs`

  * ターン開始時の全ユニット自動レーダーを実行
* `XenoSteelRadarAbility.cs`

  * 現在行動中のユニットが任意のタイミングで使用する手動レーダーを担当予定


## Sprint 5.5-6

取得した敵情報の保持

## Sprint 5.5-7

メイン画面への視界反映

## Sprint 5.5-8

戦術マップ基盤

## 実装項目

### 非戦闘コマンド

* [ ] 非戦闘コマンドの基本構造を作成
* [ ] 既存のSkillシステムとは分離して管理
* [ ] 情報戦システムを利用する共通入口を用意

### 視界

* [ ] ユニットごとの視界範囲を設定
* [ ] 建物などの遮蔽物を考慮した視界判定
* [ ] 視界内に入った敵を認識待ち状態として管理
* [ ] 1ターン経過後に視認情報を取得
* [ ] 視界から外れた場合の情報状態を管理

### レーダー

* [ ] `XenoUnitData`などから機体ごとのレーダー性能を設定
* [ ] 索敵精度を設定
* [ ] 敵側のステルス値を設定
* [ ] 索敵精度とステルスを比較する探知判定
* [ ] 視界外の敵を探知可能にする
* [ ] 探知失敗時は敵情報を表示しない
* [ ] レーダー処理を機体以外からも利用できる共通システムとして設計

### 情報管理

* [ ] 実際の敵位置とプレイヤーが取得した情報を分離
* [ ] 視認情報を保存
* [ ] 探知情報を保存
* [ ] 情報取得元を区別できる構造を作成
* [ ] 後続Sprintで戦術マップやAIから利用できる構造にする

### 戦術マップ基盤

* [ ] ステージ全体のマップ構造を表示
* [ ] 建物などのマップ構造を表示
* [ ] 味方ユニットの視界を表示
* [ ] 視認した敵を表示
* [ ] 探知した敵を表示
* [ ] 視認・探知などの情報状態を区別して表示できる構造を作成

### メイン画面

* [ ] 行動中のユニットを基準に表示範囲を制御
* [ ] 行動中のユニットの視界外にある敵を表示しない
* [ ] 視界外の地形・敵情報をメイン画面から隠す
* [ ] 戦術マップとは別に、実際の3D空間としての視界を管理

## 共通システムの利用対象

情報戦システムは、特定の機体機能だけに依存しない構造とする。

将来的に以下から同じ仕組みを利用できるようにする。

* 機体搭載レーダー
* マップ上のレーダー設備
* パイロット固有機能
* 非戦闘コマンド
* その他の情報戦機能

---

# Sprint 5.6：戦術マップ・情報状態の拡張

## 目的

Sprint 5.5で取得できるようになった視認・探知情報を戦術マップ上で管理し、プレイヤーが戦場全体の状況を情報として把握できるようにする。

## 完了条件

* 戦術マップからステージ全体を確認できる
* 視認情報を戦術マップへ反映できる
* レーダーによる探知情報を戦術マップへ反映できる
* 視認と探知で表示方法を区別できる
* 敵が視界外へ移動した場合、最後に確認した位置を情報として扱える
* 古い敵情報を適切に扱える
* 戦術マップを拡大して確認できる
* 行動中のユニットが変わるとメイン画面の視界も切り替わる
* 戦術マップにはメイン画面では確認できないステージ全体の構造が表示される

## 実装項目

* [ ] 戦術マップUI
* [ ] ステージ全体の簡略マップ表示
* [ ] 味方ユニット表示
* [ ] 視認した敵の表示
* [ ] 探知した敵の表示
* [ ] 情報状態ごとの表示
* [ ] 敵の最終確認位置の保存
* [ ] 敵が情報範囲外へ移動した場合の処理
* [ ] 古い情報の扱い
* [ ] 戦術マップの拡大表示
* [ ] 行動ユニット変更時のメイン画面視界更新

---

# Sprint 5.7：通信妨害・通信傍受

## 目的

敵味方間の情報共有そのものを戦闘要素として扱えるようにする。

## 完了条件

* 通信妨害を設定できる
* 通信妨害によって敵の探知を妨害できる
* 敵が視認した味方について、通信妨害によって他の敵へ情報共有できない状態を作れる
* 通信傍受によって敵の行動に関する情報を取得できる
* 通信妨害・通信傍受を機体、パイロット、マップ設備などから利用できる

## 実装項目

### 通信妨害

* [ ] ジャミング性能を設定
* [ ] 敵の探知処理への影響
* [ ] 敵同士の視認情報共有への影響
* [ ] 戦術マップへの情報共有制限の反映

### 通信傍受

* [ ] 敵通信の傍受処理
* [ ] 傍受によって取得できる情報の定義
* [ ] 敵の行動予測に利用できる情報の保存
* [ ] 戦術マップなどへの情報反映

### 共通化

* [ ] 通信妨害処理を共通システム化
* [ ] 通信傍受処理を共通システム化
* [ ] 機体・パイロット・マップ設備・非戦闘コマンドから利用可能にする

---

# Sprint 5.8：情報戦と敵AIの統合

## 目的

敵AIが単純に全ユニットの位置を把握するのではなく、視認・探知・通信によって取得した情報をもとに行動するようにする。

## 完了条件

* 敵AIが自身の視界を利用できる
* 敵AIが自身のレーダーを利用できる
* 敵AIが取得した敵情報を保持できる
* 敵AIが最後に確認した位置を利用できる
* 敵AIが情報を共有できる
* 通信妨害によって敵AIの情報共有を制限できる
* 通信傍受によって敵の行動情報を取得できる
* 敵AIがプレイヤーのユニット位置を常に把握する状態にならない

## 実装項目

* [ ] 敵AIへの視認情報連携
* [ ] 敵AIへの探知情報連携
* [ ] 敵AIの情報保持
* [ ] 最終確認位置の利用
* [ ] 敵ユニット間の情報共有
* [ ] 通信妨害による情報共有制限
* [ ] 通信傍受による情報取得
* [ ] 情報戦を考慮したAI行動判断
* [ ] 未確認の敵を前提としたAI処理

## Sprint 5.8完了後

Sprint 5.5～5.8によって、以下の情報戦システムを一通り成立させる。

* 視認
* 探知
* レーダー
* ステルス
* 情報保持
* 最終確認位置
* 戦術マップ
* 通信妨害
* 通信傍受
* 敵AIへの情報連携

その後、既存のSprint 6へ移行する。

--

# Sprint 6：ユニット編成・成長・セーブ／ロード

## 目的

ステージを攻略するだけでなく、ユニットを編成・成長させ、その状態を次回以降のプレイへ引き継げるようにする。

## 完了条件

* 出撃するユニットを編成できる
* ユニットの経験値を管理できる
* 経験値によってレベルアップできる
* レベルアップによってユニットの成長を反映できる
* レベルに応じてSkillを習得・解放できる
* ゲームの進行状態を保存できる
* 保存した進行状態をロードできる

## 実装項目

* [ ] ユニット編成システム
* [ ] 出撃ユニットの選択
* [ ] パイロット・機体の組み合わせ管理
* [ ] 経験値管理
* [ ] レベルアップ処理
* [ ] レベルアップによるステータス成長
* [ ] レベルに応じたSkill習得・解放
* [ ] 解放済みSkillをSkill選択UIへ反映
* [ ] セーブデータ構造
* [ ] セーブ処理
* [ ] ロード処理
* [ ] ステージ解放状態の保存
* [ ] ユニットの成長状態・編成状態の保存

## 注意点

* `XenoUnitData`は機体そのものの基本データとして引き続き使用する
* ステージごとの初期配置と、プレイヤーが保持しているユニット・成長状態は分離して管理する
* Sprint 5で作成したBattle Sceneを、Sprint 6の編成システムから利用できる構造にする
* セーブ／ロードはステージ単位ではなく、ゲーム全体の進行状態を保存する

---

# Sprint 7：戦闘演出・UI・バランスの強化

## 目的

Sprint 4で構築した最低限の戦闘演出・UIを基盤として、主要ユニット・主要Skillの演出を強化する。

## 完了条件

* Skillごとに異なる戦闘演出を設定できる
* 主要Skillで専用の戦闘演出を再生できる
* 必要なSkillでカットインを表示できる
* Timelineを利用した専用演出を再生できる
* 戦闘UI全体のデザインを統一する
* ステータス・Skillのバランスを調整できる

## 実装項目

* [ ] `XenoSteelCombatPresentationData`を利用したSkillごとの演出設定の拡張
* [ ] `Special`演出の実装
* [ ] Timelineによる専用戦闘演出
* [ ] 専用攻撃アニメーションの追加
* [ ] 攻撃エフェクトの追加
* [ ] 必要なSkillへのカットイン表示
* [ ] 戦闘演出の再生タイミング調整
* [ ] ステータスバランス調整
* [ ] Skillバランス調整
* [ ] 戦闘UIデザイン統一

## Sprint 4から引き継ぐ演出基盤

* `XenoSteelAttackPresentation.cs`

  * 攻撃演出の再生を担当する
* `XenoSteelCombatPresentationData.cs`

  * Skillごとの演出方式を設定する
* `SkillData.presentation`

  * Skillから戦闘演出データを参照する
* `XenoSteelUnitFacing.cs`

  * UnitのFacingを管理する
* `IUnitFacing.cs`

  * TBSFの移動処理とXenoSteelのFacing処理を接続する

Sprint 7ではこれらを利用して、Sprint 4で実装した`SimpleMotion` / `NormalMotion`から、主要Skill向けの`Special`演出へ拡張する。


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
