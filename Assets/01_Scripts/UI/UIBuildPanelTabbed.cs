using RTSSystem.Buildables;
using RTSSystem.Builders;
using System.Collections.Generic;
using UnityEngine;

namespace RTSSystem.UI
{
    public class UIBuildPanelTabbed : MonoBehaviour
    {
        [SerializeField] BuilderBase _defaultBuilder;
        [SerializeField] GameObject _categoryUIPrefab;
        [SerializeField] GameObject _itemUIPrefab;
        [SerializeField] Transform _categoryUIRoot;
        [SerializeField] Transform _itemUIRoot;

        [Header("DEBUG ONLY")]
        [SerializeField] BuilderBase DEBUG_NewBuilderToSet;
        [SerializeField] bool DEBUG_SetNewBuilder = false;

        BuilderBase _linkedBuilder = null;
        BuildableObjectBaseSO.Type _selectedCategory = BuildableObjectBaseSO.Type.Building;
        Dictionary<BuildableObjectBaseSO, UIBuildableItemPicker> _itemPickerUIMap = new();

        private void Start()
        {
            SetBuilder(_defaultBuilder);
        }

        private void Update()
        {
            if (DEBUG_SetNewBuilder)
            {
                DEBUG_SetNewBuilder = false;
                SetBuilder(DEBUG_NewBuilderToSet);
            }
        }

        public void SetBuilder(BuilderBase inBuilder)
        {
            if (_linkedBuilder != null)
            {
                //Clean out existing category UI
                for (int childIndex = _categoryUIRoot.childCount - 1; childIndex >= 0 ; childIndex--)
                {
                    var childGO = _categoryUIRoot.GetChild(childIndex).gameObject;
                    Destroy(childGO);
                }

                //Remove listeners
                _linkedBuilder.OnBuildQueued.RemoveListener(OnBuildQueued);
                _linkedBuilder.OnBuildStarted.RemoveListener(OnBuildStarted);
                _linkedBuilder.OnBuildPaused.RemoveListener(OnBuildPaused);
                _linkedBuilder.OnBuildResumed.RemoveListener(OnBuildResumed);
                _linkedBuilder.OnBuildCancelled.RemoveListener(OnBuildCancelled);
                _linkedBuilder.OnBuildTicked.RemoveListener(OnBuildTicked);
                _linkedBuilder.OnBuildCompleted.RemoveListener(OnBuildCompleted);
            }

            _linkedBuilder = inBuilder;

            if (_linkedBuilder != null)
            {
                _linkedBuilder.OnBuildQueued.AddListener(OnBuildQueued);
                _linkedBuilder.OnBuildStarted.AddListener(OnBuildStarted);
                _linkedBuilder.OnBuildPaused.AddListener(OnBuildPaused);
                _linkedBuilder.OnBuildResumed.AddListener(OnBuildResumed);
                _linkedBuilder.OnBuildCancelled.AddListener(OnBuildCancelled);
                _linkedBuilder.OnBuildTicked.AddListener(OnBuildTicked);
                _linkedBuilder.OnBuildCompleted.AddListener(OnBuildCompleted);
            }

            RefreshUI(true);
        }

        private void OnCategorySelected(BuildableObjectBaseSO.Type inCategoryType)
        {
            _selectedCategory = inCategoryType;
            RefreshUI(false);
        }

        private void OnBuildQueued(BuilderBase.BuildData inBuildData)
        {
            RefreshItemUIQueuedInformation(inBuildData.ObjectBeingBuilt);
        }

        private void OnBuildStarted(BuilderBase.BuildData inBuildData)
        {
            RefreshItemUIQueuedInformation(inBuildData.ObjectBeingBuilt);
        }

        private void OnBuildPaused(BuilderBase.BuildData inBuildData)
        {

        }

        private void OnBuildResumed(BuilderBase.BuildData inBuildData)
        {

        }

        private void OnBuildCancelled(BuilderBase.BuildData inBuildData)
        {
            RefreshItemUIQueuedInformation(inBuildData.ObjectBeingBuilt);
        }

