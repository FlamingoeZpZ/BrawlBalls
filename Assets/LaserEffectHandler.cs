using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class LaserEffectHandler : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private VisualEffect myEffect;
    [SerializeField] private AnimationCurve initBeam;
    private const float TimeToActivate = 0.6f;

    private float width;
    [SerializeField] private Weapon w;
    private float lifeTime;
    private bool firstTimeAwake = true;
    

    private void OnEnable()
    {
        if(firstTimeAwake)
        {
            
            //Network behaviour children need to start enabled :(
            transform.GetChild(0).gameObject.SetActive(false);
            gameObject.SetActive(false);
            
            firstTimeAwake = false;
            return;
        }
        StartCoroutine(HandleSizeChange());
        myEffect.SendEvent(StaticUtilities.ActivateID);
        lifeTime = w.GetAbility.Cooldown * 0.6f;
        
    }

    private void Start()
    {
        width = w.Range.y;
        myEffect.SetFloat(StaticUtilities.DelayID, TimeToActivate);
        myEffect.SetVector4(StaticUtilities.ColorID, lineRenderer.material.GetColor(StaticUtilities.ColorID));
    }

    private void LateUpdate()
    {
        //This sucks but cope
        float maxDist = 0;
        for(int i = 0; i < w.HitCount; ++i)
        {
            float d = w.Hits[i].distance;
            if (maxDist < d)
            {
                maxDist = d;
            }
        }

        if (maxDist == 0) maxDist = w.Range.x;
        maxDist *= 3;
        myEffect.SetFloat(StaticUtilities.PositionID, maxDist);
        lineRenderer.SetPosition(1,maxDist*Vector3.forward);
        
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0)
        {
            BallPlayer.LocalBallPlayer.EnableControls();
            w.ToggleActive();
            gameObject.SetActive(false);
            myEffect.SendEvent(StaticUtilities.EndID);
        }
    }



    private IEnumerator HandleSizeChange()
    {
        float curTime = 0;

        while (curTime < TimeToActivate)
        {
            curTime += Time.deltaTime;
            lineRenderer.startWidth = Mathf.Lerp(0.01f, width, initBeam.Evaluate(curTime / TimeToActivate));
            yield return null;
        }
        w.ToggleActive();
        lineRenderer.startWidth = width;
        BallPlayer.LocalBallPlayer.DisableControls();
    }

    public void SetProperty(int id, float amount)
    {
        lineRenderer.material.SetFloat(id, amount);
    }
}
