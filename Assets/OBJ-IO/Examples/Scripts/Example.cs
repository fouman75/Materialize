using System.IO;
using Plugins.Extension;
using Plugins.Mesh.OBJ;
using UnityEngine;

namespace Examples.Scripts
{
	[RequireComponent(typeof(MeshFilter))]
	public class Example : MonoBehaviour
	{
		//------------------------------------------------------------------------------------------------------------	
		private const string INPUT_PATH = @"Assets/OBJ-IO/Examples/Meshes/Teapot.obj";
		private const string OUTPUT_PATH = @"Assets/OBJ-IO/Examples/Meshes/Teapot_Modified.obj";

		//------------------------------------------------------------------------------------------------------------	
		private void Start()
		{
			//	Load the OBJ in
			var lStream = new FileStream(INPUT_PATH, FileMode.Open);
			var lOBJData = OBJLoader.LoadOBJ(lStream);
			var lMeshFilter = GetComponent<MeshFilter>();
			lMeshFilter.mesh.LoadOBJ(lOBJData);
			lStream.Close();
		
			lStream = null;
			lOBJData = null;

			//	Wiggle Vertices in Mesh
			var lVertices = lMeshFilter.mesh.vertices;
			for (int lCount = 0; lCount < lVertices.Length; ++lCount)
			{
				lVertices[lCount] = lVertices[lCount] + Vector3.up * Mathf.Sin(lVertices[lCount].x) * 4f;
			}
			lMeshFilter.mesh.vertices = lVertices;

			//	Export the new Wiggled Mesh
			if (File.Exists(OUTPUT_PATH))
			{
				File.Delete(OUTPUT_PATH);
			}
			lStream = new FileStream(OUTPUT_PATH, FileMode.Create);
			lOBJData = lMeshFilter.mesh.EncodeOBJ();
			OBJLoader.ExportOBJ(lOBJData, lStream);
			lStream.Close();
		}
	}
}