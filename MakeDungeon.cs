using UnityEngine;
using System.Collections;

public class MakeDungeon : MonoBehaviour {

	public int Complexity;

	public GameObject CORNER, EDGE, FLOOR, GAUNTLET;

	public GameObject JOURNEY, NORMAL, ELITE, BOSS;
	public GameObject BRIDGE33, BRIDGE55, BRIDGE37, BRIDGE35, BRIDGE75, BRIDGE77;
	public GameObject DOOR, CRYSTAL;

	public int NO_OF_ROOMS;
	public int NO_OF_PATHS;

	GameObject[,] Room_List;
	bool[,] NoCollidePath;
	int[,] RoomSizes;

	int[] R_Cs = {100, 30, 10}; //Room Chances

	int PB = 280*2; //Padding between rooms

	// Use this for initialization
	void Start () {
		InitialPath();


		for (int i = 0; i < Complexity; i++)
			AddBonus();
	}
	
	// Update is called once per frame
	void Update () {
	}

	//Adds a linear path of a random 'wind' to the grid.
	void InitialPath()
	{
		int RoomsToSpawn = NO_OF_ROOMS;

		Room_List = new GameObject[RoomsToSpawn*2,RoomsToSpawn*2];
		RoomSizes = new int[RoomsToSpawn*2,RoomsToSpawn*2];

		NoCollidePath = new bool[RoomsToSpawn*2,RoomsToSpawn*2]; //Grid - all vacant - ensures no out of bounds
		for (int i = 0; i < RoomsToSpawn*2; i++)
			for (int j = 0; j < RoomsToSpawn*2; j++)
				NoCollidePath[i,j] = false;

		int[] lastPos = {RoomsToSpawn,RoomsToSpawn}; //Keep track of last room for Bridging.
		int[] curPos = {RoomsToSpawn,RoomsToSpawn}; //Cursor to move through grid (starts at centre)
		int RoomsSpawned = 0; //No rooms yet, so 0 spawned.

		Room_List[NO_OF_ROOMS,NO_OF_ROOMS] = (GameObject)GameObject.Instantiate(JOURNEY,new Vector3(curPos[0]*PB,0,curPos[1]*PB), Quaternion.identity);
		Room_List[NO_OF_ROOMS,NO_OF_ROOMS].name = "START_0_" + curPos[1] + "," + curPos[0]; //X[0] Col     Y[1] Row;
		RoomSizes[NO_OF_ROOMS,NO_OF_ROOMS] = 3;

		int LastRoomSize = 3;
		int LastDir = 0;

		NoCollidePath[RoomsToSpawn,RoomsToSpawn] = true;

		do
		{
			int nextDir = Random.Range(1,5);
			int[] moveBy = {0,0};
			switch(nextDir)
			{
			case 1: // North
				moveBy = new int[]{0,-1};
				break;
			case 2: // East
				moveBy = new int[]{1,0};
				break;
			case 3: // South
				moveBy = new int[]{0,1};
				break;
			case 4: // West
				moveBy = new int[]{-1,0};
				break;
			}


			//Check nothing is in the way at the next square
			if (NoCollidePath[curPos[0]+moveBy[0], curPos[1]+moveBy[1]] == false)
			{
				int NEXT_ROOM_IS = Random.Range(1,100);
				//NEXT_ROOM_IS = 5;

				int RoomSize;
				string TAG;

				//Boss (0 - 10) or last room (ALWAYS Minimum of 1 Boss)
				if (NEXT_ROOM_IS <= 10) //-1 Because Create Room called next.
				{
					RoomSize=7;
					TAG = "_BOSS";
				}
				//Elite (11 - 30)
				else if (NEXT_ROOM_IS >= 11 && NEXT_ROOM_IS <= 30)
				{
					RoomSize=5;
					TAG = "_ELITE";
				}
				//Normal (31 - 100)
				else
				{
					RoomSize=3;
					TAG = "_NORMAL";
				}

				Vector3 BridgePos = Vector3.zero;

				// Destroy blocking chunks for the last room
				foreach(Transform child in Room_List[lastPos[0],lastPos[1]].transform)
				{
					if ((nextDir == 1 && child.name == "South") ||
					    (nextDir == 2 && child.name == "East") ||
					    (nextDir == 3 && child.name == "North") ||
					    (nextDir == 4 && child.name == "West"))
					{
							BridgePos = child.position;
							Destroy(child.gameObject);
					}
				}

				GameObject thisBridge;

				if (LastRoomSize == 3 && RoomSize == 3)
					thisBridge = (GameObject)GameObject.Instantiate(BRIDGE33,
				                                                    BridgePos,
				                                                    Quaternion.Euler(new Vector3(0,-90*(nextDir-1),0)));
				else if (LastRoomSize == 5 && RoomSize == 5)
					thisBridge = (GameObject)GameObject.Instantiate(BRIDGE55,
					                                                BridgePos,
					                                                Quaternion.Euler(new Vector3(0,(-90*(nextDir-1)),0)));
				else if ((LastRoomSize == 5 && RoomSize == 3) || (LastRoomSize == 3 && RoomSize == 5))
					thisBridge = (GameObject)GameObject.Instantiate(BRIDGE35,
					                                                BridgePos,
					                                                Quaternion.Euler(new Vector3(0,-90*(nextDir-1),0)));
				else if ((LastRoomSize == 3 && RoomSize == 7) || (LastRoomSize == 7 && RoomSize == 3))
					thisBridge = (GameObject)GameObject.Instantiate(BRIDGE37,
					                                                BridgePos,
					                                                Quaternion.Euler(new Vector3(0,-90*(nextDir-1),0)));
				else if ((LastRoomSize == 5 && RoomSize == 7) || ((LastRoomSize == 7 && RoomSize == 5)))
					thisBridge = (GameObject)GameObject.Instantiate(BRIDGE75,
					                                                BridgePos,
					                                                Quaternion.Euler(new Vector3(0,-90*(nextDir-1),0)));
				else //if (LastRoomSize == 7 && RoomSize == 7)
					thisBridge = (GameObject)GameObject.Instantiate(BRIDGE77,
					                                                BridgePos,
					                                                Quaternion.Euler(new Vector3(0,-90*(nextDir-1),0)));

				thisBridge.transform.parent = Room_List[lastPos[0],lastPos[1]].transform;
				thisBridge.name = "BRIDGE_" + RoomsSpawned;

				//And add a crystal door
				GameObject NEW_DOOR = (GameObject)GameObject.Instantiate(DOOR,
				                       						 BridgePos,
				                       						 Quaternion.Euler(new Vector3(0,(-90*(nextDir-1)),0)));
				NEW_DOOR.transform.position += -NEW_DOOR.transform.forward * 40.2f; // Move back 40 units into archway.

				// And a Crystal!
				GameObject.Instantiate(CRYSTAL,
				                       new Vector3(curPos[0]*PB,0,curPos[1]*PB), Quaternion.identity);

				curPos[0]+=moveBy[0]; curPos[1]+=moveBy[1]; //Update cursor

				GameObject thisRoom;

				RoomSizes[curPos[0],curPos[1]] = RoomSize;

				if (RoomSize == 5)
					thisRoom = (GameObject)GameObject.Instantiate(ELITE,new Vector3(curPos[0]*PB,0,curPos[1]*PB), Quaternion.identity);
				else if (RoomSize == 7)
					thisRoom = (GameObject)GameObject.Instantiate(BOSS,new Vector3(curPos[0]*PB,0,curPos[1]*PB), Quaternion.identity);
				else
					thisRoom = (GameObject)GameObject.Instantiate(NORMAL,new Vector3(curPos[0]*PB,0,curPos[1]*PB), Quaternion.identity);

				thisRoom.name = "START_" + RoomsSpawned.ToString() + TAG + "_" + curPos[1] + "," + curPos[0]; //X[0] Col     Y[1] Row

				// Destroy blocking chunks for this room
				foreach(Transform child in thisRoom.transform)
				{
					if ((nextDir == 1 && child.name == "North") ||
					    (nextDir == 2 && child.name == "West") ||
					    (nextDir == 3 && child.name == "South") ||
					    (nextDir == 4 && child.name == "East"))
					{
						Destroy(child.gameObject);
					}
				}

				//Update no collide path and add room to map
				NoCollidePath[curPos[0],curPos[1]] = true;
				Room_List[curPos[0],curPos[1]] = thisRoom;

				RoomsSpawned++;


				lastPos = curPos;
				LastRoomSize = RoomSize;
			}

		} while (RoomsSpawned < RoomsToSpawn); //Keep going until map full
	}

