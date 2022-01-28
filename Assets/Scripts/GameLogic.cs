using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct Board
{
	public Card[] upcomingRow;
	public Card[] opponentRow;
	public Card[] playerRow;
}
public class GameLogic : MonoBehaviour
{
	Board board;
	[SerializeField] GameObject cardObj;
    void Start()
    {
		board = new Board();
		board.upcomingRow = new Card[4];
		board.opponentRow = new Card[4];
		board.playerRow = new Card[4];
		CreateCards();
    }

    void Update()
    {
        
    }

	void CreateCards()
	{
		float[] initialCoords = new float[3] {0,0,0};
		float verticalSpacing = 10;
		float horisontalSpacing = 7;

		for (int xx = 0; xx < 4; xx++)
		{
			board.upcomingRow[xx] = Instantiate(cardObj).GetComponent<Card>();
			board.upcomingRow[xx].setStats(1, 2, 4, 8);
			board.upcomingRow[xx].setPos(initialCoords[0] + xx * horisontalSpacing, initialCoords[1] + verticalSpacing * 2, initialCoords[2]);
			board.opponentRow[xx] = Instantiate(cardObj).GetComponent<Card>();
			board.opponentRow[xx].setStats(3, 4, 4, 3);
			board.opponentRow[xx].setPos(initialCoords[0] + xx * horisontalSpacing, initialCoords[1] + verticalSpacing, initialCoords[2]);
			board.playerRow[xx] = Instantiate(cardObj).GetComponent<Card>();
			board.playerRow[xx].setStats(xx, 2+xx, 4+xx, 8+xx);
			board.playerRow[xx].setPos(initialCoords[0] + xx * horisontalSpacing, initialCoords[1], initialCoords[2]);
		}
	}
}
