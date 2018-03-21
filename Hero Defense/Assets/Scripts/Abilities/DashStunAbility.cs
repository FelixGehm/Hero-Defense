using UnityEngine;
using System.Collections;

public class DashStunAbility : MonoBehaviour
{

    static float abilityCooldown = 4.0f;
    float currentCooldown = abilityCooldown;

    public float dashRange = 3.0f;
    public float dashSpeed = 1.0f;

    // Use this for initialization
    void Start()
    {
        GetComponent<CharacterEventManager>().OnAbilityTwo += Cast;

    }

    void Update()
    {
        currentCooldown -= Time.deltaTime;
    }

    public void Cast()
    {
        Debug.Log("Cast(): DashStun");
        if (currentCooldown <= 0)
        {
            currentCooldown = abilityCooldown;

            Dash();
            Stun();
        }
    }

    private void Dash()
    {
        transform.position += dashSpeed * GetDirectionVectorBetweenPlayerAndMouse();
    }

    private void Stun()
    {

    }

    private Vector3 GetDirectionVectorBetweenPlayerAndMouse()
    {
        Vector3 playerPos = transform.position;
        Vector3 mousePos = Input.mousePosition;
        return Vector3.Normalize(mousePos - playerPos);
    }
}
