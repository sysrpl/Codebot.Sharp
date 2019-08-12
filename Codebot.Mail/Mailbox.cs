namespace Codebot.Mail
{
    public class Mailbox
    {
        string name = "";
        string address = "";

        public void Copy(Mailbox source)
        {
            if (source == null)
            {
                name = "";
                address = "";
            }
            else
            {
                name = source.name;
                address = source.address;
            }
        }

        public string Name
        {
            get => name;
            set => name = string.IsNullOrWhiteSpace(value) ? "" : value.Trim();
        }

        public string Address
        {
            get => address;
            set => address = string.IsNullOrWhiteSpace(value) ? "" : value.Trim();
        }

    }
}
