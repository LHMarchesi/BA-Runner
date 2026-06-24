using UnityEngine;

public class LeaderboardUI : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private LeaderboardItem itemPrefab;

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        foreach (Transform child in content)
            Destroy(child.gameObject);

        var entries = LeaderBoardManager.Entries;

        for (int i = 0; i < entries.Count; i++)
        {
            var item =
                Instantiate(itemPrefab, content);

            item.Setup(i + 1, entries[i]);
        }
    }
}