using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using util;

//[ExecuteInEditMode]
public class WaterPlaneGenerator : MonoBehaviour {

    public float size = 1;
    public int gridSize = 1;
    public int offset = 0;
     
    private MeshFilter filter;

	// Use this for initialization
	void Start () {
        filter = GetComponent<MeshFilter>();
        filter.mesh = GenerateMesh();
	}
	
    private Mesh GenerateMesh(){
        Mesh mesh = new Mesh();
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();

        for (int x = 0; x <= gridSize; x ++){
            for (int y = 0; y <= gridSize; y++)
            {
                Vector3 tmp = Util.ConvertToHexa(
                    size * (x / ((float)gridSize)), 
                    size * (y / ((float)gridSize)), 
                    (size / 2.0f) + offset, 
                    (size / 2.0f) + offset);
                vertices.Add(tmp * Util.HexaRatio());
                //vertices.Add(new Vector3(-size * 0.5f + size * (x / ((float)gridSize)), 0, -size * 0.5f + size * (y / ((float)gridSize))));
                normals.Add(Vector3.up);
                uvs.Add(new Vector2(x / (float)gridSize, y / (float)gridSize));
            }
        }
        var triangles = new List<int>();
        var vertCount = gridSize + 1;

        for (int i = 0; i < vertCount * vertCount - vertCount; i++){
            if((i + 1) % vertCount == 0){
                continue;
            }
            triangles.AddRange(new List<int>() {
                i, i + 1, i + vertCount,
                i + 1, i + vertCount + 1, i+ vertCount});
                
                //triangulate in the other direction
                //i + 1 + vertCount, i + vertCount, i,
                //i, i + 1, i + 1 + vertCount

        }
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        return mesh;
    }
}
