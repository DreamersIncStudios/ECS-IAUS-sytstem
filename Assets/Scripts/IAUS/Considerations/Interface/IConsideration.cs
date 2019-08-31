namespace IAUS {
    public interface IConsideration : Prototype<IConsideration> {
        string NameId { get; }
        float Score { get; set; }
        bool Inverse { get; set; }

        void Consider();

    }

    public interface ICompositeConsideration : IConsideration {


    }
}