using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] Image imageBorder;
    [SerializeField] GameObject lvlUpObj;
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;

    Animal _animal;
    public void Init(Animal animal)
    {
        _animal = animal;
        UpdateData();

        _animal.OnHPChanged += UpdateData;

    }

    void UpdateData()
    {
        image.sprite = _animal.Base.FrontSprite;
        nameText.text = _animal.Base.Name;
        levelText.text = "Lvl " + _animal.Level;
        hpBar.SetHP((float)_animal.HP / _animal.MaxHp);
        checkLvlUpImage();
    }

    public void checkLvlUpImage()
    {
        var evolution = _animal.CheckForEvolution();
        if (evolution != null)
            lvlUpObj.SetActive(true);
        else
            lvlUpObj.SetActive(false);
    }

    //Change color of selected party member
    public void SetSelected(bool selected)
    {
        if (selected)
        {
            nameText.color = GlobalSettings.i.HighlightedColor;
            var borderColor = imageBorder.color;
            borderColor.a = 1f;
            imageBorder.color = borderColor;
        }

        else
        {
            nameText.color = Color.black;
            var borderColor = imageBorder.color;
            borderColor.a = 0f;
            imageBorder.color = borderColor;
        }
    }

    public void SetSelectedSwap(bool selected)
    {
        if (selected)
        {
            gameObject.GetComponent<Image>().color = Color.grey;
            nameText.color = Color.white;
            var borderColor = imageBorder.color;
            borderColor.a = 0f;
            imageBorder.color = borderColor; ;
        }

        else
        {
            gameObject.GetComponent<Image>().color = Color.white;
            nameText.color = Color.black;
            var borderColor = imageBorder.color;
            borderColor.a = 0f;
            imageBorder.color = borderColor;
        }
    }

}
