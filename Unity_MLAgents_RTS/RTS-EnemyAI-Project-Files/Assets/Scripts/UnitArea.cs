using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using TMPro;

public class UnitArea : MonoBehaviour
{
    [Tooltip("if true, enable training mode")]
    public bool trainingMode;

    public List<Unit> UnitAgents { get; private set; }
    
    public List<GameObject> Resources { get; private set; }


}
