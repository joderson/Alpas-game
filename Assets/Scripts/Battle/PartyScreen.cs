using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum PartyScreenState { PartyScreen, ChoiceBox, Swap, Replace, Add, Release, Busy}

public class PartyScreen : MonoBehaviour
{
    [SerializeField] Text messageText;
    [SerializeField] GameObject choiceBox;
    [SerializeField] StorageScreen animalStorage;

    PartyMemberUI[] memberSlots;
    List<Animal> animals;
    AnimalParty party;
    Text[] choices;

    PartyScreenState state;

    int selection = 0;
    int choiceSelection = 0;
    int swapSelection = 0;

    public Animal SelectedMember {
        get=> animals [selection];
        set => animals[selection] = value;
    }

    public AnimalParty Party => party;

    public PartyScreenState State => state;

    //Party Screen can be called from different states like ActionSelection, RunningTurn, AboutToUse
    public BattleState? CalledFrom { get; set; }

    public void Init()
    {
        //Returns all the partyMemberUI components that are attached to the partyScreen
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);

        party = AnimalParty.GetPlayerParty();
        SetPartyData();

        party.OnUpdated += SetPartyData;
        //animalStorage.Init();
    }

    private void Start()
    {
        if (choiceBox != null)
        {
            //get the children in choicebox
            choices = choiceBox.GetComponentsInChildren<Text>();
            UpdateChoiceBox(choiceSelection);
        }
    }

    public void SetPartyData()
    {
        messageText.text = "Choose an Animal";
        animals = party.Animals;

        for (int i = 0; i < memberSlots.Length; i++)
        {
        if (i < animals.Count)
        {
            memberSlots[i].gameObject.SetActive(true);
            memberSlots[i].Init(animals[i]);
        }
        else
            memberSlots[i].gameObject.SetActive(false);
        }

        UpdateMemberSelection(selection);

        
    }

    //Handles party screen selection
    public void HandleUpdate(Action onSelected, Action onBack)
    {
        if (state == PartyScreenState.PartyScreen)
        {
            var prevSelection = selection;

            if (Input.GetKeyDown(SettingsManager.i.getKey("RIGHT")) || Input.GetKeyDown(SettingsManager.i.getKey("RIGHT1")))
                ++selection;
            else if (Input.GetKeyDown(SettingsManager.i.getKey("LEFT")) || Input.GetKeyDown(SettingsManager.i.getKey("LEFT1")))
                --selection;
            else if (Input.GetKeyDown(SettingsManager.i.getKey("DOWN")) || Input.GetKeyDown(SettingsManager.i.getKey("DOWN1")))
                selection += 2;
            else if (Input.GetKeyDown(SettingsManager.i.getKey("UP")) || Input.GetKeyDown(SettingsManager.i.getKey("UP1")))
                selection -= 2;

            //Restrict value of currentMove between 0 and no. of animals moves
            selection = Mathf.Clamp(selection, 0, animals.Count - 1);

            if (selection != prevSelection)
                UpdateMemberSelection(selection);

            if (Input.GetKeyDown(SettingsManager.i.getKey("CONFIRM")) || Input.GetKeyDown(SettingsManager.i.getKey("CONFIRM1")))
            {
                onSelected?.Invoke();
                if (onSelected == null)
                {
                    if (animals.Count > 0)
                    {
                        EnableChoiceBox(true);
                        messageText.text = $" {SelectedMember.Base.Name} was selected. Choose an Action";
                    }
                }

            }
            //Go back to select action screen if esc or backspace is pressed
            else if (Input.GetKeyDown(SettingsManager.i.getKey("BACK")) || Input.GetKeyDown(SettingsManager.i.getKey("BACK1")))
            {
                onBack?.Invoke();

            }
        } 
        else if (state == PartyScreenState.ChoiceBox)
        {
            var prevChoiceSelection = choiceSelection;

            if (Input.GetKeyDown(SettingsManager.i.getKey("DOWN")) || Input.GetKeyDown(SettingsManager.i.getKey("DOWN1")))
                ++choiceSelection;
            else if (Input.GetKeyDown(SettingsManager.i.getKey("UP")) || Input.GetKeyDown(SettingsManager.i.getKey("UP1")))
                --choiceSelection;

            choiceSelection = Mathf.Clamp(choiceSelection, 0, choices.Length-1);

            if (choiceSelection != prevChoiceSelection)
                UpdateChoiceBox(choiceSelection);

            if (Input.GetKeyDown(SettingsManager.i.getKey("CONFIRM")) || Input.GetKeyDown(SettingsManager.i.getKey("CONFIRM1")))
            {
                StartCoroutine(ChoiceSelection(choiceSelection));
            }
            //else, if press escape, then go back
            else if (Input.GetKeyDown(SettingsManager.i.getKey("BACK")) || Input.GetKeyDown(SettingsManager.i.getKey("BACK1")))
            {
                choiceSelection = 0;
                EnableChoiceBox(false);
            }
        }
        if (state == PartyScreenState.Swap)
        {
            var prevSelection = swapSelection;

            if (Input.GetKeyDown(SettingsManager.i.getKey("RIGHT")) || Input.GetKeyDown(SettingsManager.i.getKey("RIGHT1")))
                ++swapSelection;
            else if (Input.GetKeyDown(SettingsManager.i.getKey("LEFT")) || Input.GetKeyDown(SettingsManager.i.getKey("LEFT1")))
                --swapSelection;
            else if (Input.GetKeyDown(SettingsManager.i.getKey("DOWN")) || Input.GetKeyDown(SettingsManager.i.getKey("DOWN1")))
                swapSelection += 2;
            else if (Input.GetKeyDown(SettingsManager.i.getKey("UP")) || Input.GetKeyDown(SettingsManager.i.getKey("UP1")))
                swapSelection -= 2;

            swapSelection = Mathf.Clamp(swapSelection, 0, animals.Count - 1);

            if (swapSelection != prevSelection)
                UpdateMemberSelection(swapSelection);

            if (Input.GetKeyDown(SettingsManager.i.getKey("CONFIRM")) || Input.GetKeyDown(SettingsManager.i.getKey("CONFIRM1")))
            {
                if (selection == swapSelection)
                    messageText.text = "Cannot swap with same animal";
                else
                    SwapAnimal(swapSelection, selection);
            }
            else if (Input.GetKeyDown(SettingsManager.i.getKey("BACK")) || Input.GetKeyDown(SettingsManager.i.getKey("BACK1")))
            {
                ResetSelection();
            }
        }
    }

    IEnumerator ChoiceSelection(int selection)
    {
        if(selection == 0)
        {
            memberSlots[this.selection].SetSelectedSwap(true);
            messageText.text = $"Chosen animal to swap: {SelectedMember.Base.Name}";
            UpdateMemberSelection(swapSelection);
            yield return new WaitForSeconds(0.2f);
            state = PartyScreenState.Swap;

        }
        else if (selection == 1)
        {
            //Replace
            state = PartyScreenState.Replace;
            animalStorage.gameObject.SetActive(true);
        }
        else if (selection == 2)
        {
            if(animals.Count >= 6)
            {
                StartCoroutine(DialogManager.Instance.ShowDialogText("Party is full, cannot add animal"));
            }
            else
            {
                state = PartyScreenState.Add;
                animalStorage.gameObject.SetActive(true);
            }
        }
        else if (selection == 3)
        {
            //Release
            //todo: add comfirmation dialogue before releasing
            if (animals.Count == 1)
            {
                StartCoroutine(DialogManager.Instance.ShowDialogText("Cannot release if you only have one animal"));
            }
            else
            {
                animals.Remove(SelectedMember);
            }
            ResetSelection();

        }
    }

    void SwapAnimal(int currentIndex, int swapIndex)
    {
        var tempAnimal = animals[currentIndex];
        animals[currentIndex] = animals[swapIndex];
        animals[swapIndex] = tempAnimal;
        ResetSelection();
    }

    public void ResetSelection()
    {
        memberSlots[this.selection].SetSelectedSwap(false);
        SetPartyData();
        EnableChoiceBox(false);
        selection = 0;
        choiceSelection = 0;
        swapSelection = 0;
        UpdateMemberSelection(selection);
        UpdateChoiceBox(choiceSelection);
    }
    public void EnableChoiceBox(bool enable)
    {
        choiceBox.SetActive(enable);
        if (enable)
        {
            state = PartyScreenState.ChoiceBox;
            messageText.text = $" {SelectedMember.Base.Name} was selected. Choose an Action";
        }
        else
        {
            state = PartyScreenState.PartyScreen;
            messageText.text = "Choose an Animal";
        }
    }

    public void UpdateMemberSelection(int selectedMember)
    {
        for (int i = 0; i < animals.Count; i++)
        {
            if (i == selectedMember)
                memberSlots[i].SetSelected(true);
            else
                memberSlots[i].SetSelected(false);
        }
    }

    void UpdateChoiceBox(int selectedChoice)
    {
        for (int i = 0; i < choices.Length; i++)
        {
            if (i == selectedChoice)
                choices[i].color = GlobalSettings.i.HighlightedColor;
            else
                choices[i].color = Color.black;
        }
    }

    //Set text on partyScreen
    public void SetMessageText(string message)
    {
        messageText.text = message;
    }
}
