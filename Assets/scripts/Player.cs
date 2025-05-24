using UnityEngine;

public class Player : MonoBehaviour 
{
    private Animator anim;
    private CharacterController controller;

    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float rotationSpeed = 10f;
    public float gravity = 180f;

    [Header("Animation Parameters")]
    public string walkParam = "IsWalking";
    public string runParam = "IsRunning";

    private Vector3 moveDirection = Vector3.zero;
    private float currentSpeed;
    private bool isRunning;

    void Start() 
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();

        if (controller == null)
            Debug.LogError("CharacterController not found!");
        if (anim == null)
            Debug.LogError("Animator not found!");

        currentSpeed = walkSpeed;
    }

    void Update()
    {
        if (controller == null || anim == null) return;

        // Получаем ввод с клавиатуры
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Проверяем бег (Shift + движение)
        isRunning = Input.GetKey(KeyCode.LeftShift) && vertical > 0;
        currentSpeed = isRunning ? runSpeed : walkSpeed;

        // Поворот персонажа при нажатии стрелок влево/вправо
        if (horizontal != 0)
        {
            transform.Rotate(0, horizontal * rotationSpeed * Time.deltaTime, 0);
        }

        // Движение вперед только при зажатой стрелке вверх
        if (vertical > 0)
        {
            moveDirection = transform.forward * vertical * currentSpeed;
        }
        else
        {
            moveDirection = Vector3.zero;
        }

        controller.Move(moveDirection * Time.deltaTime);

        // Управление анимациями
        bool isMoving = vertical > 0;
        anim.SetBool(walkParam, isMoving && !isRunning);
        anim.SetBool(runParam, isMoving && isRunning);
    }
}