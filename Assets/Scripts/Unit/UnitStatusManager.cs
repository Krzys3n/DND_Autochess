using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UnitStatusManager : MonoBehaviour
{
    private Unit owner;
    private List<ActiveEffect> activeEffects = new List<ActiveEffect>();

    [Header("UI References")]
    public GameObject iconTemplate; 
    public Transform statusContainer; 

    void Awake() => owner = GetComponent<Unit>();

    public bool HasCondition(ConditionType type) => activeEffects.Exists(e => e.Data.type == type);

    // ZMIANA: Dodajemy parametr 'caster', żeby wiedzieć kto nałożył efekt
    public void ApplyEffect(StatusEffectSO data, Unit caster = null)
    {
        if (data == null || iconTemplate == null) return;
        
        if (HasCondition(ConditionType.Invulnerable) && data.type != ConditionType.Invulnerable) return;
        if (HasCondition(ConditionType.Petrified) && data.type != ConditionType.Petrified) return;

        ActiveEffect existing = activeEffects.Find(e => e.Data.type == data.type);
        if (existing != null) {
            existing.ResetTimer();
            // Opcjonalnie: można zaktualizować castera, jeśli chcemy by 
            // ostatnia osoba nakładająca Burn zbierała dmg
            return;
        }

        // Przekazujemy castera do nowej instancji efektu
        ActiveEffect newEffect = new ActiveEffect(data, owner, statusContainer, iconTemplate, caster);
        activeEffects.Add(newEffect);
        newEffect.OnEffectStart();
    }

    void Update()
    {
        float dt = Time.deltaTime;
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            activeEffects[i].UpdateEffect(dt);
            if (activeEffects[i].IsExpired)
            {
                activeEffects[i].OnEffectEnd();
                activeEffects.RemoveAt(i);
            }
        }
    }

    public void ClearAllEffects()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            activeEffects[i].OnEffectEnd();
        }
        activeEffects.Clear();
    }

    [System.Serializable]
    public class ActiveEffect
    {
        public StatusEffectSO Data { get; private set; }
        private Unit target;
        private Unit caster; // ZMIANA: Zapamiętujemy sprawcę
        private float timer;
        private float tickTimer;
        private GameObject iconInstance;

        public bool IsExpired => timer <= 0;

        public ActiveEffect(StatusEffectSO data, Unit target, Transform container, GameObject template, Unit caster)
        {
            this.Data = data;
            this.target = target;
            this.caster = caster; // Przypisujemy sprawcę
            this.timer = data.duration;

            if (container != null && template != null)
            {
                // 1. Tworzymy nową ikonkę wewnątrz listy (StatusEffectList)
                iconInstance = Instantiate(template, container);
                iconInstance.SetActive(true);
                
                // 2. Szukamy komponentu Image na nowym obiekcie
                Image img = iconInstance.GetComponent<Image>();
                if (img != null) 
                {
                    // 3. Przypisujemy Sprite z Twojego StatusEffectSO (ikonkę)
                    img.sprite = data.icon;
                    img.color = data.iconTint; 
                }
            }
        }

        public void ResetTimer() => timer = Data.duration;

        public void OnEffectStart()
        {
            target.RecalculateStats();
            switch (Data.type)
            {
                case ConditionType.Petrified:
                // Jednostka staje się posągiem - nie można jej zaznaczyć
                target.isTargetable = false; 
                break;
            }
        }

        public void OnEffectEnd()
        {
            switch (Data.type)
            {
                case ConditionType.Chilled: target.attackSpeed /= (1f - Data.modifierValue); break;
                case ConditionType.Corroded:
                    target.defense = Mathf.RoundToInt(target.defense / (1f - Data.modifierValue));
                    target.magicDefense = Mathf.RoundToInt(target.magicDefense / (1f - Data.modifierValue));
                    break;
                case ConditionType.Poisoned: target.attack = Mathf.RoundToInt(target.attack / (1f - Data.modifierValue)); break;
                case ConditionType.Blinded: target.hitChance += 0.5f; break;
                case ConditionType.Petrified:
                // Po zakończeniu petryfikacji jednostka znów jest celem
                target.isTargetable = true;
                break;
            }
            if (iconInstance != null) Destroy(iconInstance);
        }

        public void UpdateEffect(float dt)
        {
            timer -= dt;
            
            if (Data.type == ConditionType.Burned)
            {
                tickTimer += dt;
                if (tickTimer >= Data.tickRate)
                {
                    ApplyBurnDamage();
                    tickTimer = 0;
                }
            }
        }

        private void ApplyBurnDamage()
        {
            int burnDmg = Mathf.Max(1, Mathf.RoundToInt(target.maxHP * Data.modifierValue));

            // ZMIANA: Jeśli mamy castera, to on zadaje obrażenia (zapisze się w jego trackerze)
            if (caster != null)
            {
                // Wywołujemy TakeDamage u celu, ale jako źródło podajemy castera
                target.TakeDamage(burnDmg, DamageType.True, caster, false);
            }
            else
            {
                // Jeśli nie ma castera (np. efekt środowiskowy), cel sam sobie zadaje dmg
                target.TakeDamage(burnDmg, DamageType.True, null, false);
            }
        }
    }

    [Header("Debug / Testing")]
    public StatusEffectSO effectToTest;

    // Ta funkcja pojawi się w menu pod prawym przyciskiem myszy na komponencie w Inspektorze
    [ContextMenu("Test Apply Effect")]
    public void TestApply()
    {
        if (effectToTest != null)
        {
            // Testowo nakładamy efekt, jako castera podajemy samych siebie
            ApplyEffect(effectToTest, owner);
            Debug.Log($"<color=orange>[Debug]</color> Ręcznie nałożono status: {effectToTest.effectName} na {owner.unitName}");
        }
        else
        {
            Debug.LogWarning("Nie przypisałeś StatusEffectSO do pola 'effectToTest'!");
        }
    }
}