using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using Random = UnityEngine.Random;

public class LevelGenerator : MonoBehaviour

{

    /*
    
    cardinal directions according to this script:
    
    north: negative z
    east: negative x
    south: positive z
    west: positive x
    
    */

    [Header("The seed used for random level generation")]
    // variable to determine the value of the seed
    public int seedValue;

    [Header("Rooms and floors")]
    [Tooltip("Number of floors in the level.")]
    public int floors;
    [Tooltip("Maximum amount of room to be generated per floor.")]
    public int maxRooms;
    [Tooltip("Amount of rooms to be added to each floor.")]
    public int extraRooms;

    [Header("Room dimensions")]
    [Tooltip("The size of the room, determines both the width and breadth of the room.")]
    [Range (2.0f, 1000.0f)]
    public float roomWidth;
    [Tooltip("The height of the room.")]
    [Range (2.0f, 100.0f)]
    public float roomHeight;
    [Tooltip("The width of the walls.")]
    public float wallWidth;
    //[Tooltip("The prefab used for the dead end walls. You will most likely keep this empty")]
    //public GameObject wallPrefab;

    [Header("Doors")] 
    [Tooltip("The width of the door.")]
    public float doorWidth;
    [Tooltip("The height of the door.")] 
    public float doorHeight;
    //[Tooltip("The prefab used to create doors. Leave this empty if you want the algorithm to make its own prefab.")]
    //public GameObject doorPrefab;
    [Tooltip("The type of doors that the script will create if you don't have your own prefab.")]
    public DoorType doorType;
    [Tooltip("The speed at which the door opens.")]
    public float doorSpeed;

    [Header("Secret Rooms")] 
    [Tooltip("The amount of secret rooms per floor.")]
    public int maxSecretRooms;
    [Tooltip("The amount of secret rooms that will be added per floor.")]
    public int extraSecretRooms;

    [Header("Materials")]
    [Tooltip("If you have your own materials, put them in the array. If the array is left empty the algorithm will generate its own textures.")]
    public Material[] materials = new Material[4];
    
    [Header("Room Prefab")]
    [Tooltip("The prefab you want the level to generate the level out of. Leave this empty if you want the algorithm to make its own prefab.")]
    public GameObject roomPrefab;

    [Header("Interiors")]
    public GameObject roomInterior;
    [Tooltip("The strength of lights. This number is further scaled by the size of the room.")]
    public float lightIntensity;

    [Header("Debugging")]
    [Tooltip("How many iterations should the program go through to determine if the program is stuck. Higher numbers give extra probability of all options being depleted at the cost of performance.")]
    public int failsafeThreshold;
    
    private float[] xCoordinates;
    private float[] zCoordinates;
    private float[] xSecretCoordinates;
    private float[] zSecretCoordinates;

    public int currentIteration;
    public int lastSuccessfulIteration;
    
    // placement point for the doors and walls
    public Vector3 placementPoint;
    // rotation angle for doors and walls
    public int rotationAngle;
    
    // reference for parenting walls and doors
    public GameObject doorWallParentTo;
    
    // variables for building the level
    private int floorCount;
    private int roomCount;
    private int secretRoomCount;
    private float roomTop;
    private float roomBottom;
    private float roomOriginX;
    private float roomOriginZ;
    private float tempX;
    private float tempZ;
    private bool isGenerating = true;

    // level generator game object to reference and for parenting
    private GameObject LevelGeneratorObject;

    // initial values for making a formula for the room count on each floor
    private int initialMaxRooms;
    private int initialMaxSecretRooms;

    // list of rooms to reference later for parenting and adding components to
    public GameObject[] roomList;

    // two checks, so that by the time the algorithm gets to raycasting the rooms are ready
    private bool roomsReady = false;
    private bool secretRoomsReady = false;

    // can the wall be destroyed (secret rooms)
    //public bool destructible;
    
    // starts the algorithm after errors are checked
    private bool readyToStart = false;
    // bool so that doors are only created once
    private bool createDoorsInUpdate = true;

    // list of all empties for doors and walls to have components attached later
    public List<GameObject> doorsAndWalls = new List<GameObject>();

    // enum for door types, used enum because it is a drop down in the editor and only allows one of these
    // double doors removed because they don't function properly
    public enum DoorType
    
    {
    
        SlidingSingleLeft,
        SlidingSingleRight,
        //SlidingDouble
        
    }

    void Start()
    
