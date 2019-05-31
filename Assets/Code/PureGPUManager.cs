using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

public class PureGPUManager : MonoBehaviour
{
	public Mesh mesh;
	public Material material;

	public ComputeShader computeShader;

	private int gravityKernelID;
	private int velocityKernelID;

	private ComputeBuffer positionBuffer;
	private ComputeBuffer velocityBuffer;

	private ComputeBuffer colorBuffer;

	private ComputeBuffer argsBuffer;
	private float speed = 1;

	private const double G = 6.67408e-11f;

	[GradientUsage(true)] public Gradient gradient;

	private uint[] args = { 0, 0, 0, 0, 0 };

	public void Start()
	{
		gravityKernelID = computeShader.FindKernel("CSGravityKernel");
		velocityKernelID = computeShader.FindKernel("CSVelocityKernel");

		int stride = Marshal.SizeOf<Vector4>();
		positionBuffer = new ComputeBuffer(SceneManager.ParticleCount, stride);
		Vector4[] positionData = new Vector4[SceneManager.ParticleCount];

		velocityBuffer = new ComputeBuffer(SceneManager.ParticleCount, stride);
		Vector4[] velocityData = new Vector4[SceneManager.ParticleCount];

		colorBuffer = new ComputeBuffer(SceneManager.ParticleCount, stride);
		Vector4[] colorData = new Vector4[SceneManager.ParticleCount];

		positionData[0] = Vector3.zero;
		positionData[0].w = 10000f;
		colorData[0] = Color.black;

		for (int i = 1; i < SceneManager.ParticleCount; i++)
		{
			positionData[i] = Random.insideUnitCircle * 100f;
			positionData[i].w = SceneManager.Instance.sizeWeight.Evaluate((float)i / SceneManager.ParticleCount);

			Vector3 vec = -(Vector3)positionData[i].normalized;
			vec *= Mathf.Sqrt((float)(G * (positionData[0].w + positionData[i].w) / ((Vector3)positionData[i]).magnitude)) * 0.001f;

			velocityData[i] = new Vector4(vec.y, -vec.x, 0, 0);

			colorData[i] = gradient.Evaluate(1f - positionData[i].w);
		}

		positionBuffer.SetData(positionData);
		velocityBuffer.SetData(velocityData);
		colorBuffer.SetData(colorData);

		material.SetBuffer("positionBuffer", positionBuffer);
		material.SetBuffer("colorBuffer", colorBuffer);
		
		computeShader.SetBuffer(gravityKernelID, "positionBuffer", positionBuffer);
		computeShader.SetBuffer(gravityKernelID, "velocityBuffer", velocityBuffer);

		computeShader.SetBuffer(velocityKernelID, "positionBuffer", positionBuffer);
		computeShader.SetBuffer(velocityKernelID, "velocityBuffer", velocityBuffer);

		mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10000f);
		argsBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
		args[0] = mesh != null ? mesh.GetIndexCount(0) : 0;
		args[1] = (uint)SceneManager.ParticleCount;
		argsBuffer.SetData(args);
	}	

	public void Update()
	{
		UpdateBuffers();

		Graphics.DrawMeshInstancedIndirect(mesh, 0, material, mesh.bounds, argsBuffer, 0, null, ShadowCastingMode.Off, false);
	}

	public void UpdateBuffers()
	{
		computeShader.SetFloat("_Speed", speed * 1000);

		computeShader.Dispatch(gravityKernelID, Mathf.Clamp(SceneManager.ParticleCount / 64, 1, short.MaxValue), 1, 1);
		computeShader.Dispatch(velocityKernelID, Mathf.Clamp(SceneManager.ParticleCount / 64, 1, short.MaxValue), 1, 1);
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