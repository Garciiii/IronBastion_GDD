# SETUP MANUAL — Iron Bastion
## Tower Defense 3D Isométrico | Unity 6 (6000.0.73f1)
## UC Gamificação | ISTEC Lisboa | Grupo Iron Bastion | 2024/25

---

> **ATENÇÃO:** Este ficheiro contém APENAS as configurações que o código não pode fazer
> automaticamente (Tags, Layers, prefabs, cena, assets). Siga a ordem abaixo.

---

## 0. ABRIR O PROJETO

1. Abrir o Unity Hub → **Open** → selecionar a pasta `IronBastion_GDD/`
2. Versão obrigatória: **6000.0.73f1**
3. Aguardar compilação inicial (pode demorar 2–5 minutos)
4. **0 erros na consola** antes de avançar

---

## 1. TAGS

**Edit → Project Settings → Tags and Layers → Tags**

Adicionar (botão `+`):
| Tag |
|-----|
| `Enemy` |
| `Tower` |
| `Projectile` |
| `Waypoint` |

---

## 2. LAYERS

**Edit → Project Settings → Tags and Layers → Layers**

| User Layer | Nome |
|------------|------|
| Layer 6 | `Enemy` |
| Layer 7 | `Tower` |
| Layer 8 | `Buildable` |

---

## 3. PHYSICS COLLISION MATRIX

**Edit → Project Settings → Physics → Layer Collision Matrix**

- Desativar colisões desnecessárias para performance:
  - `Enemy` vs `Enemy`: **OFF**
  - `Tower` vs `Tower`: **OFF**
  - `Projectile` vs `Projectile`: **OFF**
  - `Projectile` vs `Tower`: **OFF**
  - `Projectile` vs `Buildable`: **OFF**
- Manter **ON**: `Projectile` vs `Enemy`, `Tower` vs `Enemy`

---

## 4. CRIAR ScriptableObject ASSETS

### 4.1 EnemyData Assets

**Right-click em `Assets/_Project/Resources/Data/Enemies/` → Create → IronBastion → Enemy Data**

Criar 3 assets com estes valores:

#### EnemyData_Common
| Campo | Valor |
|-------|-------|
| enemyName | Corrompido Comum |
| maxHealth | 100 |
| moveSpeed | 3.5 |
| goldReward | 10 |
| livesLost | 1 |
| slowResistance | 0 |
| hasDotImmunity | false |
| modelScale | 0.8 |
| placeholderColor | `(0.2, 0.7, 0.2, 1)` — verde |
| alwaysShowHealthBar | false |

#### EnemyData_Fast
| Campo | Valor |
|-------|-------|
| enemyName | Corrompido Veloz |
| maxHealth | 50 |
| moveSpeed | 7 |
| goldReward | 6 |
| livesLost | 1 |
| slowResistance | 0.5 |
| hasDotImmunity | false |
| modelScale | 0.65 |
| placeholderColor | `(1.0, 0.8, 0.0, 1)` — amarelo |
| alwaysShowHealthBar | false |

#### EnemyData_Tank
| Campo | Valor |
|-------|-------|
| enemyName | Corrompido Tanque |
| maxHealth | 500 |
| moveSpeed | 1.2 |
| goldReward | 40 |
| livesLost | 3 |
| slowResistance | 0 |
| hasDotImmunity | **true** |
| dotImmunityDuration | 2 |
| modelScale | 1.3 |
| placeholderColor | `(0.6, 0.1, 0.1, 1)` — vermelho escuro |
| alwaysShowHealthBar | **true** |

---

### 4.2 TowerData Assets

**Right-click em `Assets/_Project/Resources/Data/Towers/` → Create → IronBastion → Tower Data**

Criar 4 assets:

#### TowerData_Archer
| Campo | Valor |
|-------|-------|
| towerName | Torre Arqueira |
| cost | 75 |
| upgradeCost | 38 |
| damageLevel1 | 15 |
| damageLevel2 | 22 |
| rangeLevel1 | 5 |
| rangeLevel2 | 5 |
| fireRateLevel1 | 1.5 |
| fireRateLevel2 | 2.1 |
| defaultTargeting | First |
| projectileSpeed | 12 |

