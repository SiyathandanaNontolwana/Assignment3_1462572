using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum GameState { START, PLAYER, ENEMY, SECONDENEMY, DISARMED , WIN, LOSS }

public class BattleSystem : MonoBehaviour
{

    public GameState currentState;

    public GameObject playerPrefab, enemyPrefab, enemySecondPrefab;

    //ON SCREEN INDICATOR
    public GameObject playerTurnText, enemyTurnText, enemyDisarmedText;
    //

    //PLAYER BUTTONS
    public GameObject playerHealButton;
    public GameObject playerAttackButton;
    public GameObject playerSpecialAttackButton;
    public GameObject playerDisarmButton;

    //Screen effects
    private ScreenShake shake;
    public GameObject enemyDeathEffect;
    public GameObject playerDeathEffect;
    public GameObject playerHealEffect;
    public GameObject enemyHealEffect;
    //Screen effects ended

    Unit playerUnit, enemyUnit, enemySecondUnit;

    public BattleHUD playerHUD, enemyHUD, enemySecondHUD;

    //RANDOMIZER VARIABLES
    private bool critHit = false;
    private bool disArmHit = false;

    void Update()
    {

        //Close game
        if (Input.GetButton("Cancel"))
        {
            Debug.Log("game closed");
            Application.Quit();
        }

        if(currentState == GameState.PLAYER)
        {
            disArmHit = false;
            playerTurnText.SetActive(true);
            enemyTurnText.SetActive(false);
            enemyDisarmedText.SetActive(false);
        }
        else if (currentState == GameState.ENEMY || currentState == GameState.DISARMED)
        {
            playerTurnText.SetActive(false);
            enemyTurnText.SetActive(true);
        }

        if (disArmHit == true)
        {
            enemyDisarmedText.SetActive(true);
        }
        else if (disArmHit != true)
        {
            enemyDisarmedText.SetActive(false);
        }

        playerHealthTracker();
        enemyHealthTracker();
    }

    void Start()
    {
        currentState = GameState.START;
        StartCoroutine(SetUpBattle());

        shake = GameObject.FindGameObjectWithTag("ScreenShake").GetComponent<ScreenShake>();
    }


    // BATTLE SETUP AT THE START OF THE GAME
    IEnumerator SetUpBattle()
    {
        GameObject playerGameObject = Instantiate(playerPrefab, new Vector3(-6.5f, -2f, 0f), Quaternion.identity);
        playerUnit = playerGameObject.GetComponent<Unit>();

        GameObject enemyGameObject = Instantiate(enemyPrefab, new Vector3(6.5f, -2f, 0f), Quaternion.identity);
        enemyUnit = enemyGameObject.GetComponent<Unit>();

        GameObject enemySecondGameObject = Instantiate(enemySecondPrefab, new Vector3(3f, -2.3f, 0f), Quaternion.identity);
        enemySecondUnit = enemySecondGameObject.GetComponent<Unit>();


        playerHUD.SetHUD(playerUnit);
        enemyHUD.SetHUD(enemyUnit);
        enemySecondHUD.SetHUD(enemySecondUnit);

        yield return new WaitForSeconds(.5f);

        currentState = GameState.PLAYER;
        PlayerTurn();
    }

    //PLAYER TURN
    void PlayerTurn()
    {

    }

    //DURING PLAYER TURN. ATTACK OPTION THEY HAVE.
    IEnumerator PlayerAttack()
    {
        //Deal damage to enemy

        bool isDead = enemyUnit.TakeDamage(playerUnit.damage);
        bool isSecEnemyDead = enemySecondUnit.TakeDamage(playerUnit.damage - 1);

        shake.cameraShake();

        enemyHUD.setHP(enemyUnit.currentHP);
        enemySecondHUD.setHP(enemySecondUnit.currentHP);
        Debug.Log(enemyUnit.currentHP);
        yield return new WaitForSeconds(1f);

        //Check if enemy is dead
        if (isDead)
        {
            //End battle
            enemyUnit.gameObject.SetActive(false);
            Instantiate(enemyDeathEffect, new Vector3(6.5f, -2f, 0f), Quaternion.identity);

        }
        else
        {
            //Enemy turn
            currentState = GameState.ENEMY;
            StartCoroutine(EnemyTurn());
        }

        //check if second enemy is dead
        if (isSecEnemyDead)
        {
            //End battle
            enemySecondUnit.gameObject.SetActive(false);
            Instantiate(enemyDeathEffect, new Vector3(5.5f, -2f, 0f), Quaternion.identity);
        }
        else
        {
            //Enemy turn
            currentState = GameState.ENEMY;
            StartCoroutine(EnemyTurn());
        }

        if(isDead && isSecEnemyDead)
        {

            currentState = GameState.WIN;
            EndGame();
        }
    }

    //SPECIAL ATTACK OPTION
    IEnumerator PlayerSpecialAttack()
    {
        Randomizer(playerUnit.critAmount);

        if (critHit == true)
        {
            shake.cameraShake();
            enemyUnit.TakeDamage(playerUnit.damage + playerUnit.critModifier);
        }
        else if (critHit == false)
        {
            enemyUnit.TakeDamage(0);
        }

        //Deal damage to enemy
        bool isDead = enemyUnit.TakeDamage(0);

        enemyHUD.setHP(enemyUnit.currentHP);

        Debug.Log(enemyUnit.currentHP);
        yield return new WaitForSeconds(1f);

        //Check if enemy is dead
        if (isDead)
        {
            //End battle
            enemyUnit.gameObject.SetActive(false);
            Instantiate(enemyDeathEffect, new Vector3(6.5f, -2f, 0f), Quaternion.identity);
            currentState = GameState.WIN;
            EndGame();
        }
        else
        {
            //Enemy turn
            currentState = GameState.ENEMY;
            StartCoroutine(EnemyTurn());
        }
    }

