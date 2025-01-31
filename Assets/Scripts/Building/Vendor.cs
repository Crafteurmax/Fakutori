using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vendor : Building
{
    [SerializeField] BuildingInput vendorInput;

    [Header("Paper")]
    [SerializeField] private int maximumNumberOfPapers;
    private int currentNumberOfPapers = 0;
    [SerializeField] private List<GameObject> papers = new List<GameObject>();
    private int displayedPaperIndex = 0;
    [SerializeField] private GameObject displayedPapersParent;
    float timer = 0f;
    [SerializeField] private float timeToDisapear;

    private void Awake() {
        vendorInput.Initialize();
        BuildingManager.Instance.AddBuildingInput(vendorInput.GetPosition(), vendorInput);
        SetBuildingType(BuildingType.Vendor);

        GetComponentInChildren<BuildingInput>().SetIsBeltInput(true);
    }

    private void Update() 
    {
        Item item = vendorInput.GetItem();

        if (item != null)
        {
            if (timer == 0f) {
                GoalController.Instance.IncreaseScore(item.ToString());
                

                // Papers
                currentNumberOfPapers++;
                if (currentNumberOfPapers <= papers.Count) {
                    item.transform.position = papers[displayedPaperIndex].transform.position;
                    item.transform.rotation = papers[displayedPaperIndex].transform.rotation;
                    item.transform.SetParent(displayedPapersParent.transform);
                    item.transform.localScale = papers[displayedPaperIndex].transform.localScale;

                    displayedPaperIndex = (displayedPaperIndex + 1) % papers.Count;
                }                
                item.enabled = false;
                
                if (currentNumberOfPapers == maximumNumberOfPapers) {
                    animator.SetTrigger("Produce");
                    timer = animationTime;
                } else {
                    vendorInput.SetItem(null);
                }
            } else {
                timer -= Time.deltaTime;
                if (timer < (animationTime - timeToDisapear)) {
                    foreach (Transform child in displayedPapersParent.transform) {
                        child.gameObject.GetComponent<Item>().SetInvisible(true);
                    }
                }
                if (timer < 0f) {
                    timer = 0f;
                    vendorInput.SetItem(null);
                    Transform child = displayedPapersParent.transform.childCount != 0 ? displayedPapersParent.transform.GetChild(0) : null;
                    while (child != null) {
                        Item childItem = child.gameObject.GetComponent<Item>();
                        childItem.enabled = true;
                        childItem.SetInvisible(false);
                        childItem.ResetRotationAndScale();

                        childItem.transform.SetParent(ItemFactory.Instance.transform);

                        ItemFactory.Instance.Release(childItem);
                        child = displayedPapersParent.transform.childCount != 0 ? displayedPapersParent.transform.GetChild(0) : null;
                    }
                    currentNumberOfPapers = 0;
                }
            }

            

            
        }
    }

    public override void Release()
    {
        BuildingManager.Instance.RemoveBuildingInput(vendorInput.GetPosition());

        vendorInput.Reset();
        base.Release();
    }
}
