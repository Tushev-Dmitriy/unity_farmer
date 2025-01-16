using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    [SerializeField] GameObject inventoryUI;
    [SerializeField] Inventory inventory;
    [SerializeField] List<GameObject> slotObjects;
    [SerializeField] GameObject bedSlot;
    [SerializeField] GameObject itemPrefab;
    
    int MaxStackSize = 32;
    Transform bedInventoryUI;

    public static Action<InventoryItem> onBedBtnClick;

    private void Awake()
    {
        bedInventoryUI = inventoryUI.transform.GetChild(3);
    }
    
    private void OnEnable()
    {
        BedWork.onBedClick += OnBedClicked;
    }

    private void OnDisable()
    {
        BedWork.onBedClick -= OnBedClicked;
    }
    
    void OnBedClicked(bool onlySeed, string actionBtnText)
    {
        UpdateSlots(onlySeed, actionBtnText);
    }
    
    private void CreateInventoryItem(GameObject slot, InventorySystem itemData)
    {
        var tempItem = Instantiate(itemPrefab, slot.transform);
        var inventoryItem = tempItem.GetComponent<InventoryItem>();
        var itemIcon = itemData.isSeed ? itemData.item.seedIcon : itemData.item.icon;
        inventoryItem.SetupSlot(itemIcon, itemData.count, itemData.item);
    }
    
    void UpdateSlots(bool onlySeed, string actionBtnText)
    {
        ClearSlots();

        for (int i = 0; i < inventory.inventorySystem.Count; i++)
        {
            if (i < slotObjects.Count)
            {
                InventorySystem itemData = inventory.inventorySystem[i];
                GameObject slot = slotObjects[i];

                if (onlySeed && itemData.isSeed)
                {
                    CreateInventoryItem(slot, itemData);
                }
                else if (!onlySeed)
                {
                    CreateInventoryItem(slot, itemData);
                }
            }
        }

        if (onlySeed)
        {
            inventoryUI.transform.localPosition = new Vector3(200, 0, 0);
            bedInventoryUI.gameObject.SetActive(true);
            
            var button = bedInventoryUI.GetChild(1).GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(SubToBedBtnEvent);
            bedInventoryUI.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = actionBtnText;
        }
        else
        {
            inventoryUI.transform.localPosition = Vector3.zero;
            bedInventoryUI.gameObject.SetActive(false);
        }

        inventoryUI.SetActive(true);
    }
    
    private void AddItemToSlots(FlowerData item, int count)
    {
        while (count > 0)
        {
            bool addToExistSlot = false;
            foreach (var slotObject in slotObjects)
            {
                if (slotObject.transform.childCount == 0) continue;

                var inventoryItem = slotObject.transform.GetChild(0).GetComponent<InventoryItem>();
                if (inventoryItem == null) continue;

                var itemIcon = inventoryItem.GetComponent<Image>().sprite;
                if ((itemIcon == item.icon || itemIcon == item.seedIcon) && inventoryItem.itemCount < MaxStackSize)
                {
                    int spaceLeft = MaxStackSize - inventoryItem.itemCount;
                    int amountToAdd = Mathf.Min(count, spaceLeft);
                    inventoryItem.IncreaseCount(amountToAdd);
                    count -= amountToAdd;
                    addToExistSlot = true;
                    break;
                }
            }

            if (!addToExistSlot)
            {
                foreach (var slotObject in slotObjects)
                {
                    if (slotObject.transform.childCount == 0)
                    {
                        CreateInventoryItem(slotObject, new InventorySystem { item = item, count = Mathf.Min(count, MaxStackSize) });
                        count -= Mathf.Min(count, MaxStackSize);
                        break;
                    }
                }
            }

            if (!addToExistSlot && count > 0)
            {
                Debug.Log("������ ���������");
                break;
            }
        }

        UpdateSlots(false, "1");
    }

    void SubToBedBtnEvent()
    {
        if (bedSlot.transform.childCount > 0)
        {
            var inventoryItem = bedSlot.transform.GetChild(0).GetComponent<InventoryItem>();
            if (inventoryItem.itemCount >= 4)
            {
                onBedBtnClick?.Invoke(inventoryItem);
            }
        }
    }

    void ClearSlots()
    {
        foreach (GameObject slotObject in slotObjects)
        {
            if (slotObject.transform.childCount > 0)
            {
                Destroy(slotObject.transform.GetChild(0).gameObject);
            }
        }

        if (bedSlot.transform.childCount > 0)
        {
            Destroy(bedSlot.transform.GetChild(0).gameObject);
        }
    }

}
