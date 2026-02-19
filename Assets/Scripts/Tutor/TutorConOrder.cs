using UnityEngine;
using UnityEngine.SceneManagement;

public class Tutorial : MonoBehaviour
{
    [Tooltip("Имя сцены или индекс для загрузки")]
    [SerializeField] private string _sceneToLoad = "";

    [Tooltip("Если true — загрузка будет асинхронной")]
    [SerializeField] private bool _useAsync = true;

    [Tooltip("Задержка перед загрузкой (сек)")]
    [SerializeField] private float _delay = 0f;

    private bool _isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_isTriggered) return;

        if (other.CompareTag("Player"))
        {
            _isTriggered = true;
            if (_delay > 0f)
                StartCoroutine(DelayedLoad());
            else
                LoadTargetScene();
        }
    }

    private System.Collections.IEnumerator DelayedLoad()
    {
        yield return new WaitForSeconds(_delay);
        LoadTargetScene();
    }

    private void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(_sceneToLoad))
        {
            Debug.LogWarning($"TutorConOrder: не задана сцена для загрузки на GameObject '{gameObject.name}'");
            _isTriggered = false;
            return;
        }

        if (_useAsync)
        {
            SceneManager.LoadSceneAsync(_sceneToLoad);
        }
        else
        {
            SceneManager.LoadScene(_sceneToLoad);
        }
    }
}