    {

        // if the seed value has been changed from its default, which is 0
        if (seedValue != 0)

        {

            // set the seed for random number generation
            Random.InitState(seedValue);

        }
        
        // reference level generator object for later
        LevelGeneratorObject = GameObject.Find("Level Generator");

        // run the error check before the level starts to generate
        // these type of errors are logic errors of configuration, i.e., the door height is greater than the room height or the door width is greater than the room width
        if (readyToStart == false)

        {
            
            ErrorChecks();
            
        }

        // set initial max rooms and secret rooms
        initialMaxRooms = maxRooms;
        initialMaxSecretRooms = maxSecretRooms;
        
        // for loop for generating floors, each iteration is a separate floor
        for (int floorNum = 0; floorNum < floors; floorNum++)

        {

            // max rooms will be the initial rooms plus extra rooms multiplied by floor number,
            // i.e. max rooms: 10, extra rooms: 2, floors: 3 = floor 1: 10 rooms, floor 2: 12 rooms etc
            maxRooms = initialMaxRooms + (extraRooms * floorNum);
            // same for secret rooms except using secret rooms instead of normal
            maxSecretRooms = initialMaxSecretRooms + (extraSecretRooms * floorNum);

            // reset rooms being ready
            roomsReady = false;
            secretRoomsReady = false;
            
            // if the algorithm is ready
            if (readyToStart == true)

            {
            
                // generate the level by creating textures, coordinates and instantiating both types of rooms
                CreateTextures();
                CreateCoordinates();
                InstantiateRooms();
                InstantiateSecretRooms();

            }

            // once the rooms are ready, create the doors and interiors
            if (secretRoomsReady == true && roomsReady == true)

            {
            
                CreateDoors();
                CreateInteriors();
            
            }
            
        }
        
    }

    private void Update()
    
    {

        // update is only used for doors, the first frame of update, run the createDoorsInUpdate method once
        if (createDoorsInUpdate == true)

        {

            // set to false to stop this from repeating or being done more than once
            createDoorsInUpdate = false;
            
            // run a for loop
            for (int i = 0; i < doorsAndWalls.Count; i++)

            {

                // if the object in index i is a door, add door component
                if (doorsAndWalls[i].gameObject.name == "Door")

                {
                    
                    doorsAndWalls[i].gameObject.AddComponent<Doors>();
                    
                }

                // else, add a wall component
                else
                
                {
                
                    doorsAndWalls[i].gameObject.AddComponent<Walls>();
                    
                }

            }

        }
        
    }

    private void CreateTextures()

    {

        // if the user didn't assign materials for the level generator, create default ones using code
        // any of these 4 can be replaced by the user's materials inside the editor (or the code can be changed for a different colour)

        // code is the same for all 4, except the colour and material names are different
        if (materials[0] == null)

        {

            // create a new material using Unity's standard material
            Material floorMaterial = new Material(Shader.Find("Standard"));
            // make a new colour, in this case, brown to represent wood
            Color floorColour = new Color(0.4f, 0.2f, 0.0f, 1.0f);
            // set the colour of floor material
            floorMaterial.SetColor("_Color", floorColour);
            // change the material name to Floor Material
            floorMaterial.name = "Floor Material";
            // and add it to the array of materials
            materials[0] = floorMaterial;

        }
        
        if (materials[1] == null)

        {

            Material ceilingMaterial = new Material(Shader.Find("Standard"));
            Color ceilingColour = new Color(0.9f, 0.9f, 0.9f, 1.0f);
            ceilingMaterial.SetColor("_Color", ceilingColour);
            ceilingMaterial.name = "Ceiling Material";
            materials[1] = ceilingMaterial;

        }
        
        if (materials[2] == null)

        {

            Material wallMaterial = new Material(Shader.Find("Standard"));
            Color wallColour = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            wallMaterial.SetColor("_Color", wallColour);
            wallMaterial.name = "Wall Material";
            materials[2] = wallMaterial;

        }
        

        if (materials[3] == null)

        {

            Material doorMaterial = new Material(Shader.Find("Standard"));
            Color doorColour = new Color(0.3f, 0.3f, 0.3f, 1.0f);
            doorMaterial.SetColor("_Color", doorColour);
            doorMaterial.name = "Door Material";
            materials[3] = doorMaterial;

        }

    }

    private void CreateCoordinates()