#### TowerData_Cannon
| Campo | Valor |
|-------|-------|
| towerName | Torre Canhão |
| cost | 150 |
| upgradeCost | 75 |
| damageLevel1 | 80 |
| damageLevel2 | 110 |
| rangeLevel1 | 4 |
| rangeLevel2 | 4 |
| fireRateLevel1 | 0.4 |
| fireRateLevel2 | 0.4 |
| splashRadiusLevel1 | 1.5 |
| splashRadiusLevel2 | 2.2 |
| defaultTargeting | First |
| projectileSpeed | 8 |

#### TowerData_Ice
| Campo | Valor |
|-------|-------|
| towerName | Torre de Gelo |
| cost | 125 |
| upgradeCost | 63 |
| damageLevel1 | 0 |
| damageLevel2 | 0 |
| rangeLevel1 | 4 |
| rangeLevel2 | 5 |
| fireRateLevel1 | 0 |
| fireRateLevel2 | 0 |
| slowPercentLevel1 | 0.5 |
| slowPercentLevel2 | 0.5 |
| slowDuration | 0.8 |
| slowPulseInterval | 0.5 |
| projectilePrefab | **null** (sem projétil) |

#### TowerData_Arcane
| Campo | Valor |
|-------|-------|
| towerName | Torre Arcana |
| cost | 200 |
| upgradeCost | 100 |
| damageLevel1 | 35 |
| damageLevel2 | 43 |
| rangeLevel1 | 6 |
| rangeLevel2 | 6 |
| fireRateLevel1 | 0.8 |
| fireRateLevel2 | 0.8 |
| dotDamagePerSecLevel1 | 5 |
| dotDamagePerSecLevel2 | 8 |
| dotDurationLevel1 | 3 |
| dotDurationLevel2 | 4 |
| synergyRadius | 4 |
| defaultTargeting | Strongest |
| projectileSpeed | 10 |

---

### 4.3 WaveData Assets — Nível 1

**Right-click em `Assets/_Project/Resources/Data/Waves/Level1/` → Create → IronBastion → Wave Data**

Criar 5 assets `L1_Wave1` a `L1_Wave5`:

#### L1_Wave1
| Campo | Valor |
|-------|-------|
| waveNumber | 1 |
| spawnInterval | 0.8 |
| enemyGroups[0] | enemyData=EnemyData_Common, count=8, healthOverride=0 |

#### L1_Wave2
| Campo | Valor |
|-------|-------|
| waveNumber | 2 |
| spawnInterval | 0.8 |
| enemyGroups[0] | enemyData=EnemyData_Common, count=10, healthOverride=0 |
| enemyGroups[1] | enemyData=EnemyData_Fast, count=2, healthOverride=0 |

#### L1_Wave3
| Campo | Valor |
|-------|-------|
| waveNumber | 3 |
| spawnInterval | 0.7 |
| enemyGroups[0] | enemyData=EnemyData_Common, count=12, healthOverride=0 |
| enemyGroups[1] | enemyData=EnemyData_Fast, count=4, healthOverride=0 |

#### L1_Wave4
| Campo | Valor |
|-------|-------|
| waveNumber | 4 |
| spawnInterval | 0.7 |
| enemyGroups[0] | enemyData=EnemyData_Common, count=8, healthOverride=0 |
| enemyGroups[1] | enemyData=EnemyData_Fast, count=6, healthOverride=0 |
| enemyGroups[2] | enemyData=EnemyData_Tank, count=1, healthOverride=0 |

#### L1_Wave5 (Boss Wave)
| Campo | Valor |
|-------|-------|
| waveNumber | 5 |
| isBossWave | **true** |
| spawnInterval | 0.6 |
| enemyGroups[0] | enemyData=EnemyData_Common, count=5, healthOverride=0 |
| enemyGroups[1] | enemyData=EnemyData_Fast, count=5, healthOverride=0 |
| enemyGroups[2] | enemyData=EnemyData_Tank, count=1, healthOverride=**750** ← Boss! |

