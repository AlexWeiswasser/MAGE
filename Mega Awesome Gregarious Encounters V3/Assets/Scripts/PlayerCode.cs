using PlayerControls;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.UI.Image;

public class PlayerCode : MonoBehaviour
{
    private Controls controls;//Controller variable needed for some reason

    public LayerMask floorLayer;

	Rigidbody2D rb;

    Collider2D staffCollider;

    SpriteRenderer playerRenderer;
    SpriteRenderer staffRenderer;
    public SpriteRenderer steepleRend;
    public SpriteRenderer brimRend;
    public SpriteRenderer starRend1;
    public SpriteRenderer starRend2;
    public SpriteRenderer starRend3;

    TextMeshProUGUI CounterText;

    Canvas canvas;

    public int playerID;

    private Gamepad gamepad;

    private PlayerInput playerInput;

    private PlayerInputManager playerInputManager;

    //Gameplay Map
    private InputAction spellCast;
    private InputAction moveCast;

    //Menu Map
    private InputAction selectLeft;
    private InputAction selectRight;

    private List<GameObject> listOfSpells = new List<GameObject>();//array of spawned spell's by this player

    Vector2 direction;//Angle between Stick Position And Player Position
    private Vector2 stickPos;//Stick's Angle
    Vector2 playerPos;//Player Transform but 2D

    GameObject Staff;//Staff the gameobject the child of this guy
    GameObject StaffChild;//Staff's Visual Child
    GameObject projectile;
    GameObject Reticle;
    public GameObject[] Spells;//Projectile for our players
    public GameObject[] BigSpells;//Projectile for counterspell
    public GameObject PlayerDeath;//Particles on death
    public GameObject Evaporator;//Deletes projectiles on counterspell
    public GameObject JuttingRock;
	public GameObject JuttingParticle;

	private List<GameObject> SpawnPoints = new List<GameObject>();

    public float StaffDistance = 2f;//Distance of Staff and Player
    public float SpinSpeed = 10000f;//Speed at which staff spins(VISUAL ONLY)
    public float health = 3f;//player's health
    float WhichCounterSpell = 0;
    float angle;
    private float screenShake = 0;
    float BigSpellTimer = .175f;
    float MageNumber = 0;
    float RightTriggerCounter = 0;
    float parryWindow;
    float CounterSpellKnockback;

	bool CanShoot; // Can you use your attack?
    bool CanMove; // Can you use your move abillity?
    bool Iframes; // Should you be taking damage right now?
    bool IframesVisual; // Makes the flashy visual
    bool StaffHit; // Was the staff, child of this object, hit?
    bool CheckingForLoad; // Checking if the scene is loading I think
    bool IsDefeated; // You dead?
    bool HasShook; // I think this is for screenshake
    private bool CauseOfTimeSlow; // The game is slow. Was that you?
    bool bigShotAlready; // Have you shot your counterspell? Disable this in it's if statment to make counterspell spamable. 

	//Dumb Corner a.k.a the real treasure was the friends we made along the way
	InputActionAsset InputActionAsset;

    //School Differences
    float ShootTimer;
    float MoveTimer;
    float jumpForce;
    float shootKnockback;
    float SideCapStrength; // If jumping sideways in same direction how weakened is it?
	float UpCapStrength; // If jumping sideways in same direction how weakened is it?
	float fireCooldown;
    float moveCooldown;
    float SpellSpawnDistance = -1.3f; // How far your spell spawns from you.

