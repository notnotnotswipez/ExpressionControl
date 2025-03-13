using ExpressionControl.Messages;
using LabFusion.SDK.Modules;

namespace ExpressionControl
{
    public class ExpressionControlModule : Module
    {
        public override string Name => "ExpressionControl";
        public override string Author => "notnotnotswipez";
        public override Version Version => new Version(1, 0, 2);
        public override ConsoleColor Color => ConsoleColor.Yellow;

        protected override void OnModuleRegistered()
        {
            // Register messages for this module
            ModuleMessageHandler.RegisterHandler<ExpressionMessage>();
        }

        protected override void OnModuleUnregistered()
        {
            // Optional: Add cleanup logic here if needed
        }
    }
}
