using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClothMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject _iconPrefab;
    [SerializeField] private Transform _iconsContainer;
    private ClothScriptableObject[] _clothsData;
    private List<Button> _instantiatedButtons = new List<Button>();
    public event Action<int> OnClothIconClicked;
    public void Init(ClothScriptableObject[] cloth)
    {
        _clothsData = cloth;

        CreateSlots();
    }

    private void CreateSlots()
    {
        for (int i = 0; i < _clothsData.Length; i++)
        {
            GameObject iconInstance = Instantiate(_iconPrefab, _iconsContainer);

            Image imageComponent = iconInstance.GetComponent<Image>();
            imageComponent.sprite = _clothsData[i].icon;

            Button button = iconInstance.GetComponentInChildren<Button>();
            _instantiatedButtons.Add(button);
            int clothIndex = i;
            button.onClick.AddListener(() => OnClothIconClicked?.Invoke(clothIndex));       
        }
    }

    private void OnDestroy()
    {
        foreach (Button button in _instantiatedButtons)
        {
            button.onClick.RemoveAllListeners();
        }
        _instantiatedButtons.Clear();
    }
}