    {
        
        // create an array for x coordinates, where the length will be the max rooms for the floor
        xCoordinates = new float[maxRooms];
        // do the same for a z coordinate array
        zCoordinates = new float[maxRooms];
        // set the roomlist to be as long as max rooms and max secret rooms combined
        roomList = new GameObject[maxRooms + maxSecretRooms];
        
        //these will be wiped and reused each time a new floor is made
        
        // increase the floor count
        floorCount++;
        // set the top of the room to be the room height by the floor count, i.e., room height: 3, room 1: 3, room 2: 6 etc.
        roomTop = (roomHeight * floorCount);
        // using that number, set the bottom of the room to be that minus the room height to get the floor: i.e., room 1: 0, room 2: 3 etc.
        roomBottom = roomTop - roomHeight;
        
        //reset roomOriginX, roomOriginZ, roomCount and secretRoomCount
        roomOriginX = 0;
        roomOriginZ = 0;
        roomCount = 0;
        secretRoomCount = 0;
        
        //add the initial room at 0,0 or wherever the level generator empty is dropped
        xCoordinates[0] = roomOriginX;
        zCoordinates[0] = roomOriginZ;
        
        // set the room count to one, as the first coordinate is assigned outside the while loop
        roomCount = 1;
        
        // keep generating new coordinates until an array is filled, doesn't matter if it's x or z coordinates array
        while (roomCount < xCoordinates.Length)

        {
            
            // set is generating to true so that the while loop is entered
            // this loop will be continued until a new unique coordinate is generated
            isGenerating = true;
            
            // keep generating coordinates until the new coordinate isn't a duplicate
            while (isGenerating)

            {
                
                // set the temporary numbers to equal to room origins, so that the original numbers can be used as a reference to reset the rng for temporary numbers
                tempX = roomOriginX;
                tempZ = roomOriginZ;
                
                // decide whether to increase x or z
                int verticalOrHorizontal = Random.Range(1, 3);
                // decide whether to increase or decrease either x or z, depending on which is chosen first
                int increaseOrDecrease = Random.Range(1, 3);

                // if verticalOrHorizontal is 1, it will alter the x axis, which moves the room left or right
                if (verticalOrHorizontal == 1)

                {

                    // if leftOrRight is one, increase x axis by the roomWidth
                    if (increaseOrDecrease == 1)

                    {

                        tempX = roomOriginX + roomWidth;

                    }

                    // else if leftOrRight is two, decrease x axis by the roomWidth
                    else if (increaseOrDecrease == 2)
                
                    {
                
                        tempX = roomOriginX - roomWidth;

                    }

                }

                // if verticalOrHorizontal is 2, it will alter the z axis, which moves the room forward and backwards
                else if (verticalOrHorizontal == 2)

                {

                    if (increaseOrDecrease == 1)

                    {

                        tempZ = roomOriginZ + roomWidth;

                    }

                    // else if leftOrRight is two, decrease x axis by the roomWidth
                    else if (increaseOrDecrease == 2)
                
                    {
                
                        tempZ = roomOriginZ - roomWidth;

                    }

                }

                // once the new coordinate is generated, check the new coordinate against the rest of the arrays to make sure it's unique
                if (CheckForDuplicates() == false)

                {
                    
                    // create new coordinate
                    xCoordinates[roomCount] = tempX;
                    zCoordinates[roomCount] = tempZ;
                    roomCount++;

                    // set the temporary value to be the actual value
                    roomOriginX = tempX;
                    roomOriginZ = tempZ;
                    
                    // and finally set generating number boolean to false so that this loop is exited and re-entered until the desired amount of coordinates are generated
                    isGenerating = false;
                    lastSuccessfulIteration = currentIteration + 1;

                }

                // each time the loop is iterated through, increase the iteration count
                currentIteration++;

                // if the current iteration is more than the last successful plus the failsafe threshold (configured in the editor)
                if (currentIteration > lastSuccessfulIteration + failsafeThreshold)

                {

                    // get a new random coordinate. the values for this can be between zero and the amount of successfully added rooms
                    int failsafe = Random.Range(0, roomCount);
                    // set the x origin to the index of the failsafe number
                    roomOriginX = xCoordinates[failsafe];
                    // do the same for z
                    roomOriginZ = zCoordinates[failsafe];
                    // set the last successful iteration to current, so that the new coordinate has a chance to expand
                    lastSuccessfulIteration = currentIteration;
                    // set is generating to false to break out of the while loop and start again
                    isGenerating = false;

                }

            }

        }

        // repeat for secret rooms
        xSecretCoordinates = new float[maxSecretRooms];
        zSecretCoordinates = new float[maxSecretRooms];
        
        while (secretRoomCount < maxSecretRooms)

        {

            isGenerating = true;
            
            while (isGenerating)

            {
                
                int connectedRoom = Random.Range(0, xCoordinates.Length);
                roomOriginX = xCoordinates[connectedRoom];
                roomOriginZ = zCoordinates[connectedRoom];

                int loopCounter = 0;
                while (loopCounter < 4)

                {

                    // set the query coordinates, 
                    if (loopCounter == 0)

                    {
                        
                        // query north
                        tempX = roomOriginX;
                        tempZ = roomOriginZ - roomWidth;
                        
                    }
                    
                    else if (loopCounter == 1)

                    {
                        
                        // query east
                        tempX = roomOriginX - roomWidth;
                        tempZ = roomOriginZ;
                        
                    }
                    
                    else if (loopCounter == 3)

                    {
                        
                        // query south
                        tempX = roomOriginX;
                        tempZ = roomOriginZ + roomWidth;
                        
                    }
                    
                    else

                    {
                        
                        // query west
                        tempX = roomOriginX + roomWidth;
                        tempZ = roomOriginZ;
                        
                    }
                    
                    if (CheckForDuplicates() == false)

                    {
                    
                        xSecretCoordinates[secretRoomCount] = tempX;
                        zSecretCoordinates[secretRoomCount] = tempZ;
                        secretRoomCount++;
                        isGenerating = false;
                        // set loop counter to 4 to break out of the while loop
                        loopCounter = 4;
                        
                    }

                    loopCounter++;

                }

            }

        }
        
    }
    
