using System;
using System.Collections.Generic;

namespace TBC;

public sealed class BattleParticipant(Player player, Battle battle, bool isAttacker)
{
    public Player Player { get; init; } = player;
    public f32 MaxHp { get; set; } = player.MaxHp;
    public f32 Defence { get; set; } = player.Defence;
    public f32 Attack { get; set; } = player.Attack;
    public Battle Battle { get; init; } = battle;
    public bool IsAttacker { get; init; } = isAttacker;
    public f32 Hp { get; set; } = player.MaxHp;
    public List<(Action<BattleParticipant> preTurn, Action<BattleParticipant> postTurn, i32 turnsLeft)> TemporaryBuffs { get; } = [];


    public void PreTurn()
    {
        for(i32 i = 0; i < TemporaryBuffs.Count; i++)
            TemporaryBuffs[i].preTurn(this);
    }

    public void PostTurn()
    {
        for(i32 i = TemporaryBuffs.Count-1; i >= 0; i++)
        {
            (_, Action<BattleParticipant> post, i32 turnsLeft) = TemporaryBuffs[i];

            post(this);
            turnsLeft--;

            if(turnsLeft <= 0)
                TemporaryBuffs.RemoveAt(i);
            else
                TemporaryBuffs[i] = TemporaryBuffs[i] with { turnsLeft = turnsLeft };
        }
    }
}
