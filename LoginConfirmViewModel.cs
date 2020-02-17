using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Randevu.Core.Services.Api;
using Randevu.Core.Services.Interfaces;
using Randevu.Core.Stores.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Timers;
using Randevu.Models.Accounts;

namespace Randevu.Core.ViewModels
{
    public class LoginConfirmViewModel : MvxViewModel<string>
    {
        private string _timerAutentification;
        public string TimerAutentification
        {
            get => _timerAutentification;
            set => SetProperty(ref _timerAutentification, value);
        }

        private int _progressAutentification;
        public int ProgressAutentification
        {
            get => _progressAutentification;
            set => SetProperty(ref _progressAutentification, value);
        }

        private string _confirmationCode;
        public string ConfirmationCode
        {
            get => _confirmationCode;
            set => SetProperty(ref _confirmationCode, value);
        }

        private bool _canConfirm;
        public bool CanConfirm
        {
            get => _canConfirm;
            set => SetProperty(ref _canConfirm, value);
        }

        private bool _canResend;
        public bool CanResend
        {
            get => _canResend;
            set => SetProperty(ref _canResend, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand ConfirmCommand { get; set; }
        public ICommand ResendCommand { get; set; }
        public ICommand StartTimerCommand { get; set; }

        private readonly IMvxNavigationService _mvxNavigationService;
        private readonly IAuthenticationApi _authenticationService;
        private readonly IToastService _toastService;
        private readonly IDeviceInfoService _deviceInfoService;
        private readonly IAuthenticationStore _authenticationStore;
        private readonly System.Timers.Timer _timer;

        private string _email;

        public LoginConfirmViewModel(IMvxNavigationService mvxNavigationService,
            IAuthenticationApi authenticationService,
            IToastService toastService,
            IDeviceInfoService deviceInfoService,
            IAuthenticationStore authenticationStore)
        {
            _mvxNavigationService = mvxNavigationService;
            _authenticationService = authenticationService;
            _toastService = toastService;
            _deviceInfoService = deviceInfoService;
            _authenticationStore = authenticationStore;

            _timer = new Timer(1000);
            _timer.Elapsed += OnTimerTick;

            ConfirmCommand = new MvxAsyncCommand(Confirm);
            ResendCommand = new MvxAsyncCommand(Resend);
            StartTimerCommand = new MvxCommand(StartTimer);

            CanConfirm = true;
        }

        private void OnTimerTick(object sender, ElapsedEventArgs e)
        {
            ProgressAutentification--;

            if (ProgressAutentification == 0)
            {
                CanConfirm = false;
                CanResend = true;
                _timer.Stop();
            }
            else
            {
                CanConfirm = true;
            }
        }

        public override void Prepare(string parameter)
        {
            _email = parameter;
        }

        private async Task Confirm()
        {
            try
            {
                IsLoading = true;

                var token = await _authenticationService.Login2FA(new Login2FAModel
                {
                    Code = ConfirmationCode,
                    DeviceId = _deviceInfoService.Id,
                    DeviceName = _deviceInfoService.DeviceName,
                    DeviceToken = _deviceInfoService.FirebaseToken,
                    Email = _email,
                    Provider = "Email"
                }).ConfigureAwait(false);

                await _authenticationStore.SetToken(token)
                    .ConfigureAwait(false);

                await _mvxNavigationService.Navigate<HomeViewModel>()
                    .ConfigureAwait(false);
            }
            catch (Exception)
            {
                _toastService.Show("Не вдалось підтвердити код, будь ласка, спробуйте ще раз", Models.ToastType.Error);
                CanResend = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task Resend()
        {
            CanResend = false;

            try
            {
                await _authenticationService.Send2FACode(new Randevu.Models.Accounts.Code2FAModel
                {
                    Email = _email,
                    Provider = "Email"
                }).ConfigureAwait(false);

                StartTimer();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void StartTimer()
        {
            ProgressAutentification = 60;

            _timer.Start();
        }
    }
}
