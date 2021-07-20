using System;
using System.Collections;
using Player;
using UnityEngine;

namespace Mobs
{
    public class MainMob : MonoBehaviour, IDamageable
    {
        [SerializeField] protected Animator mobAnimator;
        [SerializeField] protected Collider[] colliders;
        [SerializeField] protected GameObject mapMarkerPrefab;
        [SerializeField] protected int mobHP;
        [SerializeField] protected int mobDamage;

        private static Transform _target;
        private RectTransform _mapMarker;
        protected Vector3 TargetPos;
        protected Transform MobTransform;
        protected PlayerStatus PlayerStatusScript;

        internal float MobSpeed = 0.35f;
        protected float DamageTimeDelay;
        protected bool IsAttacking;

        public event Action<MobStats> OnApplyDamage;

        public int HP { get; set; }
        public bool IsAlive => HP > 0;

        private void OnEnable()
        {
            if (_mapMarker != null) _mapMarker.gameObject.SetActive(true);
        }
        
        public void ApplyDamage(int damage)
        {
            HP -= damage;
            
            if (IsAlive) return;
            
            foreach (var hitBox in colliders) hitBox.enabled = false;
            _mapMarker.gameObject.SetActive(false);

            Config.MobsKills++;

            StartCoroutine(Death(DeathType(damage)));
        }

        protected void MobAppeared()
        {
            MobTransform = transform;
            _target = MobSpawner.instance.target;
            PlayerStatusScript = _target.GetComponentInParent<PlayerStatus>();
            HP = mobHP;

            TargetPos = _target.position;
            TargetPos.y = MobTransform.position.y;

            _mapMarker = Instantiate(mapMarkerPrefab, UI.MapCircle).GetComponent<RectTransform>();
            _mapMarker.anchoredPosition = new Vector2((TargetPos - MobTransform.position).x, (TargetPos - MobTransform.position).z) * -80;
        }

        protected void MobMove(Vector3 moveDir, float closeDistance)
        {
            TargetPos = _target.position;
            TargetPos.y = MobTransform.position.y;
            MobTransform.LookAt(TargetPos, Vector3.up);

            //Vector3 moveDir = targetPos - mobTransform.position;

            if (MobSpeed > 0)
            {
                if (!IsAlive)
                {
                    MobSpeed -= Time.deltaTime;

                    if (IsAttacking) Attack(false);
                }
                else if (moveDir.magnitude < closeDistance)
                {
                    MobSpeed -= Time.deltaTime;

                    if (!IsAttacking) Attack(true);
                }

                MobTransform.Translate(Vector3.forward * Time.deltaTime * MobSpeed);
                _mapMarker.anchoredPosition = new Vector2(moveDir.x, moveDir.z) * -80;
            }

            if (!IsAttacking || !IsAlive) return;
            
            DamageTimeDelay -= Time.deltaTime;

            if (DamageTimeDelay <= 0)
            {
                PlayerStatusScript.ApplyDamage(mobDamage);
                DamageTimeDelay = 0.7f;
            }

            if (moveDir.magnitude > closeDistance * 1.3f) Attack(false);
        }

        public virtual IEnumerator Death(int deathType)
        {
            mobAnimator.SetTrigger("Death Trigger " + deathType);

            yield return new WaitForSeconds(2.7f);

            MobSpawner.instance.mobPool.AddFirst(gameObject);
            MobSpeed = 0.35f;
            HP = mobHP;

            foreach (var collider in colliders) collider.enabled = true;
            gameObject.SetActive(false);
        }

        private int DeathType(int damage) => damage / (float)mobHP >= 0.5f ? 2 : 1;

        protected virtual void Attack(bool start)
        {
            IsAttacking = start;
        }
    }

    public struct MobStats
    {
        private int HP;
    }
}