---

## 5. CRIAR PREFABS DE INIMIGOS

### Estrutura base de todos os inimigos:

1. **GameObject → 3D Object → Capsule** (corpo principal)
2. Configurar no Inspector:
   - Tag: `Enemy`
   - Layer: `Enemy`
   - Rigidbody: `Is Kinematic = true`, `Use Gravity = false`
   - CapsuleCollider: manter padrão
3. Adicionar filho `HealthBarCanvas`:
   - Add Component: **Canvas** → Render Mode: **World Space**
   - Scale: `(0.01, 0.01, 0.01)`, Width: `100`, Height: `15`
   - Adicionar filho **Image** (background cinzento `#333333`)
   - Adicionar filho **Image** (fill vermelho → amarelo → verde, type: **Filled**, Horizontal)
   - Add Component: **HealthBar** no GameObject do Canvas
     - Arrastar `fillImage` para o campo correspondente
     - Configurar `healthGradient`:
       - Key 0% → vermelho `#E53935`
       - Key 50% → amarelo `#FDD835`
       - Key 100% → verde `#43A047`
4. Add Component: **EnemyCommon** / **EnemyFast** / **EnemyTank**
   - Arrastar o `EnemyData_Common` (ou Fast/Tank) para o campo `data`
5. Salvar em `Assets/_Project/Prefabs/Enemies/`

---

## 6. CRIAR PREFABS DE TORRES

### Estrutura base de todas as torres:

1. **GameObject → 3D Object → Cylinder** (base)
   - Escala: `(0.8, 0.3, 0.8)`
2. Adicionar filho **Cube** (corpo/torre):
   - Escala: `(0.5, 1.2, 0.5)`, posição Y: `0.75`
3. Adicionar filho **GameObject vazio** chamado `FirePoint`:
   - Posição: `(0, 1.2, 0.5)`
4. Configurar no Inspector:
   - Tag: `Tower`
   - Layer: `Tower`
   - Add Component: **BoxCollider** em Trigger para clique (Is Trigger: true)
   - Add Component: `TowerArcher` / `TowerCannon` / `TowerIce` / `TowerArcane`
   - Arrastar o TowerData correspondente para o campo `towerData`
   - Arrastar `FirePoint` para o campo `firePoint`
5. **Cores do MeshRenderer** (aplicar ao Cube filho):
   - Arqueira: `(1.0, 0.8, 0.1)` — dourado
   - Canhão: `(0.3, 0.3, 0.35)` — cinzento metálico
   - Gelo: `(0.4, 0.8, 1.0)` — azul claro
   - Arcana: `(0.7, 0.1, 0.9)` — roxo
6. Salvar em `Assets/_Project/Prefabs/Towers/`

---

## 7. CRIAR PREFABS DE PROJÉTEIS

### Projétil Base / Arqueira
1. **GameObject → 3D Object → Sphere**, escala `(0.2, 0.2, 0.2)`
2. Add Component: **ProjectileBase**
3. Material: amarelo `(1.0, 0.9, 0.0)`
4. Layer: `Projectile`; Tag: `Projectile`
5. Salvar: `Prefabs/Projectiles/Projectile_Arrow.prefab`

### Projétil Splash / Canhão
1. Sphere escala `(0.35, 0.35, 0.35)`, material cinzento escuro
2. Add Component: **ProjectileSplash**
   - Enemy Layer Mask: `Enemy`
3. Salvar: `Prefabs/Projectiles/Projectile_Cannonball.prefab`

### Projétil DoT / Arcana
1. Sphere escala `(0.25, 0.25, 0.25)`, material roxo `(0.7, 0.1, 0.9)`
2. Add Component: **ProjectileDoT**
   - dotDamagePerSecond: `5` (fallback; Torre Arcana sobrepõe em runtime)
   - dotDuration: `3`
