using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaApp.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            _currentViewModel = _homePageViewModel;
            ChangeTo(0);
        }

        public void ChangeTo(int i)
        {
            ShowHome = false;
            ShowNews = false;
            ShowMessage = false;
            ShowProfile = false;
            if (i == 0)
            {
                CurrentViewModel = HomePageViewModel;
                ShowHome = true;
            }
            else if (i == 1)
            {
                CurrentViewModel = NewsPageViewModel;
                ShowNews = true;
            }
            else if (i == 2)
            {
                CurrentViewModel = MessagePageViewModel;
                ShowMessage = true;
            }
            else if (i == 3)
            {
                CurrentViewModel = ProfilePageViewModel;
                ShowProfile = true;
            }
        }

        [ObservableProperty]
        private ViewModelBase _currentViewModel;

        [ObservableProperty]
        private HomePageViewModel _homePageViewModel = new HomePageViewModel();

        [ObservableProperty]
        private NewsPageViewModel _newsPageViewModel = new NewsPageViewModel();

        [ObservableProperty]
        private MessagePageViewModel _messagePageViewModel = new MessagePageViewModel();

        [ObservableProperty]
        private ProfilePageViewModel _profilePageViewModel = new ProfilePageViewModel();



        [ObservableProperty]
        public bool _showHome = true;

        [ObservableProperty]
        public bool _showNews;

        [ObservableProperty]
        public bool _showMessage;

        [ObservableProperty]
        public bool _showProfile;
    }
}
