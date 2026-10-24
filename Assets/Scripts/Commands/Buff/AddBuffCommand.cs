/*using QFramework;
using SeerGoGame.Systems;

namespace SeerGoGame.Commands
{
    public class AddBuffCommand : AbstractCommand
    {
        private readonly IBuffable mTarget;
        private readonly int mBuffId;

        public AddBuffCommand(IBuffable target, int buffId)
        {
            mTarget = target;
            mBuffId = buffId;
        }

        protected override void OnExecute()
        {
            this.GetSystem<BuffSystem>().ApplyBuff(mTarget, mBuffId);
        }
    }
}
namespace QFramework.Example
{
    public class RemoveBuffCommand : AbstractCommand
    {
        private readonly IBuffable mTarget;
        private readonly int mBuffId;

        public RemoveBuffCommand(IBuffable target, int buffId)
        {
            mTarget = target;
            mBuffId = buffId;
        }

        protected override void OnExecute()
        {
            mTarget.RemoveBuff(mBuffId);
        }
    }
}
*/