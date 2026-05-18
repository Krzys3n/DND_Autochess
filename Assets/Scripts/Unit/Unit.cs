using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public enum ConditionType
{
    Stunned, Paralyzed, Silenced, Disarmed, Burned, 
    Wounded, Chilled, Corroded,Hexed, Blinded, Petrified, 
    Poisoned, Prone, Invulnerable, Immortal
}

public enum DamageType { Physical, Magic, True }

[System.Serializable]
public class ShieldInstance
{
    public int amount;
    public float expirationTime; // Time.time + duration
    public bool isPermanent;

    public ShieldInstance(int amount, float duration)
    {
        this.amount = amount;
        this.isPermanent = duration <= 0;
        this.expirationTime = isPermanent ? -1f : Time.time + duration;
    }

    public bool IsExpired => !isPermanent && Time.time >= expirationTime;
}

public class Unit : MonoBehaviour
{
    [Header("Data Source")]
    public UnitData unitData;   // Dla jednostek gracza
    public EnemyData enemyData; // Dla potworów
    
    [Header("Evolution State")]
    public int evolutionLevel = 1; // 1 = Bronze, 2 = Silver, 3 = Gold

    [Header("Identity (Loaded from SO)")]
    public string unitName;
    public SynergyData faction;
    public List<SynergyData> activeClasses = new List<SynergyData>();

    [Header("Working Stats")]
    public int maxHP;
    public int currentHP;
    public int attack;
    public float vampirism = 0f; // 0.2f = 20% wampiryzmu
    // public int magicAttack; // Możesz to usunąć, jeśli już nie używasz
    public int defense;
    public int magicDefense;
    public float damageReduction = 0f;
    public float critChance;
    public float critDamage = 1.4f;
    public bool canAbilitiesCrit = false;
    public float attackSpeed; 
    public float attackRange; 
    public int abilityPower; // To zostaje (jako mnożnik, np. 100)

    // DODAJ TE TRZY LINIE PONIŻEJ:
    public int baseAbilityPower; // Baza obrażeń umiejętności
    public float hitChance;      // Szansa na trafienie (np. 1.0)
    public float dodgeChance;    // Szansa na unik (np. 0.05)
    // unitTier dla rzadkości (rarity) ze sklepu
    public int unitTier = 1; 
    public bool isEnemy; 

    [Header("Item Bonuses (Flat)")]
    public int itemHealthBonus;
    public int itemAttackBonus;
    public float itemVampirismBonus;
    public int itemArmorBonus;
    public int itemMagicResistBonus;
    public int itemAbilityPowerBonus;
    public int itemStartingManaBonus;
    public int itemManaRegenBonus;

    [Header("Item Bonuses (Percent)")]
    public float itemAttackSpeedPercent;
    public float itemCritChancePercent;
    public float itemAttackDamagePercent;
    public float itemHealthPercentBonus;
    public float itemArmorPercentBonus;
    public float itemMagicResistPercentBonus;

    [Header("Item specific counters")]
    public float stackADPercent = 0f; // np. 0.02f za każdy stack
    public float stackASPercent = 0f; // np. 0.02f za każdy stack
    public int bonusDefenseFromStacks = 0; // Dodaj to pole!
    public int bonusMagicDefenseFromStacks = 0; // Dodaj to pole!
    public int dwarvenStacks = 0;
    public int staffOfLightningHits = 0;
    public int flickerStacks = 0; // Licznik dla Scimitar of Speed
    public float quiverAuraBonus = 0f;
    public int flamingFuryHitCounter = 0; // Liczy otrzymane ciosy
    public bool guaranteedNextCrit = false; // Flaga dla następnego ataku
    private float itemTickTimer = 0f;
    private const float ITEM_TICK_RATE = 0.5f; // Aura odświeża się co 0.5 sekundy
    public int currentItemTick = 0;
    public bool hasSpellShield = false; // Flaga Tarczy Antymagicznej
    public bool immuneToCrits = false;
    public bool immuneToForcedMovement = false;
    public int archmagiStacks = 0;
    public ShieldInstance currentArchmagiShield;

    [Header("Temporary Items (Thief's Gloves)")]
    [HideInInspector] public int tempItemCount = 0;


    [Header("Mana System")]
    public int maxMana = 100;       
    public int currentMana = 0;     
    public int manaPerAttack = 10;  
    private float manaAccumulator = 0f; // Pomocnicza zmienna do płynnego naliczania many
    public float currentManaRegen;     // Wartość wyliczona w RecalculateStats

    [Header("Shield System")]
    public List<ShieldInstance> activeShields = new List<ShieldInstance>();

    public int CurrentTotalShield
    {
        get
        {
            int total = 0;
            foreach (var s in activeShields) total += s.amount;
            return total;
        }
    }

    [Header("Modular Ability")]
    public Ability activeAbility; // NOWOŚĆ: Przyjmuje ScriptableObject umiejętności

    private float attackTimer = 0f; 

    [Header("Status Flags")]
    public bool isTargetable = true; // Domyślnie każdy jest widoczny
    // Dodaj tę właściwość, aby ułatwić sprawdzanie w innych skryptach
    public bool CanBeTargeted => isTargetable && !HasCondition(ConditionType.Petrified);

    [Header("UI")]
    public GameObject uiPrefab; 

    [HideInInspector] public Unit targetEnemy; 
    [HideInInspector] public Tile currentTile;
    private GameObject uiInstance; 
    private SpriteRenderer spriteRenderer;

    private Vector3 battleStartPosition;
    private Tile battleStartTile;

    [Header("Equipment & UI Slots")]
    public List<ItemData> equippedItems = new List<ItemData>();
    public int maxItems = 3;
    public Image[] itemSlotIcons; // Przeciągnij tu Hold_Item_Image 1, 2, 3 z Inspectora

    [Header("Round Stats")]
    public float roundPhysDamageDealt = 0;
    public float roundMagicDamageDealt = 0;
    public float roundTrueDamageDealt = 0; // NOWE
    public float roundPhysDamageTanked = 0;  // NOWE
    public float roundMagicDamageTanked = 0; // NOWE
    public float roundHealingDone = 0;        // NOWE      
    public float roundShieldingGenerated = 0; // Ile tarczy rzucono (surowa wartość)
    public float roundShieldingAbsorbed = 0;  // Ile obrażeń tarcza faktycznie przyjęła
  
