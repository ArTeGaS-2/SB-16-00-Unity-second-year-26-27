using UnityEngine;
public class Loot : MonoBehaviour
{
    private PlayerStats playerScore; // Клас об'єкту статистики
    public int lootReward = 100; // Нагорода за підбір

    private void Start()
    {
        // Прив'язуємо об'єкт зі статистикою до змінної playerScore
        playerScore = PlayerStats.Instance;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Якщо зіштовхнулись з гравцем
        if (collision.gameObject.CompareTag("Player"))
        {
            // Нараховуємо нагороду
            playerScore.AddScore(lootReward);
            // Знищуємо лут
            Destroy(gameObject);
        }
    }
}
