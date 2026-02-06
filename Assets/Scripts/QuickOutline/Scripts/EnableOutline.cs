using UnityEngine;

public class EnableOutline : MonoBehaviour
{
    public Outline outlineScript;
    
    void Awake()
    {
        if(outlineScript == null)
            outlineScript = GetComponent<Outline>();

        if(outlineScript == null)
            outlineScript = GetComponentInParent<Outline>(true);
        
        if(outlineScript == null)
            outlineScript = GetComponentInChildren<Outline>(true);

        if(outlineScript != null)
        {
            outlineScript.enabled = false;
        }else
        {
            Debug.LogWarning($"[EnableOutline] No Outline found on {name} or its children.", this);
        }
    }

    public void OnMouseEnter()
    {
        if(outlineScript != null)
            outlineScript.enabled = true;
    }

    public void OnMouseExit()
    {
        if(outlineScript != null)
            outlineScript.enabled = false;
    }
}