    private void Awake()
    {
        playerID = GameObject.FindGameObjectsWithTag("Player").Length;
        controls = new Controls();
        playerInput = GetComponent<PlayerInput>();
        playerInputManager = GameObject.Find("Manager").GetComponent<PlayerInputManager>();
        playerInput.actions["Aim"].performed += OnAimPerformed;
        GameObject.DontDestroyOnLoad(this);
    }
    // Start is called before the first frame update
    void Start()
    {
		SceneManager.sceneLoaded += OnSceneLoad;

        InputActionAsset inputActionAsset = playerInput.actions;

        //Gameplay Map
        spellCast = inputActionAsset.FindAction("Gameplay/Shoot");
        moveCast = inputActionAsset.FindAction("Gameplay/Movement");

        //Menu Map
        selectLeft = inputActionAsset.FindAction("Menu/SelectLeft");
        selectRight = inputActionAsset.FindAction("Menu/SelectRight");

        Transform TransStaff = transform.GetChild(0);
        Staff = TransStaff.gameObject;
        Transform TransStaffChild = TransStaff.GetChild(0);
        StaffChild = TransStaffChild.gameObject;
        Transform TransReticle = TransStaff.GetChild(1);
        Reticle = TransReticle.gameObject;
        staffCollider = Staff.GetComponent<Collider2D>();
        staffRenderer = StaffChild.GetComponent<SpriteRenderer>();
        playerRenderer = this.GetComponent<SpriteRenderer>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        CanShoot = true;
        CanMove = true;
        Iframes = true;
        IframesVisual = true;
        StaffHit = false;
        CauseOfTimeSlow = false;
        CheckingForLoad = false;
        HasShook = false;
        bigShotAlready = false;


        StartCoroutine(IframeCooldown());
        StartCoroutine(StopTheFreakingVelocity());
        FindStuff();

        if (spellCast == null) Debug.LogError("SpellCast action not found.");
        if (moveCast == null) Debug.LogError("MoveCast action not found.");

        this.transform.position = SpawnPoints[playerID - 1].transform.position;
        this.rb.velocity = new Vector2(0f, 0f);
    }
    // Update is called once per frame
    void Update()
    {
        TooCloseToWallRay(); // Check if too close to wall to fire a spell
		RockRay(); // Check if close enough to wall to wall jump for rock mage
		if (MageNumber == 1)//Fire Mage
        {
            jumpForce = 3300;
            SideCapStrength = .75f;
            UpCapStrength = .75f;

			fireCooldown = .35f;
            shootKnockback = 450;
            CounterSpellKnockback = 700;
            
            moveCooldown = .275f;
            parryWindow = .275f;     
		}
        if (MageNumber == 2)//Lightning Mage
        {
			jumpForce = 3000;
			SideCapStrength = .7f;
			UpCapStrength = .85f;

			fireCooldown = .35f;
			shootKnockback = 450;
			CounterSpellKnockback = 700;

			moveCooldown = .275f;
			parryWindow = .275f;
		}
        if (MageNumber == 3)//Harry Mage
        {
			jumpForce = 10000;
			SideCapStrength = 1f;
			UpCapStrength = 1f;

			fireCooldown = 0f;
			shootKnockback = 100;
			CounterSpellKnockback = 700;

			moveCooldown = .0f;
			parryWindow = .0f;
		}
        if (MageNumber == 4)//Warp Mage
        {
			jumpForce = 0;
			SideCapStrength = .90f;
			UpCapStrength = .90f;

			fireCooldown = .35f;
			shootKnockback = 0;
			CounterSpellKnockback = 0;

			moveCooldown = .375f;
			parryWindow = .375f;
		}
        if (MageNumber == 5)//Earth Mage
        { 
			jumpForce = 0;
			SideCapStrength = 1f;
			UpCapStrength = .85f;

			fireCooldown = .70f;
			shootKnockback = 1250;
			CounterSpellKnockback = 1250;

			moveCooldown = .500f;
			parryWindow = .300f;
		}
        if (selectRight.triggered)
        {
            if (RightTriggerCounter >= 1)
            {
                SwitchToGameplayControls();
            }
            RightTriggerCounter++;
        }
        if (RightTriggerCounter == 0 && SceneManager.GetActiveScene().name == "Setup Scene")
        {
            GameObject.Find("DirectionText" + playerID).GetComponent<TextMeshProUGUI>().text = "Press L2 to swap between schools! Press R2 to select. ";
        }
        if (RightTriggerCounter == 1 && SceneManager.GetActiveScene().name == "Setup Scene")
        {
            GameObject.Find("DirectionText" + playerID).GetComponent<TextMeshProUGUI>().text = "Press L2 to randomize your colors! Press R2 to select. ";
        }
        if (RightTriggerCounter >= 2 && SceneManager.GetActiveScene().name == "Setup Scene")
        {
            GameObject.Find("DirectionText" + playerID).GetComponent<TextMeshProUGUI>().text = "Hold L2 and R2 to ready up! ";
        }
        if (RightTriggerCounter == 1)
        {
            GameObject.Find("StarPreview1" + playerID).GetComponent<SpriteRenderer>().enabled = true;
            GameObject.Find("StarPreview2" + playerID).GetComponent<SpriteRenderer>().enabled = true;
            GameObject.Find("StarPreview3" + playerID).GetComponent<SpriteRenderer>().enabled = true;
            GameObject.Find("BrimPreview" + playerID).GetComponent<SpriteRenderer>().enabled = true;
            GameObject.Find("SteeplePreview" + playerID).GetComponent<SpriteRenderer>().enabled = true;
        }
        if (selectLeft.triggered)
        {
            if (RightTriggerCounter == 1)//textMeshPro.color = new Color(1f, 0.5f, 0f, 1f);  
            {
                Color randomColor = GenerateAGoodRandomColor();

                steepleRend.color = randomColor;
                brimRend.color = randomColor;

                Color inverseColor = InverseColor(randomColor);

                starRend1.color = inverseColor;
                starRend2.color = inverseColor;
                starRend3.color = inverseColor;

                GameObject.Find("StarPreview1" + playerID).GetComponent<SpriteRenderer>().color = inverseColor;
                GameObject.Find("StarPreview2" + playerID).GetComponent<SpriteRenderer>().color = inverseColor;
                GameObject.Find("StarPreview3" + playerID).GetComponent<SpriteRenderer>().color = inverseColor;
                GameObject.Find("BrimPreview" + playerID).GetComponent<SpriteRenderer>().color = randomColor;
                GameObject.Find("SteeplePreview" + playerID).GetComponent<SpriteRenderer>().color = randomColor;
            }
            else if (RightTriggerCounter == 0)
            {
                if (MageNumber == 0)
                {
                    MageNumber = 1;
                    GameObject.Find("MageTypeText" + playerID).GetComponent<TextMeshProUGUI>().text = "FLAMES";
                    GameObject.Find("MageTypeText" + playerID).GetComponent<TextMeshProUGUI>().color = Color.red;
					StaffChild.GetComponent<SpriteRenderer>().color = Color.red;
					Reticle.GetComponent<SpriteRenderer>().color = Color.red;
				}
                else if (MageNumber == 1)
                {
                    MageNumber = 2;
                    GameObject.Find("MageTypeText" + playerID).GetComponent<TextMeshProUGUI>().text = "LIGHTNING";
                    GameObject.Find("MageTypeText" + playerID).GetComponent<TextMeshProUGUI>().color = Color.blue;
					StaffChild.GetComponent<SpriteRenderer>().color = Color.blue;
					Reticle.GetComponent<SpriteRenderer>().color = Color.blue;
				}
                else if (MageNumber == 2)
                {
                    MageNumber = 3;
                    GameObject.Find("MageTypeText" + playerID).GetComponent<TextMeshProUGUI>().text = "HARRY";
                    GameObject.Find("MageTypeText" + playerID).GetComponent<TextMeshProUGUI>().color = Color.grey;
					StaffChild.GetComponent<SpriteRenderer>().color = Color.grey;
					Reticle.GetComponent<SpriteRenderer>().color = Color.grey;
				}
                else if (MageNumber == 3)
                {
                    MageNumber = 4;
                    GameObject.Find("MageTypeText" + playerID).GetComponent<TextMeshProUGUI>().text = "WARP";
                    GameObject.Find("MageTypeText" + playerID).GetComponent<TextMeshProUGUI>().color = Color.yellow;
					StaffChild.GetComponent<SpriteRenderer>().color = Color.yellow;
					Reticle.GetComponent<SpriteRenderer>().color = Color.yellow;
				}
                else if (MageNumber == 4)
                {
                    MageNumber = 5;
                    GameObject.Find("MageTypeText" + playerID).GetComponent<TextMeshProUGUI>().text = "EARTH";
                    GameObject.Find("MageTypeText" + playerID).GetComponent<TextMeshProUGUI>().color = Color.green;
					StaffChild.GetComponent<SpriteRenderer>().color = Color.green;
					Reticle.GetComponent<SpriteRenderer>().color = Color.green;
				}
                else if (MageNumber == 5)
                {
                    MageNumber = 1;
                    GameObject.Find("MageTypeText" + playerID).GetComponent<TextMeshProUGUI>().text = "FLAMES";
                    GameObject.Find("MageTypeText" + playerID).GetComponent<TextMeshProUGUI>().color = Color.red;
					StaffChild.GetComponent<SpriteRenderer>().color = Color.red;
					Reticle.GetComponent<SpriteRenderer>().color = Color.red;
				}
            }
        }
        if (Time.timeScale == .1f)
        {
            BigSpellTimer -= Time.deltaTime * 10;
        }
        if (screenShake != 0)
        {
            Camera.main.GetComponent<ScreenShakeCode>().spellShake(screenShake);
            screenShake = 0;
        }
        if (IsDefeated)
        {
            if (!HasShook)
            {
                StartCoroutine(DeathShake());
            }
            gamepad.SetMotorSpeeds(0f, 0f);
            return;
        }
        playerPos = new Vector2(transform.position.x, transform.position.y);//Player's Position in 2D
        direction = (playerPos - (stickPos * 1000000)).normalized;//Angle Between Player And Staff
        Vector3 directionToPlayer = (transform.position - Staff.transform.position).normalized;
        angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        Staff.transform.rotation = Quaternion.Euler(0, 0, angle);

        if (health <= 0)
        {
            starRend3.enabled = false;
        }
        if (health <= 2)
        {
            starRend2.enabled = false;
        }
        if (health <= 1)
        {
            starRend1.enabled = false;
        }
        if (health <= -1)
        {
            Instantiate(PlayerDeath, new Vector3(transform.position.x, transform.position.y), Quaternion.identity);
            IsDefeated = true;
            this.transform.position = new Vector2(0f, -10000f); //MAKE A BETTER DEATH SYSTEM
            playerPos = new Vector2(transform.position.x, transform.position.y);
            direction = (playerPos - (stickPos * 100)).normalized;
            directionToPlayer = (transform.position - Staff.transform.position).normalized;
            angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
            Staff.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        if (Time.timeScale == 1)
        {
            CounterText.text = "";
            bigShotAlready = false;
        }
        if (spellCast.triggered && Time.timeScale == .1f && CauseOfTimeSlow)
        {
            if (BigSpellTimer < 0)
            {
                WhichCounterSpell = 1;
                CheckCounterSpell();
            }
        }
        ReassignGamepad();
        if (spellCast.IsPressed() && moveCast.IsPressed() && SceneManager.GetActiveScene().name == "Setup Scene" && !CheckingForLoad)
        {
            StartCoroutine(CheckForLoad());
        }
    }
    public void FixedUpdate()
    {
        if (IframesVisual)
        {
            if (playerRenderer.enabled)
            {
                playerRenderer.enabled = false;
                steepleRend.enabled = false;
                brimRend.enabled = false;
                starRend1.enabled = false;
                starRend2.enabled = false;
                starRend3.enabled = false;
            }
            else
            {
                playerRenderer.enabled = true;
                steepleRend.enabled = true;
                brimRend.enabled = true;
                starRend1.enabled = true;
                starRend2.enabled = true;
                starRend3.enabled = true;
            }
        }
        else
        {
            playerRenderer.enabled = true;
            steepleRend.enabled = true;
            brimRend.enabled = true;
            starRend1.enabled = true;
            starRend2.enabled = true;
            starRend3.enabled = true;
        }
    }
    public void OrbitFunction()
    {
        if (Mathf.Abs(playerPos.x) > 5000 || Mathf.Abs(playerPos.y) > 5000)
        {
            return;
        }
        Staff.transform.position = playerPos - direction * 1.75f;//Orbit Position-wise
    }
    public void MoveAndBlock()
    {
        if (moveCast.triggered && CanMove && !CauseOfTimeSlow && Time.timeScale != .1f && !IsDefeated && MageNumber != 5)
        {
            WhichCounterSpell = 0;

            // Get the current velocity
            Vector2 currentVelocity = rb.velocity;

            // Normalize the velocity components to the range 0-1 based on magnitude
            float xFactor = Mathf.Clamp01(Mathf.Abs(currentVelocity.x) / 50f);
            float yFactor = Mathf.Clamp01(Mathf.Abs(currentVelocity.y) / 50f);

            // Determine how much the force should be scaled based on the direction of velocity
            float xScale = (currentVelocity.x * direction.x > 0) ? (1f - xFactor) : SideCapStrength;
            float yScale = (currentVelocity.y * direction.y > 0) ? (1f - yFactor) : UpCapStrength;

            // Apply the scaled force in the direction
            Vector2 adjustedDirection = new Vector2(direction.x * xScale, direction.y * yScale);

            // Apply the jump force
            rb.velocity += adjustedDirection * jumpForce * Time.fixedDeltaTime;

            if(MageNumber == 4)
            {
                TeleportRay();
            }

            staffCollider.enabled = true;
            staffRenderer.enabled = true;
            Reticle.SetActive(false);
            CanMove = false;

			StartCoroutine(MovementCooldown());
        }
    }
    public void Attack()
    {
		if (spellCast.triggered && CanShoot && !CauseOfTimeSlow && Time.timeScale != .1f && !IsDefeated)
        {
            projectile = Instantiate(Spells[(int)MageNumber - 1], Staff.transform.position + Staff.transform.right * SpellSpawnDistance, Quaternion.identity, transform);
			ProjectileCode prefabData = projectile.GetComponent<ProjectileCode>();
			prefabData.SetPrefabIndex((int)MageNumber - 1);
            rb.velocity += direction * shootKnockback * Time.fixedDeltaTime;
            listOfSpells.Add(projectile);

            staffCollider.enabled = false;
            staffRenderer.enabled = false;
            StaffHit = false; 
			CanShoot = false;

            StartCoroutine(ShootCooldown());
        }
    }
    private void OnAimPerformed(InputAction.CallbackContext ctx)
    {
        stickPos = ctx.ReadValue<Vector2>();
    }
    IEnumerator ShootCooldown()
    {
        // Start a short vibration
        float LeftRand = UnityEngine.Random.Range(0.05f, 0.5f);
        float RightRand = UnityEngine.Random.Range(0.05f, 0.7f);
        gamepad.SetMotorSpeeds(LeftRand, RightRand);

        // Wait for a short duration (e.g., 0.1 seconds)
        yield return new WaitForSeconds(0.1f);

        // Stop the vibration
        gamepad.SetMotorSpeeds(0f, 0f);
        yield return new WaitForSeconds(fireCooldown);
        CanShoot = true;
    }
    IEnumerator MovementCooldown()
    {
        yield return new WaitForSeconds(parryWindow);

        staffCollider.enabled = false;
        staffRenderer.enabled = false;
        StaffHit = false; 
		Reticle.SetActive(true);

        yield return new WaitForSeconds(moveCooldown-parryWindow);

        CanMove = true;
    }
    IEnumerator DeathShake()
    {
        // Start a short vibration
        float LeftRand = UnityEngine.Random.Range(0.5f, .8f);
        float RightRand = UnityEngine.Random.Range(0.5f, .8f);
        gamepad.SetMotorSpeeds(LeftRand, RightRand);

        // Wait for a short duration (e.g., 0.1 seconds)
        yield return new WaitForSeconds(0.3f);

        // Stop the vibration
        gamepad.SetMotorSpeeds(0f, 0f);

        HasShook = true;
    }
    IEnumerator HitShake()
    {
        float LeftRand = UnityEngine.Random.Range(0.5f, .8f);
        float RightRand = UnityEngine.Random.Range(0.5f, .8f);
        gamepad.SetMotorSpeeds(LeftRand, RightRand);

        // Wait for a short duration (e.g., 0.1 seconds)
        yield return new WaitForSeconds(0.05f);

        // Stop the vibration
        gamepad.SetMotorSpeeds(0f, 0f);
    }
    IEnumerator CounterSpellShake()
    {
        float LeftRand = UnityEngine.Random.Range(.9f, 1f);
        float RightRand = UnityEngine.Random.Range(0f, 0f);
        gamepad.SetMotorSpeeds(LeftRand, RightRand);

        // Wait for a short duration (e.g., 0.1 seconds)
        yield return new WaitForSeconds(0.1f);

        // Stop the vibration
        gamepad.SetMotorSpeeds(0f, 0f);
    }
    IEnumerator IframeCooldown()
    {
        yield return new WaitForSeconds(1f);
        IframesVisual = false;
        Iframes = false;
    }
    IEnumerator IframeCooldown2()
    {
        yield return new WaitForSeconds(.1f);
        IframesVisual = false;
        Iframes = false;
    }
    IEnumerator TimeSlow()
    {
        Time.timeScale = .1f;
        yield return new WaitForSecondsRealtime(.6f);//how long the big spell time slow is
        Time.timeScale = 1.0f;
        CauseOfTimeSlow = false;
        Evaporator.SetActive(false);
    }
    IEnumerator ScreenShakeController(float shake)
    {
        screenShake = shake;
        yield return new WaitForSeconds(.15f);
        screenShake = 0f;
    }
    IEnumerator CheckForLoad()
    {
        CheckingForLoad = true;
        bool GoingToLoad = false;
        GameObject loadBar = GameObject.Find("TriggerBar" + playerID);
        RectTransform rt = loadBar.GetComponent<RectTransform>();
        if (rt.sizeDelta.x >= 400)
        {
            GoingToLoad = true;
            for (int i = 1; i <= GameObject.FindGameObjectsWithTag("Player").Length; i++)
            {
                loadBar = GameObject.Find("TriggerBar" + i);
                rt = loadBar.GetComponent<RectTransform>();
                if (rt.sizeDelta.x < 400 || GameObject.FindGameObjectsWithTag("Player").Length == 1)
                {
                    GoingToLoad = false;
                }
            }
            if (GoingToLoad)
            {
                SceneManager.LoadScene(0);
            }
        }
        else
        {
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);
            float checkDuration = 0.8f;
            float elapsedTime = 0f;
            bool bothPressed = true;

            while (elapsedTime < checkDuration)
            {
                // Check if both actions are true
                if (!(spellCast.IsPressed() && moveCast.IsPressed()))
                {
                    bothPressed = false;
                    rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);
                    break;
                }

                elapsedTime += Time.deltaTime;
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, elapsedTime * 500f);
                yield return null; // Wait until the next frame
            }

