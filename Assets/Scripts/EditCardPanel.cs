using Models;
using TMPro;
using UnityEngine;

public class EditCardPanel : MonoBehaviour
{
    public Cards card { private set; get; }
    [SerializeField] private TMP_InputField cardNameTMP;
    [SerializeField] private TextMeshProUGUI cardID;
    public void InitializePanel(Cards card)
    {
        this.card = card;
        cardNameTMP.text = card.Name;
        cardID.text = card.Card_ID.ToString();
        gameObject.SetActive(true);
    }
    
}
