namespace Blox.Actions
{
    public abstract class Action
    {
        
    }
    
    public abstract class Action<I,O> : Action
    {
        private readonly I m_Input;
        
        protected Action(I input)
        {
            m_Input = input;
        }

        public O Invoke()
        {
            return Execute(m_Input);
        }
        
        protected abstract O Execute(I input);
    }
}