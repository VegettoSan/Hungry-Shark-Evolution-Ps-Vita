// Typed enum equality avoids Object equality/boxing in the legacy Mono comparer.
#if UNITY_PSP2 || UNITY_EDITOR
using System.Collections.Generic;
public sealed class HSEVitaAreaComparer : IEqualityComparer<AreaTransitionManager.Area>
{
    public bool Equals(AreaTransitionManager.Area x, AreaTransitionManager.Area y) { return x == y; }
    public int GetHashCode(AreaTransitionManager.Area value) { return (int)value; }
}
#endif
