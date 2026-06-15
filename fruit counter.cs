using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class fruitcounter : MonoBehaviour
{
    // Start is called before the first frame update
    public TextMeshProUGUI fruitText;
    private int fruitCount;

    // Start is called before the first frame update
    void Start()
    {
        fruitCount = 0;
        UpdateFruitText();
    }

    // Update is called once per frame
    void UpdateFruitText()
    {
        fruitText.text = "Fruit : " + fruitCount.ToString();
    }

    public void showFruit(int amount)
    {
        fruitCount = amount;
        UpdateFruitText();
    }
}
