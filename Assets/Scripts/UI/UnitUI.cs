using UnityEngine;
using UnityEngine.UI;

public class UnitUI : MonoBehaviour
{
    [Header("HP & Shield Bar")]
    public Image healthFill;
    public Image shieldFill; // NOWOŚĆ: Przeciągnij tu nowy obrazek tarczy

    
    [Header("Mana Bar")]
    public GameObject manaContainer; 
    public Image manaFill;

    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 0.8f, 0); // Przesunięcie nad głowę

    private Unit owner;

    public void Setup(Unit unit)
    {
        owner = unit;
        
        // Sprawdzamy, czy jednostka w ogóle ma manę/umiejętność
        if (owner.activeAbility == null)
        {
            manaContainer.SetActive(false);
        }
        else
        {
            manaContainer.SetActive(true);
        }

        // Jeśli nie przypiszesz shieldFill w inspektorze, skrypt nie wyrzuci błędu
        if (shieldFill != null)
        {
            shieldFill.gameObject.SetActive(false);
        }
    }

    void LateUpdate()
    {
        if (owner == null)
        {
            Destroy(gameObject);
            return;
        }

        // 1. Aktualizacja HP
        float hpPercent = (float)owner.currentHP / owner.maxHP;
        healthFill.fillAmount = hpPercent;

        // 2. Aktualizacja Tarczy (Shield)
        // W UnitUI.cs - LateUpdate
        if (shieldFill != null)
        {
            int totalShield = owner.CurrentTotalShield;

            if (totalShield > 0)
            {
                shieldFill.gameObject.SetActive(true);
                
                // 1. Obliczamy sam procent tarczy względem MaxHP
                float shieldPercent = (float)totalShield / owner.maxHP;
                
                // 2. Jeśli chcesz, by tarcza była "połową paska" (0.5), ustawiamy po prostu:
                shieldFill.fillAmount = shieldPercent; 

                // DODATKOWA WSKAZÓWKA:
                // Aby tarcza nie zasłaniała zielonego paska HP, ustaw w Unity:
                // - Obrazek shieldFill powinien mieć kolor półprzezroczysty (np. jasnoniebieski z Alfą 150)
                // - W hierarchii (Inspector) shieldFill powinien być PONIŻEJ healthFill, 
                //   ale healthFill musi mieć mniejszy fill, żeby było widać tarczę pod spodem.
            }
            else
            {
                shieldFill.gameObject.SetActive(false);
            }
        }

        // 3. Aktualizacja Many
        if (manaContainer != null && manaContainer.activeSelf)
        {
            manaFill.fillAmount = (float)owner.currentMana / owner.maxMana;
        }
    }
}