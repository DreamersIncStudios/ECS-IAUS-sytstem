using Unity.Mathematics;

namespace IAUS.Core.GOAP
{
    public interface IActionStrategy
    {
        bool CanPerform { get; }
        bool Complete { get; }

        void Start()
        {
        }

        void Update(float deltaTime)
        {
        }

        void Stop()
        {
            
        }
    }

    public class IdleStrategy : IActionStrategy
    {
        public bool CanPerform => true;
        public bool Complete { get; private set; }


        public IdleStrategy(float duration)
        {
        }
    }

    public class GotoLocation : IActionStrategy
    {
        public bool CanPerform => true;
        public bool Complete { get; private set; }

        public void Start()
        {
        }

        public void Update(float deltaTime)
        {
        }

        public void Stop()
        {
        }
        
        public GotoLocation()
        {
        }
    }
}