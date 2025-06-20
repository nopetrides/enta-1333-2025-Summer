using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Targeting : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;   // Movement speed in units per second

  
    public void StartMoving(List<GridNode> path)
    {
        StopAllCoroutines();                         // Stop any ongoing movement
        StartCoroutine(MoveAlongPath(path));         // Start moving along the new path
    }

    private IEnumerator MoveAlongPath(List<GridNode> path)
    {
        foreach (GridNode node in path)
        {
            Vector3 target = node.WorldPosition + Vector3.up * 0.5f;   // Slightly read the target position
            while (Vector3.Distance(transform.position, target) > 0.05f)
            {
                // Move towards the target node
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                yield return null;            // Wait for the next frame
            }
        }
    }
}
