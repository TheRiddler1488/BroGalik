using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody rb;
    private Vector3 moveInput;
    public float rotationSpeed = 10f; // Скорость поворота персонажа

    void Start()
    {   
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Движение персонажа
        float mx = Input.GetAxisRaw("Horizontal");
        float mz = Input.GetAxisRaw("Vertical");
        moveInput = new Vector3(mx, 0f, mz).normalized * moveSpeed;

        // Поворот персонажа к курсору
        RotateTowardsMouse();

        // Применяем движение
        rb.linearVelocity = new Vector3(moveInput.x, rb.linearVelocity.y, moveInput.z); // Сохраняем вертикальную скорость
    }

    void RotateTowardsMouse()
    {
        // Получаем позицию мыши в экранных координатах
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // Получаем точку пересечения с землёй (плоскость Y=0)
            Vector3 targetPoint = hit.point;
            targetPoint.y = transform.position.y; // Сохраняем высоту персонажа

            // Вычисляем направление к точке
            Vector3 direction = (targetPoint - transform.position).normalized;

            // Плавный поворот к направлению
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
}