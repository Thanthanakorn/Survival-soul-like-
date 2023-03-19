using UnityEngine;

public class AttackState : State
{
    public CombatStanceState combatStanceState;
    
    public EnemyAttackAction[] enemyAttacks;
    public EnemyAttackAction currentAttack;
    private static readonly int Vertical = Animator.StringToHash("Vertical");
    private static readonly int Horizontal = Animator.StringToHash("Horizontal");

    public override State Tick(EnemyManager enemyManager, EnemyStats enemyStats,
        EnemyAnimatorManager enemyAnimatorManager)
    {
        //var transform1 = transform;
        //Vector3 targetDirection = enemyManager.currentTarget.transform.position - transform1.position;
        //float viewAbleAngle = Vector3.Angle(targetDirection, transform1.forward);
        if (enemyManager.isPerformingAction)
            return combatStanceState;
        
        if (currentAttack != null)
        {
            //If we are too close to the enemy to perform current attack, get a new attack
            if (enemyManager.distanceFromTarget < currentAttack.minimumDistanceNeededToAttack)
            {
                return this;
            }
            if (enemyManager.distanceFromTarget < currentAttack.maximumDistanceNeededToAttack)
            {
                //If out enemy is within uor attacks viewable angle, we attack
                if (enemyManager.viewAbleAngle <= currentAttack.maximumAttackAngle &&
                    enemyManager.viewAbleAngle >= currentAttack.minimumAttackAngle)
                {
                    if (enemyManager.currentRecoveryTime <= 0 && enemyManager.isPerformingAction == false)
                    {
                        enemyAnimatorManager.animator.SetFloat(Vertical, 0, 0.1f, Time.deltaTime);
                        enemyAnimatorManager.animator.SetFloat(Horizontal, 0,0.1f,Time.deltaTime);
                        enemyAnimatorManager.PlayTargetAnimation(currentAttack.actionAnimation, true);
                        enemyManager.isPerformingAction = true;
                        enemyManager.currentRecoveryTime = currentAttack.recoveryTime;
                        currentAttack = null;
                        return combatStanceState;
                    }
                }
            }
        }
        else
        {
            GetNewAttack(enemyManager);
        }
        return combatStanceState;
    }

    private void GetNewAttack(EnemyManager enemyManager)
    {
        var currentTargetPosition = enemyManager.currentTarget.transform.position;
        var position = transform.position;
        var targetDirection = currentTargetPosition - position;
        var viewableAngle = Vector3.Angle(targetDirection, transform.forward);
        enemyManager.distanceFromTarget = Vector3.Distance(currentTargetPosition, position);

        var maxScore = 0;

        for (int i = 0; i < enemyAttacks.Length; i++)
        {
            var enemyAttackAction = enemyAttacks[i];
            if (enemyManager.distanceFromTarget <= enemyAttackAction.maximumDistanceNeededToAttack
                && enemyManager.distanceFromTarget >= enemyAttackAction.minimumDistanceNeededToAttack)
            {
                if (viewableAngle <= enemyAttackAction.maximumAttackAngle
                    && viewableAngle >= enemyAttackAction.minimumAttackAngle)
                {
                    maxScore += enemyAttackAction.attackScore;
                }
            }
        }

        var randomValue = Random.Range(0, maxScore);
        var temporaryScore = 0;

        for (int i = 0; i < enemyAttacks.Length; i++)
        {
            var enemyAttackAction = enemyAttacks[i];
            if (enemyManager.distanceFromTarget <= enemyAttackAction.maximumDistanceNeededToAttack
                && enemyManager.distanceFromTarget >= enemyAttackAction.minimumDistanceNeededToAttack)
            {
                if (viewableAngle <= enemyAttackAction.maximumAttackAngle
                    && viewableAngle >= enemyAttackAction.minimumAttackAngle)
                {
                    if (currentAttack != null) return;

                    temporaryScore += enemyAttackAction.attackScore;

                    if (temporaryScore > randomValue)
                    {
                        currentAttack = enemyAttackAction;
                    }
                }
            }
        }

    }
}
