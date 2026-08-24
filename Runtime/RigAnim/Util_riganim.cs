#if HAS_ANIMRIG
using UnityEngine.Animations.Rigging;

partial class Util
{
    public static void AddWeightedTransform(this MultiParentConstraint rigConstraint, in WeightedTransform weightedTransform)
    {
        var sourcesR = rigConstraint.data.sourceObjects;
        sourcesR.Add(weightedTransform);
        rigConstraint.data.sourceObjects = sourcesR;
    }
}
#endif