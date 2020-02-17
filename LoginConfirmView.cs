using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using Randevu.Core.ViewModels;
using System.Threading.Tasks;

namespace Randevu.Droid.Views
{
    [MvxFragmentPresentation(
        ActivityHostViewModelType = typeof(EnterContainerViewModel),
        AddToBackStack = true,
        FragmentContentId = Resource.Id.enter_content,
        EnterAnimation = Resource.Animation.fade_in,
        ExitAnimation = Resource.Animation.no_animation,
        PopEnterAnimation = Resource.Animation.no_animation,
        PopExitAnimation = Resource.Animation.fade_out)]
    public class LoginConfirmView : BaseFragment<LoginConfirmViewModel>
    {
        private EditText _codeField;

        protected override int FragmentId => Resource.Layout.login_confirm_view;

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            _codeField = view.FindViewById<EditText>(Resource.Id.input_autent);
            _codeField.RequestFocus();

            var input = (InputMethodManager)Activity.GetSystemService(Android.Content.Context.InputMethodService);
            Task.Delay(300).ContinueWith((_) => input.ToggleSoftInput(ShowFlags.Forced, HideSoftInputFlags.None));

            ViewModel.StartTimerCommand.Execute(null);
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();

            var inputMethodManager = (InputMethodManager)Activity.GetSystemService(Android.Content.Context.InputMethodService);
            inputMethodManager.HideSoftInputFromWindow(Activity.CurrentFocus.WindowToken, 0);

            Activity.CurrentFocus.ClearFocus();
        }
    }
}
