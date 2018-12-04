using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winston_Build3
{
    public class AdminService
    {
        
        public string Prefix { get; private set; }
        public ulong AnnouncementId { get; private set; }
        public string DefaultRole { get; private set; }

        /**
         * <summary>
         * Sets prefix to be used by CommandHandler
         * </summary>
         */
        public void SetPrefix(string prefix)
        {
            Prefix = prefix;
        }

        /**
         * <summary>
         * Sets Id of text channel for announcements
         * </summary>
         */
        public void SetAnnouncementId(ulong id)
        {
            AnnouncementId = id;
        }

        /**
         * <summary>
         * Sets default role for new users
         * </summary>
         */
        public void SetDefaultRole(string role)
        {
            DefaultRole = role;
        }


    }
}