3. Salvar: `Prefabs/Projectiles/Projectile_Arcane.prefab`

---

## 8. LIGAR PROJÉTEIS ÀS TORRES

No TowerData de cada torre, arrastar o prefab de projétil correto:
- TowerData_Archer → `Projectile_Arrow`
- TowerData_Cannon → `Projectile_Cannonball`
- TowerData_Ice → **null** (sem projétil — usa OverlapSphere)
- TowerData_Arcane → `Projectile_Arcane`

---

## 9. MONTAR A CENA LEVEL1

### 9.1 Estrutura de GameObjects na Hierarquia

```
Level1 (cena)
├── --- MANAGERS ---
│   ├── GameManager         → Add: GameManager (startingGold=150, startingLives=10)
│   ├── WaveManager         → Add: WaveManager (timeBetweenWaves=15)
│   │                           waves: arrastar L1_Wave1..L1_Wave5 em ordem
│   ├── EnemySpawner        → Add: EnemySpawner
│   │   └── SpawnPoint      → Transform em (-2, 0.5, 10)
│   ├── ObjectPool          → Add: ObjectPool
│   └── PathManager         → Add: PathManager
│       ├── WP1             → Position (8, 0.5, 10)
│       ├── WP2             → Position (8, 0.5, 4)
│       ├── WP3             → Position (20, 0.5, 4)
│       ├── WP4             → Position (20, 0.5, 14)
│       └── EndPoint        → Position (30, 0.5, 14)
│
├── --- CÂMARA ---
│   └── Main Camera
│       Position: (0, 18, -12)
│       Rotation: (50, 45, 0)
│       Projection: Orthographic
│       Size: 9
│       Background: #0D1B2A
│       Clear Flags: Solid Color
│       → Add: CameraController
│          panLimitX: (-5, 35)
│          panLimitZ: (-5, 25)
│
├── --- GRID ---
│   └── GridManager         → Add: GridManager
│       gridWidth: 15
│       gridHeight: 10
│       cellSize: 2
│       originPosition: (0, 0, 0)
│       pathTiles: ver tabela abaixo
│
├── --- ILUMINAÇÃO ---
│   └── Directional Light   → Rotation (50, 45, 0), Intensity 1.2
│
└── --- UI ---
    └── Canvas (Screen Space Overlay)
        ├── HUD
        │   ├── TextGold    → TMP "150"
        │   ├── TextLives   → TMP "10"
        │   └── TextWave    → TMP "VAGA 0/5"
        ├── WaveControls    → Add: WaveUI
        │   ├── ButtonStartWave
        │   ├── TextCountdown
        │   └── TextWaveStatus
        ├── TowerShop       → Add: TowerShopUI
        ├── TowerInfoPanel  → Add: TowerInfoPanel (inactive)
        ├── PausePanel      (inactive)
        ├── VictoryPanel    (inactive)
        └── GameOverPanel   (inactive)
```

---

### 9.2 PathTiles — configurar no GridManager

No campo `pathTiles` (List<Vector2Int>) do GridManager, adicionar estes tiles:

**Linha 5** (coluna, 5): (0,5) (1,5) (2,5) (3,5) (4,5)
**Coluna 4** (4, linha): (4,4) (4,3) (4,2)
**Linha 2** (coluna, 2): (5,2) (6,2) (7,2) (8,2) (9,2) (10,2)
**Coluna 10** (10, linha): (10,3) (10,4) (10,5) (10,6) (10,7)
**Linha 7** (coluna, 7): (11,7) (12,7) (13,7) (14,7)

Total: **22 tiles de caminho** (marcados a castanho automaticamente pelo GridManager)

---

### 9.3 Ligar Referências no Inspector

**EnemySpawner:**
- `spawnPoint` → arrastar o Transform `SpawnPoint`

**WaveManager:**
- `waves` → arrastar L1_Wave1, L1_Wave2, L1_Wave3, L1_Wave4, L1_Wave5 (por ordem)