        private void OnBuildTicked(BuilderBase.BuildData inBuildData)
        {
            UIBuildableItemPicker buildableItemUI = null;

            if (_itemPickerUIMap.TryGetValue(inBuildData.ObjectBeingBuilt, out buildableItemUI))
            {
                buildableItemUI.SetProgress(inBuildData.CurrentBuildProgress);
            }
        }

        private void OnBuildCompleted(BuilderBase.BuildData inBuildData)
        {
            RefreshItemUIQueuedInformation(inBuildData.ObjectBeingBuilt);
        }

        private void OnBuildableItemSelected(BuildableObjectBaseSO inBuildableItem)
        {
            _linkedBuilder.RequestToQueueBuild(inBuildableItem);
        }

        private void OnBuildableItemCancelled(BuildableObjectBaseSO inBuildableItem)
        {
            if (_linkedBuilder.RequestToCancelBuild(inBuildableItem))
            {
                _itemPickerUIMap[inBuildableItem].ClearProgress();
            }
        }

        private void RefreshItemUIQueuedInformation(BuildableObjectBaseSO inBuildableItem)
        {
            UIBuildableItemPicker buildableItemUI = null;

            if (_itemPickerUIMap.TryGetValue(inBuildableItem, out buildableItemUI))
            {
                buildableItemUI.SetNumQueued(_linkedBuilder.GetNumberOfItemQueued(inBuildableItem));
            }
        }

        private void RefreshUI(bool regenerateCategoryUI)
        {
            _itemPickerUIMap.Clear();

            //Clean out existing UI
            for (int childIndex = _itemUIRoot.childCount - 1; childIndex >= 0; childIndex--)
            {
                var childGO = _itemUIRoot.GetChild(childIndex).gameObject;
                Destroy(childGO);
            }

            if (_linkedBuilder == null)
            {
                return;
            }

            //Check if the category UI needs to spawn
            if (regenerateCategoryUI)
            {
                BuildableObjectBaseSO.Type previouslySelectedCategory = _selectedCategory;
                _selectedCategory = BuildableObjectBaseSO.Type.NotSet;
                var rawCategoryTypes = System.Enum.GetValues(typeof(BuildableObjectBaseSO.Type));

                foreach (var rawCategoryType in rawCategoryTypes)
                {
                    BuildableObjectBaseSO.Type categoryType = (BuildableObjectBaseSO.Type)rawCategoryType;

                    if (categoryType == BuildableObjectBaseSO.Type.NotSet)
                    {
                        continue;
                    }

                    if (_linkedBuilder.GetBuildableItemsForType(categoryType) == null)
                    {
                        continue;
                    }

                    if (_selectedCategory == BuildableObjectBaseSO.Type.NotSet)
                    {
                        _selectedCategory = categoryType;
                    }

                    if (previouslySelectedCategory == categoryType)
                    {
                        _selectedCategory = previouslySelectedCategory;
                    }

                    var categoryUI = Instantiate(_categoryUIPrefab, _categoryUIRoot);
                    var categoryUILogic = categoryUI.GetComponent<UICategoryPicker>();
                    categoryUILogic.Bind(categoryType);
                    categoryUILogic.OnCategorySelected.AddListener(OnCategorySelected);
                }
            }      

            //Get the available items to build
            var availableBuildables = _linkedBuilder.GetBuildableItemsForType(_selectedCategory);

            if (availableBuildables == null)
            {
                return;
            }

            //Spawn UI
            foreach (var availableBuildable in availableBuildables)
            {
                var itemUI = Instantiate(_itemUIPrefab, _itemUIRoot);
                var itemUILogic = itemUI.GetComponent<UIBuildableItemPicker>();
                _itemPickerUIMap[availableBuildable] = itemUILogic;
                itemUILogic.Bind(availableBuildable);
                itemUILogic.OnItemSelected.AddListener(OnBuildableItemSelected);
                itemUILogic.OnItemCancelled.AddListener(OnBuildableItemCancelled);
                itemUILogic.SetNumQueued(_linkedBuilder.GetNumberOfItemQueued(availableBuildable));
            }
        }
    }
}