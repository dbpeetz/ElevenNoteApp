using ElevenNote.MobileApp.Models;
using ElevenNote.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ElevenNote.MobileApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NotesPage : ContentPage
    {
        private List<NoteListItemViewModel> Notes { get; set; }

        private int? _noteId = null;
        public NotesPage(int? noteId)
        {
            InitializeComponent();
            _noteId = noteId;
            SetupUi(); 
        }

        private async void SetupUi()
        {
            // Set the appropriate title.
            this.Title = _noteId.HasValue ? "Edit Note" : "New Note";

            // If we're creating a new note, disable the Starred switch.
            if (!_noteId.HasValue) fldIsStarred.IsEnabled = false;

            // If we're editing a note, load the note.
            if (_noteId.HasValue)
            {
                // Add a delete button.
                // Add delete option.
                this.ToolbarItems.Add(new ToolbarItem("Delete", null, async () =>
                {
                    // Confirm they want to delete.
                    if (await DisplayAlert("Well?", "Are you sure you want to delete this note?", "Yep", "Nope"))
                    {
                        await App.NoteService.Delete(_noteId.Value).ContinueWith(async task =>
                        {
                            if (task.Result)
                            {
                                await DisplayAlert("Great!", "The note has been deleted.", "Okie Dokie");
                                await Navigation.PopAsync(true);
                            }
                            else
                            {
                                await DisplayAlert("Bummer", "The note could not be deleted. Are you sure it's still there?", "Okie Dokie");
                            }
                        }, TaskScheduler.FromCurrentSynchronizationContext());
                    }
                }));

                // Show wait indicator while we load the notes.
                panProgress.IsVisible = true;
                fldProgressMessage.Text = "Please wait, loading note...";
                pleaseWait.IsRunning = true;

                await App.NoteService.GetById(_noteId.Value).ContinueWith(async task =>
                {
                    var note = task.Result;

                    // If we didn't get a note back, it's possible it's been deleted on the server.
                    // Let them know and pop back to the notes list view.
                    if (note == null)
                    {
                        await DisplayAlert("Whoops", "That note couldn't be found. Maybe it's been deleted?",
                            "Ok");
                        await Navigation.PopAsync();
                        return;
                    }

                    // If we did get the note back, populate the page.
                    fldIsStarred.IsToggled = note.IsStarred;
                    fldTitle.Text = note.Title;
                    fldNoteDetails.Text = note.Content;

                    // Hide the progress message.
                    fldProgressMessage.Text = "";
                    pleaseWait.IsRunning = false;
                    panProgress.IsVisible = false;

                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        private async void BtnSave_OnClicked(object sender, EventArgs e)
        {
            fldProgressMessage.Text = "Saving, one moment...";
            pleaseWait.IsRunning = true;
            panProgress.IsVisible = true;

            if (_noteId.HasValue)
            {
                // Update the note.
                await App.NoteService.Update(new NoteEdit()
                {
                    NoteId = _noteId.Value,
                    Title = fldTitle.Text.Trim(),
                    Content = fldNoteDetails.Text.Trim(),
                    IsStarred = fldIsStarred.IsToggled
                }).ContinueWith(async task =>
                {
                    var success = task.Result;
                    if (success)
                    {
                        fldProgressMessage.Text = "";
                        pleaseWait.IsRunning = false;
                        panProgress.IsVisible = false;
                        await DisplayAlert("Great!", "The note's been updated.", "Cool!");
                        await Navigation.PopAsync(true);
                    }
                    else
                    {
                        fldProgressMessage.Text = "";
                        pleaseWait.IsRunning = false;
                        panProgress.IsVisible = false;
                        await DisplayAlert("Bummer", "The note could not be saved. Are you connected?", "Okie Dokie");
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            else
            {
                // Create the note.
                await App.NoteService.AddNew(new NoteCreate()
                {
                    Title = fldTitle.Text.Trim(),
                    Content = fldNoteDetails.Text.Trim()
                }).ContinueWith(async task =>
                {
                    var success = task.Result;
                    if (success)
                    {
                        await DisplayAlert("Great!", "The note was added.", "Cool!");
                        await Navigation.PopAsync(true);
                    }
                    else
                    {
                        fldProgressMessage.Text = "";
                        pleaseWait.IsRunning = false;
                        panProgress.IsVisible = false;

                        await DisplayAlert("Bummer", "The note could not be saved. Are you connected?", "Okie Dokie");
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());

            }
        }


        /// <summary>
        /// Updates the notes list view.
        /// </summary>
        /// <returns></returns>
        private async Task PopulateNotesList()
        {
            await App
                .NoteService
                .GetAll()
                .ContinueWith(task =>
                {
                    var notes = task.Result;

                    Notes = notes
                        .OrderByDescending(note => note.IsStarred) // descending because 1 is greater than 0, and true == 1
                        .ThenByDescending(note => note.CreatedUtc) // show newest notes first
                        .Select(s => new NoteListItemViewModel
                        {
                            NoteId = s.NoteId,
                            Title = s.Title,
                            StarImage = s.IsStarred ? "starred.png" : "notstarred.png"
                        })
                        .ToList();

                    lvwNotes.ItemsSource = Notes;

                    // Clear any item selection.
                    lvwNotes.SelectedItem = null;

                }, TaskScheduler.FromCurrentSynchronizationContext());

        }

        private void SetupUI()
        {
            lvwNotes.IsPullToRefreshEnabled = true;
            lvwNotes.Refreshing += async (o, args) =>
            {
                await PopulateNotesList();
                lvwNotes.IsRefreshing = false;
                lblNoNotes.IsVisible = !Notes.Any();
            };
        }

        #region Event Handlers 
        protected override async void OnAppearing()
        {
            await PopulateNotesList();
        }
        #endregion


    }
}
