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
using static UnityEngine.Rendering.DebugUI;
using TMPro;
//using System;


public delegate void PlayerAction();

public class PlayerHandler : MonoBehaviour
{
    private PlayerInput playerInput;
    private Vector2 moveAmount;
    private CharacterController controller;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Animator animator;
    private SkinnedMeshRenderer playerRenderer;

    //[SerializeField] private AudioClip dmgSFX;

    public enum PlayerNumber
    {
        PlayerOne, 
        PlayerTwo
    }
    public enum PlayerState
    {
        Idle,
        Running,
        Aiming,
        Dead
    }

    [Header("Main Player Attributes")]
    public PlayerState _playerState;
    public PlayerNumber playerNumber;
    public float playerHealth = 50.0f;
    public bool playerCanMove;
    public float maxPlayerSpeed = 25.0f;
    public float playerWeight = 0.0f;
    public float respawnTime = 3.0f;
    public bool invincible = false;
    public float invincibilityTime = 3.0f;
    public float healthRegenDelay = 4.0f;
    public float healthRegenPerSecond = 5.0f;
    private IEnumerator healthRegenCoroutine;

    [Header("Weapon Info")]
    public Transform weaponPlaceholderTransform;
    public GameObject defaultAttackWeapon;
    public GameObject staminaBar;  
    public float attackStamina = 1.0f;
    //public ParticleSystem weaponBreakParticleSystem;

    [Header("Appearance")]
    public Material flashMaterial;
    private Material defaultMaterial;
    public ParticleSystem PlayerWalkingParticleSystem;
    private float maxWalkingParticleSpeed;
    private float maxEmissionRateOverTime;

    [HideInInspector] public bool knockedBack = false;
    [HideInInspector] public Transform currentSpawnPosition;
    [HideInInspector] public int playerCurrentRoundScore;
    [HideInInspector] public List<GameObject> playerCurrentHoldingCheeses;
    [HideInInspector] public int playerTotalRoundScore = 0;
    [HideInInspector] public GameObject weaponEquippedObject;
    [HideInInspector] public GameObject possibleWeaponPickup;
    [HideInInspector] public StatTracker stats = new StatTracker();
    public PlayerUIHandler playerUIHandler;

    [HideInInspector] public GameStateManager.GameState gameStateBeforePause;
    [HideInInspector] public string actionMapBeforePause;

    //public event Action OnStaminaUse;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        playerUIHandler = GetComponent<PlayerUIHandler>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;

        playerRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        defaultMaterial = playerRenderer.material;
        maxWalkingParticleSpeed = PlayerWalkingParticleSystem.main.startSpeed.constant;
        maxEmissionRateOverTime = PlayerWalkingParticleSystem.emission.rateOverTime.constant;

        _playerState = PlayerState.Idle;
        playerCanMove = false;
        //currentSpawnPosition = transform;
        SetupDefaultAttack();

