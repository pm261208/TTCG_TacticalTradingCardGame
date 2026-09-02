using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Splines;
using static UnityEngine.Rendering.DebugUI.Table;

public class FieldVisual : MonoBehaviour{


    private void Awake() {
        CardGameManager.Instance.OnMatchStart += CardGameManager_OnMatchStart;
    }

    private void CardGameManager_OnMatchStart(object sender, System.EventArgs e) {
         for (int collum = 0; collum < 5; collum++) {
            for (int row = 0; row < 5; row++) {
                Tile tile = CardGameManager.Instance.field[collum, row];
                float tileSpacingX = 1.2f;
                float tileSpacingZ = 1.3f;
                int tileCollum = int.Parse(tile.tileId.ToString()[1].ToString());
                int tileRow = int.Parse(tile.tileId.ToString()[0].ToString());
                tile.transform.parent = this.transform;

                if (CardGameManager.Instance.localPlayer == CardGameManager.Instance.player1) {
                    tile.transform.position = new Vector3(tileSpacingX * (tileRow - 3), 0, tileSpacingZ * (tileCollum - 3));
                } else {
                    tile.transform.position = new Vector3(tileSpacingX * (tileRow - 3), 0, -tileSpacingZ * (tileCollum - 3));
                }
            }
         }
         
    }

    public void UpdateCardPositions() {
        List<Card> fieldCards = new List<Card>{ };
        List<Tile> fieldTiles = new List<Tile>{ };

        foreach (Tile tile in CardGameManager.Instance.field) {
            if(tile.monsterOnTile != null) {
                fieldCards.Add(tile.monsterOnTile);
                fieldTiles.Add(tile);
            }
            if(tile.spellTrapOnTile!= null) {
                fieldCards.Add(tile.spellTrapOnTile);
                fieldTiles.Add(tile);
            }
        }

        float duration = 0.25f;

        if (fieldCards.Count == 0) return;
        for (int i = 0; i < fieldCards.Count; i++) {

            Vector3 position = fieldTiles[i].transform.position + new Vector3(0, 0.11f, 0);
            fieldCards[i].transform.DOMove(position, duration);
            fieldCards[i].transform.DORotate(new(90, 0, 0), duration);
            if (!fieldCards[i].isSet) {
                fieldCards[i].GetComponent<CardVisual>().UnflipCard();
            } else {
                fieldCards[i].GetComponent<CardVisual>().FlipCard(); 
            }
            
        }

    }
    public IEnumerator WaitForUpdateCardsPosition(float s) {
        yield return new WaitForSeconds(s);
        UpdateCardPositions();
    }

    public void ShowSummonedCard(Card card, Tile tile) {

        float duration = 0.25f;
        Vector3 position = tile.transform.position + new Vector3(0, 1f, 0);
        card.transform.DOMove(position, duration);
        card.transform.DORotate(new Vector3(90, 0, 0), duration);


    }
}
