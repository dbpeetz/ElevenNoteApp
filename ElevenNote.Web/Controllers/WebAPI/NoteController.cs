using ElevenNote.Models;
using ElevenNote.Service;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ElevenNote.Web.Controllers.WebAPI
{

    [Authorize]
    [RoutePrefix("api/Note")]
    public class NoteController : ApiController
    {

        public bool SetStarState(int noteId, bool newState)
        {
            var userId = Guid.Parse(User.Identity.GetUserId());
            var service = new NoteService(userId);

            var detail = service.GetNoteById(noteId);

            var updatedNote = new NoteEdit
            {
                NoteId = detail.NoteID,
                Title = detail.Title,
                Content = detail.Content,
                IsStarred = newState
            };

            return service.UpdateNote(updatedNote);
        }

        [Route("{id}/Star")]
        [HttpPut]
        public bool ToggleStarOn(int id) => SetStarState(id, true);

        [Route("{id}/Star")]
        [HttpDelete]
        public bool ToggleStarOff(int id) => SetStarState(id, false);

    }
}
