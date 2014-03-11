using UnityEngine;
using System.Collections;

public class BearControlls : MonoBehaviour {

	Animator animator;
	Renderer[] renderers;
	CharacterController cCont;
	GameObject mCam;
	Camera mCamComp;

	float speed = 10.0f;
	
	//Player Stats
	int CurHP = 5;
	int MaxHP = 5;

	//Shield Stats
	int CurSP = 5;
	int MaxSP = 5;
	float SP_Expended_At = 0;
	float SP_RECHARGE_TIME = 5.0f;
	bool SP_Recharging = false;
	float SP_Icon_Alpha = 0.10f;

	//No Control - for things like delays on pick ups, knock backs, etc.
	bool NoControl = false;
	float NoControlTime = 0.0f;

	// Pick Ups
	GameObject NearestPickUp;
	bool PickUpInRange = false;
	
	//Combat Vars
	SwordCollisions swordCols;
	ShieldCollisions shieldCols;

	//Invulnerable Vars
	bool Invulnerable = false;
	bool FlashSwitch = false;
	float INV_FLASH_TIME = 0.1f;
	float MAX_FLASH_TIME = 1.1f;
	float StartFlash = 0;
	float LastFlash = 0;

	// BEAR UI STUFF
	public Texture PlayerFrame;
	public Texture Heart, Shield, ShieldDep;
	
	//Sword Collisider
	BoxCollider swordBox;
	BoxCollider shieldBox;
	
	float lastCrumb = 0;
	public GameObject origCrumb;

	float lastAttack;
	
	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator>();
		renderers = GetComponentsInChildren<Renderer>();
		cCont = GetComponent<CharacterController>();
		mCam = GameObject.FindGameObjectWithTag("MainCamera");
		mCamComp = mCam.GetComponent<Camera>();
		swordBox = GameObject.FindGameObjectWithTag("Weapon_R_Hand").GetComponent<BoxCollider>();
		shieldBox = GameObject.FindGameObjectWithTag("Bear_Shield").GetComponent<BoxCollider>();
		lastAttack = Time.time;
		
		swordCols = GetComponentInChildren<SwordCollisions>();
		shieldCols = GetComponentInChildren<ShieldCollisions>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		//First check for any NoControl state
		if (NoControl)
		{
			if (Time.time > NoControlTime)
				NoControl = false;
			else
				return;
		}

		// Check for Pick Up!
		DoPickUp();

		if (Input.GetButton("Fire2"))
			speed = 60.0f;
		else if (!animator.GetBool("isHolding"))
			speed = 40.0f;
		else
			speed = 25.0f;


		//GRAPHICAL PRIORITY
		if (Invulnerable)
			InvulnFlash();

		//Block Stuff
		UpdateBlockStats();
		if (Block())
			return;

		//Check For Attacks
		if (Attack())
			return;
		
		//RotateFromThumbStick
		float hori = Input.GetAxis("Hori"); float vert = Input.GetAxis("Vert");
		
		if (hori != 0 || vert != 0)
		{
			float RFTS = Mathf.Atan2(hori, vert);
		    RFTS *= Mathf.Rad2Deg;
			RFTS += mCam.transform.rotation.eulerAngles.y;
			transform.eulerAngles = new Vector3(transform.rotation.x, RFTS, transform.rotation.z);
			
			animator.SetBool("isWalking", true);
			
			hori = Mathf.Abs(hori); vert = Mathf.Abs(vert);
			
			animator.speed = (hori > vert) ? hori*1.5f : vert*1.5f;
		}
		else
		{
			animator.SetBool("isWalking", false);
			animator.speed = 1;
		}
		
		Vector3 camForward = mCam.transform.forward;
		camForward = new Vector3(camForward.x, 0, camForward.z);
		
		Vector3 camRight = mCam.transform.right;
		camRight = new Vector3(camRight.x, 0 , camRight.z);
		
		Vector3 newMoveF = Input.GetAxis("Vert") * (camForward * speed) * Time.deltaTime;
		Vector3 newMoveR = Input.GetAxis("Hori") * (camRight * speed) * Time.deltaTime;
		Vector3 gravity = (Vector3.down*20) * Time.deltaTime;

