using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

public class ComputeManager : MonoBehaviour
{
	public int instanceCount;

	public Mesh mesh;
	public Material material;

	public ShadowCastingMode castShadows = ShadowCastingMode.Off;
	public bool receiveShadows;

	public ComputeShader computeShader;

	private int gravityKernelID;
	private int velocityKernelID;

	private ComputeBuffer positionBuffer;
	private ComputeBuffer velocityBuffer;

	private ComputeBuffer colorBuffer;

	private ComputeBuffer argsBuffer;
	private float speed = 1;

	[GradientUsage(true)] public Gradient gradient;

	private uint[] args = { 0, 0, 0, 0, 0 };

	private const float G = 6.67408f * 0.00000000001f;

	public void Start()
	{
		gravityKernelID = computeShader.FindKernel("CSGravityKernel");
		velocityKernelID = computeShader.FindKernel("CSVelocityKernel");

		positionBuffer = new ComputeBuffer(instanceCount, Marshal.SizeOf(typeof(Vector4)));
		velocityBuffer = new ComputeBuffer(instanceCount, Marshal.SizeOf(typeof(Vector4)));
		colorBuffer = new ComputeBuffer(instanceCount, Marshal.SizeOf(typeof(Vector4)));

		Vector4[] positionData = new Vector4[instanceCount];
		Vector4[] velocityData = new Vector4[instanceCount];
		Vector4[] colorData = new Vector4[instanceCount];

		for (int i = 0; i < instanceCount; i++)
		{
			positionData[i] = Random.insideUnitCircle * 5f;
			positionData[i].w = Random.Range(0.5f, 1.5f);

			//velocityData[i] = Vector3.Normalize(Random.insideUnitCircle) * 0.00001f;

			colorData[i] = gradient.Evaluate(1f - (positionData[i].w - 0.5f));
		}

		positionBuffer.SetData(positionData);
		velocityBuffer.SetData(velocityData);
		colorBuffer.SetData(colorData);

		material.SetBuffer("positionBuffer", positionBuffer);
		material.SetBuffer("colorBuffer", colorBuffer);

		mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10000f);

		computeShader.SetBuffer(gravityKernelID, "positionBuffer", positionBuffer);
		computeShader.SetBuffer(gravityKernelID, "velocityBuffer", velocityBuffer);

		computeShader.SetBuffer(velocityKernelID, "positionBuffer", positionBuffer);
		computeShader.SetBuffer(velocityKernelID, "velocityBuffer", velocityBuffer);

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
		computeShader.SetFloat("_Speed", speed);

		computeShader.Dispatch(gravityKernelID, instanceCount / 64, 1, 1);
		computeShader.Dispatch(velocityKernelID, instanceCount / 64, 1, 1);
	}

	public void OnDisable()
	{
		positionBuffer?.Release();
		positionBuffer = null;

		velocityBuffer?.Release();
		velocityBuffer = null;

		colorBuffer?.Release();
		colorBuffer = null;

		argsBuffer?.Release();
		argsBuffer = null;
	}

	public void OnGUI()
	{
		GUI.Box(new Rect(0, 0, 216, 48), GUIContent.none);
		speed = GUI.HorizontalSlider(new Rect(8, 8, 200, 20), speed, 1f, (float)1E3);
		GUI.Label(new Rect(8, 20, 200, 30), $"Time sped up by: {speed:N0}x{1000}");
	}
}