    private void InstantiateRooms()

    {

        for (int i = 0; i < maxRooms; i++)

        {

            // set the x room origin to be the value indexed by i
            roomOriginX = xCoordinates[i];
            // do the same for z
            roomOriginZ = zCoordinates[i];

            // instantiate the prefab
            if (roomPrefab != null)

            {
                
                // create a new game object called room, instantiate the prefab
                GameObject room = Instantiate(roomPrefab, new Vector3(roomOriginX, roomBottom, roomOriginZ), transform.rotation);
                // set the room name to be Room X plus one to make it start counting from one and F plus floor count, i.e., Room 1 F1
                room.name = "Room " + (i + 1) + " F" + floorCount;
                // parent this to the level generator game object to clear up the hierarchy
                room.transform.SetParent(LevelGeneratorObject.transform);
                // and add it to the list of rooms, using i as an index
                roomList[i] = room;

            }

            // else, make a new prefab from scratch if there isn't one already
            else
            
            {
            
                // assign a new empty game object to roomPrefab
                roomPrefab = new GameObject("Room " + 1 + " F" + floorCount);
                // and add the RoomPrefab script to it
                roomPrefab.AddComponent<RoomPrefab>();
                // set the tag of this object to be Level
                roomPrefab.tag = "Level";
                // set the room to be static as it won't be moved during gameplay, sadly, this doesn't have the added benefit of potential performance increase as its done at run time.
                // however, at least this will prevent any developer from accidentally moving the rooms while messing around with this in the editor
                roomPrefab.isStatic = true;
                // set the room to be a parent of the level generator object
                roomPrefab.transform.SetParent(LevelGeneratorObject.transform);
                // add the room prefab to the room list
                roomList[i] = roomPrefab;

            }

        }

        // once all the rooms are done, set this to true so that the algorithm will be able to move on to doors
        roomsReady = true;

    }

    private void InstantiateSecretRooms()

    {
        
        // for loop for creating the rooms
        for (int i = 0; i < maxSecretRooms; i++)

        {

            // x coordinates using secret coordinates
            roomOriginX = xSecretCoordinates[i];
            // same for z
            roomOriginZ = zSecretCoordinates[i];
            
            // instantiate the prefab, no need to create a prefab as it was already created by regular rooms
            GameObject secretRoom = Instantiate(roomPrefab, new Vector3(roomOriginX, roomBottom, roomOriginZ), transform.rotation);
            // set name to secret room plus number and floor
            secretRoom.name = "Secret Room " + (i + 1) + " F" + floorCount;
            // set tag for level, used for raycasting but for things that developers can add, such as jumping by the player colliding with the level
            secretRoom.tag = "Level";
            // set level generator as parent
            secretRoom.transform.SetParent(LevelGeneratorObject.transform);
            // add to room list
            roomList[i + maxRooms] = secretRoom;

        }

        // set this to ready so that doors can start generating if this and normal rooms are true
        secretRoomsReady = true;

    }

    private void CreateDoors()