		cCont.Move(newMoveF + newMoveR + gravity);
	}

	#region DoPickUp
	void DoPickUp()
	{
		if (Input.GetKeyDown(KeyCode.Q) && PickUpInRange)
		{
			animator.SetBool("isHolding", true);
			InvokeNoControl(1.0f);

			Transform setParent = null;

			Transform[] transforms = GetComponentsInChildren<Transform>();

			foreach (Transform t in transforms)
			{
				if (t.name == "L__Hand")
					setParent = t;
			}

			NearestPickUp.transform.parent = setParent;
			NearestPickUp.transform.localPosition = Vector3.zero;
		}
	}
	#endregion

	#region DropPickUp
	public void DropPickUp(float pause)
	{
		animator.SetBool("isHolding", false);
		InvokeNoControl(pause);
	}
	#endregion

	#region InvokeNoControl
	// Invoked whenever we need to stop the bear acting for x-amount of time.
	// Notifies called if already NoControlled
	public bool InvokeNoControl(float ForHowLong)
	{
		if (NoControl)
			return false;

		NoControlTime = Time.time + ForHowLong;
		NoControl = true;

		return true;
	}
	#endregion

	#region AssignNearestPickUp
	public void AssignNearestPickUp(GameObject pickup)
	{
		NearestPickUp = pickup;
		PickUpInRange = true;
	}
	#endregion

	#region UnAssignNearestPickUp
	public void UnAssignNearestPickUp(GameObject pickup)
	{
		if (NearestPickUp == pickup)
		{
			NearestPickUp = null;
			PickUpInRange = false;
		}
	}
	#endregion

	#region Attack
	bool Attack()
	{	
		if (Input.GetButtonDown("Fire1") && !animator.GetBool("isAttacking"))
		{
			animator.SetBool("isAttacking", true);
			swordBox.collider.enabled = true;
				
			//swordCols.hitThisSwing = false;
				
			lastAttack = Time.time;

			return true;
		}

		return false;
	}
	#endregion

	#region SwordColOff
	public void SwordColOff()
	{
		swordBox.collider.enabled = false;
		animator.SetBool("isAttacking", false);
	}
	#endregion

	#region Block
	bool Block()
	{
		if (Input.GetAxis("R_Trigger") == -1 && CurSP > 0)
		{
			animator.speed = 1.5f;
			shieldBox.enabled = true;
			shieldCols.shieldUp = true;
			animator.SetBool("isBlocking", true);
			return true;
		}

		shieldBox.enabled = false;
		shieldCols.shieldUp = false;
		animator.SetBool("isBlocking", false);
		return false;
	}
	#endregion

	#region BlockAttack
	public void BlockAttack()
	{
		CurSP-=1;
		
		if (!SP_Recharging)
		{
			SP_Recharging = true;
			SP_Expended_At = Time.time;
		}
	}
	#endregion

	#region UpdateBlockStats
	void UpdateBlockStats()
	{
		if (SP_Recharging)
		{
			//Recharge Icon Alpha
			SP_Icon_Alpha = (Time.time-SP_Expended_At)/SP_RECHARGE_TIME;

			if (Time.time >= SP_Expended_At + SP_RECHARGE_TIME)
			{
				CurSP+=1;

				if (CurSP == MaxSP)
					SP_Recharging = false;
				else
				{
					SP_Expended_At = Time.time;
					SP_Icon_Alpha = (Time.time-SP_Expended_At)/SP_RECHARGE_TIME;
				}
			}
		}
	}
	#endregion

	#region Hit
	public bool Hit(int DMG)
	{
		if (Invulnerable)
			return false;
		else
		{
			Invulnerable = true;
			FlashSwitch = true;
			StartFlash = Time.time;
			LastFlash = Time.time;

			SwitchColour(Color.red);

			CurHP-=DMG;
			return true;
		}
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
	
	void OnGUI()
	{

		//Draw Hearts
		for (int i = 0; i < MaxHP; i++)
		{
			if (i >= CurHP) // Draw Filled In
				GUI.color = new Color(1,1,1,0.10f);
			else
				GUI.color = new Color(1,1,1,1);
			GUI.DrawTexture(new Rect(64+(i*64), 0, 64, 64), Heart);
		}
		//Draw Shields
		for (int i = 0; i < MaxSP; i++)
		{
			if (i == CurSP) // Draw Filled In
			{
				GUI.color = new Color(1,1,1, SP_Icon_Alpha);
				GUI.DrawTexture(new Rect(64+(i*64), 64, 64, 64), ShieldDep);
			}
			else if (i > CurSP)
			{
				GUI.color = new Color(1,1,1, 0.01f);
				GUI.DrawTexture(new Rect(64+(i*64), 64, 64, 64), ShieldDep);
			}
			else
			{
				GUI.color = new Color(1,1,1,1);
				GUI.DrawTexture(new Rect(64+(i*64), 64, 64, 64), Shield);
			}
		}

		//GUI.color = Color.white;
		//GUI.DrawTexture(new Rect(0, 64, PlayerFrame.width, PlayerFrame.height), PlayerFrame);
	}
	
	private float pushPower = 10.0f;
    void OnControllerColliderHit(ControllerColliderHit hit)
	{
        Rigidbody body = hit.collider.attachedRigidbody;
        if (body == null || body.isKinematic)
            return;
        
        if (hit.moveDirection.y < -0.3f)
            return;
        
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
        body.velocity = pushDir * pushPower;
    }
}
