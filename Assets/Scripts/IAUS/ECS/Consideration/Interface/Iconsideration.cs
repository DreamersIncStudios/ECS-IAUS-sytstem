using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IAUS.ECS.Consideration
{
    public interface IConsideration : Prototype<IConsideration>
    {
        float Score { get; set; }
        //      bool Inverse { get; set; } Replace using Response curve Slope
        ResponseType responseType { get; set; }
        void Consider();
        float Output(float input);
        float M { get; set; }
        float K { get; set; }
        float B { get; set; }
        float C { get; set; }

    }
    public interface ICompositeConsideration : IConsideration
    {


    }

    public enum ResponseType
    {
        LinearQuad,
        Log,
        Logistic

    }
}