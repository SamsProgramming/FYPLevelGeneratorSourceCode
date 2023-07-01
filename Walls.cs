using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walls : MonoBehaviour

{
    
    private float wallWidth;
    private float doorWidth;
    private float doorHeight;

    private bool destructible = false;
    private Material wallMaterial;
    private GameObject wallObject;
    private GameObject doorWallParentTo;
    private Vector3 placementPoint;
    private float rotationAngle;

    private LevelGenerator levelGeneratorScript;
    
    // Start is called before the first frame update
    private void Start()

    {

        //reference level generator
        levelGeneratorScript = GameObject.Find("Level Generator").GetComponent<LevelGenerator>();
        
        // set all these variables to match their values in the level generator
        wallWidth = levelGeneratorScript.wallWidth;
        doorWidth = levelGeneratorScript.doorWidth;
        doorHeight = levelGeneratorScript.doorHeight;
        wallMaterial = levelGeneratorScript.materials[2];
        doorWallParentTo = levelGeneratorScript.doorWallParentTo;
        placementPoint = levelGeneratorScript.placementPoint;
        rotationAngle = levelGeneratorScript.rotationAngle;

        // creating a wall
        // create a new object using a primitive cube
        wallObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        // call it wall
        wallObject.name = "Wall";
        // code is the same as door, get its placement point, set rotation, set scale, material, tag
        wallObject.transform.position = placementPoint;
        wallObject.transform.rotation = Quaternion.Euler(0, rotationAngle, 0);
        wallObject.transform.localScale = new Vector3(doorWidth, doorHeight, wallWidth);
        wallObject.GetComponent<MeshRenderer>().material = wallMaterial;
        wallObject.tag = "DoorWall";

        // parents to doorWallParentTo
        // problem with this, doing this inside start only referenced last game object referenced and didn't create the wall and reference the last one as I wanted
        // this in update is as broken as in start, at least it doesn't clutter up the hierarchy so much
        wallObject.transform.SetParent(doorWallParentTo.transform, true);

    }

    // if an object is colliding with the wall
    private void OnCollisionEnter(Collision other)
    
    {

        // and the object is the player
        if (GameObject.FindWithTag("Player"))

        {

            // if E is pressed
            if (Input.GetKeyDown(KeyCode.E))

            {

                // and if the wall is destructible
                if (destructible == true)

                {

                    // destroy the wall
                    Destroy(gameObject);

                }
                
            }
            
        }
        
    }
}