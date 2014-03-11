using UnityEngine;
using System.Collections;

public class Spintop : MonoBehaviour {

	//ENEMY STATS
	int CurHP = 3;
	int MARBLE_LO = 5, MARBLE_HI = 15;

	//Visual Components
	Animator myAnim;
	Renderer[] renderers;
	
	//Player Tracker
	GameObject Bear;

	//Invulnerable Vars
	bool Invulnerable = false;
	bool FlashSwitch = false;
	float INV_FLASH_TIME = 0.1f;
	float MAX_FLASH_TIME = 1.1f;
	float StartFlash = 0;
	float LastFlash = 0;	
	
	//STATES THE ENEMY CAN ASSUME
	bool TRANSIT_STATE = true;
	bool COMBAT_STATE = false;
	bool DAZED_STATE = false;
	
	//TRANSIT STATE VARIABLES	
	float RUN_SPEED = 40.0f;
	float WALK_SPEED = 10.0f;
	
	Vector3 OSL; //ORIGINAL SPAWN LOCATION
	
	float WANDER_RADIUS = 40.0f;
	float LAST_WANDER_START;
	bool wasWandering = false;

	//DAZED STATE VARIABLES;
	float KBACK_MAX_DIST = 10.0f;
	float KBackTravelled = 0;
	bool ShouldDaze = false;
	float SHIELD_DAZE_TIME = 3.0f;
	float DAZE_TIME = 1.5f;
	float DazeEnd = 0;
	bool DazeTimerActive = false;
	
	//COMBAT STATE VARIABLES
	float CHARGE_MAX_DIST = 80.0f;
	float ChargeTravelled = 0;
	Vector3 LastPos;
	
	// Use this for initialization
	void Start () {
		//Make note of OSL
		OSL = transform.position;
		LAST_WANDER_START = Time.time;

		renderers = GetComponentsInChildren<Renderer>();
		
		myAnim = GetComponent<Animator>();
		Bear = GameObject.FindGameObjectWithTag("Player");
	}
	
	// Update is called once per frame
	void Update () {
		//GRAPHICAL PRIORITY
		if (Invulnerable)
			InvulnFlash();
		
		if (TRANSIT_STATE)
			Transit();
		
		if (COMBAT_STATE)
			Combat();

		if (DAZED_STATE)
			Dazed();
	}
	
	#region Transit
	void Transit()
	{
		if (true) //Let's do some Wandering!
		{
			if (Vector3.Distance(transform.position, OSL) > WANDER_RADIUS)
			{
				LAST_WANDER_START = Time.time;
				transform.LookAt(OSL);
				transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
				wasWandering = false;
			}
			
			//Translation
			if (Time.time > LAST_WANDER_START + 2)
			{
				LAST_WANDER_START = Time.time;
				
				wasWandering = !wasWandering;
				
				if (!wasWandering)
				{
					myAnim.SetBool("isWalking", true);
					transform.eulerAngles = new Vector3(0,Random.Range(0,360),0);
				}
				else
					myAnim.SetBool("isWalking", false);
			}
			
			if (!wasWandering)
				transform.position += transform.forward * WALK_SPEED * Time.deltaTime;
		}

		if (Vector3.Distance(Bear.transform.position, transform.position) < 40.0f)
		{
			TRANSIT_STATE = false;
			collider.isTrigger = true;
			COMBAT_STATE = true;

			myAnim.SetBool("isWalking", false);
			myAnim.SetBool("isSpinning", true);

			transform.LookAt(Bear.transform.position);
			transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

			ShouldDaze = false;

			ChargeTravelled = 0;
			LastPos = transform.position;
		}
	}
	#endregion
	
	#region Combat
	void Combat()
	{
		transform.position += transform.forward * 30 * Time.deltaTime;

		ChargeTravelled += Vector3.Distance(transform.position, LastPos);
		LastPos = transform.position;

		if (ChargeTravelled >= CHARGE_MAX_DIST)
		{
			COMBAT_STATE = false;
			DAZED_STATE = true;
			collider.isTrigger = false;
			KBackTravelled = 0;
			LastPos = transform.position;
			myAnim.SetBool("isSpinning", false);

			if (ShouldDaze)
				myAnim.SetBool("isDazed", true);
		}
	}
	#endregion