    {

        // x coordinate for the ray
        float rayX;
        // z coordinate
        float rayZ;
        // temporary object for raycasting
        GameObject raycastOrigin = new GameObject();
        // output of the raycast hit
        RaycastHit hit;
        
        // secret rooms first
        for (int i = 0; i < secretRoomCount; i++)

        {
            
            // create a second for loop to prevent all rooms being made at once

            for (int x = 0; x < 4; x++)

            {

                // all 4 directions are the same except the raycast origin will be in a different place and the placement point will be different too
                
                if (x == 0)

                {
                    
                    // north door

                    // set coordinates to be slightly in front of the doorway and in the middle of it
                    rayX = xSecretCoordinates[i] + (roomWidth / 2);
                    rayZ = zSecretCoordinates[i] + (roomWidth - (wallWidth));
                    placementPoint = new Vector3(rayX, (roomBottom + (doorHeight / 2)), rayZ + wallWidth);
                    // perform raycast and spawn door or wall
                    raycastOrigin.transform.position = new Vector3(rayX, (roomBottom + (doorHeight / 2)), rayZ);

                    // cast a ray, starting at raycast object and going forward and down with distance of door height, store output to hit
                    if (Physics.Raycast(raycastOrigin.transform.position, transform.TransformDirection(0, -1, 1), out hit,
                        doorHeight))

                    {
                        
                        // if a different room is hit, create a wall because this is a secret room
                        // functionality for this room to be destructible isn't enabled
                        if (hit.collider.CompareTag("Level"))

                        {

                            if (hit.transform.position.y == roomBottom)

                            {
                        
                                GameObject wallPrefab = new GameObject("Wall");
                                doorWallParentTo = wallPrefab;
                                // angle for rotation
                                rotationAngle = 0;
                                wallPrefab.tag = "DoorWall";
                                wallPrefab.transform.position = placementPoint;
                                // set parent to be in the room list at index i plus max rooms to be indexed for secret rooms, true tells the program to keep the door where it is rather than move it to the parent
                                wallPrefab.transform.SetParent(roomList[i + maxRooms].transform, true);
                                doorsAndWalls.Add(wallPrefab);
                    
                            }

                        }
                        
                        else if (hit.collider.CompareTag("DoorWall"))

                        {
                            
                            // do nothing
                            
                        }
                
                    }

                    else

                    {
                        
                        GameObject wallPrefab = new GameObject("Wall");
                        doorWallParentTo = wallPrefab;
                        rotationAngle = 0;
                        wallPrefab.tag = "DoorWall";
                        wallPrefab.transform.position = placementPoint;
                        wallPrefab.transform.SetParent(roomList[i + maxRooms].transform, true);
                        doorsAndWalls.Add(wallPrefab);

                    }
                    
                }
                
                else if (x == 1)

                {
                
                    // east door
                    
                    rayX = xSecretCoordinates[i] + (roomWidth - wallWidth);
                    rayZ = zSecretCoordinates[i] + (roomWidth / 2);
                    placementPoint = new Vector3(rayX + wallWidth, (roomBottom + (doorHeight / 2)), rayZ);
                    // perform raycast and spawn door or wall
                    raycastOrigin.transform.position = new Vector3(rayX, (roomBottom + (doorHeight / 2)), rayZ);

                    // cast a ray, starting at raycast object and going right and down with distance of door height, store output to hit
                    if (Physics.Raycast(raycastOrigin.transform.position, transform.TransformDirection(1, -1, 0), out hit,
                        doorHeight))

                    {
                        
                        // if a different room is hit, create a wall because this is a secret room
                        if (hit.collider.CompareTag("Level"))

                        {

                            if (hit.transform.position.y == roomBottom)

                            {
                        
                                GameObject wallPrefab = new GameObject("Wall");
                                doorWallParentTo = wallPrefab;
                                rotationAngle = 90;
                                wallPrefab.tag = "DoorWall";
                                wallPrefab.transform.position = placementPoint;
                                wallPrefab.transform.SetParent(roomList[i + maxRooms].transform, true);
                                doorsAndWalls.Add(wallPrefab);
                    
                            }

                        }
                        
                        else if (hit.collider.CompareTag("DoorWall"))

                        {
                            
                            // do nothing
                            
                        }
                
                    }

                    else

                    {
                        
                        GameObject wallPrefab = new GameObject("Wall");
                        doorWallParentTo = wallPrefab;
                        rotationAngle = 90;
                        wallPrefab.tag = "DoorWall";
                        wallPrefab.transform.position = placementPoint;
                        wallPrefab.transform.SetParent(roomList[i + maxRooms].transform, true);
                        doorsAndWalls.Add(wallPrefab);

                    }
                    
                }

                else if (x == 2)

                {
                    
                    // south door
            
                    rayX = xSecretCoordinates[i] + (roomWidth / 2);
                    rayZ = zSecretCoordinates[i] + wallWidth;
                    placementPoint = new Vector3(rayX - wallWidth, (roomBottom + (doorHeight / 2)), rayZ);
                    // perform raycast and spawn door or wall
                    raycastOrigin.transform.position = new Vector3(rayX, (roomBottom + (doorHeight / 2)), rayZ);

                    // cast a ray, starting at raycast object and going back and down with distance of door height, store output to hit
                    if (Physics.Raycast(raycastOrigin.transform.position, transform.TransformDirection(0, -1, -1), out hit,
                        doorHeight))

                    {
                        
                        // if a different room is hit, create a wall because this is a secret room
                        if (hit.collider.CompareTag("Level"))

                        {

                            if (hit.transform.position.y == roomBottom)

                            {
                        
                                GameObject wallPrefab = new GameObject("Wall");
                                doorWallParentTo = wallPrefab;
                                rotationAngle = 180;
                                wallPrefab.tag = "DoorWall";
                                wallPrefab.transform.position = placementPoint;
                                wallPrefab.transform.SetParent(roomList[i + maxRooms].transform, true);
                                doorsAndWalls.Add(wallPrefab);
                    
                            }

                        }
                        
                        else if (hit.collider.CompareTag("DoorWall"))

                        {
                            
                            // do nothing
                            
                        }
                
                    }

                    else

                    {
                        
                        GameObject wallPrefab = new GameObject("Wall");
                        doorWallParentTo = wallPrefab;
                        rotationAngle = 180;
                        wallPrefab.tag = "DoorWall";
                        wallPrefab.transform.position = placementPoint;
                        wallPrefab.transform.SetParent(roomList[i + maxRooms].transform, true);
                        doorsAndWalls.Add(wallPrefab);

                    }

                }
             
                else

                {
                
                    // west door
            
                    rayX = xSecretCoordinates[i] + wallWidth;
                    rayZ = zSecretCoordinates[i] + (roomWidth / 2);
                    placementPoint = new Vector3(rayX - wallWidth, (roomBottom + (doorHeight / 2)), rayZ);
                    // perform raycast and spawn door or wall
                    raycastOrigin.transform.position = new Vector3(rayX, (roomBottom + (doorHeight / 2)), rayZ);

                    // cast a ray, starting at raycast object and going left and down with distance of door height, store output to hit
                    if (Physics.Raycast(raycastOrigin.transform.position, transform.TransformDirection(-1, -1, 0), out hit,
                        doorHeight))

                    {

                        // if a different room is hit, create a wall because this is a secret room
                        if (hit.collider.CompareTag("Level"))

                        {

                            if (hit.transform.position.y == roomBottom)

                            {
                        
                                GameObject wallPrefab = new GameObject("Wall");
                                doorWallParentTo = wallPrefab;
                                rotationAngle = 270;
                                wallPrefab.tag = "DoorWall";
                                wallPrefab.transform.position = placementPoint;
                                wallPrefab.transform.SetParent(roomList[i + maxRooms].transform, true);
                                doorsAndWalls.Add(wallPrefab);
                    
                            }

                        }
                        
                        else if (hit.collider.CompareTag("DoorWall"))

                        {
                            
                            // do nothing
                            
                        }
                
                    }

                    else

                    {
                        
                        GameObject wallPrefab = new GameObject("Wall");
                        doorWallParentTo = wallPrefab;
                        rotationAngle = 270;
                        wallPrefab.tag = "DoorWall";
                        wallPrefab.transform.position = placementPoint;
                        wallPrefab.transform.SetParent(roomList[i + maxRooms].transform, true);
                        doorsAndWalls.Add(wallPrefab);
                        

                    }

                }

            }

        }
        
        // and also for normal rooms

        for (int j = 0; j < roomCount; j++)

        {
            
            // create a second for loop to prevent all rooms being made at once

            for (int y = 0; y < 4; y++)

            {

                if (y == 0)

                {
                    
                    // north door

                    // set coordinates to be slightly in front of the doorway and in the middle of it
                    rayX = xCoordinates[j] + (roomWidth / 2);
                    rayZ = zCoordinates[j] + (roomWidth - (wallWidth));
                    placementPoint = new Vector3(rayX, (roomBottom + (doorHeight / 2)), rayZ + wallWidth);
                    // perform raycast and spawn door or wall
                    raycastOrigin.transform.position = new Vector3(rayX, (roomBottom + (doorHeight / 2)), rayZ);

                    // cast a ray, starting at raycast object and going forward and down with distance of door height, store output to hit
                    if (Physics.Raycast(raycastOrigin.transform.position, transform.TransformDirection(0, -1, 1), out hit,
                        doorHeight))

                    {
                        
                        // if a different room is hit, create a door
                        if (hit.collider.CompareTag("Level"))
                            
                        {
                            
                            if (hit.transform.position.y == roomBottom)

                            {
                        
                                GameObject doorPrefab = new GameObject("Door");
                                doorWallParentTo = doorPrefab;
                                rotationAngle = 0;
                                doorPrefab.tag = "DoorWall";
                                doorPrefab.transform.position = placementPoint;
                                doorPrefab.transform.SetParent(roomList[j].transform, true);
                                doorsAndWalls.Add(doorPrefab);
                    
                            }

                        }
                        
                        else if (hit.collider.CompareTag("DoorWall"))

                        {
                            
                            // do nothing
                            
                        }
                
                    }

                    else

                    {
                        
                        GameObject wallPrefab = new GameObject("Wall");
                        doorWallParentTo = wallPrefab;
                        rotationAngle = 0;
                        wallPrefab.tag = "DoorWall";
                        wallPrefab.transform.position = placementPoint;
                        wallPrefab.transform.SetParent(roomList[j].transform, true);
                        doorsAndWalls.Add(wallPrefab);

                    }
                    
                }
                
                else if (y == 1)

                {
                
                    // east door

                    rayX = xCoordinates[j] + (roomWidth - wallWidth);
                    rayZ = zCoordinates[j] + (roomWidth / 2);
                    placementPoint = new Vector3(rayX + wallWidth, (roomBottom + (doorHeight / 2)), rayZ);
                    // perform raycast and spawn door or wall
                    raycastOrigin.transform.position = new Vector3(rayX, (roomBottom + (doorHeight / 2)), rayZ);

                    // cast a ray, starting at raycast object and going right and down with distance of door height, store output to hit
                    if (Physics.Raycast(raycastOrigin.transform.position, transform.TransformDirection(1, -1, 0), out hit,
                        doorHeight))

                    {
                        
                        // if a different room is hit, create a door
                        if (hit.collider.CompareTag("Level"))

                        {
                            
                            if (hit.transform.position.y == roomBottom)

                            {
                        
                                GameObject doorPrefab = new GameObject("Door");
                                doorWallParentTo = doorPrefab;
                                rotationAngle = 90;
                                doorPrefab.tag = "DoorWall";
                                doorPrefab.transform.position = placementPoint;
                                doorPrefab.transform.SetParent(roomList[j].transform, true);
                                doorsAndWalls.Add(doorPrefab);
                    
                            }

                        }
                        
                        else if (hit.collider.CompareTag("DoorWall"))

                        {
                            
                            // do nothing
                            
                        }
                
                    }

                    else

                    {
                        
                        GameObject wallPrefab = new GameObject("Wall");
                        doorWallParentTo = wallPrefab;
                        rotationAngle = 90;
                        wallPrefab.tag = "DoorWall";
                        wallPrefab.transform.position = placementPoint;
                        wallPrefab.transform.SetParent(roomList[j].transform, true);
                        doorsAndWalls.Add(wallPrefab);

                    }
                    
                }

                else if (y == 2)

                {
                    
                    // south door
            
                    rayX = xCoordinates[j] + (roomWidth / 2);
                    rayZ = zCoordinates[j] + wallWidth;
                    placementPoint = new Vector3(rayX - wallWidth, (roomBottom + (doorHeight / 2)), rayZ);
                    // perform raycast and spawn door or wall
                    raycastOrigin.transform.position = new Vector3(rayX, (roomBottom + (doorHeight / 2)), rayZ);

                    // cast a ray, starting at raycast object and going back and down with distance of door height, store output to hit
                    if (Physics.Raycast(raycastOrigin.transform.position, transform.TransformDirection(0, -1, -1), out hit,
                        doorHeight))

                    {
                        
                        // if a different room is hit, create a door
                        if (hit.collider.CompareTag("Level"))

                        {
                            
                            if (hit.transform.position.y == roomBottom)

                            {
                        
                                GameObject doorPrefab = new GameObject("Door");
                                doorWallParentTo = doorPrefab;
                                rotationAngle = 180;
                                doorPrefab.tag = "DoorWall";
                                doorPrefab.transform.position = placementPoint;
                                doorPrefab.transform.SetParent(roomList[j].transform, true);
                                doorsAndWalls.Add(doorPrefab);
                    
                            }

                        }
                        
                        else if (hit.collider.CompareTag("DoorWall"))

                        {
                            
                            // do nothing
                            
                        }
                
                    }

                    else

                    {
                        
                        GameObject wallPrefab = new GameObject("Wall");
                        doorWallParentTo = wallPrefab;
                        rotationAngle = 180;
                        wallPrefab.tag = "DoorWall";
                        wallPrefab.transform.position = placementPoint;
                        wallPrefab.transform.SetParent(roomList[j].transform, true);
                        doorsAndWalls.Add(wallPrefab);

                    }

                }
             
                else

                {
                
                    // west door
            
                    rayX = xCoordinates[j] + wallWidth;
                    rayZ = zCoordinates[j] + (roomWidth / 2);
                    placementPoint = new Vector3(rayX - wallWidth, (roomBottom + (doorHeight / 2)), rayZ);
                    // perform raycast and spawn door or wall
                    raycastOrigin.transform.position = new Vector3(rayX, (roomBottom + (doorHeight / 2)), rayZ);

                    // cast a ray, starting at raycast object and going left and down with distance of door height, store output to hit
                    if (Physics.Raycast(raycastOrigin.transform.position, transform.TransformDirection(-1, -1, 0), out hit,
                        doorHeight))

                    {

                        // if a different room is hit, create a door
                        if (hit.collider.CompareTag("Level"))

                        {
                            
                            if (hit.transform.position.y == roomBottom)

                            {
                        
                                GameObject doorPrefab = new GameObject("Door");
                                doorWallParentTo = doorPrefab;
                                rotationAngle = 270;
                                doorPrefab.tag = "DoorWall";
                                doorPrefab.transform.position = placementPoint;
                                doorPrefab.transform.SetParent(roomList[j].transform, true);
                                doorsAndWalls.Add(doorPrefab);
                    
                            }

                        }
                        
                        else if (hit.collider.CompareTag("DoorWall"))

                        {
                            
                            // do nothing
                            
                        }
                
                    }

                    else

                    {
                        
                        GameObject wallPrefab = new GameObject("Wall");
                        doorWallParentTo = wallPrefab;
                        rotationAngle = 270;
                        wallPrefab.tag = "DoorWall";
                        wallPrefab.transform.position = placementPoint;
                        wallPrefab.transform.SetParent(roomList[j].transform, true);
                        doorsAndWalls.Add(wallPrefab);
                        

                    }

                }

            }

        }
        
        // once raycasting is done, delete the temporary game object as it is no longer needed
        Destroy(raycastOrigin);

    }

