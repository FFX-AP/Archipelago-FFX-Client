using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using System.Numerics;
using Color = Archipelago.MultiClient.Net.Models.Color;

namespace ArchipelagoFFX.GUI; 

public static class Colors {
    extension(Color color) {
        public Vector4 to_vector4() => new(color.R / 255f, color.G / 255f, color.B / 255f, 1.0f);
    }

    // Pascal case names to mimic enums
    private static readonly Vector4 BrightenFactor = new(0.2f, 0.2f, 0.2f, 0.0f);

    public static readonly Vector4 Default = Color.White.to_vector4();

    public static readonly Vector4 PlayerSelf  = Color.Magenta.to_vector4() + BrightenFactor;
    public static readonly Vector4 PlayerOther = Color.Yellow.to_vector4();

    public static readonly Vector4 ItemFiller = Color.Cyan.to_vector4();
    public static readonly Vector4 ItemTrap   = Color.Salmon.to_vector4();
    public static readonly Vector4 ItemProg   = Color.Plum.to_vector4();
    public static readonly Vector4 ItemUseful = Color.SlateBlue.to_vector4() + BrightenFactor;

    // Location color is slightly modified to be more readable on dark backgrounds
    public static readonly Vector4 Location = Color.Green.to_vector4() + BrightenFactor;

    public static Vector4 get_item_color(ItemInfo item) {
        Vector4 item_color = ItemFiller;

        if (item.Flags.HasFlag(ItemFlags.Trap)) {
            item_color = ItemTrap;
        } else if (item.Flags.HasFlag(ItemFlags.Advancement)) {
            item_color = ItemProg;
        } else if (item.Flags.HasFlag(ItemFlags.NeverExclude)) {
            item_color = ItemUseful;
        }

        return item_color;
    }
}