            if (!bothPressed)
            {
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0f);
            }
            else
            {
                GameObject.Find("ReadyText" + playerID).GetComponent<TextMeshProUGUI>().text = "READY";
            }

        }
        CheckingForLoad = false;
    }

    //TODO: Figure out why the player flies away from spawn after start
    IEnumerator StopTheFreakingVelocity()
    {
        yield return new WaitForSeconds(0.001f);
        this.rb.velocity = new Vector2(0f, 0f);
    }
	public void CollisionDetected(CounterCode childScript)
    {
        StaffHit = true;
    }
    void CheckCounterSpell()
    {
        // Check if both conditions are met and within the allowed time window
        if (WhichCounterSpell == 1 && !bigShotAlready)
        {
            bigShotAlready = true;
            Evaporator.SetActive(false);
            CounterText.text = "COUNTERSPELL";
            GameObject BigProjectile = Instantiate(BigSpells[(int)MageNumber - 1], transform.position + Staff.transform.right * -4f, Quaternion.identity, transform);
            StartCoroutine(CounterSpellShake());
			CounterProjectileCode prefabData = BigProjectile.GetComponent<CounterProjectileCode>();
			prefabData.SetPrefabIndex((int)MageNumber - 1);
			//BigProjectile.transform.localScale = Spells[(int)MageNumber - 1].transform.localScale;
			rb.velocity += direction * CounterSpellKnockback * Time.fixedDeltaTime;
			listOfSpells.Add(BigProjectile);
			WhichCounterSpell = 0;
            BigSpellTimer = .175f;
        }
    }
    public bool IsPrefabSpawnedByParent(GameObject prefab)
    {
        return listOfSpells.Contains(prefab);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Cases where nothing should happen
        if (listOfSpells.Contains(collision.gameObject) || collision.gameObject.tag == "Staff" || Iframes)
        {
            return;
        }
        //Cases where something should happen
        if (collision.gameObject.tag == "Spell" || collision.gameObject.tag == "BigSpell")
        {
			if (StaffHit && !bigShotAlready)//Cases where COUNTERSPELL
            {
                StaffHit = false;
                float staffAngle = Staff.transform.rotation.eulerAngles.z;
                float spellAngle = collision.transform.rotation.eulerAngles.z;
                float finalAngle = Mathf.Abs(staffAngle - spellAngle);
                if (Mathf.Abs(staffAngle - (spellAngle - 180)) <= 15f || Mathf.Abs(staffAngle - (spellAngle + 180)) <= 15f)
                {
                    GameObject ThingHitByStaff = collision.gameObject;
                    Destroy(ThingHitByStaff);
                    CauseOfTimeSlow = true;
                    Evaporator.SetActive(true);
                    CounterText.text = "COUNTER";
                    Iframes = true;
                    StartCoroutine(IframeCooldown2());
                    StartCoroutine(TimeSlow());
                }
            }
            else if (!CauseOfTimeSlow)//If you're not counterspelling...
            {
                if (collision.gameObject.tag == "BigSpell")//and hit with a big spell
                {
                    if (SceneManager.GetActiveScene().name != "Setup Scene")
                    {
                        health--;
                    }
                    StartCoroutine(ScreenShakeController(.85f));
                    Vector2 Knockback2;
                    Knockback2 = collision.gameObject.GetComponent<Rigidbody2D>().velocity.normalized;
                    rb.velocity += Knockback2 * 17500 * Time.fixedDeltaTime;
                    IframesVisual = true;
                    Iframes = true;
                    StartCoroutine(HitShake());
                    StartCoroutine(IframeCooldown());
                    staffCollider.enabled = false;
                    staffRenderer.enabled = false;
					StaffHit = false;
				}
                else if (collision.gameObject.tag == "Spell")//and hit with a normal spell
                {
                    if (SceneManager.GetActiveScene().name != "Setup Scene")
                    {
                        health--;
                    }
                    StartCoroutine(ScreenShakeController(.2f));
                    Vector2 Knockback;
                    Knockback = collision.gameObject.GetComponent<Rigidbody2D>().velocity.normalized;
                    rb.velocity += Knockback * 2500 * Time.fixedDeltaTime;
                    IframesVisual = true;
                    Iframes = true;
                    StartCoroutine(HitShake());
                    StartCoroutine(IframeCooldown());
                    staffCollider.enabled = false;
                    staffRenderer.enabled = false;
					StaffHit = false;
				}
            }
        }
    }
	private void OnCollisionEnter2D(Collision2D collision)
	{
		//Cases where nothing should happen
		if (listOfSpells.Contains(collision.gameObject) || collision.gameObject.tag == "Staff" || Iframes)
		{
			return;
		}
		//Cases where something should happen
		if (collision.gameObject.tag == "Spell" || collision.gameObject.tag == "BigSpell")
		{
			if (StaffHit && !bigShotAlready)//Cases where COUNTERSPELL
			{
				StaffHit = false;
				float staffAngle = Staff.transform.rotation.eulerAngles.z;
				float spellAngle = collision.transform.rotation.eulerAngles.z;
				float finalAngle = Mathf.Abs(staffAngle - spellAngle);
				if (Mathf.Abs(staffAngle - (spellAngle - 180)) <= 15f || Mathf.Abs(staffAngle - (spellAngle + 180)) <= 15f)
				{
					GameObject ThingHitByStaff = collision.gameObject;
					Destroy(ThingHitByStaff);
					CauseOfTimeSlow = true;
					Evaporator.SetActive(true);
					CounterText.text = "COUNTER";
					Iframes = true;
					StartCoroutine(IframeCooldown2());
					StartCoroutine(TimeSlow());
				}
			}
			else if (!CauseOfTimeSlow)//If you're not counterspelling...
			{
				if (collision.gameObject.tag == "BigSpell")//and hit with a big spell
				{
					if (SceneManager.GetActiveScene().name != "Setup Scene")
					{
						health--;
					}
					StartCoroutine(ScreenShakeController(.85f));
					Vector2 Knockback2;
					Knockback2 = collision.gameObject.GetComponent<Rigidbody2D>().velocity.normalized;
					rb.velocity += Knockback2 * 17500 * Time.fixedDeltaTime;
					IframesVisual = true;
					Iframes = true;
					StartCoroutine(HitShake());
					StartCoroutine(IframeCooldown());
					staffCollider.enabled = false;
					staffRenderer.enabled = false;
					StaffHit = false;
				}
				else if (collision.gameObject.tag == "Spell")//and hit with a normal spell
				{
					if (SceneManager.GetActiveScene().name != "Setup Scene")
					{
						health--;
					}
					StartCoroutine(ScreenShakeController(.2f));
					Vector2 Knockback;
					Knockback = collision.gameObject.GetComponent<Rigidbody2D>().velocity.normalized;
					rb.velocity += Knockback * 2500 * Time.fixedDeltaTime;
					IframesVisual = true;
					Iframes = true;
					StartCoroutine(HitShake());
					StartCoroutine(IframeCooldown());
					staffCollider.enabled = false;
					staffRenderer.enabled = false;
					StaffHit = false;
				}
			}
		}
	}
	private void ReassignGamepad()
    {
        if (gamepad == null)
        {
            gamepad = Gamepad.current;
        }
    }

    Color GenerateAGoodRandomColor()
    {
        return UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.75f, 1f);
    }

    // Calculate the inverse color
    Color InverseColor(Color color)
    {
        return new Color(1 - color.r, 1 - color.g, 1 - color.b);
    }

    private void FindStuff()
    {
        SpawnPoints = new List<GameObject>();
        SpawnPoints.AddRange(GameObject.FindGameObjectsWithTag("Spawn"));
        SpawnPoints.Sort((go1, go2) => string.Compare(go1.name, go2.name));
        GameObject textObject = GameObject.Find("CounterText");
        if (textObject != null)
        {
            CounterText = textObject.GetComponent<TextMeshProUGUI>();
        }
        if (SceneManager.GetActiveScene().name == "Setup Scene")
        {
            GameObject.Find("ControllerText" + playerID).GetComponent<TextMeshProUGUI>().text = "Player " + playerID + " Connected";
        }
        this.transform.position = SpawnPoints[playerID - 1].transform.position;
        canvas = FindObjectOfType<Canvas>();
        this.rb.velocity = new Vector2(0f, 0f);
    }

    void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        RightTriggerCounter = 0;
        IsDefeated = false;
        IframesVisual = true;
        Iframes = true;
        StartCoroutine(IframeCooldown());
        if (SceneManager.GetActiveScene().name != "Setup Scene")
        {
            playerInputManager.DisableJoining();
        }
        else
        {
            OrbitFunction();
			RightTriggerCounter = 0;
            MageNumber = 0;
			GameObject.Find("StarPreview1" + playerID).GetComponent<SpriteRenderer>().enabled = false;
            GameObject.Find("StarPreview2" + playerID).GetComponent<SpriteRenderer>().enabled = false;
            GameObject.Find("StarPreview3" + playerID).GetComponent<SpriteRenderer>().enabled = false;
            GameObject.Find("BrimPreview" + playerID).GetComponent<SpriteRenderer>().enabled = false;
            GameObject.Find("SteeplePreview" + playerID).GetComponent<SpriteRenderer>().enabled = false;
            playerInputManager.EnableJoining();

            SwitchToMenuControls(); // This junk is for getting the player back in the select menu 
			MageNumber = 1;
			GameObject.Find("MageTypeText" + playerID).GetComponent<TextMeshProUGUI>().text = "FLAMES";
			GameObject.Find("MageTypeText" + playerID).GetComponent<TextMeshProUGUI>().color = Color.red;
			StaffChild.GetComponent<SpriteRenderer>().color = Color.red;
			Reticle.GetComponent<SpriteRenderer>().color = Color.red;
		}
        health = 3f;
        FindStuff();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
        if (gamepad != null)
        {
            gamepad.SetMotorSpeeds(0f, 0f);
        }
    }
    public void SwitchToGameplayControls()
    {
        playerInput.SwitchCurrentActionMap("Gameplay");
        GameObject.Find("ImageCam" + playerID).GetComponent<RawImage>().color = new Color(255f, 255f, 255f, 255f);
    }
    public void SwitchToMenuControls()
    {
        playerInput.SwitchCurrentActionMap("Menu");
    }
	public void RockRay()
    {
		Vector3 origin = transform.position; // Raycast origin from object position
		RaycastHit2D hitFloor = Physics2D.Raycast(origin, Staff.transform.right, -4.5f, floorLayer);
		if (hitFloor.collider != null)
		{
			if (moveCast.triggered && MageNumber == 5)
			{
				rb.velocity = rb.velocity * .7f; // Controls velocity held over after jump with rock wiz

				rb.velocity += direction * 3750 * Time.fixedDeltaTime; // Rock wiz jump strength

				// Calculate the distance between the origin and hit point
				float distance = Vector3.Distance(origin, hitFloor.point);

				// Instantiate the rectangle prefab
				GameObject rectangle = Instantiate(JuttingRock);

				// Position the rectangle at the midpoint between the origin and hit point
				rectangle.transform.position = (origin + (Vector3)hitFloor.point) / 2;

				// Calculate the angle between the origin and hit point
				Vector2 direction2 = hitFloor.point - (Vector2)origin;
				float angle = Mathf.Atan2(direction2.y, direction2.x) * Mathf.Rad2Deg;
				rectangle.transform.rotation = Quaternion.Euler(0, 0, angle);

				// Scale the rectangle to stretch from the origin to the hit point
				rectangle.transform.localScale = new Vector3(distance, rectangle.transform.localScale.y, 1); // Adjust the Y scale if needed

				Instantiate(JuttingParticle, new Vector3(hitFloor.point.x, hitFloor.point.y), Quaternion.identity);
			}
		}
        else if(moveCast.triggered && MageNumber == 5 && CanMove)
        {
			WhichCounterSpell = 0;

			staffCollider.enabled = true;
			staffRenderer.enabled = true;
			Reticle.SetActive(false);
			CanMove = false;

			StartCoroutine(MovementCooldown());
		}
	}
	public void TooCloseToWallRay()
    {
		Vector3 origin = transform.position;
		RaycastHit2D ReticleInFloor = Physics2D.Raycast(origin, Staff.transform.right, -1.95f, floorLayer);
		if (ReticleInFloor.collider != null)
        {
            SpellSpawnDistance = 0f;
        }
        else
        {
            SpellSpawnDistance = -1.3f;
		}
	}
	public void TeleportRay()
    {
		Vector3 origin = transform.position;
		RaycastHit2D TeleportLocation = Physics2D.Raycast(origin, Staff.transform.right, 8.5f, floorLayer);

        rb.velocity = rb.velocity * .50f;

		Vector2 currentVelocity = rb.velocity;

		// Normalize the velocity components to the range 0-1 based on magnitude
		float xFactor = Mathf.Clamp01(Mathf.Abs(currentVelocity.x) / 50f);
		float yFactor = Mathf.Clamp01(Mathf.Abs(currentVelocity.y) / 50f);

		// Determine how much the force should be scaled based on the direction of velocity
		float xScale = (currentVelocity.x * direction.x > 0) ? (1f - xFactor) : SideCapStrength;
		float yScale = (currentVelocity.y * direction.y > 0) ? (1f - yFactor) : UpCapStrength;

		// Apply the scaled force in the direction
		Vector2 adjustedDirection = new Vector2(direction.x * xScale, direction.y * yScale);

		// Apply the jump force
		rb.velocity += adjustedDirection * 2500f * Time.fixedDeltaTime;

		if (TeleportLocation.collider != null)
        {
            transform.position = TeleportLocation.point;
        }
        else
        {
            transform.position += Staff.transform.right * 8.5f;

		}
	}
}
