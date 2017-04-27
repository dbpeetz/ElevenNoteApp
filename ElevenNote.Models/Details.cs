using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElevenNote.Models
{
    public class Details
    {
        public int NoteID { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public bool IsStarred { get; set; }

        public DateTimeOffset CreatedUtc { get; set; }

        public DateTimeOffset? ModifiedUtc { get; set; }

        public override string ToString() => $"[{NoteID}] {Title}";


    }
}
