using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance; // Сінглтон
    public TextMeshProUGUI counterObj; // Посилання на текстовий об'єкт
    private int playerScore = 0; // Рахунок
    private void Start()
    {
        if (Instance == null) // Якщо статів у грі ще немає
        {
            // Створюємо об'єкт зі статами
            Instance = new PlayerStats();
        }
        counterObj.text = $"Рахунок: {playerScore}";
    }
    public void AddScore(int score)
    {
        playerScore += score;
        counterObj.text = $"Рахунок: {playerScore}";
    }
}