	void AddBonus()
	{
		//Mask out 1-room thick border
		for (int i = 1; i < (NO_OF_ROOMS*2)-1; i++)
		{
			for (int j = 1; j < (NO_OF_ROOMS*2)-1; j++)
			{
				// If there is a room here
				if (NoCollidePath[i,j] == true)
				{

					bool BRANCH_FINISH = false;

					//Check to see if there is an empty space next to it.
					for (int ii = i-1; ii < i+2; ii++)
					{
						for (int jj = j-1; jj < j+2; jj++)
						{
							int nextDir = 0;
							//No corners or middle please.
							if ((ii == i-1 && jj == j-1) ||
						    	(ii == i-1 && jj == j+1) ||
						    	(ii == i+1 && jj == j-1) ||
						    	(ii == i+1 && jj == j+1) ||
						    	(ii == i && jj == j))
									continue;

							//Set nextDir accordingly
							if (ii == i-1 && jj == j) //N
								nextDir = 4;
							if (ii == i && jj == j+1) //E
								nextDir = 3;
							if (ii == i+1 && jj == j) //S
								nextDir = 2;
							if (ii == i && jj == j-1) //W
								nextDir = 1;

							//Yes, we have an empty!
							if (NoCollidePath[ii,jj] == false)
							{
								int ShouldAddBonus = Random.Range(1,100);
								if (ShouldAddBonus < 80)
								{
									BRANCH_FINISH = true;
									break;
								}

								int NEXT_ROOM_IS = Random.Range(1,100);
								//NEXT_ROOM_IS = 95; //NORMAL
								
								int RoomSize;
								string TAG;
								
								//Boss (0 - 10) or last room (ALWAYS Minimum of 1 Boss)
								if (NEXT_ROOM_IS <= 10) //-1 Because Create Room called next.
								{
									RoomSize=7;
									TAG = "_BOSS";
								}
								//Elite (11 - 30)
								else if (NEXT_ROOM_IS >= 11 && NEXT_ROOM_IS <= 30)
								{
									RoomSize=5;
									TAG = "_ELITE";
								}
								//Normal (31 - 100)
								else
								{
									RoomSize=3;
									TAG = "_NORMAL";
								}

								Vector3 BridgePos = Vector3.zero;
								
								// Destroy blocking chunks for the last room
								foreach(Transform child in Room_List[i,j].transform)
								{
									if ((nextDir == 1 && child.name == "South") ||
									    (nextDir == 2 && child.name == "East") ||
									    (nextDir == 3 && child.name == "North") ||
									    (nextDir == 4 && child.name == "West"))
									{
										BridgePos = child.position;
										Destroy(child.gameObject);
									}
								}

								GameObject thisBridge;
								
								if (RoomSizes[i,j] == 3 && RoomSize == 3)
									thisBridge = (GameObject)GameObject.Instantiate(BRIDGE33,
									                                                BridgePos,
									                                                Quaternion.Euler(new Vector3(0,-90*(nextDir-1),0)));
								else if (RoomSizes[i,j] == 5 && RoomSize == 5)
									thisBridge = (GameObject)GameObject.Instantiate(BRIDGE55,
									                                                BridgePos,
									                                                Quaternion.Euler(new Vector3(0,(-90*(nextDir-1)),0)));
								else if ((RoomSizes[i,j] == 5 && RoomSize == 3) || (RoomSizes[i,j] == 3 && RoomSize == 5))
									thisBridge = (GameObject)GameObject.Instantiate(BRIDGE35,
									                                                BridgePos,
									                                                Quaternion.Euler(new Vector3(0,-90*(nextDir-1),0)));
								else if ((RoomSizes[i,j] == 3 && RoomSize == 7) || (RoomSizes[i,j] == 7 && RoomSize == 3))
									thisBridge = (GameObject)GameObject.Instantiate(BRIDGE37,
									                                                BridgePos,
									                                                Quaternion.Euler(new Vector3(0,-90*(nextDir-1),0)));
								else if ((RoomSizes[i,j] == 5 && RoomSize == 7) || ((RoomSizes[i,j] == 7 && RoomSize == 5)))
									thisBridge = (GameObject)GameObject.Instantiate(BRIDGE75,
									                                                BridgePos,
									                                                Quaternion.Euler(new Vector3(0,-90*(nextDir-1),0)));
								else //if (LastRoomSize == 7 && RoomSize == 7)
									thisBridge = (GameObject)GameObject.Instantiate(BRIDGE77,
									                                                BridgePos,
									                                                Quaternion.Euler(new Vector3(0,-90*(nextDir-1),0)));
								
								thisBridge.transform.parent = Room_List[i,j].transform;
								thisBridge.name = "BRIDGE_TO_BONUS";

								
								GameObject thisRoom;
								
								if (RoomSize == 5)
									thisRoom = (GameObject)GameObject.Instantiate(ELITE,new Vector3(ii*PB,0,jj*PB), Quaternion.identity);
								else if (RoomSize == 7)
									thisRoom = (GameObject)GameObject.Instantiate(BOSS,new Vector3(ii*PB,0,jj*PB), Quaternion.identity);
								else
									thisRoom = (GameObject)GameObject.Instantiate(NORMAL,new Vector3(ii*PB,0,jj*PB), Quaternion.identity);
								
								thisRoom.name = "START_BONUS_" + jj + "," + ii + TAG;
								thisRoom.transform.parent = Room_List[i,j].transform;
								//thisBridge.transform.parent = thisRoom.transform;
								
								// Destroy blocking chunks for the last room
								foreach(Transform child in thisRoom.transform)
								{
									if ((nextDir == 1 && child.name == "North") ||
									    (nextDir == 2 && child.name == "West") ||
									    (nextDir == 3 && child.name == "South") ||
									    (nextDir == 4 && child.name == "East"))
									{
										Destroy(child.gameObject);
									}
								}

								NoCollidePath[ii,jj] = true;
								Room_List[ii,jj] = thisRoom;
								RoomSizes[ii, jj] = RoomSize;

								BRANCH_FINISH = true;
								{
									break; //Room added so no more branches
								}
							}

							if (BRANCH_FINISH)
							{
								break;
							}

						}

					}
				}

			}
		}

	}


