﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MCE_API_SERVER.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        public static SettingsPage Instance { 
            get {
                if (instance == null)
                    return new SettingsPage();
                else
                    return instance;
            }
        }
        private static SettingsPage instance;

        public SettingsPage()
        {
            if (instance == null)
                instance = this;
            else
                return; // shouldn't happen

            InitializeComponent();

            // make sure settings update
            LogRequestsCheck.CheckedChanged += (object sender, CheckedChangedEventArgs e) => Settings.LogRequests = e.Value;
            LogMesTimeCheck.CheckedChanged += (object sender, CheckedChangedEventArgs e) => Settings.LogMesTime = e.Value;
            LogMesTypeCheck.CheckedChanged += (object sender, CheckedChangedEventArgs e) => Settings.LogMesType = e.Value;
            MaxMesInput.Completed += (object sender, EventArgs e) =>
            {
                string text = "";
                for (int i = 0; i < MaxMesInput.Text.Length; i++)
                    if (char.IsDigit(MaxMesInput.Text[i]))
                        text += MaxMesInput.Text[i];

                int numb = 100;
                int.TryParse(text, out numb);
                if (numb < 1)
                    numb = 1;

                Settings.MaxMessagesInConsole = numb;
                MaxMesInput.Text = numb.ToString();
            };
        }

        protected override void OnAppearing()
        {
            // make sure settings are up-to date
            // updae checks
            LogRequestsCheck.IsChecked = Settings.LogRequests;
            LogMesTimeCheck.IsChecked = Settings.LogMesTime;
            LogMesTypeCheck.IsChecked = Settings.LogMesType;
            MaxMesInput.Text = Settings.MaxMessagesInConsole.ToString();

            // update selected message filters
            // first clear
            SelectedFilterContainer.Children.Clear();

            bool anyCreated = false;

            for (int i = 0; i < Settings.MesLogFilter.Length; i++) {
                if (Settings.MesLogFilter[i]) {
                    Grid grid = new Grid();

                    // add image showing color of this mes type
                    string name = MesFilterPage.FilterNames[i];
                    if (name == "Exception") // colors are same, would be same image
                        name = "Error";
                    ImageSource imageSource = ImageSource.FromResource(Util.GetFullResourceName($"FilterColorImages/{name}.PNG"), Util.CurrentAssembly);
                    Image image = new Image()
                    {
                        Source = imageSource,
                        Aspect = Aspect.AspectFit,
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Start
                    };
                    grid.Children.Add(image);

                    // add filter name
                    Label l = new Label();
                    l.Text = $"     {MesFilterPage.FilterNames[i]}";
                    l.HorizontalOptions = LayoutOptions.Start;
                    l.VerticalOptions = LayoutOptions.Center;
                    grid.Children.Add(l);

                    SelectedFilterContainer.Children.Add(grid);
                    anyCreated = true;
                }
            }

            // display "None" is all mes types are disabled
            if (!anyCreated) {
                Label l = new Label();
                l.Text = $"●None";
                l.HorizontalOptions = LayoutOptions.Start;
                l.VerticalOptions = LayoutOptions.Center;
                SelectedFilterContainer.Children.Add(l);
            }
        }

        private void Btn_MesFilter_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new MesFilterPage());
        }

        private void Btn_Save_Clicked(object sender, EventArgs e)
        {
            Settings.Save();
            DisplayAlert("Save", "Saved settings", "Ok");
        }
    }
}