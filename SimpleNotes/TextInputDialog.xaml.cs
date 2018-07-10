using SimpleNotes.Helpers;
using System;
using System.ComponentModel;
using System.Windows;

namespace SimpleNotes
{
    public partial class TextInputDialog : Window
    {
        public string Text { get => noteNameTbx.Text; set => noteNameTbx.Text = value; }
        public event EventHandler<SubmitEventArgs<string>> Submitted;

        public TextInputDialog(string text = "")
        {
            InitializeComponent();
            Text = text;
            noteNameTbx.GotFocus += (s, e) => noteNameTbx.SelectAll();
        }

        public bool ShowDialog(Window owner)
        {
            base.Owner = owner;
            return base.ShowDialog().GetValueOrDefault(false);
        }

        private void okBtn_Click(object sender, RoutedEventArgs e) => DialogResult = true;

        protected virtual bool SubmitUnsuccesfull()
        {
            SubmitEventArgs<string> eventArgs = new SubmitEventArgs<string>(Text);
            Submitted?.Invoke(this, eventArgs);
            return eventArgs.Cancel;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (DialogResult.GetValueOrDefault(false) && SubmitUnsuccesfull())
                e.Cancel = true;
            base.OnClosing(e);
        }

        public class SubmitEventArgs<T> : EventArgs
        {
            public T Value { get; protected set; }
            public bool Cancel { get; set; } = false;

            public SubmitEventArgs(T value)
                => Value = value;
        }

        protected override void OnSourceInitialized(EventArgs e)
            => WindowHelper.RemoveIcon(this);
    }
}
