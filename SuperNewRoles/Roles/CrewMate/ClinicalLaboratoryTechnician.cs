using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Modifiers;
using SuperNewRoles.Roles.Neutral;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;


    class ClinicalLaboratoryTechnician : RoleBase<ClinicalLaboratoryTechnician>
    {
        public override RoleId Role { get; } = RoleId.ClinicalLaboratoryTechnician;
        public override Color32 RoleColor { get; } = Lovers.Instance.RoleColor;
        public override List<Func<AbilityBase>> Abilities { get; } = [
            () => new ClinicalLaboratoryTechnicianAbility(
            ClinicalLaboratoryTechnicianCoolTime
        )
        ];
        public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override AssignedTeamType AssignedTeam => AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Crewmate;
    public override TeamTag TeamTag => TeamTag.Crewmate;
    public override RoleTag[] RoleTags => [];
    public override short IntroNum => 1;
    public override RoleTypes IntroSoundType => RoleTypes.Crewmate;
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Crewmate;

    [CustomOptionFloat("ClinicalLaboratoryTechnicianCoolTime", 0f, 180f, 2.5f, 0f, translationName: "CoolTime")]
    public static float ClinicalLaboratoryTechnicianCoolTime;
}

public class ClinicalLaboratoryTechnicianAbility : AbilityBase
{
    public float CoolTime { get; }
    private EventListener<NameTextUpdateEventData> _nameTextUpdateEvent;
    private EventListener<NameTextUpdateVisiableEventData> _nameTextUpdateVisiableEvent;
    public byte Sample1 { get; private set; }
    public byte Sample2 { get; private set; }

    private GetSampleAbility getSampleAbility;
    public ClinicalLaboratoryTechnicianAbility(float coolTime)
    {
        CoolTime = coolTime;
    }

