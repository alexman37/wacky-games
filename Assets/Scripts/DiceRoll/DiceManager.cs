using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiceManager : MonoBehaviour
{
    public static DiceManager Instance { get; private set; }
    public GameObject dicePrefab;
    public int totalDice = 1;
    List<GameObject> diceList = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created  
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            ResetDice();
        }
        else
        {
            Destroy(this);
        }
    }

    // Update is called once per frame  
    void Update()
    {
        
    }

    public void RollDice()
    {
        foreach (GameObject dice in diceList)
        {
            Rigidbody rb = dice.GetComponent<Rigidbody>();
            rb.AddForce(new Vector3(Random.Range(-5, 5), Random.Range(10, 15), Random.Range(-5, 5)), ForceMode.Impulse);
            rb.AddTorque(new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), Random.Range(-10, 10)), ForceMode.Impulse);
        }
    }

    public void ResetDice()
    {
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
