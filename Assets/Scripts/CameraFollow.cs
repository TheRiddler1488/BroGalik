using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // Цель (игрок)

    [Header("Camera Settings")]
    [SerializeField] private float height = 15f; // Высота камеры над целью
    [SerializeField] private float distance = 10f; // Смещение камеры назад
    [SerializeField] private float angle = 45f; // Угол наклона камеры (как в Dota 2)
    [SerializeField] private float smoothSpeed = 0.125f; // Скорость сглаживания движения

    [Header("Zoom Settings")]
    [SerializeField] private float minZoom = 5f; // Минимальное расстояние (зум)
    [SerializeField] private float maxZoom = 20f; // Максимальное расстояние
    [SerializeField] private float zoomSpeed = 5f; // Скорость зума

    [Header("Map Bounds (Optional)")]
    [SerializeField] private bool useMapBounds = false; // Использовать границы карты
    [SerializeField] private Vector2 minBounds = new Vector2(-50, -50); // Мин. координаты карты
    [SerializeField] private Vector2 maxBounds = new Vector2(50, 50); // Макс. координаты карты

    private Vector3 offset; // Смещение камеры относительно цели
    private Vector3 velocity; // Для сглаживания

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("Цель для камеры не задана!");
            enabled = false;
            return;
        }
        // Начальное смещение камеры
        UpdateOffset();
    }

    void Update()
    {
        UpdateZoom();
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Позиция, к которой стремится камера
        Vector3 desiredPosition = target.position + offset;

        // Плавное движение к желаемой позиции
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);
        transform.position = smoothedPosition;

        // Устанавливаем угол обзора (изометрический)
        transform.rotation = Quaternion.Euler(angle, 0f, 0f);

        // Ограничиваем позицию камеры, если включены границы
        if (useMapBounds)
        {
            float camHeight = height * Mathf.Cos(angle * Mathf.Deg2Rad);
            Vector3 clampedPos = transform.position;
            clampedPos.x = Mathf.Clamp(clampedPos.x, minBounds.x, maxBounds.x);
            clampedPos.z = Mathf.Clamp(clampedPos.z, minBounds.y, maxBounds.y);
            clampedPos.y = camHeight; // Сохраняем высоту
            transform.position = clampedPos;
        }
    }

    private void UpdateZoom()
    {
        // Зум колёсиком мыши
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            distance = Mathf.Clamp(distance - scroll * zoomSpeed, minZoom, maxZoom);
            UpdateOffset();
        }
    }

    private void UpdateOffset()
    {
        // Вычисляем смещение камеры
        float y = height;
        float z = -distance * Mathf.Cos(angle * Mathf.Deg2Rad);
        offset = new Vector3(0f, y, z);
    }

    // Визуализация в редакторе
    void OnDrawGizmosSelected()
    {
        if (useMapBounds)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3(
                (minBounds.x + maxBounds.x) / 2,
                0,
                (minBounds.y + maxBounds.y) / 2
            );
            Vector3 size = new Vector3(
                maxBounds.x - minBounds.x,
                1f,
                maxBounds.y - minBounds.y
            );
            Gizmos.DrawWireCube(center, size);
        }
    }

    /* Закомментированное управление мышью (движение к краям экрана)
    [Header("Edge Movement")]
    [SerializeField] private float edgeBorder = 25f; // Граница экрана для движения (в пикселях)
    [SerializeField] private float moveSpeed = 20f; // Скорость движения камеры мышью

    private bool isLockedToTarget = true;

    private void HandleInput()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 moveDirection = Vector3.zero;

        if (mousePos.x <= edgeBorder)
            moveDirection.x -= 1f; // Влево
        if (mousePos.x >= Screen.width - edgeBorder)
            moveDirection.x += 1f; // Вправо
        if (mousePos.y <= edgeBorder)
            moveDirection.z -= 1f; // Вниз
        if (mousePos.y >= Screen.height - edgeBorder)
            moveDirection.z += 1f; // Вверх

        if (moveDirection != Vector3.zero)
        {
            isLockedToTarget = false;
            transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isLockedToTarget = true;
        }
    }
    */
}