    [Header("Projectile Settings")]
    public GameObject defaultProjectilePrefab; // Przeciągnij tu swój Sprite z skryptem Projectile
    public float projectileSpeed = 10f;
    public Transform firePoint; // Opcjonalnie: pusty GameObject np. przy dłoni unita

    [HideInInspector] public UnitStatusManager statusManager;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        statusManager = GetComponent<UnitStatusManager>();
        if (statusManager == null) statusManager = gameObject.AddComponent<UnitStatusManager>();
    }

    void Start()
    {
        // 1. Najpierw tworzymy pustą listę (bezpieczne dla każdego)
        activeClasses = new List<SynergyData>();

        // 2. Sprawdzamy, czy to Gracz czy Przeciwnik
        if (unitData != null)
        {
            // Gracz: Kopiujemy klasy z jego SO
            if (unitData.unitClasses != null)
            {
                activeClasses = new List<SynergyData>(unitData.unitClasses);
            }
            LoadDataFromSO();
        }
        else if (enemyData != null)
        {
            // Przeciwnik: Ładujemy dedykowane dane wroga
            LoadEnemyData();
        }

        // Inicjalizacja stanu
        currentHP = maxHP;
        attackTimer = 0f;
        
        // Szukamy UI
        UnitUI ui = GetComponentInChildren<UnitUI>(true); 
        if (ui != null)
        {
            ui.Setup(this);
            uiInstance = ui.gameObject; 
        }

        ApplyEvolutionVisuals();
    }

    void Update()
    {
            // Jeśli jednostka nie może się ruszać/atakować
        if (HasCondition(ConditionType.Stunned) || 
            HasCondition(ConditionType.Petrified) || 
            HasCondition(ConditionType.Paralyzed)) 
        {
            return; // Przerywamy Update, jednostka nic nie robi
        }
        // Tylko jeśli trwa walka i jednostka żyje
        if (GameManager.Instance.currentState == GameState.Combat && currentHP > 0)
        {
            itemTickTimer += Time.deltaTime;
            if (itemTickTimer >= ITEM_TICK_RATE)
            {
                itemTickTimer = 0f;
                currentItemTick++;
                TriggerItemTicks();
            }
        }
        // 1. Sprawdzenie pozycji
        bool isActuallyOnBench = (currentTile != null && currentTile.isBenchSlot);

        // 2. Zarządzanie paskami UI
        if (uiInstance != null)
        {
            uiInstance.SetActive(!isActuallyOnBench);
        }

        // 3. Logika Walki i Regeneracji
        if (GameManager.Instance != null && GameManager.Instance.currentState == GameState.Combat)
        {
            if (!isActuallyOnBench)
            {
                // --- NOWA LOGIKA REGENERACJI MANY ---
                if (currentMana < maxMana && currentManaRegen > 0)
                {
                    // Dodajemy ułamek many do akumulatora
                    manaAccumulator += currentManaRegen * Time.deltaTime;

                    // Jeśli uzbieraliśmy przynajmniej 1 punkt many
                    if (manaAccumulator >= 1f)
                    {
                        int manaToAdd = Mathf.FloorToInt(manaAccumulator);
                        currentMana = Mathf.Min(maxMana, currentMana + manaToAdd);
                        manaAccumulator -= manaToAdd; // Zostawiamy resztę w akumulatorze
                    }
                }
                // ------------------------------------

                HandleCombatLogic();
            }
            else 
            {
                targetEnemy = null;
                manaAccumulator = 0f; // Resetujemy na ławce
            }
            
        }

        if (activeShields.Count > 0)
        {
            CleanupExpiredShields();
        }

    }

    private void OnValidate()
    {
        if (unitData != null || enemyData != null)
        {
            
            ApplyEvolutionVisuals();
        }
    }

    public void LoadDataFromSO()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (unitData != null)
        {
            unitName = unitData.unitName;
            faction = unitData.faction;
            activeClasses = new List<SynergyData>(unitData.unitClasses);

            // TYLKO BAZA, ZERO MATEMATYKI I PRZEDMIOTÓW
            if (evolutionLevel == 1) {
                maxHP = unitData.maxHP; attack = unitData.attack; 
                defense = unitData.defense; magicDefense = unitData.magicDefense;
                baseAbilityPower = unitData.baseAbilityPower;
            }
            else if (evolutionLevel == 2) {
                maxHP = unitData.maxHP_Lvl2; attack = unitData.attack_Lvl2;
                defense = unitData.defense_Lvl2; magicDefense = unitData.magicDefense_Lvl2;
                baseAbilityPower = unitData.baseAbilityPower_Lvl2;
            }
            else {
                maxHP = unitData.maxHP_Lvl3; attack = unitData.attack_Lvl3;
                defense = unitData.defense_Lvl3; magicDefense = unitData.magicDefense_Lvl3;
                baseAbilityPower = unitData.baseAbilityPower_Lvl3;
            }

            attackSpeed = unitData.attackSpeed;
            attackRange = unitData.attackRange;
            critChance = unitData.critChance;
            abilityPower = unitData.abilityPower;
            hitChance = unitData.hitChance;
            dodgeChance = unitData.dodgeChance;
            vampirism = unitData.vampirism;

            maxMana = unitData.maxMana;
            manaPerAttack = unitData.manaPerAttack;
            
            if (GameManager.Instance == null || GameManager.Instance.currentState == GameState.Setup)
                currentMana = unitData.startMana;

            if (spriteRenderer != null && unitData.unitSprite != null) {
                spriteRenderer.sprite = unitData.unitSprite;
                spriteRenderer.color = unitData.spriteTint;
            }
            if (unitData.defaultAbility != null) activeAbility = unitData.defaultAbility;
        }
        else if (enemyData != null) 
        {
            unitName = enemyData.enemyName; maxHP = enemyData.maxHP; attack = enemyData.attack;
            defense = enemyData.defense; magicDefense = enemyData.magicDefense;
            attackSpeed = enemyData.attackSpeed; attackRange = enemyData.attackRange;
            hitChance = 1.0f; dodgeChance = 0.05f; abilityPower = 100;
            baseAbilityPower = enemyData.abilityPower;

            if (spriteRenderer != null && enemyData.enemySprite != null) {
                spriteRenderer.sprite = enemyData.enemySprite; spriteRenderer.color = enemyData.spriteTint;
            }
            if (enemyData.defaultAbility != null) activeAbility = enemyData.defaultAbility;
        }
    }
    public void LoadEnemyData()
    {
        if (enemyData == null) return;

        // Tożsamość przeciwnika
        unitName = enemyData.enemyName;
        faction = null; // Przeciwnicy zazwyczaj nie mają frakcji
        
        // Inicjalizacja pustej listy klas (żeby SynergyManager nie wywalał błędu)
        activeClasses = new List<SynergyData>();

        // Statystyki bojowe z SO przeciwnika
        maxHP = enemyData.maxHP;
        currentHP = maxHP;
        attack = enemyData.attack;
        vampirism = enemyData.vampirism;
        defense = enemyData.defense;
        magicDefense = enemyData.magicDefense;
        attackSpeed = enemyData.attackSpeed;
        attackRange = enemyData.attackRange;
        abilityPower = enemyData.baseAbilityPower;

        // System Many
        maxMana = enemyData.maxMana;
        currentMana = enemyData.startMana;
        manaPerAttack = enemyData.manaPerAttack;

        // Jeśli masz te pola w Unit.cs, przypisz je również:
        critChance = 0.05f; // Domyślne 5% lub dodaj do SO
    }

    public void ApplyEvolutionVisuals()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (isEnemy)
        {
            spriteRenderer.color = Color.red;
            return;
        }

        switch (evolutionLevel)
        {
            case 1: spriteRenderer.color = Color.green; break;
            case 2: spriteRenderer.color = new Color(0.2f, 0.4f, 1f); break;
            case 3: spriteRenderer.color = new Color(1f, 0.84f, 0f); break;
        }
    }



    public void TakeDamage(int amount, DamageType type, Unit attacker = null, bool canBeDodged = true)
    {
        // 0. Sprawdzenie nieśmiertelności (absolutnej) na starcie
        if (HasCondition(ConditionType.Immortal) && currentHP - amount <= 0) 
        {
            currentHP = 1; // Nie pozwól spaść poniżej 1
            return;
        }
        if (HasCondition(ConditionType.Petrified)) 
        {
            Debug.Log($"{unitName} jest w kamieniu/niewrażliwy, ignoruje dmg");
            return;
        }

        // --- 1. LOGIKA UNIKU (Teraz na prawidłowym miejscu!) ---
        // Sprawdzamy, czy w ogóle można tego uniknąć i czy to nie jest True Damage (np. Burn)
        if (canBeDodged && type != DamageType.True)
        {
            // Pobieramy celność atakującego (jeśli atakujący istnieje, w przeciwnym razie domyślnie 1.0, czyli 100%)
            float currentHitChance = (attacker != null) ? attacker.hitChance : 1.0f;
            
            // Odejmujemy nasz Dodge Chance (np. 1.0 - 0.33 = 0.67 szansy na trafienie)
            float finalChanceToHit = currentHitChance - this.dodgeChance;

            // Random.value losuje od 0.0 do 1.0. 
            if (Random.value > finalChanceToHit)
            {
                Debug.Log($"[MISS] {unitName} zrobił unik!");
                return; // Przewijamy resztę funkcji - unik neguje wszystkie obrażenia i efekty!
            }
        }

        // 2. Mnożniki od statusów (Tylko jeśli trafienie się powiodło)
        if (HasCondition(ConditionType.Paralyzed)) amount = Mathf.RoundToInt(amount * 1.33f);
        if (HasCondition(ConditionType.Prone)) amount = Mathf.RoundToInt(amount * 1.33f);

        // 3. Efekty przedmiotów obronnych (np. Aegis, Shroud)
        if (equippedItems != null)
        {
            foreach (var item in equippedItems)
            {
                if (item != null && item.specialEffect != null)
                {
                    item.specialEffect.OnOwnerTakeDamage(this, ref amount, type);
                }
            }
        }

        // 4. Obliczanie pancerza/redukcji
        float incomingDamage = (float)amount;
        float damageMultiplier = 1f;
        if (type == DamageType.Physical)
        {
            damageMultiplier = 20f / (20f + Mathf.Max(0, defense));
        }
        else if (type == DamageType.Magic)
        {
            damageMultiplier = 20f / (20f + Mathf.Max(0, magicDefense));
        }

        float finalDamageFloat = incomingDamage * damageMultiplier;

        // --- NOWOŚĆ: APLIKOWANIE DAMAGE REDUCTION ---
        // Działa na wszystko, nawet na True Damage, bo jest poniżej 'ifów' od typów obrażeń!
        if (damageReduction > 0)
        {
            // Mathf.Clamp01 zapobiega sytuacji, w której redukcja przekroczy 100% (1.0f) i zacznie leczyć
            finalDamageFloat *= (1f - Mathf.Clamp01(damageReduction));
        }

        int damageToProcess = Mathf.RoundToInt(finalDamageFloat);
        float blockedByArmor = incomingDamage - finalDamageFloat;

        // --- 5. POCHŁANIANIE PRZEZ TARCZE ---
        int initialDamageToProcess = damageToProcess;
        if (activeShields.Count > 0 && damageToProcess > 0)
        {
            // Przechodzimy od końca listy (najnowsze tarcze chłoną pierwsze)
            for (int i = activeShields.Count - 1; i >= 0; i--)
            {
                if (damageToProcess <= 0) break;

                int absorb = Mathf.Min(damageToProcess, activeShields[i].amount);
                activeShields[i].amount -= absorb;
                damageToProcess -= absorb;
            }
            // Usuwamy zużyte tarcze
            activeShields.RemoveAll(s => s.amount <= 0);
        }
        int totalAbsorbedByShields = initialDamageToProcess - damageToProcess;
        // ----------------------------------------------

        // 6. Aktualizacja statystyk dla BattleTrackera
        float actualHPImpact = Mathf.Min((float)damageToProcess, (float)currentHP);
        
        if (attacker != null)
        {
            attacker.OnDamageDealtDealtToVictim(Mathf.RoundToInt(actualHPImpact));

            if (type == DamageType.Physical) attacker.roundPhysDamageDealt += actualHPImpact;
            else if (type == DamageType.Magic) attacker.roundMagicDamageDealt += actualHPImpact;
            else attacker.roundTrueDamageDealt += actualHPImpact;
        }

        if (type == DamageType.Physical) roundPhysDamageTanked += (blockedByArmor + totalAbsorbedByShields);
        else if (type == DamageType.Magic) roundMagicDamageTanked += (blockedByArmor + totalAbsorbedByShields);

        roundShieldingAbsorbed += totalAbsorbedByShields; // Statystyka ile tarczy "zeszło"

        // 7. Aplikacja pozostałych obrażeń na HP
        if (HasCondition(ConditionType.Immortal))
        {
            // Jeśli Immortal, HP nie spadnie poniżej 1
            currentHP = Mathf.Max(1, currentHP - damageToProcess);
            if (damageToProcess > 0) Debug.Log($"[IMMORTAL ACTIVE] HP zablokowane na 1 dla {unitName}");
        }
        else
        {
            currentHP -= damageToProcess;
            if (currentHP <= 0)
            {
                currentHP = 0;
                Die();
            }
        }
    }

    void Die()
    {
        if (isEnemy) Destroy(gameObject);
        else gameObject.SetActive(false);
    }

    Unit FindNearestEnemy()
    {
        // Pobieramy wszystkie jednostki na scenie
        Unit[] allUnits = Object.FindObjectsByType<Unit>(FindObjectsSortMode.None);
        Unit nearest = null;
        float minSqrDistance = float.MaxValue;

        foreach (Unit u in allUnits)
        {
            // 1. Podstawowe filtry: nie celuj w siebie, w nulle, w nieaktywne obiekty
            if (u == null || u == this || !u.gameObject.activeInHierarchy) 
                continue;

            // 2. Filtr "Ławki": Jednostki poza polem bitwy nie są celami
            if (u.currentTile == null || u.currentTile.isBenchSlot) 
                continue;

            // 3. KLUCZOWY FILTR FRAKCJI:
            // Celujemy TYLKO jeśli isEnemy jest inne niż nasze.
            // Jeśli łucznik ma isEnemy = false, to celuje tylko w u.isEnemy = true.
            if (u.isEnemy == this.isEnemy) 
                continue;

            // 4. Filtr Statusów: (np. Petrified / Invisible)
            if (!u.CanBeTargeted) 
                continue;

            // 5. Obliczanie dystansu (sqrMagnitude jest szybsze niż Distance)
            float sqrDist = (transform.position - u.transform.position).sqrMagnitude;
            
            if (sqrDist < minSqrDistance)
            {
                minSqrDistance = sqrDist;
                nearest = u;
            }
        }

        // DEBUG: Jeśli jednostka nie znalazła wroga, wypisz to w konsoli
        if (nearest == null)
        {
            // Debug.Log($"<color=red>{unitName}</color> nie widzi żadnych wrogów! Sprawdź isEnemy w Inspektorze.");
        }

        return nearest;
    }

    void HandleCombatLogic()
    {
        // 1. Twarde CC - jednostka w ogóle nie wykonuje logiki (ruch, ataki, mana)
        if (HasCondition(ConditionType.Stunned) || 
            HasCondition(ConditionType.Paralyzed) || 
            HasCondition(ConditionType.Petrified)) 
        {
            return; 
        }

        // Odliczanie czasu do ataku
        if (attackTimer > 0) attackTimer -= Time.deltaTime;

        // 2. Walidacja celu - SPRAWDZAMY CanBeTargeted
        // Jeśli cel zginął, jest nieaktywny, jest na ławce LUB stał się nienamierzalny (np. Petrified)
        if (targetEnemy == null || 
            !targetEnemy.gameObject.activeInHierarchy || 
            targetEnemy.currentTile == null || 
            targetEnemy.currentTile.isBenchSlot ||
            !targetEnemy.CanBeTargeted) // <-- TO ROZWIĄZUJE TWÓJ PROBLEM
        {
            targetEnemy = FindNearestEnemy();
            
            // Jeśli po szukaniu nadal nie ma wrogów, przerywamy (koniec walki lub wszyscy w kamieniu)
            if (targetEnemy == null) return; 
        }

        float distance = Vector2.Distance(transform.position, targetEnemy.transform.position);

        if (distance > attackRange)
        {
            // RUCH: Jednostka idzie w stronę celu
            float step = 2f * Time.deltaTime;
            transform.position = Vector2.MoveTowards(transform.position, targetEnemy.transform.position, step);
        }
        else
        {
            // 3. ATAK FIZYCZNY (Sprawdzamy Disarmed - rozbrojenie)
            if (attackTimer <= 0)
            {
                if (!HasCondition(ConditionType.Disarmed))
                {
                    AttackTarget();
                }
                else
                {
                    // Jeśli jest rozbrojony, timer i tak leci, żeby nie strzelił serii pocisków po zejściu statusu
                    attackTimer = 1f / attackSpeed; 
                }
            }

            // 4. UMIEJĘTNOŚĆ (Sprawdzamy Silenced - uciszenie)
            if (currentMana >= maxMana && activeAbility != null)
            {
                if (!HasCondition(ConditionType.Silenced))
                {
                    UseAbility();
                }
                // Jeśli ma Silence, nie używa skilla, ale mana zostaje (użyje jak tylko status minie)
            }
        }
    }

    void AttackTarget()
    {
        if (targetEnemy == null) return;

        // 1. Określenie typu obrażeń (fizyczne / magiczne dla Warlocka)
        
        int finalAttackDamage = attack;
        bool isCrit = ((Random.value < this.critChance) || this.guaranteedNextCrit) && !targetEnemy.immuneToCrits;
        if (isCrit)
        {
            this.guaranteedNextCrit = false;
            finalAttackDamage = Mathf.RoundToInt(attack * critDamage);
            Debug.Log($"[CRIT!] {unitName} uderza krytycznie za {finalAttackDamage}!");
        }
        else if (this.guaranteedNextCrit)
        {
            // Opcjonalnie: Jeśli atakujący miał gwarantowanego kryta z rękawic, ale uderzył w Adamantium, 
            // kryt się "marnuje" (rozbija o zbroję).
            this.guaranteedNextCrit = false; 
            Debug.Log($"Gwarantowany krytyk {unitName} rozbija się o adamantium jednostki {targetEnemy.unitName}!");
        }

        DamageType type = DamageType.Physical;

        if (activeClasses != null)
        {
            foreach (var cls in activeClasses)
            {
                if (cls != null && cls.synergyName == "Warlock")
                {
                    type = DamageType.Magic;
                    break;
                }
            }
        }

        // 2. LOGIKA DYSTANSOWA VS WRĘCZ
        // Zakładamy, że jednostki z attackRange > 1.5 są dystansowe
        if (attackRange > 1.5f && defaultProjectilePrefab != null)
        {
            // WYSTRZAŁ: Przekazujemy wszystkie dane do pocisku, który "dostarczy" je przy uderzeniu
            SpawnProjectile(targetEnemy, type, finalAttackDamage);
        }
        else
        {
            // ATAK WRĘCZ: Zadajemy obrażenia i nakładamy efekty natychmiast
            targetEnemy.TakeDamage(finalAttackDamage, type, this);
            ApplyOnHitEffects(targetEnemy);
        }

        // 3. Przyrost many i reset timera (dzieje się w momencie wyprowadzenia ataku, nie uderzenia)
        currentMana += manaPerAttack;
        attackTimer = 1f / attackSpeed;
    }

    // Nowa pomocnicza metoda do efektów On-Hit
    public void ApplyOnHitEffects(Unit target)
    {
        if (equippedItems != null && target != null)
        {
            foreach (ItemData item in equippedItems)
            {
                if (item != null && item.specialEffect != null)
                {
                    item.specialEffect.OnHit(this, target);
                }
            }
        }
    }

    // Nowa metoda do spawnowania pocisku
    void SpawnProjectile(Unit target, DamageType type, int damage)
    {
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        GameObject projObj = Instantiate(defaultProjectilePrefab, spawnPos, Quaternion.identity);
        Projectile projScript = projObj.GetComponent<Projectile>();

        if (projScript != null)
        {
            // POPRAWKA: używamy lokalnego 'target'
            projScript.Setup(target, this, damage, type, projectileSpeed);
        }
    }

    public void Heal(int amount)
    {
        int actualHeal = Mathf.Min(amount, maxHP - currentHP);
        currentHP += actualHeal;
        
        // Zapisujemy ile jednostka uleczyła (siebie lub innych)
        roundHealingDone += actualHeal;
        
    }

    void UseAbility()
    {
        if (HasCondition(ConditionType.Silenced)) return;
        
        // --- POWIADOMIENIE PRZEDMIOTÓW O CASTOWANIU ---
        if (equippedItems != null)
        {
            foreach (var item in equippedItems)
            {
                if (item != null && item.specialEffect != null)
                {
                    item.specialEffect.OnAbilityCast(this);
                }
            }
        }

        if (activeAbility != null && targetEnemy != null)
        {
            activeAbility.Execute(this, targetEnemy); // Wywołanie logiki z ScriptableObject
            currentMana = 0; // Reset paska
        }
    }

    // Ta funkcja liczy całą matematykę umiejętności zadających obrażenia
    public int CalculateFinalAbilityDamage(float apScaling, Unit target)
    {
        // 1. Święta Baza i Mnożnik AP
        float apBonus = Mathf.Max(0, this.abilityPower) / 100f;
        float finalMultiplier = 1f + (apBonus * apScaling);
        int finalDamage = Mathf.RoundToInt(this.baseAbilityPower * finalMultiplier);

        // 2. LOGIKA KRYTYKÓW Z UMIEJĘTNOŚCI
        if (this.canAbilitiesCrit)
        {
            // Krytyk wchodzi jeśli (wylosowano LUB jest gwarantowany) ORAZ (cel nie jest odporny)
            bool isCrit = ((Random.value < this.critChance) || this.guaranteedNextCrit) && (target != null && !target.immuneToCrits);

            if (isCrit)
            {
                this.guaranteedNextCrit = false; // Zużywamy ładunek
                finalDamage = Mathf.RoundToInt(finalDamage * this.critDamage);
                Debug.Log($"[ABILITY CRIT!] Zaklęcie {unitName} trafia krytycznie!");
            }
            else if (this.guaranteedNextCrit && target != null && target.immuneToCrits)
            {
                // Cel ma Adamantium, ładunek się marnuje
                this.guaranteedNextCrit = false;
                Debug.Log($"Gwarantowany krytyk z zaklęcia rozbija się o Adamantium jednostki {target.unitName}!");
            }
        }

        return finalDamage;
    }

    public bool ConsumeSpellShield()
    {
        if (hasSpellShield)
        {
            hasSpellShield = false;
            
            // Tutaj w przyszłości możesz dodać dźwięk rozbijanego szkła 
            // lub particle effect (np. Instantiate(shieldBreakPrefab, transform.position...))
            
            Debug.Log($"[Spellguard Shield] {unitName} wchłania wrogie zaklęcie bez szwanku!");
            return true; // Zwraca TRUE = Tarcza zadziałała, zablokuj zaklęcie!
        }
        return false; // Zwraca FALSE = Brak tarczy, zaklęcie wchodzi normalnie.
    }


    public void SaveBattleState()
    {
        battleStartPosition = transform.position;
        battleStartTile = currentTile;
    }

    public void ResetAfterBattle()
    {
        gameObject.SetActive(true);
        transform.position = battleStartPosition;
        currentTile = battleStartTile;
        if (currentTile != null) currentTile.currentUnit = this;
        
        currentHP = maxHP;
        currentMana = 0; // Reset many po bitwie
        targetEnemy = null;
        attackTimer = 0f;
        UnitStatusManager statusManager = GetComponent<UnitStatusManager>();
        if (statusManager != null)
        {
            statusManager.ClearAllEffects();
        }
        this.ResetRoundStats();
        this.ClearTemporaryItems();
        this.ResetStackableStats();
        this.RecalculateStats();
        

        if (equippedItems != null)
        {
            foreach (var item in equippedItems)
            {
                if (item != null && item.specialEffect != null)
                {
                    // To wywoła się tylko przed walką, resetując np. cooldown tarczy
                    item.specialEffect.Apply(this); 
                }
            }
        }
        
    }

    public bool EquipItem(ItemData newItem)
    {
        if (newItem == null) return false;
       

        // --- KROK 1: WALIDACJA KLAS (Zanim cokolwiek zrobimy) ---
        if (newItem.specialEffect is AddClassEffect classEffect)
        {
            // Sprawdzamy, czy jednostka już ma tę klasę
            if (!classEffect.CanApply(this))
            {
                Debug.LogWarning($"{unitName} już ma klasę {classEffect.classToAdd.synergyName}! Zwracam na ławkę.");
                return false; 
            }
            if (SynergyManager.Instance != null)
            {
                SynergyManager.Instance.RecalculateSynergies();
            }
        }

        // --- KROK 2: JEDNORAZÓWKI (CONSUMABLES) ---
        if (newItem.isConsumable)
        {
            // Odpalamy efekt tylko dla przedmiotu zużywalnego i kończymy
            if (newItem.specialEffect != null)
            {
                newItem.specialEffect.Apply(this);
            }
            Debug.Log($"[Unit] Zużyto przedmiot jednorazowy: {newItem.itemName}");
            return true; 
        }

        // --- KROK 3: LOGIKA CRAFTINGU (ŁĄCZENIE) ---
        for (int i = 0; i < equippedItems.Count; i++)
        {
            if (equippedItems[i] != null && !equippedItems[i].isFinishedItem && !newItem.isFinishedItem)
            {
                ItemData result = ItemCraftingManager.Instance.CheckRecipe(equippedItems[i], newItem);
                
                if (result != null)
                {
                    Debug.Log($"[Unit] Łączenie na jednostce: {result.itemName}");
                    equippedItems[i] = result; // Zamiana składnika na produkt
                    
                    // NOWOŚĆ: Odpalamy Apply dla NOWEGO, połączonego przedmiotu!
                    if (result.specialEffect != null)
                    {
                        result.specialEffect.Apply(this);
                    }
                    
                    RecalculateStats();
                    UpdateItemUI();
                    return true; 
                }
            }
        }

        // --- KROK 4: DODAWANIE DO WOLNEGO SLOTU ---
        if (equippedItems.Count < maxItems)
        {
            equippedItems.Add(newItem);
            
            // NOWOŚĆ: Odpalamy Apply dopiero gdy przedmiot bezpiecznie usiadł w slocie!
            if (newItem.specialEffect != null)
            {
                newItem.specialEffect.Apply(this);
                
            }
            if (SynergyManager.Instance != null)
            {
                SynergyManager.Instance.RecalculateSynergies();
            }
            RecalculateStats();
            UpdateItemUI();
            Debug.Log($"[Unit] {unitName} wyposażył {newItem.itemName} (ID: {newItem.GetInstanceID()})");
            return true;
        }

        // --- KROK 5: BRAK MIEJSCA ---
        Debug.LogWarning($"[Unit] Brak miejsca na {newItem.itemName}!");
        return false;
    }

    public bool CanEquipItem(ItemData newItem)
    {
        // 1. Jeśli chcemy założyć Rękawice, ale mamy już JAKIKOLWIEK przedmiot - blokujemy!
        if (newItem.itemName == "Gloves of Thievery" && equippedItems.Count > 0)
        {
            Debug.Log("Aby założyć Gloves of Thievery, postać nie może mieć innych przedmiotów!");
            return false;
        }

        // 2. Jeśli postać MA już na sobie Rękawice, nie możemy dołożyć jej NICZEGO - blokujemy!
        // (Bo rękawice de facto zajmują całe miejsce)
        foreach (var item in equippedItems)
        {
            if (item.itemName == "Gloves of Thievery")
            {
                Debug.Log("Ta postać ma Gloves of Thievery, nie można dodać więcej przedmiotów!");
                return false;
            }
        }

        // 3. Klasyczny limit 3 slotów
        if (equippedItems.Count >= 3) return false;

        return true; // Jeśli przeszło wszystkie testy - można założyć!
    }


    public void RecalculateStats()
    {
        // 1. CZYSTA BAZA (Wgrywa tylko bazowe staty z UnitData/EnemyData)
        if (unitData != null) LoadDataFromSO();
        else if (enemyData != null) LoadEnemyData(); // upewnij się, że masz tę metodę lub używa ona LoadDataFromSO

        // 2. ZBIERANIE PRZEDMIOTÓW (Podlicza wszystkie itemHealthBonus, itemAttackDamagePercent itp.)
        CalculateItemStats();
        dodgeChance = 0.05f;
        damageReduction = 0f;
        immuneToCrits = false;
        immuneToForcedMovement = false;
        canAbilitiesCrit = false;
        critDamage = 1.4f;



        // 3. ODPALENIE PASYWÓW (Dla AddClassEffect, Vorpal Sword itp.)
        if (equippedItems != null)
        {
            // Tworzymy kopię listy (.ToList()), żeby Gloves mogły bezpiecznie dodawać nowe itemy do oryginału
            var itemsToProcess = equippedItems.ToList(); 

            foreach (var item in itemsToProcess)
            {
                if (item != null && item.specialEffect != null)
                {
                    item.specialEffect.ApplyPassives(this);
                }
            }
        }

        // 4. FINALNA MATEMATYKA: (Baza + Bonusy Płaskie) * (1 + Bonusy Procentowe)
        maxHP = Mathf.RoundToInt((maxHP + itemHealthBonus) * (1f + (itemHealthPercentBonus / 100f)));
        
        // Zabezpieczenie HP - leczenie do maxa w Setupie
        if (GameManager.Instance == null || GameManager.Instance.currentState == GameState.Setup)
            currentHP = maxHP;

        attack = Mathf.RoundToInt((attack + itemAttackBonus) * (1f + (itemAttackDamagePercent / 100f) + stackADPercent));
        
        defense = Mathf.RoundToInt((defense + itemArmorBonus + bonusDefenseFromStacks) * (1f + (itemArmorPercentBonus / 100f)));
        magicDefense = Mathf.RoundToInt((magicDefense + itemMagicResistBonus + bonusDefenseFromStacks) * (1f + (itemMagicResistPercentBonus / 100f)));



        abilityPower += itemAbilityPowerBonus;
        float archmagiMultiplier = 1f + (archmagiStacks * 0.1f);
        abilityPower = Mathf.RoundToInt(abilityPower * archmagiMultiplier);

        vampirism += itemVampirismBonus;


        // 2. Sumowanie wszystkich "stackowalnych" bonusów procentowych do AS
        // Flicker: 5% za stos
        float stackASPercent = (flickerStacks * 5f) / 100f + quiverAuraBonus; 
        // Tu możesz dopisać inne przedmioty: stackASPercent += (inneStosy * Xf) / 100f;

        // --- 3. TWOJA FORMUŁA ATTACK SPEED ---
        // baseAttackSpeed * (1 + bonus_przedmiotów + bonus_stosów)
        attackSpeed = attackSpeed * (1f + (itemAttackSpeedPercent / 100f) + stackASPercent);
        attackSpeed = Mathf.Min(attackSpeed, 2.5f);

        
        critChance += (itemCritChancePercent / 100f);

        currentMana += itemStartingManaBonus;
        currentMana = Mathf.Min(currentMana, maxMana);
        
        // Ustawienie passywnego regen z przedmiotów (i ewentualnie z bazy, jeśli dodasz)
        currentManaRegen = itemManaRegenBonus; 

        // Ostatnie zabezpieczenie HP
        currentHP = Mathf.Min(currentHP, maxHP);

        
        //  NOWA SEKCJA: MODYFIKATORY STATUSÓW (Kolejność ma znaczenie!)
    
        // Zmniejszenie AS (Chilled)
        if (HasCondition(ConditionType.Chilled)) 
            attackSpeed *= (1f - 0.30f); // np. sztywne 30% lub Data.modifierValue

        // Zmniejszenie Ataku (Poisoned)
        if (HasCondition(ConditionType.Poisoned)) 
            attack = Mathf.RoundToInt(attack * 0.75f);

        // Zmniejszenie Celności (Blinded)
        if (HasCondition(ConditionType.Blinded)) 
            hitChance -= 0.5f;

        // Zmniejszenie Pancerza (Corroded) - TYLKO Defense
        if (HasCondition(ConditionType.Corroded)) 
            defense = Mathf.RoundToInt(defense * 0.67f);

        // Zmniejszenie Odporności (Hexed) - TYLKO Magic Defense
        if (HasCondition(ConditionType.Hexed)) 
            magicDefense = Mathf.RoundToInt(magicDefense * 0.67f);


        // 5. AKTUALIZACJA UI
        if (uiInstance != null)
        {
            UpdateItemUI();
        }
    }
    // ZMIANA: Upewnij się, że ta metoda zawsze poprawnie włącza/wyłącza Image
    public void UpdateItemUI()
    {
        if (itemSlotIcons == null || itemSlotIcons.Length == 0) return;

        for (int i = 0; i < itemSlotIcons.Length; i++)
        {
            if (itemSlotIcons[i] == null) continue; // Zabezpieczenie przed brakiem przypisania

            if (i < equippedItems.Count)
            {
                // Jest przedmiot - pokaż go
                itemSlotIcons[i].sprite = equippedItems[i].itemIcon;
                itemSlotIcons[i].enabled = true;
                
                // Reset alfy na wypadek gdyby UI go ukryło
                Color c = itemSlotIcons[i].color;
                c.a = 1f;
                itemSlotIcons[i].color = c;
            }
            else
            {
                // Brak przedmiotu - ukryj całkowicie
                itemSlotIcons[i].sprite = null;
                itemSlotIcons[i].enabled = false;
            }
        }
    }

    // ZMIANA: Ta metoda musi czyścić wszystko i odświeżać statystyki!
    public void ReturnItemsToBench()
    {
        if (equippedItems.Count == 0) return;

        // 1. Zwracamy przedmioty na ławkę
        foreach (ItemData item in equippedItems)
        {
            bool success = BenchManager.Instance.AddItem(item);
            if (!success)
            {
                Debug.LogWarning($"Ławka pełna! Nie można zwrócić {item.itemName}");
                // Tutaj opcjonalnie: GoldManager.Instance.AddGold(item.sellPrice);
            }
        }

        // 2. Czyścimy dane
        equippedItems.Clear();

        // 3. KLUCZOWE: Resetujemy statystyki do bazowych (bez itemów)
        RecalculateStats();

        // 4. KLUCZOWE: Odświeżamy UI, żeby ikony zniknęły
        UpdateItemUI();

        Debug.Log($"[Unit] Przedmioty zdjęte z {unitName}");
    }


    public void ResetRoundStats()
    {
        roundPhysDamageDealt = 0;
        roundMagicDamageDealt = 0;
        roundTrueDamageDealt = 0;
        roundPhysDamageTanked = 0;
        roundMagicDamageTanked = 0;
        roundHealingDone = 0;
        roundShieldingGenerated = 0; // Resetuj oba
        roundShieldingAbsorbed = 0;
        ResetStackableStats();




        RecalculateStats();
    }

     public void CalculateItemStats()
    {
        // 1. Zerowanie
        itemHealthBonus = 0; itemAttackBonus = 0; itemVampirismBonus = 0f;
        itemArmorBonus = 0; itemMagicResistBonus = 0; itemAbilityPowerBonus = 0;
        itemStartingManaBonus = 0; itemManaRegenBonus = 0;

        itemAttackSpeedPercent = 0f; itemCritChancePercent = 0f; itemAttackDamagePercent = 0f;
        itemHealthPercentBonus = 0f; itemArmorPercentBonus = 0f; itemMagicResistPercentBonus = 0f;

        if (equippedItems == null) return;

        // 2. Sumowanie "głupich" cyferek
        foreach (var item in equippedItems)
        {
            if (item == null) continue;

            // Płaskie (Flat)
            itemHealthBonus += item.healthBonus;
            itemAttackBonus += item.attackBonus;
            itemVampirismBonus += item.vampirism;
            itemArmorBonus += item.armorBonus;
            itemMagicResistBonus += item.magicResistBonus;
            itemAbilityPowerBonus += item.abilityPowerBonus;
            itemStartingManaBonus += item.startingMana;
            itemManaRegenBonus += item.manaRegen;

            // Procentowe (%)
            itemAttackSpeedPercent += item.attackSpeedPercent;
            itemCritChancePercent += item.critChancePercent;
            itemAttackDamagePercent += item.attackDamagePercent;
            itemHealthPercentBonus += item.healthPercentBonus;
            itemArmorPercentBonus += item.armorPercentBonus;
            itemMagicResistPercentBonus += item.magicResistPercentBonus;
        }
        
        // I TO WSZYSTKO! Żadnych efektów, żadnych klas, żadnych resetów.
    }
    // Wewnątrz klasy Unit.cs
    public void TriggerOnHitEffects(Unit target)
    {
        if (target == null || equippedItems == null) return;

        foreach (var item in equippedItems)
        {
            if (item != null && item.specialEffect != null)
            {
                item.specialEffect.OnHit(this, target);
            }
        }
    }

    public bool HasCondition(ConditionType type) => statusManager != null && statusManager.HasCondition(type);

    public void AddCondition(StatusEffectSO effect, Unit source = null) => statusManager?.ApplyEffect(effect, source);

    /// SHIELDS
    private void CleanupExpiredShields()
    {
        int initialCount = activeShields.Count;
        activeShields.RemoveAll(s => s.IsExpired);
         
    }
    public void AddShield(int amount, float duration)
    {
        activeShields.Add(new ShieldInstance(amount, duration));
        roundShieldingGenerated += amount; // Statystyka do trackera
       
    }
    public void RefreshArchmagiShield(int amount, float duration)
    {
        
        // Jeśli tarcza już istnieje, nie wygasła i wciąż jest na liście:
        if (currentArchmagiShield != null && !currentArchmagiShield.IsExpired /* && shields.Contains(currentArchmagiShield) */) 
        {
            // ODŚWIEŻAMY (nadpisujemy wartość i resetujemy czas)
            currentArchmagiShield.amount = amount;
            currentArchmagiShield.expirationTime = Time.time + duration;
        }
        else
        {
            // Jeśli nie było tarczy (lub stara pękła/wygasła), tworzymy nową
            currentArchmagiShield = new ShieldInstance(amount, duration);
            
            // DODAJ DO SWOJEJ LISTY TARCZ:
            activeShields.Add(currentArchmagiShield);
        }
    }

    public void RestoreHP(int amount)
    {
        if (HasCondition(ConditionType.Wounded)) amount = Mathf.RoundToInt(amount * 0.67f);
        currentHP = Mathf.Min(currentHP + amount, maxHP);
    }
    public void SetVisualColor(Color newColor)
    {
        // Szukamy SpriteRenderer w tym obiekcie lub dzieciach (np. w grafice postaci)
        var renderer = GetComponentInChildren<SpriteRenderer>();
        if (renderer != null) 
        {
            renderer.color = newColor;
        }
    }

    public void OnDamageDealtDealtToVictim(int finalDamage)
    {
        if (vampirism <= 0 || finalDamage <= 0) return;

        // Dzielimy przez 100f, aby 20 stało się 0.2
        float vPercent = vampirism / 100f; 
        int healAmount = Mathf.RoundToInt(finalDamage * vPercent);

        if (healAmount > 0)
        {
            int hpBefore = currentHP;
            RestoreHP(healAmount);
            
            int actualHealed = currentHP - hpBefore;
            if (actualHealed > 0)
            {
                roundHealingDone += actualHealed; // Dodanie do trackera
                Debug.Log($"[Lifesteal] Zadano {finalDamage}, leczenie {healAmount} ({vampirism}%)");
            }
        }
    }
    public void ResetStackableStats(){
        stackADPercent = 0f; // np. 0.02f za każdy stack
        stackASPercent = 0f; // np. 0.02f za każdy stack
        bonusDefenseFromStacks = 0; // Dodaj to pole!
        bonusMagicDefenseFromStacks = 0; // Dodaj to pole!
        dwarvenStacks = 0;
        staffOfLightningHits = 0;
        flickerStacks = 0;
        quiverAuraBonus = 0f;
        flamingFuryHitCounter = 0;
        guaranteedNextCrit = false;
        hasSpellShield = false;
        critDamage = 1.4f;
        currentItemTick = 0;
        itemTickTimer = 0f;
        archmagiStacks = 0;
        currentArchmagiShield = null;
        


    }

    private void TriggerItemTicks()
    {
        if (equippedItems == null) return;

        foreach (var item in equippedItems)
        {
            if (item != null && item.specialEffect != null)
            {
                item.specialEffect.OnTick(this);
            }
        }
    }
    
        // Dodaje item na czas walki
    public void AddTemporaryItem(ItemData item)
    {
        if (item == null) return;
        equippedItems.Add(item);
        tempItemCount++;
    }

    // Usuwa itemy po walce
    public void ClearTemporaryItems()
    {
        if (tempItemCount > 0)
        {
            // Usuwamy tyle przedmiotów z końca listy, ile zostało dodanych tymczasowo
            int startIndex = equippedItems.Count - tempItemCount;
            if (startIndex >= 0)
            {
                equippedItems.RemoveRange(startIndex, tempItemCount);
            }
            tempItemCount = 0;
            
            // Ważne: Po wyczyszczeniu NIE wywołuj tu RecalculateStats, 
            // zrobi to GameManager w odpowiednim momencie rundy.
            Debug.Log($"[Unit] {unitName} wyczyścił skradzione przedmioty.");
        }
    }

}