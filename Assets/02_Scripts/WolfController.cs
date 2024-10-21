using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class WolfController : MonoBehaviour, IDamagable
{
    public enum State
    {
        IDLE, ATTACK, DIE, WALKING, RUNNING
    }

    public State state = State.IDLE;
    [SerializeField] private float traceDist = 10.0f;
    [SerializeField] private float attackDist = 2.0f;
    public bool isDie = false;

    private Transform playerTr;
    private Transform monsterTr;
    private NavMeshAgent agent;
    private Animator animator;

    private readonly int hashIsWalking = Animator.StringToHash("IsWalking");
    private readonly int hashIsDetected = Animator.StringToHash("IsDetected");
    private readonly int hashIsRunning = Animator.StringToHash("IsRunning");
    private readonly int hashIsAttack = Animator.StringToHash("IsAttack");
    private readonly int hashHit = Animator.StringToHash("Hit");
    private readonly int hashDie = Animator.StringToHash("Die");

    private float distance;

    private float hp = 100.0f;
    private Vector3 targetPosition; // 몬스터가 이동할 랜덤한 좌표
    private bool battleMode = false;
    private float transitionDuration;
    private Vector3 randomDirection;
    private float moveInterval = 2.0f;  // 랜덤 방향을 설정하는 주기

    private const float IDLE_WAIT = 3;
    private const float WALKING_WAIT = 4;
    private const float RUNNING_WAIT = 2;
    private const float ATTACK_WAIT = 1;
    private const float ATTACK_DAMAGE = 30;
    public float playerHp = 100;

    void OnEnable()
    {
        StartCoroutine(CheckMonsterState());
        StartCoroutine(MonsterAction());
    }

    void Awake()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("PLAYER");
        playerTr = playerObj.GetComponent<Transform>();

        monsterTr = transform;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    IEnumerator CheckMonsterState()
    {
        while (!isDie)
        {
            if (state == State.DIE) yield break;

            distance = Vector3.Distance(monsterTr.position, playerTr.position);

            if (distance <= attackDist)
            {
                state = State.ATTACK;
                yield return new WaitForSeconds(ATTACK_WAIT);
                state = State.IDLE;

            }
            else if (distance <= traceDist)
            {
                animator.SetBool(hashIsWalking, false);
                animator.SetBool(hashIsDetected, true);
                LookAtPlayer();
                yield return new WaitForSeconds(RUNNING_WAIT);
                state = State.RUNNING;
            }
            else if (distance > traceDist && battleMode == true)
            {
                state = State.RUNNING;
            }
            else if ((state == State.IDLE || state == State.WALKING) && distance <= traceDist)
            {
                state = State.RUNNING;
            }
            else
            {
                if ((state == State.WALKING || state == State.ATTACK) && distance > traceDist)
                {
                    yield return new WaitForSeconds(IDLE_WAIT);
                    state = State.IDLE;
                }
                yield return new WaitForSeconds(IDLE_WAIT);

                if (battleMode == false)
                {
                    state = State.WALKING;
                }

            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    IEnumerator MonsterAction()
    {
        while (!isDie)
        {
            switch (state)
            {
                case State.IDLE:
                    agent.isStopped = true;
                    animator.SetBool(hashIsWalking, false);
                    animator.SetBool(hashIsDetected, false);
                    animator.SetBool(hashIsRunning, false);
                    animator.SetBool(hashIsAttack, false);
                    break;

                case State.WALKING:
                    agent.isStopped = false;

                    if (!animator.GetBool(hashIsWalking))
                        animator.SetBool(hashIsWalking, true);

                    if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                    {
                        targetPosition = GenerateRandomPosition();
                        agent.SetDestination(targetPosition);
                    }
                    break;

                case State.RUNNING:
                    agent.isStopped = false;
                    animator.SetBool(hashIsRunning, true);
                    animator.SetBool(hashIsDetected, false);
                    battleMode = true;
                    agent.SetDestination(playerTr.position);  // 플레이어 추적
                    break;

                case State.ATTACK:
                    agent.isStopped = true;
                    animator.SetBool(hashIsRunning, false);
                    animator.SetBool(hashIsWalking, false);
                    if (distance < attackDist)
                    {
                        animator.SetBool(hashIsAttack, true);
                        playerHp -= ATTACK_DAMAGE;
                    }
                    else
                    {
                        animator.SetBool(hashIsAttack, false);
                    }
                    break;

                case State.DIE:
                    isDie = true;
                    agent.isStopped = true;
                    animator.SetTrigger(hashDie);
                    GetComponent<CapsuleCollider>().enabled = false;
                    Invoke(nameof(ReturnPool), 3.0f);
                    break;
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    // 랜덤한 좌표를 생성하는 메서드 (IDLE 상태일 때 호출)
    Vector3 GenerateRandomPosition()
    {
        Vector3 randomDirection = new Vector3(
            Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f)
        ).normalized;
        float randomDistance = Random.Range(5f, 10f);
        return monsterTr.position + randomDirection * randomDistance;
    }

    void LookAtPlayer()
    {
        Vector3 directionToPlayer = playerTr.position - monsterTr.position;
        directionToPlayer.y = 0;
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
            monsterTr.rotation = Quaternion.Slerp(monsterTr.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    Vector3 GenerateRandomXZDirection()
    {
        Vector3 randomDirection = new Vector3(
            Random.Range(-10f, 10f),
            0,
            Random.Range(-10f, 10f)
        ).normalized;
        return randomDirection;
    }

    void ReturnPool()
    {
        hp = 100.0f;
        isDie = false;
        state = State.IDLE;
        GetComponent<CapsuleCollider>().enabled = true;
        this.gameObject.SetActive(false);
    }

    public void OnDamaged()
    {
        animator.SetTrigger(hashHit);
        hp -= 20.0f;
        if (hp <= 0.0f)
        {
            state = State.DIE;
        }
    }
}