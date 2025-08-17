namespace LRParser.Parser
{
    public readonly struct ParserAction
    {
        public enum Type
        {
            Shift,
            Reduce,
            Accept,
            Error
        }

        public readonly Type Action;
        public readonly int StateOrProdId; // or production ID

        public static ParserAction Default => new(Type.Error, -1);

        public ParserAction(Type action, int stateOrProdId)
        {
            StateOrProdId = stateOrProdId;
            Action = action;
        }
    }
}
