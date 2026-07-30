using System;

namespace TBC;

public sealed class Battle
{
    public u64 Id { get; init; }
    public BattleParticipant Attacker { get; }
    public BattleParticipant Defender { get; }
    public bool AttackerTurn { get; private set; }
    public BattleParticipant CurrentPlayer => AttackerTurn ? Attacker : Defender;
    public BattleParticipant NextPlayer => !AttackerTurn ? Attacker : Defender;
    public Action<Player, Player> OnGameEnd { get; set; }


    public Battle(Player attacker, Player defender)
    {
        Id = unchecked((u64)Random.Shared.NextInt64());
        Attacker = new(attacker, this, true);
        Defender = new(defender, this, false);
    }


    public f32 GetRandomFactor()
        => (2f * Random.Shared.NextSingle() - 1f) * 0.33f + 1f;

    public void TurnEnd()
        => AttackerTurn ^= true;

    public void TurnAttack(out f32 damage)
    {
        damage = CurrentPlayer.Player.Attack * GetRandomFactor() / NextPlayer.Player.Defence;
        NextPlayer.Hp -= damage;

        if(NextPlayer.Hp <= 0f)
            OnGameEnd(CurrentPlayer.Player, NextPlayer.Player);

        TurnEnd();
    }

    public void Surrender()
        => OnGameEnd(NextPlayer.Player, CurrentPlayer.Player);

    public void Defend()
    {
        CurrentPlayer.TemporaryBuffs.Add((p => p.Defence *= 2f, p => p.Defence /= 2f, 1));
        TurnEnd();
    }

    // Item(), Spell(), Defend()
}
