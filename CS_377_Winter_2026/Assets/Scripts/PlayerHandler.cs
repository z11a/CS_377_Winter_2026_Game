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
    private SkinnedMeshRenderer playerRenderer;

    public enum PlayerNumber
    {
        Player1, 
        Player2
    }
    public enum PlayerState
    {
        Idle,
        Running,
        Aiming,
        Dead
    }
    
    public float playerHealth = 50.0f;
    public float playerSpeed = 5.0f;
    //public float gravity = -2.0f; // not sure if we need this yet.
    public float respawnTime = 3.0f;
    public float invincibilityTime = 3.0f;
    public Material flashMaterial;
    private Material defaultMaterial;

    [HideInInspector] public bool knockedBack = false;
    [HideInInspector] public Transform currentSpawnPosition;
    [HideInInspector] public PlayerState _playerState;
    [HideInInspector] public int playerCurrentRoundScore;
    [HideInInspector] public List<GameObject> playerCurrentHoldingCheeses;
    [HideInInspector] public int playerTotalRoundScore;
    [HideInInspector] public PlayerNumber playerNumber;
    [HideInInspector] public GameObject weaponEquippedObject;
    [HideInInspector] public Transform rightHandTransform;    // this is needed for when the player is holding a weapon, there might be a better way of finding this bone though.

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        playerRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        defaultMaterial = playerRenderer.material;

        _playerState = PlayerState.Idle;
        rightHandTransform = transform.Find("mouse_rig/spine/spine_01/arm_r/forearm_r/forearm_r_end");
    }   

    // Update is called once per frame
    void Update()
    {
        if (GameStateManager.instance._gameState == GameStateManager.GameState.inGame)
        {
            //MovementHandlerCharacterController();
            AnimationHandler();
        }
    }

    private void FixedUpdate()
    {
        if (GameStateManager.instance._gameState == GameStateManager.GameState.inGame)
        {
            MovementHandlerRigidbody();
        }
    }

    private void MovementHandlerRigidbody()
    {
        if (_playerState == PlayerState.Dead)
        {
            return;
        }

        Vector3 movement = new Vector3(moveAmount.x, 0, moveAmount.y);

        rb.angularVelocity = Vector3.zero;

        if (movement != Vector3.zero)
        {
            if (knockedBack)
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
            if (!knockedBack) { 
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
        if (weaponEquippedObject != null)
        {
            weaponEquippedObject.GetComponent<IWeapon>().Attack();
        }
    }

    private IEnumerator HitFlash()
    {
        playerRenderer.material = flashMaterial;
        yield return new WaitForSeconds(0.1f);
        playerRenderer.material = defaultMaterial;
    }

    private IEnumerator RespawnHandler()
    {
        Debug.Log(playerNumber + " died.");

        GetComponent<PlayerInput>().DeactivateInput();
        rb.constraints = RigidbodyConstraints.None;
        animator.SetTrigger("Death");

        yield return new WaitForSeconds(respawnTime);

        rb.position = currentSpawnPosition.position;
        rb.constraints = RigidbodyConstraints.FreezeRotationX;
        rb.constraints = RigidbodyConstraints.FreezeRotationZ;
        rb.rotation = Quaternion.identity;
        rb.linearVelocity = Vector3.zero;
        animator.SetTrigger("Idle");
        knockedBack = false;
        playerHealth = 50.0f;
        _playerState = PlayerState.Idle;
        Destroy(weaponEquippedObject);
        GetComponent<PlayerInput>().ActivateInput();
        //yield return new WaitForSeconds(invincibilityTime);
    }

    public void TakeDamage(float damageAmount)
    {
        if (_playerState == PlayerState.Dead)
        {
            Debug.Log("Player is already dead.");
            return;
        }

        playerHealth -= damageAmount;
        StartCoroutine(HitFlash());

        if (playerHealth <= 0.0f)
        {
            _playerState = PlayerState.Dead;
            Debug.Log("Setting player to dead.");
            StartCoroutine(RespawnHandler());
        }
    }
}
