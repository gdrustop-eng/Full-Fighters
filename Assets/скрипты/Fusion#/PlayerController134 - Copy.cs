using UnityEngine;
using Fusion;

public class PlayerController : NetworkBehaviour
{
    public float speed = 5f;

    // Использование Update гарантирует, что нажатия клавиш считываются мгновенно
    void Update()
    {
        // Проверяем, что этот объект принадлежит ТЕКУЩЕМУ игроку
        if (Object != null && Object.HasInputAuthority)
        {
            float moveX = 0f;

            // Считываем нажатия клавиш A/D или Стрелок
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                moveX = -1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                moveX = 1f;

            // Двигаем персонажа
            if (moveX != 0)
            {
                transform.Translate(new Vector3(moveX, 0, 0) * speed * Time.deltaTime);
            }
        }
    }
}