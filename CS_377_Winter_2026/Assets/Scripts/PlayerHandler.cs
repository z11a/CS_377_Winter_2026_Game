using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using UnityEditor;

public class PlayerHandler : MonoBehaviour
{
    private PlayerInput playerInput;
    private Vector2 moveAmount;
    private CharacterController controller;
    private Rigidbody rb;
    private Animator animator;

    public enum PlayerNumber
    {
        Player1, 
        Player2
    }
    public enum PlayerState
    {
        Idle,
        Running,
        Knockback,
        Aiming
    }

    public float playerHealth = 50.0f;
    public float playerSpeed = 5.0f;
    //public float gravity = -2.0f; // not sure if we need this yet.
    public float respawnTime = 3.0f;
    public float invincibilityTime = 3.0f;
    
    [HideInInspector] public PlayerState _playerState;
    [HideInInspector] public int playerCurrentRoundScore;
    [HideInInspector] public List<GameObject> playerCurrentHoldingCheeses;
    [HideInInspector] public int playerTotalRoundScore;
    [HideInInspector] public PlayerNumber playerNumber;
    [HideInInspector] public GameObject weaponEquippedObject;
    [HideInInspector] public Transform rightHandTransform;    // this is needed for when the player is holding a weapon, there might be a better way of finding this bone though.

    private Coroutine deathCoroutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;

        _playerState = PlayerState.Idle;
        rightHandTransform = transform.Find("mouse_rig/spine/spine_01/arm_r/forearm_r/forearm_r_end");

        StartCoroutine(DeathHandler());
    }   

    // Update is called once per frame
    void Update()
    {
        if (GameStateManager._gameState == GameStateManager.GameState.inGame)
        {
            //MovementHandlerCharacterController();
            AnimationHandler();
        }
    }

    private void FixedUpdate()
    {
        if (GameStateManager._gameState == GameStateManager.GameState.inGame)
        {
            MovementHandlerRigidbody();
        }
    }

    private IEnumerator DeathHandler()
    {
        yield return null;

        while (true)
        {
            if (playerHealth <= 0.0f)
            {
                Debug.Log(playerNumber + " died.");

                GetComponent<PlayerInput>().DeactivateInput();
                yield return new WaitForSeconds(respawnTime);
                GetComponent<PlayerInput>().ActivateInput();
                playerHealth = 50.0f;
                yield return new WaitForSeconds(invincibilityTime);
                yield return null;
            }
            yield return null;
        } 
    }

    private void MovementHandlerRigidbody()
    {
        Vector3 movement = new Vector3(moveAmount.x, 0, moveAmount.y);

        rb.angularVelocity = Vector3.zero;

        if (movement != Vector3.zero)
        {
            if (_playerState == PlayerState.Knockback)
            {
                rb.AddForce(movement * playerSpeed * Time.deltaTime * 3, ForceMode.Force);
            }
            else
            {
                rb.linearVelocity = movement * playerSpeed * Time.deltaTime;
                _playerState = PlayerState.Running;
            }
            rb.MoveRotation(Quaternion.LookRotation(movement));
        }
        else
        {
            if (_playerState != PlayerState.Knockback) { 
                rb.linearVelocity = Vector3.zero;
                _playerState = PlayerState.Idle;
            }
        }
    }

    private void MovementHandlerCharacterController()
    {
        Vector3 movement = new Vector3(moveAmount.x, 0, moveAmount.y);

        if (movement != Vector3.zero)
        {
            controller.Move(movement * playerSpeed * Time.deltaTime);
            transform.forward = movement;
            _playerState = PlayerState.Running;
        }
        else
        {
            _playerState = PlayerState.Idle;
        }

        if (!controller.isGrounded)
        {
            controller.Move(new Vector3(0.0f, -0.2f, 0.0f));
        }

    }

    private void AnimationHandler()
    {
        switch (_playerState)
        {
            case PlayerState.Idle:
                animator.SetBool("Running", false);
                break;
            case PlayerState.Running:
                animator.SetBool("Running", true);
                break;
        }
    }

    public void OnMove(InputValue value)
    {
        moveAmount = value.Get<Vector2>();
    }

    public void OnAttack()
    {
        if (weaponEquippedObject != null) {
            weaponEquippedObject.GetComponent<IWeapon>().Attack();
        }
        else
        {
            Debug.Log(playerNumber + " trying to attack with no weapon equipped.");
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {

    }
}
