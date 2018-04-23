using System.Collections.Specialized;
using UnityEngine;
using static CrowdControllable;


public class UIStatusText : MonoBehaviour
{

    //public enum Status { _default, taunted, stunned, silenced, blind, crippled, bleeding }

    public TextMesh textMesh;

    private ObservableStatusCollection observableCollection;

    private void Start()
    {
        CrowdControllable crowdControllable = transform.parent.parent.GetComponent<CrowdControllable>();
        observableCollection = crowdControllable.myStatuses;

        if (observableCollection != null)
        {
            observableCollection.CollectionChanged += UpdateUIText;
        }
        else
        {
            Debug.LogWarning("No ObservableCollection found on UIStatusText!");
        }
    }

    private void UpdateUIText(object sender, NotifyCollectionChangedEventArgs e)
    {
        //Debug.Log("UpdateUIText()");

        SetTextToCollectionContent();        
    }

    private void SetTextToCollectionContent()
    {
        textMesh.text = "";
        foreach (Status s in observableCollection)
        {
            string tmp = s.ToString() + " ";
            textMesh.text += tmp;
        }
    }

    /*
    private void ItemAdded(NotifyCollectionChangedEventArgs e)
    {
        foreach (Status status in e.NewItems)
        {
            textMesh.text = text;
        }
    }

    private void ItemRemoved(NotifyCollectionChangedEventArgs e)
    {
        foreach (Status status in e.OldItems)
        {
            //textMesh.text -= "Test";
        }
    }
    */
}
