using UnityEngine;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour
{
	private static SceneManager Instance;

	private string previousScene;

	private void Awake()
	{
		if (Instance == null) Instance = this;
		else if (Instance != this) Destroy(gameObject);

		DontDestroyOnLoad(this);

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