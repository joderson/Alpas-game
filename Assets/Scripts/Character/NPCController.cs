using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NPCController : MonoBehaviour, Interactable, ISavable
{

    [SerializeField] GameObject questionMark;
    [SerializeField] GameObject inProgressMark;

    [SerializeField] Dialog dialog;

    [Header("Quests")]
    [SerializeField] QuestBase questToStart;
    [SerializeField] QuestBase questToComplete;

    [Header("Movement")]
    [SerializeField] List<Vector2> movementPattern;
    [SerializeField] float timeBetweenPattern;




    NPCState state;
    float idleTimer = 0f;
    int currentPattern = 0;
    Quest activeQuest;

    Character character;
    ItemGiver itemGiver;
    AnimalGiver animalGiver;
    Healer healer;
    Merchant merchant;
    TrainerController trainer;

    Fader fader;


    private void Awake()
    {

        character = GetComponent<Character>();
        itemGiver = GetComponent<ItemGiver>();
        animalGiver = GetComponent<AnimalGiver>();
        healer = GetComponent<Healer>();
        merchant = GetComponent<Merchant>();
        trainer = GetComponent<TrainerController>();
    }

    public void Start()
    {
        fader = FindObjectOfType<Fader>();

        if (questToStart != null || questToComplete != null || (animalGiver != null && animalGiver.CanBeGiven()))
        {
            if (questionMark != null)
            {
                questionMark.SetActive(true);
            }
        }
        else
        {
            questionMark?.SetActive(false);
        }
    }


    public IEnumerator Interact(Transform initiator)
    {
        if (state == NPCState.Idle)
        {
            state = NPCState.Dialog;
            character.LookTowards(initiator.position);

            if(questToComplete != null)
            {
                var quest = new Quest(questToComplete);
                yield return quest.CompleteQuest(initiator);
                questToComplete = null;
                questionMark.SetActive(false);

                Debug.Log($"{quest.Base.Name} completed");
            }

            if (itemGiver != null && itemGiver.CanBeGiven())
            {
                yield return itemGiver.GiveItem(initiator.GetComponent<PlayerController>());
            }
            else if (animalGiver != null && animalGiver.CanBeGiven())
            {
                yield return animalGiver.GiveAnimal(initiator.GetComponent<PlayerController>());
                questionMark.SetActive(false);
            }
            else if (questToStart != null)
            {
                activeQuest = new Quest(questToStart);
                yield return activeQuest.StartQuest();
                questToStart = null;
                questionMark.SetActive(false);
                inProgressMark.SetActive(true);

                //For trainer quests
                if (trainer!= null)
                {
                    yield return StartCoroutine(trainer.Interact(initiator));

                    if (trainer.BattleLostState)
                    {
                        yield return activeQuest.CompleteQuest(initiator);
                        activeQuest = null;
                        inProgressMark.SetActive(false);
                    }
                }
                //For other quests
                else
                {
                    if (activeQuest.CanBeCompleted())
                    {
                        yield return activeQuest.CompleteQuest(initiator);
                        activeQuest = null;
                        inProgressMark.SetActive(false);
                    }
                }
            }
            else if(activeQuest != null)
            {
                //For trainerQuests
                if (trainer != null)
                {
                    if (!trainer.BattleLostState)
                    {
                        yield return DialogManager.Instance.ShowDialog(activeQuest.Base.InProgressDialogue);
                        yield return StartCoroutine(trainer.Interact(initiator));
                    }
                    else {

                        yield return activeQuest.CompleteQuest(initiator);

                        //Speical case where DrDoom will open endcredit scene
                        if(activeQuest.Base.Name == "DefeatDrDoom")
                        {
                            yield return fader.FadeIn(0.5f);
                            activeQuest = null;
                            inProgressMark.SetActive(false);
                            //auto save game before moving on the endcredits
                            if (!PlayerPrefs.HasKey("SavedGame"))
                                PlayerPrefs.SetString("SavedGame", "saveSlot1");
                            SavingSystem.i.Save("saveSlot1");
                            SceneManager.LoadScene("EndCredits");
                            
                        }
                        else
                        {
                            activeQuest = null;
                            inProgressMark.SetActive(false);
                        }

                    }
                }
                //For other quests
                else {
                    if (activeQuest.CanBeCompleted())
                    {
                        yield return activeQuest.CompleteQuest(initiator);
                        activeQuest = null;
                        inProgressMark.SetActive(false);
                    }
                    else
                    {
                        yield return DialogManager.Instance.ShowDialog(activeQuest.Base.InProgressDialogue);
                    }
                }
            }
            else if (healer != null)
            {
                yield return healer.Heal(initiator, dialog);
            }
            else if (merchant != null)
            {
                yield return merchant.Trade();
            }
            else
            {
                yield return DialogManager.Instance.ShowDialog(dialog);
            }


            idleTimer = 0f;
            state = NPCState.Idle;
        }
    }

    private void Update()
    {

        if (state == NPCState.Idle)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer > timeBetweenPattern)
            {
                idleTimer = 0f;
                if (movementPattern.Count > 0)
                    StartCoroutine(Walk());
            }
        }
        character.HandleUpdate();
    }

    IEnumerator Walk()
    {
        state = NPCState.Walking;
        var oldPos = transform.position;

        yield return character.Move(movementPattern[currentPattern]);
        if (transform.position != oldPos)
            currentPattern = (currentPattern + 1) % movementPattern.Count;

        state = NPCState.Idle;
    }

    public object CaptureState()
    {
        var saveData = new NPCQuestSaveData();
        saveData.activeQuest = activeQuest?.GetSaveData();

        if (questToStart != null)
            saveData.questToStart = (new Quest(questToStart)).GetSaveData();
        if (questToComplete != null)
            saveData.questToStart = (new Quest(questToComplete)).GetSaveData();

        return saveData;

    }

    public void RestoreState(object state)
    {
        var saveData = state as NPCQuestSaveData;
        if (saveData != null)
        {
            activeQuest = (saveData.activeQuest != null) ? new Quest(saveData.activeQuest) : null;
            questToStart = (saveData.questToStart != null) ? new Quest(saveData.questToStart).Base : null;
            questToComplete = (saveData.questToComplete != null) ? new Quest(saveData.questToComplete).Base : null;
        }
    }
}

[System.Serializable]
public class NPCQuestSaveData
{
    public QuestSaveData activeQuest;
    public QuestSaveData questToStart;
    public QuestSaveData questToComplete;
}

public enum NPCState { Idle, Walking, Dialog }
