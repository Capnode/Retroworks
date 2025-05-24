using System.Threading.Tasks;
using Retroworks.RCBus.Interfaces;
using Retroworks.RCBus.ViewModels;

namespace Retroworks.RCBus.Services.Internals;

public class DialogService
{
    public async Task ShowDialog<THost, TDialogViewModel>(THost host, TDialogViewModel dialogViewModel)
        where TDialogViewModel : DialogViewModel
        where THost : IDialogProvider
    {
        // Set host dialog to provided one
        host.Dialog = dialogViewModel;
        dialogViewModel.Show();

        // Wait for dialog to close
        await dialogViewModel.WaitAsync();
    }
}