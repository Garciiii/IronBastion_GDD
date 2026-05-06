# CHECKLIST — Iron Bastion | Configuração Manual no Unity
# O que falta fazer no Unity após copiar os scripts

---

## 1. CONFIGURAÇÃO DO PROJETO

- [ ] Criar projeto Unity 2022.3 LTS como **3D** (não 2D!)
- [ ] `Edit > Project Settings > Player` → Product Name: "Iron Bastion"
- [ ] `Edit > Project Settings > Quality` → ajustar para PC

---

## 2. TAGS E LAYERS

`Edit > Project Settings > Tags and Layers`

**Tags a criar:**
- `Enemy`
- `Tower`
- `Projectile`
- `Waypoint`

**Layers a criar:**
- Layer 6: `Enemy`
- Layer 7: `Tower`
- Layer 8: `Buildable`

---

## 3. BUILD SETTINGS

`File > Build Settings` → adicionar cenas nesta ordem:
- Index 0: `Scenes/MainMenu`
- Index 1: `Scenes/Level1`
- Index 2: `Scenes/Level2`
- Index 3: `Scenes/Level3`

---

## 4. ESTRUTURA DE PASTAS (criar em Assets/)

```
Assets/
├── Scripts/          ← copiar todos os .cs aqui (com subpastas)
├── ScriptableObjects/
├── Data/
│   ├── Towers/
│   ├── Enemies/
│   └── Waves/
│       ├── Level1/
│       ├── Level2/
│       └── Level3/
├── Prefabs/
│   ├── Enemies/
│   ├── Towers/
│   ├── Projectiles/
│   └── UI/
├── Scenes/
├── Materials/
└── Audio/
    ├── Music/
    └── SFX/
```

---

## 5. SCRIPTABLEOBJECTS — Criar Assets

### Torres (Menu: Assets > Create > IronBastion > Tower Data)

**TowerData_Archer.asset**
- Tower Name: "Arqueira"
- Cost: 75 | Upgrade Cost: 38
- Damage Level1: 15 | Damage Level2: 22
- Range Level1: 3.0 | Range Level2: 3.0
- Fire Rate Level1: 1.5 | Fire Rate Level2: 2.1
- Default Targeting: First
- Projectile Speed: 10

**TowerData_Cannon.asset**
- Tower Name: "Canhão"
- Cost: 150 | Upgrade Cost: 75
- Damage Level1: 80 | Damage Level2: 110
- Range Level1: 2.5 | Range Level2: 2.5
- Fire Rate Level1: 0.4 | Fire Rate Level2: 0.4
- Splash Radius Level1: 1.2 | Splash Radius Level2: 1.8
- Default Targeting: First
- Projectile Speed: 5

**TowerData_Ice.asset**
- Tower Name: "Gelo"
- Cost: 125 | Upgrade Cost: 63
- Damage Level1: 0 | Damage Level2: 0
- Range Level1: 2.5 | Range Level2: 3.0
- Slow Percent Level1: 0.5 | Slow Percent Level2: 0.7
- Slow Duration: 0.7 | Slow Pulse Interval: 0.5
- Default Targeting: First
- Projectile Prefab: NONE

**TowerData_Arcane.asset**
- Tower Name: "Arcana"
- Cost: 200 | Upgrade Cost: 100
- Damage Level1: 35 | Damage Level2: 43
- Range Level1: 3.5 | Range Level2: 3.5
- Fire Rate Level1: 0.8 | Fire Rate Level2: 0.8
- DoT Damage Per Sec Level1: 5 | Level2: 8
- DoT Duration Level1: 3 | Level2: 4
- Synergy Radius: 2.5
- Default Targeting: Strongest

### Inimigos (Menu: Assets > Create > IronBastion > Enemy Data)

**EnemyData_Common.asset**
- Enemy Name: "Corrompido Comum"
- Max Health: 100 | Move Speed: 2.0
- Gold Reward: 10 | Lives Lost: 1
- Slow Resistance: 0 | Has Dot Immunity: false
- Model Scale: 1.0
- Placeholder Color: #4CAF50 (verde)
- Always Show Health Bar: false

