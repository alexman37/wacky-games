using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Rolls dice for either a dice based game or for determining round order in a 1 on 1 gamemode.
/// Generally only 1 dice is rolled for determining order. If totalDice > 1, we roll multiple dice using 
/// the non singular verisons of the methods.
/// </summary>
public class DiceManager : MonoBehaviour
{
    public static DiceManager Instance { get; private set; }
    public GameObject dicePrefab;
    public int totalDice = 1;
    GameObject createdDice;
    List<GameObject> diceList = new List<GameObject>();
    bool isRolling = false;
    public Vector3[] resultValues;

    // Start is called once before the first execution of Update after the MonoBehaviour is created  
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            if (totalDice == 1)
            {
                ResetDiceSingular();
            }
            else
            {
                ResetDice();
            }                
        }
        else
        {
            Destroy(this);
        }
    }

    // Update is called once per frame  
    void Update()
    {
        if (isRolling)
        {
            if (createdDice != null && createdDice.transform.position.y < -10)
            {
                ResetDiceSingular();
            }
            if (createdDice != null)
            {
                createdDice.TryGetComponent<Rigidbody>(out Rigidbody rb);
                if (rb != null && rb.IsSleeping())
                {
                    Debug.Log("Dice has settled.");
                    isRolling = false;
                    int faceValue = DiceValueCalculator.GetTopFaceValue(createdDice);
                    Debug.Log("Dice settled with top face: " + faceValue);
                }
            }
        }       
    }

    public void RollDiceSingular()
    {
        Rigidbody rb = createdDice.GetComponent<Rigidbody>();
        rb.AddForce(new Vector3(Random.Range(-5, 5), Random.Range(10, 15), Random.Range(-5, 5)), ForceMode.Impulse);
        rb.AddTorque(new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), Random.Range(-10, 10)), ForceMode.Impulse);
        isRolling = true;
    }

    public void RollDice()
    {
        foreach (GameObject dice in diceList)
        {
            Rigidbody rb = dice.GetComponent<Rigidbody>();
            rb.AddForce(new Vector3(Random.Range(-5, 5), Random.Range(10, 15), Random.Range(-5, 5)), ForceMode.Impulse);
            rb.AddTorque(new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), Random.Range(-10, 10)), ForceMode.Impulse);
        }
        isRolling = true;
    }

    public void ResetDiceSingular()
    {
        isRolling = false;
        if(createdDice != null) Destroy(createdDice);
        int x = Random.Range(-2, 2);
        int y = Random.Range(-2, 2);
        int z = Random.Range(-2, 2);
        createdDice = Instantiate(dicePrefab, new Vector3(0, 1, 0), Quaternion.Euler(x * 90, y * 90, z * 90));
    }

    public void ResetDice()
    {
        isRolling = false;
        foreach (GameObject dice in diceList)
        {
            Destroy(dice);
        }
        diceList.Clear();
        for (int i = 0; i < totalDice; i++)
        {
            int x = Random.Range(-2, 2);
            int y = Random.Range(-2, 2);
            int z = Random.Range(-2, 2);
            diceList.Add(Instantiate(dicePrefab, new Vector3(x * y, 1, x * z), Quaternion.Euler(x * 90, y * 90, z * 90)));
        }
    }
}
