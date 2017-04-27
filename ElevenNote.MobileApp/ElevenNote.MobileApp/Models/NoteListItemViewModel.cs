using System;
using System.Collections.Generic;
using System.Text;

namespace ElevenNote.MobileApp.Models
{
    public class NoteListItemViewModel
    {
        /// <summary>
        /// The note's ID on the server.
        /// </summary>
        public int NoteId { get; set; }

        /// <summary>
        /// The note's title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The icon to use when displaying the note.
        /// </summary>
        public string StarImage { get; set; }
    }
}
