using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICursor : MonoBehaviour
{
    [Header("Standard Cursors")]
    public Texture2D moveCursor;
    public Texture2D friendlyCursor;
    public Texture2D attackCursor;
    [Space]
    [Header("Selection Cursors")]
    public Texture2D defaultSelection;
    public Texture2D friendlySelection;
    public Texture2D enemySelection;
    [Space]
    public GameObject movementClickEffectPrefab;
    private GameObject movementClickEffectGO;
    private ParticleSystem movementClickEffectPs;

    [HideInInspector]
    public bool isSelecting = false;

    #region Singleton
    public static UICursor instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one intance of GameManager!");
            return;
        }
        instance = this;
    }
    #endregion

    // Use this for initialization
    void Start()
    {
        SetMoveCursor();
        movementClickEffectGO = Instantiate(movementClickEffectPrefab);
        movementClickEffectPs = movementClickEffectGO.GetComponent<ParticleSystem>();
    }

    public void SetMoveCursor()
    {
        if (!isSelecting)
        {
            Cursor.SetCursor(moveCursor, Vector2.zero, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(defaultSelection, new Vector2(defaultSelection.width / 2, defaultSelection.height / 2), CursorMode.Auto);
        }

    }

    public void SetAttackCursor()
    {
        if (!isSelecting)
        {
            Cursor.SetCursor(attackCursor, Vector2.zero, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(enemySelection, new Vector2(enemySelection.width / 2, enemySelection.height / 2), CursorMode.Auto);
        }

    }

    public void SetFriendlyCursor()
    {
        if (!isSelecting)
        {
            Cursor.SetCursor(friendlyCursor, Vector2.zero, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(friendlySelection, new Vector2(friendlySelection.width / 2, friendlySelection.height / 2), CursorMode.Auto);
        }
    }

    public void PlayMovementClickEffectAtPosition(Vector3 pos)
    {
        movementClickEffectGO.transform.position = pos;
        movementClickEffectPs.Play();
    }
}
