using Retroworks.RCBus.ViewModels;

namespace Retroworks.RCBus.Interfaces;

public interface IDialogProvider
{
    DialogViewModel Dialog { get; set; }
}