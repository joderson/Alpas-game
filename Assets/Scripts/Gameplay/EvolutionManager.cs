using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionManager : MonoBehaviour
{
    [SerializeField] GameObject evolutionUI;
    [SerializeField] Image animalImage;
    [SerializeField] GameObject quizUI;

    [SerializeField] string evolutionMusic;

    private QuizUI quizScript;

    public event Action OnStartEvolution;
    public event Action OnCompleteEvolution;

    public static EvolutionManager i { get; private set; }

    private void Awake()
    {
        i = this;
        quizScript = quizUI.GetComponent<QuizUI>();
    }

    //Evolve
    public IEnumerator Evolve(Animal animal, Evolution evolution)
    {
        quizUI.SetActive(true);

        AudioManager.i.Play(evolutionMusic);

        quizScript.Reset();
        yield return new WaitUntil(() => quizScript.CorrectAns != null);

        //if qns answered correctly
        if (quizScript.CorrectAns == true)
        {
            Debug.Log("Qn is answered correctly");
            yield return new WaitForSeconds(0.5f);
            yield return DialogManager.Instance.ShowDialogText($"Good Job! You have answered correctly!");

            quizUI.SetActive(false);

            OnStartEvolution?.Invoke();
            evolutionUI.SetActive(true);


            //pokemon before evoluion
            animalImage.sprite = animal.Base.FrontSprite;
            yield return DialogManager.Instance.ShowDialogText($"{animal.Base.Name} is evolving");
            var oldAnimal = animal.Base;

            animal.Evolve(evolution);

            //evolved pokemon
            animalImage.sprite = animal.Base.FrontSprite;
            yield return DialogManager.Instance.ShowDialogText($"{oldAnimal.Name} evolved into {animal.Base.Name}");

           

            //deactive ui and continue gameplay
            evolutionUI.SetActive(false);
            

        }
        else {
            Debug.Log("Qn is answered wrongly");
            yield return new WaitForSeconds(0.5f);
            yield return DialogManager.Instance.ShowDialogText($"You got the answer wrong! Try again");

            if (quizScript.GetCorrectAnsString!=null)
            {
                yield return DialogManager.Instance.ShowDialogText($"The correct answer should be {quizScript.GetCorrectAnsString}.");
            }

            quizUI.SetActive(false);

        }

        OnCompleteEvolution?.Invoke();



    }

}
