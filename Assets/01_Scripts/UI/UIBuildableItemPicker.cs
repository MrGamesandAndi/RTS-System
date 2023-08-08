using RTSSystem.Buildables;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RTSSystem.UI
{
    public class UIBuildableItemPicker : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _itemLabel;
        [SerializeField] GameObject _numQueuedPanel;
        [SerializeField] TextMeshProUGUI _numQueuedLabel;
        [SerializeField] Image _itemImage;
        [SerializeField] Image _inProgressIndicator;

        public UnityEvent<BuildableObjectBaseSO> OnItemSelected = new();
        public UnityEvent<BuildableObjectBaseSO> OnItemCancelled = new();

        public BuildableObjectBaseSO ItemSO { get; private set; } = null;

        public void Bind(BuildableObjectBaseSO inItemSO)
        {
            ItemSO = inItemSO;
            _itemLabel.text = ItemSO.ObjectName;
            _itemImage.sprite = ItemSO.UIImage;
            _numQueuedPanel.SetActive(false);
        }

        public void ClearProgress()
        {
            _inProgressIndicator.fillAmount = 0f;
        }

        public void SetProgress(float inAmount)
        {
            _inProgressIndicator.fillAmount = 1f - inAmount;
        }

        public void SetNumQueued(int inNumQueued)
        {
            if (inNumQueued > 1)
            {
                _numQueuedLabel.text = inNumQueued.ToString();
                _numQueuedPanel.SetActive(true);
            }
            else
            {
                _numQueuedPanel.SetActive(false);
            }
        }

        public void OnButtonSelected(BaseEventData inPointerEventData)
        {
            var pointerEventData = inPointerEventData as PointerEventData;

            if (pointerEventData.button == PointerEventData.InputButton.Left)
            {
                OnItemSelected.Invoke(ItemSO);
            }
            else if (pointerEventData.button == PointerEventData.InputButton.Right)
            {
                OnItemCancelled.Invoke(ItemSO);
            }
        }
    }
}