using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentNavigation : MonoBehaviour
{
    [SerializeField] private Vector3 _desiredDestination;
    NavMeshAgent navMeshAgent;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        _desiredDestination = transform.position;
        navMeshAgent.destination = _desiredDestination;
    }

    public void SetDestination(Vector3 destination)
    {
        _desiredDestination = destination;
        navMeshAgent.destination = _desiredDestination;
    }
}