**EnemyData_Fast.asset**
- Enemy Name: "Corrompido Veloz"
- Max Health: 50 | Move Speed: 4.0
- Gold Reward: 6 | Lives Lost: 1
- Slow Resistance: 0.5 | Has Dot Immunity: false
- Model Scale: 0.8
- Placeholder Color: #FFC107 (amarelo)
- Always Show Health Bar: false

**EnemyData_Tank.asset**
- Enemy Name: "Corrompido Tanque"
- Max Health: 500 | Move Speed: 0.8
- Gold Reward: 40 | Lives Lost: 3
- Slow Resistance: 0 | Has Dot Immunity: true | Dot Immunity Duration: 2
- Model Scale: 1.5
- Placeholder Color: #B71C1C (vermelho escuro)
- Always Show Health Bar: true

### Vagas (Menu: Assets > Create > IronBastion > Wave Data)

Criar 25 assets no total:
- Data/Waves/Level1/ → WaveData_L1_W1 a W5
- Data/Waves/Level2/ → WaveData_L2_W1 a W8
- Data/Waves/Level3/ → WaveData_L3_W1 a W12

Configurar cada uma com base nas tabelas do GDD.
Vagas boss: definir Health Override nos Tanques (L1W5→750, L2W8→900, L3W12→1500).

---

## 6. PREFABS — Criar e Configurar

### Inimigos (Prefabs/Enemies/)

Para cada inimigo (Common, Fast, Tank):
1. Criar GameObject vazio → adicionar Capsule como filho
2. Adicionar componente EnemyBase/EnemyCommon/EnemyFast/EnemyTank
3. Adicionar Rigidbody (Is Kinematic: true, Use Gravity: false)
4. Adicionar CapsuleCollider → Layer: Enemy → Tag: Enemy
5. Adicionar filho com Canvas (World Space, 1x0.1, escala 0.01)
   → Adicionar Slider ou Image para HealthBar
   → Adicionar componente HealthBar
6. Arrastar EnemyData correspondente para o campo "Data"
7. Salvar como Prefab

### Torres (Prefabs/Towers/)

Para cada torre (Archer, Cannon, Ice, Arcane):
1. Criar GameObject vazio → adicionar Cube como filho (escala 0.8)
2. Adicionar componente TowerArcher/TowerCannon/TowerIce/TowerArcane
3. Adicionar BoxCollider → Layer: Tower → Tag: Tower
4. Criar filho vazio "FirePoint" → posicionar no topo (Y+0.5)
5. Arrastar TowerData correspondente para o campo "Tower Data"
6. Arrastar FirePoint para o campo "Fire Point"
7. Salvar como Prefab

### Projéteis (Prefabs/Projectiles/)

**Projectile_Arrow** (para Arqueira e Arcana):
1. Criar GameObject → Sphere (escala 0.15)
2. Adicionar componente ProjectileBase
3. Adicionar SphereCollider (Is Trigger: true) → Tag: Projectile
4. Velocidade: 10 (Arqueira) / 7 (Arcana)

**Projectile_Cannon** (para Canhão):
1. Criar GameObject → Sphere (escala 0.25, material cinzento escuro)
2. Adicionar componente ProjectileSplash
3. Configurar Enemy Layer Mask → Layer "Enemy"
4. Velocidade: 5

---

## 7. CONFIGURAÇÃO DE CENAS

### MainMenu.unity
1. Criar Canvas com UI de menu (botões Jogar, Créditos, Sair)
2. Adicionar script de gestão de menu (pode usar UIManager ou script simples)

### Level1.unity

**Hierarquia necessária:**
```
Level1 (cena)
├── --- MANAGERS ---
│   ├── GameManager        → componente GameManager (Starting Gold: 150, Lives: 10, Next Level: 2)
│   ├── WaveManager        → componente WaveManager (arrastar 5 WaveData)
│   ├── EnemySpawner       → componente EnemySpawner
│   ├── ObjectPool         → componente ObjectPool
│   └── GridManager        → componente GridManager
├── --- MAPA ---
│   ├── PathManager        → componente PathManager
│   │   ├── WP0 (pos: -1, 0, 9)
│   │   ├── WP1 (pos: 4, 0, 9)
│   │   ├── WP2 (pos: 4, 0, 4)
│   │   ├── WP3 (pos: 12, 0, 4)
│   │   └── WP4 (pos: 12, 0, 9)
│   └── Grid               → plano 3D 15x10 com células GridCell
├── --- CÂMARA ---
│   └── Main Camera        → Perspective, pos: (7, 10, 5), rot: (-60, 0, 0)
└── --- UI ---
    └── Canvas (Screen Space Overlay)
        ├── HUD (ouro, vidas, vaga, botão iniciar, botão pausa)
        ├── TowerShopUI (painel inferior)
        ├── TowerInfoPanel (painel lateral, começa inativo)
        ├── PausePanel (começa inativo)
        ├── VictoryPanel (começa inativo)
        └── GameOverPanel (começa inativo)
```

