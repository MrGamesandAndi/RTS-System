using RTSSystem.Buildables;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace RTSSystem.UI
{
    public class UICategoryPicker : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _categoryLabel;

        public UnityEvent<BuildableObjectBaseSO.Type> OnCategorySelected = new();

        public BuildableObjectBaseSO.Type CategoryType { get; private set; } = BuildableObjectBaseSO.Type.NotSet;

        public void Bind(BuildableObjectBaseSO.Type inCategoryType)
        {
            CategoryType = inCategoryType;
            _categoryLabel.text = CategoryType.ToString();
        }

        public void OnButtonSelected()
        {
            OnCategorySelected.Invoke(CategoryType);
        }
    }
}