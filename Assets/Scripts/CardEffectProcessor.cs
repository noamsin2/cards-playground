using UnityEngine;
using Models;
public class CardEffectProcessor : MonoBehaviour
{
    public void ApplyEffect(CardEffect effect, bool isOpponent)
    {
        switch (effect.action)
        {
            case "On Draw":
                ExecuteEffect(effect, isOpponent);
                break;
            case "On Play":
                ExecuteEffect(effect, isOpponent);
                break;
            default:
                Debug.LogWarning($"Unknown action: {effect.action}");
                break;
        }
    }

    private void ExecuteEffect(CardEffect effect, bool isOpponent)
    {
        Debug.Log(effect.effect);
        if (isOpponent)
        {
            switch (effect.effect)
            {

                case "Draw X Cards":
                    MatchManager.Instance.OpponentDrawCard();
                    Debug.Log($"OPPONENT Drew {effect.value} cards!");
                    break;
                case "Deal X Damage":
                    MatchManager.Instance.DamagePlayer(effect.value, "player");
                    Debug.Log($"Dealt {effect.value} damage!");
                    break;
                case "Heal X Health":
                    MatchManager.Instance.HealPlayer(effect.value, "opponent");
                    Debug.Log($"Healed {effect.value} HP!");
                    break;
                default:
                    Debug.LogWarning($"Unknown effect: {effect.effect}");
                    break;
            }
        }
        else
        {
            switch (effect.effect)
            {

                case "Draw X Cards":
                    MatchManager.Instance.DrawCards(effect.value);
                    Debug.Log($"Drew {effect.value} cards!");
                    break;
                case "Deal X Damage":
                    MatchManager.Instance.DamagePlayer(effect.value, "opponent");
                    Debug.Log($"Dealt {effect.value} damage!");
                    break;
                case "Heal X Health":
                    MatchManager.Instance.HealPlayer(effect.value, "player");
                    Debug.Log($"Healed {effect.value} HP!");
                    break;
                default:
                    Debug.LogWarning($"Unknown effect: {effect.effect}");
                    break;
            }
        }
    }
}