    //PLAYER OPTION FOR HEAL
    IEnumerator PlayerHeal()
    {


        playerUnit.Heal(5);
        Instantiate(playerHealEffect, new Vector3(-6.5f, -1.75f, 0), Quaternion.identity);
        playerHUD.setHP(playerUnit.currentHP);

        if(enemySecondUnit.currentHP >= 0f)
        {
            enemySecondUnit.Heal(5);
            Instantiate(enemyHealEffect, new Vector3(3f, -2.5f, 0), Quaternion.identity);
            enemySecondHUD.setHP(enemySecondUnit.currentHP);
        }
        else
        {
            enemySecondUnit.Heal(0);
        }


        yield return new WaitForSeconds(1f);
        currentState = GameState.ENEMY;
        StartCoroutine(EnemyTurn());
    }

    //PLAYER DISARM OPTION
    IEnumerator PlayerDisArm()
    {
        DisarmRandomizer(playerUnit.disarmChance);

        if(disArmHit == true)
        {
            currentState = GameState.DISARMED;
            yield return new WaitForSeconds(1.25f);
            bool isDead = playerUnit.TakeDamage(0);
            //shake.cameraShake();

            currentState = GameState.PLAYER;
        }
        else if(disArmHit == false)
        {
            currentState = GameState.ENEMY;
            StartCoroutine(EnemyTurn());
        }

       
    }

    //ENEMY ACTION
    IEnumerator EnemyTurn()
    {

        yield return new WaitForSeconds(1f);

        bool isDead = playerUnit.TakeDamage(enemyUnit.damage);
        shake.cameraShake();

        playerHUD.setHP(playerUnit.currentHP);

        if (isDead)
        {
            playerUnit.gameObject.SetActive(false);
            Instantiate(playerDeathEffect, new Vector3(-6.5f, -2f, 0f), Quaternion.identity);
            currentState = GameState.LOSS;
            EndGame();
        }
        else
        {
            //CHECK IF SECOND ENEMY IS ALIVE
            if (enemySecondUnit.currentHP >= 0f)
            {
                currentState = GameState.SECONDENEMY;
                StartCoroutine(SecondEnemyTurn());
            }
            else
                currentState = GameState.PLAYER;

        }


    }

    //SECOND ENEMY ACTION

    IEnumerator SecondEnemyTurn()
    {

        yield return new WaitForSeconds(1f);

        bool isDead = playerUnit.TakeDamage(enemySecondUnit.damage);
        shake.cameraShake();

        playerHUD.setHP(playerUnit.currentHP);

        if (isDead)
        {
            playerUnit.gameObject.SetActive(false);
            Instantiate(playerDeathEffect, new Vector3(-6.5f, -2f, 0f), Quaternion.identity);
            currentState = GameState.LOSS;
            EndGame();
        }
        else
        {
            currentState = GameState.PLAYER;
            PlayerTurn();
        }


    }

    //PLAYER BUTTONS

    public void AttackButton()
    {
        if (currentState != GameState.PLAYER)
        {
            return;
        }
        else
            StartCoroutine(PlayerAttack());
    }

    public void HealButton()
    {
        if (currentState != GameState.PLAYER)
        {
            return;
        }
        else
            StartCoroutine(PlayerHeal());
    }

    public void SpecialAttackButton()
    {
        if (currentState != GameState.PLAYER)
        {
            return;
        }
        else
            StartCoroutine(PlayerSpecialAttack());
    }

    public void DisarmButton()
    {
        if (currentState != GameState.PLAYER)
        {
            return;
        }
        else
            StartCoroutine(PlayerDisArm());
    }

    //RANDONMIZER FUNCTIONS

    private bool Randomizer(int randomNum)
    {
        randomNum = (int)(Random.Range(1f, 20f));
        if (randomNum >= 10f)
        {
            critHit = true;
            return (true);
        }
        else
            critHit = false;
        return (false);
    }

    private bool DisarmRandomizer(int disarmChance)
    {
        disarmChance = (int)(Random.Range(1f, 20f) + 2);
        if(disarmChance >= 10f)
        {
            //DISARM ENEMY
            disArmHit = true;
            return true;
        }
        disArmHit = false;
        return (false);
    }

    //HEALTH TRACKER FUNCTIONS

    private void enemyHealthTracker()
    {
        int currentEnemyHealth = enemyUnit.currentHP + enemySecondUnit.currentHP;
        int collectiveEnemyMaxHP = enemyUnit.maxHP + enemySecondUnit.maxHP;

        if(currentEnemyHealth <= collectiveEnemyMaxHP - 20)
        {
            playerDisarmButton.SetActive(true);
        }
    }

    private void playerHealthTracker()
    {
        int playerCurrentHealth = playerUnit.currentHP;
        int playerMaxHealth = playerUnit.maxHP;

        if(playerCurrentHealth <= playerMaxHealth/2)
        {
            playerHealButton.SetActive(true);
        }
    }

    //WIN OR LOSE CONDITION
    void EndGame()
    {
        if (currentState == GameState.WIN)
        {
            Debug.Log("You Win");

        }
        else if (currentState == GameState.LOSS)
        {
            Debug.Log("You Lost");

        }

    }
}
