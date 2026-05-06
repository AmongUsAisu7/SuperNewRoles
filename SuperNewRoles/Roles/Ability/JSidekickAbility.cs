using System;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules.Events.Bases;
using UnityEngine;
using Hazel;

namespace SuperNewRoles.Roles.Ability;

public class JSidekickAbility : AbilityBase
{
    public bool CanUseVent { get; }

    public CustomVentAbility VentAbility { get; private set; }
    public KnowOtherAbility KnowJackalAbility { get; private set; }
    public ImpostorVisionAbility ImpostorVisionAbility { get; private set; }
    private PlayerArrowsAbility _playerArrowsAbility;


    public JSidekickAbility(bool canUseVent)
    {
        CanUseVent = canUseVent;
    }

    public override void AttachToAlls()
    {
        VentAbility = new CustomVentAbility(
            () => CanUseVent
        );
        KnowJackalAbility = new KnowOtherAbility(
            (player) => player.IsJackalTeam(),
            () => true
        );
        ImpostorVisionAbility = new ImpostorVisionAbility(
            () => Jackal.JackalImpostorVision
        );

        AbilityParentAbility parentAbility = new(this);
        Player.AttachAbility(VentAbility, parentAbility);
        Player.AttachAbility(KnowJackalAbility, parentAbility);
        Player.AttachAbility(ImpostorVisionAbility, parentAbility);
        Player.AttachAbility(_playerArrowsAbility, new AbilityParentAbility(this));
    }
}
