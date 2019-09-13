namespace IAUS {
    public interface IConsideration : Prototype<IConsideration> {
        string NameId { get; }
        float Score { get; set; }
  //      bool Inverse { get; set; } Replace using Response curve Slope
        CharacterContext Agent { get; set; }
        ResponseType responseType{ get; set; }
        void Consider();
        float Output(float input);
        float M { get; set; }
        float K { get; set; }
        float B { get; set; }

    }

    public interface ICompositeConsideration : IConsideration {


    }

    public enum ResponseType {
        LinearQuad,
        Log,
        Logistic
        
    }
}