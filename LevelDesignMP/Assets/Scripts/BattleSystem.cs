using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum GameState { START, PLAYER, ENEMY, WIN, LOSS }

public class BattleSystem : MonoBehaviour
{

    public GameState currentState;

    public GameObject playerPrefab, enemyPrefab;

    //Screen effects
    private ScreenShake shake;
    public GameObject enemyDeathEffect;
    public GameObject playerDeathEffect;
    public GameObject playerHealEffect;
    //Screen effects ended

    Unit playerUnit, enemyUnit;

    public BattleHUD playerHUD, enemyHUD;

    private bool critHit = false;

    void Update()
    {

        //Close game
        if (Input.GetButton("Cancel"))
        {
            Debug.Log("game closed");
            Application.Quit();
        }
    }

    void Start()
    {
        currentState = GameState.START;
        StartCoroutine(SetUpBattle());

        shake = GameObject.FindGameObjectWithTag("ScreenShake").GetComponent<ScreenShake>();
    }



    IEnumerator SetUpBattle()
    {
        GameObject playerGameObject = Instantiate(playerPrefab, new Vector3(-6.5f, -2f, 0f), Quaternion.identity);
        playerUnit = playerGameObject.GetComponent<Unit>();

        GameObject enemyGameObject = Instantiate(enemyPrefab, new Vector3(6.5f, -2f, 0f), Quaternion.identity);
        enemyUnit = enemyGameObject.GetComponent<Unit>();

        playerHUD.SetHUD(playerUnit);
        enemyHUD.SetHUD(enemyUnit);

        yield return new WaitForSeconds(.5f);

        currentState = GameState.PLAYER;
        PlayerTurn();
    }

    void PlayerTurn()
    {

    }

    IEnumerator PlayerAttack()
    {
        //Deal damage to enemy

        bool isDead = enemyUnit.TakeDamage(playerUnit.damage);

        shake.cameraShake();

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

    IEnumerator PlayerHeal()
    {


        playerUnit.Heal(5);
        Instantiate(playerHealEffect, new Vector3(-6.5f, -1.75f, 0), Quaternion.identity);
        playerHUD.setHP(playerUnit.currentHP);

        yield return new WaitForSeconds(1f);
        currentState = GameState.ENEMY;
        StartCoroutine(EnemyTurn());
    }

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
            currentState = GameState.PLAYER;
            PlayerTurn();
        }


    }

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
}
