using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMob : MonoBehaviour, IDamageable
{
    [SerializeField] protected Animator mobAnimator;
    [SerializeField] protected Collider[] colliders;
    [SerializeField] protected GameObject mapMarkerPrefab;
    [SerializeField] protected int mobHP;
    [SerializeField] protected int mobDamage;

    protected static Transform target;
    protected RectTransform mapMarker;
    protected Vector3 targetPos;
    protected Transform mobTransform;
    protected PlayerStatus playerStatusScript;

    internal float mobSpeed = 0.35f;
    protected float damageTimeDelay;
    protected bool isAttacking = false;

    public int HP { get; set; }
    public bool IsAlive => HP > 0;

    private void OnEnable()
    {
        if (mapMarker != null) mapMarker.gameObject.SetActive(true);
    }

    public void ApplyDamage(int damage)
    {
        HP -= damage;
        if (!IsAlive)
        {
            foreach (Collider hitBox in colliders) hitBox.enabled = false;
            mapMarker.gameObject.SetActive(false);

            Config.MobsKills++;

            StartCoroutine(Death(DeathType(damage)));
        }
    }

    protected void MobAppeared()
    {
        mobTransform = transform;
        target = MobSpawner.instance.target;
        playerStatusScript = target.GetComponentInParent<PlayerStatus>();
        HP = mobHP;

        targetPos = target.position;
        targetPos.y = mobTransform.position.y;

        mapMarker = Instantiate(mapMarkerPrefab, UI.MapCircle).GetComponent<RectTransform>();
        mapMarker.anchoredPosition = new Vector2((targetPos - mobTransform.position).x, (targetPos - mobTransform.position).z) * -80;
    }

    protected void MobMove(Vector3 moveDir, float closeDistance)
    {
        targetPos = target.position;
        targetPos.y = mobTransform.position.y;
        mobTransform.LookAt(targetPos, Vector3.up);

        //Vector3 moveDir = targetPos - mobTransform.position;

        if (mobSpeed > 0)
        {
            if (!IsAlive)
            {
                mobSpeed -= Time.deltaTime;

                if (isAttacking) Attack(false);
            }
            else if (moveDir.magnitude < closeDistance)
            {
                mobSpeed -= Time.deltaTime;

                if (!isAttacking) Attack(true);
            }

            mobTransform.Translate(Vector3.forward * Time.deltaTime * mobSpeed);
            mapMarker.anchoredPosition = new Vector2(moveDir.x, moveDir.z) * -80;
        }

        if (isAttacking && IsAlive)
        {
            damageTimeDelay -= Time.deltaTime;

            if (damageTimeDelay <= 0)
            {
                playerStatusScript.ApplyDamage(mobDamage);
                damageTimeDelay = 0.7f;
            }

            if (moveDir.magnitude > closeDistance * 1.3f) Attack(false);
        }
    }

    public virtual IEnumerator Death(int deathType)
    {
        mobAnimator.SetTrigger("Death Trigger " + deathType);

        yield return new WaitForSeconds(2.7f);

        MobSpawner.instance.mobPool.AddFirst(gameObject);
        mobSpeed = 0.35f;
        HP = mobHP;

        foreach (Collider collider in colliders) collider.enabled = true;
        gameObject.SetActive(false);
    }

    private int DeathType(int damage)
    {
        if (damage / (float)mobHP >= 0.5f)
        {
            return 2;
        }
        else
        {
            return 1;
        }
    }

    protected virtual void Attack(bool start)
    {
        isAttacking = start;
    }
}
