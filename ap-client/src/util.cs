using System.Numerics;

internal static class ApUtilExt {
    extension(Vector2 vec) {
        public Vector2 game_remap_720p() {
            return new Vector2(
                vec.X * 512f / 1280f,
                vec.Y * 416f /  720f
            );
        }

        public Vector2 game_remap_1080p() {
            return new Vector2(
                vec.X * 512f / 1920f,
                vec.Y * 416f / 1080f
            );
        }
    }
}
