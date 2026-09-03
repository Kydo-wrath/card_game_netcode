using TMPro;
using UnityEngine;

public class CardAspectScript : MonoBehaviour
{
    [SerializeField] private CardBase CardDataScript;
    [SerializeField] private GameObject Illustration;
    [SerializeField] private GameObject Card;
    [SerializeField] private TMP_Text Name;
    [SerializeField] private TMP_Text Description;
    [SerializeField] private TMP_Text GoldValue;
    [SerializeField] private TMP_Text TimeOrAlertValue;
    [SerializeField] private TMP_Text illustratorName;

    private void OnValidate()
    {
        Illustration.GetComponent<Renderer>().material = CardDataScript.CardIllustration;
        Card.GetComponent<Renderer>().material = CardDataScript.CardAspect;
        Name.text = CardDataScript.CardName;
        Description.text = CardDataScript.description;
        Name.text = CardDataScript.CardName;
        illustratorName.text = CardDataScript.IllustratorName;

        if (CardDataScript is AgressionCards agression)
        {
            GoldValue.gameObject.SetActive(false);
            TimeOrAlertValue.gameObject.SetActive(true);
            TimeOrAlertValue.text = agression.awarnessLevel.ToString();
        }
        else if (CardDataScript is CrimeCards Crime)
        {
            TimeOrAlertValue.gameObject.SetActive(true);
            GoldValue.gameObject.SetActive(true);
            TimeOrAlertValue.text = Crime.awarnessLevel.ToString();
            GoldValue.text = Crime.goldGain.ToString();
        }
        else if (CardDataScript is CardWorks work)
        {
            TimeOrAlertValue.gameObject.SetActive(true);
            GoldValue.gameObject.SetActive(true);
            TimeOrAlertValue.text = work.numberOfTurn.ToString();
            GoldValue.text = work.goldGain.ToString();
        }
        else if (CardDataScript is CardObject objectItem)
        {
            TimeOrAlertValue.gameObject.SetActive(false);
            GoldValue.gameObject.SetActive(true);
            GoldValue.text = objectItem.Cost.ToString();
        }
        else if (CardDataScript is CardEvent eventCard || CardDataScript is TrickCard Trick)
        {
            TimeOrAlertValue.gameObject.SetActive(false);
            GoldValue.gameObject.SetActive(false);
        }
    }

    public CardBase GetCardData()
    {
        return CardDataScript;
    }

}
