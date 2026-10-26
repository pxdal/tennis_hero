using UnityEngine;

public static class BoxUtils
{
    public static RaycastHit2D[] BoxCastAll(
            this BoxCollider2D box, 
            Vector2 direction, 
            float distance = Mathf.Infinity, 
            int layerMask = Physics2D.DefaultRaycastLayers,
            float minDepth = -Mathf.Infinity,
            float maxDepth = Mathf.Infinity
        )
    {
        Vector2 origin = box.transform.position;
        Vector2 size = box.size;

        float angle = box.transform.rotation.z;

        return Physics2D.BoxCastAll(origin, size, angle, direction, distance, layerMask, minDepth, maxDepth);
    }

    public static RaycastHit2D[] BoxCastUpToEndpoint(
            this BoxCollider2D box,
            Vector2 endPoint,
            int layerMask = Physics2D.DefaultRaycastLayers,
            float minDepth = -Mathf.Infinity,
            float maxDepth = Mathf.Infinity
        )
    {

        Vector2 origin = box.transform.position;

        Vector2 direction = (endPoint - origin);
        float distance = direction.magnitude;
        direction.Normalize();

        return box.BoxCastAll(direction, distance, layerMask, minDepth, maxDepth);
    }
}
