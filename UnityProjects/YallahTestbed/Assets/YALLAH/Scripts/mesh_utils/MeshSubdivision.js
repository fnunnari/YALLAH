//#pragma strict
//MeshSubdivision, Author: realm_1, modified by: Kiarash Tamaddon
//This script subdivides a mesh by using center subdivision.
//
//Usage: attach the code to the intended mesh
//Note: Problem! By running the script, orientation of the mesh changes!


private var verts : Vector3[];
private var norms : Vector3[];
private var uvs : Vector2[];
private var trigs : int[];
private var mesh : Mesh;
private var originalMesh : Mesh;



function Awake() {

	var skinned_mesh_renderer = GetComponent(SkinnedMeshRenderer);
    var mesh_filter = GetComponent(MeshFilter);
    var original_mesh : Mesh = null;
    if(skinned_mesh_renderer != null) {
        original_mesh = skinned_mesh_renderer.sharedMesh;
    } else if (mesh_filter != null) {
        original_mesh = mesh_filter.sharedMesh;
    } else {
        //assert (false);
    }
 
	updatemesh();
 
	originalMesh=new Mesh();
	CopyMesh(mesh, originalMesh);

	//
	subdivide(true);
}
 
function subdivide(center : boolean) {
 
	verts = mesh.vertices;
	trigs = mesh.triangles;
	uvs = mesh.uv;
	norms = mesh.normals;
 
	Debug.Log("enter subdividing: "+verts.length);
 
	var nv = new Array(verts);
	var nt = new Array(trigs);
	var nu = new Array(uvs);
	var nn = new Array(norms);
	
	if(center) {
		for(var ii : int = 2;ii<trigs.length;ii+=3) {
 
			var p0trigwhomi : int = trigs[ii-2];
			var p1trigwhomi : int = trigs[ii-1];
			var p2trigwhomi : int = trigs[ii];
 
			var p0trigwheremi : int = ii-2;
			var p1trigwheremi : int = ii-1;
			var p2trigwheremi : int = ii;
 
			var p0mi : Vector3 = verts[p0trigwhomi];
			var p1mi : Vector3 = verts[p1trigwhomi];
			var p2mi : Vector3 = verts[p2trigwhomi];
 
			var p0mn : Vector3 = norms[p0trigwhomi];
			var p1mn : Vector3 = norms[p1trigwhomi];
			var p2mn : Vector3 = norms[p2trigwhomi];
 
			var p0mu : Vector2 = uvs[p0trigwhomi];
			var p1mu : Vector2 = uvs[p1trigwhomi];
			var p2mu : Vector2 = uvs[p2trigwhomi];
 
			var p0modmi : Vector3 = (p0mi+p1mi+p2mi)/3;
			var p0modmn : Vector3 = ((p0mn+p1mn+p2mn)/3).normalized;
			var p0modmu : Vector2 = (p0mu+p1mu+p2mu)/3;	
 
			var p0modtrigwhomi = nv.length;
 
			nv.push(p0modmi);
			nn.push(p0modmn);
			nu.push(p0modmu);
 
			nt[p0trigwheremi] = p0trigwhomi;
			nt[p1trigwheremi] = p1trigwhomi;
			nt[p2trigwheremi] = p0modtrigwhomi;
 
			nt.push(p0modtrigwhomi);
			nt.push(p1trigwhomi);
			nt.push(p2trigwhomi);
 
			nt.push(p0trigwhomi);
			nt.push(p0modtrigwhomi);
			nt.push(p2trigwhomi);
		}
 
	}
 
	verts = nv.ToBuiltin(Vector3);
	norms = nn.ToBuiltin(Vector3);
	uvs = nu.ToBuiltin(Vector2);
	trigs = nt.ToBuiltin(int);
 
	applyuvs();
	applymesh();
	//mesh.RecalculateNormals();
 
	Debug.Log("exit subdividing: "+verts.length);
}
 
function applyuvs() {
	uvs = new Vector2[verts.length];
	for(var i : int = 0;i<verts.length;i++)
		uvs[i] = Vector2(verts[i].x,verts[i].y);
}
 
function updatemesh() {
	mesh = GetComponent(SkinnedMeshRenderer).sharedMesh;
	Debug.Log("updatemesh");

}
 
function applymesh() {
	print(verts.length);
	if(verts.length > 65000){
		Debug.Log("Exiting... Too many vertices");
		return;
	}
	mesh.Clear();
	mesh.vertices = verts;
	mesh.uv = uvs;
	if(mesh.uv2 != null)
		mesh.uv2 = uvs;
	mesh.normals = norms;
	mesh.triangles = trigs;
	mesh.RecalculateBounds();
	if(GetComponent(MeshCollider) != null)
		GetComponent(MeshCollider).sharedMesh = mesh;
	updatemesh();

	Debug.Log("applymesh");
}
 
function CopyMesh(fromMesh : Mesh, toMesh : Mesh){
	toMesh.Clear();
	toMesh.vertices=fromMesh.vertices;
	toMesh.normals=fromMesh.normals;
	toMesh.uv=fromMesh.uv;
	toMesh.triangles=fromMesh.triangles;
}