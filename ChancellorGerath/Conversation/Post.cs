using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChancellorGerath.Conversation
{
	public record Post(string Server, string Channel, string User, string Content, DateTime? Timestamp, bool IsQuoted = false)
	{
	}
}
