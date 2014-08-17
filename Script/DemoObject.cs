//------------------------------------------------------------------
// 2014		www.JamJamCat.com
// Ivory,Tsai
// 
// For demo gameobject to Preview the spritesheet
//------------------------------------------------------------------
using UnityEngine;
using System.Collections;

public class DemoObject : MonoBehaviour 
{
	int counter_=0;
	Sprite[] spriteAry_;
	SpriteRenderer sprRenderer_;
	
	//-----------------------------------------------------------------------------
	void Start () 
	{
		setup (gameObject.name);
		StartCoroutine("showSprite");
	}
	
	//-----------------------------------------------------------------------------
	void setup (string myName ) 
	{
		// set position
		Vector3 pos = new Vector3(0.0f,-0.5f,1.0f);
		gameObject.transform.Translate(pos);
		
		// load all frames in Sprite array
		// asset must in Resources folder
		spriteAry_ = Resources.LoadAll<Sprite>(myName);
		
		// Attach a SpriteRenender to gameobject
		sprRenderer_ = gameObject.GetComponent<SpriteRenderer>();
		sprRenderer_.sprite = spriteAry_[0];
	}
	
	//-----------------------------------------------------------------------------
	IEnumerator showSprite()
	{
		while(true)
		{
			if(spriteAry_.Length <= counter_)
			{
				counter_=0;
			}
			sprRenderer_.sprite = spriteAry_[counter_];
			counter_++;
			yield return new WaitForSeconds(0.15f); 
		}
	}
}
