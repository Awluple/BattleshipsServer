namespace BattleshipsServer
{
    static class Users {
        private static int userId = 1000;

        public static int getUserId() {
            userId++;
            return userId;
        }
    }
}