using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    private bool didDie = false;


    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float jumpForce = 5f;
    [SerializeField] private float stepHeight = 0.3f; // Karakterin atlayabileceği maksimum adım yüksekliği
    [SerializeField] private float stepRayDistance = 0.2f; // Engeli algılamak için yatay raycast mesafesi

    
    [Header("Jump Settings")]
    public float coyoteTime = 0.2f; // Platformdan düştükten sonra zıplama için izin verilen süre
    private float coyoteTimeCounter; // Geri sayım sayacı
    private bool hasJumped = false;
    public float jumpResetDelay = 0.2f;    // Zıpladıktan sonra hasJumped'i sıfırlamak için bekleme süresi
    private float jumpResetTimer = 0f;


    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;


    [Header("Attack Settings")]
    public float attackCooldown = 0.5f;   // Saldırı tekrarına izin veren süre
    public float attackDuration = 0.2f;   // Saldırı animasyon/işlem süresi
    public Transform attackPoint;         // Kılıcın ucu/kılıca yakın bir nokta
    public float attackRange = 0.5f;
    public LayerMask enemyLayer;
    public int attackDamage = 10;


    [Header("Wall Climb Settings")]
    public Transform wallCheck;
    public float wallCheckDistance = 0.5f;
    public float wallSlideSpeed = 2f;
    public float wallClimbSpeed = 3f;
    public LayerMask wallLayer;


    [Header("Dash Settings")]
    [Tooltip("Dash hızı (x ekseninde). Karakter ne kadar güçlü atılsın?")]
    public float dashSpeed = 10f;
    [Tooltip("Dash süresi. (Karakterin hızlı şekilde ilerleyeceği zaman aralığı)")]
    public float dashDuration = 0.2f;
    [Tooltip("Dash tekrar kullanılmadan önce beklenmesi gereken süre.")]
    public float dashCooldown = 1f;
    [Tooltip("Dash için kullanılacak tuş. Örn: E.")]
    public KeyCode dashKey = KeyCode.E;


    // Duvardan geri sekme (Wall Bounce) ayarları
    [Header("Wall Bounce Settings")]
    [Tooltip("Wall Bounce için kullanılacak tuş (örneğin R).")]
    public KeyCode wallBounceKey = KeyCode.Space;
    [Tooltip("Duvardan sekme sırasında x ekseninde uygulanacak kuvvet.")]
    public float wallBounceHorizontalForce = 5f;
    [Tooltip("Duvardan sekme sırasında y ekseninde uygulanacak kuvvet.")]
    public float wallBounceVerticalForce = 5f;
    [Tooltip("Wall Bounce süresi.")]
    public float wallBounceDuration = 0.2f;
    [Tooltip("Wall Bounce yapabilmek için bekleme süresi.")]
    public float wallBounceCooldown = 1f;


    [Header("Wall Bounce Facing Cooldown")]
    public float wallBounceFacingCooldown = 0.5f;
    private float wallBounceFacingTimer = 0f;
    

    [Header("Bullet Deflect Settings")]
    public float bulletThrowForce = 10f;


    [Header("Combo Settings")]
    public float comboTimeout = 1f; // Komboyu devam ettirmek için max bekleme süresi

    
    [Header("Throw Projectile Settings")]
    public GameObject projectilePrefab; // Fırlatılacak prefab
    public Transform firePoint; // Fırlatma noktası (örneğin karakterin elinin bulunduğu nokta)
    public float projectileSpeed = 10f; // Fırlatma hızı
    public float fireCooldown = 1f; // Ateş etme arası bekleme süresi
    public int projectileAmount = 5; // Ateş etme arası bekleme süresi
    private float fireTimer = 0f;
    public KeyCode FireProjectileeKey = KeyCode.T;


    [Header("Time Slowdown Settings")]
    public float slowFactor = 0.2f;
    public float slowDuration = 0.5f; // Yavaşlatmanın süresi (saniye cinsinden, zaman ölçeğinden bağımsız olarak real time)


    [Header("Camera Shake")]
    public float dashCamShake;
    public float dashCamShakeDuration;
    public float hitCamShake;
    public float hitCamShakeDuration;
    public float perryCamShake;
    public float perryCamShakeDuration;
    public float killCamShake;
    public float killCamShakeDuration;

    
    [Header("Particle Effects")]
    public GameObject dashParticle;
    public GameObject deflectBulletParticle;


    private float comboTimer = 0f;
    private bool inCombo = false;
    private int lastAttackType = 0;
    private Rigidbody2D _rb;
    private Animator _anim;

    // Hareket ve durum değişkenleri
    private float _horizontalInput;
    private bool _isRunning;
    private bool _isGrounded;

    // Attack variables
    private bool _isAttacking;
    private bool canAttack;
    private Coroutine attackCooldownCoroutine;

    // Duvarla ilgili durumlar
    private bool _isTouchingWall;
    private bool _isWallSliding;

    // Dash ile ilgili durum değişkenleri
    private bool _isDashing;
    private float _dashTimeLeft;      // Kalan dash süresi
    private float _lastDashTime;      // Son dash yapılan zaman

    // Wall Bounce durum değişkenleri
    private bool _isWallBouncing;
    private float _wallBounceTimeLeft;
    private float _lastWallBounceTime;

    public bool attacking;


    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
    }


    void Start()
    {
        canAttack = true;
    }


    void Update()
    {
        if (didDie)
            return;

        if(dashParticle)
        {
            if (_isDashing && !dashParticle.activeInHierarchy)
            {
                dashParticle.SetActive(true);
            }
            else if (_isDashing == false && dashParticle.activeInHierarchy)
            {
                dashParticle.SetActive(false);
            }
        }


        // Yatay girdi (A/D veya Sol/Sağ ok tuşları)
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        // Koşma tuşu
        _isRunning = Input.GetKey(KeyCode.LeftShift);

        // Zıplama
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if ((!hasJumped) && (_isGrounded || coyoteTimeCounter > 0f))
            {
                Jump();
                hasJumped = true;
                coyoteTimeCounter = 0f;
            }
        }


        if (Input.GetKeyDown(KeyCode.P))
        {
            Die();
        }
        

        // Saldırı
        // Kombo saldırı denemesi
        if (Input.GetKeyDown(KeyCode.F) && canAttack)
        {
            AttemptComboAttack();
        }

        // Kombo süresi takibi
        if (comboTimer > 0f)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
            {
                // Süre bitti, kombo sıfırlanır
                inCombo = false;
                lastAttackType = 0;
            }
        }

        if (_isGrounded)
        {
            if (jumpResetTimer <= 0f)
            {
                coyoteTimeCounter = coyoteTime;
                hasJumped = false;  // Karakter yere tamamen iniş yaptıktan sonra zıplamaya yeniden izin ver.
            }
        }else
        {
            coyoteTimeCounter -= Time.deltaTime;
            if (jumpResetTimer > 0f)
                jumpResetTimer -= Time.deltaTime;
        }


        // Dash giriş (E tuşu)
        if (Input.GetKeyDown(dashKey))
        {
            // Cooldown geçmiş mi, şu an dash yapmıyor muyuz?
            if (!_isDashing && Time.time >= _lastDashTime + dashCooldown)
            {
                StartDash();
            }
        }

        // Eğer dash halindeysek kalan süreyi düşürüyoruz
        if (_isDashing)
        {
            _dashTimeLeft -= Time.deltaTime;
            if (_dashTimeLeft <= 0f)
            {
                StopDash();
            }
        }

        // DUVARDAN GERİ SEKME (Wall Bounce) girişleri
        // Sadece duvara değiyorken (wallCheck) wall bounce yapılabilir
        if (Input.GetKeyDown(wallBounceKey) && (_isTouchingWall || wallBounceFacingTimer > 0f))
        {
            if (!_isWallBouncing && Time.time >= _lastWallBounceTime + wallBounceCooldown)
                if(_isTouchingWall)
                {
                    StartWallBounce();
                }else
                {
                    StartWallBounce(-1);
                }
        }

        if (_isWallBouncing)
        {
            _wallBounceTimeLeft -= Time.deltaTime;
            if (_wallBounceTimeLeft <= 0f)
                StopWallBounce();
        }

        if (wallBounceFacingTimer > 0f)
        {
            wallBounceFacingTimer -= Time.deltaTime;
        }


        if(Input.GetKeyDown(FireProjectileeKey) && fireTimer <= 0f && projectileAmount > 0)
        {
            FireProjectile();
            fireTimer = fireCooldown; // Ateş ettikten sonra cooldown süresini ayarla.
            projectileAmount--;
        }

        if (fireTimer > 0f)
        fireTimer -= Time.deltaTime;

        UpdateAnimator();
    }


    void FixedUpdate()
    {
        if(didDie)
            return;

        // Zeminde miyiz kontrolü
        CheckGround();

        // Duvar kontrolü
        CheckWall();

        if(_isDashing)
        {
            HandleStep(stepRayDistance * 2);
        }

        // Eğer dash, wall bounce veya saldırı durumunda normal hareket iptal edilsin
        if (_isDashing || _isWallBouncing)
            return;

        // Duvardaysak duvar slide hareketi
        if (_isWallSliding && !_isGrounded)
        {
            WallSlideMovement();
        }else
        {
            // Normal yatay hareket
            float currentSpeed = _isRunning ? runSpeed : walkSpeed;
            Vector2 velocity = _rb.linearVelocity;
            
            velocity.x = _horizontalInput * currentSpeed;
            _rb.linearVelocity = velocity;
        }

        HandleStep(stepRayDistance);

        // Yön değiştirme (Sprite flip)
        if (_horizontalInput > 0 && transform.localScale.x < 0)
        {
            Flip();
        }else if (_horizontalInput < 0 && transform.localScale.x > 0)
        {
            Flip();
        }
    }


    #region Handle Step

    private void HandleStep(float _stepRayDistance)
    {
        // Yalnızca karakter hareket ediyorsa veya dash atiyorsa step kontrolü yapalım:
        if (Mathf.Abs(_horizontalInput) < 0.1f && !_isDashing)
            return;
        
        // BoxCollider2D bileşenini alalım:
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box == null) return;
        
        // Karakterin hareket ettiği yönde, collider'ın alt sağ veya alt sol köşesinden raycast başlatıyoruz.
        float direction = Mathf.Sign(_horizontalInput);
        // Origin: collider'ın alt kenarının, hareket yönünde olan köşesi (biraz içeri offset ekleyerek)
        Vector2 origin = new Vector2(box.bounds.center.x + direction * box.bounds.extents.x, box.bounds.min.y + 0.01f);
        
        // Alt köşeden, hareket yönünde bir raycast yapıyoruz:
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * direction, _stepRayDistance, groundLayer);
        
        // Eğer engel algılanırsa, üstteki boşluğu kontrol etmek için origin'i stepHeight kadar yukarı kaydırıp raycast yapalım:
        if (hit)
        {
            Vector2 upperOrigin = new Vector2(origin.x, origin.y + stepHeight);
            RaycastHit2D upperHit = Physics2D.Raycast(upperOrigin, Vector2.right * direction, _stepRayDistance, groundLayer);
            
            // Eğer üstte engel yoksa, karakteri adım atacak şekilde yukarı taşıyalım:
            if (upperHit.collider == null)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y + stepHeight, transform.position.z);
            }
        }
        
        // (Opsiyonel:) Debug çizgileri ile raycastleri görebilirsiniz:
        Debug.DrawRay(origin, Vector2.right * direction * _stepRayDistance, Color.red);
        Debug.DrawRay(new Vector2(origin.x, origin.y + stepHeight), Vector2.right * direction * _stepRayDistance, Color.green);
    }
    #endregion


    #region Wall Climb & Check Wall

    private void CheckWall()
    {
        float direction = transform.localScale.x;
        Vector2 checkPos = wallCheck.position;
        RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.right * direction, wallCheckDistance, wallLayer);

        _isTouchingWall = (!_isGrounded && hit.collider != null);
        _isWallSliding = _isTouchingWall;
    }


    private void WallSlideMovement()
    {
        Vector2 velocity = _rb.linearVelocity;

        if (velocity.y < -wallSlideSpeed)
        {
            velocity.y = -wallSlideSpeed;
        }
        
        _rb.linearVelocity = velocity;
    }
    #endregion


    #region Wall Bounce

    private void StartWallBounce(int direction = 1)
    {
        
        _isWallBouncing = true;
        _wallBounceTimeLeft = wallBounceDuration;
        _lastWallBounceTime = Time.time;

        // Karakter duvara değiyorsa, bakış yönünün tersine doğru wall bounce yaparız
        float bounceDirection = transform.localScale.x * -1f;
        _rb.linearVelocity = new Vector2(bounceDirection * direction * wallBounceHorizontalForce, wallBounceVerticalForce);
        if (_anim) _anim.SetTrigger("WallBounce");
    }


    private void StopWallBounce()
    {
        _isWallBouncing = false;
        Vector2 currentVel = _rb.linearVelocity;
        currentVel.x = 0f;
        _rb.linearVelocity = currentVel;
    }
    #endregion


    #region Dash

    private void StartDash()
    {
        _isDashing = true;
        _dashTimeLeft = dashDuration;
        _lastDashTime = Time.time;

        // Karakterin baktığı yönü alalım
        float dashDirection = Mathf.Sign(transform.localScale.x);

        // Karakteri yatay eksende hızla fırlat
        _rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);

        SoundManager.PlaySound(SoundManager.soundType.Dash);

        StartCoroutine(Camera.Shake(dashCamShakeDuration, dashCamShake));
    }


    private void StopDash()
    {
        _isDashing = false;
        
        // Su anlik bu kismin bir etkisi yok ileride bir degisiklik olursa diye ekledim
        Vector2 currentVel = _rb.linearVelocity;
        currentVel.x = 0f;
        _rb.linearVelocity = currentVel;
    }
    #endregion


    #region  Jump & Ground Check

    private void Jump()
    {
        Vector2 velocity = _rb.linearVelocity;
        velocity.y = jumpForce;
        _rb.linearVelocity = velocity;

        if (_anim)
            _anim.SetTrigger("JumpUp"); // Yukarı zıplama animasyonu

        jumpResetTimer = jumpResetDelay;  // Zıpladıktan sonra belirli süre boyunca resetlenmemesi için timer başlatılır.
        hasJumped = true;
    }


    private void CheckGround()
    {
        Collider2D hit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        _isGrounded = hit != null;
    }
    #endregion


    #region Flip

    private void Flip()
    {
        if (_isTouchingWall)
        {
            wallBounceFacingTimer = wallBounceFacingCooldown;
        }
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    #endregion


    #region Attack-Combo

    private void AttemptComboAttack()
    {
        // Eğer komboda değilsek veya süre dolduysa ilk saldırı (1) ile başla
        if (!inCombo || comboTimer <= 0f)
        {
            lastAttackType = 1;
            inCombo = true;
            StartAttack(1);
        }
        else
        {
            // Önceki saldırıyla aynı olmayan rastgele bir saldırı tipi seç (1,2,3)
            int nextAttack = lastAttackType;
            while (nextAttack == lastAttackType)
            {
                nextAttack = Random.Range(1, 4); // 1, 2 veya 3
            }
            lastAttackType = nextAttack;
            StartAttack(nextAttack);
        }

        // Kombo süresini yenile
        comboTimer = comboTimeout;
    }


    private void StartAttack(int attackType)
    {
        canAttack = false;
        _isAttacking = true;

        // Hangi saldırı tipiyse, Animator'da ilgili trigger'ı tetikle
        if (_anim)
        {
            // Aynı karede tetiklenme karışmasın diye önce resetliyoruz
            _anim.ResetTrigger("Attack1");
            _anim.ResetTrigger("Attack2");
            _anim.ResetTrigger("Attack3");

            if (attackType == 1)
            {
                _anim.SetTrigger("Attack1");

                SoundManager.PlaySound(SoundManager.soundType.Attack1, 1f);

            }else if (attackType == 2)
            {
                _anim.SetTrigger("Attack2");

                SoundManager.PlaySound(SoundManager.soundType.Attack2, 1f);
            }
            else if (attackType == 3)
            {
                _anim.SetTrigger("Attack3");

                SoundManager.PlaySound(SoundManager.soundType.Attack3, 1f);
            }
        }
        

        // Mevcut saldırı mantığı (hasar verme vb.)
        PerformAttack();
        StartCoroutine(ResetAttack(attackDuration, attackCooldown));
    }


    private void PerformAttack()
    {
        Debug.Log("Attack");

        // Attack alanındaki tüm objeleri alıyoruz (layer filtresi uygulamıyoruz ki hem enemy hem de bullet kontrol edilebilsin)
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, attackRange);
        foreach (Collider2D obj in hitObjects)
        {
            // Eğer objenin layer'ı "bullet" ise:
            if (obj.gameObject.layer == LayerMask.NameToLayer("BULLET"))
            {
                if (obj.TryGetComponent(out Rigidbody2D bulletRb))
                {
                    DeflectBullet(bulletRb);

                    ActivateSlowTime();
                }
            }
            else if(obj.TryGetComponent(out NPCBase enemyScript))
            {
                // NPC 1 ise attack yapiyor mu kontrolu lazim
                if (enemyScript.TryGetComponent(out NPC1Controller npc1Controller))
                {
                    bool isNPCAttacking = npc1Controller.isAttacking;
                    bool isNPCFaking = npc1Controller.isFakeAttack;

                    // attack gercek ise perryle, degilse normal vurus yap
                    if(isNPCAttacking && !isNPCFaking)
                    {
                        PerryMeleeAttack();
                    }else
                    {
                        enemyScript.TakeDamage(50f);
                        StartCoroutine(Camera.Shake(hitCamShakeDuration, hitCamShake));
                    }
                }else // NPC 2 ise zaten perryleme olmayacagi icin normal vurus yap
                {
                    enemyScript.TakeDamage(50f);
                    StartCoroutine(Camera.Shake(hitCamShakeDuration, hitCamShake));
                }

                SoundManager.PlaySound(SoundManager.soundType.HitEnemy, 0.6f);

                ActivateSlowTime();
            }
        }
    }


    private IEnumerator ResetAttack(float attackDuration, float attackCooldown)
    {
        // İlk olarak attackDuration süresi kadar bekle (saldırı animasyonunun süresi)
        yield return new WaitForSecondsRealtime(attackDuration);
        _isAttacking = false;  // Saldırı animasyonu tamamlandıktan sonra attacking durumu sıfırlansın

        // Daha sonra attackCooldown süresi kadar bekle (oyuncunun yeniden saldırıya geçebilmesi için)
        yield return new WaitForSecondsRealtime(attackCooldown);
        canAttack = true;
    }
    #endregion


    #region Perry-Deflect Attack

    private void PerryMeleeAttack()
    {
        Debug.Log("Perry Melee");

        if(attackCooldownCoroutine != null) StopCoroutine(attackCooldownCoroutine);
        attackCooldownCoroutine = StartCoroutine(ResetAttack(0, 0));

        StartCoroutine(Camera.Shake(perryCamShakeDuration, perryCamShake));

        SoundManager.PlaySound(SoundManager.soundType.Perry, 1f);
    }


    private void DeflectBullet(Rigidbody2D bulletRb)
    {
        Debug.Log("Deflect Bullet");

        // Karakterin aninda tekrar attack yapabilmesini sagla
        if(attackCooldownCoroutine != null) StopCoroutine(attackCooldownCoroutine);
        attackCooldownCoroutine = StartCoroutine(ResetAttack(0, 0));

        StartCoroutine(Camera.Shake(perryCamShakeDuration, perryCamShake));

        SoundManager.PlaySound(SoundManager.soundType.DeflectBulet, 0.6f);

        // Karakterin facing yönünü al (örneğin, sağa bakıyorsa +1, sola -1)
        float facing = Mathf.Sign(transform.localScale.x);
        // x bileşeni kesinlikle karakterin tersine, y bileşeni hafif rastgele (örnek: -0.5 ile 0.5 arası)
        Vector2 throwDirection = new Vector2(facing, Random.Range(-0.5f, 0.5f)).normalized;
        bulletRb.AddForce(throwDirection * bulletThrowForce, ForceMode2D.Impulse);

        if(deflectBulletParticle) Instantiate(deflectBulletParticle, bulletRb.position, Quaternion.identity);
    }
    #endregion


    #region Throw Projectile

    public void FireProjectile()
    {
        // Prefab'i firePoint konumunda instantiate ediyoruz.
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        
        // Karakterin baktığı yönü belirleyelim (örneğin, scale.x pozitifse sağ, negatifse sol)
        float direction = Mathf.Sign(transform.localScale.x);
        
        // Eğer projectile'da Rigidbody2D varsa, ona hız atayarak fırlatalım:
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if(rb != null)
        {
            rb.linearVelocity = new Vector2(direction * projectileSpeed, 0f);
        }
    }
    #endregion


    #region Animations

    private void UpdateAnimator()
    {
        if (_anim == null)
            return;

        _anim.SetFloat("Speed", Mathf.Abs(_rb.linearVelocity.x));
        _anim.SetFloat("YAxisSpeed", Mathf.Abs(_rb.linearVelocity.y));
        _anim.SetBool("IsWallSlidingDown", _isWallSliding);
        _anim.SetBool("IsDashing", _isDashing);
        _anim.SetBool("IsFalling", !_isGrounded && _rb.linearVelocity.y < 0);
    }
    #endregion


    #region Death

    public void Die()
    {
        if(didDie)
            return;

        Debug.Log("Died");
        
        didDie = true;
        _rb.linearVelocity = Vector2.zero;

        SoundManager.PlaySound(SoundManager.soundType.Death, 0.8f);
        
        if(_anim != null)
        {
            _anim.SetTrigger("Die");
        }
    }
    #endregion


    #region Time Slowdown

    public void ActivateSlowTime()
    {
        StartCoroutine(SlowTimeCoroutine());
    }

    private IEnumerator SlowTimeCoroutine()
    {
        // Zamanı yavaşlat
        Time.timeScale = slowFactor;
        // FixedUpdate için zaman adımını da yavaşlatmak önemlidir
        Time.fixedDeltaTime = 0.02f * slowFactor;

        // Belirtilen süre kadar bekle (real time, yani zaman ölçeğinden etkilenmez)
        yield return new WaitForSecondsRealtime(slowDuration);

        // Zamanı tekrar normale döndür
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
    #endregion


    #region  Gizmos Debug
    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.grey;
            float direction = (transform != null) ? transform.localScale.x : 1f;
            Vector2 startPos = wallCheck.position;
            Vector2 endPos = startPos + direction * wallCheckDistance * Vector2.right;
            Gizmos.DrawLine(startPos, endPos);
        }
    }
    #endregion
}
