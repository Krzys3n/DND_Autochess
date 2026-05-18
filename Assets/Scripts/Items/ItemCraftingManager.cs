using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemCraftingManager : MonoBehaviour
{
    public static ItemCraftingManager Instance;

    // Lista przechowująca wszystkie przedmioty, które można stworzyć (Finished Items)
    private List<ItemData> allRecipes = new List<ItemData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Opcjonalnie: DontDestroyOnLoad(gameObject); // Jeśli manager ma przetrwać między scenami
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadRecipes();
    }

    /// <summary>
    /// Ładuje wszystkie obiekty ItemData z folderu Resources/FinishedItems.
    /// Upewnij się, że Twoje gotowe przedmioty (np. Amulet) są w tym folderze!
    /// </summary>
    private void LoadRecipes()
    {
        // Ta metoda działa zarówno w Edytorze, jak i w gotowym Buildzie gry
        ItemData[] loadedItems = Resources.LoadAll<ItemData>("Items/FinishedItems");
        
        // Filtrujemy, żeby na liście były tylko przedmioty oznaczone jako Finished
        allRecipes = loadedItems.Where(item => item.isFinishedItem).ToList();

        Debug.Log($"<color=cyan>ItemCraftingManager:</color> Załadowano {allRecipes.Count} receptur z Resources/FinishedItems.");
    }

    /// <summary>
    /// Sprawdza, czy z dwóch komponentów można stworzyć przedmiot finalny.
    /// </summary>
    public ItemData CheckRecipe(ItemData a, ItemData b)
    {
        if (a == null || b == null) return null;

        // Szukamy receptury, porównując nazwy przedmiotów (itemName), a nie referencje do plików
        ItemData resultTemplate = allRecipes.FirstOrDefault(r => 
            (r.recipeComponentA != null && r.recipeComponentB != null) &&
            (
                // Opcja 1: A+B
                (r.recipeComponentA.itemName == a.itemName && r.recipeComponentB.itemName == b.itemName) || 
                // Opcja 2: B+A
                (r.recipeComponentA.itemName == b.itemName && r.recipeComponentB.itemName == a.itemName)
            )
        );

        if (resultTemplate != null)
        {
            // Tworzymy unikalną instancję wyniku (żeby Rękawice miały swoje ID!)
            ItemData uniqueInstance = ScriptableObject.Instantiate(resultTemplate);
            
            Debug.Log($"<color=magenta>[Crafting]</color> Połączono {a.itemName} + {b.itemName} = {uniqueInstance.itemName}");
            
            return uniqueInstance;
        }

        return null; 
    }

    /// <summary>
    /// Pomocnicza metoda do odświeżenia bazy w trakcie pracy w edytorze (opcjonalnie)
    /// </summary>
    public void RefreshDatabase()
    {
        LoadRecipes();
    }
}