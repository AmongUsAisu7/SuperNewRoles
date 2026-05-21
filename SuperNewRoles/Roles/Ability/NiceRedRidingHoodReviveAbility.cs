using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Crewmate;
using AmongUs.GameOptions;

namespace SuperNewRoles.Roles.Ability;

public class NiceRedRidingHoodReviveAbility : AbilityBase, IAbilityCount
{
    private EventListener<MurderEventData> _murderEventListener;
    private EventListener<ExileEventData> _exileEventListener; // 追放イベントに変更

    public ExPlayerControl Killer { get; set; }
    public bool IsRevivable { get; set; }
    public int RemainingReviveCount { get; set; }
    public int NiceRedRidingHoodCount { get; set; }
    public bool NiceRedRidingHoodIsKillerDeathRevive { get; set; }

    public NiceRedRidingHoodReviveAbility(int niceRedRidingHoodCount, bool niceRedRidingHoodIsKillerDeathRevive)
    {
        NiceRedRidingHoodCount = niceRedRidingHoodCount;
        NiceRedRidingHoodIsKillerDeathRevive = niceRedRidingHoodIsKillerDeathRevive;
    }

    public int MaxCount => NiceRedRidingHoodCount;

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        RemainingReviveCount = NiceRedRidingHoodCount;

        _murderEventListener = MurderEvent.Instance.AddListener(OnMurderEvent);
        
        // 誰かが追放された瞬間に発火するイベントを監視
        _exileEventListener = ExileEvent.Instance.AddListener(OnExileEvent);
    }

    public override void DetachToAlls()
    {
        _murderEventListener?.RemoveListener();
        _exileEventListener?.RemoveListener();
        base.DetachToAlls();
    }

    private void OnMurderEvent(MurderEventData data)
    {
        // 自分が殺されたら、そのターンのキラーを記録して復活待機状態にする
        if (data.target == Player && data.killer != Player && RemainingReviveCount > 0)
        {
            Killer = data.killer;
            IsRevivable = true;
            Logger.Info($"[赤ずきん] 死亡。キラー({Killer.Player.Data.PlayerName})をロックしました。", "NiceRedRidingHood");
        }
    }

    private void OnExileEvent(ExileEventData data)
    {
        // 自分が復活可能状態で、キラーが記録されている場合のみ進む
        if (!IsRevivable || Killer == null) return;

        // 今まさに追放されたプレイヤー（data.player）が、自分を殺したキラーだった場合
        if (data.player == Killer)
        {
            Logger.Info($"[赤ずきん] キラーが追放されました。復活処理を実行します。", "NiceRedRidingHood");
            Revive();
        }
        else
        {
            // キラー以外の人が追放された＝会議を跨ぐことになるため、
            // 「仕様通り」ここで復活権利（フラグ）を消滅させ、次のターンでは復活できないようにする
            IsRevivable = false;
            Killer = null;
            Logger.Info($"[赤ずきん] キラー以外が追放されたため、このターンの復活権利は消滅しました。", "NiceRedRidingHood");
        }
    }

    [CustomRPC]
    public void Revive()
    {
        if (!IsRevivable || RemainingReviveCount <= 0) return;

        Player.Player.Revive();
        RoleManager.Instance.SetRole(Player, RoleTypes.Crewmate);
        FinalStatusManager.SetFinalStatus(Player, FinalStatus.Alive);

        RemainingReviveCount--;
        IsRevivable = false;
        Killer = null;
        Player.Data.IsDead = false;

        Logger.Info($"[赤ずきん] 復活成功。残り回数: {RemainingReviveCount}", "NiceRedRidingHood");
    }
}
