using UnityEngine;

public class CloneSkill : MonoBehaviour
{
    public GameObject clonePrefab;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            GameObject clone = Instantiate(
                clonePrefab,
                transform.position,
                Quaternion.identity);

            clone.GetComponent<CloneFollow>().player = transform;

            Destroy(clone,10f);
        }
    }
}