    private void CreateInteriors()

    {
        
        for (int i = 0; i < maxRooms; i++)

        {

            roomOriginX = xCoordinates[i];
            roomOriginZ = zCoordinates[i];
            
            // instantiate the prefab
            if (roomInterior != null)

            {
                
                GameObject interior = Instantiate(roomInterior, new Vector3((roomOriginX + (roomWidth / 2)), roomBottom, roomOriginZ + (roomWidth / 2)), transform.rotation);
                interior.name = "Interior";
                interior.transform.SetParent(roomList[i].transform, true);

            }

            // else, make a new prefab from scratch if there isn't one already
            else
            
            {
            
                // assign a new empty game object to roomPrefab
                roomInterior = new GameObject("Interior");
                roomInterior.transform.position = new Vector3((roomOriginX + (roomWidth / 2)), roomBottom, roomOriginZ + (roomWidth / 2));
                // add a light as a child object
                GameObject lightObject = new GameObject("Lamp");
                // create a light by adding the light component
                Light lightComponents = lightObject.AddComponent<Light>();
                // set it to be at the top of the room, slightly below the ceiling
                lightObject.transform.position = new Vector3(0, (roomTop - 0.1f), 0);
                // set the intensity of the light, edited in editor
                lightComponents.intensity = lightIntensity;
                // set the range to scale with the room width
                lightComponents.range = roomWidth;
                // parent to interior
                lightObject.transform.SetParent(roomInterior.transform, false);
                // parent to room in index
                roomInterior.transform.SetParent(roomList[i].transform, true);

            }

        }
        
        // repeat for secret rooms

        for (int j = 0; j < maxSecretRooms; j++)

        {
            
            roomOriginX = xSecretCoordinates[j];
            roomOriginZ = zSecretCoordinates[j];
            
            GameObject secretRoomInterior = Instantiate(roomInterior, new Vector3((roomOriginX + (roomWidth / 2)), roomBottom, roomOriginZ + (roomWidth / 2)), transform.rotation);
            secretRoomInterior.name = "Interior";
            secretRoomInterior.transform.SetParent(roomList[j + maxRooms].transform, true);

        }
        
    }

