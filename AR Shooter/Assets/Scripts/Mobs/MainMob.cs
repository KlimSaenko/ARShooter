using System.Collections;
using Player;
using UnityEngine;

namespace Game.Mobs
{
    public class MainMob : MonoBehaviour, IDamageable
    {
        [Header("Common")]
        [SerializeField] private protected Animator mobAnimator;
        [SerializeField] private protected Collider[] colliders;
        [SerializeField] private protected GameObject mapMarkerPrefab;
        [SerializeField] private protected int mobHP;
        [SerializeField] private protected int mobDamage;

        private static Transform _target;
        private RectTransform _mapMarker;
        private protected Vector3 TargetPos;
        private protected PlayerStatus PlayerStatusScript;

        internal float MobSpeed = 0.35f;
        private protected float DamageTimeDelay;
        private protected bool IsAttacking;

        private protected Transform MobTransform;
        private protected virtual Transform _mobTransform => transform;
        private protected virtual Quaternion CurrentLookRotation => transform.rotation;

        private int _hp;
        public int HP
        {
            get => _hp;
            set
            {
                if (value < 0)
                    _hp = 0;
                else _hp = value;
            }
        }
        public bool IsAlive => HP > 0;

        private void OnEnable()
        {
            MobAppeared();
            if (_mapMarker != null) _mapMarker.gameObject.SetActive(true);

            OnMobActivated();
        }

        private protected virtual void OnMobActivated()
        {

        }

        private protected static readonly int getDamageId = Animator.StringToHash("GetDamage");
        private protected static readonly int blendId = Animator.StringToHash("Blend");
        private protected static readonly int moveBlendId = Animator.StringToHash("MoveBlend");
        private protected static readonly int deathId = Animator.StringToHash("Death");

        public virtual void ApplyDamage(int damage)
        {
            HP -= damage;
            
            if (IsAlive) return;
            
            foreach (var hitBox in colliders) hitBox.enabled = false;
            _mapMarker.gameObject.SetActive(false);

            Config.MobsKills++;

            StartCoroutine(Death(DeathType(damage)));
        }

        private protected void MobAppeared()
        {
            MobTransform = _mobTransform;
            HP = mobHP;
            //_target = MobSpawner.Instance.target;
            //PlayerStatusScript = _target.GetComponentInParent<PlayerStatus>();

            //TargetPos = _target.position;
            //TargetPos.y = MobTransform.position.y;

            //_mapMarker = Instantiate(mapMarkerPrefab, CommonUI.MapCircle).GetComponent<RectTransform>();
            //_mapMarker.anchoredPosition = new Vector2((TargetPos - MobTransform.position).x, (TargetPos - MobTransform.position).z) * -80;
        }

        private protected void MobMove(Vector3 moveDir, float closeDistance)
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

                MobTransform.Translate(MobSpeed * Time.deltaTime * Vector3.forward);
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

        private float _prevDeltaRotation;
        private protected float MobRotate(float lerp)
        {
            var newRotation = Quaternion.RotateTowards(MobTransform.rotation, CurrentLookRotation, 1000 * lerp * Time.deltaTime);
            var deltaRotation = Mathf.Lerp(_prevDeltaRotation, Quaternion.Angle(MobTransform.rotation, newRotation), 10 * Time.deltaTime);
            _prevDeltaRotation = deltaRotation;
            MobTransform.rotation = newRotation;

            return deltaRotation;
        }

        public virtual IEnumerator Death(int deathType)
        {
            mobAnimator.SetTrigger("Death Trigger " + deathType);

            yield return new WaitForSeconds(2.7f);

            MobSpawner.Instance.MobPool.AddFirst(gameObject);
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
