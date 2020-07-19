using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MovingThing, IHittable
{
    /// <summary>
    /// Define goo colors
    /// </summary>
    public static Color[] gooColors = { new Color(.5f,1,.4f), new Color(.6f,.25f,.7f), Color.yellow };

    /// <summary>
    /// Movement properties, cont.
    /// </summary>
    [SerializeField]
    private float moveSpeed = 0;
    [SerializeField]
    private float acceleration = 0;
    [Header("Jumping"),SerializeField]
    private float jumpHeight = 0;
    [SerializeField]
    private float jumpTime = 0;
    [SerializeField]
    private float fallGravityMultiplier = 1;

    //Shooting
    [Header("Shooting"), SerializeField]
    private Transform gunArmPivot = null;
    [SerializeField]
    private SpriteRenderer gunArmSprite = null;
    [SerializeField]
    private Transform gunFirePoint = null;
    [SerializeField]
    private float rateOfFire = 1;
    [SerializeField]
    private float shotLaunchSpeed = 0;
    [SerializeField]
    private GameObject slimeballPrefab = null;
    [SerializeField]
    private ParticleSystem shotParticles = null;

    /// <summary>
    /// How much goo does the player have
    /// </summary>
    [Header("Goo"), SerializeField]
    private int maxGooAmount = 100;
    [SerializeField]
    private Image[] gooMeter = null;
    [SerializeField]
    private TextMeshProUGUI gooMeterLabel = null;
    [SerializeField]
    private Image currentGooColorSelector = null;

    /// <summary>
    /// How long are we invincible for?
    /// </summary>
    private float iTime = 0;
    

    [Header("Sound FX"), SerializeField]
    private AudioClip onLandSfx = null;
    [SerializeField]
    private AudioClip onShootSfx = null;
    [SerializeField]
    private AudioClip onNoShootSfx = null;
    [SerializeField]
    private AudioClip onGetSfx = null;
    [SerializeField]
    private AudioClip onHurtSfx = null;

    [Header("Death"), SerializeField]
    private ParticleSystem deathParticles;
    [SerializeField]
    private AudioSource deathAudio;
    [SerializeField]
    private Image deathScreen;

    [Header("Hat"), SerializeField]
    private SpriteRenderer hat;
    [SerializeField]
    private TextMeshProUGUI waveCount;

    /// <summary>
    /// How much goo do we have, currently?
    /// </summary>
    private int[] gooAmount;

    /// <summary>
    /// Which type of slime are we using?
    /// </summary>
    private SlimeType activeSlimeType = 0;

    //Get current slime count
    public int CurrentGooAmount
    {
        get { return gooAmount[(int)activeSlimeType];  }
        set { gooAmount[(int)activeSlimeType] = Mathf.Max(value,0);  }
    }

    /// <summary>
    /// Get all slime we have
    /// </summary>
    public int TotalGooAmount
    {
        get {return gooAmount[0] + gooAmount[1] + gooAmount[2];  }
    }

    /// <summary>
    /// When was the last shot fired, and how long is a single shot, respectively
    /// </summary>
    private float lastShotTime, shotDelay;

    /// <summary>
    /// Refrence to camera
    /// </summary>
    private Camera mainCam;

    /// <summary>
    /// Actual speed used when jumping
    /// </summary>
    private float jumpSpeed = 0;

    /// <summary>
    /// Refrence to moving thing
    /// </summary>
    private MovingThing movementController;

    /// <summary>
    /// Refrence to sprite renderer
    /// </summary>
    private new SpriteRenderer renderer;
    private Material playerMat;

    /// <summary>
    /// Refrence to audio source
    /// </summary>
    private new AudioSource audio;

    /// <summary>
    /// Refrence to animator
    /// </summary>
    private Animator animator;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        //Get components
        renderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        audio = GetComponent<AudioSource>();

        playerMat = renderer.sharedMaterial;
        playerMat.SetColor("_OverlayColor", Color.black);

        //Find main camera
        mainCam = Camera.main;

        //Caluclate jump speed
        CalculateJumpSpeed();

        //Calculate shot delay
        shotDelay = 1f / rateOfFire;
        lastShotTime = Time.time;

        //Update goo display
        gooAmount = new int[3];
        gooAmount[0] = maxGooAmount / 3;
        gooAmount[1] = maxGooAmount / 3;
        gooAmount[2] = maxGooAmount / 3;

        platformer.onGrounded.AddListener(() =>
        {
            audio.PlayOneShot(onLandSfx);
        });

        UpdateActiveSlimeColor();
        UpdateSlimeMeterDisplay();
    }

    // Update is called once per frame
   void Update()
   {
        float dt = Time.deltaTime;

        if (iTime > 0)
        {
            iTime -= dt;
            if (iTime <= 0)
            {
                playerMat.SetColor("_OverlayColor", Color.black);
            }
        }

        ShootUpdate(dt);
        MoveUpdate(dt);
    }

    /// <summary>
    /// Pickup
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Pickup"))
        {
            //Get pickup and add amount
            SlimePickup pickup = other.GetComponent<SlimePickup>();

            //How much can we add, max?
            int amt = Mathf.Min(maxGooAmount-TotalGooAmount, pickup.amount);

            if (amt > 0)
            {
                //Add goo
                gooAmount[(int)pickup.type] += amt;
                UpdateSlimeMeterDisplay();

                //Update scale
                UpdateScale();

                //Destroy pickup
                Destroy(other.gameObject);

                audio.PlayOneShot(onGetSfx);
            }
        }
    }

    /// <summary>
    /// Update movement
    /// </summary>
    /// <param name="dt"></param>
    private void MoveUpdate(float dt)
    {
        //Move
        float hor = Input.GetAxisRaw("Horizontal");
        velocity.x += hor * acceleration * dt;
        velocity.x = Mathf.Clamp(velocity.x, -moveSpeed, moveSpeed);

        animator.SetBool("Walking", hor != 0);

        //Jump
        if (platformer.IsGrounded && Input.GetButtonDown("Jump"))
        {
            velocity.y = jumpSpeed;
            //Variable jump height
        }
        else if (velocity.y > 1 && !Input.GetButton("Jump"))
        {
            velocity.y = Mathf.Lerp(velocity.y, 0, dt * 15);
        }

        //Handle base move code
        HandleMovement(dt, velocity.y < 0 ? fallGravityMultiplier : 1);
    }

    /// <summary>
    /// Update shooting
    /// </summary>
    private void ShootUpdate(float dt)
    {
        //Get mouse position
        Vector3 mousePoint = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mousePoint.z = 0;

        //Find angle between character and mouse
        Vector3 diff = mousePoint - gunArmPivot.position;
        float angle = Vector3.Angle(Vector3.right, diff);
        if (diff.y < 0) angle = 360 - angle;

        gunArmPivot.localEulerAngles = Vector3.forward * angle;

        //Flip sprites
        renderer.flipX = hat.flipX = gunArmSprite.flipY = diff.x < 0;


        //Switch active goo type
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            activeSlimeType += (int)Mathf.Sign(scroll);
            UpdateActiveSlimeColor();
        }


        //Actually fire
        if (Input.GetButton("Fire1") && Time.time >= lastShotTime + shotDelay)
        {
            lastShotTime = Time.time;
            if (CurrentGooAmount > 0)
            {
                animator.SetTrigger("Shoot");

                //Particles
                shotParticles.Play();

                //SFX
                audio.PlayOneShot(onShootSfx);

                //Instantiate slimeball and set velocity
                ShotSlimeballController ball = (Instantiate(slimeballPrefab, gunFirePoint.position, Quaternion.identity) as GameObject).GetComponent<ShotSlimeballController>();

                Vector2 vel = (Vector2)(diff.normalized * shotLaunchSpeed);
                ball.SetVelocity(vel + (Mathf.Sign(vel.x) == Mathf.Sign(velocity.x) ? velocity : Vector2.zero));
                ball.SetSlimeType(activeSlimeType);

                //Take goo
                CurrentGooAmount -= 2;
                if (CurrentGooAmount <= 0) UpdateActiveSlimeColor();

                //Update scale
                UpdateScale();

                //Check if we've died
                if (TotalGooAmount <= 0)
                    OnDeath();

                //Update display
                UpdateSlimeMeterDisplay();
            }
            else
            {
                audio.PlayOneShot(onNoShootSfx);
            }
        }
    }

    /// <summary>
    /// Update player scale to reflect slime amount
    /// </summary>
    private void UpdateScale()
    {
        //Calculate scale
        float scale = (float)TotalGooAmount / maxGooAmount;
        scale = (scale + 1) * 0.5f;

        //Update for player
        transform.localScale = Vector3.one * scale;

        //Zoom camera in/out
        mainCam.orthographicSize = scale * 10;

        //Recalculate bounds
        platformer.CalculateRaySpacing();
    }

    /// <summary>
    /// Calculate our gravity and jump speed based on our entered parameters
    /// </summary>
    protected void CalculateJumpSpeed()
    {
        gravity = (2 * jumpHeight) / (jumpTime * jumpTime);
        jumpSpeed = Mathf.Abs(gravity) * jumpTime;
    }

    /// <summary>
    /// Update the display to reflect our currently selected slime color
    /// </summary>
    private void UpdateActiveSlimeColor()
    {
        if (activeSlimeType < 0) activeSlimeType = SlimeType.Gold;
        else if ((int)activeSlimeType > 2) activeSlimeType = SlimeType.Green;
        
        //Update display tints
        Color c = gooColors[(int)activeSlimeType];

        currentGooColorSelector.color = CurrentGooAmount > 0 ? c : Color.gray;

        renderer.color = c;
        gunArmSprite.color = c;

        ParticleSystem.MainModule main = shotParticles.main;
        main.startColor = c;
    }

    /// <summary>
    /// Show how much slime we currently have
    /// </summary>
    private void UpdateSlimeMeterDisplay()
    {
        gooMeterLabel.text = "Slime: " + TotalGooAmount + "/" + maxGooAmount;
        float prevFillAmount = 0;
        for (int i = 0; i < 3; i++)
        {
            float fill = prevFillAmount + ((float)gooAmount[i] / maxGooAmount);
            gooMeter[i].fillAmount = fill;
            prevFillAmount = fill;
        }
    }

    /// <summary>
    /// Called when the player is hit
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="knockback"></param>
    public void OnHit(int amount, Vector2 knockback)
    {
        if (iTime <= 0)
        {
            iTime = .25f;
        }
        else return;

        for (int i=0;i<3;i++)
        {
            //Get slime type
            int type = i + (int)activeSlimeType;
            if (type > 2) type -= 3;

            //Subtract from that
            int maxSubtract = Mathf.Min(gooAmount[type], amount);
            gooAmount[type] -= maxSubtract;

            //If we have nothing left to subtract, leave loop
            if (maxSubtract >= amount)
            {
                break;
            }
            else
            {
                amount -= maxSubtract;
            }
        }

        velocity += knockback;

        //Update scale
        UpdateScale();

        audio.PlayOneShot(onHurtSfx);

        CameraController.instance?.AddCameraShake(amount/2);

        playerMat.SetColor("_OverlayColor", Color.white);

        //Check if we've died
        if (TotalGooAmount <= 0)
            OnDeath();

        UpdateSlimeMeterDisplay();
    }

    public Team GetTeam()
    {
        return Team.Player;
    }

    /// <summary>
    /// Called when we die
    /// </summary>
    private void OnDeath()
    {
        int count = Mathf.Max(0, WaveSpawner.instance?.GetWaveNum() ?? 0);
        waveCount.text = string.Format("You lasted {0} wave",count);
        if (count != 1) waveCount.text += "s";

        ParticleSystem.MainModule main = deathParticles.main;
        main.startColor = renderer.color;

        deathParticles.Play();
        deathAudio.Play();
        deathParticles.transform.SetParent(null);
        deathParticles.transform.localScale = Vector3.one * 3;

        audio.PlayOneShot(onHurtSfx);

        CameraController.instance?.AddCameraShake(5);

        deathScreen.gameObject.SetActive(true);
        deathScreen.CrossFadeAlpha(0, 0, false);
        deathScreen.CrossFadeAlpha(0.5f, 1f, false);

        gameObject.SetActive(false);
    }
}
