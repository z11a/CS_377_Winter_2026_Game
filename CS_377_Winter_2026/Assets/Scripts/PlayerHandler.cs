using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using UnityEditor;
using static PlayerHandler;
using static UnityEngine.UI.GridLayoutGroup;
using System.Security.Claims;

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
    public float respawnTime = 3.0f;
    public float invincibilityTime = 3.0f;
    public Material flashMaterial;
    private Material defaultMaterial;
    public PlayerUIManager playerUIManager;

    [HideInInspector] public PlayerState _playerState;
    [HideInInspector] public PlayerNumber playerNumber;

    [HideInInspector] public bool knockedBack = false;
    [HideInInspector] public Transform currentSpawnPosition;
    [HideInInspector] public int playerCurrentRoundScore;
    [HideInInspector] public List<GameObject> playerCurrentHoldingCheeses;
    [HideInInspector] public int playerTotalRoundScore = 0;
    [HideInInspector] public GameObject weaponEquippedObject;
    [HideInInspector] public GameObject possibleWeaponPickup;
    [HideInInspector] public StatTracker stats = new StatTracker();

    public Transform weaponPlaceholderTransform;

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
    }   

    // Update is called once per frame
    void Update()

    {
        if (GameStateManager.instance._gameState == GameStateManager.GameState.inGame)
        {
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
                rb.AddForce(movement * playerSpeed * Time.deltaTime * 1.5f * 9f, ForceMode.Force);
            }
            else
            {
                rb.linearVelocity = movement * playerSpeed * Time.deltaTime * 9f;
            }
            _playerState = PlayerState.Running;
            rb.MoveRotation(Quaternion.LookRotation(movement));
        }
        else
        {
            if (!knockedBack) { 
                rb.linearVelocity = Vector3.zero;
            }
            _playerState = PlayerState.Idle;
        }
    }

    //private void MovementHandlerCharacterController()
    //{
    //    Vector3 movement = new Vector3(moveAmount.x, 0, moveAmount.y);

    //    if (movement != Vector3.zero)
    //    {
    //        controller.Move(movement * playerSpeed * Time.deltaTime);
    //        transform.forward = movement;
    //        _playerState = PlayerState.Running;
    //    }
    //    else
    //    {
    //        _playerState = PlayerState.Idle;
    //    }

    //    if (!controller.isGrounded)
    //    {
    //        controller.Move(new Vector3(0.0f, -0.2f, 0.0f));
    //    }

    //}

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
        if (animator == null)
        {
            return;
        }
        animator.SetFloat("RunningSpeed", Vector2.Distance(value.Get<Vector2>(), Vector2.zero));
    }

    public void OnAttack()
    {
        if (weaponEquippedObject != null)
        {
            weaponEquippedObject.GetComponent<IWeapon>().Attack();
            stats.timesAttack++;
        }
        else
        {
            // add default attack here
        }
    }

    public void OnInteract()
    {
        if (possibleWeaponPickup == null)
        {
            Debug.Log("Nothing to pick up.");
            return;
        }

        if (weaponEquippedObject != null)
        {
            if (weaponEquippedObject.GetComponent<IWeapon>().attackCoroutine != null)       // stop swinging a previous weapon
            {
                StopCoroutine(weaponEquippedObject.GetComponent<IWeapon>().attackCoroutine);
            }
            OnDropWeapon();
        }

        weaponEquippedObject = possibleWeaponPickup;
        possibleWeaponPickup = null;

        MeleeHandler weaponMeleeHandler = weaponEquippedObject.GetComponent<MeleeHandler>();
        weaponMeleeHandler.owner = this.gameObject;
        weaponMeleeHandler.unequippedCollider.enabled = false;
        weaponMeleeHandler.meshRenderer.materials = weaponMeleeHandler.defaultMaterialList;

        weaponMeleeHandler._ItemState = IItem.ItemState.Collected;

        playerSpeed -= weaponEquippedObject.GetComponent<Rigidbody>().mass;
        animator.SetFloat("WeaponSwingSpeed", weaponMeleeHandler.swingSpeed);

        weaponEquippedObject.transform.parent = weaponPlaceholderTransform;
        weaponEquippedObject.transform.localPosition = Vector3.zero;
        weaponEquippedObject.transform.localRotation = Quaternion.Euler(30.864f, -8.384f, -38.901f);

        return;
    }

    public void OnDropWeapon()
    {
        if (weaponEquippedObject == null)
        {
            return;
        }
        Rigidbody weaponRB = weaponEquippedObject.GetComponent<Rigidbody>();
        weaponEquippedObject = null;
        weaponRB.transform.parent = null;

        //weaponRB.transform.position += transform.right * 0.25f;       // i was trying to get the player to throw the weapon slightly ahead of them, but it feels janky
        weaponRB.GetComponent<MeleeHandler>().unequippedCollider.isTrigger = false;
        weaponRB.GetComponent<MeleeHandler>().unequippedCollider.enabled = true;
        weaponRB.isKinematic = false;
        weaponRB.useGravity = true;
        //weaponRB.AddForce(transform.up / 3, ForceMode.Force);
        playerSpeed += weaponRB.mass;

        StartCoroutine(DespawnWeapon(weaponRB.gameObject));
    }

    private IEnumerator DespawnWeapon(GameObject weapon)
    {
        yield return new WaitForSeconds(1.0f);
        Destroy(weapon);
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

        DropCheeses();
        yield return new WaitForSeconds(respawnTime);

        rb.position = currentSpawnPosition.position;
        rb.rotation = Quaternion.identity;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.linearVelocity = Vector3.zero;
        animator.SetTrigger("Idle");
        knockedBack = false;
        playerHealth = 50.0f;
        playerSpeed = 25.0f;
        //playerUIManager.UpdateHealth((int)player);
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
        //playerUI.UpdateHealth((int)Mathf.Max(playerHealth, 0));

        if (playerHealth <= 0.0f)
        {
            _playerState = PlayerState.Dead;
            Debug.Log("Setting player to dead.");
            StartCoroutine(RespawnHandler());
        }
    }

    public void DropCheeses()
    {
        foreach (GameObject cheese in playerCurrentHoldingCheeses)
        {
            cheese.transform.position = transform.position + new Vector3(0.0f, 0.2f, 0.0f);   // have to use transform.position not rb.position, otherwise the floating animation sets the position before this line
            cheese.GetComponent<CheeseHandler>().StartFloatingAnimation();
            //cheese.GetComponent<CheeseHandler>().rb.useGravity = true;
            //cheese.GetComponent<CheeseHandler>().rb.isKinematic = false;
        }
        playerCurrentHoldingCheeses.Clear();
        //playerUI.UpdateCheeses(0);
    }


}
