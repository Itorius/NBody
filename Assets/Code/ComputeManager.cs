using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

public class ComputeManager : MonoBehaviour
{
	public int instanceCount = 128;

	public Mesh mesh;
	public Material material;

	public ShadowCastingMode castShadows = ShadowCastingMode.Off;
	public bool receiveShadows;

	public ComputeShader computeShader;
	private int kernelID;

	private ComputeBuffer positionBuffer;

	private ComputeBuffer argsBuffer;
	private ComputeBuffer colorBuffer;

	private uint[] args = { 0, 0, 0, 0, 0 };

	public void Start()
	{
		kernelID = computeShader.FindKernel("CSGravityKernel");

		positionBuffer = new ComputeBuffer(instanceCount, Marshal.SizeOf(typeof(Vector4)));
		colorBuffer = new ComputeBuffer(instanceCount, Marshal.SizeOf(typeof(Vector4)));

		Vector4[] positionData = new Vector4[instanceCount];
		Vector4[] colorData = new Vector4[instanceCount];
		for (int i = 0; i < instanceCount; i++)
		{
			positionData[i] = Random.insideUnitSphere * 10f;
			positionData[i].w = Random.Range(1f, 2f);
			colorData[i] = Random.ColorHSV();
		}

		positionBuffer.SetData(positionData);
		colorBuffer.SetData(colorData);

		material.SetBuffer("positionBuffer", positionBuffer);
		material.SetBuffer("colorBuffer", colorBuffer);

		mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10000f);

		computeShader.SetBuffer(kernelID, "positionBuffer", positionBuffer);

		argsBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
		args[0] = mesh != null ? mesh.GetIndexCount(0) : 0;
		args[1] = (uint)instanceCount;
		argsBuffer.SetData(args);
	}

	public void Update()
	{
		UpdateBuffers();

		Graphics.DrawMeshInstancedIndirect(mesh, 0, material, mesh.bounds, argsBuffer, 0, null, castShadows, receiveShadows);
	}

	public void UpdateBuffers()
	{
		computeShader.SetFloat("_Time", Time.deltaTime);

		computeShader.Dispatch(kernelID, instanceCount / 64, 1, 1);
	}

	public void OnDisable()
	{
		positionBuffer?.Release();
		positionBuffer = null;

		//colorBuffer?.Release();
		//colorBuffer = null;

		argsBuffer?.Release();
		argsBuffer = null;
	}

	public void OnGUI()
	{
		GUI.Label(new Rect(265, 12, 200, 30), "Instance Count: " + instanceCount.ToString("N0"));
	}
}