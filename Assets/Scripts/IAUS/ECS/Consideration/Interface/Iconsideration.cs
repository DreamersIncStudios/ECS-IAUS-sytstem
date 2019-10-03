using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IAUS.ECS.Consideration
{
    public interface IConsideration : Prototype<IConsideration>
    {
        float Score { get; set; }
        //      bool Inverse { get; set; } Replace using Response curve Slope
        ResponseTypeECS responseType { get; set; }
        float Output(float input);
        float M { get; set; }
        float K { get; set; }
        float B { get; set; }
        float C { get; set; }

    }
    public interface ICompositeConsideration : IConsideration
    {


    }

    public enum ResponseTypeECS
    {
        LinearQuad,
        Log,
        Logistic

    }
}