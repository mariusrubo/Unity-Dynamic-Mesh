using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class treehouse : MonoBehaviour {
    // controls
    [Range(0.05f, 0.25f)]
    public float width;
    [Range(1f, 3f)]
    public float length;
    [Range(1.5f, 3.5f)]
    public float elevation;

    public bool Sextuple;
    [Range(0.2f, 0.8f)]
    public float k1;
    [Range(0f, 5f)]
    public float LiftPlatform2;
    [Range(0f, 5f)]
    public float LiftPlatform2Inlay;
    
    // some basic points and vectors
    public Material mat1;
    Vector3 M; // center of this whole arrangement
    Vector3 a, b, c; // basic direction vectors, length = 1
    Vector3 d, e, f; // derived from a, b and c
    Vector3[] Layers;
    [Range(0f, 1f)]
    public float Offsetting;
    float[] OffsetsIndividualized; // give each bar a different height when offsetting upwards
    Vector3[] FloorMap;
    Vector3[] HexagonCrossing; // das Ur-Modell der einzelnen "Kreuzung", auf der die Balken zusammen kommen

    // the first platform
    GameObject Platform1;
    public Vector3[][] platform1_description;
    Hexagon3D[] platform1; // transform to redundant but full description as Unity uses it

    // the second platform
    GameObject Platform2;
    public Vector3[][] platform2_description;
    Hexagon3D[] platform2; 

    // the second platform, inlays
    GameObject Platform2inlays;
    public Vector3[][] Platform2inlays_description;
    Hexagon3D[] platform2inlays;


    // Use this for initialization
    void Start() {

        // set vertices of floor, and parallel floor beneath
        M = new Vector3(0, 1, 0);
        a = new Vector3(-1, 0, 0);
        b = new Vector3(-0.5f, 0, Mathf.Sqrt(3) / 2);
        c = new Vector3(0.5f, 0, Mathf.Sqrt(3) / 2);
        d = (a + c / 2); 
        e = (c + a / 2);
        f = (c - b / 2);

        OffsetsIndividualized = new float[60]; // create different values for offsets. Only do this once for all: Platform1[1] and Platform2[1] will just get the same offset.
        System.Random rnd = new System.Random();
        for (int i = 0; i < 60; i++) { OffsetsIndividualized[i] = (float)rnd.Next(0, 100)/100; }
        //Layers = new Vector3[2];
        FloorMap = new Vector3[16];
        HexagonCrossing = new Vector3[25];


        // initialize platform1
        Platform1 = new GameObject();
        Platform1.name = "Platform1";
        Platform1.transform.parent = transform;
        platform1_description = new Vector3[24][];
        platform1 = new Hexagon3D[24];
        for (int i = 0; i < 24; i++)
        {
            string name = "Platform1_" + i.ToString();
            platform1[i] = new Hexagon3D(mat1, Platform1.transform, name);
        }

        // initialize platform2
        Platform2 = new GameObject();
        Platform2.name = "Platform2";
        Platform2.transform.parent = transform;
        platform2_description = new Vector3[30][];
        platform2 = new Hexagon3D[30];
        for (int i = 0; i < 30; i++)
        {
            string name = "Platform2_" + i.ToString();
            platform2[i] = new Hexagon3D(mat1, Platform2.transform, name);
        }

        // initialize platform2, inlays
        Platform2inlays = new GameObject();
        Platform2inlays.name = "Platform2inlays";
        Platform2inlays.transform.parent = transform;
        Platform2inlays_description = new Vector3[60][];
        platform2inlays = new Hexagon3D[60];
        for (int i = 0; i < 60; i++)
        {
            string name = "Platform2inlays_" + i.ToString();
            platform2inlays[i] = new Hexagon3D(mat1, Platform2inlays.transform, name);
        }

    }

    // Update is called once per frame
    void Update() {
        // Update all the coordinates according to length and width of individual bars
        d = (a + c / 2);
        e = (c + a / 2);
        f = (c - b / 2);


        UpdateLayers();
        UpdateFloorMap();
        UpdateHexagonCrossing();

        // Update Platform 1 accordingly
        UpdatePlatform1();
        for (int i = 0; i < 24; i++)
        {
            platform1[i].vertices = CreateHexagon3D(platform1_description[i]);
            UpdateMesh(platform1[i].mesh, platform1[i].vertices, platform1[i].triangles, platform1[i].uv);
        }

        // Update Platform 2
        UpdatePlatform2();
        for (int i = 0; i < 30; i++)
        {
            platform2_description[i] = LinearilyMove(platform2_description[i], new Vector3(0, LiftPlatform2 + LiftPlatform2 *OffsetsIndividualized[i]* Offsetting, 0)); // offset individually
            platform2[i].vertices = CreateHexagon3D(platform2_description[i]);
            UpdateMesh(platform2[i].mesh, platform2[i].vertices, platform2[i].triangles, platform2[i].uv);
        }

        // Update Platform 2, inlays
        UpdatePlatform2inlays();
        for (int i = 0; i < 60; i++)
        {
            Platform2inlays_description[i] = LinearilyMove(Platform2inlays_description[i], new Vector3(0, (LiftPlatform2 + LiftPlatform2Inlay) + (LiftPlatform2 + LiftPlatform2Inlay) * OffsetsIndividualized[i]* Offsetting, 0));
            platform2inlays[i].vertices = CreateHexagon3D(Platform2inlays_description[i]);
            UpdateMesh(platform2inlays[i].mesh, platform2inlays[i].vertices, platform2inlays[i].triangles, platform2inlays[i].uv);
        }

        // Update texture
        mat1.mainTextureScale = new Vector2(length, width * 4);

    }

    // simply take all vertices in a Vector3[] and add an offset
    Vector3[] LinearilyMove(Vector3[] vecs, Vector3 offset)
    {
        for (int i = 0; i < vecs.Length; i++)
        {
            vecs[i] = vecs[i] + offset;
        }
        return vecs;
    }

    void UpdateLayers()
    {
        Layers = new Vector3[]
        {
            new Vector3(0, elevation, 0), // ..................................................................[0] bottom of Platform 1 
            new Vector3(0, elevation + width, 0), // ..........................................................[1] top of Platform 1
            new Vector3(0, elevation + width, 0), // ..........................................................[2] bottom of Platform 2
            new Vector3(0, elevation + width + width*Mathf.Sqrt(3)/2, 0), // ..................................[3] top of Platform 2 must be sqrt(3)/2 times as thick as the first (e.g. 13cm instead of 15cm))
            new Vector3(0, elevation + width, 0), // ..........................................................[4] bottom of Platform2Inlay
            new Vector3(0, elevation + width + width*Mathf.Sqrt(3)/2, 0), // ..................................[5] top of Platform2Inlay 
        };

    }

    void UpdateFloorMap()
    {
        FloorMap = new Vector3[]
        {
            M, // center
            M + a * length, // point 1
            M + b * length, // point 2
            M + c * length, // point 3
            M - a * length, // point 4
            M - b * length, // point 5
            M - c * length, // point 6
            M + (a + b)*length, // point 7
            M + (b + c)*length, // point 8
            M + (c - a)*length, // point 9
            M - (a + b)*length, // point 10
            M - (b + c)*length, // point 11
            M + (a - c)*length, // point 12

            M + (c + k1*b)*length, // point 13 needed for Platform2Inlays
            M + (c - k1*a)*length, // point 14
            M + (c - k1*c)*length, // point 15
            M + (c - k1*b/2)*length, // point 16
            M + (c + k1*a/2)*length // point 17
        };
    }

    // Recalculate based on width and length values
    void UpdateHexagonCrossing()
    {
        float k = width / Mathf.Sqrt(3);
        HexagonCrossing = new Vector3[]
        {
            new Vector3(0, 0, 0), // center, or point 0
            k * (a + b), // point 1
            k * (b + c), // point 2
            k * (-a + c), // point 3
            k * (-a - b), // point 4
            k * (-b -c), // point 5
            k * (a - c), // point 6

            k/2 * (a + b), // point 7
            k/2 * (b + c), // point 8
            k/2 * (-a + c), // point 9
            k/2 * (-a - b), // point 10
            k/2 * (-b -c), // point 11
            k/2 * (a - c), // point 12

            k*b, // point 13
            k*c, // point 14
            k*(-a), // point 15
            k*(-b), // point 16
            k*(-c), // point 17
            k*a, // point 18

            k*3/2*b, // point 19
            k*3/2*c, // point 20
            -k*3/2*a, // point 21
            -k*3/2*b, // point 22
            -k*3/2*c, // point 23
            k*3/2*a // point 24
        };
    }

    // Construct all bars' vertices from Layers, Floormap and HexagonCrossing
    void UpdatePlatform1()
    {
        platform1_description[0] = new Vector3[]
        {
            Layers[1] + FloorMap[0] + HexagonCrossing[0], // point 0
            Layers[1] + FloorMap[0] + HexagonCrossing[2], // point 1
            Layers[1] + FloorMap[3] + HexagonCrossing[6], // point 2
            Layers[1] + FloorMap[3] + HexagonCrossing[0], // point 3
            Layers[1] + FloorMap[3] + HexagonCrossing[5], // point 4
            Layers[1] + FloorMap[0] + HexagonCrossing[3], // point 5
            Layers[0] + FloorMap[0] + HexagonCrossing[0], // point 6
            Layers[0] + FloorMap[0] + HexagonCrossing[2], // point 7
            Layers[0] + FloorMap[3] + HexagonCrossing[6], // point 8
            Layers[0] + FloorMap[3] + HexagonCrossing[0], // point 9
            Layers[0] + FloorMap[3] + HexagonCrossing[5], // point 10
            Layers[0] + FloorMap[0] + HexagonCrossing[3], // point 11
        };

        platform1_description[1] = new Vector3[]
        {
            Layers[1] + FloorMap[3] + HexagonCrossing[0], // point 0
            Layers[1] + FloorMap[3] + HexagonCrossing[4], // point 1
            Layers[1] + FloorMap[4] + HexagonCrossing[2], // point 2
            Layers[1] + FloorMap[4] + HexagonCrossing[0], // point 3
            Layers[1] + FloorMap[4] + HexagonCrossing[1], // point 4
            Layers[1] + FloorMap[3] + HexagonCrossing[5], // point 5
            Layers[0] + FloorMap[3] + HexagonCrossing[0], // point 6
            Layers[0] + FloorMap[3] + HexagonCrossing[4], // point 7
            Layers[0] + FloorMap[4] + HexagonCrossing[2], // point 8
            Layers[0] + FloorMap[4] + HexagonCrossing[0], // point 9
            Layers[0] + FloorMap[4] + HexagonCrossing[1], // point 10
            Layers[0] + FloorMap[3] + HexagonCrossing[5], // point 11
        };

        platform1_description[2] = new Vector3[]
        {
            Layers[1] + FloorMap[3] + HexagonCrossing[0], // point 0
            Layers[1] + FloorMap[3] + HexagonCrossing[1], // point 1
            Layers[1] + FloorMap[8] + HexagonCrossing[5], // point 2
            Layers[1] + FloorMap[8] + HexagonCrossing[8], // point 3
            Layers[1] + FloorMap[8] + HexagonCrossing[9], // point 4
            Layers[1] + FloorMap[3] + HexagonCrossing[14], // point 5
            Layers[0] + FloorMap[3] + HexagonCrossing[0], // point 6
            Layers[0] + FloorMap[3] + HexagonCrossing[1], // point 7
            Layers[0] + FloorMap[8] + HexagonCrossing[5], // point 8
            Layers[0] + FloorMap[8] + HexagonCrossing[8], // point 9
            Layers[0] + FloorMap[8] + HexagonCrossing[9], // point 10
            Layers[0] + FloorMap[3] + HexagonCrossing[14], // point 11
        };

        platform1_description[3] = new Vector3[]
        {
            Layers[1] + FloorMap[3] + HexagonCrossing[0], // point 0
            Layers[1] + FloorMap[3] + HexagonCrossing[14], // point 1
            Layers[1] + FloorMap[9] + HexagonCrossing[8], // point 2
            Layers[1] + FloorMap[9] + HexagonCrossing[9], // point 3
            Layers[1] + FloorMap[9] + HexagonCrossing[6], // point 4
            Layers[1] + FloorMap[3] + HexagonCrossing[4], // point 5
            Layers[0] + FloorMap[3] + HexagonCrossing[0], // point 6
            Layers[0] + FloorMap[3] + HexagonCrossing[14], // point 7
            Layers[0] + FloorMap[9] + HexagonCrossing[8], // point 8
            Layers[0] + FloorMap[9] + HexagonCrossing[9], // point 9
            Layers[0] + FloorMap[9] + HexagonCrossing[6], // point 10
            Layers[0] + FloorMap[3] + HexagonCrossing[4], // point 11
        };

        for (int i = 1; i < 6; i++) // copy this 5 times
        {
            Quaternion rotation = Quaternion.Euler(new Vector3(0, 60 * i, 0));
            for (int j = 0; j < 4; j++) // there are 4 bars here
            {
                platform1_description[4 * i + j] = new Vector3[12];

                if (Sextuple)
                {
                    for (int k = 0; k < 12; k++) // they all have 12 corners
                    {
                        platform1_description[4 * i + j][k] = rotation * (platform1_description[j][k] - M) + M; // copy from equivalent bar with i=0
                    }
                }
            }
        }

    }

    void UpdatePlatform2()
    {
        platform2_description[0] = new Vector3[]
        {
            Layers[3] + FloorMap[9] + HexagonCrossing[9], // point 0
            Layers[3] + FloorMap[9] + HexagonCrossing[24], // point 1
            Layers[3] + FloorMap[8] + HexagonCrossing[22], // point 2
            Layers[3] + FloorMap[8] + HexagonCrossing[8], // point 3
            Layers[3] + FloorMap[8] + HexagonCrossing[9], // point 4
            Layers[3] + FloorMap[9] + HexagonCrossing[8], // point 5
            Layers[2] + FloorMap[9] + HexagonCrossing[9], // point 6
            Layers[2] + FloorMap[9] + HexagonCrossing[24], // point 7
            Layers[2] + FloorMap[8] + HexagonCrossing[22], // point 8
            Layers[2] + FloorMap[8] + HexagonCrossing[8], // point 9
            Layers[2] + FloorMap[8] + HexagonCrossing[9], // point 10
            Layers[2] + FloorMap[9] + HexagonCrossing[8], // point 11
        };


        platform2_description[1] = new Vector3[]
        {
            Layers[3] + FloorMap[9] + HexagonCrossing[9], 
            Layers[3] + FloorMap[9] + HexagonCrossing[23], 
            Layers[3] + FloorMap[0] + HexagonCrossing[21], 
            Layers[3] + FloorMap[0] + HexagonCrossing[0], 
            Layers[3] + FloorMap[0] + HexagonCrossing[20], 
            Layers[3] + FloorMap[9] + HexagonCrossing[24], 
            Layers[2] + FloorMap[9] + HexagonCrossing[9], 
            Layers[2] + FloorMap[9] + HexagonCrossing[23], 
            Layers[2] + FloorMap[0] + HexagonCrossing[21], 
            Layers[2] + FloorMap[0] + HexagonCrossing[0], 
            Layers[2] + FloorMap[0] + HexagonCrossing[20], 
            Layers[2] + FloorMap[9] + HexagonCrossing[24],
        };

        platform2_description[2] = new Vector3[]
        {
            Layers[3] + FloorMap[13] + HexagonCrossing[0], // point 5
            Layers[3] + FloorMap[13] + HexagonCrossing[9], // point 0
            Layers[3] + FloorMap[14] + HexagonCrossing[8], // point 1
            Layers[3] + FloorMap[14] + HexagonCrossing[0], // point 2
            Layers[3] + FloorMap[14] + HexagonCrossing[24], // point 3
            Layers[3] + FloorMap[13] + HexagonCrossing[22], // point 4
            Layers[2] + FloorMap[13] + HexagonCrossing[0], // point 11
            Layers[2] + FloorMap[13] + HexagonCrossing[9], // point 6
            Layers[2] + FloorMap[14] + HexagonCrossing[8], // point 7
            Layers[2] + FloorMap[14] + HexagonCrossing[0], // point 8
            Layers[2] + FloorMap[14] + HexagonCrossing[24], // point 9
            Layers[2] + FloorMap[13] + HexagonCrossing[22], // point 10
        };

        platform2_description[3] = new Vector3[]
        {
            Layers[3] + FloorMap[14] + HexagonCrossing[0], // point 5
            Layers[3] + FloorMap[14] + HexagonCrossing[11], // point 0
            Layers[3] + FloorMap[15] + HexagonCrossing[10], // point 1
            Layers[3] + FloorMap[15] + HexagonCrossing[0], // point 2
            Layers[3] + FloorMap[15] + HexagonCrossing[20], // point 3
            Layers[3] + FloorMap[14] + HexagonCrossing[24], // point 4
            Layers[2] + FloorMap[14] + HexagonCrossing[0], // point 11
            Layers[2] + FloorMap[14] + HexagonCrossing[11], // point 6
            Layers[2] + FloorMap[15] + HexagonCrossing[10], // point 7
            Layers[2] + FloorMap[15] + HexagonCrossing[0], // point 8
            Layers[2] + FloorMap[15] + HexagonCrossing[20], // point 9
            Layers[2] + FloorMap[14] + HexagonCrossing[24], // point 10
        };

        platform2_description[4] = new Vector3[]
        {
            Layers[3] + FloorMap[15] + HexagonCrossing[0], 
            Layers[3] + FloorMap[15] + HexagonCrossing[7], 
            Layers[3] + FloorMap[13] + HexagonCrossing[12], 
            Layers[3] + FloorMap[13] + HexagonCrossing[0], 
            Layers[3] + FloorMap[13] + HexagonCrossing[22], 
            Layers[3] + FloorMap[15] + HexagonCrossing[20], 
            Layers[2] + FloorMap[15] + HexagonCrossing[0], 
            Layers[2] + FloorMap[15] + HexagonCrossing[7], 
            Layers[2] + FloorMap[13] + HexagonCrossing[12], 
            Layers[2] + FloorMap[13] + HexagonCrossing[0], 
            Layers[2] + FloorMap[13] + HexagonCrossing[22], 
            Layers[2] + FloorMap[15] + HexagonCrossing[20], 
        };
   
        for (int i = 1; i < 6; i++) // copy this 5 times
        {
            Quaternion rotation = Quaternion.Euler(new Vector3(0, 60*i, 0));
            for (int j = 0; j < 5; j++) // there are 5 bars here
            {
                platform2_description[5*i+j] = new Vector3[12];

                if (Sextuple)
                {
                    for (int k = 0; k < 12; k++) // they all have 12 corners
                    {
                        platform2_description[5 * i + j][k] = rotation * (platform2_description[j][k] - M) + M; // copy from equivalent bar with i=0
                    }
                }
            }
        }
        

    }

    void UpdatePlatform2inlays()
    {
        Platform2inlays_description[0] = new Vector3[]
        {
            Layers[5] + FloorMap[13] + HexagonCrossing[22],
            Layers[5] + FloorMap[13] + HexagonCrossing[22]-2*d*width / Mathf.Sqrt(3),
            Layers[5] + FloorMap[3] + HexagonCrossing[14],
            Layers[5] + FloorMap[3] + HexagonCrossing[0],
            Layers[5] + FloorMap[3] + HexagonCrossing[1],
            Layers[5] + FloorMap[13] + HexagonCrossing[22]-2*e*width / Mathf.Sqrt(3),
            Layers[4] + FloorMap[13] + HexagonCrossing[22],
            Layers[4] + FloorMap[13] + HexagonCrossing[22]-2*d*width / Mathf.Sqrt(3),
            Layers[4] + FloorMap[3] + HexagonCrossing[14],
            Layers[4] + FloorMap[3] + HexagonCrossing[0],
            Layers[4] + FloorMap[3] + HexagonCrossing[1],
            Layers[4] + FloorMap[13] + HexagonCrossing[22]-2*e*width / Mathf.Sqrt(3),
        };

        Platform2inlays_description[1] = new Vector3[]
        {
            Layers[5] + FloorMap[3] + HexagonCrossing[0],
            Layers[5] + FloorMap[3] + HexagonCrossing[14],
            Layers[5] + FloorMap[14] + HexagonCrossing[24]+2*d*width / Mathf.Sqrt(3),
            Layers[5] + FloorMap[14] + HexagonCrossing[24],
            Layers[5] + FloorMap[14] + HexagonCrossing[24]-2*f*width / Mathf.Sqrt(3),
            Layers[5] + FloorMap[3] + HexagonCrossing[4],
            Layers[4] + FloorMap[3] + HexagonCrossing[0],
            Layers[4] + FloorMap[3] + HexagonCrossing[14],
            Layers[4] + FloorMap[14] + HexagonCrossing[24]+2*d*width / Mathf.Sqrt(3),
            Layers[4] + FloorMap[14] + HexagonCrossing[24],
            Layers[4] + FloorMap[14] + HexagonCrossing[24]-2*f*width / Mathf.Sqrt(3),
            Layers[4] + FloorMap[3] + HexagonCrossing[4],
        };

        Platform2inlays_description[2] = new Vector3[]
        {
            Layers[5] + FloorMap[3] + HexagonCrossing[0],
            Layers[5] + FloorMap[3] + HexagonCrossing[4],
            Layers[5] + FloorMap[16] + HexagonCrossing[0]+(e+f/2)*(width / Mathf.Sqrt(3)),
            Layers[5] + FloorMap[16] + HexagonCrossing[0]+(d/2+e/2)*(width / Mathf.Sqrt(3)),
            Layers[5] + FloorMap[16] + HexagonCrossing[0]+(d-f/2)*(width / Mathf.Sqrt(3)),
            Layers[5] + FloorMap[3] + HexagonCrossing[5],
            Layers[4] + FloorMap[3] + HexagonCrossing[0],
            Layers[4] + FloorMap[3] + HexagonCrossing[4],
            Layers[4] + FloorMap[16] + HexagonCrossing[0]+(e+f/2)*(width / Mathf.Sqrt(3)),
            Layers[4] + FloorMap[16] + HexagonCrossing[0]+(d/2+e/2)*(width / Mathf.Sqrt(3)),
            Layers[4] + FloorMap[16] + HexagonCrossing[0]+(d-f/2)*(width / Mathf.Sqrt(3)),
            Layers[4] + FloorMap[3] + HexagonCrossing[5],
        };

        Platform2inlays_description[3] = new Vector3[]
        {
            Layers[5] + FloorMap[3] + HexagonCrossing[0],
            Layers[5] + FloorMap[3] + HexagonCrossing[5],
            Layers[5] + FloorMap[15] + HexagonCrossing[3]+(e+f)*(width / Mathf.Sqrt(3)),
            Layers[5] + FloorMap[15] + HexagonCrossing[3]+d*(width / Mathf.Sqrt(3)),
            Layers[5] + FloorMap[15] + HexagonCrossing[2]+(e+f)*(width / Mathf.Sqrt(3)),
            Layers[5] + FloorMap[3] + HexagonCrossing[6],
            Layers[4] + FloorMap[3] + HexagonCrossing[0],
            Layers[4] + FloorMap[3] + HexagonCrossing[5],
            Layers[4] + FloorMap[15] + HexagonCrossing[3]+(e+f)*(width / Mathf.Sqrt(3)),
            Layers[4] + FloorMap[15] + HexagonCrossing[3]+d*(width / Mathf.Sqrt(3)),
            Layers[4] + FloorMap[15] + HexagonCrossing[2]+(e+f)*(width / Mathf.Sqrt(3)),
            Layers[4] + FloorMap[3] + HexagonCrossing[6]
        };

        Platform2inlays_description[4] = new Vector3[]
        {
            Layers[5] + FloorMap[3] + HexagonCrossing[0],
            Layers[5] + FloorMap[3] + HexagonCrossing[6],
            Layers[5] + FloorMap[17] + HexagonCrossing[0]-(d+e/2)*(width / Mathf.Sqrt(3)),
            Layers[5] + FloorMap[17] + HexagonCrossing[0]+(f-e/2)*(width / Mathf.Sqrt(3)),
            Layers[5] + FloorMap[17] + HexagonCrossing[0]+(f+e/2)*(width / Mathf.Sqrt(3)),
            Layers[5] + FloorMap[3] + HexagonCrossing[1],
            Layers[4] + FloorMap[3] + HexagonCrossing[0],
            Layers[4] + FloorMap[3] + HexagonCrossing[6],
            Layers[4] + FloorMap[17] + HexagonCrossing[0]-(d+e/2)*(width / Mathf.Sqrt(3)),
            Layers[4] + FloorMap[17] + HexagonCrossing[0]+(f-e/2)*(width / Mathf.Sqrt(3)),
            Layers[4] + FloorMap[17] + HexagonCrossing[0]+(f+e/2)*(width / Mathf.Sqrt(3)),
            Layers[4] + FloorMap[3] + HexagonCrossing[1],
        };

        Platform2inlays_description[6] = new Vector3[]
        {
            Layers[5] + FloorMap[9] + HexagonCrossing[1]-e*(width / Mathf.Sqrt(3)),
            Layers[5] + FloorMap[9] + HexagonCrossing[6]+(d-f)*(width / Mathf.Sqrt(3)),
            Layers[5] + FloorMap[14] + HexagonCrossing[11],
            Layers[5] + FloorMap[14] + HexagonCrossing[0],
            Layers[5] + FloorMap[14] + HexagonCrossing[8],
            Layers[5] + FloorMap[9] + HexagonCrossing[1]+(d-f)*(width / Mathf.Sqrt(3)),
            Layers[4] + FloorMap[9] + HexagonCrossing[1]-e*(width / Mathf.Sqrt(3)),
            Layers[4] + FloorMap[9] + HexagonCrossing[6]+(d-f)*(width / Mathf.Sqrt(3)),
            Layers[4] + FloorMap[14] + HexagonCrossing[11],
            Layers[4] + FloorMap[14] + HexagonCrossing[0],
            Layers[4] + FloorMap[14] + HexagonCrossing[8],
            Layers[4] + FloorMap[9] + HexagonCrossing[1]+(d-f)*(width / Mathf.Sqrt(3)),
        };

        // create other two by rotation
        Platform2inlays_description[5] = new Vector3[12];
        Platform2inlays_description[8] = new Vector3[12];
        for (int k = 0; k < 12; k++)
        {
            Platform2inlays_description[5][k] = Quaternion.Euler(new Vector3(0, 120, 0)) * (Platform2inlays_description[6][k] - FloorMap[3]) + FloorMap[3];
            Platform2inlays_description[8][k] = Quaternion.Euler(new Vector3(0, -120, 0)) * (Platform2inlays_description[6][k] - FloorMap[3]) + FloorMap[3];
        }


        Vector3 M09 = M +(c - a) / 2 * length; // another Hexagon just between 0 and 9, which I think I don't need elsewhere
        Platform2inlays_description[7] = new Vector3[]
        {
            Layers[5] + FloorMap[16] + HexagonCrossing[0] - (e-f/2)*(width / Mathf.Sqrt(3)),
            Layers[5] + FloorMap[16] + HexagonCrossing[0] - (d-f/2)*(width / Mathf.Sqrt(3)),
            Layers[5] + M09 + HexagonCrossing[0] + (e+f/2)*(width / Mathf.Sqrt(3)),
            Layers[5] + M09 + HexagonCrossing[0] + (e-f/2)*(width / Mathf.Sqrt(3)),
            Layers[5] + M09 + HexagonCrossing[0] + (d-f/2)*(width / Mathf.Sqrt(3)),
            Layers[5] + FloorMap[16] + HexagonCrossing[0] - (e+f/2)*(width / Mathf.Sqrt(3)),
            Layers[4] + FloorMap[16] + HexagonCrossing[0] - (e-f/2)*(width / Mathf.Sqrt(3)),
            Layers[4] + FloorMap[16] + HexagonCrossing[0] - (d-f/2)*(width / Mathf.Sqrt(3)),
            Layers[4] + M09 + HexagonCrossing[0] + (e+f/2)*(width / Mathf.Sqrt(3)),
            Layers[4] + M09 + HexagonCrossing[0] + (e-f/2)*(width / Mathf.Sqrt(3)),
            Layers[4] + M09 + HexagonCrossing[0] + (d-f/2)*(width / Mathf.Sqrt(3)),
            Layers[4] + FloorMap[16] + HexagonCrossing[0] - (e+f/2)*(width / Mathf.Sqrt(3)),
        };

        Vector3 M23 = M + (b + c) / 2 * length; // another Hexagon just between 0 and 9, which I think I don't need elsewhere
        Platform2inlays_description[9] = new Vector3[]
        {
            Layers[5] + FloorMap[17] + HexagonCrossing[0] - (f-e/2)*(width / Mathf.Sqrt(3)),
            Layers[5] + FloorMap[17] + HexagonCrossing[0] - (f+e/2)*(width / Mathf.Sqrt(3)),
            Layers[5] + M23 + HexagonCrossing[0] - (d+e/2)*(width / Mathf.Sqrt(3)),
            Layers[5] + M23 + HexagonCrossing[0] - (d-e/2)*(width / Mathf.Sqrt(3)),
            Layers[5] + M23 + HexagonCrossing[0] + (f+e/2)*(width / Mathf.Sqrt(3)),
            Layers[5] + FloorMap[17] + HexagonCrossing[0] + (d+e/2)*(width / Mathf.Sqrt(3)),
            Layers[4] + FloorMap[17] + HexagonCrossing[0] - (f-e/2)*(width / Mathf.Sqrt(3)),
            Layers[4] + FloorMap[17] + HexagonCrossing[0] - (f+e/2)*(width / Mathf.Sqrt(3)),
            Layers[4] + M23 + HexagonCrossing[0] - (d+e/2)*(width / Mathf.Sqrt(3)),
            Layers[4] + M23 + HexagonCrossing[0] - (d-e/2)*(width / Mathf.Sqrt(3)),
            Layers[4] + M23 + HexagonCrossing[0] + (f+e/2)*(width / Mathf.Sqrt(3)),
            Layers[4] + FloorMap[17] + HexagonCrossing[0] + (d+e/2)*(width / Mathf.Sqrt(3)),
        };


        for (int i = 1; i < 6; i++) // copy this 5 times
        {
            Quaternion rotation = Quaternion.Euler(new Vector3(0, 60 * i, 0));
            for (int j = 0; j < 10; j++) // there are 10 bars here
            {
                Platform2inlays_description[10 * i + j] = new Vector3[12];

                if (Sextuple)
                {
                    for (int k = 0; k < 12; k++) // they all have 12 corners
                    {
                        Platform2inlays_description[10 * i + j][k] = rotation * (Platform2inlays_description[j][k] - M) + M; // copy from equivalent bar with i=0
                    }
                }
            }
        }


    }

    // General Hexagon 3D class

    public class Hexagon3D
    {
        public GameObject bar;
        public Transform transform;
        public MeshFilter meshfilter;
        public Mesh mesh;
        public Vector3[] vertices;
        public int[] triangles;
        public Vector2[] uv;
        public Material mat;
        public MeshRenderer meshRenderer;

        // initialize
        public Hexagon3D(Material material, Transform parent, string name)
        {
            bar = new GameObject();
            bar.name = name;
            bar.transform.parent = parent;
            meshfilter = bar.AddComponent<MeshFilter>();
            mesh = new Mesh();
            //bar.GetComponent<MeshFilter>().mesh = mesh;
            mesh = bar.GetComponent<MeshFilter>().mesh;
            vertices = new Vector3[60];
            triangles = new int[]
            {
                0,1,2,
                3,4,5,
                6,7,8,
                9,10,11,
                12,13,14,
                15,16,17,
                18,19,20,
                21,22,23,
                24,25,26,
                27,28,29,
                30,31,32,
                33,34,35,
                36,37,38,
                39,40,41,
                42,43,44,
                45,46,47,
                48,49,50,
                51,52,53,
                54,55,56,
                57,58,59
            };
            uv = new Vector2[60];

            meshRenderer = bar.AddComponent<MeshRenderer>();
            mat = material;
            meshRenderer.material = mat;

        }
    }

    void UpdateMesh(Mesh mesh, Vector3[] vertices, int[] triangles, Vector2[] uv)
    {
        float u = 4 * width + length;
        uv = new Vector2[60]
            {
            new Vector2((3*width+length)/u, 0.125f), // 0
            new Vector2((2*width+length)/u, 0f), // 1
            new Vector2((2*width+length)/u, 0.25f), // 2
            new Vector2((2*width+length)/u, 0f), // 3
            new Vector2((2*width)/u, 0f), // 4
            new Vector2((2*width+length)/u, 0.25f), // 5
            new Vector2((2*width)/u, 0f), // 6
            new Vector2((2*width)/u, 0.25f), // 7
            new Vector2((2*width+length)/u, 0.25f), // 8
            new Vector2((2*width)/u, 0f), // 9
            new Vector2((width)/u, 0.125f), // 10
            new Vector2((2*width)/u, 0.25f), // 11
            new Vector2((2*width)/u, 0.5f), // 12
            new Vector2((width)/u, 0.625f), // 13
            new Vector2((2*width)/u, 0.75f), // 14
            new Vector2((2*width)/u, 0.75f), // 15
            new Vector2((2*width+length)/u, 0.75f), // 16
            new Vector2((2*width+length)/u, 0.5f), // 17
            new Vector2((2*width+length)/u, 0.5f), // 18
            new Vector2((2*width)/u, 0.5f), // 19
            new Vector2((2*width)/u, 0.75f), // 20
            new Vector2((2*width+length)/u, 0.75f), // 21
            new Vector2((3*width+length)/u, 0.625f), // 22
            new Vector2((2*width+length)/u, 0.5f), // 23
            new Vector2((2*width+length)/u, 0f), // 24
            new Vector2((3*width+length)/u, 0f), // 25
            new Vector2((3*width+length)/u, 0.75f), // 26
            new Vector2((3*width+length)/u, 0.75f), // 27
            new Vector2((2*width+length)/u, 0.75f), // 28
            new Vector2((2*width+length)/u, 0f), // 29
            new Vector2((2*width)/u, 0f), // 30
            new Vector2((2*width+length)/u, 0f), // 31
            new Vector2((2*width+length)/u, 0.75f), // 32
            new Vector2((2*width+length)/u, 0.75f), // 33
            new Vector2((2*width)/u, 0.75f), // 34
            new Vector2((2*width)/u, 0f), // 35
            new Vector2((width)/u, 0f), // 36
            new Vector2((2*width)/u, 0f), // 37
            new Vector2((2*width)/u, 0.75f), // 38
            new Vector2((2*width)/u, 0.75f), // 39
            new Vector2((width)/u, 0.75f), // 40
            new Vector2((width)/u, 0f), // 41
            new Vector2(0f, 0f), // 42
            new Vector2((width)/u, 0f), // 43
            new Vector2((width)/u, 0.75f), // 44
            new Vector2((width)/u, 0.75f), // 45
            new Vector2(0f, 0.75f), // 46
            new Vector2(0f, 0f), // 47
            new Vector2((2*width+length)/u, 0.25f), // 48
            new Vector2((2*width)/u, 0.25f), // 49
            new Vector2((2*width)/u, 0.5f), // 50
            new Vector2((2*width)/u, 0.5f), // 51
            new Vector2((2*width+length)/u, 0.5f), // 52
            new Vector2((2*width+length)/u, 0.25f), // 53
            new Vector2((3*width+length)/u, 0f), // 54
            new Vector2(1f, 0f), // 55
            new Vector2(1f, 0.75f), // 56
            new Vector2(1f, 0.75f), // 57
            new Vector2((3*width+length)/u, 0.75f), // 58
            new Vector2((3*width+length)/u, 0f), // 59
            };

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.uv2 = uv;
        mesh.RecalculateNormals();
    }

    // create a Hexagon3D's vertices in Unity's somewhat redundant notation from the 12 vertices it actually consists of (first the top hexagon clockwise, then the bottom hexagon clockwise)
    public Vector3[] CreateHexagon3D(Vector3[] hexagon3D)
    {
        Vector3[] verts = new Vector3[60]
        {
            hexagon3D[0], // 0
            hexagon3D[1], // 1
            hexagon3D[5], // 2
            hexagon3D[1], // 3
            hexagon3D[2], // 4
            hexagon3D[5], // 5
            hexagon3D[2], // 6
            hexagon3D[4], // 7
            hexagon3D[5], // 8
            hexagon3D[2], // 9
            hexagon3D[3], // 10
            hexagon3D[4], // 11
            hexagon3D[10], // 12
            hexagon3D[9], // 13
            hexagon3D[8], // 14
            hexagon3D[8], // 15
            hexagon3D[7], // 16
            hexagon3D[11], // 17
            hexagon3D[11], // 18
            hexagon3D[10], // 19
            hexagon3D[8], // 20
            hexagon3D[7], // 21
            hexagon3D[6], // 22
            hexagon3D[11], // 23
            hexagon3D[1], // 24
            hexagon3D[0], // 25
            hexagon3D[6], // 26
            hexagon3D[6], // 27
            hexagon3D[7], // 28
            hexagon3D[1], // 29
            hexagon3D[2], // 30
            hexagon3D[1], // 31
            hexagon3D[7], // 32
            hexagon3D[7], // 33
            hexagon3D[8], // 34
            hexagon3D[2], // 35
            hexagon3D[3], // 36
            hexagon3D[2], // 37
            hexagon3D[8], // 38
            hexagon3D[8], // 39
            hexagon3D[9], // 40
            hexagon3D[3], // 41
            hexagon3D[4], // 42
            hexagon3D[3], // 43
            hexagon3D[9], // 44
            hexagon3D[9], // 45
            hexagon3D[10], // 46
            hexagon3D[4], // 47
            hexagon3D[5], // 48
            hexagon3D[4], // 49
            hexagon3D[10], // 50
            hexagon3D[10], // 51
            hexagon3D[11], // 52
            hexagon3D[5], // 53
            hexagon3D[0], // 54
            hexagon3D[5], // 55
            hexagon3D[11], // 56
            hexagon3D[11], // 57
            hexagon3D[6], // 58
            hexagon3D[0], // 59
        };
        return verts;
    }

}
