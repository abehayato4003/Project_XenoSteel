# XenoSteel フォルダ構成（最終版）

## 1. ルート構成

- **XenoSteel/**
  - `Art/`
  - `Prefabs/`
  - `CombatAnim/`
  - `Scripts/`
  - `Data/`
  - `Scenes/`

---

## 2. Art

- **Art/**
  - **Models/**: メカ・敵モデル
  - **Textures/**
  - **Materials/**
  - **Animations/**

---

## 3. Prefabs

- **Prefabs/**
  - **Units/**: PlayerUnit, EnemyUnit
  - **Cells/**: SquareCell, XenoCell
  - **UI/**
  - **Effects/**: 汎用エフェクト

---



---

## 4. CombatAnim（メカごとの技演出）

- **CombatAnim/**
  - **Mechs/**
    - **Astar/**（例）
      - **m-01/**(Meleeのm)
        - `Timeline.playable`
        - **Animations/**
        - **Effects**
        - **Cutins/**
      - **m-02/**
      - **m-03/**
      - **r-01/**(Rangeのr)
      - **r-02/**
    - **P-As/**（例）
      - **m-01/**
      - **m-02/**
      - **r-01/**
    - **Other/**
      - **m-01/**
      - **m-02/**
      - **r-01/**
  - **Backgrounds（共通背景）/**
    - **Space/**
    - **Plain/**
    - **City/**
    - **Base/**
    - **other/**



---

## 5. Scripts

- **Scripts/**
  - **Core/**
  - **Units/**
  - **AI/**
  - **Events/**
  - **Story/**
  - **Combat/**（技演出の再生制御）

---

## 6. Data

- **Data/**
  - **Units/**
  - **Story/**
  - **Stages/**

---

## 7. Scenes

- **Scenes/**
  - **Battle/**
  - **Dialogue/**
  - **Combat/**（技演出専用シーン）
