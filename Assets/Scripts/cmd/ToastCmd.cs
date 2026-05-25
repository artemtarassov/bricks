public class ToastCmd
{
    private ViewModel viewModel => ViewModel.Instance;

    private string msg;
    public ToastCmd(string msg)
    {
        this.msg = msg;
    }
    public void Run()
    {
        viewModel.ShowToast(msg);
    }
}