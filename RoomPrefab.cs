using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class RoomPrefab : MonoBehaviour

{

    private float roomWidth;
    private float wallWidth;
    private float doorHeight;
    private float doorWidth;
    private float roomHeight;

    private Material[] roomMaterials = new Material[4];
    private Mesh mesh;
    private LevelGenerator levelGeneratorScript;

    void Start()

    {

        levelGeneratorScript = GameObject.Find("Level Generator").GetComponent<LevelGenerator>();

        // assign the room width, wall width, door height, door width and room height to match the values from the level generator script
        roomWidth = levelGeneratorScript.roomWidth;
        wallWidth = levelGeneratorScript.wallWidth;
        doorHeight = levelGeneratorScript.doorHeight;
        doorWidth = levelGeneratorScript.doorWidth;
        roomHeight = levelGeneratorScript.roomHeight;

        // get materials from level generator
        for (int i = 0; i < roomMaterials.Length; i++)

        {
            
            roomMaterials[i] = levelGeneratorScript.materials[i];
            
        }

        CreateRoom();

    }
    
    private void CreateRoom()

    {

        // add a mesh renderer
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        // get the material from array
        GetComponent<MeshRenderer>().materials = roomMaterials;
        // add a mesh filter
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();

        // create a new mesh and call it room
        mesh = new Mesh();
        mesh.name = "Room";

        CreateVertices();
        CreateTriangles();

        // recalculate the normals after the mesh is created from triangles
        mesh.RecalculateNormals();
        // set the mesh filter to be the mesh
        meshFilter.mesh = mesh;
        // recalculate the bounds of the mesh once submeshes are added
        mesh.RecalculateBounds();
        
        CreateColliders();

    }

    private void CreateVertices()

    {
        
        // create a vector 3 array holding all 80 vertices needed
        // this includes duplicates as without duplicates it had a graphical issue
        Vector3[] vertices = new Vector3[80]

        {

            // floor - 0, 1, 2, 3
            new Vector3(0, 0, 0),
            new Vector3((0 + roomWidth), 0, 0),
            new Vector3(0, 0, (0 + roomWidth)),
            new Vector3((0 + roomWidth), 0, (0 + roomWidth)),
            
            //roof - 4, 5, 6, 7
            new Vector3(0, roomHeight, 0),
            new Vector3((0 + roomWidth), roomHeight, 0),
            new Vector3(0, roomHeight, (0 + roomWidth)),
            new Vector3((0 + roomWidth), roomHeight, (0 + roomWidth)),
            
            //north wall
            new Vector3((0 + (wallWidth / 2)), 0, (0 + (wallWidth / 2))), 
            new Vector3(((roomWidth / 2) - (doorWidth / 2)), 0, (0 + (wallWidth / 2))), 
            new Vector3((0 + (wallWidth / 2)), roomHeight, (0 + (wallWidth / 2))), 
            new Vector3(((roomWidth / 2) - (doorWidth / 2)), roomHeight, (0 + (wallWidth / 2))), 
            new Vector3((roomWidth / 2) + (doorWidth / 2), 0, (0 + (wallWidth / 2))), 
            new Vector3((roomWidth - (wallWidth / 2)), 0, (0 + (wallWidth / 2))), 
            new Vector3((roomWidth / 2) + (doorWidth / 2), roomHeight, (0 + (wallWidth / 2))), 
            new Vector3((roomWidth - (wallWidth / 2)), roomHeight, (0 + (wallWidth / 2))), 
            new Vector3(((roomWidth / 2) - (doorWidth / 2)), doorHeight, (0 + (wallWidth / 2))), 
            new Vector3((roomWidth / 2) + (doorWidth / 2), doorHeight, (0 + (wallWidth / 2))), 
            
            //east wall
            new Vector3((0 + (wallWidth / 2)), 0, (0 + (wallWidth / 2))),
            new Vector3((0 + (wallWidth / 2)), 0, ((roomWidth / 2) - (doorWidth / 2))),
            new Vector3((0 + (wallWidth / 2)), roomHeight, (0 + (wallWidth / 2))),
            new Vector3((0 + (wallWidth / 2)), roomHeight, ((roomWidth / 2) - (doorWidth / 2))),
            new Vector3((0 + (wallWidth / 2)), 0, ((roomWidth / 2) + (doorWidth / 2))), 
            new Vector3((0 + (wallWidth / 2)), 0, (roomWidth - (wallWidth / 2))), 
            new Vector3((0 + (wallWidth / 2)), roomHeight, (roomWidth / 2) + (doorWidth / 2)), 
            new Vector3((0 + (wallWidth / 2)), roomHeight, (roomWidth - (wallWidth / 2))),
            new Vector3((0 + (wallWidth / 2)), doorHeight, ((roomWidth / 2) - (doorWidth / 2))),
            new Vector3((0 + (wallWidth / 2)), doorHeight, ((roomWidth / 2) + (doorWidth / 2))),

            // south wall
            new Vector3((0 + (wallWidth / 2)), 0, (roomWidth - (wallWidth / 2))), 
            new Vector3(((roomWidth / 2) - (doorWidth / 2)), 0, (roomWidth - (wallWidth / 2))), 
            new Vector3((0 + (wallWidth / 2)), roomHeight, (roomWidth - (wallWidth / 2))), 
            new Vector3(((roomWidth / 2) - (doorWidth / 2)), roomHeight, (roomWidth - (wallWidth / 2))), 
            new Vector3((roomWidth / 2) + (doorWidth / 2), 0, (roomWidth - (wallWidth / 2))), 
            new Vector3((roomWidth - (wallWidth / 2)), 0, (roomWidth - (wallWidth / 2))), 
            new Vector3((roomWidth / 2) + (doorWidth / 2), roomHeight, (roomWidth - (wallWidth / 2))), 
            new Vector3((roomWidth - (wallWidth / 2)), roomHeight, (roomWidth - (wallWidth / 2))), 
            new Vector3(((roomWidth / 2) - (doorWidth / 2)), doorHeight, (roomWidth - (wallWidth / 2))), 
            new Vector3((roomWidth / 2) + (doorWidth / 2), doorHeight, (roomWidth - (wallWidth / 2))),
            
            // west wall
            new Vector3((roomWidth - (wallWidth / 2)), 0, (0 + (wallWidth / 2))),
            new Vector3((roomWidth - (wallWidth / 2)), 0, ((roomWidth / 2) - (doorWidth / 2))),
            new Vector3((roomWidth - (wallWidth / 2)), roomHeight, (0 + (wallWidth / 2))),
            new Vector3((roomWidth - (wallWidth / 2)), roomHeight, ((roomWidth / 2) - (doorWidth / 2))),
            new Vector3((roomWidth - (wallWidth / 2)), 0, (roomWidth / 2) + (doorWidth / 2)), 
            new Vector3((roomWidth - (wallWidth / 2)), 0, (roomWidth - (wallWidth / 2))), 
            new Vector3((roomWidth - (wallWidth / 2)), roomHeight, (roomWidth / 2) + (doorWidth / 2)), 
            new Vector3((roomWidth - (wallWidth / 2)), roomHeight, (roomWidth - (wallWidth / 2))),
            new Vector3((roomWidth - (wallWidth / 2)), doorHeight, ((roomWidth / 2) - (doorWidth / 2))),
            new Vector3((roomWidth - (wallWidth / 2)), doorHeight, (roomWidth / 2) + (doorWidth / 2)), 
            
            // door points, 2, 5, 9, 10
            
            // north door frame
            // room edge
            new Vector3(((roomWidth / 2) - (doorWidth / 2)), 0, 0), 
            new Vector3((roomWidth / 2) + (doorWidth / 2), 0, 0), 
            new Vector3(((roomWidth / 2) - (doorWidth / 2)), doorHeight, 0), 
            new Vector3((roomWidth / 2) + (doorWidth / 2), doorHeight, 0),  
            // wall
            new Vector3(((roomWidth / 2) - (doorWidth / 2)), 0, (0 + (wallWidth / 2))), 
            new Vector3((roomWidth / 2) + (doorWidth / 2), 0, (0 + (wallWidth / 2))), 
            new Vector3(((roomWidth / 2) - (doorWidth / 2)), doorHeight, (0 + (wallWidth / 2))), 
            new Vector3((roomWidth / 2) + (doorWidth / 2), doorHeight, (0 + (wallWidth / 2))),  
            
            // east door frame
            // room edge
            new Vector3(0, 0, ((roomWidth / 2) - (doorWidth / 2))),
            new Vector3(0, 0, ((roomWidth / 2) + (doorWidth / 2))),
            new Vector3(0, doorHeight, ((roomWidth / 2) - (doorWidth / 2))),
            new Vector3(0, doorHeight, ((roomWidth / 2) + (doorWidth / 2))),
            // wall
            new Vector3((0 + (wallWidth / 2)), 0, ((roomWidth / 2) - (doorWidth / 2))),
            new Vector3((0 + (wallWidth / 2)), 0, ((roomWidth / 2) + (doorWidth / 2))), 
            new Vector3((0 + (wallWidth / 2)), doorHeight, ((roomWidth / 2) - (doorWidth / 2))),
            new Vector3((0 + (wallWidth / 2)), doorHeight, ((roomWidth / 2) + (doorWidth / 2))),

            // south door frame
            // room edge
            new Vector3(((roomWidth / 2) - (doorWidth / 2)), 0, roomWidth), 
            new Vector3((roomWidth / 2) + (doorWidth / 2), 0, roomWidth), 
            new Vector3(((roomWidth / 2) - (doorWidth / 2)), doorHeight, roomWidth), 
            new Vector3((roomWidth / 2) + (doorWidth / 2), doorHeight, roomWidth),
            // wall
            new Vector3(((roomWidth / 2) - (doorWidth / 2)), 0, (roomWidth - (wallWidth / 2))), 
            new Vector3((roomWidth / 2) + (doorWidth / 2), 0, (roomWidth - (wallWidth / 2))), 
            new Vector3(((roomWidth / 2) - (doorWidth / 2)), doorHeight, (roomWidth - (wallWidth / 2))), 
            new Vector3((roomWidth / 2) + (doorWidth / 2), doorHeight, (roomWidth - (wallWidth / 2))),

            // west door frame
            // room edge
            new Vector3(roomWidth, 0, ((roomWidth / 2) - (doorWidth / 2))),
            new Vector3(roomWidth, 0, ((roomWidth / 2) + (doorWidth / 2))),
            new Vector3(roomWidth, doorHeight, ((roomWidth / 2) - (doorWidth / 2))),
            new Vector3(roomWidth, doorHeight, ((roomWidth / 2) + (doorWidth / 2))),
            // wall
            new Vector3((roomWidth - (wallWidth / 2)), 0, ((roomWidth / 2) - (doorWidth / 2))),
            new Vector3((roomWidth - (wallWidth / 2)), 0, (roomWidth / 2) + (doorWidth / 2)), 
            new Vector3((roomWidth - (wallWidth / 2)), doorHeight, ((roomWidth / 2) - (doorWidth / 2))),
            new Vector3((roomWidth - (wallWidth / 2)), doorHeight, (roomWidth / 2) + (doorWidth / 2)), 
            
        };

        // set the meshes vertices to be the vertex array
        mesh.vertices = vertices;
        
    }

    private void CreateTriangles()

    {
        
        // create triangles using the vertices
        
        int[] floorTriangles = new int[6]

        {

            // floor
            0, 2, 1,
            2, 3, 1

        };

        int[] roofTriangles = new int[6]

        {
            
            //roof
            6, 4, 5,
            7, 6, 5

        };

        int[] walls = new int [72]

        {

            // north wall
            // first part
            10, 8, 9,
            11, 10, 9,
            // second part
            14, 12, 13,
            15, 14, 13,
            // part over the door
            14, 11, 16,
            17, 14, 16,

            // east wall
            // first part
            18, 20, 19,
            20, 21, 19,
            // second part
            22, 24, 23,
            24, 25, 23,
            // part over the door
            21, 24, 26,
            24, 27, 26,
            
            // south wall
            // first part
            28, 30, 29,
            30, 31, 29,
            // second part
            32, 34, 33,
            34, 35, 33,
            // part over the door
            31, 34, 36,
            34, 37, 36,

            // west wall
            // first part
            40, 38, 39,
            41, 40, 39,
            // second part
            44, 42, 43,
            45, 44, 43,
            // part over the door
            44, 41, 46,
            47, 44, 46
            
        };

        int[] frames = new int[72]

        {
            
            // north wall
            // top
            54, 50, 55,
            50, 51, 55,
            // left
            52, 48, 54,
            48, 50, 54,
            // right
            49, 53, 55,
            51, 49, 55,

            // east wall
            // top
            58, 62, 63,
            59, 58, 63,
            // left
            56, 60, 62,
            58, 56, 62,
            // right
            61, 57, 63,
            57, 59, 63,

            // south wall ADD 16
            // top
            66, 70, 71,
            67, 66, 71,
            // left
            64, 68, 70,
            66, 64, 70,
            // right
            69, 65, 71,
            65, 67, 71,

            // west wall
            // top
            78, 74, 79,
            74, 75, 79,
            // left
            76, 72, 78,
            72, 74, 78,
            // right
            73, 77, 79,
            75, 73, 79,

            /*
             
             old code without duplicate verts
             
            // east wall
            // top
            54, 26, 27,
            55, 54, 27,
            // left
            52, 19, 26,
            54, 52, 26,
            // right
            22, 53, 27,
            53, 55, 27,
            
            // west wall
            // top
            46, 62, 47,
            62, 63, 47,
            // left
            39, 60, 46,
            60, 62, 46,
            // right
            61, 42, 47,
            63, 61, 47,
            */

        };

        // set the sub mesh count to be 4, as there will be 4 sub meshes creating the room
        // 4 sub meshes match their materials from the material array
        mesh.subMeshCount = 4;
        // sub mesh 1 is the floor,
        mesh.SetTriangles(floorTriangles, 0);
        // sub mesh 2 is the roof
        mesh.SetTriangles(roofTriangles, 1);
        // sub mesh 3 are the four walls
        mesh.SetTriangles(walls, 2);
        // and the last sub mesh are the door frames
        mesh.SetTriangles(frames, 3);
        
    }

    private void CreateColliders()

    {
        
        // floor collider
        BoxCollider floorCollider = gameObject.AddComponent<BoxCollider>();
        floorCollider.size = new Vector3(roomWidth, 0, roomWidth);
        floorCollider.center = new Vector3((roomWidth / 2), 0, (roomWidth / 2));
        
        // roof collider
        BoxCollider roofCollider = gameObject.AddComponent<BoxCollider>();
        roofCollider.size = new Vector3(roomWidth, 0, roomWidth);
        roofCollider.center = new Vector3 ((roomWidth / 2), roomHeight, (roomWidth / 2));

        // north wall colliders
        // left part
        BoxCollider northWallCollider1 = gameObject.AddComponent<BoxCollider>();
        northWallCollider1.size = new Vector3(((roomWidth / 2) - (doorWidth / 2)), roomHeight, (wallWidth / 2));
        northWallCollider1.center = new Vector3(((roomWidth * 0.75f) + (doorWidth / 4)), (roomHeight / 2), (wallWidth / 4));
        // right part
        BoxCollider northWallCollider2 = gameObject.AddComponent<BoxCollider>();
        northWallCollider2.size = new Vector3(((roomWidth / 2) - (doorWidth / 2)), roomHeight, (wallWidth / 2));
        northWallCollider2.center = new Vector3(((roomWidth / 4) - (doorWidth / 4)), (roomHeight / 2), (wallWidth / 4));
        // center
        BoxCollider northWallCollider3 = gameObject.AddComponent<BoxCollider>();
        northWallCollider3.size = new Vector3(doorWidth, (roomHeight - doorHeight), (wallWidth / 2));
        northWallCollider3.center = new Vector3(roomWidth / 2, (roomHeight - ((roomHeight - doorHeight) / 2)), (wallWidth / 4));
        
        // east wall colliders
        // left part
        BoxCollider northWallCollider4 = gameObject.AddComponent<BoxCollider>();
        northWallCollider4.size = new Vector3((wallWidth / 2), roomHeight, (roomWidth / 2) - (doorWidth / 2));
        northWallCollider4.center = new Vector3((wallWidth / 4), (roomHeight / 2), (roomWidth * 0.75f) + (doorWidth / 4));
        // right part
        BoxCollider northWallCollider5 = gameObject.AddComponent<BoxCollider>();
        northWallCollider5.size = new Vector3((wallWidth / 2), roomHeight, (roomWidth / 2) - (doorWidth / 2));
        northWallCollider5.center = new Vector3((wallWidth / 4), (roomHeight / 2), ((roomWidth / 4) - (doorWidth / 4)));
        // center
        BoxCollider northWallCollider6 = gameObject.AddComponent<BoxCollider>();
        northWallCollider6.size = new Vector3((wallWidth / 2), (roomHeight - doorHeight), doorWidth);
        northWallCollider6.center = new Vector3((wallWidth / 4), (roomHeight - ((roomHeight - doorHeight) / 2)), (roomWidth / 2));
        
        // south wall colliders
        // left part
        BoxCollider northWallCollider7 = gameObject.AddComponent<BoxCollider>();
        northWallCollider7.size = new Vector3(((roomWidth / 2) - (doorWidth / 2)), roomHeight, (wallWidth / 2));
        northWallCollider7.center = new Vector3(((roomWidth * 0.75f) + (doorWidth / 4)), (roomHeight / 2), (roomWidth - (wallWidth / 4)));
        // right part
        BoxCollider northWallCollider8 = gameObject.AddComponent<BoxCollider>();
        northWallCollider8.size = new Vector3(((roomWidth / 2) - (doorWidth / 2)), roomHeight, (wallWidth / 2));
        northWallCollider8.center = new Vector3(((roomWidth / 4) - (doorWidth / 4)), (roomHeight / 2), (roomWidth - (wallWidth / 4)));
        // center
        BoxCollider northWallCollider9 = gameObject.AddComponent<BoxCollider>();
        northWallCollider9.size = new Vector3(doorWidth, (roomHeight - doorHeight), (wallWidth / 2));
        northWallCollider9.center = new Vector3(roomWidth / 2, (roomHeight - ((roomHeight - doorHeight) / 2)), (roomWidth - (wallWidth / 4)));
        
        // west wall colliders
        // left part
        BoxCollider northWallCollider10 = gameObject.AddComponent<BoxCollider>();
        northWallCollider10.size = new Vector3((wallWidth / 2), roomHeight, (roomWidth / 2) - (doorWidth / 2));
        northWallCollider10.center = new Vector3((roomWidth - (wallWidth / 4)), (roomHeight / 2), (roomWidth * 0.75f) + (doorWidth / 4));
        // right part
        BoxCollider northWallCollider11 = gameObject.AddComponent<BoxCollider>();
        northWallCollider11.size = new Vector3((wallWidth / 2), roomHeight, (roomWidth / 2) - (doorWidth / 2));
        northWallCollider11.center = new Vector3((roomWidth - (wallWidth / 4)), (roomHeight / 2), ((roomWidth / 4) - (doorWidth / 4)));
        // center
        BoxCollider northWallCollider12 = gameObject.AddComponent<BoxCollider>();
        northWallCollider12.size = new Vector3((wallWidth / 2), (roomHeight - doorHeight), doorWidth);
        northWallCollider12.center = new Vector3((roomWidth - (wallWidth / 4)), (roomHeight - ((roomHeight - doorHeight) / 2)), (roomWidth / 2));
        
    }
    
}