**UIManager / WaveUI:**
- Arrastar todos os campos TextMeshProUGUI e Button correspondentes

**TowerShopUI:**
- Preencher os arrays de prefabs, custos e estado de desbloqueio:
  - Index 0: TowerArcher prefab, custo 75, unlocked=true
  - Index 1: TowerCannon prefab, custo 150, unlocked=true
  - Index 2: TowerIce prefab, custo 125, unlocked=true
  - Index 3: TowerArcane prefab, custo 200, unlocked=true

---

## 10. CONFIGURAR BUILD SETTINGS

**File → Build Settings:**

1. Adicionar cenas:
   - Slot 0: `MainMenu` (se existir)
   - Slot 1: `Level1`
2. Platform: **PC, Mac & Linux Standalone**
3. Architecture: **x86_64**
4. Compression: **LZ4HC**
5. **Build** → pasta `/Build/`

---

## 11. CHECKLIST FINAL DE TESTES

Antes de entregar, verificar **cada ponto** na consola Unity (Play Mode):

| # | Teste | OK? |
|---|-------|-----|
| 1 | 0 erros de compilação na consola | ☐ |
| 2 | Inimigos seguem os waypoints sem desvios | ☐ |
| 3 | Barras de vida aparecem ao tomar dano | ☐ |
| 4 | Tanque tem barra de vida sempre visível | ☐ |
| 5 | Torres rodam para o alvo e disparam | ☐ |
| 6 | Projéteis do Canhão causam dano em área | ☐ |
| 7 | Torre de Gelo abranda inimigos (sem projétil) | ☐ |
| 8 | Torre Arcana aplica DoT (inimigo perde vida gradualmente) | ☐ |
| 9 | Ouro atualiza no HUD ao matar inimigos | ☐ |
| 10 | Vidas diminuem quando inimigo chega ao fim | ☐ |
| 11 | Game Over ao chegar a 0 vidas (painel aparece) | ☐ |
| 12 | Vitória ao completar as 5 vagas | ☐ |
| 13 | Estrelas calculadas corretamente (8-10 = 3★) | ☐ |
| 14 | Upgrade de torres funciona (custo debitado) | ☐ |
| 15 | Venda devolve 50% do total investido | ☐ |
| 16 | Pausa (Escape) funciona (Time.timeScale=0) | ☐ |
| 17 | Pan da câmara funciona (botão do meio + WASD) | ☐ |
| 18 | Botão "Iniciar Vaga" ativa/desativa corretamente | ☐ |
| 19 | Countdown entre vagas funciona | ☐ |
| 20 | Build .exe corre fora do editor sem erros | ☐ |

---

## 12. ESTRUTURA FINAL DE PASTAS

```
Assets/
└── _Project/
    ├── Prefabs/
    │   ├── Enemies/       EnemyCommon.prefab, EnemyFast.prefab, EnemyTank.prefab
    │   ├── Towers/        TowerArcher.prefab, TowerCannon.prefab, TowerIce.prefab, TowerArcane.prefab
    │   └── Projectiles/   Projectile_Arrow.prefab, Projectile_Cannonball.prefab, Projectile_Arcane.prefab
    ├── Resources/
    │   └── Data/
    │       ├── Enemies/   EnemyData_Common.asset, EnemyData_Fast.asset, EnemyData_Tank.asset
    │       ├── Towers/    TowerData_Archer.asset, TowerData_Cannon.asset, TowerData_Ice.asset, TowerData_Arcane.asset
    │       └── Waves/
    │           └── Level1/  L1_Wave1.asset ... L1_Wave5.asset
    ├── Scenes/            Level1.unity, MainMenu.unity
    └── Scripts/           29 ficheiros .cs já criados pelo Claude Code — prontos a usar
```

---

*Iron Bastion | UC Gamificação | ISTEC Lisboa | 2024/25*
*Tower Defense 3D Isométrico | Unity 6 (6000.0.73f1)*
