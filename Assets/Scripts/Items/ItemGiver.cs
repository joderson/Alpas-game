using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGiver : MonoBehaviour, ISavable
{
    [SerializeField] ItemBase item;
    [SerializeField] int count;
    [SerializeField] Dialog dialog;

    bool used = false;

    public IEnumerator GiveItem(PlayerController player)
    {
        yield return DialogManager.Instance.ShowDialog(dialog);

        player.GetComponent<Inventory>().AddItem(item, count);

        used = true;

        AudioManager.i.PlaySfx(AudioId.ItemObtained, pauseMusic: true);

        string dialogtext = $"{player.Name} received {item.Name}";
        if (count > 1)
            dialogtext = $"{player.Name} received {count} {item.Name}s";
        
        yield return DialogManager.Instance.ShowDialogText(dialogtext);
    }

    public bool CanBeGiven()
    {
        return item != null && !used && count>0;
    }

    public object CaptureState()
    {
        return used;
    }

    public void RestoreState(object state)
    {
        used = (bool)state;
    }
}
