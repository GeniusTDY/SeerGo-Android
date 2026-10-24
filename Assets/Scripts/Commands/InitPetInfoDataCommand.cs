using QFramework;
using SeerGoGame.Events;
using SeerGoGame.Interfaces;

namespace SeerGoGame.Commands
{
    public class InitPetInfoDataCommand : AbstractCommand
    {
        protected override async void OnExecute()
        {
            await this.GetModel<IPetInfoModel>().LoadMonsterConfigAsync();
            LoadProgressController.Instance.CompleteTask("spriteInfo");

            await this.GetModel<IPersonalityModel>().LoadNatureInfoConfigAsync();
            LoadProgressController.Instance.CompleteTask("spriteInfo");

            await this.GetModel<IAttributeModel>().LoadAttributeConfigAsync();
            LoadProgressController.Instance.CompleteTask("spriteInfo");

            await this.GetModel<ISkillModel>().LoadSkillData();
            LoadProgressController.Instance.CompleteTask("spriteInfo");

            await this.GetModel<ISkillModel>().LoadSkillEffectInfoConfigAsync();
            LoadProgressController.Instance.CompleteTask("spriteInfo");

            await this.GetModel<ITypeRelationModel>().LoaTypeRelationConfigAsync();
            LoadProgressController.Instance.CompleteTask("spriteInfo");

            await this.GetModel<IAnimationFrameModel>().LoadAnimationConfigAsync();
            LoadProgressController.Instance.CompleteTask("spriteInfo");

            await this.GetModel<IExpConfigModel>().LoadExpConfigAsync();
            LoadProgressController.Instance.CompleteTask("spriteInfo");

            await this.GetModel<IBattleEffectModel>().LoadBattleEffectInfoConfigAsync();
            LoadProgressController.Instance.CompleteTask("spriteInfo");

            this.SendEvent<LoadGameDataSuccessEvent>();

        }

    }
}
