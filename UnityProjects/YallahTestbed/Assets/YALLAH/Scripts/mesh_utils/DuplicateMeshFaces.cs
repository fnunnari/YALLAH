using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/** Small utility to duplicate and reverse the triangles of a mesh and avoid back-face culling.
 * Modified from:
 * http://answers.unity3d.com/questions/280741/how-make-visible-the-back-face-of-a-mesh.html
 */
public class DuplicateMeshFaces : MonoBehaviour {

	void Start ()
    //void Awake()
    {
        SkinnedMeshRenderer skinned_mesh_renderer = GetComponent<SkinnedMeshRenderer>();
        MeshFilter mesh_filter = GetComponent<MeshFilter>();
        Mesh original_mesh = null;
        if (skinned_mesh_renderer != null)
        {
            original_mesh = skinned_mesh_renderer.sharedMesh;
        }
        else if (mesh_filter != null)
        {
            original_mesh = mesh_filter.sharedMesh;
        }
        else
        {
            // Assert.isNotNull (original_mesh);
            Debug.Log("Double Face Mesh: No mesh found!!!");
            return;
        }


        Assert.IsNotNull(original_mesh) ;

        // Make a copy of the mesh
        Mesh mesh = Instantiate(original_mesh);
        // Debug.Log("Number of vertices before: " + mesh.vertices.Length);

        var vertices = mesh.vertices;
        var uv = mesh.uv;
        var normals = mesh.normals;
        int szV = vertices.Length;
        var newVerts = new Vector3[szV * 2];
        var newUv = new Vector2[szV * 2];
        var newNorms = new Vector3[szV * 2];
        for (int j = 0; j < szV; j++)
        {
            // duplicate vertices and uvs:
            newVerts[j] = newVerts[j + szV] = vertices[j];
            newUv[j] = newUv[j + szV] = uv[j];
            // copy the original normals...
            newNorms[j] = normals[j];
            // and revert the new ones
            newNorms[j + szV] = -normals[j];
        }
        var triangles = mesh.triangles;
        int szT = triangles.Length;
        var newTris = new int[szT * 2]; // double the triangles
        for (int i = 0; i < szT; i += 3)
        {
            // copy the original triangle
            newTris[i] = triangles[i];
            newTris[i + 1] = triangles[i + 1];
            newTris[i + 2] = triangles[i + 2];
            // save the new reversed triangle
            int j = i + szT;
            newTris[j] = triangles[i] + szV;
            newTris[j + 2] = triangles[i + 1] + szV;
            newTris[j + 1] = triangles[i + 2] + szV;
        }
        mesh.vertices = newVerts;
        mesh.uv = newUv;
        mesh.normals = newNorms;
        mesh.triangles = newTris; // assign triangles last!

        // Debug.Log("Number of vertices after: " + mesh.vertices.Length);

        // Tells the renderer to display copy    
        if (skinned_mesh_renderer != null)
        {
            Debug.Log("Replacing mesh for skinned mesh renderer");
            skinned_mesh_renderer.sharedMesh = mesh;
        }
        else if (mesh_filter != null)
        {
            Debug.Log("Replacing mesh for mesh filter");
            mesh_filter.sharedMesh = mesh;
            // mesh_filter.mesh = mesh;
        }

    } // end Start()
	
}
