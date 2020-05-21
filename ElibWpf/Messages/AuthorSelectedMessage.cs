using MVVMLibrary.Messaging;

namespace ElibWpf.Messages
{
    public class AuthorSelectedMessage : MessageBase
    {
        public AuthorSelectedMessage(int authorId)
        {
            this.AuthorId = authorId;
        }

        public int AuthorId { get; }
    }
}