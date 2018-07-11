using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MageQSpell : NetworkBehaviour
{
    [Header("Spell Settings")]
    public float spellDuration = 10;
    public float startSize = 0.1f;
    public float fullSize = 10;
    public float expandingSpeed = 10;
    public float dmgBuff = 50;
    public float armorBuff = 50;


    private float startTime;
    public Transform[] particlesTransform;

    void Start()
    {
        startTime = Time.time;
        transform.localScale += new Vector3(startSize, startSize, startSize);
        foreach (Transform p in particlesTransform)
        {
            p.localScale += new Vector3(startSize, startSize, startSize);
        }

    }

    // Update is called once per frame
    void Update()
    {
        ExpandToFullSize();
        if (Time.time - startTime >= spellDuration) Destroy(gameObject);
    }

    private bool isFullSize;
    void ExpandToFullSize()
    {

        if (!isFullSize)
        {
            float scaleFactor = expandingSpeed * Time.deltaTime;
            transform.localScale += new Vector3(scaleFactor, scaleFactor, scaleFactor);

            foreach (Transform p in particlesTransform)
            {
                p.localScale += new Vector3(scaleFactor, scaleFactor, scaleFactor);
            }


            if (transform.localScale.x >= fullSize) isFullSize = true;
        }
    }


    private GameObject player;
    private CharacterStats stats;
    private bool isBuffActive = false;
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player") && collision.gameObject.GetComponent<NetworkIdentity>().isLocalPlayer) //checks for only the local player
        {
            if (!isBuffActive)
            {
                Debug.Log("Buff added");
                player = collision.gameObject;
                stats = player.GetComponent<CharacterStats>();
                stats.physicalDamage.AddModifier(dmgBuff);
                stats.magicDamage.AddModifier(dmgBuff);
                stats.armor.AddModifier(armorBuff);
                isBuffActive = true;
            }
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player") && collision.gameObject.GetComponent<NetworkIdentity>().isLocalPlayer) //checks for only the local player
        {
            Debug.Log("Buff removed");
            if (isBuffActive)
            {
                stats.physicalDamage.RemoveModifier(dmgBuff);
                stats.magicDamage.RemoveModifier(dmgBuff);
                stats.armor.RemoveModifier(armorBuff);
                isBuffActive = false;
            }
        }
    }

    private void OnDestroy()
    {
        if (isBuffActive)
        {
            stats.physicalDamage.RemoveModifier(dmgBuff);
            stats.magicDamage.RemoveModifier(dmgBuff);
            stats.armor.RemoveModifier(armorBuff);
        }
    }
}