	// Creates a room of specific size, from top left corner
	void CreateRoom(int X, int Z, int W, int L, string NAME)
	{
		GameObject thisRoom = new GameObject(NAME);

		int Corner_Index = 0;
		int[] CornerRot = {270,180,0,90};

		for (int row = 0; row < L; row++)
		{
			for (int col = 0; col < W; col++)
			{
				//Determine corner piece
				if ( (row == 0 && col == 0) ||
				     (row == 0 && col == W-1) ||
				     (row == L-1 && col == 0) ||
				     (row == L-1 && col == W-1) )
				{
					GameObject thisPiece = 
						(GameObject)GameObject.Instantiate(CORNER,new Vector3(col*80,0,row*80), Quaternion.Euler(new Vector3(0,CornerRot[Corner_Index],0)));
					thisPiece.transform.parent = thisRoom.transform;

					Corner_Index++;
					continue;
				}

				//Determine Edge
				if ( (row == 0) ||
				     (row == L-1) ||
				     (col == 0) ||
				     (col == W-1) )
				{
					int WallRot = 45;

					if (row == 0)
						WallRot = 180;
					else if (row == L-1)
						WallRot = 0;
					else if (col == 0)
						WallRot = 270;
					else if (col == W-1)
						WallRot = 90;

					GameObject thisPiece =
						(GameObject)GameObject.Instantiate(EDGE,new Vector3(col*80,0,row*80), Quaternion.Euler(new Vector3(0,WallRot,0)));
					thisPiece.transform.parent = thisRoom.transform;

					continue;
				}

				//Otherwise fill with floor
				GameObject this_Piece =
					(GameObject)GameObject.Instantiate(FLOOR,new Vector3(col*80,0,row*80), Quaternion.identity);
				this_Piece.transform.parent = thisRoom.transform;
			}
		}

		// Finally move room to intended location
		thisRoom.transform.position = new Vector3(X*80,0,Z*80);
	}
}