        gameStateBeforePause = GameStateManager.GameState.notInGame;
    }

    // Update is called once per frame
    void Update()
    {
        AnimationHandler();
    }

    private void FixedUpdate()
    {
        MovementHandlerRigidbody();
    }

    private void MovementHandlerRigidbody()
    {
        //rb.AddForce(Physics.gravity);

        if (_playerState == PlayerState.Dead || !playerCanMove || GameStateManager.instance._gameState == GameStateManager.GameState.isLoading || GameStateManager.instance._gameState == GameStateManager.GameState.isPaused)
        {
            return;
        }

        //if (!playerCanMove)
        //{
        //    rb.angularVelocity = Vector3.zero;
        //    rb.linearVelocity = Vector3.zero;
        //    return;
        //}

        Vector3 movement = new Vector3(moveAmount.x, 0, moveAmount.y);

        rb.angularVelocity = Vector3.zero;
        float newPlayerSpeed = maxPlayerSpeed - playerWeight;
        if (newPlayerSpeed <= 0) { newPlayerSpeed = 0; }

        if (movement != Vector3.zero)
        {
            if (knockedBack)
            {
                rb.AddForce(movement * newPlayerSpeed * Time.fixedDeltaTime * 1.5f * 9f, ForceMode.Force);
            }
            else
            {
                rb.linearVelocity = movement * newPlayerSpeed * Time.fixedDeltaTime * 9f;
            }

            float playerSpeedFactor = newPlayerSpeed / maxPlayerSpeed * movement.magnitude;

            if (animator != null)
            {
                animator.SetFloat("RunningSpeed", playerSpeedFactor);
            }

            // walking particle system
            var main = PlayerWalkingParticleSystem.main;
            main.startSpeed = playerSpeedFactor * maxWalkingParticleSpeed;
            var emission = PlayerWalkingParticleSystem.emission;
            emission.rateOverTime = playerSpeedFactor * maxEmissionRateOverTime;
            if (PlayerWalkingParticleSystem.isStopped)
            {
                PlayerWalkingParticleSystem.Play();
            }

            _playerState = PlayerState.Running;
            rb.MoveRotation(Quaternion.LookRotation(movement));
        }
        else
        {
            if (!knockedBack) { 
                rb.linearVelocity = Vector3.zero;
            }
            if (PlayerWalkingParticleSystem.isPlaying)
            {
                PlayerWalkingParticleSystem.Stop();
            }

            _playerState = PlayerState.Idle;
        }
    }

    private void AnimationHandler()
    {
        if (_playerState == PlayerState.Dead)
        {
            return;
        }

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
        if (GameStateManager.instance._gameState == GameStateManager.GameState.isLoading || GameStateManager.instance._gameState == GameStateManager.GameState.isPaused)
        {
            return;
        }

        if (weaponEquippedObject != null)
        {
            weaponEquippedObject.GetComponent<IWeapon>().Attack();
            stats.timesAttack++;
            //OnStaminaUse?.Invoke();
        }
    }

    //public void StartStaminaCooldown()
    //{

    //}

    //private IEnumerator StaminaIncrement()
    //{
    //    yield return null;
    //}

    //public void StopStaminaCooldown()
    //{
    //    if (attackStamina != 1.0f)
    //    {
    //        StopCoroutine(StaminaIncrement());
    //    }
    //}

    public void OnInteract()
    {
        if (GameStateManager.instance._gameState == GameStateManager.GameState.isLoading || GameStateManager.instance._gameState == GameStateManager.GameState.isPaused)
        {
            return;
        }

        if (possibleWeaponPickup == null)
        {
            Debug.Log("Nothing to pick up.");
            return;
        }

        if (weaponEquippedObject != null)
        {
            if (weaponEquippedObject.GetComponent<DefaultAttack>() != null)
            {
                weaponEquippedObject.GetComponent<DefaultAttack>().equippedCollider.enabled = false;
            }
            else
            {
                weaponEquippedObject.GetComponent<IWeapon>().DropWeapon();
            }
        }

        weaponEquippedObject = possibleWeaponPickup;
        weaponEquippedObject.GetComponent<IWeapon>().PickupWeapon(this.gameObject);
        possibleWeaponPickup = null;

        return;
    }

    public void OnDropWeapon()
    {
        if (weaponEquippedObject == null || weaponEquippedObject.GetComponent<DefaultAttack>() != null)
        {
            return;
        }

        weaponEquippedObject.GetComponent<IWeapon>().DropWeapon();
        SetupDefaultAttack();
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

        playerCanMove = false;
        rb.constraints = RigidbodyConstraints.None;
        animator.ResetTrigger("Idle");
        animator.SetTrigger("Death");

        //IEnumerator weaponAttackCoroutine = weaponEquippedObject.GetComponent<IWeapon>().attackCoroutine;
        //if (weaponAttackCoroutine != null)
        //{
        //    StopCoroutine(weaponAttackCoroutine);
        //}
        if (weaponEquippedObject != null)
        {
            if (weaponEquippedObject.GetComponent<DefaultAttack>() != null)
            {
                weaponEquippedObject.GetComponent<DefaultAttack>().equippedCollider.enabled = false;
            }
            else
            {
                weaponEquippedObject.GetComponent<IWeapon>().DropWeapon();
            }
        }

        DropCheeses();
        yield return new WaitForSeconds(respawnTime);

        rb.position = currentSpawnPosition.position;
        ResetPlayerValues();
        playerCanMove = true;
        //yield return new WaitForSeconds(invincibilityTime);
    }

    public void ResetPlayerValues()
    {
        rb.rotation = Quaternion.identity;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        animator.SetTrigger("Idle");
        knockedBack = false;
        playerHealth = 50.0f;
        maxPlayerSpeed = 25.0f;
        playerWeight = 0.0f;
        _playerState = PlayerState.Idle;
        playerCurrentHoldingCheeses.Clear();

        possibleWeaponPickup = null;
        if (weaponEquippedObject != null)
        {
            if (weaponEquippedObject.GetComponent<DefaultAttack>() == null)
            {
                Destroy(weaponEquippedObject);
            }
        }
        SetupDefaultAttack();

        if (healthRegenCoroutine != null)
        {
            StopCoroutine(healthRegenCoroutine);
        }
    }

    public void SetupDefaultAttack()
    {
        Debug.Log("default weapon: " + defaultAttackWeapon.name);
        weaponEquippedObject = defaultAttackWeapon;
        weaponEquippedObject.GetComponent<DefaultAttack>().owner = this.gameObject;
        animator.SetFloat("WeaponSwingSpeed", weaponEquippedObject.GetComponent<DefaultAttack>().swingSpeed);
    }

    public void TakeDamage(float damageAmount)
    {
        if (_playerState == PlayerState.Dead || invincible)
        {
            return;
        }

        AudioManager.instance.PlaySFX(AudioManager.SFXType.Damage);

        playerHealth -= damageAmount;
        playerUIHandler.HealthUpdate(playerHealth);
        StartCoroutine(HitFlash());

        if (playerHealth <= 0.0f)
        {
            _playerState = PlayerState.Dead;
            Debug.Log("Setting player to dead.");
            StartCoroutine(RespawnHandler());
        }

        if (healthRegenCoroutine != null)
        {
            StopCoroutine(healthRegenCoroutine);
        }
        healthRegenCoroutine = HealthRegen();
        StartCoroutine(healthRegenCoroutine);
    }

    private IEnumerator HealthRegen()
    {
        yield return new WaitForSeconds(healthRegenDelay);

        while (playerHealth < 50.0f)
        {
            playerHealth += healthRegenPerSecond;
            playerUIHandler.HealthUpdate(playerHealth);
            yield return new WaitForSeconds(1.0f);
        }
    }

    public IEnumerator TakeKnockback(Vector3 direction, float duration, float strength)
    {
        knockedBack = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(direction * strength, ForceMode.Impulse);
        rb.angularVelocity = Vector3.zero;

        yield return new WaitForSeconds(duration);

        knockedBack = false;
    }

    public void DropCheeses()
    {
        foreach (GameObject cheese in playerCurrentHoldingCheeses)
        {
            StartCoroutine(ThrowCheese(cheese));
        }
        playerCurrentHoldingCheeses.Clear();
    }

    private IEnumerator ThrowCheese(GameObject cheese)
    {
        yield return null;
        Rigidbody cheeseRB = cheese.GetComponent<Rigidbody>();
        CapsuleCollider cheeseCol = cheese.GetComponent<CapsuleCollider>();
        CheeseHandler cheeseHandler = cheese.GetComponent<CheeseHandler>();

        cheese.layer = 6;
        cheeseRB.position = this.rb.position + new Vector3(0.0f, 0.75f, 0.0f);
        cheeseRB.isKinematic = false;
        cheeseRB.useGravity = true;
        cheeseCol.isTrigger = false;

        Vector3 throwDirection = Random.onUnitSphere;
        throwDirection.y = 0.5f;
        cheeseRB.AddForce(throwDirection * 7.5f, ForceMode.VelocityChange);
        cheeseRB.AddTorque(new Vector3(0.0f, 0.75f, 0.0f), ForceMode.VelocityChange);

        yield return new WaitForSeconds(0.3f);
        while (!cheeseHandler.isGrounded)
        {
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);

        cheeseRB.isKinematic = true;
        cheeseRB.useGravity = false;
        cheeseRB.MovePosition(cheeseRB.position + new Vector3(0.0f, 0.3f, 0.0f));
        cheese.layer = 0;
        yield return null;
        cheeseHandler.StartFloatingAnimation();
        cheeseCol.isTrigger = true;
    }

    public void OnPause()
    {
        if (GameStateManager.instance._gameState == GameStateManager.GameState.isLoading)
        {
            return;
        }
        if (InputManager.instance.playerInPauseMenu != null && InputManager.instance.playerInPauseMenu != this.playerInput)
        {
            return;
        }

        if (GameStateManager.instance._gameState == GameStateManager.GameState.isPaused)
        {
            playerInput.SwitchCurrentActionMap(actionMapBeforePause);

            InputManager.instance.playerInPauseMenu = null;
            GameStateManager.instance._gameState = gameStateBeforePause;
            UIManager.instance.DeactivatePauseScreen();
            Time.timeScale = 1.0f;
        }
        else
        {
            actionMapBeforePause = playerInput.currentActionMap.name;
            playerInput.SwitchCurrentActionMap("UI");

            InputManager.instance.playerInPauseMenu = this.playerInput;
            gameStateBeforePause = GameStateManager.instance._gameState;
            GameStateManager.instance._gameState = GameStateManager.GameState.isPaused;
            StartCoroutine(UIManager.instance.ActivatePauseScreen());
            Time.timeScale = 0.0f;
        }
    }

    public void OnReset()
    {
        if (SceneManager.GetActiveScene().name == "TrainingArea" & GameStateManager.instance._gameState == GameStateManager.GameState.inGame)
        {
            DontDestroyOnLoad(this.gameObject);
            ResetPlayerValues();
            currentSpawnPosition = GameplaySceneReferences.instance.playerSpawnLocations[0];
            rb.position = currentSpawnPosition.position;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}