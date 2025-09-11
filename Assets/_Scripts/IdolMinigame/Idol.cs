using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Idol : MonoBehaviour
{

    public float range = 4f;
    public GameObject rangeIndicatorPrefab;


    public void Capture()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, range);
        GameObject ind = Instantiate(rangeIndicatorPrefab, transform);
        ind.transform.localScale = new Vector3(range * 2, 0.1f, range * 2);
        foreach (var hitCollider in hitColliders)
        {
            GamePiece u = hitCollider.gameObject.GetComponent<GamePiece>();
            if (u != null)
            {
                u.Convert();
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawSphere(transform.position, range);
    }







    void Update()
    {
        if (!MinigameManager.Instance.gameRunning)
        {
            Destroy(this.gameObject);
        }
    }


}
