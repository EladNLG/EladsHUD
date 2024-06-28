using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimCurve : MonoBehaviour
{
    public float drunkness;
    public float staminaLossMultiplier;
    public AnimationCurve curve;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnValidate()
    {
        if (curve != null && curve.length == 3)
        {
            Keyframe keyframe = curve.keys[0];
            keyframe.inTangent = 6.2633f;
            keyframe.outTangent = 6.2633f;
            keyframe.weightedMode = WeightedMode.None;
            curve.MoveKey(0, keyframe);
            keyframe = curve.keys[1];
            keyframe.inTangent = 0.2911f;
            keyframe.outTangent = 0.2911f;
            keyframe.weightedMode = WeightedMode.None;
            curve.MoveKey(1, keyframe);
            keyframe = curve.keys[2];
            keyframe.inTangent = 0.1252f;
            keyframe.outTangent = 0.1252f;
            keyframe.weightedMode = WeightedMode.None;
            curve.MoveKey(2, keyframe);
            staminaLossMultiplier    = (Mathf.Abs(curve.Evaluate(drunkness) - 1.25f));
        }
    }
}
