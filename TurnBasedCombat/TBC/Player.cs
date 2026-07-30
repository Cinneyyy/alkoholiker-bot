namespace TBC;

public sealed class Player
{
    public u64 Id { get; init; }
    public f32 Attack { get; set; }
    public f32 Defence { get; set; }
    public f32 MaxHp { get; set; }
    public u32 BattlesWon { get; set; }
    public u32 BattlesLost { get; set; }
}