    public void SetSample(byte sample1, byte sample2)
    {
        Sample1 = sample1;
        Sample2 = sample2;
    }
    public override void AttachToAlls()
    {
        base.AttachToAlls();

        getSampleAbility = new GetSampleAbility(
            CoolTime,
            ModTranslation.GetString("GetSampleAbilityArrow"),
            AssetManager.GetAsset<Sprite>("ClinicalLaboratoryTechnicianButton.png"),
            false,
            (players) =>
            {
                Sample1 = players[0].PlayerId;
                Sample2 = players[1].PlayerId;
            }
        );
        Player.AttachAbility(getSampleAbility, new AbilityParentAbility(this));
        _nameTextUpdateVisiableEvent = NameTextUpdateVisiableEvent.Instance.AddListener(OnNameTextUpdateVisiable);
    }
    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _nameTextUpdateVisiableEvent?.RemoveListener();
    }
    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _nameTextUpdateEvent = NameTextUpdateEvent.Instance.AddListener(OnNameTextUpdate);
    }
    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _nameTextUpdateEvent?.RemoveListener();
    }

    private void OnNameTextUpdate(NameTextUpdateEventData data)
    {
        //if (!Player.AmOwner) return;
        if (Player.IsDead()) return;
        if (getSampleAbility.TakeSample != null)
        {
            if (!getSampleAbility.TakeSample.lovers.Any(x => x.Player.PlayerId == data.Player.PlayerId)) return;
            //if (!data.Player.IsLovers()) return;
            if (data.Player.cosmetics.nameText.text.Contains("♥")) return;
            NameText.AddNameText(data.Player, ModHelpers.Cs(Lovers.Instance.RoleColor, "♥"));
        }
        // まだ作ってないけど1人に刺してたら中抜きハートを付ける
        else if (getSampleAbility.CurrentTarget != null)
        {
            if (getSampleAbility.CurrentTarget.PlayerId != data.Player.PlayerId) return;
            if (data.Player.cosmetics.nameText.text.Contains("♡")) return;
            data.Player.cosmetics.nameText.text += ModHelpers.Cs(Lovers.Instance.RoleColor, "♡");
        }
    }

    private void OnNameTextUpdateVisiable(NameTextUpdateVisiableEventData data)
    {
    }
}

    /*public MediumSpiritVisionAbility(MediumSpiritVisionData data)
    {
        Data = data;
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _meetingStartListener = MeetingStartEvent.Instance.AddListener(OnMeetingStart);
        _visibleGhostAbility = new VisibleGhostAbility(() => isEffectActive);
        Player.AttachAbility(_visibleGhostAbility, new AbilityParentAbility(this));
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _meetingStartListener?.RemoveListener();
    }

    public override bool CheckIsAvailable()
    {
        return !isEffectActive && ExPlayerControl.LocalPlayer.Player.CanMove;
    }

    public override void OnClick()
    {
        SetSpiritVision(true);
    }

    private void OnMeetingStart(MeetingStartEventData data)
    {
        isEffectActive = false;
    }

    public void SetSpiritVision(bool isActive)
    {
        isEffectActive = isActive;
        //if (Player.AmOwner)
        //{
            if (isActive)
                message = new CustomMessage(ModTranslation.GetString("MediumSpiritVisionActiveMessage"), EffectDuration);
            else if (message?.text?.gameObject != null)
                GameObject.Destroy(message.text.gameObject);
        //}
    }
    public void FinishSpiritForce()
        => EffectTimer = 0.0001f;
}

public class MediumSpiritTalkAbility : TargetCustomButtonBase, IAbilityCount
{
    public MediumSpiritTalkData Data { get; }
    private readonly List<string> _pendingMeetingMessages = new();
    private EventListener<MeetingStartEventData> _meetingStartListener;

    public override Color32 OutlineColor => Medium.Instance.RoleColor;
    public override bool OnlyCrewmates => false;
    //public override Func<bool> IsDeadPlayerOnly => () => true;
    public override Func<ExPlayerControl, bool> IsTargetable => (player) => true;
    public override bool CheckIsAvailable() => Target != null && ExPlayerControl.LocalPlayer.Player.CanMove && IsSpiritVisionActive();
    protected override KeyType keytype => KeyType.Ability2;


    public override bool IsFirstCooldownTenSeconds => false;
    public override string buttonText => ModTranslation.GetString("MediumSpiritTalkButtonText");
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("MediumSpiritTalkButton.png") ?? HudManager.Instance.UseButton.graphic.sprite;

    public override ShowTextType showTextType => ShowTextType.ShowWithCount;

    public MediumSpiritTalkAbility(MediumSpiritTalkData data)
    {
        Data = data;
        Count = Data.MaxUses;
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _meetingStartListener = MeetingStartEvent.Instance.AddListener(OnMeetingStart);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _meetingStartListener?.RemoveListener();
        _pendingMeetingMessages.Clear();
    }

    public override bool CheckHasButton()
        => base.CheckHasButton() && HasCount();

    public override void OnClick()
    {
        this.UseAbilityCount();

        // 追放で死亡した場合は特別メッセージ
        if (Target.Data.Disconnected || Target.FinalStatus == FinalStatus.Exiled || !MurderDataManager.TryGetMurderData(Target, out var murderData))
        {
            message = ModTranslation.GetString("MediumSpiritTalkExiledMessage", Target.Data.PlayerName);
        }
        else
        {

            switch (infoType)
            {
            // 役職
                    string targetRoleName = Target.roleBase != null ? ModTranslation.GetString(Target.roleBase.RoleName) : ModTranslation.GetString(Target.Role.ToString());
                    message = ModTranslation.GetString("MediumSpiritTalkRoleMessage", Target.Data.PlayerName, targetRoleName);
                    break;
            }
        }

        // チャットに送信
        if (!string.IsNullOrEmpty(message))
        {
            _pendingMeetingMessages.Add(message);
        }


    }

    private void OnMeetingStart(MeetingStartEventData data)
    {
        if (_pendingMeetingMessages.Count <= 0) return;
        new LateTask(SendPendingMessages, 0.5f, "MediumSpiritTalkMeetingMessage");
    }

    private void SendPendingMessages()
    {
        if (!Player.AmOwner) return;
        if (HudManager.Instance?.Chat == null) return;

        foreach (string pendingMessage in _pendingMeetingMessages)
        {
            HudManager.Instance.Chat.AddChat(Player.Player, pendingMessage);
        }

        _pendingMeetingMessages.Clear();
    }
}
    */