    private bool CheckForDuplicates()

    {

        // iterate through the already added rooms
        for (int i = 0; i < roomCount; i++)

        {
            
            // if x or z matches an item in the array
            if (tempX == xCoordinates[i] && tempZ == zCoordinates[i])

            {
                
                // return true and don't add it to the array
                return true;

            }
            
        }

        // if there is more than one secret room index secret rooms also
        // made this more than one so that basic rooms wouldn't have to iterate through this as the default order is standard rooms are made before secret ones
        // lastly, secret rooms have to iterate through both these if there is more than one secret room
        if (secretRoomCount > 1)

        {
            
            for (int j = 0; j < secretRoomCount; j++)

            {
            
                // if x or z match an item in the index
                if (tempX == xSecretCoordinates[j] && tempZ == zSecretCoordinates[j])

                {
                
                    // return false and don't add
                    return true;

                }
            
            }
            
        }

        // if the program went through both without returning true, the new room isn't a duplicate and can be added
        return false;

    }

    private void ErrorChecks()

    {

        // error checks to prevent weird behaviour, such as the door frame clipping into rooms on higher floors because its taller than the room
        
        if (doorHeight > roomHeight)

        {

            Debug.LogError("ERROR: Door height can't be greater than room height.");
            UnityEditor.EditorApplication.isPlaying = false;

        }

        else if (doorWidth > roomWidth)

        {

            Debug.LogError("ERROR: Door width can't be greater than room width.");
            UnityEditor.EditorApplication.isPlaying = false;

        }

        // if no errors were found, the algorithm can start
        else

        {
            
            readyToStart = true;

        }

    }

}