**Configurar câmara:**
- Position: (7, 10, 5) para Nível 1
- Rotation: (-60, 0, 0)
- Background: cor sólida #0D1B2A

**Configurar Grid:**
- Criar plano 15x10 de células (GameObjects com BoxCollider plano, Layer: Buildable)
- Células no caminho: CellState = Blocked (marcar no Inspector)
- Células construíveis: CellState = Buildable

**Lighting:**
- Directional Light: ligeiramente inclinado (Ex: rot -30, 45, 0)
- Ambient Color: #1A2A3A (azul escuro para humor steampunk)

### Level2.unity e Level3.unity
- Seguir a mesma estrutura mas com:
  - Level2: Grid 20x12, 8 waypoints, Starting Gold: 200, Next Level: 3, arrastar 8 WaveDatas
  - Level3: Grid 22x14, 9 waypoints, Starting Gold: 250, Next Level: -1, arrastar 12 WaveDatas

---

## 8. CONFIGURAR UIManager

No GameObject Canvas do Level1:
1. Arrastar UIManager script
2. Ligar todos os campos:
   - Text Gold → TextMeshPro do ouro no HUD
   - Text Lives → TextMeshPro das vidas
   - Text Wave → TextMeshPro da vaga
   - Text Countdown → TextMeshPro do countdown
   - Button Start Wave → botão "Iniciar Vaga"
   - Button Pause → botão de pausa
   - Panel Pause/Victory/GameOver → painéis respetivos
   - Star Objects → 3 GameObjects de estrelas no painel de vitória

---

## 9. CONFIGURAR TowerShopUI

No painel de loja:
1. Arrastar TowerShopUI script para o painel
2. Para cada torre disponível no nível:
   - Arrastar o TowerData correspondente
   - Arrastar o Button
   - Ligar labels de nome e custo

---

## 10. TESTAR ANTES DA ENTREGA 3

- [ ] Compilar sem erros na consola Unity
- [ ] Inimigos spawnam e seguem o caminho corretamente
- [ ] Torres disparam e causam dano
- [ ] Ouro é ganho ao eliminar inimigos
- [ ] Vidas são perdidas ao inimigo chegar à base
- [ ] Game Over quando vidas = 0
- [ ] Vitória após última vaga
- [ ] Upgrade funciona e debita ouro
- [ ] Venda devolve 50% do investimento
- [ ] Pausa com Escape ou botão
- [ ] Build .exe funciona sem Unity instalado

---

## 11. PALETA DE MATERIAIS (criar em Materials/)

| Material | Cor Hex | Uso |
|---|---|---|
| Mat_Path | #795548 | Caminho dos inimigos |
| Mat_Buildable | #1B5E20 | Células construíveis |
| Mat_HighlightValid | #4CAF50 (alpha 0.7) | Hover válido |
| Mat_HighlightInvalid | #F44336 (alpha 0.7) | Hover inválido |
| Mat_Enemy_Common | #4CAF50 | Corrompido Comum |
| Mat_Enemy_Fast | #FFC107 | Corrompido Veloz |
| Mat_Enemy_Tank | #B71C1C | Corrompido Tanque |
| Mat_Tower_Archer | #FFC107 | Torre Arqueira |
| Mat_Tower_Cannon | #607D8B | Torre Canhão |
| Mat_Tower_Ice | #80DEEA | Torre Gelo |
| Mat_Tower_Arcane | #CE93D8 | Torre Arcana |

---

*Iron Bastion | ISTEC Lisboa | UC Gamificação 2024/25*
