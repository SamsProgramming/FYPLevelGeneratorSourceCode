using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Doors : MonoBehaviour

{

    private float doorWidth;
    private float doorHeight;
    private float wallWidth;
    private float doorSpeed;
    private Vector3 startPosition;
    private Vector3 moveTo;
    private Vector3 leftStartPosition;
    private Vector3 rightStartPosition;
    private Vector3 leftMoveTo;
    private Vector3 rightMoveTo;
    public bool doorOpen;
    
    private GameObject doorWallParentTo;
    private GameObject doorObject;
    private GameObject doorLeft;
    private GameObject doorRight;
    private Vector3 placementPoint;
    private Material doorMaterial;
    
    private LevelGenerator levelGeneratorScript;
    private LevelGenerator.DoorType doorType;
    
    void Start()

    {

        levelGeneratorScript = GameObject.Find("Level Generator").GetComponent<LevelGenerator>();
        // get the current door type from the level generator script
        doorType = levelGeneratorScript.doorType;
        // get the appropriate sizes from the level generator script
        doorWidth = levelGeneratorScript.doorWidth;
        doorHeight = levelGeneratorScript.doorHeight;
        wallWidth = levelGeneratorScript.wallWidth;
        doorSpeed = levelGeneratorScript.doorSpeed;
        // get the 4th material from the materials array of the level generator, as this one is responsible for door frames and doors
        doorMaterial = levelGeneratorScript.materials[3];
        placementPoint = levelGeneratorScript.placementPoint;
        // and lastly get the door prefab so that the door created by this script can become a child of it
        doorWallParentTo = levelGeneratorScript.doorWallParentTo;

        // logic is the same for both left and right sliding doors
        if (doorType == LevelGenerator.DoorType.SlidingSingleLeft || doorType == LevelGenerator.DoorType.SlidingSingleRight)

        {
            
            // create the object which is the door, using a primitive cube
            doorObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            // call this the door object
            doorObject.name = "Door Object";
            // set its position to the placement point
            doorObject.transform.position = placementPoint;
            // set its scale so that it fits the door frame
            doorObject.transform.localScale = new Vector3(doorWidth, doorHeight, (wallWidth / 2));
            // get the material from the array list and set it for this object
            doorObject.GetComponent<MeshRenderer>().material = doorMaterial;
            // tag it DoorWall
            doorObject.tag = "DoorWall";
            // create a box collider to be used for a trigger
            BoxCollider doorTrigger = gameObject.AddComponent<BoxCollider>();
            // set is trigger to true
            doorTrigger.isTrigger = true;
            // set its size so that it rests in the middle of the door and can be triggered from each side
            doorTrigger.size = new Vector3(doorWidth, (doorHeight / 10), (wallWidth / 2) + 2);
            // center it at the bottom of the door, above the ground so that the player can touch it
            doorTrigger.center = new Vector3(0, -(doorHeight / 2) + (doorHeight / 20), 0);
            // set its current position to be its starting position
            startPosition = transform.position;

            // if the door type is left, set its MoveTo to be left or where it starts
            if (doorType == LevelGenerator.DoorType.SlidingSingleLeft)

            {
                
                moveTo = new Vector3(transform.position.x + doorWidth, transform.position.y, transform.position.z);
                
            }
            
            // else, right of where it starts
            else if (doorType == LevelGenerator.DoorType.SlidingSingleLeft)

            {
                
                moveTo = new Vector3(transform.position.x - doorWidth, transform.position.y, transform.position.z);
                
            }
            
            // error with this code: it will move to its direction regardless of rotation
            
            // set the door object to be a child of the door prefab
            doorObject.transform.SetParent(doorWallParentTo.transform, false);

        }
        
        /*
         
         // code that was going to be used for double doors, left out, too many errors to fix
         
        else if (doorType == LevelGenerator.DoorType.SlidingDouble)

        {

            // create an empty holder object
            doorObject = new GameObject("Door Object");
            
            doorLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorLeft.name = "Left Door";
            doorLeft.transform.localScale = new Vector3((doorWidth / 2), doorHeight, (wallWidth / 2));
            doorLeft.transform.position = new Vector3(0 + (doorWidth / 4), 0, 0);
            doorLeft.GetComponent<MeshRenderer>().material = doorMaterial;
            leftStartPosition = transform.position;
            leftMoveTo = new Vector3(leftStartPosition.x - (doorWidth / 2), transform.position.y, transform.position.z);
            // set the left door to be a child of the door object
            doorLeft.transform.SetParent(doorObject.transform, false);
            
            doorRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorRight.name = "Right Door";
            doorRight.transform.localScale = new Vector3(doorWidth / 2, doorHeight, (wallWidth / 2));
            doorRight.transform.position = new Vector3(0 - (doorWidth / 4), transform.position.y, transform.position.z);
            doorRight.GetComponent<MeshRenderer>().material = doorMaterial;
            rightStartPosition = transform.position;
            rightMoveTo = new Vector3(transform.position.x - (doorWidth / 2), transform.position.y, transform.position.z);
            // set the right door to be a child of the door object
            doorRight.transform.SetParent(doorObject.transform, false);
            
            BoxCollider doorTrigger = gameObject.AddComponent<BoxCollider>();
            doorTrigger.isTrigger = true;
            doorTrigger.size = new Vector3(doorWidth, (doorHeight / 10), (wallWidth / 2) + 2);
            doorTrigger.center = new Vector3(0, -(doorHeight / 2) + (doorHeight / 20), 0);
            
            // set the door object to be a child of the door prefab
            doorObject.transform.SetParent(doorPrefab.transform, false);

        }
        */

    }

    private void Update()

    {

        // logic for both left and right doors
        if (doorType == LevelGenerator.DoorType.SlidingSingleLeft || doorType == LevelGenerator.DoorType.SlidingSingleRight)

        {
            
            // if the door is open, move object towards its goal
            if (doorOpen == true)

            {
            
                doorObject.transform.position = Vector3.MoveTowards(doorObject.transform.position, moveTo, (doorSpeed * Time.deltaTime));
            
            }

            // if it isn't open and the position isn't where it started, move it to that position
            else

            {

                if (doorObject.transform.position != startPosition)

                {
                
                    doorObject.transform.position = Vector3.MoveTowards(doorObject.transform.position, startPosition, (doorSpeed * Time.deltaTime));
                
                }

            }
            
        }

        /*
         
         // logic for double doors
         
        else
        
        {
        
            if (doorOpen == true)

            {
            
                doorLeft.transform.position = Vector3.MoveTowards(doorLeft.transform.position, leftMoveTo, (doorSpeed * Time.deltaTime));
                doorRight.transform.position = Vector3.MoveTowards(doorRight.transform.position, leftMoveTo, (doorSpeed * Time.deltaTime));
            
            }

            else

            {

                if (doorLeft.transform.position != leftStartPosition || doorRight.transform.position != rightStartPosition)

                {
                
                    doorLeft.transform.position = Vector3.MoveTowards(doorLeft.transform.position, leftStartPosition, (doorSpeed * Time.deltaTime));
                    doorRight.transform.position = Vector3.MoveTowards(doorRight.transform.position, rightStartPosition, (doorSpeed * Time.deltaTime));
                
                }

            }
            
        }
        
        */

    }

    // when the player enters the trigger, open the door
    private void OnTriggerEnter(Collider other)
    
    {
        
        if (GameObject.FindWithTag("Player"))

        {

            doorOpen = true;

        }

    }

    // once the player exits the trigger, close it
    private void OnTriggerExit(Collider other)
    
    {
        
        doorOpen = false;
        
    }
    
}