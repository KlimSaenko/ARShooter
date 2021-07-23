using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HitDecal_Prev : MonoBehaviour
{
    [SerializeField] private GameObject decalPrefab;
    [SerializeField] private AnimationCurve fadeCurve;
    private Transform poolFolder;

    private static Camera MainCam { get { return Camera.main; } }

    private static LinkedList<TextMeshPro> decals = new ();
    public static HitDecal_Prev instance;

    private void Start()
    {
        poolFolder = transform;
        instance = this;
        for (var i = 0; i < 3; i++)
        {
            decals.AddLast(Instantiate(decalPrefab, poolFolder).GetComponent<TextMeshPro>());
        }
    }

    internal void NewDecal(Vector3 pos, int damage, HitZone.ZoneType zoneType)
    {
        if (decals.Count <= 0) return;
        
        var decalText = decals.First.Value;
        decals.RemoveFirst();

        decalText.transform.position = pos + new Vector3(Random.Range(-0.1f, 0.1f), 0, Random.Range(-0.1f, 0.1f));
        decalText.transform.rotation = MainCam.transform.rotation;
        decalText.text = damage.ToString();
        
        decalText.color = zoneType switch
        {
            HitZone.ZoneType.Standard => Color.white,
            HitZone.ZoneType.Critical => Color.red,
            _ => decalText.color
        };
        decalText.gameObject.SetActive(true);

        StartCoroutine(DecalLife(decalText));
    }

    private IEnumerator DecalLife(TextMeshPro decalText)
    {
        float time = 0f;
        while (time < 1f) 
        {
            time += 3.5f * Time.deltaTime;
            decalText.alpha = fadeCurve.Evaluate(time);
            yield return new WaitForEndOfFrame();
        }
        decalText.gameObject.SetActive(false);
        decals.AddLast(decalText);
    }
}
