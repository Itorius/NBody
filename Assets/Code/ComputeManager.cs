using System.Runtime.InteropServices;
using UnityEngine;

public class ComputeManager : MonoBehaviour
{
	public int instanceCount = 128;

	//public Mesh mesh;
	//public Material material;

	//public ShadowCastingMode castShadows = ShadowCastingMode.Off;
	//public bool receiveShadows;

	public ComputeShader computeShader;
	private int kernelID;

	private ComputeBuffer positionBuffer;
	//private ComputeBuffer argsBuffer;
	//private ComputeBuffer colorBuffer;

	//private uint[] args = { 0, 0, 0, 0, 0 };

	public void Start()
	{
		kernelID = computeShader.FindKernel("CSGravityKernel");

		positionBuffer = new ComputeBuffer(instanceCount, Marshal.SizeOf(typeof(Vector4)));

		Vector4[] data = new Vector4[instanceCount];
		for (int i = 0; i < instanceCount; i++)
		{
			data[i] = Random.insideUnitSphere;
			data[i].w = Random.Range(1f, 5f);
		}

		positionBuffer.SetData(data);

		computeShader.SetBuffer(kernelID, "positionBuffer", positionBuffer);
		computeShader.Dispatch(kernelID, instanceCount / 64, 1, 1);

		//argsBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);

		//if (instanceCount < 1) instanceCount = 1;
		//instanceCount = Mathf.ClosestPowerOfTwo(instanceCount);

		//kernelID = computeShader.FindKernel("CSPositionKernel");
		//mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10000f);

		//positionBuffer?.Release();
		//colorBuffer?.Release();

		//positionBuffer = new ComputeBuffer(instanceCount, 16);
		//colorBuffer = new ComputeBuffer(instanceCount, 16);

		//// generate random colors
		//Vector4[] colors = new Vector4[instanceCount];
		//for (int i = 0; i < instanceCount; i++) colors[i] = Random.ColorHSV();
		//colorBuffer.SetData(colors);
		//// ---

		//material.SetBuffer("positionBuffer", positionBuffer);
		//material.SetBuffer("colorBuffer", colorBuffer);

		//uint numIndices = mesh != null ? mesh.GetIndexCount(0) : 0;
		//args[0] = numIndices;
		//args[1] = (uint)instanceCount;
		//argsBuffer.SetData(args);

		//computeShader.SetBuffer(kernelID, "positionBuffer", positionBuffer);
		//computeShader.SetFloat("_Dim", Mathf.Sqrt(instanceCount));
	}

	public void Update()
	{
		//UpdateBuffers();

		//Graphics.DrawMeshInstancedIndirect(mesh, 0, material, mesh.bounds, argsBuffer, 0, null, castShadows, receiveShadows);
	}

	public void UpdateBuffers()
	{
		//computeShader.SetFloat("_Time", Time.time);

		//computeShader.Dispatch(kernelID, instanceCount / 64, 1, 1);
	}

	public void OnDisable()
	{
		//positionBuffer?.Release();
		//positionBuffer = null;

		//colorBuffer?.Release();
		//colorBuffer = null;

		//argsBuffer?.Release();
		//argsBuffer = null;
	}

	public void OnGUI()
	{
		GUI.Label(new Rect(265, 12, 200, 30), "Instance Count: " + instanceCount.ToString("N0"));
	}
}