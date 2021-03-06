using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StorageScreen : MonoBehaviour
{
    [SerializeField] PartyScreen partyScreen;

    [SerializeField] Image image;
    [SerializeField] GameObject lvlUpObj;
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Text type1;
    [SerializeField] Text type2;
    [SerializeField] HPBar hpBar;
    [SerializeField] GameObject movesTextContainer;
    [SerializeField] GameObject noAnimalInStorageMessage;

    AnimalStorage storage;
    List<Animal> animals;
    StorageMemberUI[] memberSlots;
    Text[] moveTexts;

    int selection = 0;

    /*private void Awake()
    {
        foreach (var animal in animals) //for loop for every animal in animal list
        {
            animal.Init();
        }
    }*/

    private void Start()
    {
        memberSlots = GetComponentsInChildren<StorageMemberUI>(true);
        moveTexts = movesTextContainer.GetComponentsInChildren<Text>();

        storage = AnimalStorage.GetPlayerStorage();
        SetStorageData();

        storage.OnUpdated += SetStorageData;
    }

    public void SetStorageData()
    {
        animals = storage.Animals;

        if (animals.Count == 0)
            noAnimalInStorageMessage.SetActive(true);
        else
            noAnimalInStorageMessage.SetActive(false);

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

    public void ReplaceAnimalStorage(Animal inParty, Animal inStorage)
    {
        var tempAnimal = inStorage;
        animals[selection] = inParty;
        partyScreen.SelectedMember = tempAnimal;
        SetStorageData();
        partyScreen.SetPartyData();
    }

    public void AddFromStorageToParty(int selection)
    {
        partyScreen.Party.AddAnimal(animals[selection]);
        animals.RemoveAt(selection);
        SetStorageData();
        partyScreen.SetPartyData();
    }


    // Update is called once per frame
    void Update()
    {
        HandleUpdate(null, null);
    }

    public void HandleUpdate(Action onSelected, Action onBack)
    {
        var prevSelection = selection;

        if (Input.GetKeyDown(SettingsManager.i.getKey("RIGHT")) || Input.GetKeyDown(SettingsManager.i.getKey("RIGHT1")))
            ++selection;
        else if (Input.GetKeyDown(SettingsManager.i.getKey("LEFT")) || Input.GetKeyDown(SettingsManager.i.getKey("LEFT1")))
            --selection;
        else if (Input.GetKeyDown(SettingsManager.i.getKey("DOWN"))|| Input.GetKeyDown(SettingsManager.i.getKey("DOWN1")))
            selection += 5;
        else if (Input.GetKeyDown(SettingsManager.i.getKey("UP")) ||Input.GetKeyDown(SettingsManager.i.getKey("UP1")))
            selection -= 5;

        selection = Mathf.Clamp(selection, 0, animals.Count - 1);

        if (selection != prevSelection)
            UpdateMemberSelection(selection);

        if (Input.GetKeyDown(SettingsManager.i.getKey("CONFIRM")) || Input.GetKeyDown(SettingsManager.i.getKey("CONFIRM1")))
        {
            if(partyScreen.State == PartyScreenState.Replace)
                ReplaceAnimalStorage(partyScreen.SelectedMember, animals[selection]);
            if (partyScreen.State == PartyScreenState.Add)
                AddFromStorageToParty(selection);


            gameObject.SetActive(false);
            partyScreen.ResetSelection();
        }
        else if (Input.GetKeyDown(SettingsManager.i.getKey("BACK")) || Input.GetKeyDown(SettingsManager.i.getKey("BACK1")))
        {
            gameObject.SetActive(false);
            partyScreen.ResetSelection();
        }
    }

    public void UpdateMemberSelection(int selectedMember)
    {
        for (int i = 0; i < animals.Count; i++)
        {
            if (i == selectedMember)
            {
                memberSlots[i].SetSelected(true);
                SetInfoContainer(animals[selectedMember]);
            }
            else
                memberSlots[i].SetSelected(false);
        }
    }

    void SetInfoContainer(Animal animal)
    {
        image.sprite = animal.Base.FrontSprite;
        nameText.text = animal.Base.Name;
        levelText.text = "Lvl " + animal.Level;
        type1.text = animal.Base.Type1.ToString();
        type2.text = animal.Base.Type2.ToString();
        hpBar.SetHP((float)animal.HP / animal.MaxHp);
        SetMoves(animal.Moves);
    }

    void SetMoves(List<Move> moves)
    {
        for (int i = 0; i < 4; i++)
        {
            if (i < moves.Count)
                moveTexts[i].text = moves[i].Base.Name;
            else
                moveTexts[i].text = "-";
        }
    }

}

