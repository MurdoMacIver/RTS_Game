using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System;
using Random = UnityEngine.Random;

public enum UnitState
{
    Idle,
    Move,
    MoveToResource,
    Gather,
    MoveToEnemy,
    Attack
}

public class Unit : Agent
{
    public event Action OnReset;

    #region Stats
    [Header("Stats")]
    public int team = 0;

    public UnitState state;

    public int curHp;
    public int maxHp;

    public int minAttackDamage;
    public int maxAttackDamage;

    public float attackRate;
    private float lastAttackTime;

    public float attackDistance;

    public float pathUpdateRate = 1.0f;
    private float lastPathUpdateTime;

    public int gatherAmount;
    public float gatherRate;
    private float lastGatherTime;

    public ResourceSource curResourceSource;
    private Unit curEnemyTarget;

    [Header("Components")]
    public GameObject selectionVisual;
    private NavMeshAgent navAgent;
    public UnitHealthBar healthBar;

    public Player player;

    // events
    [System.Serializable]
    public class StateChangeEvent : UnityEvent<UnitState> { }
    public StateChangeEvent onStateChange;
    #endregion

    void Start ()
    {
        // get the components
        navAgent = GetComponent<NavMeshAgent>();

        SetState(UnitState.Idle);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        /// 1. Agent's hp
        /// 2. closest enemy's position
        /// 3. agent's position
        /// 4. enemy's hp



        sensor.AddObservation(curHp/maxHp);
        sensor.AddObservation(curEnemyTarget ? curEnemyTarget.curHp / curEnemyTarget.maxHp : 1f);
        sensor.AddObservation(curEnemyTarget ? Vector3.Distance(curEnemyTarget.transform.position, transform.position)/100f : 1f);
        sensor.AddObservation(curResourceSource ? Vector3.Distance(curResourceSource.transform.position, transform.position) / 100f : 1f);
    }


    void SetState (UnitState toState)
    {
        state = toState;

        // calling the event
        if(onStateChange != null)
            onStateChange.Invoke(state);

        if(toState == UnitState.Idle)
        {
            navAgent.isStopped = true;
            navAgent.ResetPath();
        }
    }

    public override void Initialize()
    {
        navAgent = GetComponent<NavMeshAgent>();

        SetState(UnitState.Idle);
    }

    public override void OnEpisodeBegin()
    {
        
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        //Debug.Log("OnActionReceived", gameObject);
        int max_i = 0;
        float cur_max = vectorAction[0];
        for (int i = 0; i < vectorAction.Length; i++)
        {
            if (vectorAction[i] >= cur_max)
            {
                cur_max = vectorAction[i];
                max_i = i;
            }
        }

        switch (max_i)
        {
            case 0:
                {
                    //Debug.Log("Agent is attacking", gameObject);
                    if (state != UnitState.Attack)
                        SetState(UnitState.Attack);
                    break;
                }
            case 1:
                {
                    //Debug.Log("Agent is gathering", gameObject);
                    if (state != UnitState.Gather)
                        SetState(UnitState.Gather);
                    break;
                }
            case 2:
                {
                    //Debug.Log("Agent is idling", gameObject);
                    if (state != UnitState.Idle)
                        SetState(UnitState.Idle);
                    break;
                }
            case 3:
                {
                    //Debug.Log("Agent is moving", gameObject);
                    if (state != UnitState.Move)
                        SetState(UnitState.Move);
                    break;
                }
            case 4:
                {
                    //Debug.Log("Agent is moving to enemy", gameObject);
                    if (state != UnitState.MoveToEnemy)
                        SetState(UnitState.MoveToEnemy);
                    break;
                }
            case 5:
                {
                    //Debug.Log("Agent is moving to resource", gameObject);
                    if (state != UnitState.MoveToResource)
                        SetState(UnitState.MoveToResource);
                    break;
                }
        }
    }

    public override void Heuristic(float[] actionsOut)
    {
        
    }

    void Update()
    {
        switch (state)
        {
            case UnitState.Move:
                {
                    //Debug.Log("Agent is moving", gameObject);
                    MoveUpdate();
                    break;
                }
            case UnitState.MoveToResource:
                {
                    //Debug.Log("Agent is moving to resource", gameObject);
                    MoveToResourceUpdate();
                    break;
                }
            case UnitState.Gather:
                {
                    //Debug.Log("Agent is gathering", gameObject);
                    GatherUpdate();
                    break;
                }
            case UnitState.MoveToEnemy:
                {
                    //Debug.Log("Agent is moving to enemy", gameObject);
                    MoveToEnemyUpdate();
                    break;
                }
            case UnitState.Attack:
                {
                    //Debug.Log("Agent is attacking", gameObject);
                    AttackUpdate();
                    break;
                }
        }
    }

