using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InstructionsManager : MonoBehaviour
{

    public List<Sprite> instructions;
    public int currentInstruction = 0;
    public Image curInstructionSprite;

    public Button nextButton;
    public Button prevButton;
    public Button skipButton;

    public Text skipText;

    // Start is called before the first frame update
    void Start()
    {
        nextButton.onClick.AddListener(nextAction);
        prevButton.onClick.AddListener(prevAction);
        skipButton.onClick.AddListener(skipAction);
    }

    void nextAction()
    {
        if (currentInstruction < 6)
        {
            currentInstruction++;
            curInstructionSprite.sprite = instructions[currentInstruction];
        }
        if (currentInstruction == 6)
        {
            skipText.text = "DONE";
        }
    }

    void prevAction()
    {
        if (currentInstruction > 0)
        {
            currentInstruction--;
            curInstructionSprite.sprite = instructions[currentInstruction];
        }
        skipText.text = "SKIP";
    }

    void skipAction()
    {
        SceneManager.LoadScene("Home");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
