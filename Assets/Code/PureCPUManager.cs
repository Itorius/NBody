using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

public class PureCPUManager : MonoBehaviour
{
	public Mesh mesh;
	public Material material;
	[GradientUsage(true)] public Gradient gradient;

	private Vector4[] positionData;
	private ComputeBuffer positionBuffer;

	private Vector3[] velocityData;

	private Vector4[] colorData;
	private ComputeBuffer colorBuffer;

	private ComputeBuffer argsBuffer;
	private uint[] args = { 0, 0, 0, 0, 0 };

	private const float G = 6.67408e-11f;
	private float speed = 1;

	public void Start()
	{
		positionData = new Vector4[SceneManager.ParticleCount];
		positionBuffer = new ComputeBuffer(SceneManager.ParticleCount, Marshal.SizeOf<Vector4>());

		velocityData = new Vector3[SceneManager.ParticleCount];

		colorData = new Vector4[SceneManager.ParticleCount];
		colorBuffer = new ComputeBuffer(SceneManager.ParticleCount, Marshal.SizeOf<Vector4>());

		positionData[0] = Vector3.zero;
		positionData[0].w = 10000f;
		colorData[0] = Color.black;

		for (int i = 1; i < SceneManager.ParticleCount; i++)
		{
			positionData[i] = Random.insideUnitCircle * 100f;
			positionData[i].w = SceneManager.Instance.sizeWeight.Evaluate((float)i / SceneManager.ParticleCount);

			Vector3 vec = -(Vector3)positionData[i].normalized;
			vec *= Mathf.Sqrt(G * (positionData[0].w + positionData[i].w) / ((Vector3)positionData[i]).magnitude) * 0.001f;

			velocityData[i] = new Vector4(vec.y, -vec.x, 0, 0);

			colorData[i] = gradient.Evaluate(1f - positionData[i].w);
		}

		positionBuffer.SetData(positionData);
		material.SetBuffer("positionBuffer", positionBuffer);

		colorBuffer.SetData(colorData);
		material.SetBuffer("colorBuffer", colorBuffer);

		mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10000f);

		argsBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
		args[0] = mesh != null ? mesh.GetIndexCount(0) : 0;
		args[1] = (uint)SceneManager.ParticleCount;
		argsBuffer.SetData(args);
	}

	public void FixedUpdate()
	{
		for (int i = 0; i < positionData.Length; i++)
		{
			if (Mathf.Abs(positionData[i].w) < 0.0f) continue;

			for (int j = 0; j < positionData.Length; j++)
			{
				if (i == j || Mathf.Abs(positionData[j].w) < 0.0f) continue;

				float distance = Vector3.Distance(positionData[i], positionData[j]);

				if (distance < 0.01f)
				{
					positionData[i].w += positionData[j].w;
					positionData[j].w = 0;

					continue;
				}

				float force = G * positionData[i].w * positionData[j].w / (distance * distance);
				velocityData[i] += force / positionData[i].w * Vector3.Normalize(positionData[j] - positionData[i]);
			}
		}

		for (int i = 0; i < positionData.Length; i++)
		{
			positionData[i] += (Vector4)velocityData[i] * speed * 1000;
		}

		positionBuffer.SetData(positionData);
	}

	public void Update()
	{
		Graphics.DrawMeshInstancedIndirect(mesh, 0, material, mesh.bounds, argsBuffer, 0, null, ShadowCastingMode.Off, false);
	}

	public void OnDisable()
	{
		positionBuffer?.Dispose();
		positionBuffer = null;
		colorBuffer?.Dispose();
		colorBuffer = null;
		argsBuffer?.Dispose();
		argsBuffer = null;
	}

	public void OnGUI()
	{
		GUI.Box(new Rect(0, 0, 216, 48), GUIContent.none);
		speed = GUI.HorizontalSlider(new Rect(8, 8, 200, 20), speed, 1f, (float)1E3);
		GUI.Label(new Rect(8, 20, 200, 30), $"Time sped up by: {speed:N0}x{1000}");
	}
}