using Twinkle.View.Enums;

namespace Twinkle.View
{
    public class Hook
    {
        public delegate void OperationHookDelegate();
        public delegate void HandlerHookDelegate();

        public TypeHook Type { get; set; }
        public string AliasControl { get; set; }
        public string AliasOperation { get; set; }
        public OperationHookDelegate Operation { get; set; }
        public HandlerHookDelegate Handler { get; set; }

        public Hook(TypeHook type, string aliasControl, string aliasOperation , HandlerHookDelegate handler)
        {
            this.Type = type;
            this.AliasControl = aliasControl;
            this.AliasOperation = aliasOperation;
            this.Handler = handler;
        }
    }
}