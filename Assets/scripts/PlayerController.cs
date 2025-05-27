using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(Animator))]
public class PlayerController : MonoBehaviour 
{
    // Компоненты
    private Animator anim;
    private CharacterController controller;
    public AudioSource moveSound;
    
    // Настройки движения
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float rotationSpeed = 10f;
    public float gravity = 180f;
    public float stepRate = 0.5f; // Интервал между шагами

    // Параметры анимаций
    [Header("Animation Parameters")]
    public string walkParam = "IsWalking";
    public string runParam = "IsRunning";
    public string repairParam = "IsRepairing";
    public string pickupParam = "IsPickingUp";

    // Состояние персонажа
    private Vector3 moveDirection = Vector3.zero;
    private float currentSpeed;
    private bool isRunning;
    private bool controlsEnabled = true;
    private bool isInteracting;
    private float nextStepTime;
    private bool wasMoving;

    void Start() 
    {
        // Инициализация компонентов
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();

        Debug.Assert(controller != null, "CharacterController not found!");
        Debug.Assert(anim != null, "Animator not found!");
        Debug.Assert(moveSound != null, "MoveSound AudioSource not assigned!");

        currentSpeed = walkSpeed;
        EnableControls();
        nextStepTime = 0f;
        wasMoving = false;
    }

    void Update()
    {
        if (!controlsEnabled) return;

        if (!isInteracting)
        {
            HandleMovement();
            HandleFootsteps();
        }
        else
        {
            // Блокировка движения во время взаимодействия
            moveDirection = Vector3.zero;
            wasMoving = false;
        }

        // Всегда применяем гравитацию
        ApplyGravity();
    }

    private void HandleMovement()
    {
        // Получаем ввод с клавиатуры
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Проверяем бег (Shift + движение)
        isRunning = Input.GetKey(KeyCode.LeftShift) && vertical > 0;
        currentSpeed = isRunning ? runSpeed : walkSpeed;

        // Поворот персонажа
        if (horizontal != 0)
        {
            transform.Rotate(0, horizontal * rotationSpeed * Time.deltaTime, 0);
        }

        // Движение вперед/назад
        if (vertical != 0)
        {
            moveDirection = transform.forward * vertical * currentSpeed;
        }
        else
        {
            moveDirection = Vector3.zero;
        }

        // Управление анимациями движения
        bool isMoving = vertical != 0;
        anim.SetBool(walkParam, isMoving);
        anim.SetBool(runParam, isRunning && isMoving);
    }

    private void HandleFootsteps()
    {
        bool isMoving = moveDirection != Vector3.zero;

        if (isMoving)
        {
            if (!wasMoving)
            {
                // Начинаем движение - проигрываем первый шаг
                PlayStepSound();
                nextStepTime = Time.time + stepRate;
                wasMoving = true;
            }
            else if (Time.time >= nextStepTime)
            {
                // Проигрываем следующий шаг
                PlayStepSound();
                nextStepTime = Time.time + stepRate;
            }
        }
        else
        {
            wasMoving = false;
        }
    }

    private void PlayStepSound()
    {
        if (moveSound != null && !moveSound.isPlaying)
        {
            // Настраиваем звук в зависимости от скорости
            moveSound.pitch = isRunning ? 1.2f : 1f;
            moveSound.volume = isRunning ? 0.8f : 0.6f;
            moveSound.Play();
        }
    }

    private void ApplyGravity()
    {
        controller.Move(moveDirection * Time.deltaTime);
    }

    public void StartInteraction(string interactionType, float duration)
    {
        isInteracting = true;
        
        switch(interactionType)
        {
            case "repair":
                anim.SetBool(repairParam, true);
                break;
            case "pickup":
                anim.SetBool(pickupParam, true);
                break;
        }

        Invoke(nameof(EndInteraction), duration);
    }

    private void EndInteraction()
    {
        isInteracting = false;
        anim.SetBool(repairParam, false);
        anim.SetBool(pickupParam, false);
    }

    public void EnableControls()
    {
        controlsEnabled = true;
        // Сбрасываем все анимации движения
        anim.SetBool(walkParam, false);
        anim.SetBool(runParam, false);
    }

    public void DisableControls()
    {
        controlsEnabled = false;
        moveDirection = Vector3.zero;
        // Сбрасываем все анимации движения
        anim.SetBool(walkParam, false);
        anim.SetBool(runParam, false);
    }
}