	#region Dazed
	void Dazed()
	{
		if (KBackTravelled < KBACK_MAX_DIST)
		{
			transform.position -= transform.forward * 20 * Time.deltaTime;
		}
		else if (!DazeTimerActive)
		{
			DazeTimerActive = true;

			if(ShouldDaze)
			{
				DazeEnd = Time.time + SHIELD_DAZE_TIME;
				collider.enabled = true; //PART OF THE AVOID HITTING BEAR TWICE BUGFIX
			}
			else
				DazeEnd = Time.time + DAZE_TIME;
		}

		if (DazeTimerActive && Time.time >= DazeEnd)
		{
			DazeTimerActive = false;
			DAZED_STATE = false;
			TRANSIT_STATE = true;
			collider.isTrigger = false;
			ShouldDaze = false;
			myAnim.SetBool("isDazed", false);
			collider.enabled = true; //PART OF THE AVOID HITTING BEAR TWICE BUGFIX
			return;
		}

		KBackTravelled += Vector3.Distance(transform.position, LastPos);
		LastPos = transform.position;
	}
	#endregion

	#region Shield Collide
	public void ShieldCollide(Vector3 colPoint)
	{
		ChargeTravelled = CHARGE_MAX_DIST;
		transform.LookAt(colPoint);
		transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
	}
	#endregion

	#region InvulnFlash
	void InvulnFlash()
	{
		if (Time.time >= StartFlash + MAX_FLASH_TIME)
		{
			Invulnerable = false;
			SwitchColour(Color.white);
			return;
		}
		
		if (Time.time >= LastFlash + INV_FLASH_TIME)
		{
			LastFlash = Time.time;
			
			if (FlashSwitch)
				SwitchColour(Color.white);
			else
				SwitchColour(Color.red);
			
			FlashSwitch = !FlashSwitch;
		}
	}
	#endregion

	#region SwitchColour
	void SwitchColour(Color switchCol)
	{
		foreach (Renderer r in renderers)
			r.material.color = switchCol;
	}
	#endregion

	#region Hit
	void Hit(int DMG)
	{
		if (false)//(Invulnerable)
			return;
		else
		{
			Invulnerable = true;
			FlashSwitch = true;
			StartFlash = Time.time;
			LastFlash = Time.time;
			
			SwitchColour(Color.red);
			
			CurHP-=DMG;
		}

		if (CurHP == 0)
		{
			int marblesToDrop = Random.Range(MARBLE_LO,MARBLE_HI);
			Debug.Log("DROPPING " + marblesToDrop + " MARBLES");
			//On a kill, we need to spawn some marbles.
			for (int i = 0; i < marblesToDrop; i++)
			{
				Instantiate(Resources.Load("Marbles/pBlue_Marble", typeof(GameObject)),
				            new Vector3(transform.position.x+Random.Range(-5,5),
				            transform.position.y+1,
				            transform.position.z+Random.Range(-5,5)),
				            Quaternion.identity);
			}

			//Spawn the despawn graphic
//			Instantiate(Resources.Load("POOF/pPoof", typeof(GameObject)),
//			            new Vector3(transform.position.x,
//			            transform.position.y+2,
//			            transform.position.z),
//			            Quaternion.identity);

			GameObject.Destroy(gameObject); //Finally, destroy the gameobject.
		}
	}
	#endregion

	void OnTriggerEnter(Collider col)
	{
		if (col.tag == "Weapon_R_Hand" && DAZED_STATE)
		{
			Hit(1);
			return;
		}

		if (col.tag == "Player" && COMBAT_STATE)
		{
			if (!Bear.GetComponent<BearControlls>().Hit(1))
				return;
		}

		if (!DAZED_STATE && col.tag != "Weapon_R_Hand")
			ShieldCollide(col.transform.position);

		if (col.tag == "Bear_Shield")
		{
			ShouldDaze = true;
			collider.enabled = false; //PART OF THE AVOID HITTING BEAR TWICE BUGFIX
		}
	}
}
