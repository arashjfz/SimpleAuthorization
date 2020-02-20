namespace SimpleAuthorization.Engine
{
    public class EngineFactory
    {
        public ISecurityEngine CreateEngine()
        {
            return new SecurityEngine();
        }
    }
}
