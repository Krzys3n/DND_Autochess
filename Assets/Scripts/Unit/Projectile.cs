using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Unit target;
    private Unit caster;
    private int damage;
    private DamageType damageType;
    private float speed = 10f;
    private bool isInitialized = false;
    private Vector3 lastKnownTargetPos;

    public void Setup(Unit target, Unit caster, int damage, DamageType type, float speed)
    {
        this.target = target;
        this.caster = caster;
        this.damage = damage;
        this.damageType = type;
        this.speed = speed;
        if (target != null) lastKnownTargetPos = target.transform.position; // <--- DODAJ TO
        isInitialized = true;
        
        // DEBUG: Sprawdźmy czy Unit faktycznie wysłał prędkość
        Debug.Log($"Pocisk zainicjowany! Cel: {target.unitName}, Prędkość: {speed}");
        
        if (this.speed <= 0) {
            Debug.LogWarning("Prędkość pocisku to 0! Ustaw projectileSpeed w Unit!");
            this.speed = 10f; // Bezpiecznik
        }
    }

    void Update()
    {
        // Czekamy, aż Setup zostanie wywołany
        if (!isInitialized) return;

        // 1. Aktualizujemy ostatnią znaną pozycję, dopóki cel żyje
        if (target != null && target.gameObject.activeInHierarchy)
        {
            lastKnownTargetPos = target.transform.position;
        }

        // 2. Ruch wykonujemy zawsze w stronę lastKnownTargetPos (niezależnie czy cel żyje)
        Vector2 oldPos = transform.position;
        transform.position = Vector2.MoveTowards(transform.position, lastKnownTargetPos, speed * Time.deltaTime);

        // Rotacja (opcjonalnie, żeby strzała zawsze patrzyła tam gdzie leci)
        Vector2 direction = (Vector2)lastKnownTargetPos - (Vector2)transform.position;
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        // 3. Sprawdzanie dystansu do punktu docelowego
        float dist = Vector2.Distance(transform.position, lastKnownTargetPos);

        if (dist < 0.2f) 
        {
            // Jeśli cel żyje w momencie uderzenia -> zadaj obrażenia
            if (target != null && target.gameObject.activeInHierarchy)
            {
                Debug.Log("TRAFIONO CEL!");
                OnHit();
            }
            else
            {
                // Jeśli cel zginął wcześniej -> po prostu usuń pocisk po dolocie
                Debug.Log("Pocisk dotarł do celu, który już nie żyje. Znikam.");
                Destroy(gameObject);
            }
        }

        // Debugowanie ruchu
        if ((Vector2)transform.position == oldPos && speed > 0 && dist > 0.1f) {
            Debug.LogError("POZYCJA POCISKU STOI W MIEJSCU!");
        }
    }

    void OnHit()
    {
        // 1. Zadaj obrażenia celowi
        target.TakeDamage(damage, damageType, caster);

        // 2. NOWOŚĆ: Wywołaj efekty OnHit z przedmiotów strzelca
        if (caster != null && caster.equippedItems != null)
        {
            caster.TriggerOnHitEffects(target);
        }
        
        // 3. Usuń pocisk
        Destroy(gameObject);
    }
    
}