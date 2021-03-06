﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SCTable : MonoBehaviour {
	
	public Vector3 tableCenter;
	public Vector3 pileLocation;
	//public float spacing;
	public GameObject hand;
	public GameObject cardObj;
	
	private SCRules rules;
	private List<GameObject> cards;
	private List<GameObject> pile;
	
	void Start(){
		cards = new List<GameObject>();
		pile = new List<GameObject>();
		rules = new SCRules();
		hand = Instantiate(hand);
		hand.transform.SetParent(transform.parent);
		hand.transform.localPosition = new Vector3(0, -30, 0);
		SCHand cont = hand.GetComponent<SCHand>();
		cont.table = this;
	}
	
	void Update(){
		//processKeys();
	}
	
	private void processKeys(){
	}
	
	public bool playExistingCard(GameObject[] cards, bool strict, ref string extra){
		SCCardInfo[] cardsToCheck = new SCCardInfo[4];
		int cardsAdded = 0;

		for(int i = 0; i < cardsToCheck.Length; ++i){
			if(cards[i] == null){
				continue;
			}
			SCCard prop = cards[i].GetComponent<SCCard>();
			cardsToCheck[i] = new SCCardInfo(prop.suit, prop.number, prop.guiCard);
			++cardsAdded;
		}
		if(!cardsToCheck[0].guiCard && strict && !rules.allowedToPlay(cardsToCheck, false)){
			return false;
		}
		rules.updateTopCards(cardsToCheck, false);

		SCHand h = hand.GetComponent<SCHand>();
		if(!h.guiHand){
			h.updateHeldCards();
		}
		
		SCAnimator anim;
		Vector3 targetPosition;
		Vector3 targetRotation;

		for(int i = 0; i < cards.Length; ++i){
			if(cards[i] == null){
				continue;
			}
			this.cards.Add(cards[i]);
			cards[i].transform.SetParent(transform);

			SCCard prop = cards[i].GetComponent<SCCard>();
			anim = cards[i].GetComponent<SCAnimator>();

			prop.setSelectable(false);
			prop.active = true;
			prop.higher = false;
			targetPosition = cloneVector3(tableCenter);
			targetPosition.x += Random.Range(-5.0f, 5.0f);
			targetPosition = fixZPosition(targetPosition, this.cards.Count - (cardsAdded - i));
			targetRotation = new Vector3(0, 0, (Random.Range(0, 2) == 0) ? Random.Range(0.0f, 12.0f) : Random.Range(348.0f, 359.0f));
			anim.moveTo(targetPosition, 0.5f, SCAnimator.EASE_OUT, 0.1f * i);
			anim.rotateToTarget(targetRotation, 0.5f, SCAnimator.EASE_OUT, 0.1f * i);
			if(i == cardsAdded - 1 && !rules.isAnyOtherCardPossible()){
				anim.callBack = scrapPile;
				extra = "repeat_turn";
			}
		}

		for(int i = 0; i < this.cards.Count - cardsAdded; ++i){
			anim = this.cards[i].GetComponent<SCAnimator>();
			targetPosition = cloneVector3(this.cards[i].transform.localPosition);
			//targetPosition.x += spacing;
			anim.moveTo(fixZPosition(targetPosition, i), 0.5f, SCAnimator.EASE_OUT);
		}

		return true;
	}
	
	public void playNewCard(SCCardInfo[] cards, Vector3 origin){
		GameObject[] cardsToPlay = new GameObject[cards.Length];
		for(int i = 0; i < cardsToPlay.Length; ++i){
			if(cards[i] == null){
				continue;
			}
			GameObject card = Instantiate(cardObj);
			SCCard prop = card.GetComponent<SCCard>();
			prop.suit = cards[i].suit;
			prop.number = cards[i].number;
			prop.createCard();
			card.transform.SetParent(transform);
			card.transform.localPosition = origin;
			cardsToPlay[i] = card;
		}
		string extra = "";
		playExistingCard(cardsToPlay, false, ref extra);
	}

	public void safeScrapPile(){
		SCAnimator anim = cards[cards.Count - 1].GetComponent<SCAnimator>();
		if(anim.inProgress && anim.callBack == null){
			Debug.Log("Waiting on animation before scrap");
			anim.callBack = scrapPile;
		}else{
			Debug.Log("Didn't wait for animation");
			scrapPile();
		}
	}

	public void scrapPile(){
		for(int i = 0; i < pile.Count; ++i){
			Vector3 targetPosition = cloneVector3(pile[i].transform.localPosition);
			targetPosition.z = 0.05f * (cards.Count + pile.Count - i);
			pile[i].transform.localPosition = targetPosition;
		}

		for(int i = 0; i < cards.Count; ++i){
			SCCard prop = cards[i].GetComponent<SCCard>();
			prop.setSelectable(false);
			prop.active = true;
			prop.higher = false;

			pile.Add(cards[i]);

			SCAnimator anim = cards[i].GetComponent<SCAnimator>();
			Vector3 targetPosition = cloneVector3(pileLocation);
			targetPosition.z = 0.05f * (cards.Count - i);
			anim.moveTo(targetPosition, 0.5f, SCAnimator.EASE_OUT);
		}

		cards = new List<GameObject>();
		SCCardInfo[] newTopCards = new SCCardInfo[4];
		newTopCards[0] = new SCCardInfo(SCCardInfo.ANY_SUIT, SCCardInfo.ANY_NUMBER);
		rules.updateTopCards(newTopCards, true);

		SCHand h = hand.GetComponent<SCHand>();
		if(!h.guiHand){
			h.updateHeldCards();
		}
	}

	public void clear(bool destroy){
		for(int i = 0; i < cards.Count; ++i){
			if(destroy){
				Destroy(cards[i]);
			}
			cards[i] = null;
		}
		cards.Clear();
		for(int i = 0; i < pile.Count; ++i){
			if(destroy){
				Destroy(pile[i]);
			}
			pile[i] = null;
		}
		pile.Clear();
		rules.resetTopCards();
	}
	
	private Vector3 cloneVector3(Vector3 x){
		return new Vector3(x.x, x.y, x.z);
	}
	
	private Vector3 fixZPosition(Vector3 position, int index){
		return new Vector3(position.x, position.y, 0.05f * (cards.Count - index));
	}

	public SCRules getRules(){
		return rules;
	}
}
