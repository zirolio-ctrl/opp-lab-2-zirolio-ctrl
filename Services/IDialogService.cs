using System.Threading.Tasks;

namespace opp_lab_2_zirolio_ctrl.Services;

public interface IDialogService
{
    Task<string?> PickXmlFileAsync();
    Task<string?> PickSaveFileAsync(string suggestedName, string ext);
    Task<bool> ConfirmAsync(string title, string message);
    void CloseMainWindow();
    void OpenFile(string path); 
}