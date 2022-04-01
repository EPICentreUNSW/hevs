

namespace HEVS
{
    internal class CollaborativeSession
    {
        public static int userID = 0;
        public static bool isHost = false;

        static CollaborativeSession singleton = null;

        public static void HostSession(int port)
        {
            if (singleton != null)
            {
                // error, can only have a single session
            }
        }

        public static void JoinSession(string address, int port)
        {
            if (singleton != null)
            {
                // error, can only have a single session
            }
        }

        public static void LeaveSession()
        {
            if (singleton == null)
            {
                // error, no session to leave
            }
        }

        void OnNewConnection()
        {
            // user joined

            // if host...
                // send new user all collaborative objects
                // inform all others that a user has joined
        }

        void OnLostConnection()
        {
            // if host...
                // inform all users that a user has left
                // remove any locks that user has
        }

        void OnJoinedConnection()
        {
            // trigger when "I" join a host
            // receive all collaborative objects from host
        }
    }
}