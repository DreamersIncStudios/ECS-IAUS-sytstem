using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utilitytest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    internal static Type GetAuthoringComponentTypeFromIComponentData(Type IComponentDataType)
    {
        string[] AssemblyQualifiedNameAttributes = IComponentDataType.AssemblyQualifiedName.Split(',');
        string AuthoringComponentAssemblyQualifiedName = $"{IComponentDataType.Name}Authoring";
        for (int i = 1; i < AssemblyQualifiedNameAttributes.Length; i++)
        {
            AuthoringComponentAssemblyQualifiedName = $"{AuthoringComponentAssemblyQualifiedName}, {AssemblyQualifiedNameAttributes[i]}";
        }

        Type AuthoringComponentType = Type.GetType(AuthoringComponentAssemblyQualifiedName);

       // AddComponentIfMissing(AuthoringComponentType);


        return AuthoringComponentType;
    }
}