    // called every frame the 'Move' state is active
    void MoveUpdate ()
    {
        /*if(Vector3.Distance(transform.position, navAgent.destination) == 0.0f)
            SetState(UnitState.Idle);*/
    }

    // called every frame the 'MoveToResource' state is active
    void MoveToResourceUpdate ()
    {
        if (curResourceSource == null || curResourceSource.quantity <= 0)
        {
            GetComponent<UnitAI>().FindNewResource();
            return;
        }

        navAgent.isStopped = false;
        navAgent.SetDestination(curResourceSource.transform.position);

        /*if(curResourceSource == null)
        {
            SetState(UnitState.Idle);
            return;
        }

        if(Vector3.Distance(transform.position, navAgent.destination) == 0.0f)
            SetState(UnitState.Gather);*/
    }

    // called every frame the 'Gather' state is active
    void GatherUpdate ()
    {
        if(curResourceSource == null || curResourceSource.quantity <= 0)
        {
            //SetState(UnitState.Idle);
            GetComponent<UnitAI>().FindNewResource();
            return;
        }

        if (Vector3.Distance(transform.position, curResourceSource.transform.position) > 2f)
        {
            MoveToResourceUpdate();
            return;
        }

        LookAt(curResourceSource.transform.position);

        if(Time.time - lastGatherTime > gatherRate)
        {
            lastGatherTime = Time.time;
            if(curResourceSource.GatherResource(gatherAmount, player))
                AddReward(.1f);
        }
    }

    // called every frame the 'MoveToEnemy' state is active
    void MoveToEnemyUpdate ()
    {
        // if our target is dead, go idle
        if(curEnemyTarget == null)
        {
            //SetState(UnitState.Idle);
            curEnemyTarget = GetComponent<UnitAI>().CheckForNearbyEnemies();
            return;
        }

        if(Time.time - lastPathUpdateTime > pathUpdateRate)
        {
            lastPathUpdateTime = Time.time;
            navAgent.isStopped = false;
            navAgent.SetDestination(curEnemyTarget.transform.position);
        }

        /*if(Vector3.Distance(transform.position, curEnemyTarget.transform.position) <= attackDistance)
            SetState(UnitState.Attack);*/
    }

    // called every frame the 'Attack' state is active
    void AttackUpdate ()
    {
        // if our target is dead, go idle
        if(curEnemyTarget == null)
        {
            //SetState(UnitState.Idle);
            curEnemyTarget = GetComponent<UnitAI>().CheckForNearbyEnemies();
            return;
        }

        if (Vector3.Distance(transform.position, curEnemyTarget.transform.position) > 2f)
        {
            MoveToEnemyUpdate();
            return;
        }

        // if we're still moving, stop
        if (!navAgent.isStopped)
            navAgent.isStopped = true;

        // attack every 'attackRate' seconds
        if(Time.time - lastAttackTime > attackRate)
        {
            lastAttackTime = Time.time;
            curEnemyTarget.TakeDamage(Random.Range(minAttackDamage, maxAttackDamage + 1));

            /// --------------------------------- ML AGENTS REWARD
            AddReward(1f);
        }

        // look at the enemy
        LookAt(curEnemyTarget.transform.position);

        // if we're too far away, move towards the enemy
        if(Vector3.Distance(transform.position, curEnemyTarget.transform.position) > attackDistance)
            SetState(UnitState.MoveToEnemy);
    }

    // called when an enemy unit attacks us
    public void TakeDamage (int damage)
    {
        curHp -= damage;

        if(curHp <= 0)
            Die();

        healthBar.UpdateHealthBar(curHp, maxHp);

        /// --------------------------------- ML AGENTS REWARD
        //AddReward(-.1f);
    }

    // called when our health reaches 0
    void Die ()
    {
        player.units.Remove(this);

        GameManager.instance.UnitDeathCheck();
        /// --------------------------------- ML AGENTS REWARD
        AddReward(-2f);

        Destroy(gameObject);
    }

    // moves the unit to a specific position
    public void MoveToPosition (Vector3 pos)
    {
        SetState(UnitState.Move);

        navAgent.isStopped = false;
        navAgent.SetDestination(pos);
    }

    // move to a resource and begin to gather it
    public void GatherResource (ResourceSource resource, Vector3 pos)
    {
        curResourceSource = resource;
        SetState(UnitState.MoveToResource);

        navAgent.isStopped = false;
        navAgent.SetDestination(pos);
    }

    // move to an enemy unit and attack them
    public void AttackUnit (Unit target)
    {
        curEnemyTarget = target;
        SetState(UnitState.MoveToEnemy);
    }

    // toggles the selection ring around our feet
    public void ToggleSelectionVisual (bool selected)
    {
        if(selectionVisual != null)
            selectionVisual.SetActive(selected);
    }

    // rotate to face the given position
    void LookAt (Vector3 pos)
    {
        Vector3 dir = (pos - transform.position).normalized;
        float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, angle, 0);
    }
}