using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using alphabeticalOrder;

public class alphabeticalOrderScript : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMAudio Audio;

    //buttons
    public Button[] buttons;
    private int numberOfPresses = 0;
    private List<Button> pressedButtonList = new List<Button>();
    private List<String> rightOrWrong = new List<string>();
    public Color[] textColors;

    //lighs
    public Renderer[] levelIndicators;
    public Material[] levelMaterials;

    //levelInfo
    public string[] level1Options;
    public string[] level2Options;
    public string[] level3Options;
    public string[] level4Options;
    public List<Button> levelOrdered = new List<Button>();
    private List<int> selectedIndices = new List<int>();

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    int stage = 0;
    private bool moduleSolved = false;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        foreach (Button button in buttons)
        {
            Button trueButton = button;
            button.selectable.OnInteract += delegate () { OnButtonPress(trueButton); return false; };
        }
    }

    void Start()
    {
        SelectLevelLabels();
        foreach(Renderer levelIndic in levelIndicators)
        {
            levelIndic.material = levelMaterials[0];
        }
        levelIndicators[0].material = levelMaterials[2];
    }

    void SelectLevelLabels()
    {
        foreach(Button button in buttons)
        {
            if(stage == 0)
            {
                button.buttonIndex = UnityEngine.Random.Range(0,22);
                while(selectedIndices.Contains(button.buttonIndex))
                {
                    button.buttonIndex = UnityEngine.Random.Range(0,22);
                }
                selectedIndices.Add(button.buttonIndex);
            }
            else if (stage == 3)
            {
                button.buttonIndex = UnityEngine.Random.Range(0,32);
                while(selectedIndices.Contains(button.buttonIndex))
                {
                    button.buttonIndex = UnityEngine.Random.Range(0,32);
                }
                selectedIndices.Add(button.buttonIndex);
            }
            else
            {
                button.buttonIndex = UnityEngine.Random.Range(0,28);
                while(selectedIndices.Contains(button.buttonIndex))
                {
                    button.buttonIndex = UnityEngine.Random.Range(0,28);
                }
                selectedIndices.Add(button.buttonIndex);
            }
            int buttonDisplay = button.buttonIndex + 1;
            button.text.text = buttonDisplay.ToString();
            if(stage == 0)
            {
                button.containedNumber = level1Options[button.buttonIndex];
            }
            else if(stage == 1)
            {
                button.containedNumber = level2Options[button.buttonIndex];
            }
            else if (stage == 2)
            {
                button.containedNumber = level3Options[button.buttonIndex];
            }
            else
            {
                button.containedNumber = level4Options[button.buttonIndex];
            }
            levelOrdered.Add(button);
        }
        Debug.LogFormat("[Alphabet Numbers #{0}] In reading order, your button labels are {1}, {2}, {3}, {4}, {5}, {6}.", moduleId, buttons[0].text.text, buttons[1].text.text, buttons[2].text.text, buttons[3].text.text, buttons[4].text.text, buttons[5].text.text);
        Debug.LogFormat("[Alphabet Numbers #{0}] In reading order, your numbers are {1}, {2}, {3}, {4}, {5}, {6}.", moduleId, buttons[0].containedNumber.ToString(), buttons[1].containedNumber.ToString(), buttons[2].containedNumber.ToString(), buttons[3].containedNumber.ToString(), buttons[4].containedNumber.ToString(), buttons[5].containedNumber.ToString());
        levelOrdered = levelOrdered.OrderBy(o=>o.containedNumber).ToList();
        Debug.LogFormat("[Alphabet Numbers #{0}] Push the buttons in this order: {1}, {2}, {3}, {4}, {5}, {6}.", moduleId, levelOrdered[0].text.text, levelOrdered[1].text.text, levelOrdered[2].text.text, levelOrdered[3].text.text, levelOrdered[4].text.text, levelOrdered[5].text.text);
        selectedIndices.Clear();
    }

    public void OnButtonPress(Button button)
    {
        if(moduleSolved || button.pressed)
        {
            return;
        }
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        GetComponent<KMSelectable>().AddInteractionPunch(0.5f);
        int pressedIndex = button.buttonPosition;
        pressedButtonList.Add(buttons[pressedIndex]);
        buttons[pressedIndex].text.color = textColors[1];
        button.pressed = true;
        numberOfPresses++;
        if(numberOfPresses == 6)
        {
            for(int i = 0; i < 6; i++)
            {
                if(pressedButtonList[i].containedNumber == levelOrdered[i].containedNumber)
                {
                    rightOrWrong.Add("Right");
                }
                else
                {
                    rightOrWrong.Add("Wrong");
                }
            }
            if(rightOrWrong.Contains("Wrong"))
            {
                GetComponent<KMBombModule>().HandleStrike();
                StartCoroutine(strikeFlash());
                Debug.LogFormat("[Alphabet Numbers #{0}] Strike! You pressed: {1}, {2}, {3}, {4}, {5}, {6}. That is incorrect.", moduleId, pressedButtonList[0].text.text, pressedButtonList[1].text.text, pressedButtonList[2].text.text, pressedButtonList[3].text.text, pressedButtonList[4].text.text, pressedButtonList[5].text.text);
            }
            else
            {
                if(stage < 3)
                {
                    StartCoroutine(passFlash());
                    Debug.LogFormat("[Alphabet Numbers #{0}] You pressed: {1}, {2}, {3}, {4}, {5}, {6}. That is correct.", moduleId, pressedButtonList[0].text.text, pressedButtonList[1].text.text, pressedButtonList[2].text.text, pressedButtonList[3].text.text, pressedButtonList[4].text.text, pressedButtonList[5].text.text);
                    levelOrdered.Clear();
                    stage++;
                    foreach(Button x in buttons)
                    {
                        x.pressed = false;
                    }
                    SelectLevelLabels();
                }
                else
                {
                    Debug.LogFormat("[Alphabet Numbers #{0}] You pressed: {1}, {2}, {3}, {4}, {5}, {6}. That is correct. Module solved.", moduleId, pressedButtonList[0].text.text, pressedButtonList[1].text.text, pressedButtonList[2].text.text, pressedButtonList[3].text.text, pressedButtonList[4].text.text, pressedButtonList[5].text.text);
                    levelOrdered.Clear();
                    moduleSolved = true;
                    StartCoroutine(passFlash());
                    GetComponent<KMBombModule>().HandlePass();
                    foreach(Button label in buttons)
                    {
                        label.text.text = "";
                    }
                }
            }
            numberOfPresses = 0;
            rightOrWrong.Clear();
            pressedButtonList.Clear();
            foreach(Button y in buttons)
            {
                y.text.color = textColors[0];
                y.pressed = false;
            }
        }
    }

    IEnumerator passFlash()
    {
        int flash = 0;
        Audio.PlaySoundAtTransform("lightFlicker", transform);
        int stageCopy = stage;
        while(flash < 10)
        {
            levelIndicators[stageCopy].material = levelMaterials[1];
            yield return new WaitForSeconds(0.1f);
            levelIndicators[stageCopy].material = levelMaterials[2];
            yield return new WaitForSeconds(0.1f);
            flash++;
        }
        levelIndicators[stageCopy].material = levelMaterials[1];
        if(stageCopy < 3)
        {
            levelIndicators[stageCopy+1].material = levelMaterials[2];
        }
    }

    IEnumerator strikeFlash()
    {
        int flash = 0;
        Audio.PlaySoundAtTransform("lightFlicker", transform);
        while(flash < 10)
        {
            levelIndicators[stage].material = levelMaterials[3];
            yield return new WaitForSeconds(0.1f);
            levelIndicators[stage].material = levelMaterials[0];
            yield return new WaitForSeconds(0.1f);
            flash++;
        }
        levelIndicators[stage].material = levelMaterials[2];
    }

#pragma warning disable 414
	private string TwitchHelpMessage = "Press the buttons with !{0} press 1 2 3 4 5 6. The buttons are numbered 1 to 6 in clockwise order, with 1 being at the top.";
#pragma warning restore 414
	private readonly int[] _buttonMap = { 0, 2, 4, 5, 3, 1 };
	private IEnumerator ProcessTwitchCommand(string command)
	{
		command = command.Trim();
		string[] split = command.ToLowerInvariant().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		if (split[0] != "press" || split.Length == 1) yield break;

		List<int> correct = new List<int> { };
		foreach (string number in split.Skip(1))
		{
			int result;
			if (!int.TryParse(number, out result)) yield break;
			if (result > buttons.Length || result == 0) yield break;
			correct.Add(result);
		}
		foreach (int number in correct)
		{
			yield return null;
			yield return new[] { buttons[_buttonMap[number - 1]].selectable };
		}
	}
}
