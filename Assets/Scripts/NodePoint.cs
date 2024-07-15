using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodePoint : MonoBehaviour
{
    [SerializeField] private SpriteRenderer nodeSprite;
    [SerializeField] private CircleCollider2D collider;

    private Vector3 coordinates = new Vector3();
    private bool isBlocked = false;
    private string nodeTag;

    public Vector3 Coordinates => coordinates;
    public bool IsBlocked => isBlocked;
    public string NodeTag => nodeTag;

    public void Init(Vector3 coordinates, bool isBlocked)
    {
        nodeTag = $"({coordinates.x},{coordinates.y})";
        name = nodeTag;
        this.coordinates = coordinates;
        this.isBlocked = isBlocked;
    }

    private void OnMouseDown()
    {
        var catPos = GridManager.Instance.Cat.position;
        catPos = new Vector3(Mathf.Round(catPos.x), Mathf.Round(catPos.y));

        if (transform.position == catPos)
            return;
        // Change the color of the sprite renderer;
        if (nodeSprite == null)
            nodeSprite = GetComponent<SpriteRenderer>();

        nodeSprite.color = Color.black;
        GridManager.Instance.UpdateBlockedState(coordinates);
        GridManager.Instance.OnNodeClick?.Invoke();
        collider.enabled = false;
    }

    public void EnableBlockState()
    {
        isBlocked = true;
    }
}
