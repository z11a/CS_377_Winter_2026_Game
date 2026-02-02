using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.SceneManagement;

public class PlayerHandler : MonoBehaviour
{
    private PlayerInput playerInput;
    private Vector2 moveAmount;
    private CharacterController controller;
    private Animator animator;

    public float playerSpeed = 10.0f;
    //public float gravity = -2.0f;
    public static playerState _playerState;

    public enum playerState 
    {
        Idle,
        Running,
        Aiming
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        _playerState = playerState.Idle;
    }   

    // Update is called once per frame

    void Update()
    {
        if (GameStateManager._gameState == GameStateManager.GameState.inGame)
        {
            MovementHandler();
            AnimationHandler();
        }
    }

    private void MovementHandler()
    {
        Vector3 movement = new Vector3(moveAmount.x, 0, moveAmount.y);

        if (movement != Vector3.zero)
        {
            controller.Move(movement * playerSpeed * Time.deltaTime);
            transform.forward = movement;
            _playerState = playerState.Running;
        }
        else
        {
            _playerState = playerState.Idle;
        }
    }

    private void AnimationHandler()
    {
        switch (_playerState)
        {
            case playerState.Idle:
                animator.SetBool("Running", false);
                break;
            case playerState.Running:
                animator.SetBool("Running", true);
                break;
        }
    }

    public void OnMove(InputValue value)
    {
        moveAmount = value.Get<Vector2>();
        Debug.Log("Moving...");
    }
}
