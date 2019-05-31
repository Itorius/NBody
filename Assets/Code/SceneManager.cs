using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour
{
	public static SceneManager Instance;

	public Slider slider;
	public TextMeshProUGUI text;

	private string previousScene;
	public static int ParticleCount;

	public AnimationCurve sizeWeight;

	private void Awake()
	{
		if (Instance == null) Instance = this;
		else if (Instance != this) Destroy(gameObject);
		DontDestroyOnLoad(this);

		slider.onValueChanged.AddListener(value =>
		{
			ParticleCount = (int)value;
			text.text = $"{ParticleCount:N0}";
		});
		slider.onValueChanged.Invoke(128);

		UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, mode) =>
		{
			foreach (Button button in FindObjectsOfType<Button>())
			{
				SwitchSceneButton component;
				if ((component = button.GetComponent<SwitchSceneButton>()) != null)
				{
					button.onClick.AddListener(() =>
					{
						if (!string.IsNullOrWhiteSpace(component.Scene)) LoadScene(component.Scene);
						else GoBack();
					});
				}
			}
		};
	}

	public void LoadScene(string scene)
	{
		if (string.IsNullOrWhiteSpace(scene)) return;

		previousScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
		UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
	}

	public void GoBack() => LoadScene(previousScene);
}