using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trash : Building
{
    [SerializeField] BuildingInput trashInput;

    [Header("Paper")]
    [SerializeField] private List<GameObject> papers = new List<GameObject>();
    private int displayedPaperIndex = 0;
    [SerializeField] private GameObject papersParent;
    private GameObject displayedPaper;
    [SerializeField] private GameObject displayedPaperPosition;

    private void Awake() {
        trashInput.Initialize();
        BuildingManager.Instance.AddBuildingInput(trashInput.GetPosition(), trashInput);
        SetBuildingType(BuildingType.Trash);

        GetComponentInChildren<BuildingInput>().SetIsBeltInput(true);
    }

    private void Update() {
        Item item = trashInput.GetItem();
        if (item != null) {
            ItemFactory.Instance.Release(item);
            trashInput.SetItem(null);
            displayedPaperIndex = (displayedPaperIndex + 1) % papers.Count;
            Destroy(displayedPaper);
            displayedPaper = Instantiate(papers[displayedPaperIndex], displayedPaperPosition.transform.position, transform.rotation, papersParent.transform);
            if (displayedPaperIndex == 0) {
                animator.SetTrigger("Produce");
                Debug.Log("Trash emptied");
                // TODO : Play the sound of trash being emptied (like on windows)
            }
        }
    }

    public override void Release()
    {
        BuildingManager.Instance.RemoveBuildingInput(trashInput.GetPosition());

        trashInput.Reset();
        base.Release();
    }
}
