using System.Collections.Generic;
using NUnit.Framework.Internal;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Tile : GameObjectBase {

    public int tileId;
    public Card monsterOnTile;
    public Card spellTrapOnTile;
    public bool isSelectable;

    [SerializeField] private TextMeshProUGUI textMeshProUGUI;
    [SerializeField] private GameObject selectedVisual;


    private void Start() {
        textMeshProUGUI.text = tileId.ToString();
        selectedVisual.gameObject.SetActive(false);
    }

    public void SelectableTile() {
        isSelectable = true;
        selectedVisual.gameObject.SetActive(true);
    }
    public void UnselectableTile() {
        isSelectable = false;
        selectedVisual.gameObject.SetActive(false);
    }


    public bool IsTileEmpty() {
        if (monsterOnTile == null) return true;
        return false;
    }
    public bool HasMonster() {
        if (monsterOnTile != null) return true;
        return false;
    }
    public bool HasSpellTrap() {
        if (spellTrapOnTile != null) return true;
        return false;
    }

    public bool IsTileBackline(ulong player) {
        if(player == CardGameManager.Instance.player1.id) {
            if (tileId.ToString()[1] == '1') {
                return true;
            } 
        }
        if (player == 2) {
            if (tileId.ToString()[1] == '5') {
                return true;
            }
        }
        return false;
    }
    public bool IsTileInYourField(ulong player) {
        if(player == CardGameManager.Instance.player1.id) {
            if (tileId.ToString()[1] == '1' || tileId.ToString()[1] == '2') {
                return true;
            } 
        }
        if (player == 2) {
            if (tileId.ToString()[1] == '5' || tileId.ToString()[1] == '4') {
                return true;
            }
        }
        return false;
    }


}
